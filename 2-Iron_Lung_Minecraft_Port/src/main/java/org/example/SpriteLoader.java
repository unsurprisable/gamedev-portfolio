package org.example;

import javax.imageio.ImageIO;
import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;

public class SpriteLoader {

    public static SpriteTexture load(String filename) {
        File file = new File("sprites/" + filename + ".png");

        if (!file.exists()) {
            System.err.println("Sprite File not found: " + file.getAbsolutePath());
            return SpriteTexture.testPattern(0);
        }

        try {
            // load image
            BufferedImage image = ImageIO.read(file);
            int width = image.getWidth();
            int height = image.getHeight();

            System.out.println("Loading sprite: " + filename + " (" + width + "x" + height + ")");

            SpriteTexture texture = new SpriteTexture(width, height);

            for (int y = 0; y < height; y++) {
                for (int x  = 0; x < width; x++) {
                    int argb = image.getRGB(x, y);
                    int alpha = (argb >> 24) & 0xFF;

                    // alpha cutoff to make pixel invisible in-game
                    if (alpha < 50) {
                        texture.setPixel(x, y, -1);
                        continue;
                    }

                    int red = (argb >> 16) & 0xFF;
                    int green = (argb >> 8) & 0xFF;
                    int blue = (argb) & 0xFF;

                    // Luma Brightness formula for typical human color perception
                    double brightness = (0.299 * red + 0.587 * green + 0.114 * blue);

                    int paletteIndex = SubmarineCamera.ColorPalette.getClosestPaletteIndex(brightness);

                    texture.setPixel(x, y, paletteIndex);
                }
            }

            return texture;

        } catch (IOException e) {
            e.printStackTrace();
            return SpriteTexture.testPattern(0);
        }
    }
}
