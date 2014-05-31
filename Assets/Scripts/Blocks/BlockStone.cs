using UnityEngine;
using System.Collections;

public class BlockStone : Block {
    IRenderBlock renderer;

    public BlockStone() {
        renderer = new RenderFullBlock(new TextureLayout(0), Color.white);
    }

    public override IRenderBlock Renderer {
        get {
            return renderer;
        }
    }
}
