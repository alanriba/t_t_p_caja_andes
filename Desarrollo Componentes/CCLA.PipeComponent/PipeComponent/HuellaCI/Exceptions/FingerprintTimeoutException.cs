using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPAuto.HuellaCI.Exceptions
{
    class FingerprintTimeoutException : Exception
    {
        public FingerprintTimeoutException()
        {
        }

        public FingerprintTimeoutException(string message)
            : base(message)
        {
        }

        public FingerprintTimeoutException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
