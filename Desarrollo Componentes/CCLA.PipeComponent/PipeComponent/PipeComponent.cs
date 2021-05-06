using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.IO;
using PipeComponent.Properties;
using System.Reflection;
using PipeComponent.Model;

namespace PipeComponent
{
    public partial class PipeComponent : ServiceBase
    {
        private Timer timerAlive;

        public PipeComponent()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string rutaLog = Settings.Default.RutaLog.Replace("./", AppDomain.CurrentDomain.BaseDirectory);
            DateTime buildDate = ApplicationInformation.CompileDate;

            Log.PathLog = rutaLog;
            Log.WriteLine(LogType.Inicio, "Servicio PipeComponent v{0} Generado el {1}", version, buildDate.ToString("dd-MM-yyyy HH:mm:ss"));

            MyConsole.Clear();
            MyConsole.WriteLine();
            MyConsole.WriteLine("Servicio PipeComponent");
            MyConsole.WriteLine("Generado el {0}", buildDate.ToString("dd-MM-yyyy HH:mm:ss"));
            MyConsole.WriteLine(new string('-', 50));
            MyConsole.WriteLine("PARÁMETROS");
            MyConsole.WriteLine(" Nombre Pipe       : " + Settings.Default.NombrePipe);
            MyConsole.WriteLine(" Nombre Impresora  : " + Settings.Default.Impresora);
            MyConsole.WriteLine(" Nombre Imp. Laser : " + Settings.Default.ImpresoraLaser);
            MyConsole.WriteLine(" Ruta Log          : " + rutaLog);
            MyConsole.WriteLine(new string('-', 50));

            PipeServer pipeServer = new PipeServer(Settings.Default.NombrePipe);
            pipeServer.StartListeningPipes();
            MyConsole.WriteLine("Servidor Pipe iniciado");
            Log.WriteLine(LogType.InicioOperacion, null);

            timerAlive = new Timer();
            timerAlive.Elapsed += new ElapsedEventHandler(timerAlive_Elapsed);
            timerAlive.Interval = 60 * 1000;
            timerAlive.Enabled = true;
            timerAlive.Start();
        }

        protected override void OnStop()
        { }

        private void timerAlive_Elapsed(object sender, EventArgs e)
        {
            MyConsole.WriteLine("...");
        }

        internal void StartOnConsoleMode(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
        }
    }
}
