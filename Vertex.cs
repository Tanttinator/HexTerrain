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
        public float height;
        public Vector2 uv = new Vector2(0f, 0f);
        public Color color = Color.white;

        public virtual float BaseHeight => 0f;
        public float Height => BaseHeight + height;
        public virtual Vector2 GlobalPos => localPos;
        public Vector3 Position => new Vector3(GlobalPos.x, Height, GlobalPos.y);
        public virtual Color Color => color;

        public Vertex(Vector2 position)
        {
            localPos = position;
        }
    }

    public class TileVertex : Vertex
    {
        public HexTile tile { get; protected set; }

        public override Vector2 GlobalPos => new Vector2(tile.position.x + localPos.x, tile.position.y + localPos.y);
        public override Color Color => tile.color;
        public override float BaseHeight => tile.Height;

        public TileVertex(HexTile tile, Vector2 position) : base(position)
        {
            this.tile = tile;
        }
    }

    public class GroundVertex : TileVertex
    {
        public GroundVertex(HexTile tile, Vector2 position) : base(tile, position)
        {

        }

        public GroundVertex Offset(Vector2 pos, float height)
        {
            GroundVertex v = new GroundVertex(tile, localPos + pos);
            v.height = this.height + height;
            return v;
        }
    }

    public class WaterVertex : TileVertex
    {
        public override float BaseHeight => tile.WaterLevel;
        public WaterVertex(HexTile tile, Vector2 position) : base(tile, position)
        {

        }

        public WaterVertex(HexTile tile, Vector2 position, Vector2 uv) : base(tile, position)
        {
            this.uv = uv;
        }

        public WaterVertex(TileVertex vert) : base(vert.tile, vert.localPos)
        {

        }
    }

    public class RiverVertex : TileVertex
    {
        public override float BaseHeight => Mathf.Lerp(-tile.chunk.world.RiverDepth, 0, tile.chunk.world.riverWaterHeight) + tile.Height;

        public RiverVertex(HexTile tile, Vector2 position) : base(tile, position)
        {

        }

        public RiverVertex(TileVertex vert) : base(vert.tile, vert.localPos)
        {

        }

        public RiverVertex Clone(Vector2 uv)
        {
            RiverVertex v = new RiverVertex(this);
            v.uv = uv;
            return v;
        }

        public RiverVertex Clone(float x, float y)
        {
            return Clone(new Vector2(x, y));
        }
    }

    public class Vertices<T> : Dictionary<Direction, T[]> where T : Vertex
    {

    }
}
