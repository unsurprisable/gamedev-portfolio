package org.example;

import net.minestom.server.command.builder.Command;
import net.minestom.server.coordinate.Pos;

public class SkipIntroCommand extends Command {
    public SkipIntroCommand() {
        super("skipintro");

        setDefaultExecutor((sender, command) -> {
            Main.player.teleport(new Pos(0.5, 1, 0.5, 180, 0));
            IntroManager.Instance.onComplete.run();
        });
    }
}
