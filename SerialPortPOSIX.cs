using System;
using Mono.Unix.Native;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace SerialPortNET
{
	class SerialPortPOSIX : ILowLevelSerialPort
	{
		int _baudRate;
		Parity _parity;
		byte _dataBits;
		DtrControl _dtrControl;
		StopBits _stopBits;

		public SerialPortPOSIX (String portName, int baudRate, Parity parity, byte dataBits, StopBits stopBits)
		{
			this.IsOpen = false;
			this.PortName = portName;

			// parameters
			this._baudRate = baudRate;
			this._parity = parity;
			this._dataBits = dataBits;
			this._stopBits = stopBits;
			this._dtrControl = DtrControl.Enable; // Default
		}

        public SerialPortPOSIX()
        {
        }

        public int BaudRate {
			get {
				return _baudRate;
			}

			set { 
				_baudRate = value; 
				setOptions ();
			}
		}

		public int BytesToRead {
			get { 
				int bytes = 0;
				Syscalls.ioctl(_FileDescriptor, Macros.FIONREAD, ref bytes); 
				return bytes;
			}
		}

		public int BytesToWrite {
			get;
		}

		public byte DataBits {
			get { return _dataBits; }
			set {
				_dataBits = value;
				setOptions ();
			}
		}

		public DtrControl DtrControl {
			get { return _dtrControl; }
			set {
				_dtrControl = value;
				setOptions ();
			}
		}

		public bool IsOpen {
			get;
			protected set;
		}

		public Parity Parity {
			get { return _parity; }
			set {
				_parity = value;
				setOptions ();
			}
		}

		public string PortName {
			get;
			set;
		}

		public StopBits StopBits {
			get { return _stopBits; }
			set {
				_stopBits = value;
				setOptions ();
			}
		}

		public void Close ()
		{
			Dispose ();
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		bool disposed;

		/// <summary>
		/// Gets called when this instance is disposed.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose (bool disposing)
		{
			if (disposed)
				return;

			if (disposing) {
				DisposeUnmanagedResources ();
			}

			disposed = true;
		}

		public void DisposeUnmanagedResources ()
		{
			Syscall.close (_FileDescriptor);
		}
		//FileStream fileStream;
		public void Open ()
		{
			int fd = Syscall.open (PortName, OpenFlags.O_RDWR | OpenFlags.O_NOCTTY | OpenFlags.O_NONBLOCK , FilePermissions.ALLPERMS);
			if (fd <= 0) {
				
				throw new IOException ("Cannot open the port.");
			}

			IsOpen = true;
			_FileDescriptor = fd;

			setOptions ();
		}

		private static int findBaudrate (int bps)
		{
			var baudValues = new Dictionary<int, int> () {				
				{ 0, Macros.B0 },
				{ 50,    Macros.B50 },
				{ 75,    Macros.B75 },
				{ 110,   Macros.B110 },
				{ 134,   Macros.B134 },
				{ 150,   Macros.B150 },
				{ 200,   Macros.B200 },
				{ 300,   Macros.B300 },
				{ 600,   Macros.B600 },
				{ 1200,  Macros.B1200 },
				{ 1800,  Macros.B1800 },
				{ 2400,  Macros.B2400 },
				{ 4800,  Macros.B4800 },
				{ 9600,  Macros.B9600 },
				{ 19200, Macros.B19200 },
				{ 38400, Macros.B38400 },
				{ 57600,   Macros.B57600 },
				{ 115200,  Macros.B115200 },
				{ 230400,  Macros.B230400 },
				{ 460800,  Macros.B460800 },
				{ 500000,  Macros.B500000 },
				{ 576000,  Macros.B576000 },
				{ 921600,  Macros.B921600 },
				{ 1000000, Macros.B1000000 },
				{ 1152000, Macros.B1152000 },
				{ 1500000, Macros.B1500000 },
				{ 2000000, Macros.B2000000 },
				{ 2500000, Macros.B2500000 },
				{ 3000000, Macros.B3000000 },
				{ 3500000, Macros.B3500000 },
				{ 4000000, Macros.B4000000 }
			};
			int bestKey = 0, minDif = int.MaxValue;
			foreach (var k in baudValues.Keys) {
				int temp = Math.Abs (k - bps);
				if (temp < minDif) {
					minDif = temp;
					bestKey = k;
				}				
			}
			return baudValues [bestKey];
		}

		public termios getOptions()
		{
			termios options;
			Syscalls.tcgetattr (_FileDescriptor, out options);
			return options;
		}
		private void setOptions ()
		{
			termios options;
			Syscalls.tcgetattr (_FileDescriptor, out options);

			/* Set baud rate */
			Syscalls.cfsetspeed (ref options, findBaudrate (_baudRate));

			/* These will ensure that the program does not become the 'owner' of the 
			 * port subject to sporatic job control and hangup signals, 
			 * and also that the serial interface driver will read incoming data bytes.
			*/

			options.c_cflag |= (Macros.CLOCAL | Macros.CREAD);

			/* Set byte size */
			options.c_cflag &= ~Macros.CSIZE; /* Mask the character size bits */
			options.c_cflag |= findByteSize (_dataBits);    /* Select 8 data bits */

			/* Set parity */
			switch (_parity) {
			case Parity.None:
				options.c_cflag &= ~Macros.PARENB; /* Mask the parity bits */								
				break;
			case Parity.Odd:
				options.c_cflag |= Macros.PARENB; /* Mask the parity bits */								
				options.c_cflag |= Macros.PARODD;
				break;
			case Parity.Even:
				options.c_cflag |= Macros.PARENB; /* Mask the parity bits */								
				options.c_cflag &= ~Macros.PARODD;
				break;

			case Parity.Mark:
				throw new NotImplementedException ();
					
			case Parity.Space:
				throw new NotImplementedException ();
					
			}

			/* Set stop bits */
			if (_stopBits == StopBits.One)
				options.c_cflag &= ~Macros.CSTOPB;
			else if (_stopBits == StopBits.Two)
				options.c_cflag |= Macros.CSTOPB;
			else
				throw new ArgumentOutOfRangeException ("Only one and two stop bits are supported");

			/* Dtr Control */
			int status = 0;
			if (_dtrControl == DtrControl.Enable) {
				Syscalls.ioctl (_FileDescriptor, Macros.TIOCMGET, ref status);
				status &= ~Macros.TIOCM_DTR;
				Syscalls.ioctl (_FileDescriptor, Macros.TIOCMSET, ref status);
			}

			// Hardware flow control 
			/*

			try {
				options.c_cflag |= Macros.CNEW_RTSCTS;
			} catch (ArgumentNullException) {
				try {					
					options.c_cflag |= Macros.CRTSCTS;
				} catch (ArgumentNullException) {
					throw new HardwareFlowControlNotSupportedException ();
				}			
			}
			*/
			/* Enables software flow control */
			/*
			options.c_iflag |= (Macros.IXON | Macros.IXOFF | Macros.IXANY);
			*/

			/* Choose raw input (as opposed to canonical) */
			options.c_lflag &= ~(Macros.ICANON | Macros.ECHO | Macros.ECHOE | Macros.ISIG);

			/* Choose raw output (as opposed to canonical) */						
			options.c_oflag &= ~Macros.OPOST;


			/* Enable parity */
			options.c_iflag |= (Macros.INPCK | Macros.ISTRIP);

			/* Set read time out and behavior */
			options.c_cc [Macros.VMIN] = 0;
			options.c_cc [Macros.VTIME] = 2; // x100 msec

			/* Write out the options */
			Syscalls.tcsetattr (_FileDescriptor, Macros.TCSAFLUSH, ref options);				

			/* Read time*/
			Syscalls.fcntl (_FileDescriptor, Macros.F_SETFL, 0);

			termios check = getOptions ();
			if (check.c_iflag != options.c_iflag)
				throw new InvalidOperationException ();
		}
        

		private static int findByteSize (byte dataBits)
		{
			switch (dataBits) {
			case 5:
				return Macros.CS5;
			case 6:
				return Macros.CS6;
			case 7:
				return Macros.CS7;
			case 8:
				return Macros.CS8;			
			default:
				throw new ArgumentOutOfRangeException ("Data bits should be either 5, 6, 7 or 8");
			}
		}

		public void Read (byte[] buffer, int offset, int count)
		{		
			IntPtr arrayPtr = Marshal.AllocHGlobal (count);
			long n = Syscall.read (_FileDescriptor, arrayPtr, (ulong)count);
			Marshal.Copy (arrayPtr, buffer, offset, count);
			Marshal.FreeHGlobal (arrayPtr);

			if (n < 0)
				throw new IOException ("Cannot read from the port.");			
		}

		int _FileDescriptor;

		public void Write (byte[] buffer, int offset, int count)
		{						
			IntPtr arrayPtr = Marshal.AllocHGlobal (count);
			Marshal.Copy (buffer, offset, arrayPtr, count);
			long n = Syscall.write (_FileDescriptor, arrayPtr, (ulong)count);
			Marshal.FreeHGlobal (arrayPtr);

			if (n < 0)
				throw new IOException ("Cannot write to the port.");

		}

        public Dictionary<string, string> GetPortNames()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Flushes input and output buffers
        /// </summary>
        public void Flush(FlushMode mode)
        {
            int queueSelector;
            if (mode == FlushMode.Input)
                queueSelector = Macros.TCIFLUSH;
            else if (mode == FlushMode.Output)
                queueSelector = Macros.TCOFLUSH;
            else
                queueSelector = Macros.TCIOFLUSH;

            Syscalls.tcflush(_FileDescriptor, queueSelector);
        }
    }
}
