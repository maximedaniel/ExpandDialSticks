using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using g3;

public class OrientedBounds : MonoBehaviour
{
    // Just for the demo I used Transforms so I can simply move them around in the scene
    public GameObject colliderObject;
    public Transform[] transforms;

    private void OnDrawGizmos()
    {
        // First wehave to convert the Unity Vector3 array
        // into the g3 type g3.Vector3d
        var points3d = new Vector3d[transforms.Length];
        for (var i = 0; i < transforms.Length; i++)
        {
            // Thanks to the g3 library implictely casted from UnityEngine.Vector3 to g3.Vector3d
            points3d[i] = transforms[i].position;
        }

        // BOOM MAGIC!!!
        var orientedBoundingBox = new ContOrientedBox3(points3d);

        // Now just convert the information back to Unity Vector3 positions and axis
        // Since g3.Vector3d uses doubles but Unity Vector3 uses floats
        // we have to explicitly cast to Vector3
        var center = (Vector3)orientedBoundingBox.Box.Center;

        var axisX = (Vector3)orientedBoundingBox.Box.AxisX;
        var axisY = (Vector3)orientedBoundingBox.Box.AxisY;
        var axisZ = (Vector3)orientedBoundingBox.Box.AxisZ;
        var extends = (Vector3)orientedBoundingBox.Box.Extent;

        // Now we can simply calculate our 8 vertices of the bounding box
        var A = center - extends.z * axisZ - extends.x * axisX - axisY * extends.y;
        var B = center - extends.z * axisZ + extends.x * axisX - axisY * extends.y;
        var C = center - extends.z * axisZ + extends.x * axisX + axisY * extends.y;
        var D = center - extends.z * axisZ - extends.x * axisX + axisY * extends.y;

        var E = center + extends.z * axisZ - extends.x * axisX - axisY * extends.y;
        var F = center + extends.z * axisZ + extends.x * axisX - axisY * extends.y;
        var G = center + extends.z * axisZ + extends.x * axisX + axisY * extends.y;
        var H = center + extends.z * axisZ - extends.x * axisX + axisY * extends.y;

        colliderObject.transform.position = center;
        Quaternion rotation = Quaternion.LookRotation(axisZ, axisY);
        colliderObject.transform.rotation = rotation;
        /*colliderObject.transform.LookAt(center + axisX, Vector3.right);
        colliderObject.transform.LookAt(center + axisY, Vector3.up);
        colliderObject.transform.LookAt(center + axisZ, Vector3.forward);*/
        Bounds b = new Bounds(new Vector3(0, 0, 0), extends * 2);
        BoxCollider bc = colliderObject.GetComponent<BoxCollider>();
        bc.size = b.size;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center, center + axisX);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(center, center + axisY);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(center, center + axisZ);
        // And finally visualize it
        /*Gizmos.color = Color.white;
        Gizmos.DrawLine(A, B);
        Gizmos.DrawLine(B, C);
        Gizmos.DrawLine(C, D);
        Gizmos.DrawLine(D, A);

        Gizmos.DrawLine(E, F);
        Gizmos.DrawLine(F, G);
        Gizmos.DrawLine(G, H);
        Gizmos.DrawLine(H, E);

        Gizmos.DrawLine(A, E);
        Gizmos.DrawLine(B, F);
        Gizmos.DrawLine(D, H);
        Gizmos.DrawLine(C, G);*/

        // And Here we ca just be amazed ;)
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
