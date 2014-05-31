using UnityEngine;
using System.Collections;

public class BlockAir : Block {
    public override bool Opaque {
        get {
            return true;
        }
    }
}
