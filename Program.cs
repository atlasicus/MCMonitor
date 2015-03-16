using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MC_Monitor
{
    class Program
    {
        static String input = null;
        static Boolean isRunning = true;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                FileConfig.LoadProfile("default");
            }
            else
                FileConfig.LoadProfile(args[0]);

            if (FileConfig.IsProfileValid())
            {
                String path = FileConfig.GetSystemPath();
                String tempArgs = FileConfig.GetProcArgs();
                String procArgs = (String.Format(tempArgs, path));

                var serverProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "java",
                        Arguments = procArgs,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true
                    }
                };

                StreamWriter serverInput = serverStart(serverProcess);

                Thread inputThread = new Thread(new ThreadStart(getInput));
                Thread backupThread = new Thread(() => checkForBackup(serverInput));

                inputThread.Start();
                backupThread.Start();

                while (isRunning)
                {
                    if (input != null)
                    {
                        if (input.Equals("status", StringComparison.OrdinalIgnoreCase))
                        {
                            if (isProcessRunning("Java") == false)
                                Console.WriteLine("{{{{{{Blackhole}}}}}}}}");
                            else
                                Console.WriteLine("Server is running");
                        }
                        else if (input.Equals("start", StringComparison.OrdinalIgnoreCase))
                        {
                            serverInput = serverStart(serverProcess);
                        }
                        else if (input.Equals("stop", StringComparison.OrdinalIgnoreCase))
                        {
                            serverInput.WriteLine(input);
                            serverStop(serverProcess);
                        }

                        else if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                        {
                            serverInput.WriteLine("stop");
                            serverStop(serverProcess);
                            isRunning = false;
                            break;
                        }

                        else if (input.Equals("backupnow", StringComparison.OrdinalIgnoreCase))
                        {
                            backup(serverInput);
                        }

                        else if (input.ToLower().Contains("load"))
                        {
                            String profile = input.Split(' ')[1];
                            FileConfig.LoadProfile(profile);

                            if (input.ToLower().Contains("-v"))
                            {
                                if (FileConfig.IsProfileValid())
                                {
                                    FileConfig.Report();
                                }
                            }
                        }

                        else
                        {
                            serverInput.WriteLine(input);
                        }

                        input = null;
                    }

                    if (isProcessRunning("Java") == false)
                    {
                        serverStop(serverProcess);
                        serverInput = serverStart(serverProcess);
                    }
                }
                    
                inputThread.Join();
                backupThread.Join();
            }
            else
            {
                while (FileConfig.IsProfileValid() == false)
                {
                    Console.Write("Enter valid profile name: ");
                    String tempName = Console.ReadLine();

                    FileConfig.LoadProfile(tempName);
                }
            }
        }

        static void ServerErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        static void ServerOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        static void checkForBackup(StreamWriter writer)
        {
            while (isRunning)
            {
                foreach (String timeIn in FileConfig.GetTimeList())
                {
                    DateTime time = Convert.ToDateTime(timeIn);
                    TimeSpan diff = time.Subtract(DateTime.Now);

                    if (diff.TotalSeconds > 5 && diff.TotalSeconds < 10)
                    {
                        backup(writer);
                    }
                }

                Thread.Sleep(5000);
            }
        }

        static void backup(StreamWriter writer)
        {
            writer.WriteLine("say System Backup started");
            writer.WriteLine("say World writing disabled");

            writer.WriteLine("save-off");
            writer.WriteLine("save-all");

            Thread.Sleep(1000);
            Backup.start();

            while (Backup.isComplete() == false)
            {
                Thread.Sleep(1000);
            }

            writer.WriteLine("save-on");

            writer.WriteLine("say System Backup complete");
            writer.WriteLine("say World writing enabled");

        }

        static Boolean isProcessRunning(String name)
        {
            Boolean isRunning = false;
            Process[] processList = Process.GetProcesses();

            foreach (Process p in processList)
            {
                if ((p.ProcessName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    isRunning = true;
            }

            return isRunning;
        }

        static StreamWriter serverStart(Process process)
        {
            process.OutputDataReceived += new DataReceivedEventHandler(ServerOutputDataReceived); //Enable reader delegate 1
            process.ErrorDataReceived += new DataReceivedEventHandler(ServerErrorDataReceived); //Enable reader delegate 2 need both
            process.Start();

            StreamWriter server = process.StandardInput;
            process.BeginOutputReadLine(); //Start writing to console
            process.BeginErrorReadLine();  //Start writing to console need both

            return server;
        }

        static void serverStop(Process process)
        {
            process.OutputDataReceived -= new DataReceivedEventHandler(ServerOutputDataReceived); //Disable reader delegate 1
            process.ErrorDataReceived -= new DataReceivedEventHandler(ServerErrorDataReceived); //Disable reader delegate 2 need both

            process.CancelErrorRead();
            process.CancelOutputRead();
        }


        static void getInput()
        {
            while (isRunning)
            {
                input = Console.ReadLine();
            }
        }
    }
}
