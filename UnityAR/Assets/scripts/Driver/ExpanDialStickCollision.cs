using UnityEngine;
using UnityEditor;
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

	private IArmController leftHand;
	private IArmController rightHand;

	private int i = 0;
	private int j = 0;
	private int nbSeparationLevels = 3;

	//private List<GameObject> goList = new List<GameObject>();

	private const int SEPARATION_LAYER = 10; // Safety Level 0
	private float proximity = 0f;
	private float gamma = 0f;
	private float distance = 0f;
	private float minDistance = 0f;
	private float maxDistance = 0f;
	private int separationLevel = 0;
	private const float minUserBodyDistance = 3f;
	private float maxLayerHeight = 10f;
	private Vector3 leftHandPos = Vector3.zero;
	private Vector3 rightHandPos = Vector3.zero;
	private Vector3 leftBackArmPos = Vector3.zero;
	private Vector3 leftFrontArmPos = Vector3.zero;
	private Vector3 rightBackArmPos = Vector3.zero;
	private Vector3 rightFrontArmPos = Vector3.zero;
	private Vector3 startPos = Vector3.zero;
	private Vector3 endPos = Vector3.zero;
	private Vector3 direction = Vector3.zero;

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
	public IArmController LeftHand
	{
		get => this.leftHand;
		set => this.leftHand = value;
	}
	public IArmController RightHand
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
	public static (Vector3 start, Vector3 end, float minDistance, float maxDistance) GetDirectionsFromBody( Transform pin,
		Vector3 leftHandPos, float minLeftHandRadius, float maxLeftHandRadius,
		Vector3 rightHandPos, float minRightHandRadius, float maxRightHandRadius,
		Vector3 leftBackArmPos, Vector3 leftFrontArmPos, float minLeftArmRadius, float maxLeftArmRadius,
		Vector3 rightBackArmPos, Vector3 rightFrontArmPos,  float minRightArmRadius, float maxRightArmRadius)
	{
		Vector3 startPos = Vector3.negativeInfinity;
		Vector3 direction = Vector3.positiveInfinity;
		Vector3 endPos = Vector3.positiveInfinity;
		float minDistance = 0f;
		float maxDistance = 0f;
		float prox = float.PositiveInfinity;
		Vector3 finalStartPos = Vector3.negativeInfinity;
		Vector3 finalDirection = Vector3.positiveInfinity;
		Vector3 finalEndPos = Vector3.positiveInfinity;
		float finalMinDistance = 0f;
		float finalMaxDistance = 0f;
		float finalProx = float.PositiveInfinity;
		//Vector3[] directionParts = new Vector3[NB_PARTS];
		float diameter = pin.localScale.x;
		float height = pin.localScale.y * 2f;
		// compute head and tail pos of cylinder
		Vector3 headPinPos = (pin.position + pin.up * (height / 2f));
		Vector3 tailPinPos = (pin.position - pin.up * (height / 2f));

		// LEFT HAND
		endPos = leftHandPos;
		startPos = tailPinPos + Vector3.Normalize(new Vector3(endPos.x, tailPinPos.y, endPos.z) - tailPinPos) * (diameter / 2.0f);
		direction = endPos - startPos;
		minDistance = minLeftHandRadius;
		maxDistance = maxLeftHandRadius;
		prox = (direction.magnitude - minDistance) / (maxDistance - minDistance);
		//prox = Mathf.InverseLerp(minDistance, maxDistance, direction.magnitude);
		if (prox <= finalProx)
		{
			finalProx = prox;
			finalDirection = direction;
			finalStartPos = startPos;
			finalEndPos = endPos;
			finalMinDistance = minDistance;
			finalMaxDistance = maxDistance;
		}


		endPos = leftHandPos;
		startPos = headPinPos + Vector3.Normalize(new Vector3(endPos.x, headPinPos.y, endPos.z) - headPinPos) * (diameter / 2.0f);
		direction = endPos - startPos;
		minDistance = minLeftHandRadius;
		maxDistance = maxLeftHandRadius;
		prox = (direction.magnitude - minDistance) / (maxDistance - minDistance);
		//prox = Mathf.InverseLerp(minDistance, maxDistance, direction.magnitude);
		if (prox <= finalProx)
		{
			finalProx = prox;
			finalDirection = direction;
			finalStartPos = startPos;
			finalEndPos = endPos;
			finalMinDistance = minDistance;
			finalMaxDistance = maxDistance;
		}

		// RIGHT HAND
		endPos = rightHandPos;
		startPos = tailPinPos + Vector3.Normalize(new Vector3(endPos.x, tailPinPos.y, endPos.z) - tailPinPos) * (diameter / 2.0f);
		direction = endPos - startPos;
		minDistance = minRightHandRadius;
		maxDistance = maxRightHandRadius;
		prox = (direction.magnitude - minDistance) / (maxDistance - minDistance);
		//prox = Mathf.InverseLerp(minDistance, maxDistance, direction.magnitude);
		if (prox <= finalProx)
		{
			finalProx = prox;
			finalDirection = direction;
			finalStartPos = startPos;
			finalEndPos = endPos;
			finalMinDistance = minDistance;
			finalMaxDistance = maxDistance;
		}


		endPos = rightHandPos;
		startPos = headPinPos + Vector3.Normalize(new Vector3(endPos.x, headPinPos.y, endPos.z) - headPinPos) * (diameter / 2.0f);
		direction = endPos - startPos;
		minDistance = minRightHandRadius;
		maxDistance = maxRightHandRadius;
		prox = (direction.magnitude - minDistance) / (maxDistance - minDistance);
		//prox = Mathf.InverseLerp(minDistance, maxDistance, direction.magnitude);
		if (prox <= finalProx)
		{
			finalProx = prox;
			finalDirection = direction;
			finalStartPos = startPos;
			finalEndPos = endPos;
			finalMinDistance = minDistance;
			finalMaxDistance = maxDistance;
		}


		// LEFT ARM
		endPos = ProjectPointLine(tailPinPos, leftBackArmPos, leftFrontArmPos);
		startPos = tailPinPos + Vector3.Normalize(new Vector3(endPos.x, tailPinPos.y, endPos.z) - tailPinPos) * (diameter / 2.0f);
		direction = endPos - startPos;
		minDistance = minLeftArmRadius;
		maxDistance = maxLeftArmRadius;
		prox = (direction.magnitude - minDistance) / (maxDistance - minDistance);
		//prox = Mathf.InverseLerp(minDistance, maxDistance, direction.magnitude);
		if (prox <= finalProx)
		{
			finalProx = prox;
			finalDirection = direction;
			finalStartPos = startPos;
			finalEndPos = endPos;
			finalMinDistance = minDistance;
			finalMaxDistance = maxDistance;
		}

		endPos = ProjectPointLine(headPinPos, leftBackArmPos, leftFrontArmPos);
		startPos = headPinPos + Vector3.Normalize(new Vector3(endPos.x, headPinPos.y, endPos.z) - headPinPos) * (diameter / 2.0f);
		direction = endPos - startPos;
		minDistance = minLeftArmRadius;
		maxDistance = maxLeftArmRadius;
		prox = (direction.magnitude - minDistance) / (maxDistance - minDistance);
		//prox = Mathf.InverseLerp(minDistance, maxDistance, direction.magnitude);
		if (prox <= finalProx)
		{
			finalProx = prox;
			finalDirection = direction;
			finalStartPos = startPos;
			finalEndPos = endPos;
			finalMinDistance = minDistance;
			finalMaxDistance = maxDistance;
		}


		// RIGHT ARM
		endPos = ProjectPointLine(tailPinPos, rightBackArmPos, rightFrontArmPos);
		startPos = tailPinPos + Vector3.Normalize(new Vector3(endPos.x, tailPinPos.y, endPos.z) - tailPinPos) * (diameter / 2.0f);
		direction = endPos - startPos;
		minDistance = minRightArmRadius;
		maxDistance = maxRightArmRadius;
		prox = (direction.magnitude - minDistance) / (maxDistance - minDistance);
		//prox = Mathf.InverseLerp(minDistance, maxDistance, direction.magnitude);
		if (prox <= finalProx)
		{
			finalProx = prox;
			finalDirection = direction;
			finalStartPos = startPos;
			finalEndPos = endPos;
			finalMinDistance = minDistance;
			finalMaxDistance = maxDistance;
		}

		endPos = ProjectPointLine(headPinPos, rightBackArmPos, rightFrontArmPos);
		startPos = headPinPos + Vector3.Normalize(new Vector3(endPos.x, headPinPos.y, endPos.z) - headPinPos) * (diameter / 2.0f);
		direction = endPos - startPos;
		minDistance = minRightArmRadius;
		maxDistance = maxRightArmRadius;
		prox = (direction.magnitude - minDistance) / (maxDistance - minDistance);
		//prox = Mathf.InverseLerp(minDistance, maxDistance, direction.magnitude);
		if (prox <= finalProx)
		{
			finalProx = prox;
			finalDirection = direction;
			finalStartPos = startPos;
			finalEndPos = endPos;
			finalMinDistance = minDistance;
			finalMaxDistance = maxDistance;
		}
		return (finalStartPos, finalEndPos, finalMinDistance, finalMaxDistance);
	}

	void Start()
	{

	}

	void OnDrawGizmos()
	{
		/*Handles.color = new Color(
			(this.separationLevel == 1) ? 1f : 0f,
			(this.separationLevel == 3) ? 1f : 0f,
			(this.separationLevel == 2) ? 1f : 0f
			);
		Handles.Label(this.startPos + this.direction * 0.5f, this.minDistance.ToString("F")+ "<" +this.distance.ToString("F") + "<" + this.maxDistance.ToString("F"));
		Handles.DrawLine(startPos, endPos);*/
	}

	void FixedUpdate()
	{
		startPos = Vector3.zero;
		endPos = Vector3.zero;
		direction = Vector3.zero;
		this.gamma = 0f;
		this.distance = this.minDistance = this.maxDistance = float.PositiveInfinity;
		bool computeDistance = false;

		leftHandPos = rightHandPos = Vector3.negativeInfinity;
		minLeftHandRadius = maxLeftHandRadius = minRightHandRadius = maxRightHandRadius = 0f;
		leftFrontArmPos = leftBackArmPos = rightFrontArmPos = rightBackArmPos =  Vector3.negativeInfinity;
		minLeftArmRadius = maxLeftArmRadius = minRightArmRadius = maxRightArmRadius = 0f;

		if (leftHand != null && leftHand.IsActive())
		{
			// Get Left Hand Collider
			leftHandCollider = leftHand.GetHandColliderAt(0);
			SphereCollider sc = leftHandCollider.GetComponent<SphereCollider>();
			leftHandPos = leftHandCollider.transform.position;
			minLeftHandRadius = sc.radius + MyCapsuleHand.STOP_RADIUS;
			maxLeftHandRadius = sc.radius + MyCapsuleHand.STOP_RADIUS + MyCapsuleHand.WARNING_RADIUS;
			//Debug.Log("minLeftHandRadius: " + minLeftHandRadius + " " + maxLeftHandRadius);


			// Get Left Arm Collider
			leftArmCollider = leftHand.GetArmColliderAt(0);
			CapsuleCollider cc = leftArmCollider.GetComponent<CapsuleCollider>();
			leftFrontArmPos = leftArmCollider.transform.position + leftArmCollider.transform.forward * (cc.height / 2.0f);
			leftBackArmPos = leftArmCollider.transform.position - leftArmCollider.transform.forward * (cc.height / 2.0f);
			minLeftArmRadius = cc.radius + MyCapsuleHand.STOP_RADIUS;
			maxLeftArmRadius = cc.radius + MyCapsuleHand.STOP_RADIUS + MyCapsuleHand.WARNING_RADIUS;
			//Debug.Log("minLeftArmRadius: " + minLeftArmRadius + " " + maxLeftArmRadius);

			computeDistance = true;
		} 
		if (rightHand != null && rightHand.IsActive())
		{
			// Get Right Hand Collider
			rightHandCollider = rightHand.GetHandColliderAt(0);
			SphereCollider sc = rightHandCollider.GetComponent<SphereCollider>();
			rightHandPos = rightHandCollider.transform.position;
			minRightHandRadius = sc.radius + MyCapsuleHand.STOP_RADIUS;
			maxRightHandRadius = sc.radius + MyCapsuleHand.STOP_RADIUS + MyCapsuleHand.WARNING_RADIUS;
			///Debug.Log("minRightHandRadius" + minRightHandRadius + " " + maxRightHandRadius);

			// Get Right Arm Collider
			rightArmCollider = rightHand.GetArmColliderAt(0);
			CapsuleCollider cc = rightArmCollider.GetComponent<CapsuleCollider>();
			rightFrontArmPos = rightArmCollider.transform.position + rightArmCollider.transform.forward * (cc.height / 2.0f);
			rightBackArmPos = rightArmCollider.transform.position - rightArmCollider.transform.forward * (cc.height / 2.0f);
			minRightArmRadius = cc.radius + MyCapsuleHand.STOP_RADIUS;
			maxRightArmRadius = cc.radius + MyCapsuleHand.STOP_RADIUS + MyCapsuleHand.WARNING_RADIUS;
			//Debug.Log("maxRightArmRadius: " + minRightArmRadius + " " + maxRightArmRadius);

			computeDistance = true;
		}
		if (computeDistance)
		{
			(Vector3 startPos, Vector3 endPos, float minDistance, float maxDistance) = GetDirectionsFromBody(
				this.transform,
				leftHandPos, minLeftHandRadius, maxLeftHandRadius,
				rightHandPos, minRightHandRadius, maxRightHandRadius,
				leftBackArmPos, leftFrontArmPos, minLeftArmRadius, maxLeftArmRadius,
				rightBackArmPos, rightFrontArmPos, minRightArmRadius, maxRightArmRadius);

			Vector3 direction = endPos - startPos;
			this.endPos = endPos;
			this.startPos = startPos;
			this.direction = direction;
			this.minDistance = minDistance;
			this.maxDistance = maxDistance;
			this.distance = direction.magnitude;
			float horizontalGamma = 1f - Mathf.InverseLerp(minDistance, maxDistance, new Vector3(direction.x, 0f, direction.z).magnitude);
			float verticalGamma = 1f - Mathf.InverseLerp(minDistance, maxDistance, new Vector3(0f, direction.y, 0f).magnitude);
			this.gamma = Mathf.Min(horizontalGamma, verticalGamma);


			this.separationLevel = (int)Mathf.Lerp(1.99f, nbSeparationLevels + 0.99f, 1f - this.gamma);
			float coeff = Mathf.Max(0, separationLevel - 1) / (float)(nbSeparationLevels - 1f);
			this.proximity = 1f - coeff;

			/*if (Row == 0 && Column == 1)
			{
				Debug.DrawLine(startPos, startPos + new Vector3(direction.x, 0f, direction.z), new Color(horizontalGamma, 0f, 0f, 1f));
				Debug.DrawLine(startPos, startPos + new Vector3(0f, direction.y, 0f), new Color(verticalGamma, 0f, 0f, 1f));
				Debug.Log("direction(" + direction + "), distance(" + distance + "), gamma(" + gamma + "), separation(" + separationLevel + ")");
			}*/
			return;

			/* Check user proximy level 1 */
			/*for (int level = 0; level < nbSeparationLevels; level++)
			{
				RaycastHit hit;
				bool touched = Physics.Raycast(startPos - Vector3.up * 100, Vector3.up, out hit, Mathf.Infinity, 1 << (SEPARATION_LAYER + level));
				if (touched)
				{
					Vector3 hitPoint = hit.point;
					if ((hitPoint.y - startPos.y) <= minDistanceFromLayer)
					{
						separationLevel = level;
						float coeff = Mathf.Max(0, level - 1) / (float)(nbSeparationLevels - 1f);
						proximity = 1f - coeff;
						//Debug.Log(pinPoint + " => " + proximity);
						Debug.DrawLine(startPos, hitPoint, Color.HSVToRGB(coeff, 0f, 0f));
						return;
					}
				}
			}*/

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
