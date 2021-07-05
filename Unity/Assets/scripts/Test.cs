using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // How many meshes to draw.
    public int population;
    // Range to draw meshes within.
    public float range;

    // Material to use for drawing the meshes.
    public Material material;

    private Matrix4x4[] matrices;
    private MaterialPropertyBlock block;

    private Mesh mesh;

    private void Setup()
    {
        Vector3 a = new Vector3(0, 0);
        Vector3 b = new Vector3(0, 10);
        Vector3 c = new Vector3(10, 0);
        Vector3 d = new Vector3(10, 10);
        Mesh mesh = CreateQuad(a, b, c, d);
        this.mesh = mesh;

        matrices = new Matrix4x4[population];
        Vector4[] colors = new Vector4[population];

        block = new MaterialPropertyBlock();

        for (int i = 0; i < population; i++)
        {
            // Build matrix.
            /*Vector3 position = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
            Quaternion rotation = Quaternion.Euler(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
            Vector3 scale = Vector3.one;

            Matrix4x4 mat = Matrix4x4.TRS(position, rotation, scale);*/

            Matrix4x4 mat = Matrix4x4.identity;
            matrices[i] = mat;

            colors[i] = Color.Lerp(Color.red, Color.blue, Random.value);
        }

        // Custom shader needed to read these!!
        block.SetVectorArray("_Colors", colors);
    }

    private Mesh CreateQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
		Mesh m = new Mesh();

		Vector3[] vertices = new Vector3[4]
		{
					a,//new Vector3(0, 0, 0),
					b,//new Vector3(width, 0, 0),
					c,//new Vector3(0, height, 0),
					d,//new Vector3(width, height, 0)
		};
		m.vertices = vertices;

		int[] tris = new int[6]
		{
                    // lower left triangle
                    0, 2, 1,
                    // upper right triangle
                    2, 3, 1
		};
		m.triangles = tris;

		Vector3[] normals = new Vector3[4]
		{
					-Vector3.forward,
					-Vector3.forward,
					-Vector3.forward,
					-Vector3.forward
		};
		m.normals = normals;

		Vector2[] uv = new Vector2[4]
		{
					new Vector2(0, 0),
					new Vector2(1, 0),
					new Vector2(0, 1),
					new Vector2(1, 1)
		};
		m.uv = uv;
        return m;
	}

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        // Draw a bunch of meshes each frame.
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices, population, block);
    }
}

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Test : MonoBehaviour
//{
//	public Material mat;
//	private Mesh mesh;
//	private Matrix4x4[] m4x4;

//	private Mesh getQuadMesh(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
//	{
//		Mesh mesh = new Mesh();
//		mesh.name = "GeneratedQuad";
//		mesh.hideFlags = HideFlags.DontSave;
//		// Points
//		Vector3[] verts = new Vector3[4] { a, b, c, d };
//		// Triangles
//		int[] tris = new int[6]
//		{
//        // lower left triangle
//        0, 2, 1,
//        // upper right triangle
//        2, 3, 1
//		};
//		mesh.SetVertices(verts);
//		mesh.SetIndices(tris, MeshTopology.Triangles, 0);
//		mesh.RecalculateBounds();
//		mesh.RecalculateNormals();
//		mesh.UploadMeshData(true);
//		return mesh;
//	}

//	// Start is called before the first frame update
//	void Start()
//	{
//		mesh = new Mesh();

//		Vector3[] vertices = new Vector3[4]
//		{
//			new Vector3(0, 0, 0),
//			new Vector3(10, 0, 0),
//			new Vector3(0, 10, 0),
//			new Vector3(10, 10, 0)
//		};
//		mesh.vertices = vertices;

//		int[] tris = new int[6]
//		{
//            // lower left triangle
//            0, 2, 1,
//            // upper right triangle
//            2, 3, 1
//		};
//		mesh.triangles = tris;

//		Vector3[] normals = new Vector3[4]
//		{
//			-Vector3.forward,
//			-Vector3.forward,
//			-Vector3.forward,
//			-Vector3.forward
//		};
//		mesh.normals = normals;

//		Vector2[] uv = new Vector2[4]
//		{
//			new Vector2(0, 0),
//			new Vector2(1, 0),
//			new Vector2(0, 1),
//			new Vector2(1, 1)
//		};
//		mesh.uv = uv;
//		/*Vector3 a = new Vector3(0, 0, 0);
//        Vector3 b = new Vector3(10, 0, 0);
//        Vector3 c = new Vector3(0, 10, 0);
//        Vector3 d = new Vector3(10, 10, 0);
//        mesh = getQuadMesh(a, b, c, d);*/

//		int population = 10;
//		int range = 10;
//		m4x4 = new Matrix4x4[population];
//		for (int i = 0; i < population; i++)
//		{
//			// Build matrix.
//			Vector3 position = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
//			Quaternion rotation = Quaternion.Euler(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
//			Vector3 scale = Vector3.one;

//			m4x4[i] = Matrix4x4.TRS(position, rotation, scale);
//			m4x4[i] = Matrix4x4.identity;


//		}


//		Graphics.DrawMeshInstanced(mesh, 0, mat, m4x4, population, null, UnityEngine.Rendering.ShadowCastingMode.On, true, gameObject.layer);

//		/*GameObject go = new GameObject("Empty");
//        go.AddComponent<MeshFilter>();
//        go.AddComponent<MeshRenderer>();
//        go.GetComponent<MeshFilter>().mesh = mesh;
//        go.GetComponent<MeshRenderer>().material = mat;*/
//	}

//	// Update is called once per frame
//	void Update()
//	{
//		Graphics.DrawMeshInstanced(mesh, 0, mat, m4x4, 0, null, UnityEngine.Rendering.ShadowCastingMode.On, true, gameObject.layer);
//		/* Vector3 a = new Vector3(0, 1, 0);
//         Vector3 b = new Vector3(0, 2, 0);
//         Vector3 c = new Vector3(1, 2, 0);
//         Vector3 d = new Vector3(1, 1, 0);
//         Mesh mesh = getQuadMesh(a, b, c, d);
//         Matrix4x4[] m4x4 = new Matrix4x4 [] { Matrix4x4.identity };
//         Graphics.DrawMeshInstanced(mesh, 0, mat, m4x4, 0, null, UnityEngine.Rendering.ShadowCastingMode.On, true, gameObject.layer);*/


//	}
//}
