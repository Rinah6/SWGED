import click
from reportlab.pdfgen import canvas
from PyPDF2 import PdfReader, PdfWriter
from io import BytesIO


def sign_document(id, input_pdf_path, output_pdf_path, image_path, x, y, page_index):
    reader = PdfReader(input_pdf_path)
    writer = PdfWriter()

    for page_num in range(len(reader.pages)):
        packet = BytesIO()
        can = canvas.Canvas(packet)

        if page_num == page_index:
            current_page = reader.pages[page_num].mediabox

            can.setFillColorRGB(255/255, 0/255, 0/255)
            can.setFont("Helvetica", 8)
            can.drawString(text='SoftGED ID: ' + id, x=x * 36 / 96, y=float(current_page.height) - (y + 249 / 4) * 72 / 96)

            can.drawImage(image=image_path, x=x * 36 / 96, y=float(current_page.height) - (y + 249) * 72 / 96, width=249 * 72 / 96, height=249 * 72 / 96, mask='auto')
            
        can.save()

        packet.seek(0)
        
        new_pdf = PdfReader(packet)

        page = reader.pages[page_num]

        if page_num == page_index:
            page.merge_page(new_pdf.pages[0])

        writer.add_page(page)

    with open(output_pdf_path, "wb") as output_file:
        writer.write(output_file)


@click.command()
@click.version_option("0.1.0", prog_name="Signature drawer")
@click.option('--id', type=click.STRING, required=True, help='The digital signature ID')
@click.option('--input-pdf', type=click.STRING, required=True, help='The input PDF path')
@click.option('--input-img', type=click.STRING, required=True, help='The image input path')
@click.option('--page-index', type=click.INT, required=True, help='The page index to sign of the target document')
@click.option('--x', type=click.FLOAT, required=True, help='The x position of the image (px)')
@click.option('--y', type=click.FLOAT, required=True, help='The y position of the image (px)')
@click.option('-o', '--output', type=click.STRING, required=True, help='The output PDF path')
def cli(id, input_pdf, input_img, page_index, x, y, output):
    sign_document(id, input_pdf, output, input_img, x, y, page_index)


if __name__ == '__main__':
    cli()
