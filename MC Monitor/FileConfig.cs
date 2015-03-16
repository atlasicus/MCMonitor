using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MC_Monitor
{
    class FileConfig
    {
        private static Boolean ProfileValid = false;
        private static String ProfileName = null;
        private static List<String> SystemTokens = new List<String>();
        private static List<String> BackupTokens = new List<String>();

        public static void LoadProfile(String input)
        {
            SystemTokens = new List<String>();
            BackupTokens = new List<String>();

            ProfileName = input;

            var profile = File.ReadLines(@"mcmonitor.ini")
                .SkipWhile(line => !line.Contains("{Profile " + ProfileName + "}"))
                .SkipWhile(line => !line.Contains("[System]"))
                .SkipWhile(line => line.StartsWith(";"))
                .Skip(1)
                .TakeWhile(line => !line.Contains("[Backup]"));

            if (profile.Any())
            {
                ProfileValid = true;
                foreach (String item in profile)
                    SystemTokens.Add(item);
            }
            else
            {
                ProfileValid = false;
                Console.WriteLine("Invalid profile");
                return;
            }

            profile = File.ReadLines(@"mcmonitor.ini")
                .SkipWhile(line => !line.Contains("{Profile " + ProfileName + "}"))
                .SkipWhile(line => !line.Contains("[Backup]"))
                .SkipWhile(line => line.StartsWith(";"))
                .Skip(1)
                .TakeWhile(line => !line.Contains("{Profile"));

            if (profile.Any())
            {
                ProfileValid = true;
                foreach (String item in profile)
                    BackupTokens.Add(item);
            }
            else
            {
                ProfileValid = false;
                Console.WriteLine("Invalid profile");
                return;
            }
        }

        public static Boolean IsProfileValid()
        {
            return ProfileValid;
        }

        public static String GetCurrentProfileName()
        {
            return ProfileName;
        }

        public static String GetJavaPath()
        {
            foreach (String item in SystemTokens)
            {
                if (item.Contains("JavaPath") && !item.StartsWith(";"))
                {
                    String[] subTokens = item.Split('=');
                    if (!subTokens[0].StartsWith(";"))
                    {
                        return subTokens[1];
                    }
                }
            }

            return "";
        }

        public static String GetSystemPath()
        {
            foreach (String item in SystemTokens)
            {
                if (item.Contains("Path") && !item.StartsWith(";"))
                {
                    String[] subTokens = item.Split('=');
                    if (!subTokens[0].StartsWith(";"))
                    {
                        return subTokens[1];
                    }
                }
            }

            return "";
        }

        public static String GetProcArgs()
        {
            foreach (String item in SystemTokens)
            {
                if (item.Contains("Args") && !item.StartsWith(";"))
                {
                    String[] subTokens = item.Split('=');

                    if (!subTokens[0].StartsWith(";"))
                        return subTokens[1];
                }
            }

            return "";
        }

        public static List<String> GetTimeList()
        {
            List<String> times = new List<String>();

            foreach (String item in SystemTokens)
            {
                if (item.Contains("Time") && !item.StartsWith(";"))
                {
                    String[] subTokens = item.Split('=');

                    if (!subTokens[0].StartsWith(";"))
                        times.Add(subTokens[1].Replace(" ", ""));
                }
            }

            return times;
        }

        public static String GetDateFormat()
        {
            foreach (String item in BackupTokens)
            {
                if (item.Contains("DateFormat") && !item.StartsWith(";"))
                {
                    String[] subTokens = item.Split('=');

                    if (!subTokens[0].StartsWith(";"))
                        return subTokens[1];
                }
            }

            return "";
        }

        public static String GetBackupPath()
        {
            foreach (String item in BackupTokens)
            {
                if (item.Contains("ZipPath") && !item.StartsWith(";"))
                {
                    String[] subTokens = item.Split('=');
                    if (!subTokens[0].StartsWith(";"))
                        return subTokens[1];
                }
            }

            return "";
        }

        public static List<String> GetBackupPlan()
        {
            List<String> backupArgs = new List<String>();

            foreach (String item in BackupTokens)
            {
                if (item.Contains("Backup") && !item.StartsWith(";"))
                {
                    String[] subTokens = item.Split('=');
                    if (!subTokens[0].StartsWith(";"))
                         backupArgs.Add(subTokens[1]);
                }
            }

            return backupArgs;
        }

        public static void Report()
        {
            Console.WriteLine();

            foreach (String item in SystemTokens)
                Console.WriteLine(item);

            Console.WriteLine();

            foreach(String item in BackupTokens)
                Console.WriteLine(item);
        }
    }
}
