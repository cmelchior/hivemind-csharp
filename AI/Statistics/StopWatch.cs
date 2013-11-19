using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HiveMind.AI.Statistics
{
	public class StopWatch
	{

		private static readonly String SINGLE_WATCH_ID = "StopWatch.class#1";

		static private StopWatch instance;
		private Dictionary<String, Watch> watches = new Dictionary<String, Watch>();

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static StopWatch GetInstance() 
		{
			if (instance == null) 
			{
				instance = new StopWatch();
			}

			return instance;
		}

		private StopWatch() {}

		public Watch Start() 
		{
			return Start(SINGLE_WATCH_ID);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public Watch Start(String name) 
		{
			Watch watch = new Watch();
			watches[name] = watch;
			watch.Start();
			return watch;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public Watch Stop(String name) 
		{
			Watch watch = watches[name];
			if (watch != null) 
			{
				watch.Stop();
			} 
			else 
			{
				throw new ArgumentException("Watch not found for: " + name);
			}

			return watch;
		}

		public Watch stop() 
		{
			return Stop(SINGLE_WATCH_ID);
		}

		public Watch get(String name) 
		{
			return watches[name];
		}

		public class Watch 
		{

			public String name;
			public long start;
			public long end;
			public bool running { get; private set; }

			public Watch Start() {
				if (running) return this;
				running = true;
				start = Environment.TickCount;
				return this;
			}

			public Watch Stop() 
			{
				if (!running) return this;
				running = false;
				end = Environment.TickCount;
				return this;
			}

			public long GetElapsedTimeInMillis() 
			{
				long elapsed;
				if (running) 
				{
					elapsed = Environment.TickCount - start;
				}
				else 
				{
					elapsed = end - start;
				}
				return elapsed;
			}

			public long GetElapsedTimeInSeconds() 
			{
				long elapsed;
				if (running) 
				{
					elapsed = ((Environment.TickCount - start) / 1000);
				}
				else 
				{
					elapsed = ((end - start) / 1000);
				}
				return elapsed;
			}

			public void Log(String prefix) 
			{
				Console.Out.WriteLine(prefix + " Elapsed time: " + GetElapsedTimeInMillis() + " ms.");
			}
		}
	}
}

