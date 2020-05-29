using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tanttinator.GridUtility;

namespace Tanttinator.HexTerrain {
    public class Edge
    {
        public Vertices<Vertex> ground { get; protected set; }
        public Vertices<WaterVertex> water { get; protected set; }
        public WaterVertex[] shore;
        Vertex[] river;

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

        Vector2 Lerp(float height)
        {
            if (height == Lower.Height) return new Vector2(0f, 0f);
            Vector2 dir = (Upper.position - Lower.position).normalized;
            float depth = 0f;

            float t = Mathf.InverseLerp(Lower.Height, Upper.Height, height);

            if (t < chunk.world.edgeFoldAngle) depth = Mathf.Lerp(0f, chunk.world.EdgeFoldWidth, Mathf.InverseLerp(0f, chunk.world.edgeFoldAngle, t));
            else if (t < 1f - chunk.world.edgeFoldAngle) depth = Mathf.Lerp(chunk.world.EdgeFoldWidth, chunk.world.edgeWidth - chunk.world.EdgeFoldWidth, Mathf.InverseLerp(chunk.world.edgeFoldAngle, 1f - chunk.world.edgeFoldAngle, t));
            else depth = Mathf.Lerp(chunk.world.edgeWidth - chunk.world.EdgeFoldWidth, chunk.world.edgeWidth, Mathf.InverseLerp(1f - chunk.world.edgeFoldAngle, 1f, t));

            return dir * depth;
        }

        ChildVertex FoldVertex(TileVertex vert, bool opposite = false)
        {
            Vector3 foldOffset = new Vector3(b.position.x - a.position.x, 0f, b.position.y - a.position.y).normalized * chunk.world.EdgeFoldWidth;
            Vector3 axis = a.ground[dir][2].Position - a.ground[dir][3].Position;
            float angle = Mathf.Atan2((b.Height - a.Height) * chunk.world.edgeFoldAngle, chunk.world.EdgeFoldWidth) * Mathf.Rad2Deg;
            foldOffset = Quaternion.AngleAxis(angle, axis) * foldOffset * (opposite ? -1 : 1);

            ChildVertex v = new ChildVertex(vert, foldOffset);
            return v;
        }

        ChildVertex FoldVertex(TileVertex vert, Vector2 uv, bool opposite = false)
        {
            ChildVertex v = FoldVertex(vert, opposite);
            v.uv = uv;
            return v;
        }

        public void Refresh()
        {
            b = chunk.world.Neighbor(a, dir);

            if (b == null)
                return;

            ground = new Vertices<Vertex>();

            ground[dir] = new Vertex[]
            {
                a.ground[dir.CounterClockwise][3],
                a.ground[dir][0],
                a.ground[dir][1],
                a.ground[dir][2],
                a.ground[dir][3],
                FoldVertex(a.ground[dir.CounterClockwise][3]),
                FoldVertex(a.ground[dir][0]),
                FoldVertex(a.ground[dir][1]),
                FoldVertex(a.ground[dir][2]),
                FoldVertex(a.ground[dir][3])
            };

            ground[dir.Opposite] = new Vertex[]
            {
                b.ground[dir.Opposite.CounterClockwise][3],
                b.ground[dir.Opposite][0],
                b.ground[dir.Opposite][1],
                b.ground[dir.Opposite][2],
                b.ground[dir.Opposite][3],
                FoldVertex(b.ground[dir.Opposite.CounterClockwise][3], true),
                FoldVertex(b.ground[dir.Opposite][0], true),
                FoldVertex(b.ground[dir.Opposite][1], true),
                FoldVertex(b.ground[dir.Opposite][2], true),
                FoldVertex(b.ground[dir.Opposite][3], true)
            };

            river = new Vertex[]
            {
                FoldVertex(Lower.river[Upstream][1], new Vector2(0f, 0.9f), b == Lower),
                FoldVertex(Lower.river[Upstream][0], new Vector2(1f, 0.9f), b == Lower),
                FoldVertex(Upper.river[Upstream.Opposite][1], new Vector2(1f, 0.1f), b == Upper),
                FoldVertex(Upper.river[Upstream.Opposite][0], new Vector2(0f, 0.1f), b == Upper)
                
            };

            water = new Vertices<WaterVertex>();

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
                    water[Upstream] = new WaterVertex[]
                    {
                        new WaterVertex(Lower, (bottomLeft == null || bottomLeft.Type != EdgeType.SHORE? ground[Upstream][0].GlobalPos : bottomLeft.water[Upstream.CounterClockwise][1].GlobalPos) - Lower.position),
                        new WaterVertex(Lower, (bottomRight == null || bottomRight.Type != EdgeType.SHORE? ground[Upstream][4].GlobalPos : bottomRight.water[Upstream.Clockwise][0].GlobalPos) - Lower.position)
                    };

                    water[Upstream.Opposite] = new WaterVertex[]
                    {
                        new WaterVertex(Upper, (topRight == null || topRight.Type != EdgeType.SHORE? ground[Upstream.Opposite][0].GlobalPos : topRight.water[Upstream.Opposite.CounterClockwise][1].GlobalPos) - Upper.position),
                        new WaterVertex(Upper, (topLeft == null || topLeft.Type != EdgeType.SHORE? ground[Upstream.Opposite][4].GlobalPos : topLeft.water[Upstream.Opposite.Clockwise][0].GlobalPos) - Upper.position)
                    };
                    break;
                case EdgeType.SHORE:
                    water[Upstream.Opposite] = new WaterVertex[]
                    {
                        new WaterVertex(Lower, ground[Upstream][4].GlobalPos + Lerp(Lower.WaterLevel) - Lower.position, new Vector2(1f, 1f)),
                        new WaterVertex(Lower, ground[Upstream][0].GlobalPos + Lerp(Lower.WaterLevel) - Lower.position, new Vector2(0f, 1f))
                    };

                    water[Upstream] = new WaterVertex[]
                    {
                        new WaterVertex(Lower, (bottomLeft == null || bottomLeft.Type != EdgeType.SHORE? chunk.world.LeftShoreVertex(this) : chunk.world.ShoreVertex(this, bottomLeft)) - Lower.position, new Vector2(0f, 0f)),
                        new WaterVertex(Lower, (bottomRight == null || bottomRight.Type != EdgeType.SHORE? chunk.world.RightShoreVertex(this) : chunk.world.ShoreVertex(bottomRight, this)) - Lower.position, new Vector2(1f, 0f))
                    };

                    shore = new WaterVertex[]
                    {
                        new WaterVertex(Lower, ground[Upstream][1].GlobalPos + Lerp(Lower.WaterLevel) - Lower.position),
                        new WaterVertex(Lower, ground[Upstream][3].GlobalPos + Lerp(Lower.WaterLevel) - Lower.position)
                    };
                    shore[0].uv = new Vector2(1f, 1f);
                    shore[1].uv = new Vector2(0f, 1f);

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

            chunk.ground.AddQuad(ground[Upstream.Opposite][5], ground[Upstream.Opposite][6], ground[Upstream.Opposite][1], ground[Upstream.Opposite][0]);
            chunk.ground.AddQuad(ground[Upstream.Opposite][6], ground[Upstream.Opposite][7], ground[Upstream.Opposite][2], ground[Upstream.Opposite][1]);
            chunk.ground.AddQuad(ground[Upstream.Opposite][7], ground[Upstream.Opposite][8], ground[Upstream.Opposite][3], ground[Upstream.Opposite][2]);
            chunk.ground.AddQuad(ground[Upstream.Opposite][8], ground[Upstream.Opposite][9], ground[Upstream.Opposite][4], ground[Upstream.Opposite][3]);

            chunk.ground.AddQuad(ground[Upstream][5], ground[Upstream][6], ground[Upstream][1], ground[Upstream][0]);
            chunk.ground.AddQuad(ground[Upstream][6], ground[Upstream][7], ground[Upstream][2], ground[Upstream][1]);
            chunk.ground.AddQuad(ground[Upstream][7], ground[Upstream][8], ground[Upstream][3], ground[Upstream][2]);
            chunk.ground.AddQuad(ground[Upstream][8], ground[Upstream][9], ground[Upstream][4], ground[Upstream][3]);

            chunk.ground.AddQuad(ground[dir.Opposite][9], ground[dir.Opposite][8], ground[dir][6], ground[dir][5]);
            chunk.ground.AddQuad(ground[dir.Opposite][6], ground[dir.Opposite][5], ground[dir][9], ground[dir][8]);

            chunk.ground.AddTriangle(ground[Upstream.Opposite][8], ground[Upstream.Opposite][7], ground[Upstream][6]);
            chunk.ground.AddTriangle(ground[Upstream.Opposite][7], ground[Upstream][7], ground[Upstream][6]);
            chunk.ground.AddTriangle(ground[Upstream.Opposite][7], ground[Upstream][8], ground[Upstream][7]);
            chunk.ground.AddTriangle(ground[Upstream.Opposite][7], ground[Upstream.Opposite][6], ground[Upstream][8]);

            if (Upper.outgoingRiver == Upstream.Opposite)
            {
                chunk.river.AddQuad(river[3], river[2], Upper.river[Upstream.Opposite][1].Clone(1f, 0f), Upper.river[Upstream.Opposite][0].Clone(0f, 0f));
                if(Lower.incomingRivers.Contains(Upstream))
                {
                    chunk.river.AddQuad(Lower.river[Upstream][1].Clone(0f, 1f), Lower.river[Upstream][0].Clone(1f, 1f), river[1], river[0]);
                    chunk.river.AddQuad(river[0], river[1], river[2], river[3]);
                }
                else if(Lower.Underwater)
                {
                    chunk.river.AddQuad(shore[1], shore[0], river[2], river[3]);
                }
            }
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
