using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPAuto.HuellaCI.Exceptions
{
    class CardTimeoutException : Exception
    {
        public CardTimeoutException()
        {
        }

        public CardTimeoutException(string message)
            : base(message)
        {
        }

        public CardTimeoutException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
