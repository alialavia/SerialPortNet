using System;
using System.Collections.Generic;
using System.Text;

namespace SerialPortNET
{

    /// <summary>
    /// Flush mode indicates which buffer to flush
    /// </summary>
    public enum FlushMode
    {
        Input,
        Output,
        InputOutput
    }
    /// <summary>
    /// DTR line and handshaking control
    /// </summary>
    public enum DtrControl : int
    {
        /// <summary>
        /// Disables the DTR line when the device is opened and leaves it disabled.
        /// </summary>
        Disable = 0,

        /// <summary>
        /// Enables the DTR line when the device is opened and leaves it on.
        /// </summary>
        Enable = 1,

        /// <summary>
        /// Enables DTR handshaking. If handshaking is enabled, it is an error for the application to adjust the line by
        /// using the EscapeCommFunction function.
        /// </summary>
        Handshake = 2
    }

    /// <summary>
    /// Specifies the parity bit for a <see cref="SerialPort"/> object.
    /// </summary>
    public enum Parity : byte
    {
        /// <summary>
        /// No parity check occurs.
        /// </summary>
        None = 0,

        /// <summary>
        /// Sets the parity bit so that the count of bits set is an odd number.
        /// </summary>
        Odd = 1,

        /// <summary>
        /// Sets the parity bit so that the count of bits set is an even number.
        /// </summary>
        Even = 2,

        /// <summary>
        /// Leaves the parity bit set to 1.
        /// </summary>
        Mark = 3,

        /// <summary>
        /// Leaves the parity bit set to 0.
        /// </summary>
        Space = 4,
    }

    /// <summary>
    /// RTS line control
    /// </summary>
    public enum RtsControl : int
    {
        /// <summary>
        /// Disables the RTS line when the device is opened and leaves it disabled.
        /// </summary>
        Disable = 0,

        /// <summary>
        /// Enables the RTS line when the device is opened and leaves it on.
        /// </summary>
        Enable = 1,

        /// <summary>
        /// Enables RTS handshaking. The driver raises the RTS line when the "type-ahead" (input) buffer
        /// is less than one-half full and lowers the RTS line when the buffer is more than
        /// three-quarters full. If handshaking is enabled, it is an error for the application to
        /// adjust the line by using the EscapeCommFunction function.
        /// </summary>
        Handshake = 2,

        /// <summary>
        /// Specifies that the RTS line will be high if bytes are available for transmission. After
        /// all buffered bytes have been sent, the RTS line will be low.
        /// </summary>
        Toggle = 3
    }

    /// <summary>
    /// Specifies the number of stop bits used on the <see cref="SerialPort"/> object.
    /// </summary>
    public enum StopBits : byte
    {
        /// <summary>
        /// One stop bit is used.
        /// </summary>
        One = 0,

        /// <summary>
        /// 1.5 stop bits are used.
        /// </summary>
        OnePointFive = 1,

        /// <summary>
        /// Two stop bits are used.
        /// </summary>
        Two = 2
    }

}
