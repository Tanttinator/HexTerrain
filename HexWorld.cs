using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tanttinator.GridUtility;

namespace Tanttinator.HexTerrain
{
    /// <summary>
    /// Contains information for a single hex grid.
    /// </summary>
    public class HexWorld : MonoBehaviour
    {
        public int chunkSize = 10;
        [SerializeField] HexChunk chunkObject = default;

        public const float COS_30 = 0.8660254038f;

        public float scale = 1f;
        public float InnerRadius => scale / 2f;
        public float OuterRadius => InnerRadius / COS_30;

        public float edgeWidth = 0.6f;

        public float WidthDiff => 2 * InnerRadius + edgeWidth;
        public float WidthOffset => WidthDiff * 0.5f;
        public float HeightDiff => WidthDiff * COS_30;

        Vector2 N => new Vector2(0f, OuterRadius);
        Vector2 NE => new Vector2(InnerRadius, OuterRadius / 2f);
        Vector2 SE => new Vector2(InnerRadius, -OuterRadius / 2f);
        Vector2 S => new Vector2(0f, -OuterRadius);
        Vector2 SW => new Vector2(-InnerRadius, -OuterRadius / 2f);
        Vector2 NW => new Vector2(-InnerRadius, OuterRadius / 2f);

        Dictionary<Coords, HexChunk> chunks = new Dictionary<Coords, HexChunk>();

        public Vector2 LeftVertex(Direction dir)
        {
            if (dir == Direction.NORTH_EAST) return N;
            if (dir == Direction.EAST) return NE;
            if (dir == Direction.SOUTH_EAST) return SE;
            if (dir == Direction.SOUTH_WEST) return S;
            if (dir == Direction.WEST) return SW;
            if (dir == Direction.NORTH_WEST) return NW;
            return N;
        }

        public Vector2 RightVertex(Direction dir)
        {
            if (dir == Direction.NORTH_EAST) return NE;
            if (dir == Direction.EAST) return SE;
            if (dir == Direction.SOUTH_EAST) return S;
            if (dir == Direction.SOUTH_WEST) return SW;
            if (dir == Direction.WEST) return NW;
            if (dir == Direction.NORTH_WEST) return N;
            return N;
        }

        public Vector2 CalculateCenter(int x, int y)
        {
            return new Vector2(x * WidthDiff + (Mathf.Abs(y) % 2 == 0 ? 0 : WidthOffset), y * HeightDiff);
        }

        /// <summary>
        /// Create new chunk at the given coordinates.
        /// </summary>
        /// <param name="coords"></param>
        HexChunk CreateChunk(Coords coords)
        {
            HexChunk chunk = Instantiate(chunkObject, transform);
            chunk.SetTiles(coords, this);
            chunks[coords] = chunk;
            return chunk;
        }

        /// <summary>
        /// Get a chunk at the given position or generate a new one.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        HexChunk GetChunk(Coords coords)
        {
            if (chunks.ContainsKey(coords))
                return chunks[coords];
            else return CreateChunk(coords);
        }

        /// <summary>
        /// Try to get a tile at the given position in world coords.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public HexTile GetTile(Coords coords)
        {
            return GetChunk(CoordsToChunkCoords(coords)).GetTile(coords);
        }

        /// <summary>
        /// Convert a tile position into a chunk position.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        Coords CoordsToChunkCoords(Coords coords)
        {
            return new Coords(Mathf.FloorToInt(coords.x / chunkSize), Mathf.FloorToInt(coords.y / chunkSize));
        }
    }
}
