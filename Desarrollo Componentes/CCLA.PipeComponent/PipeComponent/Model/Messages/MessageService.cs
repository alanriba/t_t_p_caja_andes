using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PipeComponent.Model.Messages
{
    class MessageService
    {
        public MessageCommand Command { get; set; }
        public List<string> Data { get; set; }

        public MessageService()
        { }

        public static MessageService Parse(string s)
        {
            if (s == null)
                throw new ArgumentNullException("content");

            if (s.Equals(string.Empty))
                throw new InvalidCastException("La cadena de entrada está vacía");

            if (s.Trim().Equals(string.Empty))
                throw new InvalidCastException("La cadena de entrada solo contiene espacios");

            string[] dataArray = s.Split('~');
            int commandNumber;

            if (int.TryParse(dataArray[0], out commandNumber))
            {
                if (Enum.IsDefined(typeof(MessageCommand), commandNumber))
                {
                    MessageService returnObject = new MessageService();
                    MessageCommand command = (MessageCommand)Enum.Parse(typeof(MessageCommand), commandNumber.ToString());

                    returnObject.Command = command;
                    if (dataArray.Length >= 2)
                    {
                        returnObject.Data = dataArray.Skip(1).ToList();
                    }

                    return returnObject;

                }
                else
                {
                    throw new InvalidCastException("Comando desconocido");
                }
            }
            else
            {
                throw new InvalidCastException("El comando no es numérico");
            }
        }
    }
}
