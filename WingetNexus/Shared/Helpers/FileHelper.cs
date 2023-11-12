using System.Text;
using System.Security.Cryptography;

namespace WingetNexus.Shared.Helpers
{
    public static class FileHelper
    {
        public static string GetFileChecksum(string filePath)
        {
            using (SHA256 SHA256 = System.Security.Cryptography.SHA256.Create())
            {
                using (FileStream fileStream = System.IO.File.OpenRead(filePath))
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (var hash in SHA256.ComputeHash(fileStream))
                    {
                        builder.Append(hash.ToString("x2"));
                    }

                    return builder.ToString();
                }
            }
        }
    }
}
