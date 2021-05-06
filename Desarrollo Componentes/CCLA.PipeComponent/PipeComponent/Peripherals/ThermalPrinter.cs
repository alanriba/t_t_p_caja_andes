using NiiPrinterCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using System.Collections;
using System.Drawing;
using System.IO;
using PipeComponent.Properties;

namespace PipeComponent.Peripherals
{
    class ThermalPrinter
    {
        private NIIClassLib nii;
        private long jobId;
        private readonly bool printerInitialized;

        public ThermalPrinter()
        {
            nii = new NIIClassLib();
            int ret = nii.NiiStartDoc(Settings.Default.Impresora, out jobId);
            if (ret == 0)
            {
                printerInitialized = true;
            }
            else
            {
                throw new Exception("Error al inicializar la impresora: " + GetErrorDescription(ret));
            }
        }

        #region Public Methods
        public void PrintVoucher(string printerName, string voucher)
        {
            int ret;
            string aux;
            string fullTicket;            

            fullTicket = "\x1B\x32\n";      // Set pitch default   
            fullTicket += "\x1B\x74\x00";   // No japan code
            fullTicket += "\x1B\x61\x01";   // Center align

            for (int i = 0; i < voucher.Length; i += 40)
            {
                if (i + 40 > voucher.Length) // Fin ticket
                    fullTicket += voucher.Substring(i);
                else
                    fullTicket += voucher.Substring(i, 40) + "\n";
            }

            fullTicket += "\x1B\x64\x04";   // 4LF
            fullTicket += "\x1B" + "i";     // Corte Total

            //ret = nii.NiiStartDoc(printerName, out jid);
            if (printerInitialized)
            {
                aux = "";
                foreach (char c in fullTicket)
                {
                    int tmp = c;
                    aux += string.Format("{0:X2}", Convert.ToUInt32(tmp));
                }

                ret = nii.NiiPrint(printerName, aux, aux.Length, out jobId);
                ret = nii.NiiEndDoc(printerName);
            }
            else
            {
                throw new Exception("La impresora no se encuentra inicializada.");
            }
        }

        public String StatusPrinter(String printerName)
        {
            NIIClassLib nii = new NIIClassLib();
            long niiStatusResponse;
            int response;
            response = nii.NiiGetStatus(printerName, out niiStatusResponse);
            Int64 codigo = Convert.ToInt64(niiStatusResponse);
            String responseError = GetStatusDescriptionLevel2(codigo);


            return GetStatusDescriptionLevel1(response) + "-" + (responseError.Length > 0 ? responseError : "OK");
        }

        public void ImprimirDeuda(string parametros)
        {
            string NCliente, NBoleta, FechaVencimiento, Monto, NDocumento, nomCliente;
            string sQr;
            
            
            string[] rows = parametros.Split(';');
            string[] cols;
            foreach (var row in rows)
            {
                
                cols = row.Split(',');
                NCliente = cols[0].Trim();
                NBoleta = cols[1].Trim();
                FechaVencimiento = cols[2].Trim();
                Monto = cols[3].Trim();
                NDocumento = cols[4].Trim();
                nomCliente = cols[5].Trim();
                int ret;
                string[] nombre;
                int lenNom = 0;
                int cantCharNom = 0;

                lenNom = nomCliente.Length;
                
                //NIIClassLib nii = new NIIClassLib();
                string buffer = "\x1B\x32\n"; //DEFINIR LENGUAJE
                buffer += "\x1B\x74\x00";   // No japan code
                buffer = buffer + "\x1B\x61\x01";             //Center Alignment - Refer to Pg. 3-29
                buffer = buffer + Strings.Chr(0x1d) + Strings.Chr(0x50) + Strings.Chr(0x0); //Logo desde impresora
                buffer = buffer + "\n" + Strings.Chr(0x1b) + Strings.Chr(0x61) + Strings.Chr(0x2);
                buffer = buffer + Strings.Chr(0x1b) + Strings.Chr(0x21) + Strings.Chr(0x2);
                buffer = buffer + DateTime.Now.Date.ToString("dd-MM-yyyy") + "\n";
                buffer = buffer + DateTime.Now.ToShortTimeString() + "\n";
                buffer = buffer + Strings.Chr(0x1b) + Strings.Chr(0x61) + Strings.Chr(0x1);  // align centro
                buffer = buffer + Strings.Chr(0x1b) + Strings.Chr(0x21) + Strings.Chr(0x39);
                buffer = buffer + "CUPON DE PAGO\n\n";    //Character Expansion - Pg. 3-10
                buffer = buffer + Strings.Chr(0x1b) + Strings.Chr(0x21) + Strings.Chr(0x2); // font normal
                buffer = buffer + Strings.Chr(0x1b) + Strings.Chr(0x61) + Strings.Chr(0x0); // align izq
                buffer = buffer + "\x9" + "No Cliente" + "\x9" + ": " + NCliente + "\n";
                if (lenNom > 22)
                {
                    string nombreClienteMod="";
                    int primeraVezNom = 0;
                    cantCharNom = (int)(Math.Ceiling(lenNom / 22m));
                    for (int i = 0; i <= cantCharNom - 1 ; i++)
                    {
                        if (i < (cantCharNom - 1))
                        {
                            nombreClienteMod = nombreClienteMod + nomCliente.Substring((i == 0 ? 0 : (i == 0 ? 22 : 22 * i)), 22) + "~";
                        }
                        else
                        {
                            nombreClienteMod = nombreClienteMod + nomCliente.Substring(22*i+1);
                        }
                    }
                    nombre = nombreClienteMod.Split('~');
                    foreach (var nomRow in nombre)
                    {
                        if (primeraVezNom == 0)
                        {
                            buffer = buffer + "\x9" + "Cliente" + "\x9" + "\x9" + ": " + LatinCharToPrinterChar(nomRow) + "\n";
                            primeraVezNom++;
                        }
                        else
                        {
                            buffer = buffer + "\x9" + "       " + "\x9" + "\x9" + "  " + LatinCharToPrinterChar(nomRow) + "\n";
                        }
                    }
                }
                else
                {
                    buffer = buffer + "\x9" + "Cliente" + "\x9" + "\x9" + ": " + LatinCharToPrinterChar(nomCliente) + "\n";
                }
                buffer = buffer + "\x9" + "No Boleta" + "\x9" + ": " + NBoleta + "\n";
                buffer = buffer + "\x9" + "F. Vencimiento" + "\x9" + ": " + Convert.ToDateTime(FechaVencimiento).ToString("dd-MM-yyyy") + "\n";
                buffer = buffer + "\x9" + "No Docto. Pago" + "\x9" + ": " + NDocumento + "\n\n";
                buffer = buffer + Strings.Chr(0x1b) + Strings.Chr(0x21) + Strings.Chr(0x8);
                buffer = buffer + "\x9" + "TOTAL A PAGAR" + "\x9" + ": $ " + Monto + "\n\n";
                buffer = buffer + Strings.Chr(0x1b) + Strings.Chr(0x21) + Strings.Chr(0x2);
                Monto = Monto.Replace("$", "").Replace(".", "").Replace(" ", "");
                buffer = buffer + Strings.Chr(0x1b) + Strings.Chr(0x61) + Strings.Chr(0x1);        // align centro
                sQr = NCliente + "*" + Monto + "*" + NDocumento;
                buffer = buffer + Strings.Chr(0x1b) + Strings.Chr(0x71) + Strings.Chr(0x6) + Strings.Chr(0x3) + Strings.Chr(0x0) + Strings.Chr(0x0) + Strings.Chr(sQr.Length) + Strings.Chr(0x0) + sQr + Strings.Chr(0x0);
                buffer = buffer + "\n \n";
                buffer = buffer + Strings.Chr(0x1b) + Strings.Chr(0x21) + Strings.Chr(0x2);
                buffer = buffer + "SUSCRIBE PAGO AUTOMATICO, AHORRARAS TIEMPO \n Y DINERO.\n";
                buffer = buffer + "WWW.VESPUCIONORTE.CL\n\n\n\n";                      //Specify/Cencel White/Black Invert - Pg. 3-16
                buffer = buffer + "\x1b\x64\x02";                                            //Cut  - Pg. 3-41
                //ret = nii.NiiStartDoc("NII ExD NP-K205", out jid);
                buffer += "\x1B" + "i";     // Corte Total

                if (printerInitialized)
                {
                    var aux = "";
                    foreach (char c in buffer)
                    {
                        int tmp = c;
                        aux += string.Format("{0:X2}", Convert.ToUInt32(tmp));
                    }
                    ret = nii.NiiPrint(Settings.Default.Impresora, aux, aux.Length, out jobId);
                    ret = nii.NiiEndDoc(Settings.Default.Impresora);
                }
                else
                {
                    throw new Exception("La impresora no se encuentra inicializada.");
                }
            }
        }
        #endregion

        #region Mesages - Private Methods
        private String GetErrorDescription(int errorCode)
        {
            switch (errorCode)
            {
                case -1:
                    return "Data expansion error";
                case -2:
                    return "Printer open error";
                case -3:
                    return "Document starting error";
                case -4:
                    return "Page starting error";
                case -5:
                    return "File acquisition failure";
                case -6:
                    return "Invalid argument error";
                case -7:
                    return "Temporary open error";
                case -13:
                    return "Printer output error (spooler)";
                case -31:
                    return "Resource shortage";
                case 1:
                    return "Document already started";
                case 2:
                    return "Printer is opened";
                default:
                    return errorCode.ToString();
            }
        }
        private String GetStatusDescriptionLevel1(int errorCode)
        {
            switch (errorCode)
            {
                case 0:
                    return "Ok";
                case 1:
                    return "Status information open error (recovered)";
                case -3:
                    return "Printer open error";
                case -5:
                    return "OFFLINE";
                case -6:
                    return "Invalid argument error";
                case -9:
                    return "Printer information acquisition failure";
                case -11:
                    return "Status information acquisition failure";
                case -12:
                    return "Status information open error";
                case -31:
                    return "Resource shortage";
                case -102:
                    return "Socket error";
                case -110:
                    return "Host unknown";
                case -105:
                    return "Connection error";
                case -106:
                    return "Transmission error";
                case -107:
                    return "Transmission error (Timeout)";
                case -108:
                    return "Receipt error";
                case -109:
                    return "Receipt error (Timeout)";
                case -111:
                    return "Communication error";

                default:
                    return errorCode.ToString();
            }
        }
        private String GetStatusDescriptionLevel2(Int64 codigo)
        {
            Dictionary<int, String> errores = new Dictionary<int, String>();
            //Verifico que el Bit este en codigo
            if ((codigo & 0x01) > 0)
            {
                errores.Add(0, "Papel cerca del termino");
            }
            if ((codigo & 0x02) > 0)
            {
                errores.Add(1, "Cabezal Abierto");
            }
            if ((codigo & 0x04) > 0)
            {
                errores.Add(2, "Sin Papel");
            }
            if ((codigo & 0x08) > 0)
            {
                errores.Add(3, "Temperatura Anormal o Papel Atascado");
            }
            if ((codigo & 0x10) > 0)
            {
                errores.Add(4, "Problemas con guillotina");
            }
            String response = String.Join(", ", errores.Select(x => x.Value).ToArray());
            return response;
        }
        #endregion

        private string LatinCharToPrinterChar(string text)
        {
            text = text.Replace('á', Strings.Chr(0xa0));
            text = text.Replace('é', Strings.Chr(0x82));
            text = text.Replace('í', Strings.Chr(0xa1));
            text = text.Replace('ó', Strings.Chr(0xa2));
            text = text.Replace('ú', Strings.Chr(0xa3));
            text = text.Replace('ñ', Strings.Chr(0xa4));
            text = text.Replace('Á', 'A');
            text = text.Replace('É', 'E');
            text = text.Replace('Í', 'I');
            text = text.Replace('Ó', 'O');
            text = text.Replace('Ú', 'U');
            text = text.Replace('Ñ', Strings.Chr(0xa5));
            return text;
        }
        
        public string GetLogo()
        {
            string logo = "";
            if (!File.Exists(@"C:\Users\o.guerrero\Desktop\win\logoImpresoraTermica.bmp"))
                //if (!File.Exists(@"C:\Users\o.guerrero\Desktop\win\logoImpresoraTermica.png"))
                return null;
            BitmapData data = GetBitmapData(@"C:\Users\o.guerrero\Desktop\win\logoImpresoraTermica.bmp");
            //BitmapData data = GetBitmapData(@"C:\Users\o.guerrero\Desktop\win\logoImpresoraTermica.png");
            BitArray dots = data.Dots;
            byte[] width = BitConverter.GetBytes(data.Width);

            int offset = 0;
            MemoryStream stream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write((char)0x1B);
            bw.Write('@');

            bw.Write((char)0x1B);
            bw.Write('3');
            bw.Write((byte)24);

            while (offset < data.Height)
            {
                bw.Write((char)0x1B);
                bw.Write('*');         // bit-image mode
                bw.Write((byte)33);    // 24-dot double-density
                bw.Write(width[0]);  // width low byte
                bw.Write(width[1]);  // width high byte

                for (int x = 0; x < data.Width; ++x)
                {
                    for (int k = 0; k < 3; ++k)
                    {
                        byte slice = 0;
                        for (int b = 0; b < 8; ++b)
                        {
                            int y = (((offset / 8) + k) * 8) + b;
                            // Calculate the location of the pixel we want in the bit array.
                            // It'll be at (y * width) + x.
                            int i = (y * data.Width) + x;

                            // If the image is shorter than 24 dots, pad with zero.
                            bool v = false;
                            if (i < dots.Length)
                            {
                                v = dots[i];
                            }
                            slice |= (byte)((v ? 1 : 0) << (7 - b));
                        }

                        bw.Write(slice);
                    }
                }
                offset += 24;
                bw.Write((char)0x0A);
            }
            // Restore the line spacing to the default of 30 dots.
            bw.Write((char)0x1B);
            bw.Write('3');
            bw.Write((byte)30);

            bw.Flush();
            byte[] bytes = stream.ToArray();
            return logo + Encoding.Default.GetString(bytes);

        }
        
        public BitmapData GetBitmapData(string bmpFileName)
        {
            using (var bitmap = (Bitmap)Bitmap.FromFile(bmpFileName))
            {
                var threshold = 127;
                var index = 0;
                double multiplier = 370; // this depends on your printer model. for Beiyang you should use 1000
                double scale = (double)(multiplier / (double)bitmap.Width);
                int xheight = (int)(bitmap.Height * scale);
                int xwidth = (int)(bitmap.Width * scale);
                var dimensions = xwidth * xheight;
                var dots = new BitArray(dimensions);

                for (var y = 0; y < xheight; y++)
                {
                    for (var x = 0; x < xwidth; x++)
                    {
                        var _x = (int)(x / scale);
                        var _y = (int)(y / scale);
                        var color = bitmap.GetPixel(_x, _y);
                        var luminance = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                        dots[index] = (luminance < threshold);
                        index++;
                    }
                }

                return new BitmapData()
                {
                    Dots = dots,
                    Height = (int)(bitmap.Height * scale),
                    Width = (int)(bitmap.Width * scale)
                };
            }
        }

        public class BitmapData
        {
            public BitArray Dots
            {
                get;
                set;
            }

            public int Height
            {
                get;
                set;
            }

            public int Width
            {
                get;
                set;
            }
        }
    }
}
