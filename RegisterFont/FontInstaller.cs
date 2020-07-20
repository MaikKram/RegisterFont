using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;

namespace RegisterFont
{
    public enum SendMessageTimeoutFlags : uint
    {
        SMTO_NORMAL = 0x0,
        SMTO_BLOCK = 0x1,
        SMTO_ABORTIFHUNG = 0x2,
        SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
        SMTO_ERRORONEXIT = 0x20
    }

    static public class FontInstaller
    {
        const int WM_FONTCHANGE = 0x001D;
        const int HWND_BROADCAST = 0xffff; 
        
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int AddFontResource(string lpFileName);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RemoveFontResource(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int WriteProfileString(string lpszSection, string lpszKeyName, string lpszString);

        //may hang the application - should not be used
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int SendMessageTimeout(int hWnd, uint Msg, int wParam, int lParam, SendMessageTimeoutFlags fuflags, uint timeout, out IntPtr result);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostMessage(int hWnd, uint Msg, int wParam, int lParam);

        static private bool RegisterFont(string filePath)
        {
            // Try install the font.
            try { 
                if (File.Exists(filePath))
                {
                    Log.WriteOK("trying to register \"" + filePath + "\"");
                
                    var result = -1;
                    if (Path.GetExtension(filePath).ToLower() == ".pfm")
                    {
                        result = AddFontResource(filePath + "|" + Path.ChangeExtension(filePath, ".pfb"));
                    }
                    else
                    {
                        result = AddFontResource(Path.GetFullPath(filePath));
                    }

                    if (result == 0)
                    {
                        Log.WriteError("Failed to register \"" + filePath + "\"");
                        return false;
                    }
                    else
                    {
                        Log.WriteOK("Successfully registered \"" + filePath + "\"");
                        return true;
                    }
                }
            } catch (Exception ex) {
                Log.WriteError("Exception onccured while registering font resource." + ex.Message);
                return false;
            }
            return false;
        }

        static private bool UnRegisterFont(string filePath)
        {
            // Try install the font.
            try
            {
                if (File.Exists(filePath))
                {
                    Log.WriteOK("trying to unregister \"" + filePath + "\"");

                    bool result = false;
                    if (Path.GetExtension(filePath).ToLower() == ".pfm")
                    {
                        result = RemoveFontResource(filePath + "|" + Path.ChangeExtension(filePath, ".pfb"));
                    }
                    else
                    {
                        result = RemoveFontResource(Path.GetFullPath(filePath));
                    }

                    if (result == false)
                    {
                        Log.WriteError("Failed to unregister \"" + filePath + "\"");
                        return false;
                    }
                    else
                    {
                        Log.WriteOK("Successfully unregistered \"" + filePath + "\"");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteError("Exception onccured while unregistering font resource." + ex.Message);
                return false;
            }
            return false;
        }

        static private void SendFontChangeMessage()
        {
            try
            {
                int _result = -1;
                IntPtr lpdwResult = IntPtr.Zero;
                if (Program.ArgPostMessage)
                {
                    // use post message
                    PostMessage(HWND_BROADCAST, WM_FONTCHANGE, 0, 0);
                } else if (Program.ArgSendMessageTimeOut > 0)
                {
                    // use send message timeout
                    _result = SendMessageTimeout(HWND_BROADCAST, WM_FONTCHANGE, 0, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, Program.ArgSendMessageTimeOut, out lpdwResult);
                } else {
                    // use send message  
                    _result = SendMessage(HWND_BROADCAST, WM_FONTCHANGE, 0, 0);
                }

                if (_result != 0)
                {
                    Log.WriteOK("Sending FONTCHANGE BROADCAST message OK");
                }
                else
                {
                    
                    Log.WriteError("Sending FONTCHANGE BROADCAST message FAILED. Returncode: " + Marshal.GetLastWin32Error().ToString());

                }
            } catch (Exception ex) {
                Log.WriteError("Exception onccured while sending FONTCHANGE BROADCAST message." + ex.Message);
            }
        }

        static public int RegisterSingleFont(string file)
        {
            if (RegisterFont(file))
            {
                SendFontChangeMessage();
                return 0;
            }
            else return 1;
        }
        
        static public int RegisterMultipleFont(string[] files)
        {
            byte _resultflag = 0;

            foreach (string sFile in files)
            {
                bool _bresult = RegisterFont(sFile);
                _resultflag |= _bresult ? (byte)1 : (byte)2;
                //Log.Write(_resultflag.ToString());
            }

            SendFontChangeMessage();
            return _resultflag -1;
        }

        static public int UnregisterSingleFont(string file)
        {
            if (UnRegisterFont(file))
            {
                SendFontChangeMessage();
                return 0;
            }
            else return 1;
        }

        static public int UnregisterMultipleFont(string[] files)
        {
            byte _resultflag = 0;

            foreach (string sFile in files)
            {
                bool _bresult = UnRegisterFont(sFile);
                _resultflag |= _bresult ? (byte)1 : (byte)2;
                //Log.Write(_resultflag.ToString());
            }

            SendFontChangeMessage();
            return _resultflag - 1;
        }
        /*
        static public void InstallFontPermanently(string sFilePath)
        {
            string fontsDir = Environment.GetEnvironmentVariable("windir");
            fontsDir = fontsDir == null
                ? fontsDir = "c:\\windows\\fonts"
                : fontsDir = Path.Combine(fontsDir, "fonts");
            fontsDir = Path.Combine(fontsDir, Path.GetFileName(sFilePath));

            File.Copy(sFilePath, fontsDir);
            Console.WriteLine("Font " + fontsDir + " copied...");

            int ret = WriteProfileString("fonts", "test" + " (TrueType)", Path.GetFileName(sFilePath));
            Console.WriteLine("Font " + fontsDir + " registered...");
            
            RegisterFont(sFilePath);
        }
         * */
    }
}
