using UnityEngine;
using System.Collections;

public class RenderFullBlock : IRenderBlock
{
    private TextureLayout layout;
    private Color color;

    public RenderFullBlock (TextureLayout tex, Color color)
    {
        this.layout = tex;
        this.color = color;
    }

    public void Render (MeshBuildInfo current, World world, Vector3i chunkPos, int x, int y, int z)
    {
        bool smooth = true;
        Vector3 center = Vector3.one / 2f;
        Vector3 size = Vector3.one;
        ushort def = Block.AIR;
        
        byte[] l = new byte[9];
        
        Color blockColor = color;

        if (!Block.GetInstance (world.GetBlockAt (chunkPos, x, y + 1, z, def)).Opaque) {
            if (smooth) {
                for (int zz = -1; zz <= 1; zz++) {
                    for (int xx = -1; xx <= 1; xx++) {
                        l [3 * (zz + 1) + (xx + 1)] = world.GetLightAt (chunkPos, xx + x, y + 1, -zz + z, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x, y + 1, z, 0);
            }
            CubeRenderHelper.CubeTop (current, x, y, z, layout, center, size, l, smooth, blockColor);
        }
        
        if (!Block.GetInstance (world.GetBlockAt (chunkPos, x, y - 1, z, def)).Opaque) {
            if (smooth) {
                for (int zz = -1; zz <= 1; zz++) {
                    for (int xx = -1; xx <= 1; xx++) {
                        l [3 * (zz + 1) + (xx + 1)] = world.GetLightAt (chunkPos, xx + x, y - 1, -zz + z, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x, y - 1, z, 0);
            }
            CubeRenderHelper.CubeBottom (current, x, y, z, layout, center, size, l, smooth, blockColor);
        }
        
        if (!Block.GetInstance (world.GetBlockAt (chunkPos, x + 1, y, z, def)).Opaque) {
            if (smooth) {
                for (int zz = -1; zz <= 1; zz++) {
                    for (int yy = -1; yy <= 1; yy++) {
                        l [3 * (zz + 1) + (yy + 1)] = world.GetLightAt (chunkPos, x + 1, yy + y, -zz + z, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x + 1, y, z, 0);
            }
            CubeRenderHelper.CubeEast (current, x, y, z, layout, center, size, l, smooth, blockColor);
        }
        
        if (!Block.GetInstance (world.GetBlockAt (chunkPos, x - 1, y, z, def)).Opaque) {
            if (smooth) {
                for (int zz = -1; zz <= 1; zz++) {
                    for (int yy = -1; yy <= 1; yy++) {
                        l [3 * (zz + 1) + (yy + 1)] = world.GetLightAt (chunkPos, x - 1, yy + y, -zz + z, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x - 1, y, z, 0);
            }
            CubeRenderHelper.CubeWest (current, x, y, z, layout, center, size, l, smooth, blockColor);
        }
        
        if (!Block.GetInstance (world.GetBlockAt (chunkPos, x, y, z + 1, def)).Opaque) {
            if (smooth) {
                for (int yy = -1; yy <= 1; yy++) {
                    for (int xx = -1; xx <= 1; xx++) {
                        l [3 * (yy + 1) + (xx + 1)] = world.GetLightAt (chunkPos, xx + x, -yy + y, z + 1, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x, y, z + 1, 0);
            }
            CubeRenderHelper.CubeNorth (current, x, y, z, layout, center, size, l, smooth, blockColor);
        }
        
        if (!Block.GetInstance (world.GetBlockAt (chunkPos, x, y, z - 1, def)).Opaque) {
            if (smooth) {
                for (int yy = -1; yy <= 1; yy++) {
                    for (int xx = -1; xx <= 1; xx++) {
                        l [3 * (yy + 1) + (xx + 1)] = world.GetLightAt (chunkPos, xx + x, -yy + y, z - 1, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x, y, z - 1, 0);
            }
            CubeRenderHelper.CubeSouth (current, x, y, z, layout, center, size, l, smooth, blockColor);
        }
    }
}
