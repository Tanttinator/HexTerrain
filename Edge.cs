using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tanttinator.GridUtility;

namespace Tanttinator.HexTerrain {
    public class Edge
    {
        public Vertices vertices { get; protected set; }
        HexTile a;
        HexTile b;
        Direction dir;

        HexWorld world;

        public Edge(HexTile a, Direction dir, HexWorld world)
        {
            this.a = a;
            this.dir = dir;
            this.world = world;
            vertices = new Vertices();
        }

        public void Refresh()
        {
            b = world.Neighbor(a, dir);

            if (b == null)
                return;

            vertices[dir] = new Vertex[]
            {
                a.vertices[dir.CounterClockwise][8],
                a.vertices[dir][3],
                a.vertices[dir][5],
                a.vertices[dir][7],
                a.vertices[dir][8]
            };

            vertices[dir.Opposite] = new Vertex[]
            {
                b.vertices[dir.Opposite.CounterClockwise][8],
                b.vertices[dir.Opposite][3],
                b.vertices[dir.Opposite][5],
                b.vertices[dir.Opposite][7],
                b.vertices[dir.Opposite][8]
            };
        }
    }
}
