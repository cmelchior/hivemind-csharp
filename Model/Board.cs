using System;

/// <summary>
///  Class representing a Hive hexagonal Board.
///
/// As Hive doesn't have any "real" board, the board is defined from the starting piece with gets position (0,0).
/// The rest of the board is described by a 2 dimensional grid system.
///
/// The hex grid uses a trapezoidal or axial coordinate system [2], like so:
///
///           _ _
///         /     \
///    _ _ /(0,-1) \ _ _
///  /     \  -r   /     \
/// /(-1,0) \ _ _ /(1,-1) \
/// \  -q   /     \       /
///  \ _ _ / (0,0) \ _ _ /
///  /     \       /     \
/// /(-1,1) \ _ _ / (1,0) \
/// \       /     \  +q   /
///  \ _ _ / (0,1) \ _ _ /
///        \  +r   /
///         \ _ _ /
///
///
/// If enabled, the board is maintained in "Standard Position" (SP), which is defined by the following properties:
///  - White Queen Bee is always (0,0)
///  - Black Queen coordinates must be min(R) <= 0 and 0 < Q < MAX_Q. Rotate the board clockwise until this is true.
///    This is effectively a place on the Q axis or if not able to land on R = 0, the last rotation before crossing, so
///    R > 0.
///
/// Before both queens are placed, SP is defined slightly different. In that case we adopt a small variant of the SP
/// definition in Randell Ringersolls book [1]. The only difference is that we always use the 3rd / 4th token even though
/// they might not be queens (although it usually will be the queen anyway).
///
///  - 1st token (white) is placed at (0,0).
///  - 2nd token (black) is placed at (1,0).
///  - 3rd token (white) if R > 0, flip board around Q axis, so R < 0.
///  - 4th token (black) if 3rd token has R = 0 and 4th has R > 0. Flip around Q axis so R < 0.
///
/// Standard position uses the queens as "center points" as they are the most likely to be locked in place and they cannot
/// move very far. This minimizes the chance of origin changes or rotations. Analysis of Boardspace.net games has shown
/// that Queen Bee moves are only 4.8% of all moves.
///
/// Note SP doesn't guarantee that all similar board positions have the same Zobrist Key [3]. Mirror or reflected boards
/// around the Q axis do not have the same Zobrist key. For now this is in acceptable inaccuracy as we still reduce
/// a board position from 12 different positions to 2.
///
/// << Insert example here >>
///
/// A Zobrist key is maintained for the board state. It will be recalculated for rotations/flips/center changes, so
/// the Zobrist key is always calculated on the SP board.
///
/// Note the backing hashes for the Zobrist key have a pretty high memory requirement, due to the potential board size.
/// Currently about 51*51*7*2*8 bytes ~ 2.2 MB.
///
/// @see [1] Randy Ingersoll: Play Hive like a champion
/// @see [2] http://www.redblobgames.com/grids/hexagons/
/// @see [3] http://en.wikipedia.org/wiki/Zobrist_hashing
/// </summary>
using System.Collections.Generic;
using System.Linq;
using HiveMind.Model;
using HiveMind;
using HiveMind.Debug;


namespace HiveMind.Model
{
	public class Board
	{

		private HiveAsciiPrettyPrinter printer = new HiveAsciiPrettyPrinter();
		private Dictionary<string, Hex> hexes = new Dictionary<string, Hex>();
		private HashSet<Token> tokens = new HashSet<Token>();
		private int[,] neighbors = new int[6,2] {{0,-1},{+1,-1},{1,0},{0,1},{-1, +1},{-1,0}}; // From top and clockwise round. Flat orientation 

		// Standard position variabels
		public Player WhitePlayer { get; private set; }
		public Player BlackPlayer { get; private set; }

		public StandardPositionMode StandardPositionMode { get; set; }

		private bool spQFlip = false;          // Flip around the Q axis
		private int spRotation = 0;               // How many clockwise rotations are needed to achieve Standard Position
		private int[] spOrigin = new int[2];      // Displacement of origin
		private Token[] firstTokens = new Token[5]; // Keep track of the first 4 tokens placed on the board. 2 white and 2 black

		// The board state is hashed as a Zobrist key.
		private long zobristKey = 0;

		// See [2] for details about storing hexagon maps.
		// Maximum size is 26 tokens in each directions that can be stacked 7 high.
		// This is not entirely true, but for simplicity we just use that as a first implementation
		// q, r, height, color, token type
		private long[,,,,] zobristHashes = new long[51,51,7,2,8];
						 
		public Board (Player white, Player black)
		{
			WhitePlayer = white;
			BlackPlayer = black;
		}

		private void loadZobristHashes() 
		{
			// Create the zobrist hashes needed for all edges
			Random random = new Random();
			for (int i = 0; i < zobristHashes.GetLength(0); i++) {
				for (int j = 0; j < zobristHashes.GetLength(1); j++) {
					for (int k = 0; k < zobristHashes.GetLength(2); k++) {
						for (int l = 0; l < zobristHashes.GetLength(3); l++) {
							for(int m = 0; m < zobristHashes.GetLength(4); m++) {
								zobristHashes[i,j,k,l,m] = random.NextInt64();
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Add a token to the board.
		/// </summary>
		public void AddToken(Token token, int q, int r)
		{
			if (token == null) return;
			Hex hex = findOrCreateHex(q, r);
			hex.AddToken(token);
			token.Hex = hex;
			token.Player.RemoveFromSupply(token);
			tokens.Add(token);

			updateZobristKey(token);

			// Keep track of first 4 tokens
			if (tokens.Count < 6) {
				firstTokens[tokens.Count - 1] = token;
			}
			maintainStandardPosition(token);
		}

		private void maintainStandardPosition(Token token) 
		{
			if (StandardPositionMode == StandardPositionMode.DISABLED) return;
			bool rebuildZobristKey;
			if (bothQueensPlaced() && StandardPositionMode == StandardPositionMode.ENABLED) 
			{
				rebuildZobristKey = maintainSPForMidGame(token);
			} 
			else 
			{
				rebuildZobristKey = maintainSPForOpenings(token);
			}

			if (rebuildZobristKey) 
			{
				this.rebuildZobristKey();
			}
		}

		private bool bothQueensPlaced() 
		{
			return !WhitePlayer.Queen.InSupply() && !BlackPlayer.Queen.InSupply();
		}

		// INVARIANT: All tokens are placed in a legal position
		private bool maintainSPForOpenings(Token token) 
		{
			bool rebuildZobristKey = false;

			if (firstTokens[4] != null ) 
			{
				// Should only be called if standardPostion = StandardPostionMode.LIMITED
				// This only acts as sentinel.
				return false;
			} 
			else if (firstTokens[3] != null) 
			{
				// TURN 4+, after turn 4 pieces can move
				rebuildZobristKey = moveOrigin(firstTokens[0]) || rebuildZobristKey;
				rebuildZobristKey = rotateToStandardPosition(firstTokens[1]) || rebuildZobristKey;
				rebuildZobristKey = swapAxisIfNeededForThirdToken() || rebuildZobristKey;
				rebuildZobristKey = swapAxisIfNeededForFourthToken() || rebuildZobristKey;
			} 
			else if (firstTokens[2] != null) 
			{
				// TURN 3
				rebuildZobristKey = moveOrigin(firstTokens[0]) || rebuildZobristKey;
				rebuildZobristKey = rotateToStandardPosition(firstTokens[1]) || rebuildZobristKey;
				rebuildZobristKey = swapAxisIfNeededForThirdToken() || rebuildZobristKey;
			} 
			else if (firstTokens[1] != null) 
			{
				// TURN 2
				rebuildZobristKey = moveOrigin(firstTokens[0]) || rebuildZobristKey;
				rebuildZobristKey = rotateToStandardPosition(firstTokens[1]) || rebuildZobristKey;
			} 
			else if (firstTokens[0] != null) 
			{
				// TURN 1
				rebuildZobristKey = moveOrigin(token) || rebuildZobristKey;
			} 
			else 
			{
				throw new ArgumentException("Board is empty");
			}

			return rebuildZobristKey;
		}

		private bool swapAxisIfNeededForThirdToken() 
		{
			int[] coords = getRotatedSPCoordinatesFor(firstTokens[2].Hex);
			bool oldFlip = spQFlip;
			// R must be negative for 3rd token to be in SP
			if (coords[1] > 0) 
				spQFlip = true;
			else 
				spQFlip = false;

			return oldFlip != spQFlip;
		}

		private bool swapAxisIfNeededForFourthToken() 
		{
			int[] coords = GetSPCoordinatesFor(firstTokens[2].Hex);
			bool oldFlip = spQFlip;

			// If R == 0 for 3rd piece, R must be negative for 4th piece to be in SP
			if (coords[1] == 0) 
			{
				// Only check rotation if 3rd is inline with the rest
				coords = GetSPCoordinatesFor(firstTokens[3].Hex);
				if (coords[1] > 0) 
				{
					spQFlip = true;
				}
				else 
				{
					spQFlip = false;
				}
			}

			return oldFlip != spQFlip;
		}

		private bool maintainSPForMidGame(Token token) 
		{
			bool oldFlip = spQFlip;
			bool rebuildZobristKey = false;

			spQFlip = false;
			rebuildZobristKey = moveOrigin(WhitePlayer.Queen) || rebuildZobristKey;
			rebuildZobristKey = rotateToStandardPosition(BlackPlayer.Queen) || rebuildZobristKey;

			return rebuildZobristKey || oldFlip != spQFlip;
		}

		private bool moveOrigin(Token token) 
		{
			if (token.InSupply()) throw new ArgumentException("Cannot recenter around token in supply: " + token);
			Hex hex = token.Hex;
			bool rebuildZobristKey = spOrigin[0] != hex.Q || spOrigin[1] != hex.R;
			spOrigin[0] = token.Hex.Q;
			spOrigin[1] = token.Hex.R;
			return rebuildZobristKey;
		}

		// Rebuild Zobrist key when Standard Position changes
		private void rebuildZobristKey() 
		{
			zobristKey = 0;
			foreach (Token t in tokens) 
			{
				updateZobristKey(t);
			}
		}

		// Rotate to last available space before crossing the positive Q axis.
		// Return true if rotation has changed so Zobrist key needs to be rebuild.
		private bool rotateToStandardPosition(Token token) 
		{
			if (token.Hex.Q == spOrigin[0] && token.Hex.R == spOrigin[1]) {
				// Very special case, can happen if a Beetle working as Blacks anchor point, move on top of White's origin.
				// Just keep current SP in that case.
				return false;
			}

			int oldRotation = spRotation;
			int[] sp = GetSPCoordinatesFor(token.Hex);
			int maxRotations = 6;
			while(!(sp[0] >= 0 && sp[1] >= 1)) {  // q >= 0 && r >= 1
				if (maxRotations == 0) {
					printer.print(this);
					throw new ArgumentException("Keeps rotating: Cannot find Standard Position");
				}
				rotateClockwise();
				sp = getRotatedSPCoordinatesFor(token.Hex);
				maxRotations--;
			}

			rotateCounterClockwise();
			return spRotation != oldRotation;
		}

		private void rotateClockwise() 
		{
			spRotation = (spRotation + 1) % 6;
		}

		private void rotateCounterClockwise() 
		{
			spRotation--;
			if (spRotation < 0) {
				spRotation = 5;
			}
		}

		public int[] GetSPCoordinatesFor(Hex hex) 
		{
			return GetSPCoordinatesFor(hex.Q, hex.R);
		}

		public int[] GetSPCoordinatesFor(int q, int r) 
		{
			int[] cubeCoords = HexagonUtils.ConvertToCubeCoordinates(q - spOrigin[0], r - spOrigin[1]);

			for (int i = 0; i < spRotation; i++) 
			{
				cubeCoords = HexagonUtils.RotateRight(cubeCoords);
			}

			if (spQFlip) 
			{
				int newX = -cubeCoords[1];
				int newY = -cubeCoords[0];
				int newZ = -cubeCoords[2];
				cubeCoords[0] = newX;
				cubeCoords[1] = newY;
				cubeCoords[2] = newZ;
			}

			return HexagonUtils.ConvertToAxialCoordinates(cubeCoords[0], cubeCoords[1], cubeCoords[2]);
		}

		/// <summary>
		/// Returns the SP coordinates, only taking rotation into account
		/// </summary>
		private int[] getRotatedSPCoordinatesFor(Hex hex) 
		{
			int q = hex.Q;
			int r = hex.R;
			int[] cubeCoords = HexagonUtils.ConvertToCubeCoordinates(q - spOrigin[0], r - spOrigin[1]);
			for (int i = 0; i < spRotation; i++) 
			{
				cubeCoords = HexagonUtils.RotateRight(cubeCoords);
			}
			return HexagonUtils.ConvertToAxialCoordinates(cubeCoords[0], cubeCoords[1], cubeCoords[2]);
		}

		/// <summary>
		/// Returns the hex for the given Standard Position
		/// </summary>
		/// <returns>The hex for standard position.</returns>
		/// <param name="q">Q coordinate in Standard Position</param>
		/// <param name="r">R coordinate in Standard Position</param>
		public Hex GetHexForStandardPosition(int q, int r) 
		{
			int[] cubeCoords = HexagonUtils.ConvertToCubeCoordinates(q, r);

			if (spQFlip) {
				int newX = -cubeCoords[1];
				int newY = -cubeCoords[0];
				int newZ = -cubeCoords[2];
				cubeCoords[0] = newX;
				cubeCoords[1] = newY;
				cubeCoords[2] = newZ;
			}

			for (int i = 0; i < (6 - spRotation); i++) {
				cubeCoords = HexagonUtils.RotateRight(cubeCoords);
			}

			int[] originalCoords = HexagonUtils.ConvertToAxialCoordinates(cubeCoords[0], cubeCoords[1], cubeCoords[2]);
			return findOrCreateHex(originalCoords[0] + spOrigin[0], originalCoords[1] + spOrigin[1]);
		}

		/// <summary>
		/// Enable standard position for the board. Can only be enabled when the board is empty.
		/// </summary>
		public void setStandardPositionMode(StandardPositionMode mode) {
			if (tokens.Count > 0 && mode != StandardPositionMode.DISABLED) throw new ArgumentException("Cannot enable Standard Position for non-empty board");
			StandardPositionMode = mode;
			if (mode != StandardPositionMode.DISABLED) {
				loadZobristHashes();
				spOrigin = new int[2];
				spRotation = 0;
				spQFlip = false;
			}
		}

		/// <summary>
		/// Updating the Zobrist key works by XOR, so calling it once, will insert a token, calling it twice will remove it.
		/// </summary>
		/// <param name="token">Token to update key with.</param>
		private void updateZobristKey(Token token) 
		{
			if (token == null || token.Hex == null) return;

			Hex tokenHex = token.Hex;
			int typeIndex = -1;

			BugType originalType = token.OriginalType;
			if (originalType == BugType.QUEEN_BEE) typeIndex = 0;
			else if (originalType == BugType.BEETLE) typeIndex = 1;
			else if (originalType == BugType.GRASSHOPPER) typeIndex = 2;
			else if (originalType == BugType.SPIDER) typeIndex = 3;
			else if (originalType == BugType.SOLDIER_ANT) typeIndex = 4;
			else if (originalType == BugType.MOSQUITO) typeIndex = 5;
			else if (originalType == BugType.LADY_BUG) typeIndex = 6;
			else if (originalType == BugType.PILL_BUG) typeIndex = 7;
			else throw new ArgumentException("Unknown token: " + token); 

			int[] coords = GetSPCoordinatesFor(tokenHex);
			zobristKey = zobristKey ^ zobristHashes[
				coords[0] + 25, 
				coords[1] + 25, 
				tokenHex.GetHeight() - 1, 
				getColorIndex(token.Player), 
				typeIndex];
		}

		private int getColorIndex(Player player) 
		{
			return player.IsWhitePlayer() ? 0 : 1;
		}

		/// <summary>
		/// Removes a token from the board and puts in back into the players supply.
		/// INVARIANT: Can only be removed in the order they where added.
		/// </summary>
		public void RemoveToken(int q, int r) 
		{
			Hex hex = findOrCreateHex(q, r);
			updateZobristKey(hex.SeeTopToken());
			Token token = hex.RemoveToken();
			token.Hex = null;
			token.Player.AddToSupply(token);
			tokens.Remove(token);

			// Maintain tracking of first 4 tokens.
			if (tokens.Count < 5) 
			{
				firstTokens[tokens.Count] = null;
			}

			if (tokens.Count == 0) 
			{
				spOrigin[0] = 0;
				spOrigin[1] = 0;
			}
		}

		public void MoveToken(Token token, int toQ, int toR) 
		{
			if (token.Hex != null) 
			{
				if (!(token.Hex.SeeTopToken() == token)) 
				{
					throw new ArgumentException("Cannot move a token that is not on top of the stack.");
				}
				updateZobristKey(token); // Remove current position
				Hex hex = token.Hex;
				hex.RemoveToken();
				if (hex.IsEmpty()) 
				{
					hexes.Remove(hex.ToString());
				}
			}

			Hex toHex = findOrCreateHex(toQ, toR);
			toHex.AddToken(token);
			token.Hex = toHex;
			updateZobristKey(token); // Add new position
		}

		/// <summary>
		/// Move the top creature from hex to another.
		/// </summary>
		public void MoveToken(int fromQ, int fromR, int toQ, int toR) 
		{
			Hex fromHex = FindHex(fromQ, fromR);
			if (fromHex == null) return;

			updateZobristKey(fromHex.SeeTopToken()); // Remove current position
			Token token = fromHex.RemoveToken();
			Hex toHex = findOrCreateHex(toQ, toR);
			toHex.AddToken(token);
			token.Hex = toHex;
			maintainStandardPosition(token);
		}

		/// <summary>
		/// Returns a list of all hexes with 1 or more bugs.
		/// </summary>
		/// <returns>The filled hexes.</returns>
		public List<Hex> GetFilledHexes() 
		{
			List<Hex> result = new List<Hex>();
			foreach (Hex hex in hexes.Values) 
			{
				if (!hex.IsEmpty()) 
				{
					result.Add(hex);
				}
			}
			return result;
		}

		/// <summary>
		/// Returns the hex for the given position or create a new if it doesn't exists
		/// </summary>
		private Hex findOrCreateHex(int q, int r) 
		{
			// Look through all existing hexes
			Hex hex = FindHex(q, r);
			if (hex != null) return hex;

			// Create new hex if needed
			hex = new Hex(q, r);
			hexes.Add(getKey(q, r), hex);
			return hex;
		}

		private String getKey(int q, int r) 
		{
			return ""+q+r;
		}

		/// <summary>
		/// Returns the hex for the given position or null it no creatures reside there.
		/// </summary>
		public Hex FindHex(int q, int r) 
		{
			Hex hex;
			if (hexes.TryGetValue(getKey (q, r), out hex)) {
				return hex;
			} else {
				return null;
			}; 
		}

		/// <summary>
		/// Returns the width of the board in hexes.
		/// </summary>
		public int GetWidth() 
		{
			int minQ = int.MinValue;
			int maxQ = int.MaxValue;

			foreach (Hex hex in hexes.Values) 
			{
				if (hex.SeeTopToken() == null) continue;
				int q = hex.Q;

				if (q < minQ) 
				{
					minQ = q;
				}

				if (q > maxQ) 
				{
					maxQ = q;
				}
			}

			return Math.Abs(maxQ - minQ) + 1;
		}

		/// <summary>
		/// Returns the heigh of the board in hexes.
		/// </summary>
		public int GetHeight() 
		{
			int minY = int.MaxValue;
			int maxY = int.MinValue;

			foreach (Hex hex in hexes.Values) 
			{
				if (hex.SeeTopToken() == null) continue;
				int y = hex.R;

				if (y < minY) 
				{
					minY = y;
				}

				if (y > maxY) 
				{
					maxY = y;
				}
			}

			return Math.Abs(maxY - minY) + 1;
		}

		/// <summary>
		/// Returns the top left hex or null if board is empty
		/// </summary>
		/// <returns>The top left.</returns>
		public Hex GetTopLeft() 
		{
			if (hexes.Count == 0) return null;
			Hex result = null;
			foreach (Hex hex in hexes.Values) 
			{
				if (hex.SeeTopToken() == null) continue;
				if (result == null) 
				{
					result = hex;
					continue;
				}

				if (hex.Q < result.Q) 
				{
					result = hex;
					continue;
				} 
				else if (hex.R < result.R) 
				{
					result = hex;
				}
			}

			return result;
		}

		/// <summary>
		/// Return minimum x coordinate for all hexes.
		/// </summary>
		public int GetMinQ() 
		{
			int result = int.MaxValue;
			foreach (Hex hex in hexes.Values) 
			{
				if (hex.SeeTopToken() == null) continue;
				int q = IsUsingStandardPosition() ? GetSPCoordinatesFor(hex)[0] : hex.Q;
				if (q < result) 
				{
					result = q;
				}
			}

			return result;
		}

		/// <summary>
		/// Return minimum y coordinate for all hexes.
		/// </summary>
		public int GetMinR() 
		{
			int result = int.MaxValue;
			foreach (Hex hex in hexes.Values) 
			{
				if (hex.SeeTopToken() == null) continue;
				int r = IsUsingStandardPosition() ? GetSPCoordinatesFor(hex)[1] : hex.R;
				if (r < result) 
				{
					result = r;
				}
			}

			return result;
		}

		public int GetMaxQ() 
		{
			int result = int.MinValue;
			foreach (Hex hex in hexes.Values) {
				if (hex.SeeTopToken() == null) continue;
				int q = IsUsingStandardPosition() ? GetSPCoordinatesFor(hex)[0] : hex.Q;
				if (q > result) 
				{
					result = q;
				}
			}

			return result;
		}

		public int getMaxR() 
		{
			int result = int.MinValue;
			foreach (Hex hex in hexes.Values) 
			{
				if (hex.SeeTopToken() == null) continue;
				int r = IsUsingStandardPosition() ? GetSPCoordinatesFor(hex)[1] : hex.R;
				if (r > result) 
				{
					result = r;
				}
			}

			return result;
		}

		/// <summary>
		/// Returns a list of neighbor tokens 
		/// </summary>
		public List<Token> GetNeighborTokens(Hex centerHex) 
		{
			if (centerHex == null) return new List<Token>();

			List<Token> result = new List<Token>();
			for (int i = 0; i < neighbors.GetLength(0); i++) 
			{
				int q = centerHex.Q + neighbors[i,0];
				int r = centerHex.R + neighbors[i,1];
				Hex hex = FindHex(q, r);
				if (hex != null && hex.SeeTopToken() != null) 
				{
					result.Add(hex.SeeTopToken());
				}
			}

			return result;
		}

		/// <summary>
		/// Returns a list of neighbor hexes from top and clockwise round.
		/// </summary>
		public List<Hex> GetNeighborHexes(Hex hex) 
		{
			if (hex == null) return new List<Hex>();
			List<Hex> result = new List<Hex>();
			for (int i = 0; i < neighbors.GetLength(0); i++) 
			{
				int q = hex.Q + neighbors[i,0];
				int r = hex.R + neighbors[i,1];
				result.Add(findOrCreateHex(q, r));
			}

			return result;
		}

		public Hex GetHex(int q, int r) 
		{
			return findOrCreateHex(q, r);
		}

		/// <summary>
		/// Return the hex that is the clockwise neighbor of the target hex when looking from the starting hex.
		/// INVARIANT: From and To are neighbors.
		/// </summary>
		public Hex GetClockwiseHex(Hex from, Hex to, Board board) 
		{
			int qDiff = to.Q - from.Q;
			int rDiff = to.R - from.R;

			int neighborIndex = 0;
			for (int i = 0; i < neighbors.GetLength(0); i++) 
			{
				if (neighbors[i, 0] == qDiff && neighbors[i, 1] == rDiff)
				{
					neighborIndex = i;
					break;
				}
			}

			int row = (neighborIndex + 1) % neighbors.GetLength(0);
			int clockwiseNeighborCoordinatesQ = neighbors[row, 0];
			int clockwiseNeighborCoordinatesR = neighbors [row, 1];
			return GetHex(from.Q + clockwiseNeighborCoordinatesQ, from.R + clockwiseNeighborCoordinatesR);
		}

		/// <summary>
		/// Return the hex that is the counter clockwise neighbor of the target hex when looking from the starting hex.
		/// INVARIANT: From and To are neighbors.
		/// </summary>
		public Hex GetCounterClockwiseHex(Hex from, Hex to, Board board) {
			int qDiff = to.Q - from.Q;
			int rDiff = to.R - from.R;

			int neighborIndex = 0;
			for (int i = 0; i < neighbors.GetLength(0); i++) 
			{
				if (neighbors[i, 0] == qDiff && neighbors[i, 1] == rDiff) 
				{
					neighborIndex = i;
					break;
				}
			}

			int row = (neighborIndex  + neighbors.GetLength(0) - 1) % neighbors.GetLength(0);
			int clockwiseNeighborCoordinatesQ = neighbors[row, 0];
			int clockwiseNeighborCoordinatesR = neighbors [row, 1];
			return GetHex(from.Q + clockwiseNeighborCoordinatesQ, from.R + clockwiseNeighborCoordinatesR);
		}

		/// <summary>
		/// Get list of empty hexes around a hex.
		/// </summary>
		public List<Hex> GetEmptyNeighborHexes(Token token) {
			List<Hex> result = new List<Hex>();
			foreach (Hex hex in GetNeighborHexes(token.Hex))
			{
				if (hex.IsEmpty()) 
				{
					result.Add(hex);
				}
			}

			return result;
		}

		public void Clear() {
			hexes.Clear();
		}

		/// <summary>
		/// Returns the zobrist key for the given board layout.
		/// </summary>
		public long GetZobristKey() {
			return zobristKey;
		}

		public bool IsUsingStandardPosition() {
			return (StandardPositionMode != StandardPositionMode.DISABLED);
		}
	}
}

