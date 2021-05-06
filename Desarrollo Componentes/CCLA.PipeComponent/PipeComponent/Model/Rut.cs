using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PipeComponent.Model
{
    class Rut
    {
        public int Mantisa { get; set; }
        public char DigitoVerificador { get; set; }

        public Rut()
        { }

        public Rut(int mantisa, char digitoVerificador)
        {
            Mantisa = mantisa;
            DigitoVerificador = digitoVerificador;
        }

        public Rut(string mantisa, string digitoVerificador)
        {
            Mantisa = int.Parse(mantisa);
            DigitoVerificador = char.Parse(digitoVerificador);
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", Mantisa, DigitoVerificador);
        }
    }
}
