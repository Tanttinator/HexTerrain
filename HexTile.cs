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
        public Vector2 position { get; protected set; }
        public float height { get; protected set; } = 0f;
        public Color color { get; protected set; } = Color.white;
        public Vertex center { get; protected set; }
        public Vertices vertices { get; protected set; }

        public HexTile(int x, int y, HexWorld world)
        {
            position = world.CalculateCenter(x, y);
            InitVertices(world);
        }

        /// <summary>
        /// Initialize vertex positions.
        /// </summary>
        void InitVertices(HexWorld world)
        {
            center = new Vertex(this, new Vector2(0f, 0f));
            vertices = new Vertices(center);
            foreach(Direction dir in Direction.directions)
            {
                vertices[dir][0] = new Vertex(this, world.LeftVertex(dir));
                vertices[dir][1] = new Vertex(this, world.RightVertex(dir));
            }
        }
    }
}
