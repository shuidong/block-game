using UnityEngine;
using System.Collections;

public class BlockStone : Block {
    public BlockStone() {
        name = "Stone";
        renderer = new RenderFullBlock(new TextureLayout(0), Color.white);
    }
}
