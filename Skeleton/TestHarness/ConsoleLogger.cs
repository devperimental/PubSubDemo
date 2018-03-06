using CommonTypes.Behaviours;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestHarness
{
    public class ConsoleLogger : IAppLogger
    {
        public void LogError(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            var innerException = ex.InnerException;
            var message = ex.Message;
            while (innerException != null)
            {
                message += Environment.NewLine;
                message += innerException.Message;
                innerException = innerException.InnerException;
            }

            Console.WriteLine(message);
            Console.ResetColor();
        }

        public void LogMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void LogWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
