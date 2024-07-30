namespace API.Services
{
    public class ConvertToImage
    {
        public async Task<MemoryStream> ConvertTxtToPDF(string path)
        {
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);

            var memoryStream = new MemoryStream();

            await fileStream.CopyToAsync(memoryStream);

            memoryStream.Position = 0;

            await fileStream.DisposeAsync();

            return memoryStream;
        }
    }
}
