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
using Random = UnityEngine.Random;

public class Study1 : MonoBehaviour
{

    public GameObject expanDialSticksPrefab;
	public GUISkin guiSkin;
	public int[] engagementRows;
	public int[] engagementColumns;
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

	private const int  nbTrials = 9;

	private  Vector2 [] molePositions;

	private int moleIndex;
	private const int MOLE_TO_APPEAR = 0;
	private const int MOLE_APPEARING = 1;
	private const int MOLE_APPEARED = 2;
	private const int LANDSCAPE_IS_CHANGING = 3;

	private int moleState = MOLE_TO_APPEAR;
	private bool nextMole;

	private FileLogger fileLogger;
	public float LOG_INTERVAL = 0.25f; // 0.2f;
	public float currTime = 0f;
	public float prevTime = 0f;

	IEnumerator NextMole()
	{
		// trigger most unsafe SC
		fileLogger.Log("SYSTEM_LANDSCAPE_UP");
		AllUp(1f);
		yield return new WaitForSeconds(3f);
		fileLogger.Log("SYSTEM_LANDSCAPE_DOWN");
		AllDown(1f);
		yield return new WaitForSeconds(3f);
		fileLogger.Log("SYSTEM_LANDSCAPE_BLACK");
		AllBlack(0.5f);
		yield return new WaitForSeconds(0.5f);
		// wait until all pins are down
		while (!IsAllDown())
		{
			yield return new WaitForSeconds(0.1f);
		}
		/*fileLogger.Log("SYSTEM_LANDSCAPE_WHITE");
		AllWhite(0.5f);
		yield return new WaitForSeconds(0.5f);*/
		moleIndex++;
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
				if (expanDialSticks.viewMatrix[i, j].CurrentPosition > 0) return false;
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
		fileLogger.Log("SYSTEM_MOLE_UP");
		MoleUp(1f);
		yield return new WaitForSeconds(1f);
		// wait until all pins are down
		while (!IsMoleUp())
		{
			yield return new WaitForSeconds(0.1f);
		}
		fileLogger.Log("SYSTEM_MOLE_GREEN");
		MoleGreen(0.5f);
		yield return new WaitForSeconds(0.5f);
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
					expanDialSticks.modelMatrix[i, j].TargetColor = Color.green;
				else 
					expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
			}
		}
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
		moleIndex = 0;
		moleState = MOLE_TO_APPEAR;
		fileLogger = new FileLogger();
		prevTime = currTime = 0f;
		fileLogger.Log("APPLICATION_STARTED");
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

		molePositions = new Vector2[engagementRows.Length * engagementColumns.Length];

		for (int i = 0; i < engagementRows.Length; i++)
		{
			engagementColumns = Shuffle(engagementColumns);

			for (int j = 0; j < engagementColumns.Length; j++)
			{
				int row = engagementRows[i];
				int column = engagementColumns[j];
				molePositions[(i * engagementColumns.Length) + j] = new Vector2(row, column);
			}
		}

	}

	private void OnDestroy()
	{
		fileLogger.Close();
	}

	private void HandleConnecting(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("ExpanDialSticks connecting to MQTT Broker @" + e.address + ":" + e.port);
		connected = false;
	}

	private void HandleConnected(object sender, MqttConnectionEventArgs e)
	{
		fileLogger.Log("APPLICATION_CONNECTED");
		Debug.Log("ExpanDialSticks connected.");
		connected = true;

	}

	private void HandleDisconnected(object sender, MqttConnectionEventArgs e)
	{
		fileLogger.Log("APPLICATION_DISCONNECTED");
		Debug.Log("ExpanDialSticks disconnected.");
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
			fileLogger.Log("USER_MOLE_ROTATION");
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
		fileLogger.Log("APPLICATION_ENDED");
		#if UNITY_EDITOR
		// Application.Quit() does not work in the editor so
		// UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
			UnityEditor.EditorApplication.isPlaying = false;
		#else
				Application.Quit();
		#endif
	}


	void Update () {
		// check if ExpanDialSticks is connected
		if(connected){
			 if (Input.GetKey("escape") || (moleState == MOLE_APPEARED && moleIndex >= molePositions.Length))
            {
				Quit();
            }

            if (Input.GetKeyDown("n")) 
            {
				if (moleState == MOLE_APPEARED && moleIndex < molePositions.Length)
				{
					fileLogger.Log("USER_MOLE_ROTATION");
					moleState = LANDSCAPE_IS_CHANGING;
					StartCoroutine(NextMole());
				}
            }
			if(moleState == MOLE_TO_APPEAR && moleIndex < molePositions.Length)
			{
				moleState = MOLE_APPEARING;
				StartCoroutine(ShowMole());
			}
			if (moleState == LANDSCAPE_IS_CHANGING)
			{
				if ((currTime += Time.deltaTime) - prevTime > LOG_INTERVAL)
				{
					string proximityString = "USER_PROXIMITY ";
					string positionString = "SYSTEM_POSITION ";
					for (int i = 0; i < expanDialSticks.NbRows; i++)
					{
						for (int j = 0; j < expanDialSticks.NbColumns; j++)
						{
							proximityString += expanDialSticks.viewMatrix[i, j].CurrentProximity + " ";
							positionString += expanDialSticks.viewMatrix[i, j].CurrentPosition + " ";

						}
					}
					fileLogger.Log(proximityString);
					fileLogger.Log(positionString);
					prevTime = currTime;
				}
			}
        }
    }
}
