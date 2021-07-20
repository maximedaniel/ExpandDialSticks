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

public class XP1_P2 : MonoBehaviour
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

	private Vector2[] gaugePositions;

	private int gaugeIndex;
	private const int GAUGE_TO_APPEAR = 0;
	private const int GAUGE_APPEARING = 1;
	private const int GAUGE_APPEARED = 2;
	private const int LANDSCAPE_IS_CHANGING = 3;

	private int gaugeState = GAUGE_TO_APPEAR;
	private bool nextGauge;

	//private FileLogger fileLogger;
	public const float LOG_INTERVAL = 0.2f; // 0.2f;
	private float currTime;
	private float prevTime;

	public const string MQTT_CAMERA_RECORDER = "CAMERA_RECORDER";
	public const string MQTT_EMPATICA_RECORDER = "EMPATICA_RECORDER";
	public const string MQTT_SYSTEM_RECORDER = "SYSTEM_RECORDER";
	public const string CMD_START = "START";
	public const string CMD_STOP = "STOP";
	public float shapeChangeDuration = 2f;


	private MqttClient client;

	private float currentRotation = 90f;
	private float targetRotation = 90f;
	private float speedRotation = 1f;

	private float directionTime = 0f;
	private float directionDuration = 3f;
	private float startGameTime = 0f;
	private float gameDuration = 20f;
	private float startMotionTime = 0f;
	private float motionDuration = 10f;

	private const float anglePerStep = 360f / 24f;
	private float startRotation = 90f - anglePerStep;
	private float endRotation = 270f;
	public enum DirectionRotation { CW, CCW, IDDLE };
	private DirectionRotation directionRotation = DirectionRotation.CW;

	IEnumerator NextGauge()
	{
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_BLACKING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		AllBlack(0.5f);
		yield return new WaitForSeconds(0.5f);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_DESCENDING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		AllDown(shapeChangeDuration);
		yield return new WaitForSeconds(1f);
		// wait until all pins are down
		while (!IsAllDown())
		{
			yield return new WaitForSeconds(0.1f);
		}
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("GAUGE_TO_APPEAR"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		gaugeState = GAUGE_TO_APPEAR;
	}

	IEnumerator ShowGauge()
	{

		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("GAUGE_APPEARING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		GaugeUp(shapeChangeDuration);
		yield return new WaitForSeconds(1f);
		// wait until all pins are down
		while (!IsGaugeUp())
		{
			yield return new WaitForSeconds(0.1f);
		}
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("GAUGE_DISPLAYING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		currentRotation = startRotation;
		GaugeInit(0.5f);
		yield return new WaitForSeconds(0.5f);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("GAUGE_APPEARED"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		startGameTime = Time.time;
		motionDuration = Random.Range(5f, gameDuration - 5f);
		//gaugeState = GAUGE_APPEARED;
	}

	IEnumerator Earthquake()
	{
		// trigger most unsafe SC
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_ASCENDING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		AllUpExceptGauge(shapeChangeDuration);
		yield return new WaitForSeconds(3f);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_DESCENDING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		AllDownExceptGauge(shapeChangeDuration);
		yield return new WaitForSeconds(3f);
		/*expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("TRIGGER_LANDSCAPE_BLACKING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		AllBlack(0.5f);
		yield return new WaitForSeconds(0.5f);*/
		// wait until all pins are down
		/*while (!IsAllDownExceptGauge())
		{
			yield return new WaitForSeconds(0.1f);
		}*/
		//expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("END_LANDSCAPE_CHANGING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		gaugeState = GAUGE_APPEARED;
	}

	/*void AllReset(float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.black;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
				expanDialSticks.modelMatrix[i, j].TargetPosition = -1;
				expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = duration;

			}
		}
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerShapeChange();
	}


	void AllUp(float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
				expanDialSticks.modelMatrix[i, j].TargetPosition = 40;
				expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = duration;
			}
		}
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerShapeChange();
	}*/
	void AllDown(float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
				expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
				expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = duration;
			}
		}
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerShapeChange();
	}
	bool IsAllDown()
	{
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				if (expanDialSticks.viewMatrix[i, j].CurrentPosition > 0 || expanDialSticks.viewMatrix[i, j].CurrentReaching) return false;
			}
		}
		return true;
	}
	void AllUpExceptGauge(float duration)
	{
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				if (i != (int)gaugePosition.x || j != (int)gaugePosition.y)
				{
					expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
					expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
					expanDialSticks.modelMatrix[i, j].TargetPosition = 40;
					expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = duration;
				}

			}
		}
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerShapeChange();
	}
	void AllDownExceptGauge(float duration)
	{
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				if (i != (int)gaugePosition.x || j != (int)gaugePosition.y)
				{
					expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
					expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
					expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
					expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = duration;
				}
			}
		}
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerShapeChange();
	}

	void AllBlack(float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
				expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = currentRotation;

				expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "projector";
				expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = 270f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorColor = Color.white;
				expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = duration;

				expanDialSticks.modelMatrix[i, j].TargetColor = Color.black;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
			}
		}
		expanDialSticks.triggerTextureChange();
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

	private void InitTrials()
	{

		gaugePositions = new Vector2[engagementRows.Length * engagementRows.Length];

		// Generate Squared-Latin Row Indexes
		int[] shuffledRowIndexes = new int[engagementRows.Length * engagementRows.Length];
		for (int i = 0; i < engagementRows.Length; i++)
		{
			for (int j = 0; j < engagementRows.Length; j++)
			{
				shuffledRowIndexes[i * engagementRows.Length + j] = engagementRows[(i + numeroParticipant + j) % engagementRows.Length];

			}
		}
		// Generate Shuffled Column Indexes
		int[] shuffledColumnsIndexes = new int[engagementRows.Length * engagementRows.Length];
		for (int i = 0; i < engagementRows.Length; i++)
		{
			engagementColumns = Shuffle(engagementColumns);

			for (int j = 0; j < engagementRows.Length; j++)
			{
				shuffledColumnsIndexes[i * engagementRows.Length + j] = engagementColumns[j];
			}
		}

		for (int i = 0; i < engagementRows.Length * engagementRows.Length; i++)
		{
			gaugePositions[i] = new Vector2(shuffledRowIndexes[i], shuffledColumnsIndexes[i]);
		}
	}

	bool IsGaugeUp()
	{
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		return !expanDialSticks.viewMatrix[(int)gaugePosition.x, (int)gaugePosition.y].CurrentReaching && expanDialSticks.viewMatrix[(int)gaugePosition.x, (int)gaugePosition.y].TargetPosition == 20;
	}

	void GaugeUp(float duration)
	{
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				if (i == (int)gaugePosition.x && j == (int)gaugePosition.y)
					expanDialSticks.modelMatrix[i, j].TargetPosition = 20;
				else
					expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
				expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = duration;
			}
		}
		expanDialSticks.triggerShapeChange();
	}
	void GaugeInit(float duration)
	{
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.white; //Color.green;
				if (i == (int)gaugePosition.x && j == (int)gaugePosition.y)
				{
					expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "aiguille";
					expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = currentRotation;

					expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "cadran";
					expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = 270f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 2f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorColor = Color.red;
				}

				else
				{
					expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
					expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = currentRotation;

					expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "projector";
					expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = 270f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorColor = Color.white;
				}

				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
				expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = duration;
			}
		}
		string participantNumber = "<pos=0%><b>P" + numeroParticipant + "</b>";
		string trialProgress = "<pos=90%><b>" + (gaugeIndex + 1) + "/" + gaugePositions.Length + "</b>";
		string legend = participantNumber + trialProgress;
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 16, Color.black, legend, new Vector3(90f, -90f, 0f));
		expanDialSticks.triggerTextureChange();

	}

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
		InitTrials();
		gaugeIndex = -1;
		gaugeState = GAUGE_TO_APPEAR;
		//fileLogger = new FileLogger(logEnabled);
		currTime = LOG_INTERVAL;
		prevTime = 0f;
		// Connection to MQTT Broker
		expanDialSticks.client_MqttConnect();
	}
	private void OnDestroy()
	{
		expanDialSticks.client.Publish(MQTT_CAMERA_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_STOP), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		expanDialSticks.client.Publish(MQTT_EMPATICA_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_STOP), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_STOP), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

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

		/*expanDialSticks.modelMatrix[I, J].TargetPosition = 20;
		expanDialSticks.modelMatrix[I, J].TargetShapeChangeDuration = 2f;
		expanDialSticks.modelMatrix[I, J].TargetPlaneTexture = "aiguille";
		expanDialSticks.modelMatrix[I, J].TargetPlaneRotation = currentRotation;

		expanDialSticks.modelMatrix[I, J].TargetTextureChangeDuration = 2f;
		expanDialSticks.triggerShapeChange();
		expanDialSticks.triggerTextureChange();*/

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

		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		if (e.i == (int)gaugePosition.x && e.j == (int)gaugePosition.y)
		{

			float prevRotation = currentRotation;
			currentRotation -= e.diff * anglePerStep;
			string msg = "";
			if (gaugeState == GAUGE_APPEARING)
			{
				msg += "USER_START_GAUGE" + prevRotation + " " + currentRotation;
				gaugeState = GAUGE_APPEARED;
			} else
			{
				msg += "USER_ROTATE_GAUGE " + prevRotation + " " + currentRotation;
			}
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		}
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
	void LogAllSystemData()
	{
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		int gaugeX = (int)gaugePosition.x;
		int gaugeY = (int)gaugePosition.y;

		//string colorString = "SYSTEM_COLOR ";
		string proximityString = "SYSTEM_PROXIMITY ";
		string positionString = "SYSTEM_POSITION ";
		string leftHandString = "USER_LEFT_HAND " + leftHand.ToString();
		string rightHandString = "USER_RIGHT_HAND " + rightHand.ToString();

		string pinOrientationString = "USER_PIN_ORIENTATION " + expanDialSticks.viewMatrix[gaugeX, gaugeY].CurrentAxisX + " " + expanDialSticks.viewMatrix[gaugeX, gaugeY].CurrentAxisY;
		//string pinRotationString = "USER_PIN_ROTATION " + expanDialSticks.viewMatrix[moleX, moleY].CurrentRotation;

		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				//colorString += "0x" + ColorUtility.ToHtmlStringRGB(expanDialSticks.viewMatrix[i, j].CurrentColor) + " ";
				proximityString += expanDialSticks.viewMatrix[i, j].CurrentProximity + " ";
				positionString += expanDialSticks.viewMatrix[i, j].CurrentPosition + " ";
			}
		}

		//expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(colorString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(proximityString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(positionString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(leftHandString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(rightHandString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(pinOrientationString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		/*fileLogger.Log(colorString);
		fileLogger.Log(proximityString);
		fileLogger.Log(positionString);
		fileLogger.Log(leftHandString);
		fileLogger.Log(rightHandString);
		fileLogger.Log(pinOrientationString);*/
		//fileLogger.Log(pinRotationString);
	}
	void MoveAiguille()
	{
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		int i = (int)gaugePosition.x;
		int j = (int)gaugePosition.y;
		expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = currentRotation;
		expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = 0.1f;
		expanDialSticks.triggerTextureChange();
	}
	void Update()
	{
		// check if ExpanDialSticks is connected
		if (connected)
		{
			if (Input.GetKey("escape") || (gaugeState == GAUGE_TO_APPEAR && gaugeIndex >= gaugePositions.Length))
			{
				Quit();
			}

			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				Vector2 gaugePosition = gaugePositions[gaugeIndex];
				HandleRotationChanged(new object(), new ExpanDialStickEventArgs(DateTime.Now, (int)gaugePosition.x, (int)gaugePosition.y, 0, 1, 1));
				//currentRotation += anglePerStep;
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				Vector2 gaugePosition = gaugePositions[gaugeIndex];
				HandleRotationChanged(new object(), new ExpanDialStickEventArgs(DateTime.Now, (int)gaugePosition.x, (int)gaugePosition.y, 1, 0, -1));
				//currentRotation -= anglePerStep;
			}
			if (gaugeState == GAUGE_TO_APPEAR && ++gaugeIndex < gaugePositions.Length)
			{
				gaugeState = GAUGE_APPEARING;
				StartCoroutine(ShowGauge());
			}
			if (gaugeState == GAUGE_APPEARED || gaugeState == LANDSCAPE_IS_CHANGING)
			{

				if (gaugeState == GAUGE_APPEARED && Time.time - startGameTime >= motionDuration)
				{
					gaugeState = LANDSCAPE_IS_CHANGING;
					StartCoroutine(Earthquake());
					motionDuration = Mathf.Infinity;
				}
				if (gaugeState == GAUGE_APPEARED && Time.time - startGameTime >= gameDuration)
				{
					StartCoroutine(NextGauge());
					gaugeState = GAUGE_APPEARING;
				}
				else
				{
					// Gauge Game
					float prevRotation  = currentRotation;
					switch (directionRotation)
					{
						case DirectionRotation.CW:
							currentRotation += speedRotation * Time.deltaTime;
							break;
						case DirectionRotation.CCW:
							currentRotation -= speedRotation * Time.deltaTime;
							break;
						default:
							break;
					}
					string msg = "SYSTEM_ROTATE_GAUGE " + prevRotation + " " + currentRotation;
					expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

					if (Time.time - directionTime >= directionDuration)
					{
						int nbDirections = Enum.GetNames(typeof(DirectionRotation)).Length - 1; // without IDDLE
						directionRotation = (DirectionRotation)UnityEngine.Random.Range(0, nbDirections);
						speedRotation = UnityEngine.Random.Range(5f, 15f);
						directionDuration = UnityEngine.Random.Range(3f, 9f);
						directionTime = Time.time;
					}
					MoveAiguille();
				}
			}

			if (gaugeState == LANDSCAPE_IS_CHANGING)
			{
				if ((currTime += Time.deltaTime) - prevTime > LOG_INTERVAL)
				{
					LogAllSystemData();
					prevTime = currTime;
				}

			}


		}
	}
}
