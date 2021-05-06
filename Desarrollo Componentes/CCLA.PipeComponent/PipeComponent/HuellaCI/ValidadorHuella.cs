using System;

using System.Text;
using TPAuto.HuellaCI.Exceptions;
using id3.Fingerprint;
using System.IO.Ports;
using System.Threading;
using System.IO;
using AxPTLOCXCi;
using SecuGen.FDxSDKPro.Windows;
using System.Drawing;
//using TPAuto.Other;

namespace TPAuto.HuellaCI
{
    enum TipoCedula
    {
        Antigua,
        Nueva,
        Desconocido
    }

    class ValidadorHuella
    {
        private SerialPort comPdf417;
        private TipoCedula tipoCedula;
        private SmartCard smartCard;
        private AxPTLOcx ucHuella;
        private ValidadorCedulaNueva validadorNuevo;
        private SGFingerPrintManager SecugenManager;
        private bool isID3Licensed;
        private bool dataReceived;
        private string buffer;
        private string rut;

        public string Buffer { get; set; }

        public ValidadorHuella()
        {
            try
            {
                FingerLicense.CheckLicense();
                isID3Licensed = true;

                comPdf417 = new SerialPort();
                comPdf417.BaudRate = 9600;
                comPdf417.Parity = Parity.None;
                comPdf417.DataBits = 8;
                comPdf417.StopBits = StopBits.One;
                comPdf417.Handshake = Handshake.None;
                comPdf417.ReadTimeout = 500;
                comPdf417.Encoding = Encoding.GetEncoding("Windows-1252");
                comPdf417.DataReceived += comPdf417_DataReceived;

                ucHuella = new AxPTLOcx();
                ucHuella.CreateControl();
                SecugenManager = new SGFingerPrintManager();
                smartCard = new SmartCard();
                validadorNuevo = new ValidadorCedulaNueva(smartCard);
                tipoCedula = TipoCedula.Desconocido;
            }
            catch (FingerException) { }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }

        public string ObtenerRut(int commPort, int timeout)
        {
            if (!isID3Licensed) throw new InvalidLicenseException();
            if (commPort < 1 || commPort > 255) throw new ArgumentOutOfRangeException("Debe ser mayor a 0 y menor a 255.", "commPort");
            if (timeout <= 0) throw new ArgumentOutOfRangeException("Debe ser mayor a 0.", "timeout");

            buffer = rut = "";
            dataReceived = false;
            tipoCedula = TipoCedula.Desconocido;

            comPdf417.Close();
            comPdf417.PortName = "COM" + commPort;
            comPdf417.Open();
            comPdf417.DiscardInBuffer();

            DateTime endTime = DateTime.Now.AddMilliseconds(timeout * 1000);
            while (!dataReceived && endTime.CompareTo(DateTime.Now) > 0)
            {
                Thread.Sleep(100);
            }

            buffer = comPdf417.ReadExisting();
            comPdf417.DiscardInBuffer();
            comPdf417.Close();

            if (!dataReceived) throw new CardTimeoutException("El tiempo para esperar la lectura de la cédula ha expirado.");
            if (!string.IsNullOrEmpty(buffer))
            {
                if (buffer.Contains("http") && buffer.Length > 65)
                {
                    int i, j;
                    if ((i = buffer.IndexOf("RUN=")) > 0)
                    {
                        i += 4;
                        if ((j = buffer.IndexOf('&', i)) > 0)
                        {
                            if ((j - i) >= 3 && (j - i) <= 10)
                            {
                                this.Buffer = buffer;
                                rut = buffer.Substring(i, j - i);
                                tipoCedula = TipoCedula.Nueva;
                                return rut;
                            }
                        }
                    }

                    throw new InvalidDataException("El buffer de entrada no tiene la estructura esperada.");
                }
                else if (buffer.Length >= 420)
                {
                    string rawRut = buffer.Substring(0, 9).Trim();
                    if (rawRut.Substring(0, rawRut.Length - 1).Length == 8)
                        rut = string.Format("{0}.{1}.{2}-{3}", rawRut.Substring(0, 2), rawRut.Substring(2, 3), rawRut.Substring(5, 3), rawRut.Substring(rawRut.Length - 1, 1));
                    else if (rawRut.Substring(0, rawRut.Length - 1).Length == 7)
                        rut = string.Format("{0}.{1}.{2}-{3}", rawRut.Substring(0, 1), rawRut.Substring(1, 3), rawRut.Substring(4, 3), rawRut.Substring(rawRut.Length - 1, 1));
                    else
                        throw new InvalidDataException("El buffer de entrada no tiene la estructura esperada.");

                    this.Buffer = buffer;
                    tipoCedula = TipoCedula.Antigua;
                    return rut;
                }
                else
                {
                    throw new InvalidDataException("El buffer de entrada no tiene la estructura esperada.");
                }
            }
            else
            {
                throw new InvalidDataException("El buffer de entrada está vacío.");
            }
        }

        public string ObtenerNombre()
        {
            if (tipoCedula == TipoCedula.Nueva)
            {
                return null;
            }
            else if (tipoCedula == TipoCedula.Antigua)
            {
                int i;
                if ((i = buffer.IndexOf('\0', 19)) > 0)
                    return buffer.Substring(19, i - 19);
                else
                    throw new InvalidDataException("El buffer de entrada no tiene la estructura esperada.");
            }
            else
            {
                throw new InvalidOperationException("La cédula es inválida o no se ha invocado el método ObtenerRut.");
            }
        }

        public string ObtenerDedo(string readerName, int timeout)
        {
            if (!isID3Licensed) throw new InvalidLicenseException();
            if (string.IsNullOrEmpty(readerName)) throw new ArgumentException("No puede ser nulo ni vacío.", "readerName");
            if (timeout <= 0) throw new ArgumentOutOfRangeException("Debe ser mayor a 0.", "timeout");

            if (tipoCedula == TipoCedula.Antigua)
            {
                return "Pulgar Derecho";
            }
            else if (tipoCedula == TipoCedula.Nueva)
            {
                bool cardDetected = false;
                string dedo = null;
                Exception lastException = null;
                DateTime endTime = DateTime.Now.AddMilliseconds(timeout * 1000);
                while (!cardDetected && endTime.CompareTo(DateTime.Now) > 0)
                {
                    try
                    {
                        smartCard.Connect(readerName, Share.Shared, Protocol.T0orT1);
                        cardDetected = true;
                        dedo = smartCard.GetDedo();
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        Thread.Sleep(200);
                    }
                }

                if (!cardDetected) throw new CardTimeoutException("El tiempo para esperar la lectura de la cédula ha expirado --> " + (lastException != null ? lastException.Message : null));
                if (string.IsNullOrEmpty(dedo)) throw new InvalidOperationException("No se pudo obtener el nombre del dedo almacenado en la cédula.");
                return dedo;
            }
            else
            {
                throw new InvalidOperationException("La cédula es inválida o no se ha invocado el método ObtenerRut.");
            }
        }

        public bool ValidarHuella(string readerName, int timeout)
        {
            if (!isID3Licensed) throw new InvalidLicenseException();
            if (string.IsNullOrEmpty(readerName)) throw new ArgumentException("No puede ser nulo ni vacío.", "readerName");
            if (timeout <= 0) throw new ArgumentOutOfRangeException("Debe ser mayor a 0.", "timeout");

            if (tipoCedula == TipoCedula.Antigua && !string.IsNullOrEmpty(rut) && !string.IsNullOrEmpty(buffer))
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rut), false, Encoding.GetEncoding("Windows-1252")))
                {
                    sw.Write(buffer);
                    sw.Flush();
                }

                ucHuella.Tiempo = (short)(timeout * 1000);
                ucHuella.Rut = rut;
                ucHuella.Path = AppDomain.CurrentDomain.BaseDirectory + "/";

                DateTime startTime = DateTime.Now;
                int resp = ucHuella.ValidarCedula();
                DateTime endTime = DateTime.Now;

                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rut));

                if (resp != 0 && (endTime - startTime).Seconds >= timeout)
                    throw new FingerprintTimeoutException("El tiempo para esperar la lectura de la huella ha expirado.");

                return resp == 0;
            }
            else if (tipoCedula == TipoCedula.Nueva)
            {
                bool cardDetected = false;
                Exception lastException = null;
                DateTime endTime = DateTime.Now.AddMilliseconds(timeout * 1000);                
                while (!cardDetected && endTime.CompareTo(DateTime.Now) > 0)
                {
                    try
                    {
                        smartCard.Connect(readerName, Share.Shared, Protocol.T0orT1);
                        cardDetected = true;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                    }

                    if (cardDetected)
                    {
                        int error, quality = 50;
                        byte[] image;
                        
                        error = SecugenManager.EnumerateDevice();
                        if (error != (int)SGFPMError.ERROR_NONE)
                            throw new InvalidOperationException("Error en método EnumerateDevice: " + Secugen.GetErrorDescription(error));

                        if (SecugenManager.NumberOfDevice > 0)
                        {
                            SGFPMDeviceList device = new SGFPMDeviceList();
                            error = SecugenManager.GetEnumDeviceInfo(0, device);
                            if (error != (int)SGFPMError.ERROR_NONE)
                                throw new InvalidOperationException("Error en método GetEnumDeviceInfo: " + Secugen.GetErrorDescription(error));

                            error = SecugenManager.Init(device.DevName);
                            if (error != (int)SGFPMError.ERROR_NONE)
                                throw new InvalidOperationException("Error en método Init: " + Secugen.GetErrorDescription(error));

                            error = SecugenManager.OpenDevice(device.DevID);
                            if (error != (int)SGFPMError.ERROR_NONE)
                                throw new InvalidOperationException("Error en método OpenDevice: " + Secugen.GetErrorDescription(error));

                            SGFPMDeviceInfoParam info = new SGFPMDeviceInfoParam();
                            error = SecugenManager.GetDeviceInfo(info);
                            if (error != (int)SGFPMError.ERROR_NONE)
                                throw new InvalidOperationException("Error en método GetDeviceInfo: " + Secugen.GetErrorDescription(error));

                            image = new byte[info.ImageWidth * info.ImageHeight];
                            error = SecugenManager.GetImageEx(image, timeout * 1000, 0, quality);
                            SecugenManager.CloseDevice();
                            if (error == (int)SGFPMError.ERROR_NONE)
                            {
                                Bitmap bmp = Helper.ByteArrayToBitmap(image, info.ImageWidth, info.ImageHeight);
                                FingerImage fingerImage = FingerImage.FromBitmap(bmp);
                                FingerExtract fingerExtract = new FingerExtract();
                                FingerTemplate fingerTemplate = fingerExtract.CreateTemplate(fingerImage);
                                byte[] huellaCompacta = fingerTemplate.ToIso19794CompactCard(byte.MaxValue);
                                return validadorNuevo.HuellaEsValida(huellaCompacta);
                            }
                            else
                            {
                                if (error == (int)SGFPMError.ERROR_TIME_OUT)
                                    throw new FingerprintTimeoutException("El tiempo para esperar la lectura de la huella ha expirado.");
                                else
                                    throw new InvalidOperationException("Error en método GetImageEx: " + Secugen.GetErrorDescription(error));
                            }
                        }
                        else
                        {
                            throw new DeviceNotConnectedException("No se detectó ningún lector de huella.");
                        }
                    }
                    else
                    {
                        Thread.Sleep(200);
                    }
                }

                if (!cardDetected) throw new CardTimeoutException("El tiempo para esperar la lectura de la cédula ha expirado --> " + (lastException != null ? lastException.Message : null));
                return false;
            }
            else
            {
                throw new InvalidOperationException("La cédula es inválida o no se ha invocado el método ObtenerRut.");
            }
        }

        private void comPdf417_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            dataReceived = true;
        }
    }
}

