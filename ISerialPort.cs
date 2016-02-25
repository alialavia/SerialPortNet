using System;

namespace SerialPortNET
{
    /// <summary>
    /// All SerialPort objects in SerialPortNET implement this interface.
    /// </summary>
    public interface ISerialPort : IDisposable
    {
        #region Public Methods

        /// <summary>
        /// Closes this serial port instance.
        /// </summary>
        void Close();

        /// <summary>
        /// Opens the port.
        /// </summary>
        void Open();

        /// <summary>
        /// Reads a number of bytes from the SerialPort input buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte array to write the input to. </param>
        /// <param name="offset">The offset in <paramref name="buffer"/> at which to write the bytes. </param>
        /// <param name="count">The maximum number of bytes to read. Fewer bytes are read if count is greater than the number of bytes in the input buffer. </param>
        void Read(byte[] buffer, int offset, int count);

        /// <summary>
        /// Reads all bytes from the SerialPort input buffer.
        /// </summary>
        /// <returns>An array containing the read data</returns>
        byte[] ReadAll();

        /// <summary>
        /// Run asynchronous operation.
        /// </summary>
        void RunAsync();

        /// <summary>
        /// Stop the asynchronous operation.
        /// </summary>
        void StopAsync();

        /// <summary>
        /// Writes a specified number of bytes to the serial port using data from a buffer.
        /// </summary>
        /// <param name="buffer">The byte array that contains the data to write to the port.</param>
        /// <param name="offset">The zero-based byte offset in the <paramref name="buffer"/> parameter at which to begin copying bytes to the port.</param>
        /// <param name="count">The number of bytes to write. </param>
        void Write(byte[] buffer, int offset, int count);

        /// <summary>
        /// Writes all bytes to the serial port.
        /// </summary>
        /// <param name="buffer">The byte array that contains the data to write to the port.</param>
        void WriteAll(byte[] buffer);

        #endregion Public Methods

        #region Public Events

        /// <summary>
        /// If RunAsync() is called, this event is raised when new data is available in the input buffer of the serial port.
        /// </summary>
        event SerialDataReceivedEventHandler DataReceived;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets or sets the serial baud rate.
        /// </summary>
        int BaudRate { get; set; }

        /// <summary>
        /// Gets the number of bytes of data in the receive buffer.
        /// </summary>
        int BytesToRead { get; }

        /// <summary>
        /// Gets the number of bytes of data in the send buffer.
        /// </summary>
        int BytesToWrite { get; }

        /// <summary>
        /// Gets or sets the standard length of data bits per byte.
        /// </summary>
        byte DataBits { get; set; }

        /// <summary>
        /// Gets or sets a value that enables the Data Terminal Ready (DTR) signal during serial communication.
        /// </summary>
        DtrControl DtrControl { get; set; }

        /// <summary>
        /// Gets a value indicating the open or closed status of the <see cref="SerialPort"/> object.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Gets a value indicating the running status of the <see cref="SerialPort"/> object.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets or sets the parity-checking protocol.
        /// </summary>
        Parity Parity { get; set; }

        /// <summary>
        /// Gets or sets the port for communications, including but not limited to all available COM ports.
        /// </summary>
        string PortName { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes in the internal input buffer before a <see cref="DataReceived"/> event occurs.
        /// </summary>
        int ReceivedBytesThreshold { get; set; }

        /// <summary>
        /// Gets or sets the standard number of stop bits per byte.
        /// </summary>
        StopBits StopBits { get; set; }

        #endregion Public Properties
    }
}