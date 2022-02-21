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
	private int[] engagementRows = new int[] { 4, 2, 0 };
	private int[] engagementColumns = new int[] { 1, 2, 3, 4 };
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
	private const float LOG_INTERVAL = 0.2f; // 0.2f;
	private float currTime;
	private float prevTime;

	public const string MQTT_EMPATICA_RECORDER = "EMPATICA_RECORDER";
	public const string MQTT_SYSTEM_RECORDER = "SYSTEM_RECORDER";
	public const string CMD_START = "START";
	public const string CMD_STOP = "STOP";

	private float shapeChangeDuration = 2f;
	private float shapeChangeWaitFor = 3f;

	private float aiguilleRotation = 90f;
	private float cadranRotation = 90f;
	private float speedRotation = 1f;

	private float directionTime = 0f;
	private float directionDuration = 3f;
	private float startGameTime = 0f;

	private const float initGameDuration = 20f;
	private float gameDuration = Mathf.Infinity;
	private float motionDuration = 10f;

	private const float anglePerStep = 360f / 24f;
	private float startRotation = 90f - anglePerStep;
	public enum DirectionRotation { CW, CCW, IDDLE };	
	private DirectionRotation directionRotation = DirectionRotation.CW;

	private bool training = false;

	private string stringParticipant = "";
	private int numeroParticipant = 0;
	private bool unknownParticipant = true;
	private const float maxSpeed = 20f;

	IEnumerator NextGauge()
	{
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_BLACKING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		Debug.Log("AllBlack..."); 
		AllBlack(0.5f);
		yield return new WaitForSeconds(0.5f);
		// wait for no hand presence
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("WAIT_FOR_NO_HAND_PRESENCE"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		Debug.Log("WaitForNoHandPresence...");
		List<Vector3> safePositionAndSpeeds = new List<Vector3>();
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		bool gaugeIsPresent = false;
		float waitingSince = 0f;
		int waitingCount = 1;
		while (!gaugeIsPresent)
		{
			if (!leftHand.IsActive() && !rightHand.IsActive())
			{
				safePositionAndSpeeds = FindAll();
				if (safePositionAndSpeeds.Count > 0)
				{
					foreach (Vector3 safePosAndSpeed in safePositionAndSpeeds)
					{
						if (safePosAndSpeed.x == gaugePosition.x && safePosAndSpeed.y == gaugePosition.y) gaugeIsPresent = true;
					}
				}
			}
			if (waitingSince >= shapeChangeWaitFor)
			{
				if (--waitingCount <= 0) break;
				waitingSince = 0f;
			}
			else
			{
				waitingSince += 0.1f;
				yield return new WaitForSeconds(0.1f);

			}
		}
		Debug.Log("HandIsNoPresent!");
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_DESCENDING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		Debug.Log("AllDown...");
		AllDown(shapeChangeDuration);
		yield return new WaitForSeconds(shapeChangeWaitFor);
		// wait until all pins are down
		waitingSince = 0f;
		waitingCount = 1;
		while (!IsAllDown())
		{
			if (waitingSince >= shapeChangeWaitFor)
			{
				if (--waitingCount <= 0) break;

				waitingSince = 0f;
				Debug.Log("AllDown again");
				AllDown(shapeChangeDuration);
			}
			else
			{
				waitingSince += 0.1f;
				yield return new WaitForSeconds(0.1f);
			}
		}
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_DESCENDED"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		Debug.Log("AllIsDown!");
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("GAUGE_TO_APPEAR"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		gaugeState = GAUGE_TO_APPEAR;
	}

	IEnumerator ShowGauge()
	{
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_BLACKING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		Debug.Log("AllBlack...");
		AllBlack(0.5f);
		yield return new WaitForSeconds(0.5f);
		// wait for no hand presence
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("WAIT_FOR_NO_HAND_PRESENCE"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		Debug.Log("WaitForNoHandPresence...");
		/*List<Vector3> safePositionAndSpeeds = new List<Vector3>();
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		bool gaugeIsPresent = false;*/
		float waitingSince = 0f;
		//int waitingCount = 1;
		/*while (!gaugeIsPresent)
		{
			if (!leftHand.IsActive() && !rightHand.IsActive())
			{
				safePositionAndSpeeds = FindAll();
				if (safePositionAndSpeeds.Count > 0)
				{
					foreach (Vector3 safePosAndSpeed in safePositionAndSpeeds)
					{
						if (safePosAndSpeed.x == gaugePosition.x && safePosAndSpeed.y == gaugePosition.y) gaugeIsPresent = true;
					}
				}
				if (waitingSince >= shapeChangeWaitFor)
				{
					if (--waitingCount <= 0) break;
					waitingSince = 0f;
				}
				else
				{
					waitingSince += 0.1f;
					yield return new WaitForSeconds(0.1f);
				}
			}
		}*/
		yield return new WaitForSeconds(3f);
		Debug.Log("NoHandIsPresent!");
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("GAUGE_APPEARING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		Debug.Log("GaugeUp...");
		GaugeUp(shapeChangeDuration);
		yield return new WaitForSeconds(shapeChangeWaitFor);
		// wait until gauge is u^p
		waitingSince = 0f;
		while (!IsGaugeUp())
		{

			if (waitingSince >= shapeChangeDuration)
			{
				waitingSince = 0f;
				Debug.Log("GaugeUp..."); 
				GaugeUp(shapeChangeDuration);
			}
			else
			{
				waitingSince += 0.1f;
				yield return new WaitForSeconds(0.1f);
			}
		}
		Debug.Log("GaugeIsUp!");
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("GAUGE_DISPLAYING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		cadranRotation = aiguilleRotation = startRotation;
		Debug.Log("GaugeInit!");
		GaugeInit(0.5f);
		yield return new WaitForSeconds(0.5f);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("GAUGE_APPEARED"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		gaugeState = GAUGE_APPEARED;
	}
	IEnumerator FakeEarthquake()
	{
		yield return new WaitForSeconds(shapeChangeWaitFor);
		gameDuration = (Time.time - startGameTime) + shapeChangeWaitFor;
		gaugeState = GAUGE_STARTED;
	}

	IEnumerator Earthquake()
	{
		// wait for one hand presence
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("WAIT_FOR_HAND_PRESENCE"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		Debug.Log("WaitForHandPresence...");
		List<Vector3> safePositionAndSpeeds = new List<Vector3>();
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		bool gaugeIsPresent = true;
		while (gaugeIsPresent)
		{
			if (leftHand.IsActive() || rightHand.IsActive())
			{
				safePositionAndSpeeds = FindAll();
				if (safePositionAndSpeeds.Count > 0)
				{
					gaugeIsPresent = false;
					foreach (Vector3 safePosAndSpeed in safePositionAndSpeeds)
					{
						if (safePosAndSpeed.x == gaugePosition.x && safePosAndSpeed.y == gaugePosition.y) gaugeIsPresent = true;
					}
				}
			}
			yield return new WaitForSeconds(0.1f);
		}
		Debug.Log("HandIsPresent!");
		// trigger most unsafe SC
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_ASCENDING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		AllUp(safePositionAndSpeeds);
		Debug.Log("AllUp");
		yield return new WaitForSeconds(shapeChangeWaitFor);
		// wait until all pins are up
		float waitingSince = 0f;
		int waitingCount = 1;
		while (!IsAllUp(safePositionAndSpeeds))
		{
			if (waitingSince >= shapeChangeWaitFor)
			{
				if (--waitingCount <= 0) break;

				waitingSince = 0f;
				Debug.Log("AllUp again");
				AllUp(safePositionAndSpeeds);
			}
			else
			{
				waitingSince += 0.1f;
				yield return new WaitForSeconds(0.1f);
			}
		}
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_ASCENDED"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		/*
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_DESCENDING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		Debug.Log("AllDown..."); 
		AllDown(safePositionAndSpeeds, shapeChangeDuration);
		yield return new WaitForSeconds(shapeChangeWaitFor);*/
		gameDuration = (Time.time - startGameTime) + shapeChangeWaitFor;
		gaugeState = GAUGE_STARTED;
	}

	List<Vector3> FindAll()
	{
		List<Vector3> safePositions = new List<Vector3>();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
				if (expanDialSticks.modelMatrix[i, j].CurrentProximity < 1f)
					safePositions.Add(new Vector3(i, j, expanDialSticks.modelMatrix[i, j].CurrentProximity));

		return safePositions;
	}

	void AllUp(List<Vector3> safePositionAndSpeeds)
	{
		foreach (Vector3 safePositionAndSpeed in safePositionAndSpeeds)
		{
			int i = (int)safePositionAndSpeed.x;
			int j = (int)safePositionAndSpeed.y;
			float speedCoeff = safePositionAndSpeed.z; // 1 stop, 0.5 half, 0.3
			sbyte targetPos = 40;

			float safetySpeed = maxSpeed * (1f - speedCoeff); // 20 pos per sec max
			float distance = Math.Abs(targetPos - expanDialSticks.modelMatrix[i, j].CurrentPosition);
			float safetyDuration = Math.Max(distance / safetySpeed, 0.1f);
			expanDialSticks.modelMatrix[i, j].TargetPosition = targetPos;
			Debug.Log(i + " " + j + " " + safetyDuration);
			expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = safetyDuration;
		}
		expanDialSticks.triggerShapeChange();
	}
	void AllUp(List<Vector3> safePositionAndSpeeds, float duration)
	{
		foreach (Vector3 safePositionAndSpeed in safePositionAndSpeeds)
		{
			int i = (int)safePositionAndSpeed.x;
			int j = (int)safePositionAndSpeed.y;
			float speedCoeff = safePositionAndSpeed.z; // 1 stop, 0.5 half, 0.3
			float safetyDuration = Math.Max(duration / (1f - speedCoeff), 0.1f);
			expanDialSticks.modelMatrix[i, j].TargetPosition = 40;
			//Debug.Log(i + " " + j + " " + safetyDuration);
			expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = safetyDuration;
		}
		expanDialSticks.triggerShapeChange();
	}

	void AllDown(List<Vector3> safePositionAndSpeeds, float duration)
	{
		foreach (Vector3 safePositionAndSpeed in safePositionAndSpeeds)
		{
			int i = (int)safePositionAndSpeed.x;
			int j = (int)safePositionAndSpeed.y;
			float speedCoeff = safePositionAndSpeed.z; // 1 stop, 0.5 half, 0.3
			float safetyDuration = Math.Max(duration / (1f - speedCoeff), 0.1f);
			expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
			expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = safetyDuration;
		}
		expanDialSticks.triggerShapeChange();
	}


	bool IsAllDown(List<Vector3> safePositionAndSpeeds)
	{
		foreach (Vector3 safePositionAndSpeed in safePositionAndSpeeds)
		{
			int i = (int)safePositionAndSpeed.x;
			int j = (int)safePositionAndSpeed.y;
			if (expanDialSticks.viewMatrix[i, j].CurrentPosition > 0 || expanDialSticks.viewMatrix[i, j].CurrentReaching) return false;
		}
		return true;
	}
	bool IsAllUp(List<Vector3> safePositionAndSpeeds)
	{
		foreach (Vector3 safePositionAndSpeed in safePositionAndSpeeds)
		{
			int i = (int)safePositionAndSpeed.x;
			int j = (int)safePositionAndSpeed.y;
			if (expanDialSticks.viewMatrix[i, j].CurrentPosition < 40 || expanDialSticks.viewMatrix[i, j].CurrentReaching) return false;
		}
		return true;
	}

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
	void AllBlack(float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
				expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = aiguilleRotation;

				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontTexture = "projector";
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontRotation = cadranRotation;
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontColor = Color.white;
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontChangeDuration = duration;

				expanDialSticks.modelMatrix[i, j].TargetColor = Color.black;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
			}
		}
		expanDialSticks.setBorderBackground(Color.black);
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

					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontTexture = "aiguille";
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontRotation = aiguilleRotation; 
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontSize = 2f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontColor = Color.black;
				}

				else
				{
					expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
					expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = cadranRotation;
					expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetPlaneColor = Color.white;

					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontTexture = "projector";
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontRotation = aiguilleRotation;
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontColor = Color.white;
				}

				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontChangeDuration = duration;
			}
		}
		string participantNumber = "<pos=0%><b>P" + numeroParticipant + "</b>";
		string trialProgress = "<pos=90%><b>" + (gaugeIndex + 1) + "/" + gaugePositions.Length + "</b>";
		string legend = participantNumber + trialProgress;
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 16, Color.black, legend, new Vector3(90f, -90f, 0f));
		expanDialSticks.setBorderBackground(Color.white);
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
		expanDialSticks.client_MqttConnect();
	}


	private void HandleConnecting(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("Application connecting to MQTT Broker @" + e.address + ":" + e.port + "...");
		connected = false;
	}

	private void HandleConnected(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("Application connected.");
		expanDialSticks.client.Publish(MQTT_EMPATICA_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_START), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_START), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
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

		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		if (e.i == (int)gaugePosition.x && e.j == (int)gaugePosition.y)
		{

			float prevRotation = aiguilleRotation;
			aiguilleRotation += e.diff * anglePerStep;
			string msg = "";
			msg += "USER_ROTATION " + prevRotation + " " + aiguilleRotation;
			Debug.Log(msg);
			if (gaugeState == GAUGE_APPEARED)
			{
				startGameTime = Time.time;
				motionDuration = Random.Range(5f, initGameDuration - 5f);
				gaugeState = GAUGE_STARTED;
			}
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
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

		expanDialSticks.client.Publish(MQTT_EMPATICA_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_STOP), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_STOP), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
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
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(proximityString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(positionString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(leftHandString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(rightHandString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(pinOrientationString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
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
		expanDialSticks.modelMatrix[i, j].TargetProjectorFrontRotation = aiguilleRotation;
		expanDialSticks.modelMatrix[i, j].TargetProjectorFrontChangeDuration = 0.1f;
		expanDialSticks.triggerTextureChange();
	}
	void OnGUI()
	{
		if (unknownParticipant)
		{
			// Make a text field that modifies stringToEdit.
			float midX = Screen.width / 2.0f;
			float midY = Screen.height / 2.0f;
			float componentHeight = 20;
			//GUI.Label(new Rect(midX - 50 - , midY, 100, 20), "Hello World!");

			stringParticipant = GUI.TextField(new Rect(midX - 25, midY, 50, componentHeight), stringParticipant, 25);
			if (GUI.Button(new Rect(midX + 25, midY, 70, componentHeight), "TRAINING"))
			{
				training = true;
				numeroParticipant = int.Parse(stringParticipant);
				// init trials
				InitTrials();
				gaugeIndex = -1;
				gaugeState = GAUGE_TO_APPEAR;

				currTime = LOG_INTERVAL;
				prevTime = 0f;

				string identity = "USER_IDENTITY " + numeroParticipant + " SYSTEM_TRIGGERED TRAINING";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				unknownParticipant = false;
			}
			if (GUI.Button(new Rect(midX + 75, midY+25, 50, componentHeight), "SMS"))
			{
				training = false;
				expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.SafetyRatedMonitoredStop);
				numeroParticipant = int.Parse(stringParticipant);
				// init trials
				InitTrials();
				gaugeIndex = -1;
				gaugeState = GAUGE_TO_APPEAR;

				currTime = LOG_INTERVAL;
				prevTime = 0f;
				string identity = "USER_IDENTITY " + numeroParticipant + " SYSTEM_TRIGGERED SMS TRIAL";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				unknownParticipant = false;
			}
			if (GUI.Button(new Rect(midX + 75, midY - 25, 50, componentHeight), "SSM"))
			{
				training = false;
				expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.SpeedAndSeparationMonitoring);
				numeroParticipant = int.Parse(stringParticipant);
				// init trials
				InitTrials();
				gaugeIndex = -1;
				gaugeState = GAUGE_TO_APPEAR;

				currTime = LOG_INTERVAL;
				prevTime = 0f;
				string identity = "USER_IDENTITY " + numeroParticipant + " SYSTEM_TRIGGERED SSM TRIAL";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				unknownParticipant = false;
			}
		}
	}

	void Update()
	{
		// check if ExpanDialSticks is connected
		if (connected && !unknownParticipant)
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
					if(training)
					{ 
						StartCoroutine(FakeEarthquake());

					} else
					{
						StartCoroutine(Earthquake());
					}
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
					gameDuration = Mathf.Infinity;
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
					string msg = "SYSTEM_ROTATION " + prevRotation + " " + cadranRotation;
					if ((int)prevRotation != (int)cadranRotation) {
						//Debug.Log(msg);
					}
					expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

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
