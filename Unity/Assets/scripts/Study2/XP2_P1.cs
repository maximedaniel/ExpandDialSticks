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


	private const int SAFETY_CHANGING = 0;
	private const int SAFETY_CHANGED = 1;

	private int state = SAFETY_CHANGED;
	private bool nextMole;

	//private FileLogger fileLogger;
	public float LOG_INTERVAL = 0.2f; // 0.2f;
	private float currTime;
	private float prevTime;

	public const string MQTT_CAMERA_RECORDER = "CAMERA_RECORDER";
	public const string MQTT_EMPATICA_RECORDER = "EMPATICA_RECORDER";
	public const string MQTT_SYSTEM_RECORDER = "SYSTEM_RECORDER";
	public const string CMD_START = "START";
	public const string CMD_STOP = "STOP";

	public float shapeChangeDuration = 2f;
	/*private  ExpanDialStickView.FeedbackMode[] feedbackModes = new ExpanDialStickView.FeedbackMode[] {
	ExpanDialStickView.FeedbackMode.PulseOut,
	ExpanDialStickView.FeedbackMode.Blink,
	ExpanDialStickView.FeedbackMode.PulseIn,
	ExpanDialStickView.FeedbackMode.None
	}; */
	private ExpanDialStickView.FeedbackMode[] feedbackModes = new ExpanDialStickView.FeedbackMode[] {
	ExpanDialStickView.FeedbackMode.Blink,
	ExpanDialStickView.FeedbackMode.Blink,
	ExpanDialStickView.FeedbackMode.Blink,
	ExpanDialStickView.FeedbackMode.Blink
	};
	private sbyte[] heights = new sbyte[] {
	0,
	20,
	30,
	0,
	};
	private int nbShownIndex = 0;
	int[] nbPinToShow = new int[] {0, 2, 4, 1, 3 }; 

	private MqttClient client;


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
		//fileLogger = new FileLogger(logEnabled);
		currTime = LOG_INTERVAL;
		prevTime = 0f;
		nbShownIndex = -1;
		// Connection to MQTT Broker
		expanDialSticks.client_MqttConnect();
	}

	private void OnDestroy()
	{


		expanDialSticks.client.Publish(MQTT_CAMERA_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_STOP), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		expanDialSticks.client.Publish(MQTT_EMPATICA_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_STOP), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_STOP), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

		//fileLogger.Log("END_APPLICATION");

		//fileLogger.Close();
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
	public int[] Shuffle(int[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			int rnd = Random.Range(0, array.Length);
			int temp = array[rnd];
			array[rnd] = array[i];
			array[i] = temp;
		}
		return array;
	}

	IEnumerator ShowDifferentSafetyFeedbackModes()
	{
		int[] availables = new int[] {0,1,2,3,4};
		int[] shuffledAvailables = Shuffle(availables);
		int nbShown = nbPinToShow[nbShownIndex];
		int y = 4;
		for (int i = 0; i < availables.Length; i++)
		{
			int x = availables[i];
			expanDialSticks.modelMatrix[x, y].SafetyFeedForwardEnabled = false;
			expanDialSticks.modelMatrix[x, y].SafetyFeedbackMode = ExpanDialStickView.FeedbackMode.None;
		}
		expanDialSticks.triggerSafetyChange();

		for (int i = 0; i < availables.Length; i++)
		{
			int x = availables[i];
			expanDialSticks.modelMatrix[x, y].TargetPosition = 20;
			expanDialSticks.modelMatrix[x, y].TargetShapeChangeDuration = 2f;
		}
		expanDialSticks.triggerShapeChange();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				//expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
				//expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
				expanDialSticks.modelMatrix[i, j].TargetColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = 0.1f;
			}
		}
		expanDialSticks.triggerTextureChange();

		yield return new WaitForSeconds(3f);
		// select pin to show
		for (int i = 0; i < nbShown; i++)
		{
			int x = shuffledAvailables[i];
			expanDialSticks.modelMatrix[x, y].SafetyFeedForwardEnabled = true;
			expanDialSticks.modelMatrix[x, y].SafetyFeedbackMode = feedbackModes[i];
		}
		expanDialSticks.triggerSafetyChange();

		/*int x = 2;
		int y = 2;
		Debug.Log(heights[feedbackModeIndex] + ", " + feedbackModes[feedbackModeIndex]);
		expanDialSticks.modelMatrix[x, y].SafetyFeedForwardEnabled = false;
		expanDialSticks.triggerSafetyChange();
		expanDialSticks.modelMatrix[x, y].TargetPosition = heights[feedbackModeIndex];
		expanDialSticks.modelMatrix[x, y].TargetShapeChangeDuration = 2f;
		expanDialSticks.triggerShapeChange();
		yield return new WaitForSeconds(2f);
		expanDialSticks.modelMatrix[x, y].SafetyFeedForwardEnabled = true;
		expanDialSticks.modelMatrix[x, y].SafetyFeedbackMode = feedbackModes[feedbackModeIndex];
		expanDialSticks.triggerSafetyChange();*/

		/*int y = 4;
		int x0 = 1;
		int x1 = 2;
		int x2 = 3;


		expanDialSticks.modelMatrix[x0, y].SafetyFeedForwardEnabled = true;
		expanDialSticks.modelMatrix[x0, y].SafetyFeedbackMode = ExpanDialStickView.FeedbackMode.Blink;
		expanDialSticks.modelMatrix[x1, y].SafetyFeedForwardEnabled = true;
		expanDialSticks.modelMatrix[x1, y].SafetyFeedbackMode = ExpanDialStickView.FeedbackMode.Blink;
		expanDialSticks.modelMatrix[x2, y].SafetyFeedForwardEnabled = true;
		expanDialSticks.modelMatrix[x2, y].SafetyFeedbackMode = ExpanDialStickView.FeedbackMode.Blink;
		expanDialSticks.triggerSafetyChange();
		yield return new WaitForSeconds(3f);*/
		state = SAFETY_CHANGED;

	}
	void Update()
	{
		// check if ExpanDialSticks is connected
		if (connected)
		{
			if (Input.GetKey("escape") || nbShownIndex >= nbPinToShow.Length)
			{
				Quit();
			}

			if (Input.GetKeyDown("n"))
			{
				if (state == SAFETY_CHANGED && ++nbShownIndex < nbPinToShow.Length)
				{
					state = SAFETY_CHANGING;

					StartCoroutine(ShowDifferentSafetyFeedbackModes());
				}
			}
		}
	}
}
