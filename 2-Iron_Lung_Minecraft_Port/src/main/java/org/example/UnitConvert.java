package org.example;

import net.minestom.server.coordinate.Pos;

public class UnitConvert {
    public static double mapXToWorldZ(double mapX) {
        return mapX / 4.0 + 5;
    }
    public static double mapYToWorldX(double mapY) {
        return mapY / 4.0 + 4;
    }
    public static double worldXToMapY(double worldX) {
        return (worldX - 4) * 4.0;
    }
    public static double worldZToMapX(double worldZ) {
        return (worldZ - 5) * 4.0;
    }
    public static Pos subToWorldPos(double subX, double subZ, double yaw) {
        return new Pos(subZ, 0, subX, -(float)yaw, 0);
    }
}
