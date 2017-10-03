using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HappyBin.AutoUpdater
{
	public class EventArgs<T> : EventArgs
	{
		public T Value { get; private set; }

		public DateTime TimeStamp { get { return DateTime.Now; } }

		public EventArgs(T value)
		{
			this.Value = value;
		}
	}
}