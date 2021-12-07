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
	private int[] engagementRows = new int[] { 4, 2, 0};
	private int[] engagementColumns = new int[] { 1, 2, 3, 4};
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
	private float LOG_INTERVAL = 0.2f; // 0.2f;
	private float currTime;
	private float prevTime;

	//public const string MQTT_CAMERA_RECORDER = "CAMERA_RECORDER";
	public const string MQTT_EMPATICA_RECORDER = "EMPATICA_RECORDER";
	public const string MQTT_SYSTEM_RECORDER = "SYSTEM_RECORDER";
	public const string CMD_START = "START";
	public const string CMD_STOP = "STOP";

	private float shapeChangeDuration = 2f;
	private float shapeChangeWaitFor = 3f;

	private bool training = false;

	private string stringParticipant = "";
	private int numeroParticipant = 0;
	private bool unknownParticipant = true;
	private const float maxSpeed = 20f;

	IEnumerator FakeNextMole()
	{
		yield return new WaitForSeconds(shapeChangeWaitFor);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_BLACKING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		AllBlack(0.5f);
		Debug.Log("AllBlack");
		yield return new WaitForSeconds(shapeChangeWaitFor);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_DESCENDED"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		moleState = MOLE_TO_APPEAR;
	}
	IEnumerator NextMole()
	{
		// wait for one hand presence
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("WAIT_FOR_HAND_PRESENCE"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		Debug.Log("WaitForHandPresence...");
		List<Vector3> safePositionAndSpeeds = new List<Vector3>();
		Vector2 molePosition = molePositions[moleIndex];
		bool moleIsPresent = true;
		while (moleIsPresent)
		{
			if (leftHand.IsActive() || rightHand.IsActive())
			{
				safePositionAndSpeeds = FindAll();
				if (safePositionAndSpeeds.Count > 0)
				{
					moleIsPresent = false;
					foreach (Vector3 safePosAndSpeed in safePositionAndSpeeds)
					{
						if (safePosAndSpeed.x == molePosition.x && safePosAndSpeed.y == molePosition.y) moleIsPresent = true;
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
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_BLACKING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		AllBlack(0.5f);
		Debug.Log("AllBlack");
		yield return new WaitForSeconds(0.5f);
		// wait for no hand presence
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("WAIT_FOR_NO_HAND_PRESENCE"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		Debug.Log("WaitForNoHandPresence...");
		moleIsPresent = false;
		waitingSince = 0f;
		waitingCount = 1;
		while (!moleIsPresent)
		{
			if (!leftHand.IsActive() && !rightHand.IsActive())
			{
				safePositionAndSpeeds = FindAll();
				if (safePositionAndSpeeds.Count > 0)
				{
					foreach (Vector3 safePosAndSpeed in safePositionAndSpeeds)
					{
						if (safePosAndSpeed.x == molePosition.x && safePosAndSpeed.y == molePosition.y) moleIsPresent = true;
					}
				}
			}
			if (waitingSince >= shapeChangeWaitFor)
			{
				if (--waitingCount <= 0) break;
				waitingSince = 0f;
			} else
			{
				waitingSince += 0.1f;
				yield return new WaitForSeconds(0.1f);

			}
		}
		Debug.Log("HandIsNoPresent!");
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_DESCENDING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		AllDown(safePositionAndSpeeds);
		Debug.Log("AllDown");
		yield return new WaitForSeconds(shapeChangeWaitFor); 
		// wait until all pins are down
		waitingSince = 0f;
		waitingCount = 1;
		while (!IsAllDown(safePositionAndSpeeds))
		{
			if (waitingSince >= shapeChangeWaitFor)
			{
				if (--waitingCount <= 0) break;

				waitingSince = 0f;
				Debug.Log("AllDown again");
				AllDown(safePositionAndSpeeds);
			}
			else
			{
				waitingSince += 0.1f;
				yield return new WaitForSeconds(0.1f);
			}
		}

		Debug.Log("AreAllDown");
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("LANDSCAPE_DESCENDED"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
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

	List<Vector3> FindAll()
	{
		List<Vector3> safePositions = new List<Vector3>();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
				if(expanDialSticks.modelMatrix[i, j].CurrentProximity < 1f)
				{
					safePositions.Add(new Vector3(i, j, expanDialSticks.modelMatrix[i, j].CurrentProximity));
					//Debug.Log("Safe Pos: " + i + " " + j + " " + expanDialSticks.viewMatrix[i, j].CurrentProximity);
				}
				
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
			Debug.Log("Up you go : " + i + " " + j + " " + safetyDuration);
			expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = safetyDuration;
		}
		expanDialSticks.triggerShapeChange();
	}
	void AllDown(List<Vector3> safePositionAndSpeeds)
	{
		foreach (Vector3 safePositionAndSpeed in safePositionAndSpeeds)
		{
			int i = (int)safePositionAndSpeed.x;
			int j = (int)safePositionAndSpeed.y;
			sbyte targetPos = 0;
			float safetySpeed = maxSpeed; // 20 pos per sec max
			float distance = Math.Abs(targetPos - expanDialSticks.modelMatrix[i, j].CurrentPosition);
			float safetyDuration = Math.Max(distance / safetySpeed, 0.1f);
			expanDialSticks.modelMatrix[i, j].TargetPosition = targetPos;
			expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = safetyDuration;
		}
		expanDialSticks.triggerShapeChange();
	}
	void AllDown()
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				float safetySpeed = maxSpeed; // 20 pos per sec max
				float distance = Math.Abs(0 - expanDialSticks.modelMatrix[i, j].CurrentPosition);
				float safetyDuration = Math.Max(distance / safetySpeed, 0.1f);
				expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
				expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = safetyDuration;
			}
		}
		expanDialSticks.triggerShapeChange();
	}

	void AllDown(List<Vector3> safePositionAndSpeeds, float duration)
	{
		foreach(Vector3 safePositionAndSpeed in safePositionAndSpeeds)
		{
			int i = (int)safePositionAndSpeed.x;
			int j = (int)safePositionAndSpeed.y;
			float speedCoeff = safePositionAndSpeed.z; // 1 stop, 0.5 half, 0.3
			float safetyDuration = Math.Max(duration / (1f - speedCoeff), 0.1f);
			Debug.Log(i + " " + j + " " + safetyDuration);
			expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
			expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = safetyDuration;
		}
		expanDialSticks.triggerShapeChange();
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

		expanDialSticks.setBorderBackground(Color.black);
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
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("MOLE_BLACKING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		AllBlack(0.5f);
		Debug.Log("AllBlack");
		yield return new WaitForSeconds(0.5f);
		// wait for one hand presence
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("WAIT_FOR_NO_HAND_PRESENCE"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		Debug.Log("WaitForNoHandPresence...");
		List<Vector3> safePositionAndSpeeds = new List<Vector3>();
		Vector2 molePosition = molePositions[moleIndex];
		bool moleIsPresent = false;
		float waitingSince = 0f;
		int waitingCount = 1;
		/*while (!moleIsPresent)
		{
			if (!leftHand.IsActive() && !rightHand.IsActive())
			{
				safePositionAndSpeeds = FindAll();
				if (safePositionAndSpeeds.Count > 0)
				{
					foreach (Vector3 safePosAndSpeed in safePositionAndSpeeds)
					{
						if (safePosAndSpeed.x == molePosition.x && safePosAndSpeed.y == molePosition.y) moleIsPresent = true;
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
		}*/
		yield return new WaitForSeconds(3f);
		Debug.Log("NoHandIsPresent!");
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("MOLE_APPEARING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		Debug.Log("MoleUp"); 
		MoleUp(shapeChangeDuration);
		yield return new WaitForSeconds(shapeChangeWaitFor);
		// wait until mole is up 
		waitingSince = 0f;
		while (!IsMoleUp())
		{
			if (waitingSince >= shapeChangeDuration)
			{
				waitingSince = 0f;
				Debug.Log("MoleUpAgain");
				MoleUp(shapeChangeDuration);
			}
			else
			{
				waitingSince += 0.1f;
				yield return new WaitForSeconds(0.1f);
			}
		}
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("MOLE_GREENING"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		MoleGreen(0.5f);
		Debug.Log("MoleGreen");
		yield return new WaitForSeconds(0.5f);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("MOLE_APPEARED"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		moleState = MOLE_APPEARED;
	}
	bool IsMoleUp()
	{
		Vector2 molePosition = molePositions[moleIndex]; 
		return !expanDialSticks.viewMatrix[(int)molePosition.x, (int)molePosition.y].CurrentReaching && expanDialSticks.viewMatrix[(int)molePosition.x, (int)molePosition.y].CurrentPosition == 20;
		//return expanDialSticks.modelMatrix[(int)molePosition.x, (int)molePosition.y].CurrentPosition == 20;
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
				{
					expanDialSticks.modelMatrix[i, j].TargetColor = Color.white; //Color.green;
					expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "dot";
					expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = 0f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 2f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorColor = Color.black;
				}
				else
				{
					expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
					expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "projector";
					expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = 0f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorColor = Color.white;

				}
				expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = duration;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
			}
		}
		string participantNumber = "<pos=0%><b>P" + numeroParticipant + "</b>";
		string trialProgress = "<pos=90%><b>" + (moleIndex+1) +  "/" + molePositions.Length + "</b>";
		string legend = participantNumber + trialProgress;
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 16, Color.black, legend, new Vector3(90f, -90f, 0f));
		expanDialSticks.setBorderBackground(Color.white);
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
		Vector2 molePosition = molePositions[moleIndex];
		if (e.i == (int)molePosition.x && e.j == (int)molePosition.y)
		{
			string msg = "USER_ROTATION " + e.i + " " + e.j;
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

			if (moleState == MOLE_APPEARED && moleIndex < molePositions.Length)
			{
				moleState = LANDSCAPE_IS_CHANGING;
				if (training)
				{
					StartCoroutine(FakeNextMole());
				}
				else
				{
					StartCoroutine(NextMole());
				}
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
		Vector2 molePosition = molePositions[moleIndex];
		int moleX = (int)molePosition.x;
		int moleY = (int)molePosition.y;

		//string colorString = "SYSTEM_COLOR ";
		string proximityString = "SYSTEM_PROXIMITY ";
		string positionString = "SYSTEM_POSITION ";
		string leftHandString = "USER_LEFT_HAND " + leftHand.ToString();
		string rightHandString = "USER_RIGHT_HAND " + rightHand.ToString();

		string pinOrientationString = "USER_ORIENTATION " + expanDialSticks.viewMatrix[moleX, moleY].CurrentAxisX + " " + expanDialSticks.viewMatrix[moleX, moleY].CurrentAxisY;
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

		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(proximityString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(positionString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(leftHandString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(rightHandString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(pinOrientationString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
	}

	void OnGUI()
	{
		if (connected && unknownParticipant)
		{
			// Make a text field that modifies stringToEdit.
			float midX = Screen.width / 2.0f;
			float midY = Screen.height / 2.0f;
			float componentHeight = 20;
			//GUI.Label(new Rect(midX - 50 - , midY, 100, 20), "Hello World!");

			stringParticipant = GUI.TextField(new Rect(midX-55, midY, 50, componentHeight), stringParticipant, 25);
			if (GUI.Button(new Rect(midX + 5, midY, 70, componentHeight), "TRAINING"))
			{
				training = true;
				numeroParticipant = int.Parse(stringParticipant);
				Debug.Log("TRAINING");
				// init trials
				InitTrials();
				moleIndex = -1;
				moleState = MOLE_TO_APPEAR;

				currTime = LOG_INTERVAL;
				prevTime = 0f;
				//string safetyMode = (expanDialSticks.safetyMotionMode == ExpanDialSticks.SafetyMotionMode.SafetyRatedMonitoredStop) ? "SMS":"SSM";
				string identity = "USER_IDENTITY " + numeroParticipant + " USER_TRIGGERED TRAINING";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 80, midY + 25, 50, componentHeight), "SMS"))
			{

				expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.SafetyRatedMonitoredStop);
				training = false;
				numeroParticipant = int.Parse(stringParticipant);
				Debug.Log("Start");
				// init trials
				InitTrials();
				moleIndex = -1;
				moleState = MOLE_TO_APPEAR;

				currTime = LOG_INTERVAL;
				prevTime = 0f;
				string identity = "USER_IDENTITY " + numeroParticipant + " USER_TRIGGERED SMS TRIAL";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 80, midY - 25, 50, componentHeight), "SSM"))
			{

				expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.SpeedAndSeparationMonitoring);
				training = false;
				numeroParticipant = int.Parse(stringParticipant);
				Debug.Log("Start");
				// init trials
				InitTrials();
				moleIndex = -1;
				moleState = MOLE_TO_APPEAR;

				currTime = LOG_INTERVAL;
				prevTime = 0f;
				string identity = "USER_IDENTITY " + numeroParticipant + " USER_TRIGGERED SSM TRIAL";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				unknownParticipant = false;
			}
		}
	}

	void Update () {
		// check if ExpanDialSticks is connected
		if(connected && !unknownParticipant){

			 if (Input.GetKey("escape") || (moleState == MOLE_TO_APPEAR && moleIndex >= molePositions.Length))
            {
				Quit();
            }

            if (Input.GetKeyDown("n")) 
            {
				if (moleState == MOLE_APPEARED && moleIndex < molePositions.Length)
				{
					Vector2 molePosition = molePositions[moleIndex];
					string msg = "USER_ROTATION " + molePosition.x + " " + molePosition.y; 
					expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

					moleState = LANDSCAPE_IS_CHANGING;
					if (training) {
						StartCoroutine(FakeNextMole());
					} else {
						StartCoroutine(NextMole());
					}
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
