using System;
using System.Collections.Generic;
using System.IO.Ports;
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
}
