using UnityEngine;
using System.Collections;

public class BlockGrass : Block {
    public BlockGrass() {
        renderer = new RenderFullBlock(new TextureLayout(2, 1, 3), Color.white);
    }
}
