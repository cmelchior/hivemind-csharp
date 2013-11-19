using System;
using System.Collections.Generic;

namespace HiveMind
{
	public class LimitedBuffer<E> : LinkedList<E> {
	
		private int _limit;

		public LimitedBuffer(int limit) 
		{
			this._limit = limit;
		}

		public bool Add(E e) 
		{
			AddFirst (e);
			if (Count > _limit) 
			{
				RemoveLast();
			}

			return true;
		}

		/// <summary>
		/// Returns and array with at the size of limit.
		/// All fields not filled are <c>null</c>.
		/// </summary>
		/// <returns>The array.</returns>
		public E[] ToArray()
		{
			E[] result = new E[_limit];

			Enumerator it = GetEnumerator();
			int i = 0;
			while (it.MoveNext()) {
				result [i] = it.Current;
				i++;
			}

			// null fill the rest of the array
			for (int j = i; j < _limit; j++) {
				result[j] = default(E);
			}

			return result;
		}
	}
}

