package org.example;

import net.kyori.adventure.key.Key;
import net.kyori.adventure.sound.Sound;
import net.minestom.server.coordinate.Point;
import net.minestom.server.coordinate.Vec;
import net.minestom.server.event.instance.InstanceTickEvent;
import net.minestom.server.instance.InstanceContainer;

import java.util.ArrayList;

public class SoundManager {

    public static SoundManager Instance;

    private final SubmarineMoveSFX subMoveSFX;
    private final ArrayList<MovingSound> movingSounds = new ArrayList<>();
    private final ArrayList<LoopingSound> loopingSounds = new ArrayList<>();
    private static final ArrayList<TemporaryMovingSound>  temporaryMovingSounds = new ArrayList<>();

    public SoundManager() {
        if (Instance == null) {
            Instance = this;
        }

        InstanceContainer instance = Cockpit.Instance.getInstance();

        subMoveSFX = new SubmarineMoveSFX(instance);

        movingSounds.add(new MovingSound(
            instance, ARTERIES, 22.0,
            new Vec(110.5, 0, 54.5),
            32
        ));
        movingSounds.add(new MovingSound(
            instance, VOID, 35,
            new Vec(160.5, 0, 32.5),
            28
        ));
        movingSounds.add(new MovingSound(
            instance, ARTERIES, 35,
            new Vec(213.5, 0, 184.5),
            12
        ));

        loopingSounds.add(new LoopingSound(
            AMBIENT_NOISE, 39.8, true
        ));

        Cockpit.Instance.getInstance().eventNode().addListener(InstanceTickEvent.class, event -> {
            subMoveSFX.update(event.getDuration());
            movingSounds.forEach(movingSound -> movingSound.update(event.getDuration()));
            loopingSounds.forEach(loopingSound -> loopingSound.update(event.getDuration()));
            for (int i = temporaryMovingSounds.size()-1; i >= 0; i--) {
                TemporaryMovingSound temporaryMovingSound = temporaryMovingSounds.get(i);
                temporaryMovingSound.update(event.getDuration());
                if (temporaryMovingSound.getIsMarkedForDeletion()) {
                    temporaryMovingSound.delete();
                    temporaryMovingSounds.remove(temporaryMovingSound);
                }
            }
        });
    }


    // --- SOUNDTRACKS ---
    public static Sound DARK_BRAMBLE = Sound.sound(
        Key.key("custom:dark_bramble"),
        Sound.Source.MASTER,
        0.375f, 1.0f
    );
    public static Sound THREATENING_AMBIENCE = Sound.sound(
        Key.key("custom:threatening_ambience"),
        Sound.Source.MASTER,
        0.375f, 1.0f
    );
    public static Sound OMINOUS_AMBIENCE = Sound.sound(
        Key.key("custom:ominous_ambience"),
        Sound.Source.MASTER,
        0.375f, 1.0f
    );
    public static Sound HOPELESS_AMBIENCE = Sound.sound(
        Key.key("custom:hopeless_ambience"),
        Sound.Source.MASTER,
        0.42f, 1.0f
    );


    // --- SOUND EFFECTS ---
    public static Sound OPEN_MAP = Sound.sound(
        Key.key("custom:open_map"),
        Sound.Source.MASTER,
        0.18f, 1.15f
    );
    public static Sound TAKE_PHOTO = Sound.sound(
        Key.key("custom:take_photo"),
        Sound.Source.MASTER,
        1.0f,
        1.0f
    );
    public static Sound SUCCESSFUL_PHOTOGRAPH = Sound.sound(
        Key.key("custom:successful_photograph"),
        Sound.Source.MASTER,
        0.13f, 0.8f
    );
    public static Sound TELEPORT = Sound.sound(
        Key.key("custom:teleport"),
        Sound.Source.MASTER,
        1.0f, 1.0f
    );
    public static Sound METAL_BANG = Sound.sound(
        Key.key("custom:metal_bang"),
        Sound.Source.MASTER,
        1.0f, 1.0f
    );
    public static Sound PROXIMITY = Sound.sound(
        Key.key("custom:proximity"),
        Sound.Source.MASTER,
        0.65f, 1.0f
    );
    public static Sound ARTERIES = Sound.sound(
        Key.key("custom:arteries"),
        Sound.Source.MASTER,
        1.0f, 1.0f
    );
    public static Sound VOID = Sound.sound(
        Key.key("custom:void"),
        Sound.Source.MASTER,
        1.0f, 1.0f
    );
    public static Sound AMBIENT_NOISE = Sound.sound(
        Key.key("custom:ambient_noise"),
        Sound.Source.MASTER,
        2.0f, 1.0f
    );
    public static Sound SCALY_PASS = Sound.sound(
        Key.key("custom:scaly_pass"),
        Sound.Source.MASTER,
        10.0f, 1.0f
    );
    public static Sound SCALY_GROWL_1 = Sound.sound(
        Key.key("custom:scaly_growl_1"),
        Sound.Source.MASTER,
        10.0f, 1.0f
    );
    public static Sound SCALY_GROWL_2 = Sound.sound(
        Key.key("custom:scaly_growl_2"),
        Sound.Source.MASTER,
        10.0f, 1.0f
    );
    public static Sound SCALY_GROWL_3 = Sound.sound(
        Key.key("custom:scaly_growl_3"),
        Sound.Source.MASTER,
        10.0f, 1.0f
    );
    public static Sound SCALY_GROWL_4 = Sound.sound(
        Key.key("custom:scaly_growl_4"),
        Sound.Source.MASTER,
        10.0f, 1.0f
    );
    public static Sound SCALY_GROWL = Sound.sound(
        Key.key("custom:scaly_growl"),
        Sound.Source.MASTER,
        10.0f, 1.0f
    );
    public static Sound OXYGEN_NOTIFICATION = Sound.sound(
        Key.key("custom:oxygen_notification"),
        Sound.Source.MASTER,
        5.0f, 1.0f
    );
    public static Sound SCREAMING_GUY = Sound.sound(
        Key.key("custom:screaming_guy"),
        Sound.Source.MASTER,
        0.85f, 1.0f
    );
    public static Sound STEAM_PIPE_BURST = Sound.sound(
        Key.key("custom:steam_pipe_burst"),
        Sound.Source.MASTER,
        2.0f, 1.0f
    );
    public static Sound STEAM_PIPE_LOOP = Sound.sound(
        Key.key("custom:steam_pipe_loop"),
        Sound.Source.MASTER,
        0.12f, 1.0f
    );
    public static Sound SCALY_ATTACK = Sound.sound(
        Key.key("custom:scaly_attack"),
        Sound.Source.MASTER,
        20.0f, 1.0f
    );
    public static Sound SCALY_POST_ATTACK = Sound.sound(
        Key.key("custom:scaly_post_attack"),
        Sound.Source.MASTER,
        20.0f, 1.0f
    );
    public static Sound FROG_JUMPSCARE = Sound.sound(
        Key.key("custom:frog_jumpscare"),
        Sound.Source.MASTER,
        20.0f, 1.0f
    );
    public static Sound INTRO_THEME = Sound.sound(
        Key.key("custom:intro_theme"),
        Sound.Source.MASTER,
        0.35f, 1.0f
    );
    public static Sound INTRO_RADIO = Sound.sound(
        Key.key("custom:intro_radio"),
        Sound.Source.MASTER,
        0.35f, 1.0f
    );

    public static void play(Sound sound) {
        if (Main.player == null) return;
        Main.player.playSound(sound);
    }
    public static void play(Sound sound, Point point) {
        if (Main.player == null) return;
        Main.player.playSound(sound, point);
    }
    public static void play(Sound sound, Sound.Emitter emitter) {
        if (Main.player == null) return;
        Main.player.playSound(sound, emitter);
    }

    public static void stop(Sound sound) {
        if (Main.player == null) return;
        Main.player.stopSound(sound);
    }

    public static void playTemporary(Sound sound, double soundLength, double angle, double distance) {
        if (Main.player == null) return;
        temporaryMovingSounds.add(new TemporaryMovingSound(Cockpit.Instance.getInstance(), sound, soundLength, angle, distance));
    }
}
