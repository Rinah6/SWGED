namespace API.Utils
{
    public class File
    {
        public static string GetFileName(string spath)
        {
            int count = 1;

            string? path = Path.GetDirectoryName(spath);

            if (path == null)
            {
                return spath;
            }

            string fileNameOnly = Path.GetFileNameWithoutExtension(spath);
            string extension = Path.GetExtension(spath);

            string newFullPath = spath;

            while (System.IO.File.Exists(newFullPath))
            {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count);

                newFullPath = Path.Combine(path, tempFileName + extension);

                count += 1;
            }

            return newFullPath;
        }

        public static void CreateDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static async Task CreateFile(IFormFile upload, string path)
        {
            var streamCopy = new FileStream(path, FileMode.Create);

            await upload.CopyToAsync(streamCopy);

            await streamCopy.DisposeAsync();
        }
    }
}
