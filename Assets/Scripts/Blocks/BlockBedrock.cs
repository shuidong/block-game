using UnityEngine;
using System.Collections;

public class BlockBedrock : Block {
    public BlockBedrock()
    {
        name = "Bedrock";
        renderer = new RenderFullBlock(new TextureLayout(0), Color.white * 0.15f);
        indestructable = true;
    }
}
