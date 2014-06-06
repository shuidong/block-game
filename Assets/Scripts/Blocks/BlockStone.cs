using UnityEngine;
using System.Collections;

public class BlockStone : Block
{
    public readonly uint depth;
    
    public BlockStone(uint depth)
    {
        this.depth = depth;
        name = "Stone (Depth " + (depth + 1) + ")";
        float mult = 1f - (depth * .1f);
        renderer = new RenderFullBlock(new TextureLayout(0), Color.white * mult);
    }
}
