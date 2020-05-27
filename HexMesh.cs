using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tanttinator.HexTerrain
{
    [ExecuteInEditMode]
    public class HexMesh : MonoBehaviour
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();
        List<Vector2> uvs = new List<Vector2>();

        MeshFilter mf;
        MeshFilter MF
        {
            get
            {
                if(mf == null) mf = GetComponent<MeshFilter>();
                return mf;
            }
        }

        /// <summary>
        /// Reset this mesh.
        /// </summary>
        public void Clear()
        {
            vertices.Clear();
            triangles.Clear();
            colors.Clear();
            uvs.Clear();
        }

        /// <summary>
        /// Recreate this mesh.
        /// </summary>
        public void Apply()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.colors = colors.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();

            MF.mesh = mesh;
        }

        int AddVertex(Vertex vertex)
        {
            vertices.Add(vertex.Position);
            colors.Add(vertex.Color);
            uvs.Add(vertex.uv);
            return vertices.Count - 1;
        }

        public void AddTriangle(Vertex a, Vertex b, Vertex c)
        {
            triangles.Add(AddVertex(a));
            triangles.Add(AddVertex(b));
            triangles.Add(AddVertex(c));
        }

        public void AddQuad(Vertex a, Vertex b, Vertex c, Vertex d, bool generateUVs = true)
        {
            if (generateUVs)
            {
                a.uv = new Vector2(0f, 1f);
                b.uv = new Vector2(1f, 1f);
                c.uv = new Vector2(1f, 0f);
                d.uv = new Vector2(0f, 0f);
            }
            AddTriangle(a, b, c);
            AddTriangle(a, c, d);
        }
    }
}
