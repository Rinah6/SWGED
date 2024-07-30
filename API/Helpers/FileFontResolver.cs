using PdfSharp.Fonts;

namespace API.Helpers
{
    public class FileFontResolver : IFontResolver
    {
        public string DefaultFontName => throw new NotImplementedException();

        public byte[] GetFont(string faceName)
        {
            using var ms = new MemoryStream();
            using var fs = File.Open(faceName, FileMode.Open);

            fs.CopyTo(ms);
            ms.Position = 0;

            return ms.ToArray();
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            return new FontResolverInfo("wwwroot/Fonts/Verdana.ttf");
        }
    }
}
