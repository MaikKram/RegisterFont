using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RegisterFont
{
    static class Log
    {
        static private StreamWriter _sw;
        static private string _logfilepath;

        static public void InitLogFiler(string filepath)
        {
            try
            {
                _logfilepath = filepath;
                _sw = new StreamWriter(_logfilepath);
                //_sw.AutoFlush = true;
                WriteStart();
            }
            catch(Exception e)
            {
                throw new Exception("Error creating or accessing LogFile for " + filepath + ".", e);
            }
        }

        static public void CopyTo(string filepath)
        {
            try { 
            // if only directory then, attach a filename
            if (Path.GetExtension(filepath) == String.Empty)
            {
                if (Directory.Exists(filepath))
                    filepath = Path.Combine(filepath, "registerfonts.log");
                else
                {
                    Log.WriteError(String.Format("Error copying LogFile from '{0}' to '{1}'. Target directory does not exist.", _logfilepath, filepath));
                    return;
                }
            }
         
            // if file already exists, try to create a new unique filename
            if (File.Exists(filepath))
            {
                filepath = Path.Combine(Path.GetDirectoryName(filepath), Path.GetFileNameWithoutExtension(filepath) + "_" + DateTime.Now.ToString("yyMMdd_hhmmss_ffffFFFF") + Path.GetExtension(filepath));
            }

            if (File.Exists(_logfilepath) && filepath != _logfilepath)
            {
                try
                {
                    //_sw.Close();
                    File.Copy(_logfilepath, filepath, false);
                }
                catch (Exception e)
                {
                    Log.WriteError(String.Format("Error copying LogFile from '{0}' to '{1}'. ", _logfilepath, filepath) + e.Message);
                }
            }
            }
            catch (Exception e)
            {
                throw new Exception("Error copying LogFile to " + filepath + ".", e);
            }

        }

        static public void Write(string text)
        {
            try
            {
                string _line = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\t" + text;
                _sw.WriteLine(_line);
                Console.WriteLine(_line);
                _sw.Flush();
            }
            catch (Exception e)
            {
                throw new Exception("Error writing to LogFile.", e);
            }
        }

        static public void WriteOK(string text)
        {
            Write("[OK]\t\t\t" + text);
        }

        static public void WriteStart()
        {
            Write("[START]");
        }
        
        static public void WriteExit()
        {
            Write("[EXIT]");
        }

        static public void WriteError(string text)
        {
            Write("[ERROR]\t\t" + text);
        }
        
        static public void WriteWarning(string text)
        {
            Write("[WARNING]\t" + text);
        }
    }
}
