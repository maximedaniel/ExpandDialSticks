using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArmController
{
    GameObject GetHandColliderAt(int index);
    int GetNbHandCollider();
    GameObject GetArmColliderAt(int index);
    int GetNbArmCollider();

    bool IsActive();
    string ToString();
}
