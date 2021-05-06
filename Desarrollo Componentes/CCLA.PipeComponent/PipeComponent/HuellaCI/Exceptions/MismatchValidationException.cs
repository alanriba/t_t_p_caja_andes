using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPAuto.HuellaCI.Exceptions
{
    class MismatchValidationException : Exception
    {
        public MismatchValidationException()
        { 
        }

        public MismatchValidationException(string message)
            : base(message)
        { 
        }

        public MismatchValidationException(string message, Exception inner)
            : base(message, inner)
        { 
        }
    }
}
