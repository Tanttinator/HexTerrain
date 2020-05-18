using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tanttinator.GridUtility;

namespace Tanttinator.HexTerrain
{
    /// <summary>
    /// Represents a single point in the terrain mesh.
    /// </summary>
    public class Vertex
    {
        Vector2 position;
        float height;
        public Vector2 uv = new Vector2(0f, 0f);

        HexTile tile;

        public Vector3 Position => new Vector3(tile.position.x + position.x, tile.Height + height, tile.position.y + position.y);
        public Color Color => tile.color;

        public Vertex(HexTile tile, Vector2 position)
        {
            this.tile = tile;
            this.position = position;
        }

        public Vertex(HexTile tile, Vector2 position, Vector2 uv)
        {
            this.tile = tile;
            this.position = position;
            this.uv = uv;
        }
    }

    public class Vertices : Dictionary<Direction, Vertex[]>
    {
        public Vertices()
        {
            foreach(Direction dir in Direction.directions)
            {
                this[dir] = new Vertex[9];
            }
        }
    }
}
