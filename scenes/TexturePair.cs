using Godot;
using System;

public class TexturePair : Control
{
    [Export] Texture homeTexture, awayTexture;

    public Texture HomeTexture => homeTexture;
    public Texture AwayTexture => awayTexture;
}
