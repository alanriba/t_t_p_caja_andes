using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPAuto.HuellaCI.Exceptions
{
    class DeviceNotConnectedException : Exception
    {
        public DeviceNotConnectedException()
        { 
        }

        public DeviceNotConnectedException(string message)
            : base(message)
        { 
        }

        public DeviceNotConnectedException(string message, Exception inner)
            : base(message, inner)
        { 
        }
    }
}
