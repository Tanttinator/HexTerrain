using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tanttinator.GridUtility;

namespace Tanttinator.HexTerrain
{
    /// <summary>
    /// Represents a tile in the hex grid.
    /// </summary>
    public class HexTile
    {
        public Coords coords { get; protected set; }
        public Vector2 position { get; protected set; }
        float height = 0f;
        public float Height => height * chunk.world.heightScale;
        public Color color { get; protected set; } = Color.white;
        public Vertex center { get; protected set; }
        public Vertices vertices { get; protected set; }

        public HexChunk chunk { get; protected set; }

        Dictionary<Direction, Edge> edges = new Dictionary<Direction, Edge>();

        public HexTile(Coords coords, HexChunk chunk, HexWorld world)
        {
            this.coords = coords;
            position = world.CalculateCenter(coords);
            this.chunk = chunk;
            InitVertices(world);
            edges[Direction.NORTH_EAST] = new Edge(this, Direction.NORTH_EAST, world);
            edges[Direction.EAST] = new Edge(this, Direction.EAST, world);
            edges[Direction.SOUTH_EAST] = new Edge(this, Direction.SOUTH_EAST, world);
            Refresh();
        }

        /// <summary>
        /// Initialize vertex positions.
        /// </summary>
        void InitVertices(HexWorld world)
        {
            center = new Vertex(this, new Vector2(0f, 0f));
            vertices = new Vertices();
            foreach(Direction dir in Direction.directions)
            {
                float centerHexOuterRadius = world.RiverWidth / HexWorld.COS_30 * 0.5f;

                Vector2 edgeLeft = world.LeftVertex(dir);
                Vector2 edgeMid = world.MiddleVertex(dir);
                Vector2 edgeRight = world.RightVertex(dir);

                Vector2 riverEdgeLeft = edgeMid + (edgeLeft - edgeRight).normalized * world.RiverWidth * 0.5f;
                Vector2 riverEdgeRight = edgeMid + (edgeRight - edgeLeft).normalized * world.RiverWidth * 0.5f;

                Vector2 a = edgeMid.normalized * centerHexOuterRadius;
                Vector2 b = edgeLeft.normalized * world.RiverWidth;
                Vector2 c = riverEdgeLeft - a;

                Vector2 e = edgeMid + (c - riverEdgeLeft);
                Vector2 f = riverEdgeRight + (c - riverEdgeLeft);

                vertices[dir][0] = new Vertex(this, a);
                vertices[dir][1] = new Vertex(this, b);
                vertices[dir][2] = new Vertex(this, c);
                vertices[dir][3] = new Vertex(this, riverEdgeLeft);
                vertices[dir][4] = new Vertex(this, e);
                vertices[dir][5] = new Vertex(this, edgeMid);
                vertices[dir][6] = new Vertex(this, f);
                vertices[dir][7] = new Vertex(this, riverEdgeRight);
                vertices[dir][8] = new Vertex(this, edgeRight);
            }
        }

        public void SetHeight(float height)
        {
            this.height = height;
            Refresh();
        }

        public void SetColor(Color color)
        {
            this.color = color;
            Refresh();
        }

        public Edge GetEdge(Direction dir)
        {
            if (dir == Direction.NORTH_EAST || dir == Direction.EAST || dir == Direction.SOUTH_EAST)
                return edges[dir];
            else
            {
                HexTile neighbor = chunk.world.Neighbor(this, dir);

                if (neighbor == null)
                    return null;

                return neighbor.edges[dir.Opposite];
            }
        }

        void Refresh()
        {
            RefreshSelf();
            foreach (Direction dir in Direction.directions)
                chunk.world.Neighbor(this, dir)?.RefreshSelf();
        }

        void RefreshSelf()
        {
            foreach (Edge edge in edges.Values)
                edge.Refresh();
            chunk.Refresh();
        }
    }
}
