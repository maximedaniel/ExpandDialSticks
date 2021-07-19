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

public class ExpanDialStickCollision: MonoBehaviour
{
	private float diameter = 4.0f;
	private float height = 10.0f;
	private float offset = 0.5f;

	private int i = 0;
	private int j = 0;
	private int nbSeparationLevels = 3;

	//private List<GameObject> goList = new List<GameObject>();

	private const int SEPARATION_LAYER = 10; // Safety Level 0
	private float proximity = 0f;
	private float minDistanceFromLayer = 10.0f;
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

	public float Height
	{
		get => this.height;
		set {
			this.minDistanceFromLayer = value;
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
		Vector3 pinHeadPoint = (transform.position + transform.up * (height / 2f));
		/* Check user proximy level 1 */
		for (int level = 0; level < nbSeparationLevels; level++)
		{
			RaycastHit hit;
			bool touched = Physics.Raycast(transform.position - transform.up * 100, transform.up, out hit, Mathf.Infinity, 1 << (SEPARATION_LAYER + level));
			if (touched)
			{
				Vector3 hitPoint = hit.point;
				if ((hitPoint.y - pinHeadPoint.y) <= minDistanceFromLayer)
				{
					float coeff = level / (float)nbSeparationLevels;
					proximity = 1f - coeff;
					Debug.DrawLine(transform.position - transform.up * 100, hitPoint, Color.HSVToRGB(coeff, 1f, 1f));
					return;
				}
			}
		}
		proximity = 0f;
		/*
		RaycastHit hitLevel0;
		RaycastHit hitLevel1;
		RaycastHit hitLevel2;
		bool touchedLevel0 = Physics.Raycast(transform.position - transform.up * 100, transform.up, out hitLevel0, Mathf.Infinity, 1 << SEPARATION_LAYER_0);
		bool touchedLevel1 = Physics.Raycast(transform.position - transform.up * 100, transform.up, out hitLevel1, Mathf.Infinity, 1 << SEPARATION_LAYER_1);
		bool touchedLevel2 = Physics.Raycast(transform.position - transform.up * 100, transform.up, out hitLevel2, Mathf.Infinity, 1 << SEPARATION_LAYER_2);

		Vector3 pinHeadPoint = (transform.position + transform.up * (height / 2f));
		if (touchedLevel0)
		{
			Vector3 hitLevel0Point = hitLevel0.point;
			if ((hitLevel0Point.y - pinHeadPoint.y) <= minDistanceFromLayer)
			{
				proximity = 1f;
				Debug.DrawLine(transform.position - transform.up * 100, hitLevel0.point, Color.red);
				return;
			}
		}
		else if (touchedLevel1)
		{
			Vector3 hitLevel1Point = hitLevel1.point;
			if ((hitLevel1Point.y - pinHeadPoint.y) <= minDistanceFromLayer)
			{
				proximity = 0.66f;
				Debug.DrawLine(transform.position - transform.up * 100, hitLevel1.point, Color.yellow);
				return;
			}
		}
		else if (touchedLevel2)
		{
			Vector3 hitLevel2Point = hitLevel2.point;
			if ((hitLevel2Point.y - pinHeadPoint.y) <= minDistanceFromLayer)
			{
				proximity = 0.33f;
				Debug.DrawLine(transform.position - transform.up * 100, hitLevel2.point, Color.white);
				return;
			}
		} else
		{
			proximity = 0f;
		}*/
		/*if (touchedLevel0)
		{
			Vector3 hitLevel0Point = hitLevel0.point;
			Vector3 pinHeadPoint = (transform.position + transform.up * (height/2f));
			float distanceBetweenPoints = Vector3.Distance(hitLevel0Point, pinHeadPoint);

			Color color = new Color(1f, 0f, 0f, isColliding);
			if (hitLevel0Point.y > pinHeadPoint.y) // pin under collider
			{
				if(distanceBetweenPoints >= minDistanceFromUserBody)
				{
					color.a = isColliding = 1f - (distanceBetweenPoints - minDistanceFromUserBody) / height;
					color.a = isColliding;
					Debug.DrawLine(transform.position - transform.up * 100, hitLevel0.point, color);
				} else
				{
					isColliding = 1f;
					color.a = isColliding;
					Debug.DrawLine(transform.position - transform.up * 100, hitLevel0.point, color);
				}

			} else // pin  inside collider
			{
				isColliding = 1f;
				color.a = isColliding;
				Debug.DrawLine(transform.position - transform.up * 100, hitStop.point, color);
			}
		}
		else
		{
			isColliding = 0f;
		}*/
	}

	private void OnTriggerStay(Collider other)
	{

	}

	private void OnTriggerEnter(Collider other)
	{
		/*goList.Add(other.gameObject);
		Debug.Log("["+ i + ", " + j +"] entered -> " + other.gameObject.name);

		if (goList.Count == 1)
		{
			isColliding = true;
			Debug.Log("["+ i + ", " + j +"] colliding...");
		}*/
		//isColliding = true;
	}
	private void OnTriggerExit(Collider other)
	{
		/*goList.Remove(other.gameObject);
		Debug.Log("[" + i + ", " + j + "] exited -> " + other.gameObject.name);
		if (goList.Count == 0)
		{
			isColliding = false;
			Debug.Log("[" + i + ", " + j + "] ...collision ended.");
		}*/
		//isColliding = false;


	}
}
