using System;
using System.Collections.Generic;
using System.Text;
namespace SerialPortNET
{
    /// <summary>
    /// Provides data for the <see cref="SerialPort.DataReceived"/> event.
    /// </summary>
    public class SerialDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the event type.
        /// </summary>
        public SerialData EventType { get; private set; }        
        internal SerialDataReceivedEventArgs(SerialData eventType)
        {			
			EventType = eventType;
        }
    }

    /// <summary>
    /// SerialData type stored in by <see cref="SerialDataReceivedEventArgs"/>.
    /// </summary>
	public enum SerialData
	{
        /// <summary>
        /// If received data is normal character
        /// </summary>
		Chars,
        /// <summary>
        /// If received data is End Of File character
        /// </summary>
		Eof
	}
}
