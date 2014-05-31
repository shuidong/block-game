using UnityEngine;
using System.Collections;

public class BlockDirt : Block {
    IRenderBlock renderer;

    public BlockDirt() {
        renderer = new RenderFullBlock(new TextureLayout(1), Color.white);
    }

    public override IRenderBlock Renderer {
        get {
            return renderer;
        }
    }
}
