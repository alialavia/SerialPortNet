using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace SerialPortNET
{
    /// <summary>
    /// Mono implementation of SerialPort is incomplete. This is to make up for that.
    /// </summary>
    public class SerialPort : IDisposable
    {
        #region Public Constructors

        /// <summary>
        /// Creates a new Serial Port
        /// </summary>
        /// <param name="portName">Name of the port (COM1, ...)</param>
        /// <param name="baudRate">Baud rate (9600, 115200, ...) </param>
        /// <param name="parity">Parity</param>
        /// <param name="dataBits">Number of data bits (7, 8, ...)</param>
        /// <param name="stopBits">Stop bits</param>
        public SerialPort(string portName, int baudRate, Parity parity, byte dataBits, StopBits stopBits)
        {
            this.IsOpen = false;
            this.IsRunning = false;
            this.PortName = portName;

            // parameters
            this._baudRate = baudRate;
            this._parity = parity;
            this._dataBits = dataBits;
            this._stopBits = stopBits;
            this._dtrControl = DtrControl.Enable; // Default
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Enumerate all the serial ports and their respected device name by accessing the registry.
        /// </summary>
        /// <returns>A dictionary containing device names (e.g. USBSER000, Serial1, ...) and port names (e.g. COM1, COM20, ...), as keys and values respectively. </returns>
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

        /// <summary>
        /// Closes this serial port instance
        /// </summary>
        public void Close()
        {
            Dispose();
            //NativeMethods.CloseHandle(serialHandle);
        }

        /// <summary>
        /// Called when this object is disposed
        /// </summary>
        public void Dispose()
        {
            Debug.WriteLine("SerialPort Disposed!");
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Opens the port.
        /// </summary>
        /// <exception cref="IOException">
        /// Raises IOException if cannot open port, or if some error happened during reading or writing port settings. Read the exception message to clarify.
        /// </exception>
        //[MethodImpl(MethodImplOptions.Synchronized)]
        public void Open()
        {
            serialHandle = NativeMethods.CreateFile("\\\\.\\" + this.PortName, (uint)(EFileAccess.FILE_GENERIC_READ | EFileAccess.FILE_GENERIC_WRITE), 0, IntPtr.Zero, (uint)ECreationDisposition.OpenExisting, (uint)EFileAttributes.Normal, IntPtr.Zero);
            if (serialHandle.IsInvalid)
                throw new IOException("Cannot open " + this.PortName);

            SetParams();

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

        /// <summary>
        /// Reads a number of bytes from the SerialPort input buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte array to write the input to. </param>
        /// <param name="offset">The offset in <paramref name="buffer"/> at which to write the bytes. </param>
        /// <param name="count">The maximum number of bytes to read. Fewer bytes are read if count is greater than the number of bytes in the input buffer. </param>
        /// <exception cref="IOException">Raises IOException on failure. Read exception message to clarify.</exception>
        public void Read(byte[] buffer, int offset, int count)
        {
            //serialStream.Read(buffer, offset, count); ;
            uint bytesRead = 0;
            byte[] unoffsetedBuffer = new byte[count];
            /*if (blocking)
                while (this.BytesToRead < count) ;*/

            bool success = NativeMethods.ReadFile(serialHandle, unoffsetedBuffer, (uint)count, out bytesRead, IntPtr.Zero);
            if (!success)
                throw new IOException("Read returned error :" + new Win32Exception((int)NativeMethods.GetLastError()).Message);

            unoffsetedBuffer.CopyTo(buffer, offset);
        }

        /// <summary>
        /// Reads all bytes from the SerialPort input buffer.
        /// </summary>
        /// <returns>An array containing the read data</returns>
        public byte[] ReadAll()
        {
            byte[] buffer = new byte[this.BytesToRead];
            Read(buffer, 0, BytesToRead);
            return buffer;
        }

        /// <summary>
        /// Run asynchronous operation (if <see cref="Async"/> is set to True)
        /// </summary>
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

        /// <summary>
        /// Stop the asynchronous operation (if running)
        /// </summary>
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

        /// <summary>
        /// Writes a specified number of bytes to the serial port using data from a buffer.
        /// </summary>
        /// <param name="buffer">The byte array that contains the data to write to the port.</param>
        /// <param name="offset">The zero-based byte offset in the <paramref name="buffer"/> parameter at which to begin copying bytes to the port.</param>
        /// <param name="count">The number of bytes to write. </param>
        public void Write(byte[] buffer, int offset, int count)
        {
            //serialStream.Write(buffer, offset, count);
            uint bytesWrote = 0;
            byte[] offsetedBuffer = new byte[count];
            buffer.CopyTo(offsetedBuffer, offset);
            try
            {
                bool success = NativeMethods.WriteFile(serialHandle, offsetedBuffer, (uint)count, out bytesWrote, IntPtr.Zero);
                if (!success)
                    throw new IOException("Write returned error :" + new Win32Exception((int)NativeMethods.GetLastError()).Message);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        /// <summary>
        /// Writes all bytes to the serial port.
        /// </summary>
        /// <param name="buffer">The byte array that contains the data to write to the port.</param>
        public void WriteAll(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Gets called when this instance is disposed.
        /// </summary>
        /// <param name="disposing"></param>
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

        /// <summary>
        /// If <see cref="Async"/> is true, this method is called when new data is available in the input buffer of the serial port.
        /// </summary>
        protected virtual void OnDataReceived()
        {
            SerialDataReceivedEventHandler handler = DataReceived;
            if (handler != null)
                handler(this, new SerialDataReceivedEventArgs(SerialData.Chars));
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

        private DCB GetParams()
        {
            DCB serialParams = new DCB();
            serialParams.DCBLength = (uint)Marshal.SizeOf(serialParams);

            if (serialHandle != null)
            {
                if (!NativeMethods.GetCommState(serialHandle, ref serialParams))
                    throw new IOException("GetCommState error!");
            }
            return serialParams;
        }

        private COMSTAT getStats()
        {
            uint flags = 0;
            COMSTAT stats = new COMSTAT();
            NativeMethods.ClearCommError(serialHandle, out flags, out stats);
            return stats;
        }

        private void SetParams()
        {
            if (serialHandle == null)
                return;

            DCB serialParams = GetParams();

            serialParams.BaudRate = (uint)this._baudRate;
            serialParams.ByteSize = (byte)this._dataBits;
            serialParams.StopBits = this._stopBits;
            serialParams.Parity = this._parity;
            serialParams.DtrControl = this._dtrControl;

            if (!NativeMethods.SetCommState(serialHandle, ref serialParams))
                throw new IOException("SetCommState error!");
        }

        #endregion Private Methods

        #region Public Events

        /// <summary>
        /// If <see cref="Async"/> is true, this event is raised when new data is available in the input buffer of the serial port.
        /// </summary>
        public event SerialDataReceivedEventHandler DataReceived;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets or sets the serial baud rate.
        /// </summary>
        public int BaudRate { get { return (int)GetParams().BaudRate; } set { _baudRate = value; SetParams(); } }

        /// <summary>
        /// Gets the number of bytes of data in the receive buffer.
        /// </summary>
        public int BytesToRead
        {
            get
            {
                return (int)getStats().cbInQue;
            }
        }

        /// <summary>
        /// Gets the number of bytes of data in the send buffer.
        /// </summary>
        public int BytesToWrite
        {
            get
            {
                return (int)getStats().cbOutQue;
            }
        }

        /// <summary>
        /// Gets or sets the standard length of data bits per byte.
        /// </summary>
        public byte DataBits { get { return GetParams().ByteSize; } set { _dataBits = value; SetParams(); } }

        /// <summary>
        /// Gets or sets a value that enables the Data Terminal Ready (DTR) signal during serial communication.
        /// </summary>
        public DtrControl DtrControl { get { return GetParams().DtrControl; } set { _dtrControl = value; SetParams(); } }

        /// <summary>
        /// Gets a value indicating the open or closed status of the <see cref="SerialPort"/> object.
        /// </summary>
        public bool IsOpen { private set; get; }

        /// <summary>
        /// Gets a value indicating the running status of the <see cref="SerialPort"/> object.
        /// </summary>
        public bool IsRunning { private set; get; }

        /// <summary>
        /// Gets or sets the parity-checking protocol.
        /// </summary>
        public Parity Parity { get { return GetParams().Parity; } set { _parity = value; SetParams(); } }

        /// <summary>
        /// Gets or sets the port for communications, including but not limited to all available COM ports.
        /// </summary>
        public string PortName { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes in the internal input buffer before a <see cref="DataReceived"/> event occurs.
        /// </summary>
        public int ReceivedBytesThreshold { get; set; }

        public StopBits StopBits { get { return GetParams().StopBits; } set { _stopBits = value; SetParams(); } }

        #endregion Public Properties

        #region Public Fields

        /// <summary>
        /// Set to true for asynchronous (event based) operation.
        /// </summary>
        public bool Async = true;

        #endregion Public Fields

        #region Private Fields

        private int _baudRate;
        private byte _dataBits;
        private DtrControl _dtrControl;
        private Parity _parity;
        private StopBits _stopBits;

        // For asynchronous operation
        private BackgroundWorker bgWorker = new BackgroundWorker();

        private bool disposed;

        //FileStream serialStream;
        private SafeFileHandle serialHandle;

        private object synclock = new object();

        #endregion Private Fields
    }
}