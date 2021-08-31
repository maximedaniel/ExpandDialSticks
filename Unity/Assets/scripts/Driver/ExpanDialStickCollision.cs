using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using TMPro;
using System;
using Leap.Unity;

public class ExpanDialStickCollision: MonoBehaviour
{
	private float diameter = 4.0f;
	private float height = 10.0f;
	private float offset = 0.5f;

	private MyCapsuleHand leftHand;
	private MyCapsuleHand rightHand;

	private int i = 0;
	private int j = 0;
	private int nbSeparationLevels = 3;

	//private List<GameObject> goList = new List<GameObject>();

	private const int SEPARATION_LAYER = 10; // Safety Level 0
	private float proximity = 0f;
	private float distance = 0f;
	private int separationLevel = 0;
	private const float minUserBodyDistance = 3f;
	private float maxLayerHeight = 10f;
	private float minDistanceFromLayer = 15f;
	//private bool collisionDetected = false;

	// Getters and Setters
	public int Row
	{
		get => this.i;
		set => this.i = value;
	}

	public int Column
	{
		get => this.j;
		set => this.j = value;
	}

	public float Diameter
	{
		get => this.diameter;
		set => this.diameter = value;
	}
	public MyCapsuleHand LeftHand
	{
		get => this.leftHand;
		set => this.leftHand = value;
	}
	public MyCapsuleHand RightHand
	{
		get => this.rightHand;
		set => this.rightHand = value;
	}

	public float Height
	{
		get => this.height;
		set {
			this.maxLayerHeight = value;
			//minDistanceFromLayer = maxLayerHeight + minUserBodyDistance;
			this.height = value;
		}
	}

	public float Offset
	{
		get => this.offset;
		set => this.offset = value;
	}
	public int NbSeparationLevels
	{
		get => this.nbSeparationLevels;
		set => this.nbSeparationLevels = value;
	}

	public float Proximity()
	{
		return proximity;
	}
	public float Distance()
	{
		return distance;
	}


	public int SeparationLevel()
	{
		return separationLevel;
	}

	public void EnableCollision()
	{
		this.GetComponent<BoxCollider>().enabled = true;
	}
	public void DisableCollision()
	{
		this.GetComponent<BoxCollider>().enabled = false;
	}

	void Start()
	{

	}
	void FixedUpdate()
	{
		this.distance = float.PositiveInfinity;
		Vector3 pinHeadPoint = (transform.position + transform.up * (height / 2f));
		Vector3 pinTailPoint = (transform.position - transform.up * (height / 2f));
		Vector3 pinPoint = transform.position;
		float distanceToLeftBody, distanceToRightBody;
		distanceToLeftBody = distanceToRightBody = float.PositiveInfinity;
		Vector3 pinPointToLeftBody, pinPointToRightBody;
		pinPointToLeftBody = pinPointToRightBody = pinPoint;
		if (leftHand != null && leftHand.IsActive())
		{
				// Left hand
				GameObject handCollider = leftHand.GetHandCollider();
				SphereCollider sc = handCollider.GetComponent<SphereCollider>();
				Vector3 handColliderPosition = handCollider.transform.position;
				// head
				handColliderPosition.y = pinHeadPoint.y;
				float distanceHeadToLeftHand = Vector3.Magnitude(handColliderPosition - pinHeadPoint);
				Vector3 headToLeftHand = Vector3.Normalize(handColliderPosition - pinHeadPoint); 
				if(distanceHeadToLeftHand < distanceToLeftBody)
				{
					pinPointToLeftBody = pinHeadPoint + headToLeftHand * diameter / 2f;
					distanceToLeftBody = distanceHeadToLeftHand;
				}
				// tail
				handColliderPosition.y = pinTailPoint.y;
				float distanceTailToLeftHand = Vector3.Magnitude(handColliderPosition - pinTailPoint);
				Vector3 tailToLeftHand = Vector3.Normalize(handColliderPosition - pinTailPoint); 
				if (distanceTailToLeftHand < distanceToLeftBody)
				{
					pinPointToLeftBody = pinTailPoint + tailToLeftHand * diameter / 2f;
					distanceToLeftBody = distanceTailToLeftHand;
				}
				// Left Arm 
				GameObject armCollider = leftHand.GetArmCollider();
				CapsuleCollider capsuleCollider1 = armCollider.GetComponent<CapsuleCollider>();
				Vector3 forwardArmColliderPosition = armCollider.transform.position + armCollider.transform.forward * (capsuleCollider1.height / 2.0f);
				Vector3 backwardArmColliderPosition = armCollider.transform.position - armCollider.transform.forward * (capsuleCollider1.height / 2.0f);
				// head
				forwardArmColliderPosition.y = pinHeadPoint.y;
				backwardArmColliderPosition.y = pinHeadPoint.y;
				Vector3 projectHeadLeftArm = SafeGuard.ProjectPointLine(pinHeadPoint, backwardArmColliderPosition, forwardArmColliderPosition);
				float distanceHeadToLeftArm = Vector3.Magnitude(projectHeadLeftArm - pinHeadPoint);
				Vector3 headToLeftArm = Vector3.Normalize(projectHeadLeftArm - pinHeadPoint);

				if (distanceHeadToLeftArm < distanceToLeftBody)
				{
					pinPointToLeftBody = pinHeadPoint + headToLeftArm * diameter / 2f;
					distanceToLeftBody = distanceHeadToLeftArm;
				}
				// tail
				forwardArmColliderPosition.y = pinTailPoint.y;
				backwardArmColliderPosition.y = pinTailPoint.y;
				Vector3 projectTailLeftArm = SafeGuard.ProjectPointLine(pinTailPoint, backwardArmColliderPosition, forwardArmColliderPosition);
				float distanceTailToLeftArm = Vector3.Magnitude(projectTailLeftArm - pinTailPoint);
				Vector3 tailToLeftArm = Vector3.Normalize(projectTailLeftArm - pinTailPoint);

				if (distanceTailToLeftArm < distanceToLeftBody)
				{
					pinPointToLeftBody = pinTailPoint + tailToLeftArm * diameter / 2f;
					distanceToLeftBody = distanceTailToLeftArm;
				}

		}
		if (rightHand != null && rightHand.IsActive())
		{
			// Left hand
			GameObject handCollider = rightHand.GetHandCollider();
			SphereCollider sc = handCollider.GetComponent<SphereCollider>();
			Vector3 handColliderPosition = handCollider.transform.position;
			// head
			handColliderPosition.y = pinHeadPoint.y;
			float distanceHeadToRightHand = Vector3.Magnitude(handColliderPosition - pinHeadPoint);
			Vector3 headToRightHand = Vector3.Normalize(handColliderPosition - pinHeadPoint);
			if (distanceHeadToRightHand < distanceToRightBody)
			{
				pinPointToRightBody = pinHeadPoint + headToRightHand * diameter / 2f;
				distanceToRightBody = distanceHeadToRightHand;
			}
			// tail
			handColliderPosition.y = pinTailPoint.y;
			float distanceTailToRightHand = Vector3.Magnitude(handColliderPosition - pinTailPoint);
			Vector3 tailToRightHand = Vector3.Normalize(handColliderPosition - pinTailPoint);
			if (distanceTailToRightHand < distanceToRightBody)
			{
				pinPointToRightBody = pinTailPoint + tailToRightHand * diameter / 2f;
				distanceToRightBody = distanceTailToRightHand;
			}
			// Right Arm 
			GameObject armCollider = RightHand.GetArmCollider();
			CapsuleCollider capsuleCollider1 = armCollider.GetComponent<CapsuleCollider>();
			Vector3 forwardArmColliderPosition = armCollider.transform.position + armCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			Vector3 backwardArmColliderPosition = armCollider.transform.position - armCollider.transform.forward * (capsuleCollider1.height / 2.0f);
			// head
			forwardArmColliderPosition.y = pinHeadPoint.y;
			backwardArmColliderPosition.y = pinHeadPoint.y;
			Vector3 projectHeadRightArm = SafeGuard.ProjectPointLine(pinHeadPoint, backwardArmColliderPosition, forwardArmColliderPosition);
			float distanceHeadToRightArm = Vector3.Magnitude(projectHeadRightArm - pinHeadPoint);
			Vector3 headToRightArm = Vector3.Normalize(projectHeadRightArm - pinHeadPoint);

			if (distanceHeadToRightArm < distanceToRightBody)
			{
				pinPointToRightBody = pinHeadPoint + headToRightArm * diameter / 2f;
				distanceToRightBody = distanceHeadToRightArm;
			}
			// tail
			forwardArmColliderPosition.y = pinTailPoint.y;
			backwardArmColliderPosition.y = pinTailPoint.y;
			Vector3 projectTailRightArm = SafeGuard.ProjectPointLine(pinTailPoint, backwardArmColliderPosition, forwardArmColliderPosition);
			float distanceTailToRightArm = Vector3.Magnitude(projectTailRightArm - pinTailPoint);
			Vector3 tailToRightArm = Vector3.Normalize(projectTailRightArm - pinTailPoint);

			if (distanceTailToRightArm < distanceToRightBody)
			{
				pinPointToRightBody = pinTailPoint + tailToRightArm * diameter / 2f;
				distanceToRightBody = distanceTailToRightArm;
			}
		}

		if (distanceToLeftBody != float.PositiveInfinity || distanceToRightBody != float.PositiveInfinity) {

			pinPoint = (distanceToLeftBody < distanceToRightBody) ? pinPointToLeftBody : pinPointToRightBody;
			this.distance = Mathf.Min(distanceToLeftBody, distanceToRightBody);
			/* Check user proximy level 1 */
			for (int level = 0; level < nbSeparationLevels; level++)
			{
				RaycastHit hit;
				bool touched = Physics.Raycast(pinPoint - Vector3.up * 100, Vector3.up, out hit, Mathf.Infinity, 1 << (SEPARATION_LAYER + level));
				if (touched)
				{
					Vector3 hitPoint = hit.point;
					if ((hitPoint.y - pinPoint.y) <= minDistanceFromLayer)
					{
						separationLevel = level;
						float coeff = Mathf.Max(0, level - 1)/(float)nbSeparationLevels;
						proximity = 1f - coeff;
						Debug.DrawLine(pinPoint - Vector3.up * 100, hitPoint, Color.HSVToRGB(coeff, 1f, 1f));
						return;
					}
				}
			}
		}
		separationLevel = nbSeparationLevels;
		proximity = 0f;
	}

	private void OnTriggerStay(Collider other)
	{

	}

	private void OnTriggerEnter(Collider other)
	{

	}
	private void OnTriggerExit(Collider other)
	{

	}
}
