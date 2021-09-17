#define _DEBUG_
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

	private Vector2[] gaugePositions;

	private int gaugeIndex;
	private const int GAUGE_TO_APPEAR = 0;
	private const int GAUGE_APPEARING = 1;
	private const int GAUGE_APPEARED = 2;
	private const int GAUGE_STARTED = 3;
	private const int LANDSCAPE_IS_CHANGING = 4;

	private int gaugeState = GAUGE_TO_APPEAR;
	private const sbyte gaugeHeight = 20;

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

	private float aiguilleRotation = 90f;
	private float cadranRotation = 90f;
	private float speedRotation = 1f;

	private float directionTime = 0f;
	private float directionDuration = 3f;
	private float startGameTime = 0f;
	private float gameDuration = 20f;
	private float motionDuration = 10f;

	private const float anglePerStep = 360f / 24f;
	private float startRotation = 90f - anglePerStep;
	public enum DirectionRotation { CW, CCW, IDDLE };	
	private DirectionRotation directionRotation = DirectionRotation.CW;

	public void DebugLog(string msg)
	{
		#if _DEBUG_
				Debug.Log(msg);
		#endif
	}
	IEnumerator NextGauge()
	{
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_BLACKING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		DebugLog("AllBlack..."); 
		AllBlack(0.5f);
		yield return new WaitForSeconds(0.5f);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_DESCENDING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		DebugLog("AllDown...");
		AllDown(shapeChangeDuration);
		yield return new WaitForSeconds(1f);
		// wait until all pins are down
		//float waitingSince = 0f;
		while (!IsAllDown())
		{
			/*if (waitingSince >= 2f)
			{
				waitingSince = 0f;
				AllDown(shapeChangeDuration);
			}
			else
			{
				waitingSince += 0.1f;*/
				yield return new WaitForSeconds(0.1f);
			//}
		}
		DebugLog("AllIsDown!");
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("GAUGE_TO_APPEAR"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		gaugeState = GAUGE_TO_APPEAR;
	}

	IEnumerator ShowGauge()
	{

		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("GAUGE_APPEARING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		DebugLog("GaugeUp...");
		GaugeUp(shapeChangeDuration);
		yield return new WaitForSeconds(1f);
		// wait until all pins are down
		//float waitingSince = 0f;
		while (!IsGaugeUp())
		{

			/*if (waitingSince >= 2f)
			{
				waitingSince = 0f;
				GaugeUp(shapeChangeDuration);
			}
			else
			{
				waitingSince += 0.1f;*/
				yield return new WaitForSeconds(0.1f);
			//}
		}
		DebugLog("GaugeIsUp!");
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("GAUGE_DISPLAYING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		cadranRotation = aiguilleRotation = startRotation;
		DebugLog("GaugeInit!");
		GaugeInit(0.5f);
		yield return new WaitForSeconds(0.5f);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("GAUGE_APPEARED"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		gaugeState = GAUGE_APPEARED;
	}

	IEnumerator Earthquake()
	{
		// wait for one hand presence
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("WAIT_FOR_HAND_PRESENCE"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		DebugLog("WaitForHandPresence...");
		while (!leftHand.IsActive() && !rightHand.IsActive())
			yield return new WaitForSeconds(0.1f);
		DebugLog("HandIsPresent!");
		// trigger most unsafe SC
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_ASCENDING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		DebugLog("AllUpExceptGauge..."); 
		AllUpExceptGauge(shapeChangeDuration);
		yield return new WaitForSeconds(3f);
		// wait for one hand presence
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("WAIT_FOR_HAND_PRESENCE"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		DebugLog("WaitForHandPresence..."); 
		while (!leftHand.IsActive() && !rightHand.IsActive())
			yield return new WaitForSeconds(0.1f);
		DebugLog("HandIsPresent!");
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_DESCENDING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		DebugLog("AllDownExceptGauge..."); 
		AllDownExceptGauge(shapeChangeDuration);
		yield return new WaitForSeconds(3f);
		/*expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("TRIGGER_LANDSCAPE_BLACKING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		AllBlack(0.5f);
		yield return new WaitForSeconds(0.5f);*/
		// wait until all pins are down
		/*while (!IsAllDownExceptGauge())
		{
			yield return new WaitForSeconds(0.1f);
		}
		Debug.Log("AllDownExceptGauge!");*/
		//expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("END_LANDSCAPE_CHANGING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		gaugeState = GAUGE_STARTED;
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
				//expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
				//expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
				expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
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
				if (expanDialSticks.viewMatrix[i, j].CurrentReaching)
				{
					//Debug.Log("CurrentReaching (" + i + "," + j + "):" + expanDialSticks.viewMatrix[i, j].CurrentReaching);
				}
				if (expanDialSticks.viewMatrix[i, j].CurrentPosition != 0)
				{
					//Debug.Log("CurrentPosition (" + i+","+j+"):"+ expanDialSticks.viewMatrix[i, j].CurrentPosition);
				}

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
					//expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
					//expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
					expanDialSticks.modelMatrix[i, j].TargetPosition = 40;
					expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = duration;
				}

			}
		}
		//expanDialSticks.triggerTextureChange();
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
					//expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
					//expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
					expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
					expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = duration;
				}
			}
		}
		//expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerShapeChange();
	}
	bool IsAllDownExceptGauge()
	{
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{

				if ((i != (int)gaugePosition.x || j != (int)gaugePosition.y) && expanDialSticks.viewMatrix[i, j].CurrentPosition > 0 || expanDialSticks.viewMatrix[i, j].CurrentReaching)
				{
					//Debug.Log("i, j:" + i + "," + j + "gi, gj:" + (int)gaugePosition.x + "," + (int)gaugePosition.y);

					//Debug.Log("expanDialSticks.viewMatrix[i, j].CurrentPosition:" + expanDialSticks.viewMatrix[i, j].CurrentPosition);
					//Debug.Log("expanDialSticks.viewMatrix[i, j].CurrentReaching:" + expanDialSticks.viewMatrix[i, j].CurrentReaching);
					return false;
				}
			}
		}
		return true;
	}
	void AllBlack(float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
				expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = aiguilleRotation;

				expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "projector";
				expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = cadranRotation;
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

		gaugePositions = new Vector2[engagementRows.Length * engagementColumns.Length];

		// Generate Squared-Latin Row Indexes
		int[] shuffledRowIndexes = new int[engagementRows.Length * engagementColumns.Length];
		for (int i = 0; i < engagementRows.Length; i++)
		{
			for (int j = 0; j < engagementColumns.Length; j++)
			{
				shuffledRowIndexes[i * engagementColumns.Length + j] = engagementRows[(i + numeroParticipant + j) % engagementRows.Length];
			}
		}

		// Generate Shuffled Column Indexes
		int[] shuffledColumnsIndexes = new int[engagementRows.Length * engagementColumns.Length];
		for (int i = 0; i < engagementRows.Length; i++)
		{
			engagementColumns = Shuffle(engagementColumns);

			for (int j = 0; j < engagementColumns.Length; j++)
			{
				shuffledColumnsIndexes[i * engagementColumns.Length + j] = engagementColumns[j];
			}
		}

		for (int i = 0; i < engagementRows.Length * engagementColumns.Length; i++)
		{
			gaugePositions[i] = new Vector2(shuffledRowIndexes[i], shuffledColumnsIndexes[i]);
		}
	}

	bool IsGaugeUp()
	{
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		//Debug.Log("IsGaugeUp -> gauge Index: " + gaugePosition + " position: " + expanDialSticks.viewMatrix[(int)gaugePosition.x, (int)gaugePosition.y].CurrentPosition);
		//Debug.Log("IsGaugeUp -> Gauge CurrentReaching: " + expanDialSticks.viewMatrix[(int)gaugePosition.x, (int)gaugePosition.y].CurrentReaching);
		//Debug.Log("IsGaugeUp -> Gauge CurrentPosition: " + expanDialSticks.viewMatrix[(int)gaugePosition.x, (int)gaugePosition.y].CurrentPosition);
		return !expanDialSticks.viewMatrix[(int)gaugePosition.x, (int)gaugePosition.y].CurrentReaching && expanDialSticks.viewMatrix[(int)gaugePosition.x, (int)gaugePosition.y].CurrentPosition == gaugeHeight;
	}

	void GaugeUp(float duration)
	{
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{

				if (i == (int)gaugePosition.x && j == (int)gaugePosition.y)
				{
					//Debug.Log("GaugeUp -> gauge Index: " + gaugePosition + " (" + (count++) + ")");
					expanDialSticks.modelMatrix[i, j].TargetPosition = gaugeHeight;
				}
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
					expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "LightCadran";
					expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = cadranRotation;
					expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0.6f;
					expanDialSticks.modelMatrix[i, j].TargetPlaneColor = Color.red;

					expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "aiguille";
					expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = aiguilleRotation; 
					expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 2f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorColor = Color.black;
				}

				else
				{
					expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
					expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = cadranRotation;
					expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetPlaneColor = Color.white;

					expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "projector";
					expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = aiguilleRotation;
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

			float prevRotation = aiguilleRotation;
			aiguilleRotation += e.diff * anglePerStep;
			string msg = "";
			if (gaugeState == GAUGE_APPEARED)
			{
				msg += "USER_START_GAUGE" + prevRotation + " " + aiguilleRotation;
				DebugLog("UserStartGauge!");
				startGameTime = Time.time;
				motionDuration = Random.Range(5f, gameDuration - 5f);
				gaugeState = GAUGE_STARTED;
			} else
			{
				msg += "USER_ROTATE_GAUGE " + prevRotation + " " + aiguilleRotation;
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
	void MoveAiguilleCadran()
	{
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		int i = (int)gaugePosition.x;
		int j = (int)gaugePosition.y;
		//Debug.Log("aiguilleRotation: " + aiguilleRotation + " cadranRotation: " + cadranRotation);
		expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = cadranRotation;
		expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = 0.1f;
		expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = aiguilleRotation;
		expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = 0.1f;
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
				//Debug.Log("TRIGGER StartCoroutine ShowGauge");
				gaugeState = GAUGE_APPEARING;
				StartCoroutine(ShowGauge());
			}
			if (gaugeState == GAUGE_STARTED || gaugeState == LANDSCAPE_IS_CHANGING)
			{

				if (gaugeState == GAUGE_STARTED && Time.time - startGameTime >= motionDuration)
				{
					//Debug.Log("TRIGGER StartCoroutine Earthquake");
					gaugeState = LANDSCAPE_IS_CHANGING;
					StartCoroutine(Earthquake());
					motionDuration = Mathf.Infinity;
				}

				if (gaugeState == GAUGE_STARTED && Time.time - startGameTime >= gameDuration)
				{
					/*Debug.Log("TRIGGER StartCoroutine NextGauge");
					Debug.Log("Time.time:" + Time.time);
					Debug.Log("startGameTime:" + startGameTime);
					Debug.Log("gameDuration:" + gameDuration);*/
					gaugeState = GAUGE_APPEARING;
					StartCoroutine(NextGauge());
				}
				else
				{
					// Gauge Game
					float prevRotation  = cadranRotation;
					switch (directionRotation)
					{
						case DirectionRotation.CW:
							cadranRotation += speedRotation * Time.deltaTime;
							break;
						case DirectionRotation.CCW:
							cadranRotation -= speedRotation * Time.deltaTime;
							break;
						default:
							break;
					}
					string msg = "SYSTEM_ROTATE_GAUGE " + prevRotation + " " + cadranRotation;
					expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

					if (Time.time - directionTime >= directionDuration)
					{
						int nbDirections = Enum.GetNames(typeof(DirectionRotation)).Length - 1; // without IDDLE
						directionRotation = (DirectionRotation)UnityEngine.Random.Range(0, nbDirections);
						speedRotation = UnityEngine.Random.Range(5f, 15f);
						directionDuration = UnityEngine.Random.Range(3f, 9f);
						directionTime = Time.time;
					}
					MoveAiguilleCadran();
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
