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
        public GroundVertex center { get; protected set; }
        public Vertices<GroundVertex> ground { get; protected set; }
        public Vertices<RiverVertex> river { get; protected set; }
        public RiverVertex riverCenter { get; protected set; }
        public WaterVertex waterCenter { get; protected set; }

        public Direction outgoingRiver { get; protected set; }
        public List<Direction> incomingRivers { get; protected set; } = new List<Direction>();

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
            center = new GroundVertex(this, Random.insideUnitCircle / 10f);
            ground = new Vertices<GroundVertex>();
            river = new Vertices<RiverVertex>();
            riverCenter = new RiverVertex(center);
            waterCenter = new WaterVertex(center);
            foreach(Direction dir in Direction.directions)
            {
                float centerHexOuterRadius = world.RiverWidth / HexWorld.COS_30 * 0.5f;

                Vector2 edgeLeft = world.LeftVertex(dir);
                Vector2 edgeMid = world.MiddleVertex(dir);
                Vector2 edgeRight = world.RightVertex(dir);

                Vector2 riverEdgeLeft = edgeMid + (edgeLeft - edgeRight).normalized * world.RiverWidth * 0.5f;
                Vector2 riverEdgeRight = edgeMid + (edgeRight - edgeLeft).normalized * world.RiverWidth * 0.5f;

                Vector2 a = edgeMid.normalized * centerHexOuterRadius + center.localPos;
                Vector2 b = edgeLeft.normalized * world.RiverWidth + center.localPos;
                Vector2 c = b + (edgeRight.normalized * world.RiverWidth + center.localPos - b) * 0.5f;

                ground[dir] = new GroundVertex[] {
                    new GroundVertex(this, riverEdgeLeft),
                    new GroundVertex(this, edgeMid),
                    new GroundVertex(this, riverEdgeRight),
                    new GroundVertex(this, edgeRight),
                    new GroundVertex(this, b),
                    new GroundVertex(this, c),
                    new GroundVertex(this, a)
                };
            }

            foreach (Direction dir in Direction.directions)
            {
                river[dir] = new RiverVertex[]{
                    new RiverVertex(ground[dir][0]),
                    new RiverVertex(ground[dir][2]),
                    new RiverVertex(ground[dir][4]),
                    new RiverVertex(ground[dir.Clockwise][4]),
                    new RiverVertex(ground[dir][6])
                };
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
                Vertex b = ground[dir][1];
                Vertex c = ground[dir][2];
                Vertex d = ground[dir][3];
                Vertex e = ground[dir][4];
                Vertex f = ground[dir][5];
                Vertex g = ground[dir][6];
                Vertex h = ground[dir.Clockwise][0];
                Vertex i = ground[dir.Clockwise][4];
                Vertex j = ground[dir.Clockwise][6];

                chunk.ground.AddTriangle(center, g, j);
                chunk.ground.AddTriangle(g, i, j);
                chunk.ground.AddTriangle(g, f, i);
                chunk.ground.AddTriangle(g, e, f);

                chunk.ground.AddQuad(a, b, f, e);
                chunk.ground.AddQuad(b, c, i, f);
                chunk.ground.AddQuad(c, d, h, i);
            }
        }

        void TriangulateWater()
        {
            foreach(Direction dir in Direction.directions)
            {
                Edge edge = GetEdge(dir);
                Edge rightEdge = GetEdge(dir.Clockwise);
                Edge leftEdge = GetEdge(dir.CounterClockwise);

                WaterVertex left = new WaterVertex(ground[dir.CounterClockwise][3]);
                WaterVertex right = new WaterVertex(ground[dir][3]);

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
