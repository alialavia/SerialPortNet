using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace ArduinoCommunicator
{
    public class SerialPortNETDataReceivedEventArgs : EventArgs
    {
        public SerialData EventType { get; private set; }        
        public SerialPortNETDataReceivedEventArgs(SerialData eventType)
        {
            EventType = eventType;
        }
    }
}
