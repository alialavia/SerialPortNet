﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ArduinoCommunicator
{
    /// <summary>
    /// Mono implementation of SerialPort is incomplete. This is to make up for that.
    /// </summary>
    public class MonoSerialPort : IDisposable
    {
        #region Public Fields

        public bool Async = true;

        #endregion Public Fields

        #region Public Constructors

        public MonoSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
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

        #region Public Events

        public event EventHandler<MonoSerialDataReceivedEventArgs> DataReceived;

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
            Win32File.CloseHandle(serialHandle);
        }

        public void Dispose()
        {
            if (!serialHandle.IsClosed)
                Close();
        }

        public void Open()
        {
            serialHandle = Win32File.CreateFile("\\\\.\\" + this.portName, (uint)(EFileAccess.FILE_GENERIC_READ | EFileAccess.FILE_GENERIC_WRITE), 0, IntPtr.Zero, (uint)ECreationDisposition.OpenExisting, (uint)EFileAttributes.Normal, IntPtr.Zero);
            if (serialHandle.IsInvalid)
                throw new IOException("Cannot open " + this.portName);

            DCB serialParams = new DCB();
            serialParams.DCBLength = (uint)Marshal.SizeOf(serialParams);

            if (!Win32File.GetCommState(serialHandle, ref serialParams))
                throw new IOException("GetCommState error!");

            serialParams.BaudRate = (uint)this.baudRate;
            serialParams.ByteSize = (byte)this.dataBits;
            serialParams.StopBits = this.stopBits;
            serialParams.Parity = this.parity;
            serialParams.DtrControl = DtrControl.Enable;
            if (!Win32File.SetCommState(serialHandle, ref serialParams))
                throw new IOException("SetCommState error!");

            COMMTIMEOUTS timeout = new COMMTIMEOUTS();
            timeout.ReadIntervalTimeout = 50;
            timeout.ReadTotalTimeoutConstant = 50;
            timeout.ReadTotalTimeoutMultiplier = 50;
            timeout.WriteTotalTimeoutConstant = 50;
            timeout.WriteTotalTimeoutMultiplier = 10;

            if (!Win32File.SetCommTimeouts(serialHandle, ref timeout))
                throw new IOException("SetCommTimeouts error!");

            Win32File.PurgeComm(serialHandle, (uint)(0xF));
            IsOpen = true;
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            //serialStream.Read(buffer, offset, count); ;
            uint bytesRead = 0;
            bool success = Win32File.ReadFile(serialHandle, buffer, (uint)count, out bytesRead, IntPtr.Zero);
            if (!success)
                throw new IOException("Read returned error :" + new Win32Exception((int)Win32File.GetLastError()).Message);
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
            //serialStream.Write(buffer, offset, count);
            uint bytesWrote = 0;

            bool success = Win32File.WriteFile(serialHandle, buffer, (uint)count, out bytesWrote, IntPtr.Zero);
            if (!success)
                throw new IOException("Write returned error :" + new Win32Exception((int)Win32File.GetLastError()).Message);
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void OnDataReceived()
        {
            EventHandler<MonoSerialDataReceivedEventArgs> handler = DataReceived;
            if (handler != null)
                handler(this, new MonoSerialDataReceivedEventArgs(SerialData.Chars));
        }

        #endregion Protected Methods

        #region Private Fields

        private int baudRate;
        // For async operation
        private BackgroundWorker bgWorker = new BackgroundWorker();

        private int dataBits;
        private Parity parity;
        private string portName;
        //FileStream serialStream;
        private SafeFileHandle serialHandle;

        private StopBits stopBits;

        #endregion Private Fields

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
            Win32File.ClearCommError(serialHandle, out flags, out stats);
            return stats;
        }

        #endregion Private Methods
    }
}