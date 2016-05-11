using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace SerialPortNET
{
    /// <summary>
    /// Mono implementation of SerialPort is incomplete. This is to make up for that.
    /// </summary>
    public class SerialPort : ISerialPort
    {
        #region Public Constructors
        /// <summary>
        /// Enumerate all the serial ports and their respected device name by accessing the registry.
        /// </summary>
        /// <returns>A dictionary containing device names (e.g. USBSER000, Serial1, ...) and port names (e.g. COM1, COM20, ...), as keys and values respectively. </returns>
        public static Dictionary<String, String> EnumerateSerialPorts()
        {
            var res = new Dictionary<String, String>();
            switch (Helper.RunningPlatform)
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
        /// This will create a temporary serial port depending on the operating system.
        /// It also finds the related constructor to call it from <see cref="SerialPort"/> constructor.
        /// </summary>
        static SerialPort()
        {
            switch (Helper.RunningPlatform)
            {
                case Platform.Windows:
                    tempSerialPort = new SerialPortWin32();
                    break;

                case Platform.Linux:
                    tempSerialPort = new SerialPortPOSIX();
                    break;

                default:
                    if (PlatformSpecificImplementation == null)
                        throw new NotImplementedException("SerialPort not implemented for " + Helper.RunningPlatform.ToString());
                    tempSerialPort = PlatformSpecificImplementation.Invoke(Helper.RunningPlatform);
                    break;
            }
            //serialConstructor = (tempSerialPort.GetType()).GetConstructor(new Type[] {
             //   typeof(string), typeof(int), typeof(Parity), typeof(byte), typeof(StopBits) });
        }
        public static HandleUnimplementedPlatforms PlatformSpecificImplementation;
        public delegate ILowLevelSerialPort HandleUnimplementedPlatforms(Platform p);
        
        public string ReadExisting()
        {
            return Encoding.ASCII.GetString(ReadAll());
        }

        /// <summary>
        /// Creates a new Serial Port
        /// </summary>
        /// <param name="portName">Name of the port (COM1, ...)</param>
        /// <param name="baudRate">Baud rate (9600, 115200, ...) </param>
        /// <param name="parity">Parity</param>
        /// <param name="dataBits">Number of data bits (5, 6, 7, or 8)</param>
        /// <param name="stopBits">Stop bits</param>
        public SerialPort(string portName, int baudRate, Parity parity, byte dataBits, StopBits stopBits)
        {
            NewLine = "\r\n";
            lowLevelSerialPort = tempSerialPort;
            lowLevelSerialPort.PortName = portName;
            lowLevelSerialPort.BaudRate = baudRate;
            lowLevelSerialPort.Parity = parity;
            lowLevelSerialPort.DataBits = dataBits;
            lowLevelSerialPort.StopBits = stopBits;                
        }

        public SerialPort() : this("COM1", 115200, Parity.Even, 8, StopBits.Two) { }
        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Enumerate all the serial ports and their respected device name by accessing the registry.
        /// </summary>
        /// <returns>A dictionary containing device names (e.g. USBSER000, Serial1, ...) and port names (e.g. COM1, COM20, ...), as keys and values respectively. </returns>
        public static Dictionary<String, String> GetPortNames()
        {
            return tempSerialPort.GetPortNames();
        }

        /// <summary>
        /// Closes this serial port instance
        /// </summary>
        public void Close()
        {
            StopAsync();
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
            Flush();
        }
        

        /// <summary>
        /// Reads a number of bytes from the SerialPort input buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte array to write the input to.</param>
        /// <param name="offset">The offset in <paramref name="buffer"/> at which to write the bytes.</param>
        /// <param name="count">The maximum number of bytes to read. Fewer bytes are read if count is greater than the number of bytes in the input buffer.</param>
        /// <exception cref="IOException">Raises IOException on failure. Read exception message to clarify.</exception>
        public void Read(byte[] buffer, int offset, int count)
        {
            lowLevelSerialPort.Read(buffer, offset, count);
        }

        /// <summary>
        /// Reads all bytes from the input buffer.
        /// </summary>
        /// <returns>An array containing the read data</returns>
        public virtual byte[] ReadAll()
        {
            int btr = this.BytesToRead;
            byte[] buffer = new byte[btr];
            Read(buffer, 0, btr);
            return buffer;
        }

        /// <summary>
        /// Reads all bytes from the input buffer. Will read the <see cref="NewLine"/> character(s) but will not return it.
        /// Note that this functions blocks until NewLine is seen.
        /// </summary>
        /// <returns></returns>
        public string ReadLine()
        {
            List<byte> bytes = new List<byte>();
            char[] newLineCharArray = NewLine.ToCharArray();
            while (true)
            {
                if (lowLevelSerialPort.BytesToRead < 1)
                    continue;
                byte[] buffer = new byte[1];
                lowLevelSerialPort.Read(buffer, 0, 1);
                bytes.Add(buffer[0]);
                if (bytes.Count < NewLine.Length)
                    continue;

                bool foundNewLine = true;
                for (int i = 0; i < NewLine.Length; i++)
                    if (bytes[bytes.Count - i - 1] != newLineCharArray[NewLine.Length - i - 1])
                    {
                        foundNewLine = false;
                        break;
                    }
                if (foundNewLine)
                    return System.Text.Encoding.Default.GetString(bytes.ToArray(), 0, bytes.Count - NewLine.Length);
            }
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
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
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
            {
                if (this.BytesToRead >= this.ReceivedBytesThreshold)
                {
                    OnDataReceived();
                }
            }
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            (sender as BackgroundWorker).RunWorkerAsync();
        }

        public void Flush(FlushMode mode = FlushMode.InputOutput)
        {
            lowLevelSerialPort.Flush(mode);
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

        public string NewLine { get; set; }

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

        //private static ConstructorInfo serialConstructor;
        private static ILowLevelSerialPort tempSerialPort;

        // For asynchronous operation
        private BackgroundWorker bgWorker = new BackgroundWorker();

        private bool disposed;
        private ILowLevelSerialPort lowLevelSerialPort;

        #endregion Private Fields
    }
}