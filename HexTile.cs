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
        float waterLevel = 0f;
        public float WaterLevel => waterLevel * chunk.world.heightScale;
        public bool Underwater => waterLevel > height;
        public Vertex center { get; protected set; }
        public Vertices ground { get; protected set; }
        public Vertices river { get; protected set; }
        public Vertex riverCenter { get; protected set; }
        public Vertex waterCenter { get; protected set; }

        public Direction outgoingRiver { get; protected set; }
        public List<Direction> incomingRivers { get; protected set; } = new List<Direction>();
        bool HasRiver => outgoingRiver != null || incomingRivers.Count > 0;

        public HexChunk chunk { get; protected set; }

        Dictionary<Direction, Edge> edges = new Dictionary<Direction, Edge>();

        public HexTile(Coords coords, HexChunk chunk)
        {
            this.coords = coords;
            position = chunk.world.CalculateCenter(coords);
            this.chunk = chunk;
            InitVertices(chunk.world);
            edges[Direction.NORTH_EAST] = new Edge(this, Direction.NORTH_EAST, chunk);
            edges[Direction.EAST] = new Edge(this, Direction.EAST, chunk);
            edges[Direction.SOUTH_EAST] = new Edge(this, Direction.SOUTH_EAST, chunk);
            Refresh();
        }

        /// <summary>
        /// Initialize vertex positions.
        /// </summary>
        void InitVertices(HexWorld world)
        {
            center = new Vertex(this, new Vector2(0f, 0f));
            ground = new Vertices();
            river = new Vertices();
            riverCenter = center.Clone(Mathf.Lerp(-chunk.world.RiverDepth, 0, chunk.world.riverWaterHeight));
            waterCenter = new WaterVertex(this, position);
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
                Vector2 c = (riverEdgeLeft - b) * 0.5f + b;

                Vector2 e = edgeMid + (c - riverEdgeLeft);
                Vector2 f = riverEdgeRight + (c - riverEdgeLeft);

                ground[dir][0] = new Vertex(this, a);
                ground[dir][1] = new Vertex(this, b);
                ground[dir][2] = new Vertex(this, c);
                ground[dir][3] = new Vertex(this, riverEdgeLeft);
                ground[dir][4] = new Vertex(this, e);
                ground[dir][5] = new Vertex(this, edgeMid);
                ground[dir][6] = new Vertex(this, f);
                ground[dir][7] = new Vertex(this, riverEdgeRight);
                ground[dir][8] = new Vertex(this, edgeRight);
            }

            foreach (Direction dir in Direction.directions)
            {
                river[dir][0] = ground[dir][3].Clone(Mathf.Lerp(-chunk.world.RiverDepth, 0, chunk.world.riverWaterHeight));
                river[dir][1] = ground[dir][7].Clone(Mathf.Lerp(-chunk.world.RiverDepth, 0, chunk.world.riverWaterHeight));
                river[dir][2] = ground[dir][1].Clone(Mathf.Lerp(-chunk.world.RiverDepth, 0, chunk.world.riverWaterHeight));
                river[dir][3] = ground[dir.Clockwise][1].Clone(Mathf.Lerp(-chunk.world.RiverDepth, 0, chunk.world.riverWaterHeight));
                river[dir][4] = ground[dir][0].Clone(Mathf.Lerp(-chunk.world.RiverDepth, 0, chunk.world.riverWaterHeight));
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

        public void SetWaterLevel(float waterLevel)
        {
            this.waterLevel = waterLevel;
            Refresh();
        }

        public void AddOutgoingRiver(Direction dir)
        {
            outgoingRiver = dir;
            Refresh();
        }

        public void AddIncomingRiver(Direction dir)
        {
            if (incomingRivers.Contains(dir))
                return;
            incomingRivers.Add(dir);
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

        public void Triangulate()
        {
            TriangulateGround();
            if (Underwater)
                TriangulateWater();
        }

        void TriangulateGround()
        {
            RiverUtil.TriangulateRivers(this);
            foreach(Direction dir in Direction.directions)
            {
                Vertex a = ground[dir][0];
                Vertex b = ground[dir.Clockwise][0];
                Vertex c = ground[dir.Clockwise][1];
                Vertex d = ground[dir][1];
                Vertex e = ground[dir][4];
                Vertex f = ground[dir][6];
                Vertex g = ground[dir][2];
                Vertex h = ground[dir][3];

                Vertex edgeMid = ground[dir][5];
                Vertex riverEdgeRight = ground[dir][7];
                Vertex edgeRight = ground[dir][8];

                chunk.ground.AddTriangle(center, a, b);
                chunk.ground.AddTriangle(a, c, b);
                chunk.ground.AddQuad(e, f, c, a);
                chunk.ground.AddQuad(e, a, d, g);
                chunk.ground.AddQuad(h, edgeMid, e, g);
                chunk.ground.AddQuad(edgeMid, riverEdgeRight, f, e);
                chunk.ground.AddTriangle(c, f, ground[dir.Clockwise][2]);
                chunk.ground.AddTriangle(f, riverEdgeRight, edgeRight);
                chunk.ground.AddQuad(edgeRight, ground[dir.Clockwise][3], ground[dir.Clockwise][2], f);  
            }
        }

        void TriangulateWater()
        {
            foreach(Direction dir in Direction.directions)
            {
                Edge edge = GetEdge(dir);
                Edge rightEdge = GetEdge(dir.Clockwise);
                Edge leftEdge = GetEdge(dir.CounterClockwise);

                Vertex left = new WaterVertex(this, ground[dir.CounterClockwise][8].GlobalPos);
                Vertex right = new WaterVertex(this, ground[dir][8].GlobalPos);

                if (rightEdge != null && rightEdge.water != null) right = rightEdge.water[dir.Clockwise][0];
                if (leftEdge != null && leftEdge.water != null) left = leftEdge.water[dir.CounterClockwise][1];
                if(edge != null && edge.water != null)
                {
                    left = edge.water[dir][0];
                    right = edge.water[dir][1];
                }

                chunk.water.AddTriangle(waterCenter, left, right);
            }
        }
    }
}
