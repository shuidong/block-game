using UnityEngine;
using System.Collections;

public class BlockSand : Block
{
    public BlockSand()
    {
        name = "Sand";
        renderer = new RenderFullBlock(new TextureLayout(4), Color.white);
    }
}
