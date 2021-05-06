using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;
using System.Threading;
using PipeComponent.Properties;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Net.Security;
using PipeComponent.Model;
using PipeComponent.Peripherals;
using PipeComponent.Model.Messages;
using TPAuto.HuellaCI;
using PipeComponent.Model.PdfHelper;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Diagnostics;
using PipeComponent.Utils;

namespace PipeComponent
{
    class PipeServer
    {
        public string PipeName { get; set; }

        private ThermalPrinter thermalPrinter;
        private ValidadorHuella validadorHuella;

        static AutoResetEvent autoEvent = new AutoResetEvent(false);

        public PipeServer(string pipeName)
        {
            PipeName = pipeName;
            var t = new Thread(SingleThreadApartment);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            try
            {
                thermalPrinter = new ThermalPrinter();
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogType.Error, ex.ToString());
                MyConsole.WriteLine("*** ERROR: {0}", ex.Message);
            }
        }

        private void SingleThreadApartment()
        {
            validadorHuella = new ValidadorHuella();
            autoEvent.WaitOne();
        }

        public void StartListeningPipes()
        {
            PipeSecurity ps = new PipeSecurity();
            PipeAccessRule psRule = new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), PipeAccessRights.FullControl, AccessControlType.Allow);
            //ps.AddAccessRule(psRule);
            ps.SetAccessRule(psRule);

            NamedPipeServerStream pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 254, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 1, 1, ps);
            pipeServer.BeginWaitForConnection(WaitForConnectionCallBack, pipeServer);

            //pipeServer.SetAccessControl(ps);
        }

        private void WaitForConnectionCallBack(IAsyncResult iar)
        {
            try
            {
                using (NamedPipeServerStream pipeServer = (NamedPipeServerStream)iar.AsyncState)
                {
                    pipeServer.EndWaitForConnection(iar);
                    StartListeningPipes();

                    string dataReceived;
                    byte[] bufferIn = new byte[1024 * 1000]; // 1 megabyte
                    byte[] bufferOut = new byte[1024 * 1000];  // 1 megabyte

                    pipeServer.Read(bufferIn, 0, bufferIn.Length);
                    bufferIn = bufferIn.Where(x => x != 0x00).ToArray();
                    dataReceived = Encoding.GetEncoding("Windows-1252").GetString(bufferIn, 0, bufferIn.Length);
                    Log.WriteLine(LogType.Request, dataReceived);
                    MyConsole.WriteLine("Data recibida: {0}", dataReceived);

                    if (!string.IsNullOrEmpty(dataReceived))
                    {
                        try
                        {
                            MessageService message = MessageService.Parse(dataReceived);
                            switch (message.Command)
                            {
                                case MessageCommand.ValidateFingerprint:
                                    if (message.Data != null && message.Data.Count >= 2)
                                    {
                                        try
                                        {
                                            AutentiaControl autentia = new AutentiaControl();
                                            Rut rut = new Rut(message.Data[0], message.Data[1]);
                                            var result = autentia.ValidateUser(rut);
                                            if (result.Successful)
                                            {
                                                bufferOut = MessageConverter.ToByteArray("OK", "Usuario valido");
                                            }
                                            else
                                            {
                                                bufferOut = MessageConverter.ToByteArray("NOK", result.Information);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.WriteLine(LogType.Error, ex.ToString());
                                            MyConsole.WriteLine("*** ERROR: {0}", ex.Message);
                                            bufferOut = MessageConverter.ToByteArray("NOK", ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        bufferOut = MessageConverter.ToByteArray("NOK", "Parametros insuficientes");
                                    }
                                    break;
                                case MessageCommand.PrintOnLaserPrinter:
                                    if (message.Data != null && message.Data.Count >= 2)
                                    {
                                        LaserPrinter laserPrinter = new LaserPrinter();
                                        var boletasNoImpresas = laserPrinter.Imprimir(message.Data[1]);
                                        //bufferOut = MessageConverter.ToByteArray("OK", "Archivo impreso");
                                        bufferOut = MessageConverter.ToByteArray("OK", boletasNoImpresas);
                                    }
                                    else
                                    {
                                        bufferOut = MessageConverter.ToByteArray("NOK", "Parametros insuficientes");
                                    }
                                    break;
                                case MessageCommand.PrintVoucher:
                                    if (message.Data != null && message.Data.Count >= 2)
                                    {
                                        try
                                        {
                                            /**********************************************************************
                                             * Parametros de entrada en el siguiente orden                        *
                                             * NCliente, NBoleta, FechaVencimiento, Monto, NDocumento, nomCliente;*
                                             **********************************************************************/
                                            //ThermalPrinter thermalPrinter = new ThermalPrinter();
                                            //thermalPrinter.ImprimirDeuda("15200", "123456", "12/12/2017", "$7.000", "1234567", "Cristian Azocar");
                                            thermalPrinter.ImprimirDeuda(message.Data[1]);
                                            //thermalPrinter.PrintVoucher(message.Data[0], message.Data[1]);
                                            bufferOut = MessageConverter.ToByteArray("OK", "Voucher impreso");
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.WriteLine(LogType.Error, ex.ToString());
                                            MyConsole.WriteLine("*** ERROR: {0}", ex.Message);
                                            bufferOut = MessageConverter.ToByteArray("NOK", ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        bufferOut = MessageConverter.ToByteArray("NOK", "Parametros insuficientes");
                                    }
                                    break;
                                case MessageCommand.StatusPrinter3:
                                    if (message.Data != null && message.Data.Count >= 1)
                                    {
                                        try
                                        {
                                            String respuesta = thermalPrinter.StatusPrinter(message.Data[0]);
                                            bufferOut = MessageConverter.ToByteArray(respuesta);
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.WriteLine(LogType.Error, ex.ToString());
                                            MyConsole.WriteLine("*** ERROR: {0}", ex.Message);
                                            bufferOut = MessageConverter.ToByteArray("NOK", ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        bufferOut = MessageConverter.ToByteArray("NOK", "Parametros insuficientes");
                                    }
                                    break;
                                case MessageCommand.BanmedicaToken:
                                    if (message.Data != null && message.Data.Count >= 1)
                                    {
                                        BanmedicaToken token = new BanmedicaToken();
                                        string parametros = message.Data[0];
                                        string respuesta = token.Encriptar(parametros);

                                        //laserPrinter.PrintFile(message.Data[1], message.Data[0]);
                                        bufferOut = MessageConverter.ToByteArray("OK", respuesta);
                                    }
                                    else
                                    {
                                        bufferOut = MessageConverter.ToByteArray("NOK", "Parametros insuficientes");
                                    }
                                    break;
                                case MessageCommand.ValidaHuella_ObtenerRut:
                                    //if (message.Data != null && message.Data.Count >= 1)
                                    if (true)
                                    {
                                        //var rut = validadorHuella.ObtenerRut(11, 10);
                                        Log.WriteLine(LogType.Request, "ObtenerRut -> PuertoCOM:{0}, Timeout:{1}", Settings.Default.PuertoComLectorBarra, 10);
                                        var rut = validadorHuella.ObtenerRut(Settings.Default.PuertoComLectorBarra, 10);
                                        Log.WriteLine(LogType.Response, "ObtenerRut <- Rut:{0}", rut);
                                        bufferOut = MessageConverter.ToByteArray("OK", rut);
                                    }
                                    else
                                    {
                                        bufferOut = MessageConverter.ToByteArray("NOK", "Parametros insuficientes");
                                    }
                                    break;
                                case MessageCommand.ValidaHuella_ObtenerDedo:
                                    if (true)
                                    {
                                        //var dedo = validadorHuella.ObtenerDedo("HID Global OMNIKEY 5022 Smart Card Reader 0", 30);
                                        //var dedo = validadorHuella.ObtenerDedo("OMNIKEY CardMan 5x21-CL 0", 10);
                                        Log.WriteLine(LogType.Request, "ObtenerDedo -> NombreLectorHid:{0}, Timeout:{1}", Settings.Default.NombreLectorHid, 10);
                                        var dedo = validadorHuella.ObtenerDedo(Settings.Default.NombreLectorHid, 10);
                                        Log.WriteLine(LogType.Response, "ObtenerDedo <- Dedo:{0}", dedo);
                                        bufferOut = MessageConverter.ToByteArray("OK", dedo);
                                    }
                                    else
                                    {
                                        bufferOut = MessageConverter.ToByteArray("NOK", "Parametros insuficientes");
                                    }
                                    break;
                                case MessageCommand.ValidaHuella_Valida:
                                    if (true)
                                    {
                                        //var valido = validadorHuella.ObtenerDedo("HID Global OMNIKEY 5022 Smart Card Reader 0", 30);
                                        //var valido = validadorHuella.ValidarHuella("OMNIKEY CardMan 5x21-CL 0", 10);
                                        Log.WriteLine(LogType.Request, "ValidarHuella -> NombreLectorHid:{0}, Timeout:{1}", Settings.Default.NombreLectorHid, 10);
                                        var valido = validadorHuella.ValidarHuella(Settings.Default.NombreLectorHid, 10);
                                        Log.WriteLine(LogType.Response, "ValidarHuella <- Validor:{0}", valido);
                                        bufferOut = MessageConverter.ToByteArray("OK", valido);
                                    }
                                    else
                                    {
                                        bufferOut = MessageConverter.ToByteArray("NOK", "Parametros insuficientes");
                                    }
                                    break;
                                case MessageCommand.preViewDocumentPdf:
                                    if (message.Data != null && message.Data.Count >= 1)
                                    {
                                        PdfGenerator pdfGenerator = new PdfGenerator();
                                        LaserPrinter laserPrinter = new LaserPrinter();
                                        string tipoDocumento = message.Data[0];
                                        string filename;
                                        string destinoArchivo;
                                        switch (tipoDocumento)
                                        {
                                            case "1":
                                                // certificado de afilicación educación sin carga
                                                filename = pdfGenerator.CreateDocument(DocumentType.CertificadoEducacion, message.Data[1], "certificadoEduacionSinCarga.html", "certificadoEducacionSinCarga_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                destinoArchivo = pdfGenerator.copyAndCreatePreview(filename);
                                                destinoArchivo = destinoArchivo.Replace(@"C:\inetpub\wwwroot\", @"http:\\localhost\");
                                                break;
                                            case "2":
                                                // certificado de afilicación educación con carga
                                                filename = pdfGenerator.CreateDocument(DocumentType.CertificadoEducacionCarga, message.Data[1], "certificadoEduacionConCarga.html", "certificadoEducacionConCarga_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                destinoArchivo = pdfGenerator.copyAndCreatePreview(filename);
                                                destinoArchivo = destinoArchivo.Replace(@"C:\inetpub\wwwroot\", @"http:\\localhost\");
                                                break;
                                            case "3":
                                                // certificado de salud sin carga
                                                filename = pdfGenerator.CreateDocument(DocumentType.CertificadoSalud, message.Data[1], "certificadoSaludSinCarga.html", "certificadoSaludSinCarga_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                destinoArchivo = pdfGenerator.copyAndCreatePreview(filename);
                                                destinoArchivo = destinoArchivo.Replace(@"C:\inetpub\wwwroot\", @"http:\\localhost\");
                                                break;
                                            case "4":
                                                // certificado de salud sin carga
                                                filename = pdfGenerator.CreateDocument(DocumentType.CertificadoSaludCarga, message.Data[1], "certificadoSaludConCarga.html", "certificadoSaludConCarga_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                destinoArchivo = pdfGenerator.copyAndCreatePreview(filename);
                                                destinoArchivo = destinoArchivo.Replace(@"C:\inetpub\wwwroot\", @"http:\\localhost\");
                                                break;
                                            case "5":
                                                // certificado de salud sin carga
                                                filename = pdfGenerator.CreateDocument(DocumentType.CertificadoFines, message.Data[1], "certificadoFinesSinCarga.html", "certificadoFinesSinCarga_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                destinoArchivo = pdfGenerator.copyAndCreatePreview(filename);
                                                destinoArchivo = destinoArchivo.Replace(@"C:\inetpub\wwwroot\", @"http:\\localhost\");
                                                break;
                                            case "6":
                                                // certificado de salud sin carga
                                                filename = pdfGenerator.CreateDocument(DocumentType.CertificadoFinesCarga, message.Data[1], "certificadoFinesConCarga.html", "certificadoFinesConCarga_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                destinoArchivo = pdfGenerator.copyAndCreatePreview(filename);
                                                destinoArchivo = destinoArchivo.Replace(@"C:\inetpub\wwwroot\", @"http:\\localhost\");
                                                break;
                                            case "7":
                                                // certficado cargas vigentes
                                                filename = pdfGenerator.CreateDocumentHorizontal(DocumentType.CertificadoCargaVigente, message.Data[1], "certificadoCargaVigente.html", "certificadoCargaVigente_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                destinoArchivo = pdfGenerator.copyAndCreatePreview(filename);
                                                destinoArchivo = destinoArchivo.Replace(@"C:\inetpub\wwwroot\", @"http:\\localhost\");
                                                break;
                                            case "8":
                                                // certificado cargas pendientes
                                                filename = pdfGenerator.CreateDocumentHorizontal(DocumentType.CertificadoCargaPendiente, message.Data[1], "certificadoCargaPendiente.html", "certificadoCargaPendiente_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                destinoArchivo = pdfGenerator.copyAndCreatePreview(filename);
                                                destinoArchivo = destinoArchivo.Replace(@"C:\inetpub\wwwroot\", @"http:\\localhost\");
                                                break;
                                            case "9":
                                                // certificado cargas pendientes sin cargas solo observación
                                                filename = pdfGenerator.CreateDocumentHorizontal(DocumentType.CertificadoCargaPendiente2, message.Data[1], "certificadoCargaPendiente2.html", "certificadoCargaPendiente2_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                destinoArchivo = pdfGenerator.copyAndCreatePreview(filename);
                                                destinoArchivo = destinoArchivo.Replace(@"C:\inetpub\wwwroot\", @"http:\\localhost\");
                                                break;
                                            case "10":
                                                // certificado cargas por estados
                                                filename = pdfGenerator.CreateDocumentHorizontal(DocumentType.CertificadoCargaEstado, message.Data[1], "certificadoCargaEstado.html", "certificadoCargaEstado_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                destinoArchivo = pdfGenerator.copyAndCreatePreview(filename);
                                                destinoArchivo = destinoArchivo.Replace(@"C:\inetpub\wwwroot\", @"http:\\localhost\");
                                                break;
                                            case "11":
                                                // detalle licencias
                                                filename = pdfGenerator.CreateDocument(DocumentType.LicenciaDetalle, message.Data[1], "licenciaDetalle.html", "licenciaDetalle_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                destinoArchivo = pdfGenerator.copyAndCreatePreview(filename);
                                                destinoArchivo = destinoArchivo.Replace(@"C:\inetpub\wwwroot\", @"http:\\localhost\");
                                                break;
                                            case "12":
                                                // informe de pago licencia
                                                filename = pdfGenerator.CreateDocument(DocumentType.LiquidacionPago, message.Data[1], "liquidacionPago.html", "liquidacionPago_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                destinoArchivo = pdfGenerator.copyAndCreatePreview(filename);
                                                destinoArchivo = destinoArchivo.Replace(@"C:\inetpub\wwwroot\", @"http:\\localhost\");
                                                break;

                                            default:
                                                throw new Exception("No se pudo obtener la previsualización --" + tipoDocumento);
                                        }

                                        bufferOut = MessageConverter.ToByteArray("OK", destinoArchivo);
                                    }
                                    else
                                    {
                                        bufferOut = MessageConverter.ToByteArray("NOK", "Parametros insuficientes");
                                    }
                                    break;

                                case MessageCommand.sendEmail:
                                    if (message.Data != null && message.Data.Count >= 1)
                                    {
                                        Email emailSender = new Email();
                                        var doc = message.Data[1];

                                        string[] rows = doc.Split(';');

                                        try
                                        {
                                            var _documento = rows[0].Replace(@"http:\\localhost\", @"C:\inetpub\wwwroot\");
                                            var _destinatario = rows[1];


                                            emailSender.EnviarComprobante(_destinatario, _documento);
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.WriteLine(LogType.Error, ex.ToString());
                                        }
                                        bufferOut = MessageConverter.ToByteArray("OK", "Correo Enviado");
                                    }
                                    else
                                    {
                                        bufferOut = MessageConverter.ToByteArray("NOK", "Parametros insuficientes");
                                    }
                                    break;
                                case MessageCommand.printPdf:
                                    if (message.Data != null && message.Data.Count >= 1)
                                    {
                                        LaserPrinter laserPrinter = new LaserPrinter();
                                        string tipoDocumento = message.Data[1];
                                        string destinoArchivo;

                                        // certificado de afilicación educación sin carga
                                        // filename = pdfGenerator.CreateDocument(DocumentType.CertificadoEducacion, message.Data[1], "certificadoEduacionSinCarga.html", "certificadoEducacionSinCarga_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                        // destinoArchivo = pdfGenerator.copyAndCreatePreview(filename);
                                        destinoArchivo = tipoDocumento.Replace("http:\\\\localhost\\".ToString(), @"C:\inetpub\wwwroot\");
                                        laserPrinter.PrintFile(destinoArchivo);

                                        bufferOut = MessageConverter.ToByteArray("OK", destinoArchivo);
                                    }
                                    else
                                    {
                                        bufferOut = MessageConverter.ToByteArray("NOK", "Parametros insuficientes");
                                    }
                                    break;

                                case MessageCommand.CreateAndPrintPDF:
                                    if (message.Data != null && message.Data.Count >= 1)
                                    {
                                        PdfGenerator pdfGenerator = new PdfGenerator();
                                        LaserPrinter laserPrinter = new LaserPrinter();
                                        string tipoDocumento = message.Data[0];
                                        string filename;

                                        switch (tipoDocumento)
                                        {
                                            case "1":
                                                // certificado de afilicación educación sin carga
                                                filename = pdfGenerator.CreateDocument(DocumentType.CertificadoEducacion, message.Data[1], "certificadoEduacionSinCarga.html", "certificadoEducacionSinCarga_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                laserPrinter.PrintFile(filename);

                                                break;
                                            case "2":
                                                // certificado de afilicación educación con carga
                                                filename = pdfGenerator.CreateDocument(DocumentType.CertificadoEducacionCarga, message.Data[1], "certificadoEduacionConCarga.html", "certificadoEducacionConCarga_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                laserPrinter.PrintFile(filename);

                                                break;

                                            case "3":
                                                // certificado de salud sin carga
                                                filename = pdfGenerator.CreateDocument(DocumentType.CertificadoSalud, message.Data[1], "certificadoSaludSinCarga.html", "certificadoSaludSinCarga_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                laserPrinter.PrintFile(filename);
                                                break;

                                            case "4":
                                                // certificado de salud sin carga
                                                filename = pdfGenerator.CreateDocument(DocumentType.CertificadoSaludCarga, message.Data[1], "certificadoSaludConCarga.html", "certificadoSaludConCarga_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                laserPrinter.PrintFile(filename);
                                                break;

                                            case "5":
                                                // certificado de salud sin carga
                                                filename = pdfGenerator.CreateDocument(DocumentType.CertificadoFines, message.Data[1], "certificadoFinesSinCarga.html", "certificadoFinesSinCarga_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                laserPrinter.PrintFile(filename);
                                                break;

                                            case "6":
                                                // certificado de salud sin carga
                                                filename = pdfGenerator.CreateDocument(DocumentType.CertificadoFinesCarga, message.Data[1], "certificadoFinesConCarga.html", "certificadoFinesConCarga_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                laserPrinter.PrintFile(filename);
                                                break;


                                            case "23":
                                                string[] rowsFact = message.Data[1].Split(';');


                                                if (rowsFact.Length > 40)
                                                {
                                                    //Obtenemos las primeras 40 filas
                                                    var firstFortyRows = rowsFact.Take(40).ToArray();
                                                    // Imprime las primeras 40 filas
                                                    var newData = string.Join(";", firstFortyRows);
                                                    filename = pdfGenerator.CreateDocument(DocumentType.TransitosFacturados, newData, "TransitosFacturados.html", "TransitosFacturados_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                    laserPrinter.PrintFile(filename);
                                                    Thread.Sleep(2000);

                                                    //Obtenemos las filas restantes
                                                    var facRestantes = rowsFact.Length - firstFortyRows.Length;
                                                    var nHojasFac = (double)facRestantes / (double)50;
                                                    var totalHojas = Math.Ceiling((decimal)nHojasFac);
                                                    var largoResr = 40;

                                                    if (totalHojas >= 1)
                                                    {
                                                        for (var a = 0; a <= totalHojas - 1; a++)
                                                        {
                                                            string[] cero = null;

                                                            var largo1 = rowsFact.Skip(largoResr).ToArray();

                                                            cero = largo1.Take(50).ToArray();
                                                            newData = string.Join(";", cero);
                                                            filename = pdfGenerator.CreateDocument(DocumentType.TransitosFacturados, newData, "TransitosFacturados_newPage.html", "TransitosFacturados_secondPage_" + a + ".pdf");
                                                            laserPrinter.PrintFile(filename);

                                                            largoResr = largoResr + 50;
                                                            Thread.Sleep(2000);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var restoHojasFac = rowsFact.Skip(40).ToArray();
                                                        newData = string.Join(";", restoHojasFac);
                                                        filename = pdfGenerator.CreateDocument(DocumentType.TransitosNoFacturados, newData, "TransitosFacturados.html", "TransitosFacturados.pdf");
                                                        laserPrinter.PrintFile(filename);
                                                    }
                                                }
                                                else
                                                {
                                                    //Imprimir cuando sean menor o igual a 40
                                                    filename = pdfGenerator.CreateDocument(DocumentType.TransitosFacturados, message.Data[1], "TransitosFacturados.html", "TransitosFacturados_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                    laserPrinter.PrintFile(filename);
                                                }
                                                break;
                                            case "33":
                                                string[] rowsNOFact = message.Data[1].Split(';');

                                                if (rowsNOFact.Length > 40)
                                                {
                                                    //Obtenemos las primeras 40 filas
                                                    var firstFiftyRows = rowsNOFact.Take(40).ToArray();
                                                    //imprime las primeras 40 filas
                                                    var newData = string.Join(";", firstFiftyRows);
                                                    filename = pdfGenerator.CreateDocument(DocumentType.TransitosNoFacturados, newData, "TransitosNoFacturados.html", "TransitosNoFacturados" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff") + ".pdf");
                                                    laserPrinter.PrintFile(filename);
                                                    Thread.Sleep(2000);

                                                    //Obtenemos las filas restantes
                                                    var NOFacRestantes = rowsNOFact.Length - firstFiftyRows.Length;
                                                    var nHojas = (double)NOFacRestantes / (double)50;
                                                    var totalHojasFAC = Math.Ceiling((decimal)nHojas);
                                                    var largo = 40;

                                                    if (totalHojasFAC >= 1)
                                                    {
                                                        for (var i = 0; i <= totalHojasFAC - 1; i++)
                                                        {
                                                            string[] uno = null;

                                                            var largo2 = rowsNOFact.Skip(largo).ToArray();

                                                            uno = largo2.Take(50).ToArray();
                                                            newData = string.Join(";", uno);
                                                            filename = pdfGenerator.CreateDocument(DocumentType.TransitosNoFacturados, newData, "TransitosNoFacturados_newPage.html", "TransitosNoFacturados_secondPage_" + i + ".pdf");
                                                            laserPrinter.PrintFile(filename);

                                                            largo = largo + 50;
                                                            Thread.Sleep(2000);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var restoHojas = rowsNOFact.Skip(40).ToArray();
                                                        newData = string.Join(";", restoHojas);
                                                        filename = pdfGenerator.CreateDocument(DocumentType.TransitosNoFacturados, newData, "TransitosNoFacturados_newPage.html", "TransitosNoFacturados_secondPage.pdf");
                                                        laserPrinter.PrintFile(filename);
                                                    }
                                                }
                                                else
                                                {
                                                    filename = pdfGenerator.CreateDocument(DocumentType.TransitosNoFacturados, message.Data[1], "TransitosNoFacturados.html", "TransitosNoFacturados.pdf");
                                                    laserPrinter.PrintFile(filename);
                                                }
                                                break;
                                            default:
                                                throw new Exception("El tipo de documento no es válido --> " + tipoDocumento);
                                        }

                                        bufferOut = MessageConverter.ToByteArray("OK", "Archivo creado");
                                    }
                                    else
                                    {
                                        bufferOut = MessageConverter.ToByteArray("NOK", "Parametros insuficientes");
                                    }
                                    break;
                                case MessageCommand.CloseKiosk:
                                    //Cerrar el Kiosko
                                    foreach (Process clsProcess in Process.GetProcesses().Where(clsProcess => clsProcess.ProcessName.StartsWith("chrome")))
                                    {
                                        clsProcess.Kill();
                                    }
                                    break;
                                default:
                                    bufferOut = MessageConverter.ToByteArray("NOK", "Comando no implementado");
                                    break;

                            }
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLine(LogType.Error, ex.ToString());
                            MyConsole.WriteLine("*** ERROR: {0}", ex.Message);
                            bufferOut = MessageConverter.ToByteArray("NOK", ex.Message);
                        }
                    }
                    else
                    {
                        Log.WriteLine(LogType.Error, "La data recibida es vacía o nula");
                        MyConsole.WriteLine("*** ERROR: La data recibida es vacía o nula");
                        bufferOut = MessageConverter.ToByteArray("NOK", "La data recibida es vacia o nula");
                    }

                    Log.WriteLine(LogType.Response, Encoding.GetEncoding("Windows-1252").GetString(bufferOut));
                    pipeServer.Write(bufferOut, 0, bufferOut.Length);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogType.Error, ex.ToString());
                MyConsole.WriteLine("*** ERROR: {0}", ex.Message);
            }
        }
    }
}
