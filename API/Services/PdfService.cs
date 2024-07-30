using API.Model;
using PdfSharp.Drawing;
using System.Drawing;
using PdfSharp.Fonts;
using API.Helpers;
using API.Data;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace API.Services
{
    public class PdfService
    {
        private double PixelsToPoints(double pixels, int dpi)
        {
            const double PointsPerInch = 72.0;

            return pixels / dpi * PointsPerInch;
            // return 0.75 * pixels;
        }

        private async Task<MemoryStream?> CreatePDFCopy(string url)
        {
            FileStream fileStream = new(Path.Combine("wwwroot/store", url), FileMode.Open, FileAccess.Read);

            MemoryStream memoryStream = new();

            await fileStream.CopyToAsync(memoryStream);
            // memoryStream.Position = 0;
            await fileStream.DisposeAsync();

            return memoryStream;
        }

        private void DrawBox(PdfDocument pdf, Data.Entities.Field field, Color color)
        {
            for (int i = field.FirstPage - 1; i <= field.LastPage - 1; i += 1)
            {
                PdfPage page = pdf.Pages[i];

                var scale = new
                {
                    width = page.Width / field.PDF_Width,
                    height = page.Height / field.PDF_Height
                };

                var x = PixelsToPoints(field.X * scale.width, 96);
                var y = PixelsToPoints(field.Y * scale.height, 96);
                var width = PixelsToPoints(field.Width * scale.width, 96);
                var height = PixelsToPoints(field.Height * scale.height, 96);
                var gfx = XGraphics.FromPdfPage(page);
                XRect rectangle = new(x, y, width, height);
                XPen borderPen = new(XColors.Black, 1)
                {
                    DashStyle = XDashStyle.Dash,
                    Color = XColor.FromArgb(255, color.R, color.G, color.B),
                    Width = 2
                };
                XSolidBrush brush = new(XColor.FromArgb(50, 100, 100, 100));

                gfx.DrawRectangle(brush, rectangle);
                gfx.DrawRectangle(borderPen, rectangle);

                gfx.Dispose();
            }
        }

        private void CreateBox(PdfDocument pdf, Data.Entities.UserDocument recipient)
        {
            if (pdf.Version < 14)
            {
                pdf.Version = 14;
            }

            var color = ColorTranslator.FromHtml(recipient.Color!);

            foreach (var field in recipient.Fields)
            {
                DrawBox(pdf, field, color);
            }
        }

        private XImage ByteToXImage(byte[] imageByte)
        {
            using MemoryStream stream = new(imageByte, 0, imageByte.Length, true, true);

            var image = XImage.FromStream(stream);

            return image;
        }

        private void DrawField(PdfDocument pdf, Data.Entities.Field field, XImage image)
        {
            for (int i = field.FirstPage - 1; i <= field.LastPage - 1; i += 1)
            {
                PdfPage page = pdf.Pages[i];

                var scale = new
                {
                    width = page.Width / field.PDF_Width,
                    height = page.Height / field.PDF_Height
                };

                var x = PixelsToPoints(field.X * scale.width, 96);
                var y = PixelsToPoints(field.Y * scale.height, 96);
                var width = PixelsToPoints(field.Width * scale.width, 96);
                var height = PixelsToPoints(field.Height * scale.height, 96);

                var gfx = XGraphics.FromPdfPage(page);
                gfx.DrawImage(image, x, y, width, height);
                gfx.Dispose();
            }
        }

        private void BuildField(PdfDocument pdf, Data.Entities.UserDocument recipient)
        {
            if (pdf.Version < 14)
            {
                pdf.Version = 14;
            }

            XImage? xImageSign = null;
            XImage? xImageParaphe = null;

            if (recipient.Signature != null)
            {
                xImageSign = ByteToXImage(recipient.Signature);
            }

            if (recipient.Paraphe != null)
            {
                xImageParaphe = ByteToXImage(recipient.Paraphe);
            }

            foreach (var field in recipient.Fields)
            {
                if (field.FieldType == FieldType.Signature && xImageSign != null)
                {
                    DrawField(pdf, field, xImageSign);
                }
                else if (field.FieldType == FieldType.Paraphe && xImageParaphe != null)
                {
                    DrawField(pdf, field, xImageParaphe);
                }
            }
        }

        private void AddWatermark(PdfDocument pdf)
        {
            GlobalFontSettings.FontResolver ??= new FileFontResolver();

            if (pdf.Version < 14)
            {
                pdf.Version = 14;
            }

            foreach (PdfPage page in pdf.Pages)
            {
                var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
                XFont font = new("Verdana", 100);
                XSize textSize = gfx.MeasureString("SoftGED", font);
                double centerX = page.Width / 2;
                double centerY = page.Height / 2;
                int alphaValue = 128;
                var textColor = XColor.FromArgb(alphaValue, XColor.FromArgb(1, 0, 0, 0));
                XSolidBrush brush = new(textColor);

                gfx.TranslateTransform((float)centerX, (float)centerY);
                gfx.RotateTransform(45);
                gfx.DrawString("SoftGED", font, brush, (float)(-textSize.Width / 2), (float)(-textSize.Height / 2));
                gfx.Dispose();
            }
        }

        public async Task<MemoryStream?> GeneratePDF(Guid documentId, Document document)
        {
            var file = await CreatePDFCopy(document.Url);

            if (file == null)
            {
                return null;
            }

            PdfDocument pdf = PdfReader.Open(file);

            // var recipientsList = await _userDocumentRepository.GetUsersDocuments(documentId);

            // foreach (var recipient in recipientsList)
            // {
            //     if (recipient.ProcessingDate != null)
            //     {
            //         BuildField(pdf, recipient);
            //     }
            // }

            if (document.Status != DocumentStatus.Archived)
            {
                AddWatermark(pdf);
            }

            var memoryStream = new MemoryStream();

            pdf.Save(memoryStream);

            pdf.Close();

            return memoryStream;
        }

        public async Task<MemoryStream?> GeneratePDF(Guid documentId, Document document, Guid currentUserId)
        {
            var file = await CreatePDFCopy(document.Url);

            if (file == null)
            {
                return null;
            }

            PdfDocument pdf = PdfReader.Open(file);

            // var recipientsList = await _userDocumentRepository.GetUsersDocuments(documentId);

            // foreach (var recipient in recipientsList)
            // {
            //     if (recipient.ProcessingDate == null && currentUserId == recipient.UserId && recipient.IsTheCurrentStepTurn)
            //     {
            //         CreateBox(pdf, recipient);
            //     }
            //     else
            //     {
            //         BuildField(pdf, recipient);
            //     }
            // }

            if (document.Status != DocumentStatus.Archived)
            {
                AddWatermark(pdf);
            }

            var memoryStream = new MemoryStream();

            pdf.Save(memoryStream);

            pdf.Close();

            return memoryStream;
        }
    }
}
