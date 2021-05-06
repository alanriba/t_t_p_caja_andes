using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPAuto.HuellaCI.Exceptions
{
    class ApduResponseErrorException : Exception
    {
        public ApduResponseErrorException()
        { 
        }

        public ApduResponseErrorException(string message)
            : base(message)
        { 
        }

        public ApduResponseErrorException(string message, Exception inner)
            : base(message, inner)
        { 
        }
    }
}
