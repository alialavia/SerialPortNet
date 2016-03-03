using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using MacroResolver;
using Mono.Unix.Native;

namespace SerialPortNET
{	
	[StructLayout(LayoutKind.Sequential)]
	internal struct termios {
		public int    c_iflag;    /* input flags */
		public int    c_oflag;    /* output flags */
		public int    c_cflag;    /* control flags */
		public int    c_lflag;    /* local flags */

		[MarshalAs(UnmanagedType.ByValArray, SizeConst=32)]
		public byte[]  c_cc; /* control chars */

		public int        c_ispeed;   /* input speed */
		public int        c_ospeed;   /* output speed */
	}

	internal static class Syscalls
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

		[DllImport ("libc.so.6")]
		public static extern int ioctl(int fd, Int32 options, ref int status);

		[DllImport ("libc.so.6")]
		public static extern int fcntl(int fd, int cmd, int arg);
	}		

	public static class Macros
	{
		private static MacroHelper macroHelper = new MacroHelper(typeof( Macros), new String[]{"termios.h", "unistd.h", "sys/ioctl.h", "fcntl.h"}); 
		public static int ICANON {get { return macroHelper.GetMacro(); }}
		public static int ISIG {get { return macroHelper.GetMacro(); }}

		public static int CNEW_RTSCTS {get { return macroHelper.GetMacro(); }}
		public static int CRTSCTS {get { return macroHelper.GetMacro(); }}
		public static int O_NDELAY {get { return macroHelper.GetMacro(); }}
		public static int F_SETFL {get { return macroHelper.GetMacro(); }}
		public static int FIONREAD {get { return macroHelper.GetMacro(); }}
		/* c_cc characters */
		public static int VINTR {get { return macroHelper.GetMacro(); }}
		public static int VQUIT {get { return macroHelper.GetMacro(); }}
		public static int VERASE {get { return macroHelper.GetMacro(); }}
		public static int VKILL {get { return macroHelper.GetMacro(); }}
		public static int VEOF {get { return macroHelper.GetMacro(); }}
		public static int VTIME {get { return macroHelper.GetMacro(); }}
		public static int VMIN {get { return macroHelper.GetMacro(); }}
		public static int VSWTC {get { return macroHelper.GetMacro(); }}
		public static int VSTART {get { return macroHelper.GetMacro(); }}
		public static int VSTOP {get { return macroHelper.GetMacro(); }}
		public static int VSUSP {get { return macroHelper.GetMacro(); }}
		public static int VEOL {get { return macroHelper.GetMacro(); }}
		public static int VREPRINT {get { return macroHelper.GetMacro(); }}
		public static int VDISCARD {get { return macroHelper.GetMacro(); }}
		public static int VWERASE {get { return macroHelper.GetMacro(); }}
		public static int VLNEXT {get { return macroHelper.GetMacro(); }}
		public static int VEOL2 {get { return macroHelper.GetMacro(); }}

		/* c_iflag bits */
		public static int IGNBRK {get { return macroHelper.GetMacro(); }}
		public static int BRKINT {get { return macroHelper.GetMacro(); }}
		public static int IGNPAR {get { return macroHelper.GetMacro(); }}
		public static int PARMRK {get { return macroHelper.GetMacro(); }}
		public static int INPCK {get { return macroHelper.GetMacro(); }}
		public static int ISTRIP {get { return macroHelper.GetMacro(); }}
		public static int INLCR {get { return macroHelper.GetMacro(); }}
		public static int IGNCR {get { return macroHelper.GetMacro(); }}
		public static int ICRNL {get { return macroHelper.GetMacro(); }}
		public static int IUCLC {get { return macroHelper.GetMacro(); }}
		public static int IXON {get { return macroHelper.GetMacro(); }}
		public static int IXANY {get { return macroHelper.GetMacro(); }}
		public static int IXOFF {get { return macroHelper.GetMacro(); }}
		public static int IMAXBEL {get { return macroHelper.GetMacro(); }}
		public static int IUTF8 {get { return macroHelper.GetMacro(); }}

		/* c_oflag bits */
		public static int OPOST {get { return macroHelper.GetMacro(); }}
		public static int OLCUC {get { return macroHelper.GetMacro(); }}
		public static int ONLCR {get { return macroHelper.GetMacro(); }}
		public static int OCRNL {get { return macroHelper.GetMacro(); }}
		public static int ONOCR {get { return macroHelper.GetMacro(); }}
		public static int ONLRET {get { return macroHelper.GetMacro(); }}
		public static int OFILL {get { return macroHelper.GetMacro(); }}
		public static int OFDEL {get { return macroHelper.GetMacro(); }}

		public static int VTDLY {get { return macroHelper.GetMacro(); }}
		public static int   VT0 {get { return macroHelper.GetMacro(); }}
		public static int   VT1 {get { return macroHelper.GetMacro(); }}

		public static int B0 {get { return macroHelper.GetMacro(); }}     /* hang up */
		public static int  B50 {get { return macroHelper.GetMacro(); }}
		public static int  B75 {get { return macroHelper.GetMacro(); }}
		public static int  B110 {get { return macroHelper.GetMacro(); }}
		public static int  B134 {get { return macroHelper.GetMacro(); }}
		public static int  B150 {get { return macroHelper.GetMacro(); }}
		public static int  B200 {get { return macroHelper.GetMacro(); }}
		public static int  B300 {get { return macroHelper.GetMacro(); }}
		public static int  B600 {get { return macroHelper.GetMacro(); }}
		public static int  B1200 {get { return macroHelper.GetMacro(); }}
		public static int  B1800 {get { return macroHelper.GetMacro(); }}
		public static int  B2400 {get { return macroHelper.GetMacro(); }}
		public static int  B4800 {get { return macroHelper.GetMacro(); }}
		public static int  B9600 {get { return macroHelper.GetMacro(); }}
		public static int  B19200 {get { return macroHelper.GetMacro(); }}
		public static int  B38400 {get { return macroHelper.GetMacro(); }}
		public static int  B57600 {get { return macroHelper.GetMacro(); }}
		public static int  B115200 {get { return macroHelper.GetMacro(); }}
		public static int  B230400 {get { return macroHelper.GetMacro(); }}
		public static int  B460800 {get { return macroHelper.GetMacro(); }}
		public static int  B500000 {get { return macroHelper.GetMacro(); }}
		public static int  B576000 {get { return macroHelper.GetMacro(); }}
		public static int  B921600 {get { return macroHelper.GetMacro(); }}
		public static int  B1000000 {get { return macroHelper.GetMacro(); }}
		public static int  B1152000 {get { return macroHelper.GetMacro(); }}
		public static int  B1500000 {get { return macroHelper.GetMacro(); }}
		public static int  B2000000 {get { return macroHelper.GetMacro(); }}
		public static int  B2500000 {get { return macroHelper.GetMacro(); }}
		public static int  B3000000 {get { return macroHelper.GetMacro(); }}
		public static int  B3500000 {get { return macroHelper.GetMacro(); }}
		public static int  B4000000 {get { return macroHelper.GetMacro(); }}
		public static int __MAX_BAUD {get { return macroHelper.GetMacro(); }}

		public static int CSIZE {get { return macroHelper.GetMacro(); }}
		public static int   CS5 {get { return macroHelper.GetMacro(); }}
		public static int   CS6 {get { return macroHelper.GetMacro(); }}
		public static int   CS7 {get { return macroHelper.GetMacro(); }}
		public static int   CS8 {get { return macroHelper.GetMacro(); }}
		public static int CSTOPB {get { return macroHelper.GetMacro(); }}
		public static int CREAD {get { return macroHelper.GetMacro(); }}
		public static int PARENB {get { return macroHelper.GetMacro(); }}
		public static int PARODD {get { return macroHelper.GetMacro(); }}
		public static int HUPCL {get { return macroHelper.GetMacro(); }}
		public static int CLOCAL {get { return macroHelper.GetMacro(); }}

		public static int ECHO {get { return macroHelper.GetMacro(); }}
		public static int ECHOE {get { return macroHelper.GetMacro(); }}
		public static int ECHOK {get { return macroHelper.GetMacro(); }}
		public static int ECHONL {get { return macroHelper.GetMacro(); }}
		public static int NOFLSH {get { return macroHelper.GetMacro(); }}
		public static int TOSTOP {get { return macroHelper.GetMacro(); }}

		/* tcflow() and TCXONC use these */
		public static int TCOOFF {get { return macroHelper.GetMacro(); }}
		public static int TCOON {get { return macroHelper.GetMacro(); }}
		public static int TCIOFF {get { return macroHelper.GetMacro(); }}
		public static int TCION {get { return macroHelper.GetMacro(); }}

		/* tcflush() and TCFLSH use these */
		public static int TCIFLUSH {get { return macroHelper.GetMacro(); }}
		public static int TCOFLUSH {get { return macroHelper.GetMacro(); }}
		public static int TCIOFLUSH {get { return macroHelper.GetMacro(); }}

		/* tcsetattr uses these */
		public static int TCSANOW {get { return macroHelper.GetMacro(); }}
		public static int TCSADRAIN {get { return macroHelper.GetMacro(); }}
		public static int TCSAFLUSH {get { return macroHelper.GetMacro(); }}

		/* ioctl settings */

		public static int TIOCMGET{get { return macroHelper.GetMacro(); }}
		public static int TIOCMSET{get { return macroHelper.GetMacro(); }}
		public static int TIOCM_DTR{get { return macroHelper.GetMacro(); }}
	}

}

