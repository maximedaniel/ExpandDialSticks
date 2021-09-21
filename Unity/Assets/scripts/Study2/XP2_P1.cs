#define DEBUG

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

public class XP2_P1 : MonoBehaviour
{

	public GameObject expanDialSticksPrefab;
	public GameObject capsuleHandLeftPrefab;
	public GameObject capsuleHandRightPrefab;

	private ExpanDialSticks expanDialSticks;
	private MyCapsuleHand leftHand;
	private MyCapsuleHand rightHand;

	public GUISkin guiSkin;
	public int numeroParticipant = 0;
	public int[] engagementRows;
	public int[] engagementColumns;

	public bool logEnabled = true;
	private bool connected = false;

	void Start()
	{
		leftHand = capsuleHandLeftPrefab.GetComponent<MyCapsuleHand>();
		rightHand = capsuleHandRightPrefab.GetComponent<MyCapsuleHand>();
		expanDialSticks = expanDialSticksPrefab.GetComponent<ExpanDialSticks>();
		// Listen to events
		expanDialSticks.OnConnecting += HandleConnecting;
		expanDialSticks.OnConnected += HandleConnected;
		expanDialSticks.OnDisconnected += HandleDisconnected;
		expanDialSticks.OnXAxisChanged += HandleXAxisChanged;
		expanDialSticks.OnYAxisChanged += HandleYAxisChanged;
		expanDialSticks.OnClickChanged += HandleClickChanged;
		expanDialSticks.OnRotationChanged += HandleRotationChanged;
		expanDialSticks.OnPositionChanged += HandlePositionChanged;
		expanDialSticks.onHoldingChanged += HandleHoldingChanged;
		expanDialSticks.onReachingChanged += HandleReachingChanged;

		connected = false;

		// init trials
		expanDialSticks.client_MqttConnect();
	}

	private void OnDestroy()
	{

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

	private void HandleXAxisChanged(object sender, ExpanDialStickEventArgs e)
	{

	}

	private void HandleYAxisChanged(object sender, ExpanDialStickEventArgs e)
	{

	}

	private void HandleClickChanged(object sender, ExpanDialStickEventArgs e)
	{

	}

	private void HandleRotationChanged(object sender, ExpanDialStickEventArgs e)
	{
	}

	private void HandlePositionChanged(object sender, ExpanDialStickEventArgs e)
	{

	}

	private void HandleReachingChanged(object sender, ExpanDialStickEventArgs e)
	{

	}

	private void HandleHoldingChanged(object sender, ExpanDialStickEventArgs e)
	{

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
			if (Input.GetKey("escape"))
			{
				Quit();
			}
		}
	}
}
 