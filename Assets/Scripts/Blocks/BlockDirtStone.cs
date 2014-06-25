using UnityEngine;
using System.Collections;

public class BlockDirtStone : Block
{
    public BlockDirtStone()
    {
        name = "Rocky Dirt";
        renderer = new RenderFullBlock(new TextureLayout(0), new Color32(173, 133, 92, 255));
    }
}
