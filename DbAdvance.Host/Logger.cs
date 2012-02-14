using System;

namespace DbAdvance.Host
{
    public class Logger : ILogger
    {
        public void Log(string message, params object[] parameters)
        {
            Console.WriteLine("<" + DateTime.Now.ToLongTimeString() + "> " + string.Format(message, parameters));
        }
    }
}