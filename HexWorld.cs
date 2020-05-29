using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tanttinator.GridUtility;

namespace Tanttinator.HexTerrain
{
    /// <summary>
    /// Contains information for a single hex grid.
    /// </summary>
    [ExecuteInEditMode]
    public class HexWorld : MonoBehaviour
    {
        public int chunkSize = 10;
        [SerializeField] HexChunk chunkObject = default;

        public const float COS_30 = 0.8660254038f;
        public const float TAN_30 = 0.5773502692f;

        public float scale = 1f;
        public float heightScale = 3f;
        public float InnerRadius => scale / 2f;
        public float OuterRadius => InnerRadius / COS_30;

        public float edgeWidth = 0.6f;
        [Range(0f, 1f)]
        [SerializeField] float shoreWidthMultiplier = 0.5f;
        public float ShoreWidth => edgeWidth * shoreWidthMultiplier;
        [SerializeField] float edgeFoldWidthMultiplier = 0.2f;
        public float EdgeFoldWidth => edgeWidth * edgeFoldWidthMultiplier;

        [Range(0f, 1f)]
        [SerializeField] float riverWidthMultiplier = 0.5f;
        public float RiverWidth => OuterRadius * riverWidthMultiplier;

        [SerializeField] float riverDepthMultiplier = 20f;
        public float RiverDepth => heightScale * riverDepthMultiplier;
        public float riverWaterHeight = 0.75f;

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

        public Vector2 MiddleVertex(Direction dir)
        {
            return (LeftVertex(dir) + RightVertex(dir)) * 0.5f;
        }

        public Vector2 CalculateCenter(Coords coords)
        {
            return new Vector2(coords.x * WidthDiff + (Mathf.Abs(coords.y) % 2 == 0 ? 0 : WidthOffset), coords.y * HeightDiff);
        }

        public Vector2 LerpHeight(Vertex a, Vertex b, float height)
        {
            Vertex upper = a;
            Vertex lower = b;
            if(a.Position.y < b.Position.y)
            {
                upper = b;
                lower = a;
            }

            float t = Mathf.Clamp01((height - lower.Position.y) / (upper.Position.y - lower.Position.y));

            return Vector2.Lerp(lower.GlobalPos, upper.GlobalPos, t);
        }

        public Vector2 LeftShoreVertex(Edge a)
        {
            return a.water[a.Upstream.Opposite][1].GlobalPos + (a.ground[a.Upstream][0].GlobalPos - a.ground[a.Upstream.Opposite][4].GlobalPos).normalized * ShoreWidth;
        }

        public Vector2 RightShoreVertex(Edge a)
        {
            return a.water[a.Upstream.Opposite][0].GlobalPos + (a.ground[a.Upstream][4].GlobalPos - a.ground[a.Upstream.Opposite][0].GlobalPos).normalized * ShoreWidth;
        }

        public Vector2 ShoreVertex(Edge a, Edge b)
        {
            if (a == null || a.water == null)
            {
                return RightShoreVertex(b);
            }
            if (b == null || b.water == null)
            {
                return LeftShoreVertex(a);
            }

            Vector2 va = LeftShoreVertex(a);
            Vector2 vb = RightShoreVertex(b);

            Vector2 diff = (vb - va) / 2f;

            return va + diff;
        }

        /// <summary>
        /// Create new chunk at the given coordinates.
        /// </summary>
        /// <param name="coords"></param>
        HexChunk CreateChunk(Coords coords)
        {
            HexChunk chunk = chunks[coords] = Instantiate(chunkObject, transform);
            chunk.world = this;
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
        /// Try to get a tile at the given position in world coords or create a new one.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public HexTile GetOrCreateTile(Coords coords)
        {
            return GetChunk(CoordsToChunkCoords(coords)).GetOrCreateTile(coords);
        }

        /// <summary>
        /// Try to get the neighbor of the given tile in the given direction.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public HexTile Neighbor(HexTile tile, Direction dir)
        {
            return GetTile(tile.coords.Neighbor(dir));
        }

        /// <summary>
        /// Destroy all graphics.
        /// </summary>
        public void Clear()
        {
            foreach (HexChunk chunk in chunks.Values)
                DestroyImmediate(chunk.gameObject);
            chunks.Clear();
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
