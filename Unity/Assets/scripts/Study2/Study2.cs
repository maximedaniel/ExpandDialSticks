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

public class Study2 : MonoBehaviour
{

	public GameObject expanDialSticksPrefab;
	public GameObject capsuleHandLeftPrefab;
	public GameObject capsuleHandRightPrefab;
	private MyCapsuleHand leftHand;
	private MyCapsuleHand rightHand;
	public GUISkin guiSkin;
	public int numeroParticipant = 0;
	public int[] engagementRows;
	public int[] engagementColumns;
	public bool logEnabled = true;
	private ExpanDialSticks expanDialSticks;
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

	private const int nbTrials = 9;

	private Vector2[] molePositions;

	private int moleIndex;
	private const int MOLE_TO_APPEAR = 0;
	private const int MOLE_APPEARING = 1;
	private const int MOLE_APPEARED = 2;
	private const int LANDSCAPE_IS_CHANGING = 3;

	private int moleState = MOLE_TO_APPEAR;
	private bool nextMole;

	//private FileLogger fileLogger;
	public const float LOG_INTERVAL = 0.25f; // 0.2f;
	private float currTime;
	private float prevTime;

	public const string MQTT_CAMERA_RECORDER = "CAMERA_RECORDER";
	public const string MQTT_EMPATICA_RECORDER = "EMPATICA_RECORDER";
	public const string MQTT_SYSTEM_RECORDER = "SYSTEM_RECORDER";
	public const string CMD_START = "START";
	public const string CMD_STOP = "STOP";


	private MqttClient client;

	private int I = 2;
	private int J = 2;
	private float currentRotation = 90f;
	private float targetRotation = 90f;
	private float speedRotation = 1f;

	public float currentTime = 0f;
	public float turnDuration = 3f;
	public float anglePerStep = 360f/24f;
	public float startRotation = 90f;
	public float endRotation = 270f;

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
		moleIndex = -1;
		moleState = MOLE_TO_APPEAR;
		//fileLogger = new FileLogger(logEnabled);
		currTime = LOG_INTERVAL;
		prevTime = 0f;
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
		connected = true;

		expanDialSticks.modelMatrix[I, J].TargetPosition = 20;
		expanDialSticks.modelMatrix[I, J].TargetShapeChangeDuration = 2f;
		expanDialSticks.modelMatrix[I, J].TargetPlaneTexture = "aiguille";
		expanDialSticks.modelMatrix[I, J].TargetPlaneRotation = currentRotation;

		expanDialSticks.modelMatrix[I, J].TargetTextureChangeDuration = 2f;
		expanDialSticks.triggerShapeChange();
		expanDialSticks.triggerTextureChange();

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
	void MoveAiguille()
	{
		expanDialSticks.modelMatrix[I, J].TargetPlaneRotation = currentRotation;
		expanDialSticks.modelMatrix[I, J].TargetTextureChangeDuration = 0.1f;
		expanDialSticks.triggerTextureChange();
	}
	void Update()
	{
		// check if ExpanDialSticks is connected
		if (connected)
		{
			if (Input.GetKey("escape"))
			{
				//Quit();
			}

			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				currentRotation += anglePerStep;
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				currentRotation -= anglePerStep;
			}
			currentRotation = Mathf.MoveTowardsAngle(currentRotation, targetRotation, speedRotation * Time.deltaTime);
			if (Mathf.Approximately(currentRotation, targetRotation)) {
				targetRotation = UnityEngine.Random.Range(0f, 360f);
				speedRotation = Mathf.Clamp((targetRotation - currentRotation) / 3f, 5f, 15f) ;
			} else
			{
				if (Time.time - currentTime >= turnDuration)
				{
					targetRotation = UnityEngine.Random.Range(0f, 360f);
					speedRotation = Mathf.Clamp((targetRotation - currentRotation) / 3f, 5f, 15f);
					turnDuration = UnityEngine.Random.Range(3f, 5f);
					currentTime = Time.time;
				}
			}

			MoveAiguille();
		}
	}
}
