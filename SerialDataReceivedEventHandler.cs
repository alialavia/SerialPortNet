namespace SerialPortNET
{
    /// <summary>
    /// Represents the method that will handle the DataReceived event of a SerialPort object.
    /// </summary>
    /// <param name="sender">The sender of the event, which is the <see cref="SerialPort"/> object.</param>
    /// <param name="e">A <see cref="SerialDataReceivedEventArgs"/> object that contains the event data.</param>
    public delegate void SerialDataReceivedEventHandler(object sender, SerialDataReceivedEventArgs e);
}