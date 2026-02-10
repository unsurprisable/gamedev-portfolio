package org.example;

import net.kyori.adventure.sound.Sound;
import net.minestom.server.coordinate.Point;
import net.minestom.server.coordinate.Pos;
import net.minestom.server.entity.Entity;
import net.minestom.server.entity.EntityType;
import net.minestom.server.entity.metadata.other.ArmorStandMeta;
import net.minestom.server.instance.InstanceContainer;

public class MovingSound {
    protected final LoopingSound sound;
    private final Point worldOrigin;
    private final double maxDistance;

    private final double MAX_EMITTER_DISTANCE = 16;
    private final double MIN_EMITTER_DISTANCE = 1;
    protected final Entity emitter;
    private Pos playerPos = new Pos(0, 0, 32);

    private boolean isTooFar = false;

    public MovingSound(InstanceContainer instance, Sound sound, double soundLength, Point worldOrigin, double maxDistance) {
        this.sound = new LoopingSound(sound, soundLength, false);
        this.worldOrigin = worldOrigin;
        this.maxDistance = maxDistance;

        this.emitter = new Entity(EntityType.ARMOR_STAND);
        ArmorStandMeta meta = (ArmorStandMeta) emitter.getEntityMeta();
        meta.setInvisible(true);
        meta.setMarker(true);
        meta.setHasNoGravity(true);
//        meta.setHasGlowingEffect(true); // debug
        emitter.setInstance(instance, playerPos);

        this.sound.setEmitter(emitter);
    }

    public void update(int deltaTimeMillis) {
        playerPos = Main.player.getPosition();
        updateEmitterPosition();
        sound.update(deltaTimeMillis);
    }

    private void updateEmitterPosition() {
        double distance = getWorldDistance();

        if (!isTooFar && distance > maxDistance) {
            isTooFar = true;
            sound.stop();
        } else if (distance <= maxDistance) {
            isTooFar = false;
            sound.play();
        }

        if (isTooFar) return;

        double distanceRatio = distance / maxDistance;
        double emitterDistance = MIN_EMITTER_DISTANCE + distanceRatio * (MAX_EMITTER_DISTANCE - MIN_EMITTER_DISTANCE);
        double emitterAngle = -getAngle();

        double radians = Math.toRadians(emitterAngle);
        double offsetX = -Math.sin(radians) * emitterDistance;
        double offsetZ = Math.cos(radians) * emitterDistance;

        Pos finalEmitterPos = playerPos.add(offsetX, 0, offsetZ);
        emitter.teleport(finalEmitterPos);
    }

    private double getWorldDistance() {
        return Submarine.Instance.getWorldPosition().distance(worldOrigin);
    }

    private double getAngle() {
        Pos subPosition = Submarine.Instance.getWorldPosition();
        return subPosition.yaw() - subPosition.withLookAt(worldOrigin).yaw() - 180;
    }
}
