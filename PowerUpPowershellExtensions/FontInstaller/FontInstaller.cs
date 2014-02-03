using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FontInstall
{
    public class FontInstaller
    {       
        public static bool CopyFontToWindowsFontFolder(string fontFilePath)
        {
            FileInfo fontFile = new FileInfo(fontFilePath);
            if (!fontFile.Exists)
                return false;

            var windowsFontFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            var shell = new Shell32.Shell();
            var destinationPath = Path.Combine(windowsFontFolderPath, Path.GetFileName(fontFilePath));            
            var folder = shell.NameSpace(windowsFontFolderPath);
            folder.CopyHere(fontFilePath, 32);

            return true;
        }        
    }
}