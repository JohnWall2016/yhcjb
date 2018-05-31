

namespace YHCJB.Util
{
    public static class Utils
    {
        public static string FileNameAppend(string fileName, string append)
        {
            var idx = fileName.LastIndexOf('.');
            var newFile = "";
            if (idx > 0)
            {
                newFile = fileName.Substring(0, idx);
                newFile += append;
                newFile += fileName.Substring(idx);
            }
            else
                newFile = fileName + append;
            return newFile;
        }
    }
}
