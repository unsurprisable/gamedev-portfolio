package org.example;

import net.minestom.server.MinecraftServer;
import net.minestom.server.entity.metadata.display.BlockDisplayMeta;
import net.minestom.server.instance.block.Block;
import net.minestom.server.timer.TaskSchedule;

import java.util.Random;

public class EmergencyLight {
    public EmergencyLight() {
        Random rand = new Random();
        MinecraftServer.getSchedulerManager().buildTask(() -> {
            int level = 4;
            if (rand.nextFloat() > 0.75) {
                level = rand.nextInt(5, 7);
            }

            Block light = Block.LIGHT.withProperty("level", String.valueOf(level));
            Cockpit.Instance.getInstance().setBlock(0, 2, -3, light);
            ((BlockDisplayMeta)Cockpit.Instance.lightEntity.getEntityMeta()).setBrightness(level-2, 0);
        }).repeat(TaskSchedule.tick(2)).schedule();
    }
}
