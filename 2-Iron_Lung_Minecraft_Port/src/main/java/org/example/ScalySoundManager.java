package org.example;

import net.minestom.server.event.instance.InstanceTickEvent;

import java.util.Random;

public class ScalySoundManager {

    public static ScalySoundManager Instance;

    private record AggressionLevelData(
        double minGrowlTime,
        double maxGrowlTime
    ) {}

    private final AggressionLevelData[] aggressionLevelData = new AggressionLevelData[3];
    private AggressionLevelData aggressionLevel;
    private boolean isEnabled = false;

    private double growlTimeLeft = 30;

    public ScalySoundManager() {
        if (Instance == null) {
            Instance = this;
        }

        aggressionLevelData[0] = new AggressionLevelData(90.0, 300.0);
        aggressionLevelData[1] = new AggressionLevelData(60.0, 220.0);
        aggressionLevelData[2] = new AggressionLevelData(15.0, 145.0);

        Random rand = new Random();
        Cockpit.Instance.getInstance().eventNode().addListener(InstanceTickEvent.class, event -> {
            if (!isEnabled) return;

           double deltaTime = event.getDuration() / 1000.0;

           growlTimeLeft -= deltaTime;
           if (growlTimeLeft <= 0) {
               SoundManager.playTemporary(SoundManager.SCALY_GROWL, 15, rand.nextInt(0, 360), 40);
               growlTimeLeft = rand.nextDouble(aggressionLevel.minGrowlTime,  aggressionLevel.maxGrowlTime);
           }
        });
    }

    public void setAggressionLevel(int aggressionLevel) {
        this.aggressionLevel = aggressionLevelData[aggressionLevel];
    }

    public void setEnabled(boolean enabled) {
        isEnabled = enabled;
    }
    public boolean getIsEnabled() {
        return isEnabled;
    }
}
