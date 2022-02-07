using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch=true)] //Wwatch ensure if the config is changed then the program will react in realtime



namespace Multithreading_Logging_Prototype
{
    class Program
    {
        private static bool threadContinue = true;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Program.cs");
        static void Main(string[] args)
        {
            for (int i = 0; i < 2; i++)
            {
                Thread myThread = new Thread(TestThreadFunction);
                myThread.Start();
            }

            
            Console.WriteLine("View Logs:");

            Console.ReadLine();
            while (true)
            {
                Console.ReadLine();

                try
                {
                    using (FileStream fileStream = new FileStream(
                        "..\\Debug\\logs\\Log_28-01-2022.txt",
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Write))
                    {
                        using (StreamReader streamReader = new StreamReader(fileStream))
                        {
                            log.Info("Begin");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(streamReader.ReadToEnd());
                            Console.WriteLine("-----------------------------------");
                            log.Info("End");
                            streamReader.Close();

                        }
                        fileStream.Close();
                    }



                }
                catch (Exception e)
                {

                    throw;
                }
            }
        }
        

        static void TestThreadFunction()
        {
            int x = 0;
            //while (threadContinue == true)
            //{
            //    log.Info(x);

            //    x++;
            //}
            for (int i = 0; i < 1000; i++)
            {
                log.Info(i);
                Thread.Sleep(1);
            }
        }
    }
}
