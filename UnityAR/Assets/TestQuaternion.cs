// You can also use transform.LookAt

using UnityEngine;
using System.Collections;

public class TestQuaternion : MonoBehaviour
{
    public Transform target;
    void OnDrawGizmos()
    {
		if (target != null)
        {
            Vector3 relativePos = target.position - transform.position;

            // the second argument, upwards, defaults to Vector3.up
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
            rotation *= Quaternion.AngleAxis(90, Vector3.right);
           
            transform.rotation = rotation;
        }
    }
    void Update()
    {
    }
}