using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Diagnostics;

namespace PipeComponent
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (Debugger.IsAttached || args.Contains(value: "-console"))
            {
                PipeComponent pipeComponent = new PipeComponent();
                pipeComponent.StartOnConsoleMode(args);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new PipeComponent() 
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
