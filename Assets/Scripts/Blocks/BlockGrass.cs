using UnityEngine;
using System.Collections;

public class BlockGrass : Block {
    public BlockGrass() {
        name = "Grass";
        renderer = new RenderFullBlock(new TextureLayout(2, 1, 3), Color.white);
    }
}
