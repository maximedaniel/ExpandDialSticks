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

	//private List<GameObject> goList = new List<GameObject>();
	private bool isColliding = false;
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
		set => this.height = value;
	}

	public float Offset
	{
		get => this.offset;
		set => this.offset = value;
	}

	public bool IsColliding()
	{
		return isColliding;
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
		// Bit shift the index of the layer (8) to get a bit mask
		int layerMask = 1 << 8;

		// This would cast rays only against colliders in layer 8.
		// But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
		layerMask = ~layerMask;


		bool touched = false;
		RaycastHit hit;

		Vector3 center = new Vector3(i * (diameter + offset), 0f, j * (diameter + offset));

		// dialstick oriented
		center = transform.position;

		touched = Physics.Raycast(center + transform.up * 100, -transform.up, out hit, Mathf.Infinity, layerMask);
		if (touched)
		{
			Debug.DrawLine(center, hit.point, Color.yellow);
			isColliding = true;
		} else {
			isColliding = false;
		}
		/*Vector3 right = transform.position + (Vector3.right * transform.localScale.x) / proximity;
		touched = Physics.Raycast(right + Vector3.up * 100, -Vector3.up, out hit, Mathf.Infinity, layerMask);
		if (touched)
		{
			Debug.DrawLine(right, hit.point, Color.yellow);
			isColliding = true;
			return;
		}
		else isColliding = false;

		Vector3 left = transform.position - (Vector3.right * transform.localScale.x) / proximity;
		touched = Physics.Raycast(left + Vector3.up * 100, -Vector3.up, out hit, Mathf.Infinity, layerMask);
		if (touched)
		{
			Debug.DrawLine(left, hit.point, Color.yellow);
			isColliding = true;
			return;
		}
		Vector3 front = transform.position + (Vector3.forward * transform.localScale.z) / proximity;
		touched = Physics.Raycast(front + Vector3.up * 100, -Vector3.up, out hit, Mathf.Infinity, layerMask);
		if (touched)
		{
			Debug.DrawLine(front, hit.point, Color.yellow);
			isColliding = true;
			return;
		}
		Vector3 back = transform.position - (Vector3.forward * transform.localScale.z) / proximity;
		touched = Physics.Raycast(back + Vector3.up * 100, -Vector3.up, out hit, Mathf.Infinity, layerMask);
		if (touched)
		{
			Debug.DrawLine(back, hit.point, Color.yellow);
			isColliding = true;
			return;
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
