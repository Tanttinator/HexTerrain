using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tanttinator.GridUtility;

namespace Tanttinator.HexTerrain
{
    /// <summary>
    /// Represent one chunk of the terrain mesh.
    /// </summary>
    [ExecuteInEditMode]
    public class HexChunk : MonoBehaviour
    {
        Dictionary<Coords, HexTile> tiles = new Dictionary<Coords, HexTile>();
        Coords position;

        [SerializeField] HexMesh ground = default;

        /// <summary>
        /// Add a new tile to this chunk.
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="world"></param>
        HexTile CreateTile(Coords coords, HexWorld world)
        {
            HexTile tile = tiles[coords] = new HexTile(coords, this, world);
            return tile;
        }

        /// <summary>
        /// Try to get the tile in the given position.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public HexTile GetTile(Coords coords, HexWorld world)
        {
            if (tiles.ContainsKey(coords))
                return tiles[coords];
            return CreateTile(coords, world);
        }

         /// <summary>
        /// Recalculate the mesh.
        /// </summary>
        void Triangulate()
        {
            ground.Clear();

            foreach (HexTile tile in tiles.Values)
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
