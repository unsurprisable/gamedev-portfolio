package org.example;

import net.minestom.server.coordinate.Point;
import net.minestom.server.coordinate.Pos;

import java.util.Arrays;

public class CollisionScanner {

    public static CollisionScanner Instance;

    private final CollisionScannerDirection[] scannerDirections;

    public static final double MAX_DETECTION_DISTANCE = 2.5;

    public CollisionScanner() {
        if (Instance == null) {
            Instance = this;
        }

        scannerDirections = new CollisionScannerDirection[4];
        for (int i = 0; i < scannerDirections.length; i++) {
            scannerDirections[i] = new CollisionScannerDirection();
        }
    }

    public void spawnDisplayEntities(Point compassCenter) {
        final double offset = 0.125;

        Point origin = compassCenter.add(0.172, Cockpit.BUTTON_HEIGHT/2, -0.095);

        scannerDirections[0].setInstance(Cockpit.Instance.getInstance(), origin.add(offset, 0, 0));
        scannerDirections[1].setInstance(Cockpit.Instance.getInstance(), origin.add(0, 0, -offset));
        scannerDirections[2].setInstance(Cockpit.Instance.getInstance(), origin.add(-offset, 0, 0));
        scannerDirections[3].setInstance(Cockpit.Instance.getInstance(), origin.add(0, 0, offset));
    }

    public void searchForCollisions(Pos worldPos) {
        for (int i = 0; i < 4; i++) {
            CollisionScannerDirection scannerDirection =  scannerDirections[i];
            if (scannerDirection.getIsForceDetecting()) continue;
            Pos origin = worldPos.withYaw(i * -90);

            double searchDistance = MAX_DETECTION_DISTANCE + Submarine.HITBOX_RADIUS;
            double distance = Submarine.Instance.getCamera().scanForBlocks(origin, searchDistance, 25);

            if (distance == -1) {
                scannerDirection.noObjectDetected();
            } else {
                scannerDirection.objectDetected(distance - Submarine.HITBOX_RADIUS);
            }
        }
    }

    public CollisionScannerDirection getCollisionScannerDirection(int index) {
        return scannerDirections[index];
    }
}
