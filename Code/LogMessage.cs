using System;

namespace HappyBin.AutoUpdater
{
	public class LogMessage
	{
		string _message = null;
		Exception _ex = null;

		public LogMessage() { }
		public LogMessage(string message)
		{
			this.Message = message;
		}
		public LogMessage(string format, params object[] args)
		{
			this.Message = string.Format( format, args );
		}
		public LogMessage(string format, Exception ex, params object[] args)
		{
			this.Message = string.Format( format, args );
			this.Exception = ex;
		}

		public string Message
		{
			get { return _message; }
			set
			{
				if( _message != value )
				{
					_message = value;
					this.TimeStamp = DateTime.Now;
				}
			}
		}

		public DateTime TimeStamp { get; private set; }

		public Exception Exception
		{
			get { return _ex; }
			set
			{
				if( _ex != value )
				{
					_ex = value;
					this.IsError = _ex != null;
				}
			}
		}

		public bool IsError { get; set; }
	}
}