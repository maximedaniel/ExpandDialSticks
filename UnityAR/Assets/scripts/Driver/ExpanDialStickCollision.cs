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
	private const int LEFT_HAND_INDEX = 0;
	private const int LEFT_ARM_INDEX = 1;
	private const int RIGHT_HAND_INDEX = 2;
	private const int RIGHT_ARM_INDEX = 3;
	private const int NB_PARTS = 4;
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
	private float gamma = 0f;
	private float distance = 0f;
	private int separationLevel = 0;
	private const float minUserBodyDistance = 3f;
	private float maxLayerHeight = 10f;
	private float minDistanceFromLayer = float.PositiveInfinity;// 15f;
	private Vector3 leftHandPos = Vector3.zero;
	private Vector3 rightHandPos = Vector3.zero;
	private Vector3 leftBackArmPos = Vector3.zero;
	private Vector3 leftFrontArmPos = Vector3.zero;
	private Vector3 rightBackArmPos = Vector3.zero;
	private Vector3 rightFrontArmPos = Vector3.zero;

	private float minLeftHandRadius = 0f;
	private float maxLeftHandRadius = 0f;
	private float minRightHandRadius = 0f;
	private float maxRightHandRadius = 0f;
	private float minLeftArmRadius = 0f;
	private float maxLeftArmRadius = 0f;
	private float minRightArmRadius = 0f;
	private float maxRightArmRadius = 0f;
	private GameObject leftHandCollider = null;
	private GameObject rightHandCollider = null;
	private GameObject leftArmCollider = null;
	private GameObject rightArmCollider = null;
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

	public float Gamma()
	{
		return gamma;
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

	public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
	{
		Vector3 relativePoint = point - lineStart;
		Vector3 lineDirection = lineEnd - lineStart;
		float length = lineDirection.magnitude;
		Vector3 normalizedLineDirection = lineDirection;
		if (length > .000001f)
			normalizedLineDirection /= length;

		float dot = Vector3.Dot(normalizedLineDirection, relativePoint);
		dot = Mathf.Clamp(dot, 0.0F, length);

		return lineStart + normalizedLineDirection * dot;
	}

	// Calculate distance between a point and a line.
	public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
	{
		return Vector3.Magnitude(ProjectPointLine(point, lineStart, lineEnd) - point);
	}
	// Calculate distance between a point and a line.
	public static Vector3 DirectionPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
	{
		return ProjectPointLine(point, lineStart, lineEnd) - point;
	}
	public static (Vector3 start, Vector3 end, float gam) GetDirectionsFromBody( Transform pin,
		Vector3 leftHandPos, float minLeftHandRadius, float maxLeftHandRadius,
		Vector3 rightHandPos, float minRightHandRadius, float maxRightHandRadius,
		Vector3 leftBackArmPos, Vector3 leftFrontArmPos, float minLeftArmRadius, float maxLeftArmRadius,
		Vector3 rightBackArmPos, Vector3 rightFrontArmPos,  float minRightArmRadius, float maxRightArmRadius)
	{
		Vector3 startPos = Vector3.negativeInfinity;
		Vector3 endPos = Vector3.positiveInfinity;
		Vector3 minDirection = Vector3.positiveInfinity;
		float gam = 0f;
		//Vector3[] directionParts = new Vector3[NB_PARTS];
		float diameter = pin.localScale.x;
		float height = pin.localScale.y * 2f;
		// compute head and tail pos of cylinder
		Vector3 headPinPos = (pin.position + pin.up * (height / 2f));
		Vector3 tailPinPos = (pin.position - pin.up * (height / 2f));

		// LEFT HAND

		Vector3 tailPinToLeftHand = Vector3.Normalize(new Vector3(leftHandPos.x, tailPinPos.y, leftHandPos.z) - tailPinPos);
		Vector3 tailPinToLeftHandDirection = DirectionPointLine(tailPinPos + tailPinToLeftHand * (diameter / 2.0f), new Vector3(leftHandPos.x, tailPinPos.y, leftHandPos.z) - new Vector3(0, 1f, 0), new Vector3(leftHandPos.x, tailPinPos.y, leftHandPos.z) + new Vector3(0, 1f, 0));
		if (tailPinToLeftHandDirection.magnitude <= minDirection.magnitude)
		{
			minDirection = tailPinToLeftHandDirection;
			startPos = tailPinPos + tailPinToLeftHand * (diameter / 2.0f);
			endPos = startPos + tailPinToLeftHandDirection;
			gam = 1f - Mathf.InverseLerp(minLeftHandRadius, maxLeftHandRadius, minDirection.magnitude);
		}

		Vector3 headPinToLeftHand = Vector3.Normalize(new Vector3(leftHandPos.x, headPinPos.y, leftHandPos.z) - headPinPos);
		Vector3 headPinToLeftHandDirection = DirectionPointLine(headPinPos + headPinToLeftHand * (diameter / 2.0f), new Vector3(leftHandPos.x, headPinPos.y, leftHandPos.z) - new Vector3(0, 1f, 0), new Vector3(leftHandPos.x, headPinPos.y, leftHandPos.z) + new Vector3(0, 1f, 0));
		if (headPinToLeftHandDirection.magnitude <= minDirection.magnitude)
		{
			minDirection = headPinToLeftHandDirection;
			startPos = headPinPos + headPinToLeftHand * (diameter / 2.0f);
			endPos = startPos + headPinToLeftHandDirection;
			gam = 1f - Mathf.InverseLerp(minLeftHandRadius, maxLeftHandRadius, minDirection.magnitude);
		}
		//Debug.DrawLine(headPinPos + headPinToLeftHand * (diameter / 2.0f), headPinPos + headPinToLeftHand * (diameter / 2.0f) + pinToLeftHandDirection, Color.HSVToRGB(0f, 0f, 0f));
		//directionParts[LEFT_HAND_INDEX] = pinToLeftHandDirection;

		// RIGHT HAND

		Vector3 tailPinToRightHand = Vector3.Normalize(new Vector3(rightHandPos.x, tailPinPos.y, rightHandPos.z) - tailPinPos);
		Vector3 tailPinToRightHandDirection = DirectionPointLine(tailPinPos + tailPinToRightHand * (diameter / 2.0f), new Vector3(rightHandPos.x, tailPinPos.y, rightHandPos.z) - new Vector3(0, 1f, 0), new Vector3(rightHandPos.x, tailPinPos.y, rightHandPos.z) + new Vector3(0, 1f, 0));
		if (tailPinToRightHandDirection.magnitude <= minDirection.magnitude)
		{
			minDirection = tailPinToRightHandDirection;
			startPos = tailPinPos + tailPinToRightHand * (diameter / 2.0f);
			endPos = startPos + tailPinToRightHandDirection;
			gam = 1f - Mathf.InverseLerp(minRightHandRadius, maxRightHandRadius, minDirection.magnitude);
		}

		Vector3 headPinToRightHand = Vector3.Normalize(new Vector3(rightHandPos.x, headPinPos.y, rightHandPos.z) - headPinPos);
		Vector3 headPinToRightHandDirection = DirectionPointLine(headPinPos + headPinToRightHand * (diameter / 2.0f), new Vector3(rightHandPos.x, headPinPos.y, rightHandPos.z) - new Vector3(0, 1f, 0), new Vector3(rightHandPos.x, headPinPos.y, rightHandPos.z) + new Vector3(0, 1f, 0));
		if (headPinToRightHandDirection.magnitude <= minDirection.magnitude)
		{
			minDirection = headPinToRightHandDirection;
			startPos = headPinPos + headPinToRightHand * (diameter / 2.0f);
			endPos = startPos + headPinToRightHandDirection;
			gam = 1f - Mathf.InverseLerp(minRightHandRadius, maxRightHandRadius, minDirection.magnitude);
		}
		//Debug.DrawLine(headPinPos + headPinToRightHand * (diameter / 2.0f), headPinPos + headPinToRightHand * (diameter / 2.0f) + pinToRightHandDirection, Color.HSVToRGB(0f, 0f, 0f));

		// LEFT ARM

		Vector3 tailPinToLeftArm = Vector3.Normalize(DirectionPointLine(tailPinPos, new Vector3(leftBackArmPos.x, tailPinPos.y, leftBackArmPos.z), new Vector3(leftFrontArmPos.x, tailPinPos.y, leftFrontArmPos.z)));
		Vector3 tailPinToLeftArmDirection = DirectionPointLine(tailPinPos + tailPinToLeftArm * (diameter / 2.0f), new Vector3(leftBackArmPos.x, tailPinPos.y, leftBackArmPos.z), new Vector3(leftFrontArmPos.x, tailPinPos.y, leftFrontArmPos.z));
		if (tailPinToLeftArmDirection.magnitude <= minDirection.magnitude)
		{
			minDirection = tailPinToLeftArmDirection;
			startPos = tailPinPos + tailPinToLeftArm * (diameter / 2.0f);
			endPos = startPos + tailPinToLeftArmDirection;
			gam = 1f - Mathf.InverseLerp(minLeftArmRadius, maxLeftArmRadius, minDirection.magnitude);
		}

		Vector3 headPinToLeftArm = Vector3.Normalize(DirectionPointLine(headPinPos, new Vector3(leftBackArmPos.x, headPinPos.y, leftBackArmPos.z), new Vector3(leftFrontArmPos.x, headPinPos.y, leftFrontArmPos.z)));
		Vector3 headPinToLeftArmDirection = DirectionPointLine(headPinPos + headPinToLeftArm * (diameter / 2.0f), new Vector3(leftBackArmPos.x, headPinPos.y, leftBackArmPos.z), new Vector3(leftFrontArmPos.x, headPinPos.y, leftFrontArmPos.z));
		if (headPinToLeftArmDirection.magnitude <= minDirection.magnitude)
		{
			minDirection = headPinToLeftArmDirection;
			startPos = headPinPos + headPinToLeftArm * (diameter / 2.0f);
			endPos = startPos + headPinToLeftArmDirection;
			gam = 1f - Mathf.InverseLerp(minLeftArmRadius, maxLeftArmRadius, minDirection.magnitude);
		}


		// RIGHT ARM

		Vector3 tailPinToRightArm = Vector3.Normalize(DirectionPointLine(tailPinPos, new Vector3(rightBackArmPos.x, tailPinPos.y, rightBackArmPos.z), new Vector3(rightFrontArmPos.x, tailPinPos.y, rightFrontArmPos.z)));
		Vector3 tailPinToRightArmDirection = DirectionPointLine(tailPinPos + tailPinToRightArm * (diameter / 2.0f), new Vector3(rightBackArmPos.x, tailPinPos.y, rightBackArmPos.z), new Vector3(rightFrontArmPos.x, tailPinPos.y, rightFrontArmPos.z));
		if (tailPinToRightArmDirection.magnitude <= minDirection.magnitude)
		{
			minDirection = tailPinToRightArmDirection;
			startPos = tailPinPos + tailPinToRightArm * (diameter / 2.0f);
			endPos = startPos + tailPinToRightArmDirection;
			gam = 1f - Mathf.InverseLerp(minRightArmRadius, maxRightArmRadius, minDirection.magnitude);
		}

		Vector3 headPinToRightArm = Vector3.Normalize(DirectionPointLine(headPinPos, new Vector3(rightBackArmPos.x, headPinPos.y, rightBackArmPos.z), new Vector3(rightFrontArmPos.x, headPinPos.y, rightFrontArmPos.z)));
		Vector3 headPinToRightArmDirection = DirectionPointLine(headPinPos + headPinToRightArm * (diameter / 2.0f), new Vector3(rightBackArmPos.x, headPinPos.y, rightBackArmPos.z), new Vector3(rightFrontArmPos.x, headPinPos.y, rightFrontArmPos.z));
		if (headPinToRightArmDirection.magnitude <= minDirection.magnitude)
		{
			minDirection = headPinToRightArmDirection;
			startPos = headPinPos + headPinToRightArm * (diameter / 2.0f);
			endPos = startPos + headPinToRightArmDirection;
			gam = 1f - Mathf.InverseLerp(minRightArmRadius, maxRightArmRadius, minDirection.magnitude);
		}


		return (startPos, endPos, gam);
	}

	void Start()
	{

	}
	void FixedUpdate()
	{
		this.gamma = 0f;
		this.distance = float.PositiveInfinity;
		bool computeDistance = false;

		leftHandPos = rightHandPos = Vector3.negativeInfinity;
		minLeftHandRadius = maxLeftHandRadius = minRightHandRadius = maxRightHandRadius = 0f;
		leftFrontArmPos = leftBackArmPos = rightFrontArmPos = rightBackArmPos =  Vector3.negativeInfinity;
		minLeftArmRadius = maxLeftArmRadius = minRightArmRadius = maxRightArmRadius = 0f;

		if (leftHand != null && leftHand.IsActive())
		{
			// Get Left Hand Collider
			leftHandCollider = leftHand.GetHandCollider();
			SphereCollider sc = leftHandCollider.GetComponent<SphereCollider>();
			leftHandPos = leftHandCollider.transform.position;
			minLeftHandRadius = sc.radius;
			maxLeftHandRadius = sc.radius + MyCapsuleHand.WARNING_RADIUS;
			//Debug.Log("minLeftHandRadius: " + minLeftHandRadius + " " + maxLeftHandRadius);


			// Get Left Arm Collider
			leftArmCollider = leftHand.GetArmCollider();
			CapsuleCollider cc = leftArmCollider.GetComponent<CapsuleCollider>();
			leftFrontArmPos = leftArmCollider.transform.position + leftArmCollider.transform.forward * (cc.height / 2.0f);
			leftBackArmPos = leftArmCollider.transform.position - leftArmCollider.transform.forward * (cc.height / 2.0f);
			minLeftArmRadius = cc.radius;
			maxLeftArmRadius = cc.radius + MyCapsuleHand.WARNING_RADIUS;
			//Debug.Log("minLeftArmRadius: " + minLeftArmRadius + " " + maxLeftArmRadius);

			computeDistance = true;
		} 
		if (rightHand != null && rightHand.IsActive())
		{
			// Get Right Hand Collider
			rightHandCollider = rightHand.GetHandCollider();
			SphereCollider sc = rightHandCollider.GetComponent<SphereCollider>();
			rightHandPos = rightHandCollider.transform.position;
			minRightHandRadius = sc.radius;
			maxRightHandRadius = sc.radius + MyCapsuleHand.WARNING_RADIUS;
			///Debug.Log("minRightHandRadius" + minRightHandRadius + " " + maxRightHandRadius);

			// Get Right Arm Collider
			rightArmCollider = rightHand.GetArmCollider();
			CapsuleCollider cc = rightArmCollider.GetComponent<CapsuleCollider>();
			rightFrontArmPos = rightArmCollider.transform.position + rightArmCollider.transform.forward * (cc.height / 2.0f);
			rightBackArmPos = rightArmCollider.transform.position - rightArmCollider.transform.forward * (cc.height / 2.0f);
			minRightArmRadius = cc.radius;
			maxRightArmRadius = cc.radius + MyCapsuleHand.WARNING_RADIUS;
			//Debug.Log("maxRightArmRadius: " + minRightArmRadius + " " + maxRightArmRadius);

			computeDistance = true;
		}
		if (computeDistance)
		{
			(Vector3 startPos, Vector3 endPos, float gam) = GetDirectionsFromBody(
				this.transform,
				leftHandPos, minLeftHandRadius, maxLeftHandRadius,
				rightHandPos, minRightHandRadius, maxRightHandRadius,
				leftBackArmPos, leftFrontArmPos, minLeftArmRadius, maxLeftArmRadius,
				rightBackArmPos, rightFrontArmPos, minRightArmRadius, maxRightArmRadius);
			//Debug.DrawLine(startPos, endPos, new Color(gam, gam, gam));
			this.gamma = gam;
			this.distance = (endPos - startPos).magnitude;
			/* Check user proximy level 1 */
			for (int level = 0; level < nbSeparationLevels; level++)
			{
				RaycastHit hit;
				bool touched = Physics.Raycast(startPos - Vector3.up * 100, Vector3.up, out hit, Mathf.Infinity, 1 << (SEPARATION_LAYER + level));
				if (touched)
				{
					Vector3 hitPoint = hit.point;
					if ((hitPoint.y - transform.position.y) <= minDistanceFromLayer)
					{
						separationLevel = level;
						float coeff = Mathf.Max(0, level - 1) / (float)(nbSeparationLevels - 1f);
						proximity = 1f - coeff;
						//Debug.Log(pinPoint + " => " + proximity);
						//Debug.DrawLine(pinPoint - Vector3.up * 100, hitPoint, Color.HSVToRGB(coeff, 0f, 0f));
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
