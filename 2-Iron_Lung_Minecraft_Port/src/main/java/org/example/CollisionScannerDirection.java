package org.example;

import net.kyori.adventure.text.Component;
import net.minestom.server.MinecraftServer;
import net.minestom.server.coordinate.Point;
import net.minestom.server.coordinate.Vec;
import net.minestom.server.entity.Entity;
import net.minestom.server.entity.EntityType;
import net.minestom.server.entity.metadata.display.AbstractDisplayMeta;
import net.minestom.server.entity.metadata.display.TextDisplayMeta;
import net.minestom.server.instance.InstanceContainer;
import net.minestom.server.timer.TaskSchedule;

public class CollisionScannerDirection {

    private final Entity entity;
    private final TextDisplayMeta entityMeta;

    private int maxBeepTimeTicks = 30;
    private int minBeepTimeTicks = 4;
    private int beepTimeTicksLeft = 0;
    private int targetBeepTimeTicks = 0;

    private boolean isDetecting = false;
    private double distance = 0;

    private boolean isForceDetecting = false;

    public CollisionScannerDirection() {
        this.entity = new Entity(EntityType.TEXT_DISPLAY);
        this.entityMeta = (TextDisplayMeta) entity.getEntityMeta();

        entityMeta.setText(Component.text("⬤").color(Cockpit.GLOWING_COLOR));
        entityMeta.setBillboardRenderConstraints(AbstractDisplayMeta.BillboardConstraints.FIXED);
        entityMeta.setBrightness(0, 0);
        entityMeta.setScale(new Vec(0.25));
        entityMeta.setHasNoGravity(true);
        entityMeta.setBackgroundColor(0);

        MinecraftServer.getSchedulerManager().buildTask(this::tickUpdate).repeat(TaskSchedule.nextTick()).schedule();
    }

    private void tickUpdate() {
        if (!isDetecting) return;

        if (beepTimeTicksLeft <= 0) {
            SoundManager.play(SoundManager.PROXIMITY, new Vec(0.5, 2.5, -3.5));
            entityMeta.setBrightness(Cockpit.GLOWING_BRIGHTNESS, 0);
            if (!isForceDetecting) {
                targetBeepTimeTicks = calculateTargetBeepTime(this.distance);
            }
            beepTimeTicksLeft = targetBeepTimeTicks;
        } else if (beepTimeTicksLeft <= targetBeepTimeTicks/2) {
            entityMeta.setBrightness(0, 0);
        }

        beepTimeTicksLeft--;
    }

    public void objectDetected(double distance) {
        isDetecting = true;
        this.distance = distance;
    }

    public void noObjectDetected() {
        isDetecting = false;
        beepTimeTicksLeft = 0;
        entityMeta.setBrightness(0, 0);
    }

    private int calculateTargetBeepTime(double distance) {
        double distRatio = (distance) / CollisionScanner.MAX_DETECTION_DISTANCE;
        return (int)(minBeepTimeTicks + distRatio * (maxBeepTimeTicks - minBeepTimeTicks));
    };

    public void setInstance(InstanceContainer instance, Point origin) {
        entity.setInstance(instance, origin);
    }

    public void setForceDetecting() {
        noObjectDetected();
        targetBeepTimeTicks = 2;
        beepTimeTicksLeft = 0;
        isForceDetecting = true;
        isDetecting = true;
    }
    public void stopForceDetecting() {
        isForceDetecting = false;
    }

    public boolean getIsForceDetecting() {
        return isForceDetecting;
    }
}
