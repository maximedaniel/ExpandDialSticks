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
	private bool moleDone;

	IEnumerator NextMole()
	{
		moleDone = false;

		moleIndex++;
		// trigger most unsafe SC
		AllUp(1f);
		yield return new WaitForSeconds(2f);
		AllDown(1f);
		yield return new WaitForSeconds(2f);
		// wait until all pins are down
		while (!IsAllDown())
		{
			yield return new WaitForSeconds(0.1f);
		}
		// ask user to get back
		//resetLandscape(1f);
		//yield return new WaitForSeconds(2f);
		// wait no proximity from user
		/*while (!noProximity())
		{
			yield return new WaitForSeconds(0.1f);
		}*/
		// show next mole
		ShowMole(1f);
		yield return new WaitForSeconds(2f);
		moleDone = true;
	}

	void AllUp(float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
        {
            for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks[i, j].TargetColor = Color.white;
				expanDialSticks[i, j].TargetTextureChangeDuration = duration;
				expanDialSticks[i, j].TargetPosition = 40;
                expanDialSticks[i, j].TargetShapeChangeDuration = duration;
		
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
				expanDialSticks[i, j].TargetColor = Color.white;
				expanDialSticks[i, j].TargetPosition = 0;
				expanDialSticks[i, j].TargetTextureChangeDuration = duration;
				expanDialSticks[i, j].TargetShapeChangeDuration = duration;
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
				if (expanDialSticks[i, j].CurrentPosition != 0 || expanDialSticks[i, j].CurrentReaching) return false;
			}
		}
		return true;
	}

	void resetLandscape(float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks[i, j].TargetColor = Color.black;
				expanDialSticks[i, j].TargetTextureChangeDuration = duration;
			}
		}
		expanDialSticks.triggerTextureChange();
	}

	void ShowMole(float duration)
	{
		Vector2 molePosition = molePositions[moleIndex];
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				if (i == (int)molePosition.x && j == (int)molePosition.y)
				{

					expanDialSticks[i, j].TargetColor = Color.green;
					expanDialSticks[i, j].TargetPosition = 20;
				} else
				{
					expanDialSticks[i, j].TargetColor = Color.white;
					expanDialSticks[i, j].TargetPosition = 0;
				}
				expanDialSticks[i, j].TargetTextureChangeDuration = duration;
				expanDialSticks[i, j].TargetShapeChangeDuration = duration;
			}
		}
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerShapeChange();
	}

	private bool noProximity()
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
	}

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
		moleIndex = -1;
		moleDone = true;
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


	private void HandleConnecting(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("ExpanDialSticks connecting to MQTT Broker @" + e.address + ":" + e.port);
		connected = false;
	}

	private void HandleConnected(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("ExpanDialSticks connected.");
		connected = true;

	}

	private void HandleDisconnected(object sender, MqttConnectionEventArgs e)
	{
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
			if (moleDone && moleIndex < molePositions.Length)
			{
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


	void Update () {
		// check if ExpanDialSticks is connected
		if(connected){
			 if (Input.GetKey("escape"))
            {
                Application.Quit();
            }

            if (Input.GetKeyDown("n")) 
            {

				if (moleDone && moleIndex < molePositions.Length)
				{
					StartCoroutine(NextMole());
				}

				/*if (areAllUp)
				{
					allDown(1f);
					areAllUp = false;
				}
				else
				{
					allUp(1f);
					areAllUp = true;
				}*/
            }
        }
    }
}
