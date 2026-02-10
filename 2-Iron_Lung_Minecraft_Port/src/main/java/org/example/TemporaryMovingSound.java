package org.example;

import net.kyori.adventure.sound.Sound;
import net.minestom.server.coordinate.Pos;
import net.minestom.server.coordinate.Vec;
import net.minestom.server.instance.InstanceContainer;

public class TemporaryMovingSound extends MovingSound {

    private final int soundLengthMillis;
    private int timePassedMillis = 0;
    private boolean markedForDeletion = false;

    public TemporaryMovingSound(InstanceContainer instance, Sound sound, double soundLength, double angle, double distance) {
        this.soundLengthMillis = (int)(soundLength * 1000);
        double radians =  Math.toRadians(angle + 90);
        double dx = -Math.cos(radians) * distance;
        double dz = Math.sin(radians) * distance;
        Vec offset = new Vec(dx, dz);
        Pos worldOrigin = Submarine.Instance.getWorldPosition().add(offset);
        super(instance, sound, Float.MAX_VALUE, worldOrigin, Float.MAX_VALUE);
    }

    @Override
    public void update(int deltaTimeMillis) {
        super.update(deltaTimeMillis);
        timePassedMillis += deltaTimeMillis;
        if (timePassedMillis >= soundLengthMillis) {
            markedForDeletion = true;
        }
    }

    public boolean getIsMarkedForDeletion() {
        return markedForDeletion;
    }

    public void delete() {
        super.sound.stop();
        super.emitter.remove();
    }

}
