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

        public HexWorld world;

        /// <summary>
        /// Add a new tile to this chunk.
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="world"></param>
        HexTile CreateTile(Coords coords)
        {
            HexTile tile = tiles[coords] = new HexTile(coords, this, world);
            return tile;
        }

        /// <summary>
        /// Try to get the tile in the given position or create a new one.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public HexTile GetOrCreateTile(Coords coords)
        {
            if (tiles.ContainsKey(coords))
                return tiles[coords];
            return CreateTile(coords);
        }

        /// <summary>
        /// Try to get the tile in the given position.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public HexTile GetTile(Coords coords)
        {
            if (tiles.ContainsKey(coords))
                return tiles[coords];
            return null;
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
            TriangulateEdge(tile, world.Neighbor(tile, Direction.NORTH_EAST), Direction.NORTH_EAST);
            TriangulateEdge(tile, world.Neighbor(tile, Direction.EAST), Direction.EAST);
            TriangulateEdge(tile, world.Neighbor(tile, Direction.SOUTH_EAST), Direction.SOUTH_EAST);
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
        /// Triangulate edge between two tiles.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="dir"></param>
        void TriangulateEdge(HexTile a, HexTile b, Direction dir)
        {
            if(b != null)
                ground.AddQuad(b.vertices[dir.Opposite][1], b.vertices[dir.Opposite][0], a.vertices[dir][1], a.vertices[dir][0]);
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
