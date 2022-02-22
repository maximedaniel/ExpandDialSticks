using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArmController
{
    GameObject GetHandColliderAt(int index);
    GameObject GetArmColliderAt(int index);

    bool IsActive();
    string ToString();
}
