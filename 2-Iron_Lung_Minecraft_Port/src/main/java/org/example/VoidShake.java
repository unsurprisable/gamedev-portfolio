package org.example;

import net.minestom.server.MinecraftServer;
import net.minestom.server.coordinate.Vec;
import net.minestom.server.timer.TaskSchedule;

public class VoidShake {

    private final float startingFOV = 0.1f;
    private final float maxRange = 0.008f;
    private final double maxDistance = 16;
    private final Vec worldPos = new Vec(160.5, 0, 32.5);
    private final int tickFrequency = 10;

    public VoidShake() {
        MinecraftServer.getSchedulerManager().buildTask(() -> {
           double distance = Submarine.Instance.getWorldPosition().distance(worldPos);

           if (distance > maxDistance) return;


           double distanceRatio = 1 - (distance / maxDistance);
           float shakeRange = (float)(distanceRatio * maxRange);
           new Screenshake(tickFrequency, startingFOV, startingFOV - shakeRange/2, startingFOV + shakeRange/2);
        }).repeat(TaskSchedule.tick(tickFrequency)).schedule();
    }
}
