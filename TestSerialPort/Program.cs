using System;
using System.IO;
using Mono.Unix.Native;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Threading;
using SerialPortNET;

namespace TestSerialPort
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            AbstractTest AT = new AbstractTest();
            Console.WriteLine("AbstractTest.t = {0}", AT.t);

            AbstractTest T1 = new AbstractTest();
            Console.WriteLine("AbstractTest.t = {0}", AT.t);

            AbstractTest T2 = new AbstractTest();
            Console.WriteLine("AbstractTest.t = {0}", AT.t);

            var s = new SerialPort("COM18", 9200, Parity.None, 8, StopBits.One);
            s.Open();
            Console.WriteLine(s.IsOpen);
        }
	}
}
