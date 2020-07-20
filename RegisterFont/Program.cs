using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;

namespace RegisterFont
{
    static class Program
    {

        #region DEGUB
        //[DllImport("kernel32.dll")]
        //private static extern IntPtr GetConsoleWindow();

        //[DllImport("kernel32.dll", SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        #endregion DEBUG

        //[DllImport("kernel32.dll", SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool AllocConsole();

        const int ATTACH_PARENT_PROCESS = -1;
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private static string LogFileDestination = string.Empty;

        public static bool ArgPostMessage = false;
        public static uint ArgSendMessageTimeOut = 3000;

        [STAThread]
        static void Main(string[] args)
        {
            #region DEBUG
            //args = new string[] { "Rotis Sans", "/s", "/l=temp" };
            //args = new string[] { "/?" };
           //Attach Console Windows if running from there...
            AttachConsole(ATTACH_PARENT_PROCESS);
            //System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(ATTACH_PARENT_PROCESS);
            #endregion DEBUG

            string _argFilePath = String.Empty;
 
            bool _register = true;
            //AllocConsole();
            string _argLogPath = Environment.GetEnvironmentVariable("TEMP") + "\\registerfonts.log";

            // init LogFile Writer
            try
            {
                Log.InitLogFiler(_argLogPath);
            } catch (Exception e)
            {
                Console.Write("Error initializing logfile on path " + _argLogPath + ". " + e.Message);
                System.Environment.Exit(1627);
            }

            // parse arguments
            for (int i = 0; i < args.Length && i < 4; i++)
            {
                // Help needed?
                if (HelpRequired(args[i]))
                {
                    DisplayHelp();
                    System.Environment.Exit(0);
                }

                else if (args[i].ToLower().StartsWith("/t") || args[i].ToLower().StartsWith("-t"))
                {
                    try
                    {   // timeout in milliseconds
                        ArgSendMessageTimeOut = uint.Parse(args[i].Split('=')[1]);
                        
                    } catch (Exception e)
                    {
                        Console.WriteLine("Invalid arguments.");
                        Log.WriteError("Invalid arguments.");
                        Log.WriteError(e.Message);
                        Exit(1);
                    }
                }

                else if (args[i].ToLower() == "/post" || args[i].ToLower() == "-post")
                { 
                    // use send or post message
                    ArgPostMessage = true;
                }

                else if (args[i].ToLower() == "/u" || args[i].ToLower() == "-u")
                {
                    _register = false;
                }
                else if (args[i].ToLower().StartsWith("/l") || args[i].ToLower().StartsWith("-l"))
                {
                    // is log-path argument?
                    try
                    {
                        LogFileDestination = args[i].Split('=')[1];
                        //_argLogPath = @args[i].Split('=')[1];
                        //Log.ReAllocate(_argLogPath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Invalid arguments.");
                        Log.WriteError("Invalid arguments.");
                        Log.WriteError(e.Message);
                        Exit(1);
                    }
                }
                else if (IsFileOrDirectory(args[i]))
                {
                    // is directory or file-path?
                    _argFilePath = args[i];
                }
                else
                {
                    Console.WriteLine("Invalid arguments.");
                    Log.WriteError("Invalid arguments.");
                    Exit(1);
                }
            }

            if (_argFilePath != String.Empty)
            {
                Log.WriteOK("Starting...");
                // single font ressource
                if (File.Exists(_argFilePath))
                {
                    try
                    {
                        Log.WriteOK("Installing single font-file.");
                        if (_register)
                        {
                            Log.WriteOK("Installing single font-file.");
                            Exit(FontInstaller.RegisterSingleFont(_argFilePath));
                        }
                        else
                        {
                            Log.WriteOK("Uninstalling single font-file.");
                            Exit(FontInstaller.UnregisterSingleFont(_argFilePath));

                        }
                    }
                    catch (Exception e)
                    {
                        Log.WriteError(e.Message);
                        Exit(1);
                    }
                }
                // multiple font ressource
                else if (Directory.Exists(_argFilePath))
                {
                    try
                    {
                        //string[] sFiles = new string[] { "*.fon", "*.fnt", "*.ttf", "*.ttc", "*.fot", "*.otf", "*.pfm" }.SelectMany(i => try{Directory.GetFiles(_argFilePath, i, _argSub ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)} catch{return null}).Distinct().ToArray();
                        string[] sFiles = new string[] { "*.fon", "*.fnt", "*.ttf", "*.ttc", "*.fot", "*.otf", "*.pfm" }.SelectMany(i => GetFiles(_argFilePath, i)).Distinct().ToArray();

                        if (_register)
                        {
                            Log.WriteOK("Installing multiple font-files from directory");
                            Exit(FontInstaller.RegisterMultipleFont(sFiles));
                        }
                        else
                        {
                            Log.WriteOK("Uninstalling multiple font-files from directory");
                            Exit(FontInstaller.UnregisterMultipleFont(sFiles));
                        }
                    }
                    catch (Exception e)
                    {
                        Log.WriteError(e.Message);
                        Exit(1);
                    }
                }
                else
                {
                    Log.WriteError("File or path not found!");
                    Exit(1);
                }
            }
            else
            {
                Log.WriteError("Missing file or path parameter!");
                Exit(1);
            }
        }

        private static bool HelpRequired(string param)
        {
            return param.ToLower() == "-h" || param.ToLower() == "--help" || param.ToLower() == "/?";
        }

        private static bool IsFileOrDirectory(string param)
        {
            bool test = Directory.Exists(param);
            return Directory.Exists(param) || File.Exists(param);
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("RegisterFont @2016 Cancom GmbH - HELP");
            Console.WriteLine("");
            Console.WriteLine("\t-t (/t)\t\tTimeOut in milliseconds for the program to stop broadcasting fontchanges to the systems");
            Console.WriteLine("\t\t\tprocesses. If set to 0 it will cause to wait infinite.");
            Console.WriteLine();
            Console.WriteLine("\t-post (/post)\tusing an alternative broadcast message. Timeout argument will be ignored.");
            Console.WriteLine();
            Console.WriteLine("\t-u (/u)\t\twill unregister the Fonts.");
            Console.WriteLine();
            Console.WriteLine("\t-l=<path> (/l=)\tUse an alternative path for the logile. The default path will be the temp-folder.");
            Console.WriteLine();
            Console.WriteLine("Usage: registerfont.exe [path incl. subpath or file] (/u | [/t 1000 | /post] | /l=<logfilepath>)");
            
        }

        private static void Exit(int exitcode)
        {
            switch (exitcode)
            {
                case 0:
                    {
                        Log.WriteOK("Font registering successfull.");
                        break;
                    }
                case 1:
                    {
                        Log.WriteError("Font registering failed.");
                        break;
                    }
                case 2:
                    {
                        Log.WriteWarning("Font registering partially successfull with warinings.");
                        break;
                    }
                default:
                    {
                        Log.WriteError("Undefined exit.");
                        break;
                    }
            }

            Log.WriteExit();

            if (LogFileDestination != String.Empty)
                Log.CopyTo(LogFileDestination);
            
            System.Environment.Exit(exitcode);  
        }

        private static IEnumerable<string> GetFiles(string root, string searchPattern)
        {
            Stack<string> pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                var path = pending.Pop();
                string[] next = null;
                try
                {
                    next = Directory.GetFiles(path, searchPattern);
                }
                catch (Exception e) { Log.WriteWarning(e.Message); }
                if (next != null && next.Length != 0)
                    foreach (var file in next) yield return file;
                try
                {
                    next = Directory.GetDirectories(path);
                    foreach (var subdir in next) pending.Push(subdir);
                }
                catch (Exception e) { Log.WriteWarning(e.Message); }
            }
        }
    
    }
}
