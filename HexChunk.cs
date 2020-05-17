using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tanttinator.GridUtility;

namespace Tanttinator.HexTerrain
{
    /// <summary>
    /// Represent one chunk of the terrain mesh.
    /// </summary>
    public class HexChunk : MonoBehaviour
    {
        HexTile[,] tiles;
        Coords position;

        [SerializeField] HexMesh ground = default;

        /// <summary>
        /// Create new tiles based on the position of this chunk.
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkY"></param>
        /// <param name="chunkSize"></param>
        public void SetTiles(Coords coords, HexWorld world)
        {
            int chunkSize = world.chunkSize;
            tiles = new HexTile[chunkSize, chunkSize];
            position = coords;
            for(int x = 0; x < chunkSize; x++)
            {
                for(int z = 0; z < chunkSize; z++)
                {
                    tiles[x, z] = new HexTile(coords.x * chunkSize + x, coords.y * chunkSize + z, world);
                }
            }
        }

        /// <summary>
        /// Try to get the tile in the given position.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public HexTile GetTile(Coords coords)
        {
            Coords localPos = coords - position;
            if (localPos.x < 0 || localPos.x >= tiles.GetLength(0) || localPos.y < 0 || localPos.y >= tiles.GetLength(1))
                return null;
            return tiles[localPos.x, localPos.y];
        }

         /// <summary>
        /// Recalculate the mesh.
        /// </summary>
        void Triangulate()
        {
            ground.Clear();

            foreach (HexTile tile in tiles)
                TriangulateTile(tile);

            ground.Apply();
        }

        /// <summary>
        /// Triangulate single tile.
        /// </summary>
        /// <param name="tile"></param>
        void TriangulateTile(HexTile tile)
        {
            foreach (Direction dir in Direction.directions)
                TriangulateSector(tile, dir);
        }

        /// <summary>
        /// Triangulate single sector in a tile.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="dir"></param>
        void TriangulateSector(HexTile tile, Direction dir)
        {
            ground.AddTriangle(tile.center, tile.vertices[dir][0], tile.vertices[dir][1]);
        }

        /// <summary>
        /// Mark this chunk for mesh recalculation.
        /// </summary>
        public void Refresh()
        {
            enabled = true;
        }

        private void LateUpdate()
        {
            Triangulate();
            enabled = false;
        }
    }
}
