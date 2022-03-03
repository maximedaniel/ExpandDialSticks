
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using TMPro;
using System;
using System.Globalization;
using Leap.Unity;
using Random = UnityEngine.Random;
using System.Linq;

public class DebugUnPausedPin : MonoBehaviour
{

	// ExpanDialSticks Core
	public GameObject expanDialSticksPrefab;
	private MyCapsuleHand leftHand;
	private MyCapsuleHand rightHand;
	private SafeGuard safeGuard;
	private ExpanDialSticks expanDialSticks;
	private bool connected;
	private Vector2Int prevSelect;
	private Vector2Int currSelect;
	private bool shapeChangeUp;


	void Start()
	{
		expanDialSticks = expanDialSticksPrefab.GetComponent<ExpanDialSticks>();
		// Listen to events
		expanDialSticks.OnConnecting += HandleConnecting;
		expanDialSticks.OnConnected += HandleConnected;
		expanDialSticks.OnDisconnected += HandleDisconnected;
		expanDialSticks.OnRotationChanged += HandleRotationChanged;

		prevSelect = new Vector2Int(-1, -1);
		currSelect = new Vector2Int(-1, -1);
		connected = false;
		shapeChangeUp = true; //UP
		expanDialSticks.client_MqttConnect();

	}

	void TriggerShapeChangeUp()
	{
		expanDialSticks.modelMatrix[2, 2].TargetPosition = 40;
		expanDialSticks.modelMatrix[2, 2].TargetShapeChangeDuration = 4f;
		expanDialSticks.triggerShapeChange();
	}
	void TriggerShapeChangeDown()
	{
		expanDialSticks.modelMatrix[2, 2].TargetPosition = 0;
		expanDialSticks.modelMatrix[2, 2].TargetShapeChangeDuration = 4f;
		expanDialSticks.triggerShapeChange();

	}

	private void HandleConnecting(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("Application connecting to MQTT Broker @" + e.address + ":" + e.port + "...");
		connected = false;
	}

	private void HandleConnected(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("Application connected.");
		connected = true;

	}

	private void HandleDisconnected(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("Application disconnected.");
		connected = false;
	}

	private void HandleRotationChanged(object sender, ExpanDialStickEventArgs e)
	{
		currSelect = new Vector2Int(e.i, e.j);
		if(currSelect != prevSelect)
		{
			if (shapeChangeUp)
				TriggerShapeChangeUp();
			else
				TriggerShapeChangeDown();
			shapeChangeUp = !shapeChangeUp;
			prevSelect = currSelect;
		}

	}
	void Quit()
	{
#if UNITY_EDITOR
		// Application.Quit() does not work in the editor so
		// UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
		UnityEditor.EditorApplication.isPlaying = false;
#else
						Application.Quit();
#endif
	}

	void Update()
	{
		// check if ExpanDialSticks is connected
		if (connected)
		{
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				HandleRotationChanged(new object(), new ExpanDialStickEventArgs(DateTime.Now, 0, 1, 1, 0, -1));
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
					HandleRotationChanged(new object(), new ExpanDialStickEventArgs(DateTime.Now, 0, 0, 1, 0, -1));
				//currentRotation -= anglePerStep;
			}

			if (Input.GetKey("escape"))
			{
				Quit();
			}
		}
	}
}