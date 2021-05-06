using Microsoft.Win32;
using PipeComponent.Model.AdobPrinter;
using PipeComponent.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace PipeComponent.Peripherals
{
    class LaserPrinter
    {
        #region Service
        private void PrintFormPdfData(byte[] formPdfData)
        {
            string tempFile;

            tempFile = Path.GetTempFileName();

            using (FileStream fs = new FileStream(tempFile, FileMode.Create))
            {
                fs.Write(formPdfData, 0, formPdfData.Length);
                fs.Flush();
            }
            
            string pdfArguments = string.Format("/t /o \"{0}\" \"{1}\"", tempFile, Settings.Default.ImpresoraLaser);
            string adobeReaderPath = GetAdobePath();

            try
            {
                Log.WriteLine(LogType.InicioOperacion, "Impresion documento");
                ProcessStarter processStarter = new ProcessStarter("AcroRd32", adobeReaderPath, pdfArguments);
                Log.WriteLine(LogType.Error, processStarter.ProcessName);
                processStarter.Run();
                processStarter.WaitForExit(5000);
                processStarter.Stop();
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogType.Error, ex.ToString());
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
        #endregion

        #region Console
        public void PrintFile(string filename)
        {
            if (!Environment.UserInteractive)
            {
                // Modo servicio
                var fileArray = File.ReadAllBytes(filename);
                PrintFormPdfData(fileArray);

                Log.WriteLine(LogType.InicioOperacion, "Impresion documento" + fileArray);
            }
            else
            {
                // Modo consola
                ProcessStartInfo info = new ProcessStartInfo();
                info.Verb = "print";
                info.FileName = filename;
                info.CreateNoWindow = true;
                info.WindowStyle = ProcessWindowStyle.Hidden;

                using (Process p = new Process())
                {
                    //Log.WriteLine(LogType.Error, p.ProcessName);
                    p.StartInfo = info;
                    p.Start();
                    if (!p.HasExited)
                    {
                        p.WaitForExit(0);
                    }
                    //p.CloseMainWindow();
                    p.Close();
                    KillAdobe();
                }
            }
        }

        public void PrintFile(byte[] stream)
        {
            string filename = "Documento.pdf";
            File.WriteAllBytes(filename, stream);
            PrintFile(filename);
        }

        public string Imprimir(string doc)
        {
            string[] rows = doc.Split(';');
            string[] cols;
            string boletasNoImpresas = string.Empty;
            //validamos la existencia del directorio C:/
            string path = @"c:\totalPack\tempPDF";
            if (!Directory.Exists(path))
            {
                //creamos directorio
                Directory.CreateDirectory(path);
            }

            foreach (var row in rows)
            {
                cols = row.Split(',');
                try
                {
                    var _urlPPLS = cols[0];
                    var numeroDocumento = cols[1];
                    var nombreArchivo = numeroDocumento + ".pdf";

                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(_urlPPLS, nombreArchivo);
                    PrintFile(nombreArchivo);
                }
                catch (Exception ex)
                {
                    Log.WriteLine(LogType.Error, ex.ToString());
                    boletasNoImpresas += cols[1] + ",";
                    continue;
                }
            }

            if (!string.IsNullOrEmpty(boletasNoImpresas))
            {
                boletasNoImpresas = boletasNoImpresas.Substring(0, boletasNoImpresas.Length - 1); // Borra la última coma
            }

            return boletasNoImpresas;
        }
        #endregion

        public void PrintFile(string base64, string extension)
        {
            byte[] fileArray = Convert.FromBase64String(base64);

            // Si no estamos en modo consola usamos el método de impresión para servicio
            if (!Environment.UserInteractive)
            {
                PrintFormPdfData(fileArray);
            }
            else
            {
                string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Documento." + extension.Replace(".", ""));
                File.WriteAllBytes(filename, fileArray);
                PrintFile(filename);
            }
        }

        private void KillAdobe()
        {
            foreach (Process clsProcess in Process.GetProcesses().Where(clsProcess => clsProcess.ProcessName.StartsWith("AcroRd32")))
            {
                clsProcess.Kill();
            }
        }

        private string GetAdobePath()
        {
            var adobe = Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("App Paths").OpenSubKey("AcroRd32.exe");
            var path = adobe.GetValue("").ToString();
            return path;
        }
    }
}
