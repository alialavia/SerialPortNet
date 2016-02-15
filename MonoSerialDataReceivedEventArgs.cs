using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace ArduinoCommunicator
{
    public class MonoSerialDataReceivedEventArgs : EventArgs
    {
        public SerialData EventType { get; private set; }        
        public MonoSerialDataReceivedEventArgs(SerialData eventType)
        {
            EventType = eventType;
        }
    }
}
