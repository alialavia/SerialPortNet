using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ArduinoCommunicator
{
    /// <summary>
    /// Mono implementation of SerialPort is incomplete. This is to make up for that.
    /// </summary>
    public class SerialPortNET : IDisposable
    {
        #region Public Constructors

        public SerialPortNET(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            // Do some basic settings
            this.IsOpen = false;
            this.IsRunning = false;
            this.portName = portName;
            this.baudRate = baudRate;
            this.parity = parity;
            this.dataBits = dataBits;
            this.stopBits = stopBits;
        }

        #endregion Public Constructors

        #region Public Methods

        public static Dictionary<String, String> EnumerateSerialPorts()
        {
            const string keyname = @"HARDWARE\DEVICEMAP\SERIALCOMM";
            var keys = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyname);
            var valueNames = keys.GetValueNames();
            var res = new Dictionary<String, String>();
            foreach (var k in valueNames)
            {
                res.Add(k, keys.GetValue(k) as String);
            }
            return res;
        }

        public void Close()
        {
            Dispose();
            //NativeMethods.CloseHandle(serialHandle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Open()
        {
            serialHandle = NativeMethods.CreateFile("\\\\.\\" + this.portName, (uint)(EFileAccess.FILE_GENERIC_READ | EFileAccess.FILE_GENERIC_WRITE), 0, IntPtr.Zero, (uint)ECreationDisposition.OpenExisting, (uint)EFileAttributes.Normal, IntPtr.Zero);
            if (serialHandle.IsInvalid)
                throw new IOException("Cannot open " + this.portName);

            DCB serialParams = new DCB();
            serialParams.DCBLength = (uint)Marshal.SizeOf(serialParams);

            if (!NativeMethods.GetCommState(serialHandle, ref serialParams))
                throw new IOException("GetCommState error!");

            serialParams.BaudRate = (uint)this.baudRate;
            serialParams.ByteSize = (byte)this.dataBits;
            serialParams.StopBits = this.stopBits;
            serialParams.Parity = this.parity;
            serialParams.DtrControl = DtrControl.Enable;
            if (!NativeMethods.SetCommState(serialHandle, ref serialParams))
                throw new IOException("SetCommState error!");

            COMMTIMEOUTS timeout = new COMMTIMEOUTS();
            timeout.ReadIntervalTimeout = 50;
            timeout.ReadTotalTimeoutConstant = 50;
            timeout.ReadTotalTimeoutMultiplier = 50;
            timeout.WriteTotalTimeoutConstant = 50;
            timeout.WriteTotalTimeoutMultiplier = 10;

            if (!NativeMethods.SetCommTimeouts(serialHandle, ref timeout))
                throw new IOException("SetCommTimeouts error!");

            NativeMethods.PurgeComm(serialHandle, (uint)(0xF));
            IsOpen = true;
        }

        public void Read(byte[] buffer, int offset, int count, bool blocking = true)
        {
            //serialStream.Read(buffer, offset, count); ;
            uint bytesRead = 0;
            byte[] unoffsetedBuffer = new byte[count];
            if (blocking)
                while (this.BytesToRead < count) ;

            bool success = NativeMethods.ReadFile(serialHandle, unoffsetedBuffer, (uint)count, out bytesRead, IntPtr.Zero);
            if (!success)
                throw new IOException("Read returned error :" + new Win32Exception((int)NativeMethods.GetLastError()).Message);

            unoffsetedBuffer.CopyTo(buffer, offset);
        }

        public byte[] ReadAll()
        {
            byte[] buffer = new byte[this.BytesToRead];
            Read(buffer, 0, BytesToRead);
            return buffer;
        }

        public void Run()
        {
            if (IsRunning)
                return;
            if (!IsOpen)
                return;

            if (Async)
            {
                bgWorker.DoWork += new DoWorkEventHandler(BgWorker_DoWork);
                bgWorker.ProgressChanged += BgWorker_ProgressChanged;
                bgWorker.RunWorkerAsync();
            }
            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            if (bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();
            }
            IsRunning = false;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (count != 4)
                Debugger.Break();
            //serialStream.Write(buffer, offset, count);
            uint bytesWrote = 0;
            byte[] offsetedBuffer = new byte[count];
            buffer.CopyTo(offsetedBuffer, offset);
            bool success = NativeMethods.WriteFile(serialHandle, offsetedBuffer, (uint)count, out bytesWrote, IntPtr.Zero);
            if (!success)
                throw new IOException("Write returned error :" + new Win32Exception((int)NativeMethods.GetLastError()).Message);
        }

        public void WriteAll(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                serialHandle.Dispose();
            }

            disposed = true;
        }

        protected virtual void OnDataReceived()
        {
            EventHandler<SerialPortNETDataReceivedEventArgs> handler = DataReceived;
            if (handler != null)
                handler(this, new SerialPortNETDataReceivedEventArgs(SerialData.Chars));
        }

        #endregion Protected Methods

        #region Private Methods

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (this.BytesToRead >= this.ReceivedBytesThreshold)
                {
                    OnDataReceived();
                }
            }
        }

        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private COMSTAT getStats()
        {
            uint flags = 0;
            COMSTAT stats = new COMSTAT();
            NativeMethods.ClearCommError(serialHandle, out flags, out stats);
            return stats;
        }

        #endregion Private Methods

        #region Public Events

        public event EventHandler<SerialPortNETDataReceivedEventArgs> DataReceived;

        #endregion Public Events

        #region Public Properties

        public int BytesToRead
        {
            get
            {
                return (int)getStats().cbInQue;
            }
        }

        public int BytesToWrite
        {
            get
            {
                return (int)getStats().cbOutQue;
            }
        }

        public bool IsOpen { private set; get; }
        public bool IsRunning { private set; get; }
        public int ReceivedBytesThreshold { get; set; }

        #endregion Public Properties

        #region Public Fields

        public readonly string portName;
        public bool Async = true;

        #endregion Public Fields

        #region Private Fields

        private int baudRate;

        // For async operation
        private BackgroundWorker bgWorker = new BackgroundWorker();

        private int dataBits;
        private bool disposed;
        private Parity parity;

        //FileStream serialStream;
        private SafeFileHandle serialHandle;

        private StopBits stopBits;

        #endregion Private Fields
    }
}