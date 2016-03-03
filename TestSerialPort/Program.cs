using System;
using System.IO;
using Mono.Unix.Native;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Threading;
using SerialPortNET;
using System.Collections.Generic;
using System.Reflection;
using SerialPortNET;
using System.Text;

namespace TestSerialPort
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// Used for performance measurement
			
            /* 
			DateTime t;
			TimeSpan s;
			t = DateTime.Now;
			*/

			var sp = new SerialPort("COM18", 115200, Parity.None, 8, StopBits.One);

            sp.DataReceived += (object sender, SerialDataReceivedEventArgs e) => {
				var b = sp.ReadAll();
				for (int i = 0; i < b.Length; i++)
					Console.WriteLine (b[i]);
			};

			sp.Open ();
			sp.RunAsync ();

			Console.WriteLine ("{0}", sp.IsOpen);
			Console.ReadKey ();
		}
	}
}
