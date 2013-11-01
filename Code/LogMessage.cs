using System;

namespace HappyBin.AutoUpdater
{
	public class LogMessage
	{
		string _message = null;

		public LogMessage() { }
		public LogMessage(string message)
		{
			this.Message = message;
		}
		public LogMessage(string format, params object[] args)
		{
			this.Message = string.Format( format, args );
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
	}
}