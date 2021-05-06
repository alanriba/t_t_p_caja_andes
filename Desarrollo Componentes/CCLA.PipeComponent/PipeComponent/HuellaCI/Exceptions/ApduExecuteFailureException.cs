using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPAuto.HuellaCI.Exceptions
{
    class ApduExecuteFailureException : Exception
    {
        public ApduExecuteFailureException()
        { 
        }

        public ApduExecuteFailureException(string message)
            : base(message)
        { 
        }

        public ApduExecuteFailureException(string message, Exception inner)
            : base(message, inner)
        { 
        }
    }
}
