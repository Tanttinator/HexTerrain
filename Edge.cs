using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tanttinator.GridUtility;

namespace Tanttinator.HexTerrain {
    public class Edge
    {
        public Vertices ground { get; protected set; }
        public Vertices water { get; protected set; }
        public Vertex[] shore;

        HexTile a;
        HexTile b;
        Direction dir;

        public HexTile Lower
        {
            get
            {
                if (b == null) return a;
                if (a.Height > b.Height) return b;
                return a;
            }
        }
        public HexTile Upper
        {
            get
            {
                if (b == null) return null;
                if (a.Height > b.Height) return a;
                return b;
            }
        }
        public Direction Upstream
        {
            get
            {
                if (b == null) return null;
                if (a.Height > b.Height) return dir.Opposite;
                return dir;
            }
        }
        public EdgeType Type
        {
            get
            {
                if (b == null) return EdgeType.WATER;
                if (a.Underwater && b.Underwater) return EdgeType.WATER;
                if (a.Underwater != b.Underwater) return EdgeType.SHORE;
                return EdgeType.LAND;
            }
        }

        HexChunk chunk;

        public Edge(HexTile a, Direction dir, HexChunk chunk)
        {
            this.a = a;
            this.dir = dir;
            this.chunk = chunk;
        }

        public void Refresh()
        {
            b = chunk.world.Neighbor(a, dir);

            if (b == null)
                return;

            ground = new Vertices();

            ground[dir] = new Vertex[]
            {
                a.ground[dir.CounterClockwise][8],
                a.ground[dir][3],
                a.ground[dir][5],
                a.ground[dir][7],
                a.ground[dir][8]
            };

            ground[dir.Opposite] = new Vertex[]
            {
                b.ground[dir.Opposite.CounterClockwise][8],
                b.ground[dir.Opposite][3],
                b.ground[dir.Opposite][5],
                b.ground[dir.Opposite][7],
                b.ground[dir.Opposite][8]
            };

            water = new Vertices();

            RefreshWater(true);
        }

        public void RefreshWater(bool refreshNeighbors)
        {
            if (water == null)
                return;

            Edge bottomLeft = Lower.GetEdge(Upstream.CounterClockwise);
            Edge topLeft = Upper.GetEdge(Upstream.Opposite.Clockwise);

            Edge bottomRight = Lower.GetEdge(Upstream.Clockwise);
            Edge topRight = Upper.GetEdge(Upstream.Opposite.CounterClockwise);

            switch(Type)
            {
                case EdgeType.LAND:
                case EdgeType.WATER:
                    water[Upstream] = new Vertex[]
                    {
                        new WaterVertex(Lower, (bottomLeft == null || bottomLeft.Type != EdgeType.SHORE? ground[Upstream][0].GlobalPos : bottomLeft.water[Upstream.CounterClockwise][1].GlobalPos)),
                        new WaterVertex(Lower, (bottomRight == null || bottomRight.Type != EdgeType.SHORE? ground[Upstream][4].GlobalPos : bottomRight.water[Upstream.Clockwise][0].GlobalPos))
                    };

                    water[Upstream.Opposite] = new Vertex[]
                    {
                        new WaterVertex(Upper, (topRight == null || topRight.Type != EdgeType.SHORE? ground[Upstream.Opposite][0].GlobalPos : topRight.water[Upstream.Opposite.CounterClockwise][1].GlobalPos)),
                        new WaterVertex(Upper, (topLeft == null || topLeft.Type != EdgeType.SHORE? ground[Upstream.Opposite][4].GlobalPos : topLeft.water[Upstream.Opposite.Clockwise][0].GlobalPos))
                    };
                    break;
                case EdgeType.SHORE:
                    water[Upstream.Opposite] = new Vertex[]
                    {
                        new WaterVertex(Lower, chunk.world.LerpHeight(ground[Upstream][4], ground[Upstream.Opposite][0], Lower.WaterLevel)),
                        new WaterVertex(Lower, chunk.world.LerpHeight(ground[Upstream][0], ground[Upstream.Opposite][4], Lower.WaterLevel))
                    };

                    water[Upstream] = new Vertex[]
                    {
                        new WaterVertex(Lower, (bottomLeft == null || bottomLeft.Type != EdgeType.SHORE? chunk.world.LeftShoreVertex(this) : chunk.world.ShoreVertex(this, bottomLeft))),
                        new WaterVertex(Lower, (bottomRight == null || bottomRight.Type != EdgeType.SHORE? chunk.world.RightShoreVertex(this) : chunk.world.ShoreVertex(bottomRight, this)))
                    };

                    shore = new Vertex[]
                    {
                        new WaterVertex(Lower, chunk.world.LerpHeight(ground[Upstream][1], ground[Upstream.Opposite][3], Lower.WaterLevel)),
                        new WaterVertex(Lower, chunk.world.LerpHeight(ground[Upstream][3], ground[Upstream.Opposite][1], Lower.WaterLevel))
                    };

                    if (refreshNeighbors)
                    {
                        if (bottomLeft != null)
                            bottomLeft.RefreshWater(false);
                        if (bottomRight != null)
                            bottomRight.RefreshWater(false);
                    }
                    break;
            }
        }

        public void Triangulate()
        {
            TriangulateGround();
            if (Type == EdgeType.SHORE) TriangulateShore();
            if (Type == EdgeType.WATER) TriangulateWater();
        }

        void TriangulateGround()
        {
            HexTile b = chunk.world.Neighbor(a, dir);
            if (b == null)
                return;

            chunk.ground.AddQuad(ground[dir.Opposite][4], ground[dir.Opposite][3], ground[dir][1], ground[dir][0]);
            chunk.ground.AddQuad(ground[dir.Opposite][1], ground[dir.Opposite][0], ground[dir][4], ground[dir][3]);

            chunk.ground.AddTriangle(ground[Upstream.Opposite][3], ground[Upstream.Opposite][2], ground[Upstream][1]);
            chunk.ground.AddTriangle(ground[Upstream.Opposite][2], ground[Upstream][2], ground[Upstream][1]);
            chunk.ground.AddTriangle(ground[Upstream.Opposite][2], ground[Upstream][3], ground[Upstream][2]);
            chunk.ground.AddTriangle(ground[Upstream.Opposite][2], ground[Upstream.Opposite][1], ground[Upstream][3]);
        }

        void TriangulateShore()
        {
            if (b == null || a.Underwater == b.Underwater)
                return;

            chunk.shore.AddQuad(water[Upstream.Opposite][1], water[Upstream.Opposite][0], water[Upstream][1], water[Upstream][0]);
        }

        void TriangulateWater()
        {
            if (b == null)
                return;
            if (a.Underwater && b.Underwater)
                chunk.water.AddQuad(water[Upstream.Opposite][1], water[Upstream.Opposite][0], water[Upstream][1], water[Upstream][0]);
        }
    }

    public enum EdgeType
    {
        WATER, SHORE, LAND
    }
}
