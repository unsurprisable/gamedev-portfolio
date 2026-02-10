package org.example;

import net.kyori.adventure.key.Key;
import net.kyori.adventure.sound.Sound;
import net.minestom.server.coordinate.Pos;
import net.minestom.server.entity.Entity;
import net.minestom.server.entity.EntityType;
import net.minestom.server.entity.metadata.other.ArmorStandMeta;
import net.minestom.server.instance.InstanceContainer;

import java.util.ArrayList;

public class SubmarineMoveSFX {

    private final double soundLength = 26.0;
    private double soundLengthLeft = 0;

    private final ArrayList<Entity> emitterEntities;
    private final Sound sound;

    private final double MAX_SPEED = Submarine.Instance.getMaxSpeed();
    private final double MIN_DISTANCE = 6;
    private final double MAX_DISTANCE = 16;
    private final float VOLUME = 0.8f;
    private final int SOUND_DIRECTIONS = 4;

    private Pos playerPos = new Pos(0, 0, 32);

    public SubmarineMoveSFX(InstanceContainer instance) {
        sound = Sound.sound(
            Key.key("custom:submarine_move"),
            Sound.Source.MASTER,
            VOLUME/SOUND_DIRECTIONS, 1.0f
        );

        emitterEntities = new ArrayList<>();
        emitterEntities.add(new Entity(EntityType.ARMOR_STAND));
        emitterEntities.add(new Entity(EntityType.ARMOR_STAND));
        emitterEntities.add(new Entity(EntityType.ARMOR_STAND));
        emitterEntities.add(new Entity(EntityType.ARMOR_STAND));

        for (Entity e : emitterEntities) {
            ArmorStandMeta meta = (ArmorStandMeta) e.getEntityMeta();
            meta.setHasNoGravity(true);
            meta.setMarker(true);
            meta.setInvisible(true);
            e.setInstance(instance, playerPos);
        }

        setVolumeFromSpeed(0);
    }

    public void update(int deltaTimeMillis) {
        double deltaTime = deltaTimeMillis / 1000.0;
        soundLengthLeft -= deltaTime;
        if (soundLengthLeft <= 0) {
            playSound();
            soundLengthLeft = soundLength;
        }

        playerPos = Main.player.getPosition();
        setVolumeFromSpeed(Submarine.Instance.getTotalSpeed());
    }

    private void playSound() {
        for (Entity e : emitterEntities) {
            SoundManager.play(sound, e);
        }
    }

    public void setVolumeFromSpeed(double speed) {
        double conversion = (MAX_DISTANCE - MIN_DISTANCE) / MAX_SPEED;
        double distance = (MAX_DISTANCE) - (speed * conversion);

        emitterEntities.get(0).teleport(playerPos.add(0, 0,  -distance));
        emitterEntities.get(1).teleport(playerPos.add(0, 0,  distance));
        emitterEntities.get(2).teleport(playerPos.add(-distance, 0,  0));
        emitterEntities.get(3).teleport(playerPos.add(distance, 0,  0));
    }
}
