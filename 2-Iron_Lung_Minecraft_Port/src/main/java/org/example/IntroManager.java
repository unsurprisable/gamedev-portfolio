package org.example;

import net.kyori.adventure.sound.Sound;
import net.kyori.adventure.text.Component;
import net.kyori.adventure.text.format.NamedTextColor;
import net.kyori.adventure.text.format.TextColor;
import net.kyori.adventure.text.format.TextDecoration;
import net.kyori.adventure.title.Title;
import net.minestom.server.MinecraftServer;
import net.minestom.server.coordinate.Pos;
import net.minestom.server.coordinate.Vec;
import net.minestom.server.entity.Entity;
import net.minestom.server.entity.EntityType;
import net.minestom.server.entity.attribute.Attribute;
import net.minestom.server.entity.metadata.display.AbstractDisplayMeta;
import net.minestom.server.entity.metadata.display.TextDisplayMeta;
import net.minestom.server.event.EventListener;
import net.minestom.server.event.player.PlayerChatEvent;
import net.minestom.server.event.player.PlayerMoveEvent;
import net.minestom.server.instance.block.Block;
import net.minestom.server.potion.Potion;
import net.minestom.server.potion.PotionEffect;
import net.minestom.server.sound.SoundEvent;

import java.time.Duration;

public class IntroManager {

    public static IntroManager Instance;

    public final Runnable onComplete;

    private final Component INTRO_TEXT = Component.text(
        """
        Decades ago, every known star and habitable planet vanished, leaving only
        those who were on space stations or starships. This event became known as
        The Quiet Rapture.
        
        With supplies dwindling and infrastructure crumbling, survivors are searching
        for any trace of natural resources in a universe of barren moons, lit by the
        ghostlight of vanished stars.
        
        One such moon holds a strange anomaly: an ocean of blood. You are a
        convict, tasked with exploring this anomaly, in a makeshift submarine
        nicknamed the Copper Lung. It was not designed for this depth, so you will
        be welded inside and the forward window will be closed.
        
        There was no time for training.
        
        If successful, you earn your freedom.
        
        Type "I understand." to acknowledge and continue.
        """
    ).color(TextColor.color(63, 145, 33));

    private final String[] POST_INTRO_TEXT = {
        "\nReceiving objective briefing signal...:",
        "\nTwo weeks ago, we conducted an exploration of moon AT-5 for the first time since The Quiet Rapture, leading to the discovery of a fourth Blood Ocean. A trench beneath the ocean's surface has several points of interest.",
        "\nYour task is to photograph these points of interest with the SM13's forward camera. Photos must be taken within 2 units of the specified position and 10 degrees of the specified angle. You can also use the camera to help with navigation. Only photos taken at the specified points of interest will be saved.",
        "\nSince you can't navigate by sight, pay attention to your coordinates and consult the Map (\"F\" - swap items keybind). The proximity indicators next to the sub controls will trigger if you're getting close to an obstacle.",
        "\nGood luck."
    };
    private final int POST_INTRO_TEXT_LINE_DELAY = 10;


    private final String[] FAKE_MESSAGES = {
        "This is not an expedition. It is an execution.",
        "When they put you here, they don't want you to return.",
        "Even if you do, what freedom waits for you? A dying universe?",
        "There is no hope. Hope in this void is as illusionary as starlight.",
        "I will choose to breathe my last here at the bottom of an ocean, unseen, unheard, and uncontrolled.",
        "They will get their execution.",
        "I will get my freedom.",
        "I understand."
    };
    private int messageIndex = 0;
    private final EventListener<PlayerChatEvent> chatListener;
    private final EventListener<PlayerMoveEvent> moveListener;

    private final int RADIO_MESSAGE_TIME = 37;

    private final Sound chatSound = Sound.sound(SoundEvent.BLOCK_STONE_BREAK, Sound.Source.MASTER, 0.5f, 1f);
    private final Entity mainTextDisplay;
    private boolean chatMessageSent;

    public IntroManager(Runnable onComplete) {
        if (Instance == null) {
            Instance = this;
        }

        this.onComplete = onComplete;

        mainTextDisplay = new Entity(EntityType.TEXT_DISPLAY);
        TextDisplayMeta meta = (TextDisplayMeta) mainTextDisplay.getEntityMeta();
        meta.setText(INTRO_TEXT);
        meta.setAlignment(TextDisplayMeta.Alignment.CENTER);
        meta.setHasNoGravity(true);
        meta.setBillboardRenderConstraints(AbstractDisplayMeta.BillboardConstraints.FIXED);
        meta.setBrightness(15, 0);
        meta.setScale(new Vec(0.5));
        meta.setLineWidth(400);
        mainTextDisplay.setInstance(Cockpit.Instance.getInstance(), new Vec(0.5, 7.5, 0.01));

        chatListener = EventListener.of(PlayerChatEvent.class, this::checkChatEvent);
        moveListener = EventListener.of(PlayerMoveEvent.class, this::checkMoveEvent);

        MinecraftServer.getGlobalEventHandler().addListener(chatListener);

        Main.player.playSound(SoundManager.INTRO_THEME);
    }


    private void checkChatEvent(PlayerChatEvent event) {
        event.setCancelled(true);
        if (!chatMessageSent) {
            ((TextDisplayMeta)mainTextDisplay.getEntityMeta()).setText(Component.text("Please type \"I understand.\" to acknowledge and continue.\n\n\n\n\n\n\n\n").color(TextColor.color(63, 145, 33)));
        }
        chatMessageSent = true;
        event.getPlayer().playSound(chatSound);
        if (messageIndex == FAKE_MESSAGES.length - 1) {
            for (int i = 0; i < 100; i++) {
                Main.player.sendMessage(Component.empty()); // clear the chat for last line
            }
            event.getPlayer().sendMessage(Component.text("<" + event.getPlayer().getUsername() + "> " + FAKE_MESSAGES[messageIndex++]));
            startDiveSegment();
        } else {
            event.getPlayer().sendMessage(Component.text("<∁?∏√?⊂τ> " + FAKE_MESSAGES[messageIndex++]));
        }
    }

    private void checkMoveEvent(PlayerMoveEvent event) {
        Pos newPos = event.getNewPosition().withCoord(event.getPlayer().getPosition().asVec());
        event.setNewPosition(newPos);
    }

    private void startDiveSegment() {
        Main.player.getAttribute(Attribute.MOVEMENT_SPEED).setBaseValue(0.0f);
        Main.player.setFieldViewModifier(0.03f);
        Main.player.addEffect(new Potion(PotionEffect.BLINDNESS, 255, RADIO_MESSAGE_TIME*20 + 20));
        Main.player.stopSound(SoundManager.INTRO_THEME);
        MinecraftServer.getGlobalEventHandler().removeListener(chatListener);
        Main.player.showTitle(Title.title(
            Component.text("\uF805\uF805\uF805\uF805\uF805\uE003").color(NamedTextColor.WHITE),
            Component.text(" ").color(NamedTextColor.WHITE),
            Title.Times.times(
                Duration.ZERO,
                Duration.ofMillis(750),
                Duration.ofMillis(22000)
            )
        ));
        Cockpit.Instance.getInstance().setBlock(0, 2, -1, Block.BARRIER);
        Cockpit.Instance.getInstance().setBlock(0, 2, 1, Block.BARRIER);
        Main.player.teleport(new Pos(0.5, 1, 0.5, 180, 0));
        Main.player.playSound(SoundManager.INTRO_RADIO);
        MinecraftServer.getGlobalEventHandler().addListener(moveListener);
        MinecraftServer.getSchedulerManager().buildTask(this::endIntro).delay(Duration.ofSeconds(RADIO_MESSAGE_TIME)).schedule();
    }

    private void endIntro() {
        onComplete.run();
        Cockpit.Instance.getInstance().setBlock(0, 2, -1, Block.AIR);
        Cockpit.Instance.getInstance().setBlock(0, 2, 1, Block.AIR);
        Main.player.getAttribute(Attribute.MOVEMENT_SPEED).setBaseValue(0.085f);
        Main.player.setFieldViewModifier(0.1f);
        MinecraftServer.getGlobalEventHandler().removeListener(moveListener);
        MinecraftServer.getSchedulerManager().buildTask(() -> {
            for (int i = 0; i < 100; i++) {
                Main.player.sendMessage(Component.empty()); // clear the chat
            }
            Main.player.sendMessage(Component.text(POST_INTRO_TEXT[0]).color(NamedTextColor.GREEN));
            SoundManager.play(chatSound);
            MinecraftServer.getSchedulerManager().buildTask(()->{Main.player.sendMessage(Component.text(POST_INTRO_TEXT[1]).color(NamedTextColor.GREEN));SoundManager.play(chatSound);})
                .delay(Duration.ofSeconds(3)).schedule();
            MinecraftServer.getSchedulerManager().buildTask(()->{Main.player.sendMessage(Component.text(POST_INTRO_TEXT[2]).color(NamedTextColor.GREEN));SoundManager.play(chatSound);})
                .delay(Duration.ofSeconds(3 + POST_INTRO_TEXT_LINE_DELAY)).schedule();
            MinecraftServer.getSchedulerManager().buildTask(()->{Main.player.sendMessage(Component.text(POST_INTRO_TEXT[3]).color(NamedTextColor.GREEN));SoundManager.play(chatSound);})
                .delay(Duration.ofSeconds(3 + POST_INTRO_TEXT_LINE_DELAY * 2)).schedule();
            MinecraftServer.getSchedulerManager().buildTask(()->{Main.player.sendMessage(Component.text(POST_INTRO_TEXT[4]).color(NamedTextColor.GREEN));SoundManager.play(chatSound);})
                .delay(Duration.ofSeconds(3 + POST_INTRO_TEXT_LINE_DELAY * 3)).schedule();
            MinecraftServer.getSchedulerManager().buildTask(()->SoundManager.play(SoundManager.HOPELESS_AMBIENCE))
                .delay(Duration.ofSeconds(3 + POST_INTRO_TEXT_LINE_DELAY * 3 + 5)).schedule();
        }).delay(Duration.ofSeconds(15)).schedule();
    }
}
