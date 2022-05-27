using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace VidU_Studio.util
{
    internal static class Util
    {
        internal static bool IsVideo(this StorageFile value) => 
            (value.FileType == ".mp4" || value.FileType == ".mkv" || value.FileType== ".mpeg");

        internal static List<string> SupportedFileTypes = new List<string> { ".jpeg", ".png", 
                            ".jpg", ".raw", ".mp4", ".mkv", ".mpeg"};
    }
}
