using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPAuto.HuellaCI.Exceptions
{
    class InvalidLicenseException : Exception
    {
        public InvalidLicenseException()
            : base("El componente ID3 no se encuentra licenciado.")
        { 
        }

        public InvalidLicenseException(string message)
            : base(message)
        { 
        }

        public InvalidLicenseException(string message, Exception inner)
            : base(message, inner)
        { 
        }
    }
}
