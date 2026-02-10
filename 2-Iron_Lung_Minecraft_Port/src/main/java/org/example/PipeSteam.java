package org.example;

import net.minestom.server.coordinate.Point;
import net.minestom.server.coordinate.Vec;
import net.minestom.server.event.instance.InstanceTickEvent;
import net.minestom.server.network.packet.server.play.ParticlePacket;
import net.minestom.server.particle.Particle;

import java.util.Random;

public class PipeSteam {

//    private final Particle particle = Particle.DUST.withProperties(NamedTextColor.WHITE, 2f);
    private final Particle particle = Particle.POOF;
    private final float maxSpeed = 0.25f;

    public PipeSteam(Point position, Vec velocity) {
        Random rand = new Random();

        SoundManager.play(SoundManager.STEAM_PIPE_BURST, position);
        LoopingSound sound = new LoopingSound(SoundManager.STEAM_PIPE_LOOP, 28.6, false);
        sound.setOrigin(position);
        sound.play();


        Cockpit.Instance.getInstance().eventNode().addListener(InstanceTickEvent.class, event -> {
            ParticlePacket packet = new ParticlePacket(particle, position, velocity.mul(maxSpeed), maxSpeed, 0);
            Main.player.sendPacket(packet);
            sound.update(event.getDuration());
        });
    }
}
