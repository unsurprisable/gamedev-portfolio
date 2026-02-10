package org.example;

import net.kyori.adventure.text.Component;
import net.kyori.adventure.text.format.NamedTextColor;
import net.kyori.adventure.text.format.TextDecoration;
import net.minestom.server.inventory.AbstractInventory;
import net.minestom.server.item.ItemStack;
import net.minestom.server.item.Material;

import java.util.Arrays;

public class MapObjective {
    private final ItemStack itemStack;
    private final AbstractInventory inventory;
    private final int slot;

    private boolean isCompleted;
    private final int x;
    private final int y;
    private final int yaw;

    public MapObjective(AbstractInventory inventory, int slot, int x, int y, int yaw) {
        this.inventory = inventory;
        this.slot = slot;

        this.isCompleted = false;
        this.x = x;
        this.y = y;
        this.yaw = yaw;

        Component name = Component.text("PHOTO ✕").color(NamedTextColor.RED).decoration(TextDecoration.ITALIC, false);
        Component[] lore = {
            Component.text(String.format("X_%03d", x)).color(NamedTextColor.WHITE).decoration(TextDecoration.ITALIC, false),
            Component.text(String.format("Y_%03d", y)).color(NamedTextColor.WHITE).decoration(TextDecoration.ITALIC, false),
            Component.text(String.format("A_%03d", yaw)).color(NamedTextColor.WHITE).decoration(TextDecoration.ITALIC, false),
        };

        this.itemStack = ItemStack.builder(Material.BRICK)
            .customName(name)
            .lore(Arrays.asList(lore))
            .build();

        this.inventory.setItemStack(slot, itemStack);
    }

    public ItemStack getItemStack() {
        return itemStack;
    }

    public void setCompleted() {
        this.isCompleted = true;
        inventory.setItemStack(slot, itemStack.withCustomName(
            Component.text("PHOTO ").color(NamedTextColor.GREEN).decoration(TextDecoration.ITALIC, false).append(
                Component.text("✓").color(NamedTextColor.GREEN).decoration(TextDecoration.ITALIC, false).decorate(TextDecoration.BOLD)
            )
        ));
    }

    public boolean getIsCompleted() {
        return isCompleted;
    }
    public int getMapX() {
        return x;
    }
    public int getMapY() {
        return y;
    }
    public int getYaw() {
        return yaw;
    }
}
