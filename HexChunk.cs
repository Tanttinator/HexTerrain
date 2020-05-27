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

        public HexMesh ground;
        public HexMesh water;
        public HexMesh shore;
        public HexMesh river;

        [HideInInspector] public HexWorld world;

        /// <summary>
        /// Add a new tile to this chunk.
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="world"></param>
        HexTile CreateTile(Coords coords)
        {
            HexTile tile = tiles[coords] = new HexTile(coords, this);
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
            water.Clear();
            shore.Clear();
            river.Clear();

            foreach (HexTile tile in tiles.Values)
                tile.Triangulate();

            foreach(HexTile tile in tiles.Values)
            {
                tile.GetEdge(Direction.NORTH_EAST).Triangulate();
                tile.GetEdge(Direction.EAST).Triangulate();
                tile.GetEdge(Direction.SOUTH_EAST).Triangulate();

                TriangulateCorner(tile, Direction.NORTH_EAST);
                TriangulateCorner(tile, Direction.EAST);
            }

            ground.Apply();
            water.Apply();
            shore.Apply();
            river.Apply();
        }

        void TriangulateCorner(HexTile a, Direction dir)
        {
            HexTile b = world.Neighbor(a, dir);
            HexTile c = world.Neighbor(a, dir.Clockwise);

            if (b == null || c == null)
                return;

            TriangulateCornerGround(a, b, c, dir);
            TriangulateCornerWater(a, b, c, dir);
        }

        void TriangulateCornerGround(HexTile a, HexTile b, HexTile c, Direction dir)
        {
            ground.AddTriangle(a.ground[dir][8], b.ground[dir.Opposite.CounterClockwise][8], c.ground[dir.Clockwise.Opposite][8]);
        }

        void TriangulateCornerWater(HexTile a, HexTile b, HexTile c, Direction dir)
        {
            if (a.Underwater)
            {
                if (b.Underwater && c.Underwater)
                {
                    Edge right = a.GetEdge(dir.Clockwise);
                    Edge left = a.GetEdge(dir);
                    water.AddTriangle(right.water[dir.Clockwise.Opposite][1], right.water[dir.Clockwise][0], left.water[dir.Opposite][0]);
                }
                if (!b.Underwater && c.Underwater)
                {
                    TriangulateShoreCornerWWL(c.GetEdge(dir.CounterClockwise), a.GetEdge(dir));
                }
                if (b.Underwater && !c.Underwater)
                {
                    TriangulateShoreCornerWWL(a.GetEdge(dir.Clockwise), b.GetEdge(dir.Clockwise.Clockwise));
                }
                if (!b.Underwater && !c.Underwater)
                {
                    TriangulateShoreCornerWLL(a.GetEdge(dir.Clockwise), a.GetEdge(dir));
                }
            }
            else if (b.Underwater)
                TriangulateCornerWater(b, c, a, dir.Clockwise.Clockwise);
            else if (c.Underwater)
                TriangulateCornerWater(c, a, b, dir.CounterClockwise.CounterClockwise);
        }

        void TriangulateShoreCornerWWL(Edge a, Edge b)
        {
            if (a.water == null || b.water == null)
                return;
            shore.AddQuad(
                b.water[b.Upstream.Opposite][0],
                a.water[a.Upstream.Opposite][1],
                a.water[a.Upstream][0],
                b.water[b.Upstream][1]
                );
        }

        void TriangulateShoreCornerWLL(Edge a, Edge b)
        {
            if (a.water == null || b.water == null)
                return;
            Vertex bottom = a.water[a.Upstream][0];
            bottom.uv = new Vector2(0.5f, 0f);
            Vertex left = b.water[b.Upstream.Opposite][0];
            left.uv = new Vector2(0f, 1f);
            Vertex right = a.water[a.Upstream.Opposite][1];
            right.uv = new Vector2(1f, 1f);
            shore.AddTriangle(bottom, left, right);
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
