using UnityEngine;
using System.Collections;

public class BlockDirt : Block {
    public BlockDirt() {
        renderer = new RenderFullBlock(new TextureLayout(1), Color.white);
    }
}
