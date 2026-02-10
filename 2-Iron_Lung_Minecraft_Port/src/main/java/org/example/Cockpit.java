package org.example;

import net.kyori.adventure.key.Key;
import net.kyori.adventure.sound.Sound;
import net.kyori.adventure.text.Component;
import net.kyori.adventure.text.format.NamedTextColor;
import net.kyori.adventure.text.format.TextColor;
import net.kyori.adventure.text.format.TextDecoration;
import net.minestom.server.MinecraftServer;
import net.minestom.server.component.DataComponents;
import net.minestom.server.coordinate.Point;
import net.minestom.server.coordinate.Pos;
import net.minestom.server.coordinate.Vec;
import net.minestom.server.entity.Entity;
import net.minestom.server.entity.EntityType;
import net.minestom.server.entity.metadata.display.AbstractDisplayMeta;
import net.minestom.server.entity.metadata.display.BlockDisplayMeta;
import net.minestom.server.entity.metadata.display.TextDisplayMeta;
import net.minestom.server.entity.metadata.other.GlowItemFrameMeta;
import net.minestom.server.entity.metadata.other.InteractionMeta;
import net.minestom.server.event.instance.InstanceTickEvent;
import net.minestom.server.event.player.PlayerEntityInteractEvent;
import net.minestom.server.instance.InstanceContainer;
import net.minestom.server.instance.LightingChunk;
import net.minestom.server.instance.anvil.AnvilLoader;
import net.minestom.server.instance.block.Block;
import net.minestom.server.item.ItemStack;
import net.minestom.server.item.Material;
import net.minestom.server.network.packet.server.play.SoundEffectPacket;
import net.minestom.server.sound.SoundEvent;
import net.minestom.server.timer.TaskSchedule;
import net.minestom.server.utils.Direction;

import java.util.HashSet;

public class Cockpit {

    public static Cockpit Instance;

    public enum ButtonType {LONGITUDINAL, LATERAL}

    private final InstanceContainer instance;

    // Buttons
    private int ticksSinceLastInput = 0;
    private Entity currentPressedButton = null;

    private final long cameraButtonDelay = 2650;
    private boolean cameraButtonDisabled = false;
    private final Pos cameraScreenPos = new Pos(0.5, 2.5, 2.5);

    private Entity angleDisplayEntity;
    private Entity xDisplayEntity;
    private Entity yDisplayEntity;
    private Entity compassNeedleEntity;

    final Pos controlCenter = new Pos (0.5, 2.01, -2.5, 0, -90);

    private final TextColor BUTTON_COLOR = NamedTextColor.GRAY; //TextColor.color(215, 255, 255);
    private final int BUTTON_BRIGHTNESS = 10;
    private final int BUTTON_PRESSED_BRIGHTNESS = 8;
    public static final double BUTTON_HEIGHT = 0.025;
    private final double BUTTON_PRESSED_HEIGHT = 0.01;

    public static final TextColor GLOWING_COLOR = TextColor.color(185, 220, 93);
    public static final int GLOWING_BRIGHTNESS = 13;

    public Entity lightEntity;

    public Cockpit() {
        if (Instance == null) {
            Instance = this;
        }

        this.instance = MinecraftServer.getInstanceManager().createInstanceContainer();
        this.instance.setChunkSupplier(LightingChunk::new);
        this.instance.setChunkLoader(new AnvilLoader("worlds/cockpit_world"));

        spawnControlButton(controlCenter.add(0.2, BUTTON_HEIGHT, 0.21), "▲",
            new Vec(2.5, 0.65, 0.65), 0.1f, 0.03f, ButtonType.LONGITUDINAL,
            () -> Submarine.Instance.setMoveState(Submarine.MoveState.FORWARD));
        spawnControlButton(controlCenter.add(0.2, BUTTON_HEIGHT, 0.36), "▼",
            new Vec(2.5, 0.65, 0.65), 0.1f, 0.03f, ButtonType.LONGITUDINAL,
            () -> Submarine.Instance.setMoveState(Submarine.MoveState.BACKWARD));
        spawnControlButton(controlCenter.add(-0.15, BUTTON_HEIGHT, 0.43), "▶",
            new Vec(1, 1, 2), 0.15f, 0.03f, ButtonType.LATERAL,
            () -> Submarine.Instance.setMoveState(Submarine.MoveState.RIGHT));
        spawnControlButton(controlCenter.add(-0.35, BUTTON_HEIGHT, 0.43), "◀",
            new Vec(1, 1, 2), 0.15f, 0.03f, ButtonType.LATERAL,
            () -> Submarine.Instance.setMoveState(Submarine.MoveState.LEFT));
        // setting moveState to NONE is handled in resetCockpitButton()

        new CollisionScanner();
        spawnPositionDisplays(
            new Vec(0.375, 0.55, 0.375),
            new Pos(0.233, 0, -0.21),
            new Pos(0.233, 0, -0.04),
            new Pos(-0.25, 0, 0.16),
            new Vec(0.9, 0.4, 1),
            new Pos(-0.241, 0, -0.185)
        );

        spawnCameraButton(
            new Vec(5, 1.75, 1),
            new Pos(0.55, 2.2, 2.99, 180, 0)
        );

        spawnCameraMapScreen(cameraScreenPos);

        spawnLightEntity();

        this.instance.eventNode().addListener(InstanceTickEvent.class, event -> {
            tickButtonReset();
            ticksSinceLastInput++;
            updatePositionDisplay();
        });
    }

    private void spawnLightEntity() {
        double scale = 0.26;
        lightEntity = new Entity(EntityType.BLOCK_DISPLAY);
        BlockDisplayMeta meta = (BlockDisplayMeta) lightEntity.getEntityMeta();
        meta.setBlockState(Block.OCHRE_FROGLIGHT);
        meta.setScale(new Vec(scale));
        meta.setBrightness(12, 0);
        meta.setHasNoGravity(true);
        lightEntity.setInstance(instance, new Vec(0.5-scale/2, 3.125-scale/2, -2.5-scale/2));
    }

    private void spawnControlButton(Pos pos, String icon, Vec scale, float width, float height, ButtonType buttonType, Runnable action) {
        Entity visual = new Entity(EntityType.TEXT_DISPLAY);
        TextDisplayMeta visualMeta = (TextDisplayMeta) visual.getEntityMeta();

        visualMeta.setText(Component.text(icon).color(BUTTON_COLOR).decorate(TextDecoration.BOLD));
        visualMeta.setScale(scale);
        visualMeta.setBackgroundColor(0);
        visualMeta.setBillboardRenderConstraints(AbstractDisplayMeta.BillboardConstraints.FIXED);
        visualMeta.setHasNoGravity(true);
        visualMeta.setBrightness(BUTTON_BRIGHTNESS, 0);

        visual.setInstance(instance, pos);

        Entity shadow = new Entity(EntityType.TEXT_DISPLAY);
        TextDisplayMeta shadowMeta = (TextDisplayMeta) shadow.getEntityMeta();

        shadowMeta.setText(visualMeta.getText().color(NamedTextColor.BLACK));
        shadowMeta.setScale(scale);
        shadowMeta.setBackgroundColor(0);
        shadowMeta.setBillboardRenderConstraints(AbstractDisplayMeta.BillboardConstraints.FIXED);
        shadowMeta.setHasNoGravity(true);
        shadowMeta.setBrightness(0, 0);

        shadow.setInstance(instance, pos.withY(controlCenter.y()));

        HashSet<Entity> interactionEntities = new HashSet<>();
        if (buttonType == ButtonType.LONGITUDINAL) {
            for (int i = 0; i < 4; i++) {
                Entity interaction = new Entity(EntityType.INTERACTION);
                InteractionMeta interactionMeta = (InteractionMeta) interaction.getEntityMeta();

                interactionMeta.setWidth(width);
                interactionMeta.setHeight(height);

                float xOffset = -0.019f;
                interaction.setInstance(instance, pos.sub(width - width*i - xOffset, 0, width));
                interactionEntities.add(interaction);
            }
        } else if (buttonType == ButtonType.LATERAL) {
            Entity interaction = new Entity(EntityType.INTERACTION);
            InteractionMeta interactionMeta = (InteractionMeta) interaction.getEntityMeta();

            interactionMeta.setWidth(width);
            interactionMeta.setHeight(height);

            float xOffset = 0.012f;
            float zOffset = width - 0.015f;
            interaction.setInstance(instance, pos.sub(-xOffset, 0, zOffset));
            interactionEntities.add(interaction);
        }

        MinecraftServer.getGlobalEventHandler().addListener(PlayerEntityInteractEvent.class, event -> {
           if (!interactionEntities.contains(event.getTarget())) return;

           ticksSinceLastInput = 0;

           if (currentPressedButton == visual) return;

           if (currentPressedButton != null) {
                resetCockpitButton(currentPressedButton);
            }
           action.run();

            pressCockpitButton(visual);
            currentPressedButton = visual;
        });
    }

    private void tickButtonReset() {

        if (currentPressedButton == null) return;

        int maxTickBufferTime = 5;
        if (ticksSinceLastInput >= maxTickBufferTime) {
            resetCockpitButton(currentPressedButton);
            currentPressedButton = null;
        }
    }

    private void pressCockpitButton(Entity visual) {
        visual.teleport(visual.getPosition().withY(controlCenter.y()+ BUTTON_PRESSED_HEIGHT));

        // clear the camera display when the sub moves
        if (Submarine.Instance.getCamera().getIsCameraActive()) {
            Submarine.Instance.getCamera().disableAndClearCameraMap();
        } else if (Submarine.Instance.getCamera().activePrintingTask != null) {
            Submarine.Instance.getCamera().clearPrintingTask();
        }

        TextDisplayMeta visualMeta = (TextDisplayMeta)visual.getEntityMeta();
        visualMeta.setBrightness(BUTTON_PRESSED_BRIGHTNESS, 0);

        SoundEffectPacket clickSound = new SoundEffectPacket(SoundEvent.BLOCK_STONE_BUTTON_CLICK_ON, Sound.Source.BLOCK, visual.getPosition(), 0.3f, 1.5f, 0);

        Main.player.sendPacket(clickSound);
    }

    private void resetCockpitButton(Entity visual) {
        visual.teleport(visual.getPosition().withY(controlCenter.y()+BUTTON_HEIGHT));

        Submarine.Instance.setMoveState(Submarine.MoveState.NONE);

        TextDisplayMeta visualMeta = (TextDisplayMeta)visual.getEntityMeta();
        visualMeta.setBrightness(BUTTON_BRIGHTNESS, 0);

        SoundEffectPacket releaseSound = new SoundEffectPacket(SoundEvent.BLOCK_STONE_BUTTON_CLICK_OFF, Sound.Source.BLOCK, visual.getPosition(), 0.3f, 1.5f, 0);
        Main.player.sendPacket(releaseSound);
    }

    private void spawnPositionDisplays(Vec scale, Pos xOffset, Pos yOffset, Pos angOffset, Vec compassScale, Pos compassOffset) {
        xDisplayEntity = new Entity(EntityType.TEXT_DISPLAY);
        yDisplayEntity = new Entity(EntityType.TEXT_DISPLAY);
        angleDisplayEntity = new Entity(EntityType.TEXT_DISPLAY);
        compassNeedleEntity = new Entity(EntityType.TEXT_DISPLAY);

        Entity[] entities = {xDisplayEntity, yDisplayEntity, angleDisplayEntity};

        for (Entity e : entities) {
            TextDisplayMeta meta = (TextDisplayMeta) e.getEntityMeta();
            meta.setAlignLeft(true);
            meta.setBillboardRenderConstraints(AbstractDisplayMeta.BillboardConstraints.FIXED);
            meta.setBrightness(GLOWING_BRIGHTNESS, 0);
            meta.setScale(scale);
            meta.setHasNoGravity(true);
        }

        TextDisplayMeta xMeta =  (TextDisplayMeta) xDisplayEntity.getEntityMeta();
        TextDisplayMeta yMeta =  (TextDisplayMeta) yDisplayEntity.getEntityMeta();
        TextDisplayMeta angleMeta =  (TextDisplayMeta) angleDisplayEntity.getEntityMeta();

        xMeta.setText(Component.text("X: 123.67").color(GLOWING_COLOR));
        yMeta.setText(Component.text("Y: 123.67").color(GLOWING_COLOR));
        angleMeta.setText(Component.text("067.01").color(GLOWING_COLOR));

        xDisplayEntity.setInstance(instance, controlCenter.add(xOffset));
        yDisplayEntity.setInstance(instance, controlCenter.add(yOffset));
        angleDisplayEntity.setInstance(instance, controlCenter.add(angOffset));

        TextDisplayMeta compMeta =  (TextDisplayMeta) compassNeedleEntity.getEntityMeta();

        compMeta.setAlignment(TextDisplayMeta.Alignment.CENTER);
        compMeta.setText(Component.text("▶").color(GLOWING_COLOR));
        compMeta.setBillboardRenderConstraints(AbstractDisplayMeta.BillboardConstraints.FIXED);
        compMeta.setBrightness(GLOWING_BRIGHTNESS, 0);
        compMeta.setScale(compassScale);
        compMeta.setHasNoGravity(true);
        compMeta.setTranslation(new Vec(-0.0105, -0.0556, 0));

        compassNeedleEntity.setInstance(instance, controlCenter.add(compassOffset).withY(controlCenter.y()+BUTTON_HEIGHT));

        Entity compassBack = new Entity(EntityType.TEXT_DISPLAY);

        TextDisplayMeta  compassBackMeta = (TextDisplayMeta) compassBack.getEntityMeta();
        compassBackMeta.setText(Component.text("⬤").color(NamedTextColor.BLACK));
        compassBackMeta.setBillboardRenderConstraints(AbstractDisplayMeta.BillboardConstraints.FIXED);
        compassBackMeta.setBrightness(0, 0);
        compassBackMeta.setScale(new Vec(2));
        compassBackMeta.setHasNoGravity(true);
        compassBackMeta.setBackgroundColor(0);

        Pos compassBackOrigin = controlCenter.add(compassOffset).add(-0.197, 0, 0.115).withYaw(45);
        compassBack.setInstance(instance, compassBackOrigin);

        CollisionScanner.Instance.spawnDisplayEntities(compassBackOrigin);
    }

    private void spawnCameraButton(Vec scale, Pos pos) {
        Entity glow = new Entity(EntityType.TEXT_DISPLAY);
        TextDisplayMeta glowMeta = (TextDisplayMeta)glow.getEntityMeta();

        glowMeta.setText(Component.text("■").color(GLOWING_COLOR));
        glowMeta.setScale(scale);
        glowMeta.setBackgroundColor(0);
        glowMeta.setBillboardRenderConstraints(AbstractDisplayMeta.BillboardConstraints.FIXED);
        glowMeta.setHasNoGravity(true);
        glowMeta.setBrightness(GLOWING_BRIGHTNESS, 0);

        glow.setInstance(instance, pos);

        Entity button = new Entity(EntityType.TEXT_DISPLAY);
        TextDisplayMeta buttonMeta = (TextDisplayMeta)button.getEntityMeta();
        final float scaleDiff = 0.4f;

        buttonMeta.setText(Component.text("■").color(BUTTON_COLOR));
        buttonMeta.setScale(scale.sub(scaleDiff, scaleDiff, 0));
        buttonMeta.setBackgroundColor(0);
        buttonMeta.setBillboardRenderConstraints(AbstractDisplayMeta.BillboardConstraints.FIXED);
        buttonMeta.setHasNoGravity(true);
        buttonMeta.setBrightness(2, 0);

        button.setInstance(instance, pos.add(-0.007, 0.054, -0.0001));

        Entity interaction = new Entity(EntityType.INTERACTION);
        InteractionMeta interactionMeta = (InteractionMeta)interaction.getEntityMeta();

        interactionMeta.setWidth((float)scale.x()/8);
        interactionMeta.setHeight((float)scale.y()/8);
        interactionMeta.setHasNoGravity(true);

        interaction.setInstance(instance, pos.add(-0.06, 0.13, 0.25));

        MinecraftServer.getGlobalEventHandler().addListener(PlayerEntityInteractEvent.class, event -> {
            if (cameraButtonDisabled || event.getTarget() != interaction) return;
            pressCameraButton(button);
        });
    }

    private void pressCameraButton(Entity button) {
        cameraButtonDisabled = true;
        SoundManager.play(SoundManager.TAKE_PHOTO, new Vec(1.5, 2.5, 2.999));

        Main.player.swingMainHand();

        Vec origin = new Vec(Submarine.Instance.getMapX(), Submarine.Instance.getMapY(), Submarine.Instance.getYaw());

        ProgressionManager.Instance.onPrePhotoTaken(origin.x(), origin.y(), origin.z());
        MapManager.Instance.checkIsPhotoValid(origin);

        Submarine.Instance.takePhoto();

        MinecraftServer.getSchedulerManager().scheduleTask(() -> onCameraFinish(origin),
            TaskSchedule.millis(cameraButtonDelay), TaskSchedule.stop());
    }

    private void onCameraFinish(Vec origin) {
        cameraButtonDisabled = false;
        ProgressionManager.Instance.onPostPhotoTaken(origin.x(), origin.y(), origin.z());
    }

    private void spawnCameraMapScreen(Pos pos) {
        Entity screen = new Entity(EntityType.GLOW_ITEM_FRAME);
        GlowItemFrameMeta screenMeta = (GlowItemFrameMeta)screen.getEntityMeta();

        ItemStack mapItem = ItemStack.of(Material.FILLED_MAP)
            .with(DataComponents.MAP_ID, 0);

        screenMeta.setItem(mapItem);
        screenMeta.setInvisible(true);
        screenMeta.setDirection(Direction.WEST);
        screenMeta.setHasNoGravity(true);

        screen.setInstance(instance, pos);
    }

    private void updatePositionDisplay() {

        Entity[] entities = {xDisplayEntity, yDisplayEntity, angleDisplayEntity};

        for (Entity e : entities) {
            TextDisplayMeta meta = (TextDisplayMeta)e.getEntityMeta();
            if (e == xDisplayEntity) {
                meta.setText(Component.text(String.format("X: %06.2f", Submarine.Instance.getMapX())).color(GLOWING_COLOR));
            } else if (e == yDisplayEntity) {
                meta.setText(Component.text(String.format("Y: %06.2f", Submarine.Instance.getMapY())).color(GLOWING_COLOR));
            } else if (e ==  angleDisplayEntity) {
                meta.setText(Component.text(String.format("%06.2f", Submarine.Instance.getYaw())).color(GLOWING_COLOR));
            }
        }

        float convertedYaw = (360 - (float)Submarine.Instance.getYaw());
        compassNeedleEntity.teleport(compassNeedleEntity.getPosition().withYaw(convertedYaw));
    }

    public InstanceContainer getInstance() {
        return instance;
    }
}
