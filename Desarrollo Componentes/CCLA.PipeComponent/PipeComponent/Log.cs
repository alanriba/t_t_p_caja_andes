using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PipeComponent
{
    public enum LogType
    {
        Inicio,
        Request,
        Response,
        Fin,
        Error,
        Parametros,
        Conexion,
        Desconexion,
        Timeout,
        InicioOperacion,
        Audio,
        Evento,
        Ftp,
        Microfono,
        Registry,
        Query
    }

    public class Log
    {
        private static Dictionary<LogType, string> LogTypeToString = new Dictionary<LogType, string>()
        { 
            { LogType.Inicio, "INI" },
            { LogType.Request, "REQ" },
            { LogType.Response, "RES" },
            { LogType.Error, "ERR" },
            { LogType.Fin, "FIN" },
            { LogType.Parametros, "PAR" },
            { LogType.Conexion, "CXN" },
            { LogType.Desconexion, "DXN" },
            { LogType.Timeout, "TIM" },
            { LogType.InicioOperacion, "IOP" },
            { LogType.Audio, "AUD" },
            { LogType.Evento, "EVE" },
            { LogType.Ftp, "FTP" },
            { LogType.Microfono, "MIC" },
            { LogType.Registry, "REG" },
            { LogType.Query, "QRY" }
        };

        private static readonly object Locker = new object();
        private static string Command;
        public static string PathLog;

        public static void WriteLine(LogType logType, string message)
        {
            
            StringBuilder sb;
            string logName = "PipeComponent" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            string fullPath = Path.GetFullPath(PathLog);

            if (!Directory.Exists(fullPath))
                throw new DirectoryNotFoundException("El directorio del log no pudo ser encontrado: " + fullPath);

            lock (Locker)
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(fullPath, logName), true))
                {
                    sb = new StringBuilder();

                    if (logType == LogType.Inicio)
                    {
                        sb.AppendLine();
                    }
                    sb.Append(DateTime.Now.ToString("HH:mm:ss.fff")).Append(",");
                    sb.Append(Command == null ? "000" : Command).Append(",");
                    sb.Append(LogTypeToString[logType]).Append(":");
                    sb.Append(message);

                    sw.WriteLine(sb);
                    sw.Flush();
                    sw.Close();
                    sb = null;
                    Command = null;
                }
            }
        }

        public static void WriteLine(LogType logType, string message, params object[] args)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            message = string.Format(message, args);
            WriteLine(logType, message);
        }
    }
}
