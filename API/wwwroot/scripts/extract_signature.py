import cv2
from PIL import Image
import click


def extract_signature(image_path, output_path, min_size=10):
    img = cv2.imread(image_path, cv2.IMREAD_GRAYSCALE)

    height, width = img.shape

    if height < min_size or width < min_size:
        exit(1)

    _, img_bin = cv2.threshold(img, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)

    kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (3, 3))
    img_bin = cv2.morphologyEx(img_bin, cv2.MORPH_CLOSE, kernel)

    img_pil = Image.fromarray(img_bin)
    img_pil = img_pil.convert("RGBA")

    pixdata = img_pil.load()

    print_dpi = 72
    print_size = 3.46

    new_size = int(print_dpi * print_size)

    for y in range(img_pil.size[1]):
        for x in range(img_pil.size[0]):
            if pixdata[x, y] == (255, 255, 255, 255):
                pixdata[x, y] = (255, 255, 255, 0)

    img_pil = img_pil.resize((new_size, new_size))

    img_pil.save(output_path, "PNG", dpi=(print_dpi, print_dpi))

    img_pil = Image.open(output_path)


@click.command()
@click.version_option("0.1.0", prog_name="Signature extractor")
@click.option('-i', '--input', type=click.STRING, required=True, help='The image input file path')
@click.option('-o', '--output', type=click.STRING, required=True, help='The output path')
def cli(input, output):
    extract_signature(input, output)


if __name__ == '__main__':
    cli()
