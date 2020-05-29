using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tanttinator.GridUtility;

namespace Tanttinator.HexTerrain
{
    public static class RiverUtil
    {

        public static void TriangulateRivers(HexTile tile)
        {
            Direction outgoingRiver = tile.outgoingRiver;
            List<Direction> incomingRivers = tile.incomingRivers;
            List<Direction> allRivers = new List<Direction>(incomingRivers);
            allRivers.Add(outgoingRiver);
            Vertex center = tile.center;
            Vertices<GroundVertex> ground = tile.ground;
            HexMesh mesh = tile.chunk.river;
            float riverDepth = tile.chunk.world.RiverDepth;

            Vertices<RiverVertex> river = tile.river;

            if (outgoingRiver != null || incomingRivers.Count > 0)
                center.height = -riverDepth;
            else
                return;

            if (incomingRivers.Count > 0)
                tile.riverCenter.uv = new Vector2(0.5f, 0.5f);
            else
                tile.riverCenter.uv = new Vector2(0.5f, 0.3f);

            Dictionary<Direction, List<Vector2>> centerUVs = new Dictionary<Direction, List<Vector2>>();
            foreach (Direction dir in Direction.directions) centerUVs[dir] = new List<Vector2>();

            if (outgoingRiver != null)
            {
                river[outgoingRiver][0].uv = new Vector2(0, 1);
                river[outgoingRiver][1].uv = new Vector2(1, 1);
                river[outgoingRiver][2].uv = new Vector2(0, 0.7f);
                river[outgoingRiver][3].uv = new Vector2(1, 0.7f);
            }

            foreach (Direction dir in incomingRivers)
            {
                if (outgoingRiver == dir.Clockwise)
                {
                    centerUVs[dir.CounterClockwise].Add(new Vector2(1f, 4f/10f));
                    centerUVs[dir.CounterClockwise.CounterClockwise].Add(new Vector2(1f, 5f/10f));
                    centerUVs[dir.Opposite].Add(new Vector2(1f, 6f/10f));
                    centerUVs[dir.Clockwise.Clockwise].Add(new Vector2(1f, 7f/10f));
                }
                if (outgoingRiver == dir.Clockwise.Clockwise)
                {
                    centerUVs[dir.CounterClockwise].Add(new Vector2(1f, 4f / 9f));
                    centerUVs[dir.CounterClockwise.CounterClockwise].Add(new Vector2(1f, 5f / 9f));
                    centerUVs[dir.Opposite].Add(new Vector2(1f, 6f / 9f));
                    centerUVs[dir.Clockwise].Add(new Vector2(0f, 4f / 7f));
                }
                if (outgoingRiver == dir.Opposite)
                {
                    centerUVs[dir.CounterClockwise].Add(new Vector2(1f, 4f / 8f));
                    centerUVs[dir.CounterClockwise.CounterClockwise].Add(new Vector2(1f, 5f / 8f));
                    centerUVs[dir.Clockwise].Add(new Vector2(0f, 4f / 8f));
                    centerUVs[dir.Clockwise.Clockwise].Add(new Vector2(0f, 5f / 8f));
                }
                if (outgoingRiver == dir.CounterClockwise)
                {
                    centerUVs[dir.Clockwise].Add(new Vector2(0f, 4f / 10f));
                    centerUVs[dir.Clockwise.Clockwise].Add(new Vector2(0f, 5f / 10f));
                    centerUVs[dir.Opposite].Add(new Vector2(0f, 6f / 10f));
                    centerUVs[dir.CounterClockwise.CounterClockwise].Add(new Vector2(0f, 7f / 10f));
                }
                else
                {
                    centerUVs[dir.Clockwise].Add(new Vector2(0f, 4f / 9f));
                    centerUVs[dir.Clockwise.Clockwise].Add(new Vector2(0f, 5f / 9f));
                    centerUVs[dir.Opposite].Add(new Vector2(0f, 6f / 9f));
                    centerUVs[dir.CounterClockwise].Add(new Vector2(1f, 4f / 7f));
                }

                river[dir][0].uv = new Vector2(1, 0);
                river[dir][1].uv = new Vector2(0, 0);
                river[dir][2].uv = new Vector2(1, 0.3f);
                river[dir][3].uv = new Vector2(0, 0.3f);
            }

            foreach(Direction dir in Direction.directions)
            {
                Vector2 uv = new Vector2(0f, 0f);
                if(outgoingRiver == dir)
                {
                    uv = new Vector2(0.5f, 0.6f);
                }
                else if(incomingRivers.Contains(dir))
                {
                    uv = new Vector2(0.5f, 0.4f);
                }
                else if(incomingRivers.Count > 0)
                {
                    Vector2 sum = new Vector2(0f, 0f);
                    int i = 0;
                    foreach (Vector2 v2 in centerUVs[dir])
                    {
                        sum += v2;
                        i++;
                    }
                    uv = sum / i;
                }
                else
                {
                    if (outgoingRiver == dir.Clockwise) uv = new Vector2(0f, 0.45f);
                    if (outgoingRiver == dir.Clockwise.Clockwise) uv = new Vector2(0f, 0.15f);
                    if (outgoingRiver == dir.Opposite) uv = new Vector2(0.5f, 0f);
                    if (outgoingRiver == dir.CounterClockwise.CounterClockwise) uv = new Vector2(1f, 0.15f);
                    if (outgoingRiver == dir.CounterClockwise) uv = new Vector2(1f, 0.45f);
                }
                river[dir][4].uv = uv;
            }

            foreach (Direction dir in Direction.directions)
            {
                if(allRivers.Contains(dir))
                {
                    ground[dir][1].height = -riverDepth;
                    ground[dir][5].height = -riverDepth;
                    ground[dir][6].height = -riverDepth;

                    mesh.AddQuad(
                        river[dir][3],
                        river[dir][2],
                        river[dir][0],
                        river[dir][1]
                        );

                    mesh.AddTriangle(
                        river[dir][2],
                        river[dir][3],
                        river[dir][4]
                        );

                    if(allRivers.Contains(dir.Clockwise))
                    {
                        mesh.AddQuad(
                            river[dir][4],
                            river[dir][3],
                            river[dir.Clockwise][2],
                            river[dir.Clockwise][4]
                            );
                    }
                    else
                    {
                        mesh.AddTriangle(
                                river[dir][4],
                                river[dir][3],
                                river[dir.Clockwise][4]
                                );
                    }

                    if(!allRivers.Contains(dir.CounterClockwise))
                    {
                        mesh.AddTriangle(
                            river[dir.CounterClockwise][4],
                            river[dir][2],
                            river[dir][4]
                            );
                    }
                }

                mesh.AddTriangle(
                    river[dir][4],
                    river[dir.Clockwise][4],
                    tile.riverCenter
                    );
            }
        }
    }
}
