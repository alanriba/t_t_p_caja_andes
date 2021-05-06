using PipeComponent.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;

namespace PipeComponent.Model
{
    //Creacion Rodrigo
    class CryptoHelper
    {
        private static string _key;

        private static string Key
        {
            get
            {
                if (String.IsNullOrEmpty(_key))
                {

                    //MachineKeySection keySection = (MachineKeySection)WebConfigurationManager.GetSection("system.web/machineKey");

                    //if (String.IsNullOrEmpty(keySection.DecryptionKey))


                    //    Console.Write("No es posible iniciar la criptogrfía ya no se pudo obtener la clave de encriptación. Verifique la sección <machineKey/> del archivo de configuración.");
                    ////throw new TechnicalException("No es posible iniciar la criptogrfía ya no se pudo obtener la clave de encriptación. Verifique la sección <machineKey/> del archivo de configuración.");

                    //if (keySection.DecryptionKey.Length != 64 && keySection.DecryptionKey.Length != 48 && keySection.DecryptionKey.Length != 32)
                    //    Console.Write("No es posible iniciar la criptogrfía ya que el largo de clave no coinicide con los soportados (128bits / 192bits/ 256bits == 32chars / 48chars / 64 chars). Verifique la sección <machineKey/> del archivo de configuración. El largo de la clave obtenida es: {0}. La clave obtenida desde el archivo de configuración es: {1}", keySection.DecryptionKey.Length, keySection.DecryptionKey);
                    ////throw new TechnicalException("No es posible iniciar la criptogrfía ya que el largo de clave no coinicide con los soportados (128bits / 192bits/ 256bits == 32chars / 48chars / 64 chars). Verifique la sección <machineKey/> del archivo de configuración. El largo de la clave obtenida es: {0}. La clave obtenida desde el archivo de configuración es: {1}", keySection.DecryptionKey.Length, keySection.DecryptionKey);

                    //_key = keySection.DecryptionKey;

                }

                return _key;

            }

        }



        public static string EncryptHexa(string text)
        {
            return Encrypt(text, true);
        }


        public static string DecryptHexa(string text)
        {
            return Decrypt(text, true);
        }

        public static string Encrypt(string text)
        {
            return Encrypt(text, false);
        }

        public static string Decrypt(string text)
        {
            return Decrypt(text, false);
        }

        private static string Encrypt(string text, bool convertToHexa)
        {
            using (RijndaelManaged Cipher = GetCipher())
            {
                ICryptoTransform trans = Cipher.CreateEncryptor();

                Encoding enc = new UTF8Encoding(false, true);

                byte[] plainText = enc.GetBytes(text);

                byte[] cipherText = trans.TransformFinalBlock(plainText, 0, plainText.Length);

                string result = convertToHexa ? ToHexa(Cipher.IV) + ToHexa(cipherText)

                    : Convert.ToBase64String(Cipher.IV) + Convert.ToBase64String(cipherText);

                return result;
            }
        }
        public static byte[] Encrypt(byte[] text)
        {
            using (RijndaelManaged Cipher = GetCipher())
            {
                ICryptoTransform trans = Cipher.CreateEncryptor();

                byte[] cypher = trans.TransformFinalBlock(text, 0, text.Length);

                byte[] result = new byte[Cipher.IV.Length + cypher.Length];

                Buffer.BlockCopy(Cipher.IV, 0, result, 0, Cipher.IV.Length);

                Buffer.BlockCopy(cypher, 0, result, Cipher.IV.Length, cypher.Length);

                return result;
            }

        }

        public static byte[] Decrypt(byte[] cipher)
        {
            using (RijndaelManaged Cipher = GetCipher())
            {

                Cipher.IV = cipher.Take(32).ToArray();

                byte[] cryptoBuffer = new byte[cipher.Length - 32];

                System.Buffer.BlockCopy(cipher, 32, cryptoBuffer, 0, cipher.Length - 32);

                ICryptoTransform trans = Cipher.CreateDecryptor();

                return trans.TransformFinalBlock(cryptoBuffer, 0, cryptoBuffer.Length);

            }

        }

        private static string Decrypt(string encryptedTicket, bool convertFromHexa)
        {

            using (RijndaelManaged Cipher = GetCipher())
            {

                int IVoffset;

                byte[] cipherText;

                if (convertFromHexa)

                {

                    IVoffset = Cipher.IV.Length * 2;

                    Cipher.IV = FromHexa(encryptedTicket.Substring(0, IVoffset));

                    cipherText = FromHexa(encryptedTicket.Substring(IVoffset, encryptedTicket.Length - IVoffset));

                }

                else

                {

                    IVoffset = (int)(Cipher.IV.Length * 1.375M);

                    Cipher.IV = Convert.FromBase64String(encryptedTicket.Substring(0, IVoffset));

                    cipherText = Convert.FromBase64String(encryptedTicket.Substring(IVoffset, encryptedTicket.Length - IVoffset));

                }



                ICryptoTransform trans = Cipher.CreateDecryptor();



                byte[] plainText = trans.TransformFinalBlock(cipherText, 0, cipherText.Length);



                Encoding enc = new UTF8Encoding(false, true);



                return enc.GetString(plainText);

            }

        }



        private static RijndaelManaged GetCipher()

        {

            RijndaelManaged cipher = new RijndaelManaged();

            cipher.KeySize = CryptoHelper.Key.Length * 4;

            cipher.BlockSize = CryptoHelper.Key.Length * 4;

            cipher.Mode = CipherMode.CBC;

            cipher.Padding = PaddingMode.PKCS7;

            cipher.Key = FromHexa(CryptoHelper.Key);

            cipher.GenerateIV();

            return cipher;

        }



        private static string ConstructFormString(NameValueCollection parameters)

        {

            List<String> items = new List<String>();



            foreach (String name in parameters)

                items.Add(String.Concat(name, "=", System.Web.HttpUtility.UrlEncode(parameters[name])));



            return String.Join("&", items.ToArray());

        }



        private static NameValueCollection ParseFormString(string formString)
        {

            NameValueCollection parameters = new NameValueCollection();



            string[] formSegments = formString.Split('&');



            foreach (string segment in formSegments)

            {

                string[] parts = segment.Split('=');

                if (parts.Length > 0)

                {

                    string key = parts[0].Trim();

                    string val = parts[1].Trim();



                    parameters.Add(key, val);

                }

            }



            return parameters;

        }



        private static string ToHexa(byte[] Bytes)

        {

            StringBuilder Result = new StringBuilder();

            string HexAlphabet = "0123456789ABCDEF";



            foreach (byte B in Bytes)

            {

                Result.Append(HexAlphabet[(int)(B >> 4)]);

                Result.Append(HexAlphabet[(int)(B & 0xF)]);

            }



            return Result.ToString();

        }



        private static byte[] FromHexa(string Hex)

        {

            byte[] Bytes = new byte[Hex.Length / 2];

            int[] HexValue = new int[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,

                                 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x0B, 0x0C, 0x0D,

                                 0x0E, 0x0F };



            for (int x = 0, i = 0; i < Hex.Length; i += 2, x += 1)

            {

                Bytes[x] = (byte)(HexValue[Char.ToUpper(Hex[i + 0]) - '0'] << 4 |

                                  HexValue[Char.ToUpper(Hex[i + 1]) - '0']);

            }



            return Bytes;

        }
    }
}
