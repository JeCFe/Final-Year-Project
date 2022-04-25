using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_SSL_Server
{
    //This class is used for any time the Log File is being looked at
    class FileHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("FileHandler.cs");

        //This functions looks inside the log folder and returns all files in the folder
        public static string[] LogEntries() { return Directory.GetFiles("..\\Debug\\logs"); }

        /*
         This function will return the log code from each logged message eg INFO or ERROR
       */
        private string FindLogCode(string stringFromFile)
        {
            bool inDelimRange = false;
            string logCode = "";
            foreach (char item in stringFromFile)
            {
                if (item == ']') { inDelimRange = false; break; }
                if (inDelimRange) { logCode = logCode + item; }
                if (item == '[') { inDelimRange = true; }
            }
            logCode = logCode.TrimEnd();
            return logCode;
        }

        /*
         This function will use FindLogCode to add logged event with specific log codes to a list
         Then return the list
        */
        private List<string> LogEventsWithSpecificCodes(string logCode, string path)
        {
            List<string> logsWithCode = new List<string>();
            List<string> fileEnteries = GetLogEnteries(path);
            if (logCode == "ALL") { return fileEnteries; }
            foreach (string fileEntry in fileEnteries)
            {
                if (FindLogCode(fileEntry) == logCode) { logsWithCode.Add(fileEntry); }
            }
            return logsWithCode;
        }

        /*
         This functions to get the most recent error message from the most recent log file
         Using the LogEventsWithSpecificCodes passing ERROR will return all errror messages from the most recent file
         Reversing the returned list and taking the fisrt item will ensure that the one recent is selected
         Using split to only take the message from the most recent log and returning this
        */
        public string LastError()
        {
            string[] fileEntries = LogEntries();
            string path = fileEntries[fileEntries.Length - 1];
            string logCode = "ERROR";
            List<string> fileContentWithError = LogEventsWithSpecificCodes(logCode, path);
            fileContentWithError.Reverse();
            string lastEntry = fileContentWithError[0];
            return lastEntry.Split('-').Last().TrimStart();
        }

        /*
         Uses the choice from the Log Menu to be used to tell LogEventsWithSpecificCodes which error code to look for
         Returning the resultant logs
        */
        public List<string> ReteriveSpecificLogs(int choice, string path)
        {
            List<string> logsToReturn = new List<string>();
            string logCode = "";
            switch (choice)
            {
                case 1:
                    logCode = "ALL";
                    break;
                case 2:
                    logCode = "INFO";
                    break;
                case 3:
                    logCode = "ERROR";
                    break;
                case 4:
                    logCode = "FATAL";
                    break;
                default:
                    break;
            }
            logsToReturn = LogEventsWithSpecificCodes(logCode, path);
            return logsToReturn;
        }

        /*
         This gets all the logged messages from a file 
         Returning all the logged messages
         By allowing FileShare Write means 
            That means Log4Net can use its inbuild thread friendly framework to continue to write log events whilst reading
            During testing this proved to have no realistic impacts on the logging with no loss of data
            While logs that are made during the reading process maybe missed during the first read
            They will be picked up with later reads
        */
        public List<string> GetLogEnteries(string path)
        {
            List<string> fileEntries = new List<string>();
            try
            {
                using (FileStream fileStream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Write))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        while (streamReader.EndOfStream == false){ fileEntries.Add(streamReader.ReadLine()); }
                        streamReader.Close();
                    }
                    fileStream.Close();
                }
                return fileEntries;
            }
            catch (Exception e)
            {
                log.Error("Error reteriving log data: " + e.ToString());
                return null;
            }
        }

    }
}
