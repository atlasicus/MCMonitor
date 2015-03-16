using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MC_Monitor
{
    class Backup
    {
        private static Boolean completed = false;

        public static void start()
        {
            completed = false;

            String dateFormat = FileConfig.GetDateFormat();
            String dateTime = DateTime.Now.ToString(dateFormat);

            foreach (String argument in FileConfig.GetBackupPlan())
            {
                String name = String.Format(argument, dateTime).Replace("\\ ", "\\");
                buildProcess(name);
            }

            completed = true;
        }

        private static void buildProcess(String args)
        {
            try
            {
                var backupProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        //FileName = "WZZIP",
                        FileName = FileConfig.GetBackupPath().Replace("\\", "\\\\"),
                        Arguments = args,
                        UseShellExecute = true
                    }
                };

                backupProcess.Start();

                backupProcess.WaitForExit(); //Wait until complete
            }
            catch (Exception e)
            {
                Console.WriteLine("MC Monitor cannot find the compression tool, check the file path again.\n  Error as follows:");
                Console.WriteLine(e.ToString());
                Console.WriteLine("");
                Console.WriteLine("Press Any Key to Continue...");
                Console.ReadLine();
                System.Environment.Exit(-101);
            }
        }

        public static Boolean isComplete()
        {
            return completed;
        }
    }
}
