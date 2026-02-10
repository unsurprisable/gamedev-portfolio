package org.example;

import net.minestom.server.coordinate.Pos;

public record SpriteObject(Pos position, SpriteTexture texture, double size, double verticalOffset) {

    public SpriteObject(Pos position, SpriteTexture texture) {
        this(position, texture, 1.0, 0.0);
    }
}
