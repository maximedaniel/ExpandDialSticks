using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMeshGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    MeshFilter meshFilter;
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public Transform point4;
    private enum Facing { Up, Forward, Right };
    void Start()
    {
        meshFilter = this.GetComponent<MeshFilter>();
    }
    public static Vector3[] CalculateNormals(Vector3[] v, Vector3[] p)
    {
        Vector3[] normals = new Vector3[v.Length];
        for (int i = 0; i < v.Length; i++)
		{
            Vector3 triangle = v[i];
            Vector3 side1 = p[(int)triangle.y] - p[(int)triangle.x];
            Vector3 side2 = p[(int)triangle.z] - p[(int)triangle.x];
            normals[i] = Vector3.Cross(side1, side2);
        }
        return normals;
    }

    public static Vector2[] CalculateUVs(Vector3[] v/*vertices*/, float scale)
    {
        var uvs = new Vector2[v.Length];

        for (int i = 0; i < uvs.Length; i += 3)
        {
            int i0 = i;
            int i1 = i + 1;
            int i2 = i + 2;

            Vector3 v0 = v[i0];
            Vector3 v1 = v[i1];
            Vector3 v2 = v[i2];

            Vector3 side1 = v1 - v0;
            Vector3 side2 = v2 - v0;
            var direction = Vector3.Cross(side1, side2);
            var facing = FacingDirection(direction);
            switch (facing)
            {
                case Facing.Forward:
                    uvs[i0] = ScaledUV(v0.x, v0.y, scale);
                    uvs[i1] = ScaledUV(v1.x, v1.y, scale);
                    uvs[i2] = ScaledUV(v2.x, v2.y, scale);
                    break;
                case Facing.Up:
                    uvs[i0] = ScaledUV(v0.x, v0.z, scale);
                    uvs[i1] = ScaledUV(v1.x, v1.z, scale);
                    uvs[i2] = ScaledUV(v2.x, v2.z, scale);
                    break;
                case Facing.Right:
                    uvs[i0] = ScaledUV(v0.y, v0.z, scale);
                    uvs[i1] = ScaledUV(v1.y, v1.z, scale);
                    uvs[i2] = ScaledUV(v2.y, v2.z, scale);
                    break;
            }
        }
        return uvs;
    }
    private static bool FacesThisWay(Vector3 v, Vector3 dir, Facing p, ref float maxDot, ref Facing ret)
    {
        float t = Vector3.Dot(v, dir);
        if (t > maxDot)
        {
            ret = p;
            maxDot = t;
            return true;
        }
        return false;
    }

    private static Facing FacingDirection(Vector3 v)
    {
        var ret = Facing.Up;
        float maxDot = Mathf.NegativeInfinity;

        if (!FacesThisWay(v, Vector3.right, Facing.Right, ref maxDot, ref ret))
            FacesThisWay(v, Vector3.left, Facing.Right, ref maxDot, ref ret);

        if (!FacesThisWay(v, Vector3.forward, Facing.Forward, ref maxDot, ref ret))
            FacesThisWay(v, Vector3.back, Facing.Forward, ref maxDot, ref ret);

        if (!FacesThisWay(v, Vector3.up, Facing.Up, ref maxDot, ref ret))
            FacesThisWay(v, Vector3.down, Facing.Up, ref maxDot, ref ret);

        return ret;
    }

    private static Vector2 ScaledUV(float uv1, float uv2, float scale)
    {
        return new Vector2(uv1 / scale, uv2 / scale);
    }

    private Mesh getQuadMesh(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        Mesh mesh = new Mesh();
        mesh.name = "GeneratedQuad";
        mesh.hideFlags = HideFlags.DontSave;

        Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
        Vector3[] vertices = new Vector3[8]
        {

                a-normal, //new Vector3(0, 0, 0),
				b-normal,//new Vector3(width, 0, 0),
				c-normal,//new Vector3(0, height, 0),
				d-normal,//new Vector3(width, height, 0)
				d+normal,//new Vector3(width, height, 0)
				c+normal,//new Vector3(0, height, 0),
				b+normal,//new Vector3(width, 0, 0),
                a+normal //new Vector3(0, 0, 0),
        };
        mesh.vertices = vertices;

        int[] tris = new int[36]
        {
            0, 2, 1, //face front
	        0, 3, 2,
            2, 3, 4, //face top
	        2, 4, 5,
            1, 2, 5, //face right
	        1, 5, 6,
            0, 7, 4, //face left
	        0, 4, 3,
            5, 4, 7, //face back
	        5, 7, 6,
            0, 6, 7, //face bottom
	        0, 1, 6
        };
        mesh.triangles = tris;
        Vector3[] trisVector3 = new Vector3[12]
        {
                // top face
                new Vector3(2,1,0),
                new Vector3(2,3,1),
                //bottom face
                new Vector3(7, 4, 5),
                new Vector3(7, 6, 4),
                //front face
                new Vector3(0, 5, 4),
                new Vector3(0, 1, 5),
                //back face
                new Vector3(3, 6, 7),
                new Vector3(3, 2, 6),
                //left face
                new Vector3(2, 4, 6),
                new Vector3(2, 0, 4),
                //right face
                new Vector3(1, 7, 5),
                new Vector3(1, 3, 7),
        };
        Vector2[] uv = new Vector2[8]
        {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
        };
        mesh.uv = uv;

        mesh.Optimize();
		mesh.RecalculateNormals();
        return mesh;
    }

    // Update is called once per frame
    void OnDrawGizmos()
    {
        if(point1 !=null && point2!=null && point3!=null & point4 != null)
		{
            meshFilter = this.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = getQuadMesh(point1.position, point2.position, point3.position, point4.position);
        }
    }
}
