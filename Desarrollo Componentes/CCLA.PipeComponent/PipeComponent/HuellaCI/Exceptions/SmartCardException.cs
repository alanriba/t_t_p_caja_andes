using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPAuto.HuellaCI.Exceptions
{
    class SmartCardException : Exception
    {
        public SmartCardException()
        { 
        }

        public SmartCardException(string message)
            : base(message)
        { 
        }

        public SmartCardException(string message, Exception inner)
            : base(message, inner)
        { 
        }
    }
}
