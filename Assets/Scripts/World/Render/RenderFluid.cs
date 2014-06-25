using UnityEngine;
using System.Collections;

public class RenderFluid : IRenderBlock
{
    private TextureLayout layout;
    private Color color;
    private float amount;
    private bool smoothLighting = true;
    private ushort block;

    public RenderFluid(TextureLayout tex, Color color, float amount, ushort block)
    {
        this.layout = tex;
        this.color = color;
        this.amount = amount;
        this.block = block;
    }

    public void Render (MeshBuildInfo current, World world, Vector3i chunkPos, int x, int y, int z)
    {
        SingleMeshBuildInfo transparent = current.transparent;
        float height = amount * .85f;
        Vector3 center = new Vector3(.5f, height * .5f, .5f);
        Vector3 size = new Vector3(1, height, 1);
        ushort def = Block.DIRT;
        
        byte[] l = new byte[9];
        
        Color blockColor = color;

        int xx, yy, zz;

        if (world.GetBlockAt (chunkPos, x, y + 1, z, def) != block) {
            if (smoothLighting) {
                for ( zz = -1; zz <= 1; zz++) {
                    for ( xx = -1; xx <= 1; xx++) {
                        l [3 * (zz + 1) + (xx + 1)] = world.GetLightAt (chunkPos, xx + x, y + 1, -zz + z, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x, y + 1, z, 0);
            }
            CubeRenderHelper.CubeTop (transparent, x, y, z, layout, center, size, l, smoothLighting, blockColor);
        }
        
        if (world.GetBlockAt (chunkPos, x, y - 1, z, def) != block) {
            if (smoothLighting) {
                for ( zz = -1; zz <= 1; zz++) {
                    for ( xx = -1; xx <= 1; xx++) {
                        l [3 * (zz + 1) + (xx + 1)] = world.GetLightAt (chunkPos, xx + x, y - 1, -zz + z, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x, y - 1, z, 0);
            }
            CubeRenderHelper.CubeBottom (transparent, x, y, z, layout, center, size, l, smoothLighting, blockColor);
        }
        
        if (world.GetBlockAt (chunkPos, x + 1, y, z, def) != block) {
            if (smoothLighting) {
                for ( zz = -1; zz <= 1; zz++) {
                    for ( yy = -1; yy <= 1; yy++) {
                        l [3 * (zz + 1) + (yy + 1)] = world.GetLightAt (chunkPos, x + 1, yy + y, -zz + z, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x + 1, y, z, 0);
            }
            CubeRenderHelper.CubeEast (transparent, x, y, z, layout, center, size, l, smoothLighting, blockColor);
        }
        
        if (world.GetBlockAt (chunkPos, x - 1, y, z, def) != block) {
            if (smoothLighting) {
                for ( zz = -1; zz <= 1; zz++) {
                    for ( yy = -1; yy <= 1; yy++) {
                        l [3 * (zz + 1) + (yy + 1)] = world.GetLightAt (chunkPos, x - 1, yy + y, -zz + z, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x - 1, y, z, 0);
            }
            CubeRenderHelper.CubeWest (transparent, x, y, z, layout, center, size, l, smoothLighting, blockColor);
        }
        
        if (world.GetBlockAt (chunkPos, x, y, z + 1, def) != block) {
            if (smoothLighting) {
                for ( yy = -1; yy <= 1; yy++) {
                    for ( xx = -1; xx <= 1; xx++) {
                        l [3 * (yy + 1) + (xx + 1)] = world.GetLightAt (chunkPos, xx + x, -yy + y, z + 1, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x, y, z + 1, 0);
            }
            CubeRenderHelper.CubeNorth (transparent, x, y, z, layout, center, size, l, smoothLighting, blockColor);
        }
        
        if (world.GetBlockAt (chunkPos, x, y, z - 1, def) != block) {
            if (smoothLighting) {
                for ( yy = -1; yy <= 1; yy++) {
                    for ( xx = -1; xx <= 1; xx++) {
                        l [3 * (yy + 1) + (xx + 1)] = world.GetLightAt (chunkPos, xx + x, -yy + y, z - 1, 0);
                    }
                }
            } else {
                l [4] = world.GetLightAt (chunkPos, x, y, z - 1, 0);
            }
            CubeRenderHelper.CubeSouth (transparent, x, y, z, layout, center, size, l, smoothLighting, blockColor);
        }
    }
}
