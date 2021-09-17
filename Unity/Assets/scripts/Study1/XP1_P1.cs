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

public class XP1_P1 : MonoBehaviour
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


	private  Vector2 [] molePositions;

	private int moleIndex;
	private const int MOLE_TO_APPEAR = 0;
	private const int MOLE_APPEARING = 1;
	private const int MOLE_APPEARED = 2;
	private const int LANDSCAPE_IS_CHANGING = 3;

	private int moleState = MOLE_TO_APPEAR;

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

	public void DebugLog(string msg)
	{
	#if _DEBUG_
		Debug.Log(msg);
	#endif
	}

	IEnumerator NextMole()
	{
		// wait for one hand presence
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("WAIT_FOR_HAND_PRESENCE"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		DebugLog("WaitForHandPresence..."); 
		while (!leftHand.IsActive() && !rightHand.IsActive())
			yield return new WaitForSeconds(0.1f);
		DebugLog("HandIsPresent!");
		// trigger most unsafe SC
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("TRIGGER_LANDSCAPE_ASCENDING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		AllUp(shapeChangeDuration);
		DebugLog("AllUp");
		yield return new WaitForSeconds(3f);
		// wait for one hand presence
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("WAIT_FOR_HAND_PRESENCE"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		DebugLog("WaitForHandPresence..."); 
		while (!leftHand.IsActive() && !rightHand.IsActive())
			yield return new WaitForSeconds(0.1f);
		DebugLog("HandIsPresent!");
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("TRIGGER_LANDSCAPE_DESCENDING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		AllDown(shapeChangeDuration);
		DebugLog("AllDown");
		yield return new WaitForSeconds(3f); 
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("TRIGGER_LANDSCAPE_BLACKING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		AllBlack(0.5f);
		DebugLog("AllBlack");
		yield return new WaitForSeconds(0.5f);
		// wait until all pins are down
		while (!IsAllDown())
		{
			yield return new WaitForSeconds(0.1f);
		}

		DebugLog("AreAllDown");
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("END_LANDSCAPE_CHANGING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

		//fileLogger.Log("END_LANDSCAPE_CHANGING");
		/*fileLogger.Log("SYSTEM_LANDSCAPE_WHITE");
		AllWhite(0.5f);
		yield return new WaitForSeconds(0.5f);*/
		moleState = MOLE_TO_APPEAR;
	}

	void AllReset(float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
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
	}
	void AllDown(float duration)
	{
		Vector2 molePosition = molePositions[moleIndex];
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
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
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
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.black;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
			}
		}
		expanDialSticks.triggerTextureChange();
	}
	void AllWhite(float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
			}
		}
		expanDialSticks.triggerTextureChange();
	}

	IEnumerator ShowMole()
	{

		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("TRIGGER_MOLE_APPEARING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		MoleUp(shapeChangeDuration);
		DebugLog("MoleUp");
		yield return new WaitForSeconds(1f);
		// wait until all pins are down
		while (!IsMoleUp())
		{
			yield return new WaitForSeconds(0.1f);
		}
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("TRIGGER_MOLE_GREENING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		MoleGreen(0.5f);
		DebugLog("MoleGreen");
		yield return new WaitForSeconds(0.5f);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("END_MOLE_APPEARING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
		moleState = MOLE_APPEARED;
	}
	bool IsMoleUp()
	{
		Vector2 molePosition = molePositions[moleIndex];
		return expanDialSticks[(int)molePosition.x, (int)molePosition.y].TargetPosition == 20;
	}

	void MoleUp(float duration)
	{
		Vector2 molePosition = molePositions[moleIndex];
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				if (i == (int)molePosition.x && j == (int)molePosition.y) 
					expanDialSticks.modelMatrix[i, j].TargetPosition = 20;
				else 
					expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
				expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = duration;
			}
		}
		expanDialSticks.triggerShapeChange();
	}

	void MoleGreen(float duration)
	{
		Vector2 molePosition = molePositions[moleIndex];
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				if (i == (int)molePosition.x && j == (int)molePosition.y)
					expanDialSticks.modelMatrix[i, j].TargetColor = Color.white; //Color.green;
				else
					expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
			}
		}
		string participantNumber = "<pos=0%><b>P" + numeroParticipant + "</b>";
		string trialProgress = "<pos=90%><b>" + (moleIndex+1) +  "/" + molePositions.Length + "</b>";
		string legend = participantNumber + trialProgress;
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 16, Color.black, legend, new Vector3(90f, -90f, 0f));
		expanDialSticks.triggerTextureChange();

	}

	/*private bool noProximity()
	{
		float sumProximity = 0f;
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				sumProximity += expanDialSticks[i, j].CurrentProximity;

			}
		}
		return (Mathf.Approximately(sumProximity, 0f));
	}*/

	void Start () {
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
		moleIndex = -1;
		moleState = MOLE_TO_APPEAR;
		//fileLogger = new FileLogger(logEnabled);
		currTime = LOG_INTERVAL;
		prevTime = 0f;
		// Connection to MQTT Broker
		expanDialSticks.client_MqttConnect();
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

		molePositions = new Vector2[engagementRows.Length * engagementRows.Length];

		// Generate Squared-Latin Row Indexes
		int[] shuffledRowIndexes = new int[engagementRows.Length * engagementRows.Length];
		for (int i = 0; i < engagementRows.Length; i++)
		{
			for(int j = 0; j < engagementRows.Length; j++)
			{
				shuffledRowIndexes[i * engagementRows.Length + j] = engagementRows[(i+numeroParticipant+j)%engagementRows.Length];

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
			molePositions[i] = new Vector2(shuffledRowIndexes[i], shuffledColumnsIndexes[i]);
		}
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
		Vector2 molePosition = molePositions[moleIndex];
		if (e.i == (int)molePosition.x && e.j == (int)molePosition.y)
		{
			string msg = "USER_MOLE_ROTATION " + e.i + " " + e.j;
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
			if (moleState == MOLE_APPEARED && moleIndex < molePositions.Length)
			{
				moleState = LANDSCAPE_IS_CHANGING;
				StartCoroutine(NextMole());
			}
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
		Vector2 molePosition = molePositions[moleIndex];
		int moleX = (int)molePosition.x;
		int moleY = (int)molePosition.y;

		//string colorString = "SYSTEM_COLOR ";
		string proximityString = "SYSTEM_PROXIMITY ";
		string positionString = "SYSTEM_POSITION ";
		string leftHandString = "USER_LEFT_HAND " + leftHand.ToString();
		string rightHandString = "USER_RIGHT_HAND " + rightHand.ToString();

		string pinOrientationString = "USER_PIN_ORIENTATION " + expanDialSticks.viewMatrix[moleX, moleY].CurrentAxisX + " " + expanDialSticks.viewMatrix[moleX, moleY].CurrentAxisY;
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

	void Update () {
		// check if ExpanDialSticks is connected
		if(connected){

			 if (Input.GetKey("escape") || (moleState == MOLE_TO_APPEAR && moleIndex >= molePositions.Length))
            {
				Quit();
            }

            if (Input.GetKeyDown("n")) 
            {
				if (moleState == MOLE_APPEARED && moleIndex < molePositions.Length)
				{
					Vector2 molePosition = molePositions[moleIndex];
					string msg = "USER_MOLE_ROTATION " + molePosition.x + " " + molePosition.y; 
					expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

					moleState = LANDSCAPE_IS_CHANGING;
					StartCoroutine(NextMole());
				}
            }
			if(moleState == MOLE_TO_APPEAR && ++moleIndex < molePositions.Length)
			{
				moleState = MOLE_APPEARING;
				StartCoroutine(ShowMole());
			}
			if (moleState == LANDSCAPE_IS_CHANGING)
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
