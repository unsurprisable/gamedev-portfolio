package org.example;

import net.minestom.server.MinecraftServer;
import net.minestom.server.coordinate.Pos;
import net.minestom.server.coordinate.Vec;
import net.minestom.server.entity.GameMode;
import net.minestom.server.entity.Player;
import net.minestom.server.entity.PlayerSkin;
import net.minestom.server.event.GlobalEventHandler;
import net.minestom.server.event.player.AsyncPlayerConfigurationEvent;
import net.minestom.server.event.player.PlayerDisconnectEvent;
import net.minestom.server.event.player.PlayerSpawnEvent;

public class Main {

    public static Player player;

    static void main() {
        MinecraftServer server = MinecraftServer.init();

        new Submarine(new Vec(152.30, 120.82, 90));
        new Cockpit();

        MinecraftServer.getCommandManager().register(new TeleportCommand());
        MinecraftServer.getCommandManager().register(new SkipIntroCommand());

        GlobalEventHandler globalEventHandler = MinecraftServer.getGlobalEventHandler();
        globalEventHandler.addListener(AsyncPlayerConfigurationEvent.class, event -> {
            if (!MinecraftServer.getConnectionManager().getOnlinePlayers().isEmpty()) {
                event.getPlayer().kick("§cGame in progress.\n§7This server is single-player only.");
                return;
            }
            Player configPlayer = event.getPlayer();

            event.setSpawningInstance(Cockpit.Instance.getInstance());
            configPlayer.setRespawnPoint(new Pos(.5, 7, 2.25, 180, 0));
            configPlayer.setGameMode(GameMode.ADVENTURE);
        });

        globalEventHandler.addListener(PlayerSpawnEvent.class, event -> {
            player = event.getPlayer();
            event.getPlayer().setSkin(PlayerSkin.fromUuid(event.getPlayer().getUuid().toString()));

            new IntroManager(Main::startGame);

            // initialize the camera display to be black when player joins
            Submarine.Instance.getCamera().disableAndClearCameraMap();
        });


        globalEventHandler.addListener(PlayerDisconnectEvent.class, event -> {
            System.out.println("Player has disconnected. (sorry, I didn't implement game saving :/)\nShutting down server...");
            MinecraftServer.stopCleanly();
        });

        server.start("localhost", 25565);
        System.out.println("Starting server on port 25565...");
    }

    public static void startGame() {
        new MapManager();
        new ProgressionManager();
        new SoundManager();
        new ScalySoundManager();
        new VoidShake();
    }
}
