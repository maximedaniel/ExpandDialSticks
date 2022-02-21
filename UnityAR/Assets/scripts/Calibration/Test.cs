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

public class Test : MonoBehaviour
{

	public GameObject expanDialSticksPrefab;
	private ExpanDialSticks expanDialSticks;
	public GameObject SARCameraPrefab;
	private Camera SARCamera;
	private MyCapsuleHand leftHand;
	private MyCapsuleHand rightHand;
	public GUISkin guiSkin;
	public int numeroParticipant = 0;
	public int[] engagementRows;
	public int[] engagementColumns;
	public bool logEnabled = true;
	private bool connected = false;

	private CultureInfo en = CultureInfo.CreateSpecificCulture("en-US");

	private IEnumerator coroutine;

	private const string LEFT_BENDING = "LEFT_BENDING";
	private const string RIGHT_BENDING = "RIGHT_BENDING";
	private const string TOP_BENDING = "TOP_BENDING";
	private const string BOTTOM_BENDING = "BOTTOM_BENDING";
	private const string LEFT_ROTATION = "LEFT_ROTATION";
	private const string RIGHT_ROTATION = "RIGHT_ROTATION";
	private const string PULL = "PULL";
	private const string PUSH = "PUSH";
	private const string CLICK = "CLICK";
	private const string NONE = "NONE";

	private const float JOYSTICK_THRESHOLD = 10f;

	private string stringIndex = "";
	private string stringPosition = "";
	private int currentIndex = 0;

	private string stringCameraX = "";
	private string stringCameraY = "";
	private string stringCameraZ = "";

	//private FileLogger fileLogger;
	public const float LOG_INTERVAL = 0.25f; // 0.2f;
	public const string MQTT_CAMERA_RECORDER = "CAMERA_RECORDER";
	public const string MQTT_EMPATICA_RECORDER = "EMPATICA_RECORDER";
	public const string MQTT_SYSTEM_RECORDER = "SYSTEM_RECORDER";
	public const string CMD_START = "START";
	public const string CMD_STOP = "STOP";


	void Start()
	{
		expanDialSticks = expanDialSticksPrefab.GetComponent<ExpanDialSticks>();
		SARCamera = SARCameraPrefab.GetComponent<Camera>();
		stringCameraX = SARCamera.transform.position.x.ToString();
		stringCameraY = SARCamera.transform.position.y.ToString();
		stringCameraZ = SARCamera.transform.position.z.ToString();
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
		currentIndex = 0;
		//fileLogger = new FileLogger(logEnabled);
		// Connection to MQTT Broker
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
		expanDialSticks.client.Publish(MQTT_CAMERA_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_START), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		expanDialSticks.client.Publish(MQTT_EMPATICA_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_START), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_START), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
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
	void OnGUI()
	{
		// Make a text field that modifies stringToEdit.
		float midX = Screen.width / 2.0f;
		float midY = Screen.height / 2.0f;
		float componentHeight = 20;
		float componentWidth = 50;

		stringCameraX = GUI.TextField(new Rect(midX - 100, 50, 50, componentHeight), stringCameraX, 25);
		stringCameraY = GUI.TextField(new Rect(midX - 50, 50, 50, componentHeight), stringCameraY, 25);
		stringCameraZ = GUI.TextField(new Rect(midX     , 50, 50, componentHeight), stringCameraZ, 25);


		if (GUI.Button(new Rect(midX + 50, 50, componentWidth, componentHeight), "CAMERA"))
		{
			float cameraX = float.Parse(stringCameraX);
			float cameraY = float.Parse(stringCameraY);
			float cameraZ = float.Parse(stringCameraZ);
			SARCameraPrefab.transform.position = new Vector3(cameraX, cameraY, cameraZ);
		}

		int index = 0;
		int position = 0;

		stringIndex = GUI.TextField(new Rect(midX - 25, 25, 50, componentHeight), stringIndex, 25);
		stringPosition = GUI.TextField(new Rect(midX + 25, 25, 50, componentHeight), stringPosition, 25);

		if (GUI.Button(new Rect(midX+75, 25, componentWidth, componentHeight), "SEND"))
			{
				index = int.Parse(stringIndex);
				position = int.Parse(stringPosition);
				Debug.Log(index + " " + position);
				int row = (int)(index / (float)expanDialSticks.NbColumns);
				int column = index % expanDialSticks.NbColumns;
				Debug.Log(row + " " + column);
				expanDialSticks.modelMatrix[row, column].TargetPosition = (sbyte)position;
				expanDialSticks.modelMatrix[row, column].TargetShapeChangeDuration = 2f;
				expanDialSticks.triggerShapeChange();
		
		}
		if (GUI.Button(new Rect(midX + 150, 25, componentWidth, componentHeight), "CROSS"))
		{
			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					expanDialSticks.modelMatrix[i, j].TargetColor = Color.red;
					expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = 0.1f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontTexture = "cross-reverse";
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontColor = Color.white;
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontRotation = 90f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontSize = 0.02f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontChangeDuration = 0.1f;
				}
			}
			expanDialSticks.triggerProjectorChange();
			expanDialSticks.triggerTextureChange();
		}
	}

	void Update()
	{
		// check if ExpanDialSticks is connected
		if (connected)
		{
			if (Input.GetKey("escape") || (currentIndex > expanDialSticks.NbRows * expanDialSticks.NbColumns - 1))
			{
				Quit();
			}

			if (Input.GetKeyDown("n"))
			{
				int i = currentIndex / expanDialSticks.NbColumns;// int i = 4;
				int j  = currentIndex % expanDialSticks.NbColumns; // int j = 3;

				expanDialSticks.modelMatrix[i, j].TargetPosition = 30;
				expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = 1f;
				expanDialSticks.triggerShapeChange();
				currentIndex++;
				Debug.Log(i + " " + j + " " + 30);
			}
		}
	}
}
