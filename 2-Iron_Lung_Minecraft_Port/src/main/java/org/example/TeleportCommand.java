package org.example;

import net.minestom.server.command.builder.Command;
import net.minestom.server.command.builder.arguments.ArgumentType;

public class TeleportCommand extends Command {
    public TeleportCommand() {
        super("tp");

        var objective = ArgumentType.Integer("objective");
        addSyntax((sender, context) -> {
            int objectiveNum = context.get(objective);
            MapObjective obj = MapManager.Instance.getMapObjective(objectiveNum);
            Submarine.Instance.teleport(obj.getMapX(), obj.getMapY(), obj.getYaw());
        }, objective);

        var xPos = ArgumentType.Double("xPos");
        var yPos = ArgumentType.Double("yPos");
        var yawPos = ArgumentType.Double("yaw");
        addSyntax((sender, context) -> {
           double x = context.get(xPos);
           double y = context.get(yPos);
           double yaw = context.get(yawPos);
           Submarine.Instance.teleport(x, y, yaw);
        }, xPos, yPos, yawPos);
    }
}
