using System;
using NLog;

namespace Logger_Trial
{
    class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            try
            {
                Logger.Debug("Testing");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Goodbye");
                throw;
            }

        }
    }
}
