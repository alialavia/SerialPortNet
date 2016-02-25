using System;
using System.Runtime.InteropServices;

namespace SerialPortNET
{	
	[StructLayout(LayoutKind.Sequential)]
	public struct termios {
		public int    c_iflag;    /* input flags */
		public int    c_oflag;    /* output flags */
		public int    c_cflag;    /* control flags */
		public int    c_lflag;    /* local flags */

		[MarshalAs(UnmanagedType.ByValArray, SizeConst=32)]
		public byte[]  c_cc; /* control chars */

		public int        c_ispeed;   /* input speed */
		public int        c_ospeed;   /* output speed */
	}

	public static class  Externs
	{
		// TODO: Fix libc.so.6 to a portable alternative
		[DllImport ("libc.so.6")]
		public static extern int tcgetattr(int fd, out termios t);

		[DllImport ("libc.so.6")]
		public static extern int tcsetattr (int fd, int optional_actions, ref termios t);

		[DllImport ("libc.so.6")]
		public static extern int tcsendbreak (int fd, int duration);

		[DllImport ("libc.so.6")]
		public static extern int tcdrain (int fd);

		[DllImport ("libc.so.6")]
		public static extern int tcflush (int fd, int queue_selector);

		[DllImport ("libc.so.6")]
		public static extern int tcflow (int fd, int action);

		[DllImport ("libc.so.6")]
		public static extern void cfmakeraw(termios t);

		[DllImport ("libc.so.6")]
		public static extern int cfgetispeed (ref termios t);

		[DllImport ("libc.so.6")]
		public static extern int cfgetospeed (ref termios t);

		[DllImport ("libc.so.6")]
		public static extern int cfsetispeed (ref termios t, Int32 ispeed);

		[DllImport ("libc.so.6")]
		public static extern int cfsetospeed (ref termios t, Int32 ospeed);

		[DllImport ("libc.so.6")]
		public static extern int cfsetspeed(ref termios t, Int32 speed);

		/* c_cc characters */
		public static int VINTR = 0;
		public static int VQUIT = 1;
		public static int VERASE = 2;
		public static int VKILL = 3;
		public static int VEOF = 4;
		public static int VTIME = 5;
		public static int VMIN = 6;
		public static int VSWTC = 7;
		public static int VSTART = 8;
		public static int VSTOP = 9;
		public static int VSUSP = 10;
		public static int VEOL = 11;
		public static int VREPRINT = 12;
		public static int VDISCARD = 13;
		public static int VWERASE = 14;
		public static int VLNEXT = 15;
		public static int VEOL2 = 16;

		/* c_iflag bits */
		public static int IGNBRK =  0000001;
		public static int BRKINT =  0000002;
		public static int IGNPAR =  0000004;
		public static int PARMRK =  0000010;
		public static int INPCK =   0000020;
		public static int ISTRIP =  0000040;
		public static int INLCR =   0000100;
		public static int IGNCR =   0000200;
		public static int ICRNL =   0000400;
		public static int IUCLC =   0001000;
		public static int IXON =    0002000;
		public static int IXANY =   0004000;
		public static int IXOFF =   0010000;
		public static int IMAXBEL = 0020000;
		public static int IUTF8 =   0040000;

		/* c_oflag bits */
		public static int OPOST =   0000001;
		public static int OLCUC =   0000002;
		public static int ONLCR =   0000004;
		public static int OCRNL =   0000010;
		public static int ONOCR =   0000020;
		public static int ONLRET =  0000040;
		public static int OFILL =   0000100;
		public static int OFDEL =   0000200;

		public static int VTDLY =   0040000;
		public static int   VT0 =   0000000;
		public static int   VT1 =   0040000;

		public static int  B0 = 0000000     /* hang up */;
		public static int  B50 =    0000001;
		public static int  B75 =    0000002;
		public static int  B110 =   0000003;
		public static int  B134 =   0000004;
		public static int  B150 =   0000005;
		public static int  B200 =   0000006;
		public static int  B300 =   0000007;
		public static int  B600 =   0000010;
		public static int  B1200 =  0000011;
		public static int  B1800 =  0000012;
		public static int  B2400 =  0000013;
		public static int  B4800 =  0000014;
		public static int  B9600 =  0000015;
		public static int  B19200 = 0000016;
		public static int  B38400 = 0000017;

		public static int CSIZE =   0000060;
		public static int   CS5 =   0000000;
		public static int   CS6 =   0000020;
		public static int   CS7 =   0000040;
		public static int   CS8 =   0000060;
		public static int CSTOPB =  0000100;
		public static int CREAD =   0000200;
		public static int PARENB =  0000400;
		public static int PARODD =  0001000;
		public static int HUPCL =   0002000;
		public static int CLOCAL =  0004000;

		public static int  B57600 =   0010001;
		public static int  B115200 =  0010002;
		public static int  B230400 =  0010003;
		public static int  B460800 =  0010004;
		public static int  B500000 =  0010005;
		public static int  B576000 =  0010006;
		public static int  B921600 =  0010007;
		public static int  B1000000 = 0010010;
		public static int  B1152000 = 0010011;
		public static int  B1500000 = 0010012;
		public static int  B2000000 = 0010013;
		public static int  B2500000 = 0010014;
		public static int  B3000000 = 0010015;
		public static int  B3500000 = 0010016;
		public static int  B4000000 = 0010017;
		public static int __MAX_BAUD = B4000000;

		public static int ECHO =    0000010;
		public static int ECHOE =   0000020;
		public static int ECHOK =   0000040;
		public static int ECHONL =  0000100;
		public static int NOFLSH =  0000200;
		public static int TOSTOP =  0000400;

		/* tcflow() and TCXONC use these */
		public static int TCOOFF =      0;
		public static int TCOON =       1;
		public static int TCIOFF =      2;
		public static int TCION =       3;

		/* tcflush() and TCFLSH use these */
		public static int TCIFLUSH =    0;
		public static int TCOFLUSH =    1;
		public static int TCIOFLUSH =   2;

		/* tcsetattr uses these */
		public static int TCSANOW =     0;
		public static int TCSADRAIN =   1;
		public static int TCSAFLUSH =   2;

	}		
}

