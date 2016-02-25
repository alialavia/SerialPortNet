using System;
using System.IO;
using Mono.Unix.Native;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Threading;



namespace Ubuntu
{
	class MainClass
	{
		public unsafe static void Main (string[] args)
		{							
			int fd = Syscall.open("/dev/ttyACM0", OpenFlags.O_RDWR | OpenFlags.O_NOCTTY, FilePermissions.ACCESSPERMS);
			if (fd < 0) {
				Console.WriteLine ("Err: {0}", Syscall.GetLastError ());
				return;
			}
			Syscall.fcntl (fd, FcntlCommand.F_SETFL, 0);
			termios t = new termios();
			/*
			Console.WriteLine ("ispeed={0}, ospeed={1}", t.c_ispeed, t.c_ospeed);
			Console.WriteLine ("Err: {0}", Syscall.GetLastError ());
			Console.WriteLine ("Now");
			*/
			if (Externs.tcgetattr (fd, out t) < 0) {
				Console.WriteLine ("Err: {0}", Syscall.GetLastError ());
				return;
			}
			Externs.cfsetispeed (ref t, Externs.B9600);
			Externs.cfsetospeed (ref t, Externs.B9600);

			t.c_cflag |= (Externs.CLOCAL | Externs.CREAD);

			Externs.tcsetattr (fd, 0, ref t);
			Thread.Sleep (1000);

			Console.WriteLine ("ispeed={0}, ospeed={1}", Externs.cfgetispeed(ref t), Externs.cfgetospeed(ref t));
			Console.WriteLine ("Done!");

			//Console.WriteLine ("{0}", Externs.tcgetsid(fd));
			return;

			/*
			var buf = new byte[] {65,66,67,68,69,70,71,72,73,74};
			fixed (byte* bytes = buf) {
				while (true) {				
					Syscall.write (fd, (IntPtr)bytes, 5);
					Console.Write ("{0}", buf [0]);
				}
			}*/
			Syscall.close (fd);	
		}
	}
}
