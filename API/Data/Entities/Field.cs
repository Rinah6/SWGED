namespace API.Data.Entities
{
    public class Field
    {
        public required string Variable { get; set; }
        public required int FirstPage { get; set; }
        public required int LastPage { get; set; }
        public required double X { get; set; }
        public required double Y { get; set; }
        public required double Width { get; set; }
        public required double Height { get; set; }
        public required double PDF_Width { get; set; }
        public required double PDF_Height { get; set; }
        public required FieldType FieldType { get; set; }
    }
}
