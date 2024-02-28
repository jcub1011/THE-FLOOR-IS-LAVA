using Godot;
using System.Collections.Generic;
using System.Linq;
using WorldGeneration;

namespace Players;

public partial class SpriteControls : Sprite2D
{
    static List<Texture2D> _usedSprites = new();

    [Export] Godot.Collections.Array<Texture2D> PossibleSprites;

    public void SetFlipH(bool flipH) => FlipH = flipH;

    public override void _Ready()
    {
        base._Ready();
        SetSprite();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        ReleaseSprite();
    }

    void SetSprite()
    {
        if (_usedSprites.Count == PossibleSprites.Count)
        {
            Texture = PossibleSprites[0];
            return;
        }

        Texture2D picked = PossibleSprites.PickRandom();
        while (_usedSprites.Contains(picked))
        {
            picked = PossibleSprites.PickRandom();
        }
        _usedSprites.Add(picked);
        Texture = picked;
    }

    void ReleaseSprite()
    {
        _usedSprites.Remove(Texture);
    }
}
