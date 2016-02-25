using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace SerialPortNET
{
    /// <summary>
    /// Mono implementation of SerialPort is incomplete. This is to make up for that.
    /// </summary>
    public class SerialPort : ISerialPort
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
            switch (Helper.RunningPlatform())
            {
                case Platform.Windows:
                    lowLevelSerialPort = new SerialPortWin32(portName, baudRate, parity, dataBits, stopBits);
                    break;

                case Platform.Linux:
                    throw new NotImplementedException();
                case Platform.Mac:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Enumerate all the serial ports and their respected device name by accessing the registry.
        /// </summary>
        /// <returns>A dictionary containing device names (e.g. USBSER000, Serial1, ...) and port names (e.g. COM1, COM20, ...), as keys and values respectively. </returns>
        public static Dictionary<String, String> EnumerateSerialPorts()
        {
            var res = new Dictionary<String, String>();
            switch (Helper.RunningPlatform())
            {
                case Platform.Windows:
                    const string keyname = @"HARDWARE\DEVICEMAP\SERIALCOMM";
                    var keys = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyname);
                    var valueNames = keys.GetValueNames();

                    foreach (var k in valueNames)
                    {
                        res.Add(k, keys.GetValue(k) as String);
                    }
                    break;

                case Platform.Linux:
                    throw new NotImplementedException();
                case Platform.Mac:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
            return res;
        }

        /// <summary>
        /// Closes this serial port instance
        /// </summary>
        public void Close()
        {
            Dispose();
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
        public void Open()
        {
            lowLevelSerialPort.Open();
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
            lowLevelSerialPort.Read(buffer, offset, count);
        }

        /// <summary>
        /// Reads all bytes from the SerialPort input buffer.
        /// </summary>
        /// <returns>An array containing the read data</returns>
        public virtual byte[] ReadAll()
        {
            byte[] buffer = new byte[this.BytesToRead];
            Read(buffer, 0, BytesToRead);
            return buffer;
        }

        /// <summary>
        /// Run asynchronous operation.
        /// </summary>
        public void RunAsync()
        {
            if (IsRunning)
                return;
            if (!IsOpen)
                return;

            bgWorker.DoWork += new DoWorkEventHandler(BgWorker_DoWork);
            bgWorker.RunWorkerAsync();

            IsRunning = true;
        }

        /// <summary>
        /// Stop the asynchronous operation.
        /// </summary>
        public void StopAsync()
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
            lowLevelSerialPort.Write(buffer, offset, count);
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
                lowLevelSerialPort.DisposeUnmanagedResources();
            }

            disposed = true;
        }

        /// <summary>
        /// If RunAsync() is called, this method is called when new data is available in the input buffer of the serial port.
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

        #endregion Private Methods

        #region Public Events

        /// <summary>
        /// If RunAsync() is called, this event is raised when new data is available in the input buffer of the serial port.
        /// </summary>
        public event SerialDataReceivedEventHandler DataReceived;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets or sets the serial baud rate.
        /// </summary>
        public int BaudRate { get { return lowLevelSerialPort.BaudRate; } set { lowLevelSerialPort.BaudRate = value; } }

        /// <summary>
        /// Gets the number of bytes of data in the receive buffer.
        /// </summary>
        public int BytesToRead
        {
            get { return lowLevelSerialPort.BytesToRead; }
        }

        /// <summary>
        /// Gets the number of bytes of data in the send buffer.
        /// </summary>
        public int BytesToWrite
        {
            get { return lowLevelSerialPort.BytesToWrite; }
        }

        /// <summary>
        /// Gets or sets the standard length of data bits per byte.
        /// </summary>
        public byte DataBits { get { return lowLevelSerialPort.DataBits; } set { lowLevelSerialPort.DataBits = value; } }

        /// <summary>
        /// Gets or sets a value that enables the Data Terminal Ready (DTR) signal during serial communication.
        /// </summary>
        public DtrControl DtrControl { get { return lowLevelSerialPort.DtrControl; } set { lowLevelSerialPort.DtrControl = value; } }

        /// <summary>
        /// Gets a value indicating the open or closed status of the <see cref="System.IO.Ports.SerialPort"/> object.
        /// </summary>
        public bool IsOpen
        {
            get { return lowLevelSerialPort.IsOpen; }
        }

        /// <summary>
        /// Gets a value indicating the running status of the <see cref="System.IO.Ports.SerialPort"/> object.
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Gets or sets the parity-checking protocol.
        /// </summary>
        public Parity Parity { get { return lowLevelSerialPort.Parity; } set { lowLevelSerialPort.Parity = value; } }

        /// <summary>
        /// Gets or sets the port for communications, including but not limited to all available COM ports.
        /// </summary>
        public string PortName { get { return lowLevelSerialPort.PortName; } set { lowLevelSerialPort.PortName = value; } }

        /// <summary>
        /// Gets or sets the number of bytes in the internal input buffer before a <see cref="DataReceived"/> event occurs.
        /// </summary>
        public int ReceivedBytesThreshold { get; set; }

        /// <summary>
        /// Gets or sets the standard number of stop bits per byte.
        /// </summary>
        public StopBits StopBits { get { return lowLevelSerialPort.StopBits; } set { lowLevelSerialPort.StopBits = value; } }

        #endregion Public Properties

        #region Private Fields

        // For asynchronous operation
        private BackgroundWorker bgWorker = new BackgroundWorker();

        private bool disposed;
        private ILowLevelSerialPort lowLevelSerialPort;

        #endregion Private Fields
    }
}