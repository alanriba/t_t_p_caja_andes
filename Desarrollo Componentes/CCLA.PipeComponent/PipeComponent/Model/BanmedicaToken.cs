using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PipeComponent.Model
{
    //Creación Rodrigo
    class BanmedicaToken
    {

        public string Encriptar(string parametros)
        {
            //Encodeamos en un arreglo de bytes
            byte[] value = Encoding.UTF8.GetBytes(parametros);

            //Encriptamos el arreglo de bytes

            byte[] encriptado = CryptoHelper.Encrypt(value);

            //Pasamos el arreglo encriptado a base64
            string salida = Convert.ToBase64String(encriptado);

            //Devolvemos el resultado en String
            return salida;

        }        

    }
}
