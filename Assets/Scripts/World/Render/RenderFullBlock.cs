using UnityEngine;
using System.Collections;

public class RenderFullBlock : IRenderBlock
{
    private TextureLayout layout;
    private Color color;
    private const bool SMOOTH_LIGHT = true;

    public RenderFullBlock (TextureLayout tex, Color color)
    {
        this.layout = tex;
        this.color = color;
    }

    public void Render (MeshBuildInfo current, World world, Vector3i chunkPos, int x, int y, int z)
    {
        Vector3 center = Vector3.one / 2f;
        Vector3 size = Vector3.one;
        ushort def = Block.DIRT;
        
        byte[] l = new byte[9];
        
        Color blockColor = color;

        int xx, yy, zz;

        if (!Block.GetInstance (world.GetBlockAt (chunkPos, x, y + 1, z, def)).opaque) {
            if (SMOOTH_LIGHT) {
                for ( zz = -1; zz <= 1; zz++) {
                    for ( xx = -1; xx <= 1; xx++) {
                        l [3 * (zz + 1) + (xx + 1)] = world.GetLightAt (chunkPos, xx + x, y + 1, -zz + z, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x, y + 1, z, 0);
            }
            CubeRenderHelper.CubeTop (current, x, y, z, layout, center, size, l, SMOOTH_LIGHT, blockColor);
        }
        
        if (!Block.GetInstance (world.GetBlockAt (chunkPos, x, y - 1, z, def)).opaque) {
            if (SMOOTH_LIGHT) {
                for ( zz = -1; zz <= 1; zz++) {
                    for ( xx = -1; xx <= 1; xx++) {
                        l [3 * (zz + 1) + (xx + 1)] = world.GetLightAt (chunkPos, xx + x, y - 1, -zz + z, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x, y - 1, z, 0);
            }
            CubeRenderHelper.CubeBottom (current, x, y, z, layout, center, size, l, SMOOTH_LIGHT, blockColor);
        }
        
        if (!Block.GetInstance (world.GetBlockAt (chunkPos, x + 1, y, z, def)).opaque) {
            if (SMOOTH_LIGHT) {
                for ( zz = -1; zz <= 1; zz++) {
                    for ( yy = -1; yy <= 1; yy++) {
                        l [3 * (zz + 1) + (yy + 1)] = world.GetLightAt (chunkPos, x + 1, yy + y, -zz + z, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x + 1, y, z, 0);
            }
            CubeRenderHelper.CubeEast (current, x, y, z, layout, center, size, l, SMOOTH_LIGHT, blockColor);
        }
        
        if (!Block.GetInstance (world.GetBlockAt (chunkPos, x - 1, y, z, def)).opaque) {
            if (SMOOTH_LIGHT) {
                for ( zz = -1; zz <= 1; zz++) {
                    for ( yy = -1; yy <= 1; yy++) {
                        l [3 * (zz + 1) + (yy + 1)] = world.GetLightAt (chunkPos, x - 1, yy + y, -zz + z, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x - 1, y, z, 0);
            }
            CubeRenderHelper.CubeWest (current, x, y, z, layout, center, size, l, SMOOTH_LIGHT, blockColor);
        }
        
        if (!Block.GetInstance (world.GetBlockAt (chunkPos, x, y, z + 1, def)).opaque) {
            if (SMOOTH_LIGHT) {
                for ( yy = -1; yy <= 1; yy++) {
                    for ( xx = -1; xx <= 1; xx++) {
                        l [3 * (yy + 1) + (xx + 1)] = world.GetLightAt (chunkPos, xx + x, -yy + y, z + 1, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x, y, z + 1, 0);
            }
            CubeRenderHelper.CubeNorth (current, x, y, z, layout, center, size, l, SMOOTH_LIGHT, blockColor);
        }
        
        if (!Block.GetInstance (world.GetBlockAt (chunkPos, x, y, z - 1, def)).opaque) {
            if (SMOOTH_LIGHT) {
                for ( yy = -1; yy <= 1; yy++) {
                    for ( xx = -1; xx <= 1; xx++) {
                        l [3 * (yy + 1) + (xx + 1)] = world.GetLightAt (chunkPos, xx + x, -yy + y, z - 1, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x, y, z - 1, 0);
            }
            CubeRenderHelper.CubeSouth (current, x, y, z, layout, center, size, l, SMOOTH_LIGHT, blockColor);
        }
    }
}
