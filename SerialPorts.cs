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
			if (Helper.RunningPlatform () == Platform.Windows) {
				const string keyname = @"HARDWARE\DEVICEMAP\SERIALCOMM";
				var keys = Microsoft.Win32.Registry.LocalMachine.OpenSubKey (keyname);
				var valueNames = keys.GetValueNames ();
				var res = new Dictionary<String, String> ();
				foreach (var k in valueNames) {
					res.Add (k, keys.GetValue (k) as String);
				}
			}
			return res;
		}
	}
}

