using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeGuard : MonoBehaviour
{
    MyCapsuleHand leftHand;
    Projector leftProjector;
    GameObject leftProjectorObject;
    MyCapsuleHand rightHand;
    Projector rightProjector;
    GameObject rightProjectorObject;

	public SafeGuard(MyCapsuleHand leftHand, MyCapsuleHand rightHand)
	{
		this.leftHand = leftHand;
		this.rightHand = rightHand;
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
