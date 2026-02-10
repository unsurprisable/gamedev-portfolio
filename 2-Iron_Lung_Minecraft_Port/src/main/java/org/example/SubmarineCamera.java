package org.example;

import net.minestom.server.MinecraftServer;
import net.minestom.server.coordinate.Pos;
import net.minestom.server.coordinate.Vec;
import net.minestom.server.instance.InstanceContainer;
import net.minestom.server.instance.block.Block;
import net.minestom.server.network.packet.server.play.MapDataPacket;
import net.minestom.server.timer.Task;
import net.minestom.server.timer.TaskSchedule;

import java.util.Arrays;
import java.util.List;
import java.util.Random;

import static org.example.SubmarineCamera.ColorPalette.PALETTE_COLORS;

public class SubmarineCamera {

    private final InstanceContainer oceanInstance;

    private boolean isCameraActive = false;
    public Task activePrintingTask = null;

    public SubmarineCamera(InstanceContainer oceanInstance) {
        this.oceanInstance = oceanInstance;
    }

    public static class ColorPalette {
        public static final byte[] PALETTE_COLORS = {
            (byte) 33,  // White (255)      @ 100% (255)  0 // EDITED: used to be (34) but removed since true white stands out too much
            (byte) 33,  // White (255)      @ 86%  (219)  1
            (byte) 32,  // White (255)      @ 71%  (181)  2
            (byte) 26,  // Lt Gray (167)    @ 100% (167)  3
            (byte) 90,  // W.Terra (153)    @ 100% (153)  4
            (byte) 25,  // Lt Gray (167)    @ 86%  (144)  5
            (byte) 35,  // White (255)      @ 53%  (135)  6
            (byte) 89,  // W.Terra (153)    @ 86%  (131)  7
            (byte) 24,  // Lt Gray (167)    @ 71%  (119)  8
            (byte) 46,  // Gray (112)       @ 100% (112)  9
            (byte) 88,  // W.Terra (153)    @ 71%  (108)  10
            (byte) 238, // Deepslate (100)  @ 100% (100)  11
            (byte) 45,  // Gray (112)       @ 86%  (96)   12
            (byte) 27,  // Lt Gray (167)    @ 53%  (89)   13
            (byte) 237, // Deepslate (100)  @ 86%  (86)   14
            (byte) 91,  // W.Terra (153)    @ 53%  (81)   15
            (byte) 44,  // Gray (112)       @ 71%  (80)   16
            (byte) 86,  // G.Terra (76)     @ 100% (76)   17
            (byte) 236, // Deepslate (100)  @ 71%  (71)   18
            (byte) 85,  // G.Terra (76)     @ 86%  (65)   19
            (byte) 47,  // Gray (112)       @ 53%  (59)   20
            (byte) 84,  // G.Terra (76)     @ 71%  (54)   21
            (byte) 239, // Deepslate (100)  @ 53%  (53)   22
            (byte) 87,  // G.Terra (76)     @ 53%  (40)   23
            (byte) 118, // Black (25)       @ 100% (25)   24
            (byte) 117, // Black (25)       @ 86%  (21)   25
            (byte) 116, // Black (25)       @ 71%  (17)   26
            (byte) 119  // Black (25)       @ 53%  (13)   27
        };

        private static final int[] PALETTE_BRIGHTNESS = {
            255,
            219,
            181,
            167, 153, 144, 135, 131,
            119, 112, 108, 100, 96,
            89, 86, 81, 80, 76, 71, 65, 59, 54, 53, 40,
            25, 21, 17, 13
        };

        /**
         * Determines the best-match map color ID based on a given brightness value.
         * @param targetBrightness double 0-255
         */
        public static int getClosestPaletteIndex(double targetBrightness) {
            int bestIndex = 0;
            double minDifference = Double.MAX_VALUE;

            for (int i = 0; i < PALETTE_BRIGHTNESS.length; i++) {
                double diff = Math.abs(targetBrightness - PALETTE_BRIGHTNESS[i]);

                if (diff < minDifference) {
                    minDifference = diff;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

    }

    private final Random rand = new Random();

    public byte getFuzzyColor(double baseIndex, double fuzziness) {
        double noisyIndex = baseIndex + (rand.nextGaussian() * fuzziness);
        int finalIndex = (int) Math.round(noisyIndex);
        finalIndex = Math.clamp(finalIndex, 0, PALETTE_COLORS.length-1);

        return PALETTE_COLORS[finalIndex];
    }

    private byte getWallFuzzyColor(double distance) {
        double distRatio = (distance) / MAX_WALL_DISTANCE;
        distRatio = Math.clamp(distRatio, 0.0, 1.0);

        double fogCurve = .45;
        distRatio = Math.pow(distRatio, fogCurve);

        int minIndex = 20;
        int maxIndex = PALETTE_COLORS.length - 1;
        double targetIndex = distRatio * (maxIndex - minIndex) + minIndex;

        // widens the curve of the Gaussian to allow multiple shades to be picked
        double fuzziness = 2 + ((1-distRatio) * 1.75);

        return getFuzzyColor(targetIndex, fuzziness);
    }

    private byte getSpriteFuzzyColor(int baseIndex, double distance) {
        double blockBuffer = 1; // how far away for the fog to start affecting the color
        double maxFogDistance = MAX_SPRITE_DISTANCE - blockBuffer;

        double adjustedDist = Math.clamp(distance - blockBuffer, 0, maxFogDistance);
        double distRatio = adjustedDist / maxFogDistance;

        double fogCurve = 3;
        distRatio = Math.pow(distRatio, fogCurve);

        int maxIndex = PALETTE_COLORS.length - 1;
        double targetIndex = baseIndex + (distRatio * (maxIndex - baseIndex));

        // widens the curve of the Gaussian to allow multiple shades to be picked
        double fuzziness = 1.5 + distRatio * 2;

        return getFuzzyColor(targetIndex, fuzziness);
    }

    public static final double MAX_WALL_DISTANCE = 4.5;
    public static final double MAX_SPRITE_DISTANCE = 4;
    public final long PHOTO_GENERATE_TIME = 2500;

    public void takePhoto(Pos cameraPos) {
        disableAndClearCameraMap();

        byte[] map_pixels = new byte[128 * 128];

        double[] zBuffer = new double[128];
        Arrays.fill(zBuffer, Double.MAX_VALUE);

        long photoGenerateStartTime = System.currentTimeMillis();

        /* ----- FIRST RENDERING PASS -----
                  Floor & Background
        */
        final double GROUND_ELLIPSE_CX = 64;
        final double GROUND_ELLIPSE_CY = 92;

        final double GROUND_ELLIPSE_W = 70;
        final double GROUND_ELLIPSE_H = 42;

        final double GROUND_BASE_INDEX = 24.6;
        final double GROUND_FUZZINESS = 2;
        final double VOID_BASE_INDEX = 27;
        final double VOID_FUZZINESS = 2.25;

        int ellipseTop = (int)(GROUND_ELLIPSE_CY - GROUND_ELLIPSE_H /2);
        int ellipseBottom = (int)(GROUND_ELLIPSE_CY + GROUND_ELLIPSE_H /2) + 1;

        for (int x = 0; x < 128; x++) {
            for (int y = 0; y < 128; y++) {
                int index = x + y*128;

                // ground ellipse
                double ground_dx = x - GROUND_ELLIPSE_CX;
                double ground_dy = y - GROUND_ELLIPSE_CY;
                double ellipseValue = (ground_dx * ground_dx) / (GROUND_ELLIPSE_W * GROUND_ELLIPSE_W)
                    + (ground_dy * ground_dy) / (GROUND_ELLIPSE_H * GROUND_ELLIPSE_H);

                // inside the ground ellipse
                if (ellipseValue <= 1.0) {
                    // smooth the edges into the void
                    double smoothedGroundIndex = GROUND_BASE_INDEX + ellipseValue * (VOID_BASE_INDEX - GROUND_BASE_INDEX);

                    double verticalRange = ellipseBottom - ellipseTop;
                    double brightenFactor = 2;
                    double closerBrightness = brightenFactor * Math.pow((1 - (ellipseBottom - y) / verticalRange), 3);
                    closerBrightness = Math.clamp(closerBrightness, 0.0, brightenFactor);

                    double finalGroundIndex = smoothedGroundIndex - closerBrightness;

                    map_pixels[index] = getFuzzyColor(finalGroundIndex, GROUND_FUZZINESS);
                } else {
                    map_pixels[index] = getFuzzyColor(VOID_BASE_INDEX, VOID_FUZZINESS);
                }
            }
        }

        /* ----- SECOND RENDERING PASS -----
                       Cave  Walls
        */
        double fov = 80;
        double startAngle = -fov / 2;
        double angleStep = fov / 128; // degrees per pixel column

        for (int x = 0; x < 128; x++) {
            double relativeAngle = startAngle + angleStep * x;

            Pos rayPos = cameraPos.withYaw((float)(cameraPos.yaw() + relativeAngle));

            double rawDistance = scanForBlocks(rayPos, MAX_WALL_DISTANCE, 1000);
            if (rawDistance == -1) continue; //nothing

            zBuffer[x] = rawDistance;

            // fisheye curve correction
            double correctedDist = rawDistance * Math.cos(Math.toRadians(relativeAngle));

            final double wallHeightScale = 80;

            int wallHeight = (int) (wallHeightScale / correctedDist);
            wallHeight = Math.max(0, Math.min(128, wallHeight));

            int ceiling = 64 - (wallHeight / 2);
            int floor = 64 + (wallHeight / 2);

            for (int y = 0; y <= floor; y++) {
                if (y >= 128) continue;
                int index = x + (y * 128);

                if (y >= ceiling) {
                    map_pixels[index] = getWallFuzzyColor(correctedDist);
                    continue;
                }

                // simulate wall extending up while fading out of view
                double ceilingRatio = (ceiling > 0) ? (double)(ceiling - y) / ceiling : 1.0;

                double fakeDist = correctedDist + ((MAX_WALL_DISTANCE - correctedDist) * ceilingRatio);

                map_pixels[index]  = getWallFuzzyColor(fakeDist);
            }
        }

        /* ----- THIRD RENDERING PASS -----
                        Sprites
        */
        renderSprites(map_pixels, zBuffer, cameraPos);

        /* ----- FOURTH RENDERING PASS -----
                 Screen-Space  Sprites
        */
        if (ProgressionManager.Instance.shouldPrintCornerFish) {
            ProgressionManager.Instance.shouldPrintCornerFish = false;
            SpriteTexture texture = SpriteTexture.cornerFish();
            drawScreenSprite(map_pixels, texture, 127-texture.width+3, 0); // magic number to tweak image position ik im lazy
        }
        if (ProgressionManager.Instance.shouldPrintScalyEye) {
            ProgressionManager.Instance.shouldPrintScalyEye = false;
            SpriteTexture texture = SpriteTexture.scalyEye();
            drawScreenSprite(map_pixels, texture, 0, 0);
        }

        /* ----- SENDING PACKET ----- */

        long timeUntilPhotoPrinted = PHOTO_GENERATE_TIME - (System.currentTimeMillis() - photoGenerateStartTime);
        if (timeUntilPhotoPrinted < 0) {
            pushCameraMapUpdatePacket(map_pixels);
            return;
        }

        // delay the packet until sound effect is done
        activePrintingTask = MinecraftServer.getSchedulerManager().scheduleTask(() -> {
            pushCameraMapUpdatePacket(map_pixels);
            isCameraActive = true;
            activePrintingTask = null;
        }, TaskSchedule.millis(timeUntilPhotoPrinted), TaskSchedule.stop());
    }

    public double scanForBlocks(Pos origin, double maxDistance, int steps) {
        final double stepSize = maxDistance / steps;

        Vec direction = origin.direction();

        for (double dist = 0; dist < maxDistance; dist += stepSize) {
            Pos checkPos = origin.add(direction.mul(dist));

            Block block = oceanInstance.getBlock(checkPos);

            if (block.compare(Block.COBBLED_DEEPSLATE)) {
                return dist;
            }
        }
        return -1;
    }

    private void renderSprites(byte[] pixels, double[] zBuffer, Pos cameraPos) {
        SpriteObjectScanner.activeSprites.sort((s1, s2) -> {
            double d1 = s1.position().distance(cameraPos);
            double d2 = s2.position().distance(cameraPos);
            return Double.compare(d2, d1); // sort sprites FAR to NEAR for proper transparency rendering
        });

        double halfWidth = 64;
        double halfHeight = 64;
        double focalLength = 76.3; // (approx. 80 fov)

        for (SpriteObject sprite : SpriteObjectScanner.activeSprites) {

            // --- TRANSFORMATION (World Space -> Camera Space) ---
            double relX = sprite.position().x() - cameraPos.x();
            double relZ = sprite.position().z() - cameraPos.z();

            double yawRad = Math.toRadians(cameraPos.yaw());
            double cos = Math.cos(-yawRad);
            double sin = Math.sin(-yawRad);

            double rotX = relX * cos - relZ * sin;
            double rotZ = relX * sin + relZ * cos; // distance from camera

            // clip if behind, too close, or too far
            if (rotZ < 0.4 || rotZ > MAX_WALL_DISTANCE) continue;

            double transformX = (-rotX / rotZ) * focalLength + halfWidth;

            double wallScale = 80.0;
            int baseScale = (int) Math.abs(wallScale / rotZ);

            int drawSize = (int) (baseScale * sprite.size());

            // verticalOffset = 0.0 -> centered on horizon
            // verticalOffset = 1.0 -> pushed down (floor)
            // verticalOffset = -1.0 -> pushed up (ceiling)
            int transformY = (int) (halfHeight + (sprite.verticalOffset() * (baseScale / 2.0)));

            // draw bounds
            int drawStartX = (int) (transformX - drawSize / 2.0);
            int drawEndX = drawStartX + drawSize;
            int drawStartY = (int) (transformY - drawSize / 2.0);
            int drawEndY = drawStartY + drawSize;

            // clip to screen
            if (drawStartX < 0) drawStartX = 0;
            if (drawEndX >= 128)  drawEndX = 128;
            if (drawStartY < 0) drawStartY = 0;
            if (drawEndY >= 128)  drawEndY = 128;

            // PIXEL LOOP
            for (int stripe = drawStartX; stripe < drawEndX; stripe++) {
                // check wall occlusion
                if (zBuffer[stripe] < rotZ - 0.5) continue;

                // texture mapping
                int texX = (int) Math.round((stripe - (transformX - drawSize / 2.0)) * (sprite.texture().width / (double) drawSize));

                if (texX < 0 || texX >= sprite.texture().width) continue;

                for (int y = drawStartY; y < drawEndY; y++) {
                    int texY = (int) Math.round((y - (transformY - drawSize / 2.0)) * (sprite.texture().height / (double) drawSize));

                    if (texY < 0 || texY >= sprite.texture().height) continue;

                    int colorIndex = sprite.texture().getPixel(texX, texY);

                    if (colorIndex != -1) {
                        byte finalPixel = getSpriteFuzzyColor(colorIndex, rotZ);
                        int screenIndex = stripe + (y * 128);
                        pixels[screenIndex] = finalPixel;
                    }
                }
            }
        }
    }

    private void drawScreenSprite(byte[] pixels, SpriteTexture sprite, int x, int y) {
        for (int spriteY = 0; spriteY < sprite.height; spriteY++) {
            for (int spriteX = 0; spriteX < sprite.width; spriteX++) {

                int screenX = x + spriteX;
                int screenY = y + spriteY;

                if (screenX < 0 || screenX >= 128 || screenY < 0 || screenY >= 128) continue;

                int colorIndex = sprite.getPixel(spriteX, spriteY);

                if (colorIndex != -1) {
                    int screenIndex = screenX + (screenY * 128);
                    pixels[screenIndex] = getFuzzyColor(colorIndex, 0.25);
                }
            }
        }
    }

    public void disableAndClearCameraMap() {
        isCameraActive = false;
        byte[] map_array = new byte[128 * 128];
        Arrays.fill(map_array, PALETTE_COLORS[27]); // black
        pushCameraMapUpdatePacket(map_array);
    }

    private void pushCameraMapUpdatePacket(byte[] array) {
        MapDataPacket mapDataPacket = new MapDataPacket(0, (byte) 4, true, false, List.of(),
            new MapDataPacket.ColorContent((byte)128, (byte)128, (byte)0, (byte)0, array));
        Main.player.sendPacket(mapDataPacket);
    }


    public void clearPrintingTask() {
        if (activePrintingTask != null) {
            activePrintingTask.cancel();
            activePrintingTask = null;
        }
    }


    public boolean getIsCameraActive() {
        return isCameraActive;
    }
}
