using System;
using System.Collections.Generic;
using System.Linq;

namespace HiveMind.Utils
{
	public class ThreadSafeList<T>
	{
		private List<T> _list = new List<T>();
		private object _sync = new object();
		public void Add(T value) {
			lock (_sync) {
				_list.Add(value);
			}
		}
	
		public T FirstOrDefault() {
			lock (_sync) {
				return _list.FirstOrDefault();
			}
		}

		public List<T> GetList()
		{
			lock (_sync) {
				return _list;
			}
		}
	}
}

