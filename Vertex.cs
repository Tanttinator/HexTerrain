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
        public Vector2 localPos { get; protected set; }
        float height;
        public Vector2 uv = new Vector2(0f, 0f);

        protected HexTile tile;

        public virtual Vector2 GlobalPos => new Vector2(tile.position.x + localPos.x, tile.position.y + localPos.y);
        public virtual Vector3 Position => new Vector3(tile.position.x + localPos.x, tile.Height + height, tile.position.y + localPos.y);
        public virtual Color Color => tile.color;

        public Vertex(HexTile tile, Vector2 position)
        {
            this.tile = tile;
            this.localPos = position;
        }

        public Vertex(HexTile tile, Vector2 position, Vector2 uv)
        {
            this.tile = tile;
            this.localPos = position;
            this.uv = uv;
        }
    }

    public class WaterVertex : Vertex
    {
        public override Vector2 GlobalPos => localPos;
        public override Vector3 Position => new Vector3(localPos.x, tile.WaterLevel, localPos.y);
        public override Color Color => Color.white;
        public WaterVertex(HexTile tile, Vector2 position) : base(tile, position)
        {

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
