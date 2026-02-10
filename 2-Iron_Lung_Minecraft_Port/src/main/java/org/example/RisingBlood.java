package org.example;

import net.minestom.server.coordinate.Vec;
import net.minestom.server.entity.Entity;
import net.minestom.server.entity.EntityType;
import net.minestom.server.entity.metadata.display.BlockDisplayMeta;
import net.minestom.server.event.instance.InstanceTickEvent;
import net.minestom.server.instance.block.Block;

public class RisingBlood {

    private final double width = 8;
    private final double startHeight = 1.01;
    private final double endHeight = 1.85;
    private final Entity display;
    private final double timeToRise = 90;
    private double risingTime = 0;

    public RisingBlood() {
        display = new Entity(EntityType.BLOCK_DISPLAY);
        BlockDisplayMeta meta = (BlockDisplayMeta) display.getEntityMeta();

        meta.setBlockState(Block.REDSTONE_BLOCK);
        meta.setScale(new Vec(width, 0.01, width));
        meta.setHasNoGravity(true);
        meta.setBrightness(1, 0);

        display.setInstance(Cockpit.Instance.getInstance(), new Vec(0.5 - width/2, startHeight, 0.5 - width/2));

        Cockpit.Instance.getInstance().eventNode().addListener(InstanceTickEvent.class, event -> {
            double deltaTime = event.getDuration() / 1000.0;
            risingTime += deltaTime;
            double timeRatio = Math.clamp(risingTime / timeToRise, 0.0, 1.0);
            double newHeight = startHeight + (endHeight - startHeight) * timeRatio;
            display.teleport(display.getPosition().withY(newHeight));
        });
    }
}
