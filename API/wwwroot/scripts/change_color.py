from PIL import Image
import click


def hex_to_rgb(hex_color):
    tmp  = hex_color.lstrip('#')

    return tuple(int(tmp[i:i+2], 16) for i in (0, 2, 4))


def replace_black_with_color(image_path, save_path, target_color):
    image = Image.open(image_path)

    if image.mode not in ('RGB', 'RGBA'):
        raise ValueError("Image mode should be RGB or RGBA")

    pixels = image.load()

    for y in range(image.height):
        for x in range(image.width):
            current_color = pixels[x, y]

            if image.mode == 'RGB' and current_color == (0, 0, 0):
                pixels[x, y] = target_color
            elif image.mode == 'RGBA' and current_color[:3] == (0, 0, 0):
                pixels[x, y] = (*target_color, current_color[3])

    image.save(save_path)


@click.command()
@click.version_option("0.1.0", prog_name="Black color changer")
@click.option('--input', type=click.STRING, required=True, help='The image path')
@click.option('-hc', '--hex-color', type=click.STRING, required=True, help='The target hexadecimal color')
@click.option('-o', '--output', type=click.STRING, required=True, help='The output path')
def cli(input, hex_color, output):
    replace_black_with_color(input, output, hex_to_rgb(hex_color))


if __name__ == '__main__':
    cli()
