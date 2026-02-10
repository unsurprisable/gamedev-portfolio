package org.example;

import net.minestom.server.coordinate.Pos;
import net.minestom.server.instance.Instance;
import net.minestom.server.instance.block.Block;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Random;


public class SpriteObjectScanner {

    private record SpriteTextureObjectData (SpriteTexture texture, double size, double verticalOffset){}

    public static List<SpriteObject> activeSprites = new ArrayList<>();

    private static final double debrisSize = 2.5;

    private static final HashMap<Integer, SpriteTextureObjectData> objectiveSpriteMap = new HashMap<>();
    private static final SpriteTextureObjectData[] debrisSprites = {
        new SpriteTextureObjectData(SpriteLoader.load("debris_1"), debrisSize, 1),
        new SpriteTextureObjectData(SpriteLoader.load("debris_10"), debrisSize, 1),
        new SpriteTextureObjectData(SpriteLoader.load("debris_2"), debrisSize, 1),
        new SpriteTextureObjectData(SpriteLoader.load("debris_20"), debrisSize, 1),
        new SpriteTextureObjectData(SpriteLoader.load("debris_3"), debrisSize, 1),
        new SpriteTextureObjectData(SpriteLoader.load("debris_30"), debrisSize, 1),
    };

    public static void scanWorld(Instance oceanInstance) {
        // OBJECTIVES
        objectiveSpriteMap.put(Block.WHITE_GLAZED_TERRACOTTA.id(), new SpriteTextureObjectData(
            SpriteLoader.load("objective_1"), 3.0, 0.25));
        objectiveSpriteMap.put(Block.LIGHT_GRAY_GLAZED_TERRACOTTA.id(), new SpriteTextureObjectData(
            SpriteLoader.load("objective_2"), 3.0, 0.25));
        objectiveSpriteMap.put(Block.GRAY_GLAZED_TERRACOTTA.id(), new SpriteTextureObjectData(
            SpriteLoader.load("objective_3"), 3.0, 0.25));
        objectiveSpriteMap.put(Block.BLACK_GLAZED_TERRACOTTA.id(), new SpriteTextureObjectData(
            SpriteLoader.load("objective_4"), 3.0, 0.25));
        objectiveSpriteMap.put(Block.BROWN_GLAZED_TERRACOTTA.id(), new SpriteTextureObjectData(
            SpriteLoader.load("objective_5"), 7.0, 0.25));
        objectiveSpriteMap.put(Block.RED_GLAZED_TERRACOTTA.id(), new SpriteTextureObjectData(
            SpriteLoader.load("objective_6"), 6.0, 0));
        objectiveSpriteMap.put(Block.ORANGE_GLAZED_TERRACOTTA.id(), new SpriteTextureObjectData(
            SpriteLoader.load("objective_7"), 7.0, 0.25));
        objectiveSpriteMap.put(Block.LIME_GLAZED_TERRACOTTA.id(), new SpriteTextureObjectData(
            SpriteLoader.load("objective_8"), 3.0, 0));
        objectiveSpriteMap.put(Block.YELLOW_GLAZED_TERRACOTTA.id(), new SpriteTextureObjectData(
            SpriteLoader.load("objective_9"), 6.0, 0.25));
        objectiveSpriteMap.put(Block.GREEN_GLAZED_TERRACOTTA.id(), new SpriteTextureObjectData(
            SpriteLoader.load("objective_10"), 3.0, 0));

        System.out.println("Scanning world for sprites...");

        Random rand = new Random();

        for (int x = 0; x <= 256; x++) {
            for (int z = 0; z <= 256; z++) {
                Block block = oceanInstance.getBlock(x, 0, z);

                SpriteTextureObjectData data = null;
                if (block.compare(Block.GREEN_WOOL)) {
                    data = debrisSprites[rand.nextInt(debrisSprites.length)];
                } else if (objectiveSpriteMap.containsKey(block.id())) {
                    data =  objectiveSpriteMap.get(block.id());
                }

                if (data == null) continue;

                SpriteObject sprite = new SpriteObject(
                    new Pos(x + 0.5, 0.5, z + 0.5),
                    data.texture, data.size, data.verticalOffset
                );
                activeSprites.add(sprite);
                oceanInstance.setBlock(x, 0, z, Block.AIR);
            }
        }
        System.out.println("Found " + activeSprites.size() + " sprites.");
    }
}
