package org.example;

import java.util.Arrays;

public class SpriteTexture {
    public final int width;
    public final int height;
    private final int[] pixels;

    public SpriteTexture(int width, int height) {
        this.width = width;
        this.height = height;
        this.pixels = new int[width * height];
        Arrays.fill(this.pixels, -1);
    }

    public void setPixel(int x, int y, int colorIndex) {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        pixels[y * width + x] = colorIndex;
    }

    public int getPixel(int x, int y) {
        if (x < 0 || x >= width || y < 0 || y >= height) return -1;
        return pixels[y * width + x];
    }

    // TEST FUNCTIONS

    public static SpriteTexture solidColor(int colorIndex) {
        SpriteTexture tex = new SpriteTexture(16, 16);
        Arrays.fill(tex.pixels, colorIndex);
        return tex;
    }

    // X shape for transparency
    public static SpriteTexture testPattern(int colorIndex) {
        SpriteTexture tex = new SpriteTexture(16, 16);
        for (int i = 0; i < 16; i++) {
            tex.setPixel(i, i, colorIndex);
            tex.setPixel(15-i, i, colorIndex);
        }
        //border
        for (int i = 0; i < 16; i++) {
            tex.setPixel(i, 0, colorIndex);
            tex.setPixel(i, 15, colorIndex);
            tex.setPixel(0, i, colorIndex);
            tex.setPixel(15, i, colorIndex);
        }
        return tex;
    }

    public static SpriteTexture cornerFish() {
        return SpriteLoader.load("corner_fish");
    }
    public static SpriteTexture scalyEye() {
        return SpriteLoader.load("scaly_eye");
    }
}
