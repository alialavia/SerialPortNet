using System;
using System.Collections.Generic;

namespace SerialPortNET
{
	public static class SerialPorts
	{
		/// <summary>
		/// Enumerate all the serial ports and their respected device name by accessing the registry.
		/// </summary>
		/// <returns>A dictionary containing device names (e.g. USBSER000, Serial1, ...) and port names (e.g. COM1, COM20, ...), as keys and values respectively. </returns>
		public static Dictionary<String, String> EnumerateSerialPorts()
		{
            var res = new Dictionary<String, String>();
            switch (platform)
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

        public static ISerialPort New(string portName, int baudRate, Parity parity, byte dataBits, StopBits stopBits)
        {
            switch (platform)
            {
                case Platform.Windows:
                    return new SerialPortWin32(portName, baudRate, parity, dataBits, stopBits);                    
                case Platform.Linux:
                    throw new NotImplementedException();
                case Platform.Mac:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        private static Platform platform = Helper.RunningPlatform();

    }
}

