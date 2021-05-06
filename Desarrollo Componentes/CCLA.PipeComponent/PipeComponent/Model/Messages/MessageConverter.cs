using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PipeComponent.Model.Messages
{
    class MessageConverter
    {
        public static byte[] ToByteArray(params object[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            if (!args.Any())
                return null;

            return Encoding.GetEncoding("Windows-1252").GetBytes(string.Join("~", args));
        }
    }
}
