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
        float height = 0f;
        public float Height => height;
        public Color color { get; protected set; } = Color.white;
        public Vertex center { get; protected set; }
        public Vertices vertices { get; protected set; }

        public HexChunk chunk { get; protected set; }

        public HexTile(Coords coords, HexChunk chunk, HexWorld world)
        {
            position = world.CalculateCenter(coords);
            this.chunk = chunk;
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

        void Refresh()
        {
            chunk.Refresh();
        }
    }
}
