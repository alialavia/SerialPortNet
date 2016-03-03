using System;
using System.Collections.Generic;
using System.Text;

namespace TestSerialPort
{
    class AbstractTest
    {
        private int _t = 0;
        public virtual int t { get { return _t; } set { _t = value; } }
    }
    class Test1 : AbstractTest
    {
        private int _t = 10;
        public override int t { get { return _t; } set { _t = value; } }
    }

    class Test2 : AbstractTest
    {
        //private int b = 0;
        //public override int t { get { return b; } set { b = value; } }
    }

    abstract class test
    {
        public abstract void a();
    }

}
