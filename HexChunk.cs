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

            TriangulateCorner(tile, world.Neighbor(tile, Direction.EAST), world.Neighbor(tile, Direction.NORTH_EAST), Direction.EAST);
            TriangulateCorner(tile, world.Neighbor(tile, Direction.SOUTH_EAST), world.Neighbor(tile, Direction.EAST), Direction.SOUTH_EAST);
        }

        /// <summary>
        /// Triangulate single sector in a tile.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="dir"></param>
        void TriangulateSector(HexTile tile, Direction dir)
        {
            Vertex a = tile.vertices[dir][0];
            Vertex b = tile.vertices[dir.Clockwise][0];
            Vertex c = tile.vertices[dir.Clockwise][1];
            Vertex d = tile.vertices[dir][1];
            Vertex e = tile.vertices[dir][4];
            Vertex f = tile.vertices[dir][6];
            Vertex g = tile.vertices[dir][2];
            Vertex h = tile.vertices[dir][3];

            Vertex edgeMid = tile.vertices[dir][5];
            Vertex riverEdgeRight = tile.vertices[dir][7];
            Vertex edgeRight = tile.vertices[dir][8];

            ground.AddTriangle(tile.center, a, b);
            ground.AddTriangle(a, c, b);
            ground.AddQuad(e, f, c, a);
            ground.AddQuad(e, a, d, g);
            ground.AddQuad(h, edgeMid, e, g);
            ground.AddQuad(edgeMid, riverEdgeRight, f, e);
            ground.AddTriangle(c, f, tile.vertices[dir.Clockwise][2]);
            ground.AddTriangle(f, riverEdgeRight, edgeRight);
            ground.AddQuad(edgeRight, tile.vertices[dir.Clockwise][3], tile.vertices[dir.Clockwise][2], f);
        }

        /// <summary>
        /// Triangulate edge between two tiles.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="dir"></param>
        void TriangulateEdge(HexTile a, HexTile b, Direction dir)
        {
            if (b == null)
                return;
            Edge edge = a.GetEdge(dir);
            HexTile upper = b;
            HexTile lower = a;
            Direction upstream = dir;
            if(b.Height < a.Height)
            {
                upper = a;
                lower = b;
                upstream = dir.Opposite;
            }

            ground.AddQuad(edge.vertices[dir.Opposite][4], edge.vertices[dir.Opposite][3], edge.vertices[dir][1], edge.vertices[dir][0]);
            ground.AddQuad(edge.vertices[dir.Opposite][1], edge.vertices[dir.Opposite][0], edge.vertices[dir][4], edge.vertices[dir][3]);

            ground.AddTriangle(edge.vertices[upstream.Opposite][3], edge.vertices[upstream.Opposite][2], edge.vertices[upstream][1]);
            ground.AddTriangle(edge.vertices[upstream.Opposite][2], edge.vertices[upstream][2], edge.vertices[upstream][1]);
            ground.AddTriangle(edge.vertices[upstream.Opposite][2], edge.vertices[upstream][3], edge.vertices[upstream][2]);
            ground.AddTriangle(edge.vertices[upstream.Opposite][2], edge.vertices[upstream.Opposite][1], edge.vertices[upstream][3]);
        }

        void TriangulateCorner(HexTile ta, HexTile tb, HexTile tc, Direction dir)
        {
            if (tb == null || tc == null)
                return;

            Edge a = ta.GetEdge(dir);
            Edge b = tb.GetEdge(dir.CounterClockwise.CounterClockwise);
            Edge c = ta.GetEdge(dir.CounterClockwise);

            ground.AddTriangle(a.vertices[dir][0], c.vertices[dir.CounterClockwise.Opposite][0], b.vertices[dir.CounterClockwise.CounterClockwise][0]);
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
