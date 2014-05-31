using UnityEngine;
using System.Collections;

public class BlockGrass : Block {
    IRenderBlock renderer;

    public BlockGrass() {
        renderer = new RenderFullBlock(new TextureLayout(2, 1, 3), Color.white);
    }

    public override IRenderBlock Renderer {
        get {
            return renderer;
        }
    }
}
