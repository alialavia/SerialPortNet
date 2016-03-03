using System;
using System.IO;
using Mono.Unix.Native;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Threading;
using SerialPortNET;
//using System.IO.Ports;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TestSerialPort
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            // Used for performance measurement

            /* 
			DateTime t;
			TimeSpan s;
			t = DateTime.Now;
			*/
            SerialPort sp = new SerialPort("COM18", 9600, Parity.None, 8, StopBits.One);
            sp.ReceivedBytesThreshold = 1;

            sp.DataReceived += (object sender, SerialDataReceivedEventArgs e) =>
            {
                Console.WriteLine(sp.ReadLine());
            };

            sp.Open();
            Thread.Sleep(5000);
            sp.RunAsync();
            //Console.WriteLine("{0}", sp.IsOpen);
            Console.ReadKey();
        }
    }
}
