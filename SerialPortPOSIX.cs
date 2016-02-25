using System;
using System.Collections.Generic;
using System.Text;

namespace SerialPortNET
{
    class SerialPortPOSIX : ILowLevelSerialPort
    {
        public int BaudRate
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int BytesToRead
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int BytesToWrite
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public byte DataBits
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public DtrControl DtrControl
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsOpen
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Parity Parity
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string PortName
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public StopBits StopBits
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void DisposeUnmanagedResources()
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            throw new NotImplementedException();
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
