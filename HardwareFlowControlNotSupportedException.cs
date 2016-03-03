using System;
using System.IO;

namespace SerialPortNET
{
	public class HardwareFlowControlNotSupportedException : IOException
	{
		public HardwareFlowControlNotSupportedException ()
		{
		}
	}
}

