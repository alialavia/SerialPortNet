using System;
using System.Collections.Generic;

namespace SerialPortNET
{
    /// <summary>
    /// Interface for different classes communicating with serial port through different system calls
    /// </summary>
    public interface ILowLevelSerialPort : IDisposable
    {
        #region Public Methods

        /// <summary>
        /// Closes this serial port instance.
        /// </summary>
        void Close();

        /// <summary>
        /// Implementer should dispose all unmanaged resources here.
        /// </summary>
        void DisposeUnmanagedResources();

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
        /// Writes a specified number of bytes to the serial port using data from a buffer.
        /// </summary>
        /// <param name="buffer">The byte array that contains the data to write to the port.</param>
        /// <param name="offset">The zero-based byte offset in the <paramref name="buffer"/> parameter at which to begin copying bytes to the port.</param>
        /// <param name="count">The number of bytes to write. </param>
        void Write(byte[] buffer, int offset, int count);

        void Flush(FlushMode mode);
        #endregion Public Methods

        #region Public Properties

        /// <summary>
        /// Gets or sets the serial baud rate.
        /// </summary>
        int BaudRate { get; set; }

        Dictionary<string, string> GetPortNames();

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
        /// Gets or sets the parity-checking protocol.
        /// </summary>
        Parity Parity { get; set; }

        /// <summary>
        /// Gets or sets the port for communications, including but not limited to all available COM ports.
        /// </summary>
        string PortName { get; set; }

        /// <summary>
        /// Gets or sets the standard number of stop bits per byte.
        /// </summary>
        StopBits StopBits { get; set; }

        #endregion Public Properties


    }
}