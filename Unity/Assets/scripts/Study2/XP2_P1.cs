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
using System.Linq;

public class XP2_P1 : MonoBehaviour
{
	// ExpanDialSticks Core
	public GameObject expanDialSticksPrefab;
	public GameObject capsuleHandLeftPrefab;
	public GameObject capsuleHandRightPrefab;
	private MyCapsuleHand leftHand;
	private MyCapsuleHand rightHand;
	private ExpanDialSticks expanDialSticks;
	private bool connected;

	// timer variables
	private float currTime;
	private float prevRandomTextureTime;
	private float prevRandomIconTime;
	private float prevRandomShapeTime;

	private int nbTrials = 3;
	private int[] amountFactor = new int[] {3, 6, 9 };
	private ExpanDialSticks.SafetyOverlayMode[] overlayFactor = new ExpanDialSticks.SafetyOverlayMode[] { ExpanDialSticks.SafetyOverlayMode.SafetyZoneEdge, ExpanDialSticks.SafetyOverlayMode.SafetyIntentSurface};
	private int[] targets;
	private int currTarget;
	private int currTargetIndex;
	private int[] pauses;
	private sbyte[] displacements;

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
		currTime = prevRandomTextureTime = prevRandomIconTime = prevRandomShapeTime = 0;

		int nbTargets = nbTrials * amountFactor.Length * overlayFactor.Length;
		targets = new int[nbTargets];

		int nbPins = expanDialSticks.NbRows * expanDialSticks.NbColumns;
		int[] randomPins = Enumerable.Range(0, nbPins).ToArray();

		for(int i = 0; i < nbTargets; i++)
		{
			if(i%nbPins == 0) randomPins = Shuffle(randomPins);
			targets[i] = randomPins[i%nbPins];
		}
		currTargetIndex = 0;
		currTarget = targets[currTargetIndex];

		pauses = new int[0];
		displacements = new sbyte[pauses.Length];

	}


	private void HandleConnecting(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("Application connecting to MQTT Broker @" + e.address + ":" + e.port + "...");
		connected = false;
		//StartCoroutine(ResetProjectorSafeGuard());
	}

	private void HandleConnected(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("Application connected.");
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
		int targetRow = (int) (currTarget / ExpanDialSticks.nbColumns);
		int targetColumn = (int) (currTarget % ExpanDialSticks.nbColumns);
		if(e.i == targetRow && e.j == targetColumn)
		{
			TriggerShapeChangeUnderBody();
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
	/*List<Vector3> FindAllUnsafes()
	{
		List<Vector3> safePositions = new List<Vector3>();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
				if (expanDialSticks.modelMatrix[i, j].CurrentProximity == 1f)
				{
					safePositions.Add(new Vector3(i, j, expanDialSticks.modelMatrix[i, j].CurrentProximity));
				}

		return safePositions;
	}*/

	int[] FindAllUnsafes()
	{
		List<int> safePositions = new List<int>();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
				if (expanDialSticks.modelMatrix[i, j].CurrentProximity == 1f)
				{
					safePositions.Add(i * expanDialSticks.NbColumns +  j);
				}

		return safePositions.ToArray();
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

	void TriggerShapeChangeUnderBody()
	{
		if (currTargetIndex < targets.Length)
		{
			currTarget = targets[currTargetIndex++];

			// unpause prev pins
			for (int i = 0; i < pauses.Length; i++)
			{
				int x = pauses[i] / expanDialSticks.NbColumns;
				int y = pauses[i] % expanDialSticks.NbColumns;
				expanDialSticks.modelMatrix[x, y].CurrentFeedForwarded = 0;
			}
			// get all unsafe pins
			int[] unsafes = FindAllUnsafes();
			if (unsafes.Length > 0)
			{
				// shuffle unsafe pins
				unsafes = Shuffle(unsafes);
				// select first N unsafe pins to move and pause
				int nbPauses = 3;
				pauses = new int[nbPauses];
				Array.Copy(unsafes, pauses, (unsafes.Length < nbPauses) ? unsafes.Length : nbPauses);
			}
			// pause next pins
			displacements = new sbyte[pauses.Length];
			for(int i = 0; i < pauses.Length; i++)
			{
				int x = pauses[i] / expanDialSticks.NbColumns;
				int y = pauses[i] % expanDialSticks.NbColumns;
				displacements[i] = (sbyte)Random.Range(-expanDialSticks.modelMatrix[x, y].CurrentPosition, 40 - expanDialSticks.modelMatrix[x, y].CurrentPosition);
				expanDialSticks.modelMatrix[x, y].CurrentFeedForwarded = displacements[i];
			}
			// trigger Safety Overlay
			expanDialSticks.triggerSafetyChange();


			int[] iconIndexes = Enumerable.Range(0, expanDialSticks.NbRows * expanDialSticks.NbColumns).ToArray();
			int[] randomIconIndexes = Shuffle(iconIndexes);
			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					// Target Icon
					if (currTarget == i * expanDialSticks.NbColumns + j)
					{
						expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "icon" + randomIconIndexes[i * expanDialSticks.NbColumns + j];
					}
					else
					{
						expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "default";
					}
					expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = 90f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 2f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = 0.1f;

					// Random Shape
					/*if(Array.IndexOf(stringArray, value)) { }
					sbyte randomPosition = (sbyte)Random.Range(0, 40);
					expanDialSticks.modelMatrix[i, j].TargetPosition = randomPosition;
					expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = 2f;*/
				}
			}
			expanDialSticks.triggerProjectorChange();
			//expanDialSticks.triggerShapeChange();

		}
	}

	/*void RandomIcon()
	{
		if(currTargetIndex < targets.Length)
		{
			int[] iconIndexes = Enumerable.Range(0, expanDialSticks.NbRows * expanDialSticks.NbColumns).ToArray();
			int[] randomIconIndexes = Shuffle(iconIndexes);
			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					if (currTarget == i * expanDialSticks.NbColumns + j)
					{
						expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "icon" + randomIconIndexes[i * expanDialSticks.NbColumns + j];
					}
					else
					{

						expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "default";
					}
					expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = 90f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 2f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = 0.1f;
				}
			}
			currTarget = targets[++currTargetIndex];
			expanDialSticks.triggerProjectorChange();

		}
	}*/

	void RandomColor()
	{

		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				if (expanDialSticks.viewMatrix[i, j].TargetTextureChangeDuration <= 0f) // if previous texture change is finished
				{
					Color randomColor = Random.ColorHSV(0f, 1f, 0f, 1f, 0.5f, 1f);
					float randomDuration = Random.Range(0.25f, 5f);
					expanDialSticks.modelMatrix[i, j].TargetColor = randomColor;
					expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = randomDuration;
				} else
				{
					expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = expanDialSticks.viewMatrix[i, j].TargetTextureChangeDuration;
				}
			}
		}
		expanDialSticks.triggerTextureChange();
	}



	/*void RandomShape()
	{

		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
					sbyte randomPosition = (sbyte)Random.Range(0, 40);
					expanDialSticks.modelMatrix[i, j].TargetPosition = randomPosition;
					expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = 2f;
			}
		}
		expanDialSticks.triggerShapeChange();
	}*/

	void CheckShapeChangeUnderBody()
	{
		bool hasChanged = false;
		for (int i = 0; i < pauses.Length; i++)
		{
			int x = pauses[i] / expanDialSticks.NbColumns;
			int y = pauses[i] % expanDialSticks.NbColumns;

			if (expanDialSticks.modelMatrix[x, y].CurrentProximity < 1f && displacements[i] != 0)
			{
				hasChanged = true;
				// Motion On
				expanDialSticks.modelMatrix[x, y].TargetPosition = (sbyte)(expanDialSticks.modelMatrix[x, y].CurrentPosition + displacements[i]);
				expanDialSticks.modelMatrix[x, y].TargetShapeChangeDuration = 2f;
				// Overlay Off
				expanDialSticks.modelMatrix[x, y].CurrentFeedForwarded = 0;
				// no more displacement
				displacements[i] = 0;
			}
		}
		if (hasChanged)
		{
			expanDialSticks.triggerShapeChange();
			expanDialSticks.triggerSafetyChange();
		}

	}

	void Update()
	{
		// check if ExpanDialSticks is connected
		if (connected)
		{
			currTime = Time.time;

			CheckShapeChangeUnderBody();
			// random texture every 3 secondes
			if (currTime - prevRandomTextureTime >= 3f)
			{
				Debug.Log("RandomTexture!");
				RandomColor();
				prevRandomTextureTime = currTime;
			}
			// random icon every 5 secondes
			/*if (currTime - prevRandomIconTime >= 5f)
			{
				Debug.Log("RandomIcon!");
				RandomIcon();
				prevRandomIconTime = currTime;
			}*/

			// random shape every 4 secondes
			/*if (currTime - prevRandomShapeTime >= 4f)
			{
				Debug.Log("RandomShape!");
				RandomShape();
				prevRandomShapeTime = currTime;
			}*/


			if (Input.GetKey("escape"))
			{
				Quit();
			}

			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				TriggerShapeChangeUnderBody();
				/*int x = Random.Range(0, 5);
				int y = Random.Range(0, 6);
				expanDialSticks.modelMatrix[x, y].CurrentFeedForwarded = 20;
				expanDialSticks.triggerSafetyChange();
				Debug.Log(x + " " + y + " 20");*/
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				ExpanDialSticks.SafetyOverlayMode currentOverlayMode = expanDialSticks.getOverlayMode();
				if(currentOverlayMode == ExpanDialSticks.SafetyOverlayMode.SafetyZoneEdge)
				{
					expanDialSticks.SetOverlayMode(ExpanDialSticks.SafetyOverlayMode.SafetyIntentSurface);
				} else
				{
					expanDialSticks.SetOverlayMode(ExpanDialSticks.SafetyOverlayMode.SafetyZoneEdge);
				}
			}
		}

		// FACTOR0 -> Shape | Hull | Zone
		// FACTOR1 -> 3 | 6 | 9
		// nbTrials = FACTOR0(3) x FACTOR1(3) x TRIAL(3) = 27

		// change pin color every 0.1-3.0 secondes
		// change pin icon every 0.1-3.0 secondes

		// TASK0 -> user rotates pin with given icon
		// TRIGGER0 -> system triggers shape-change under user body
		// TASK1 -> user rotates paused pins by ascending order (moving lower -> moving higher)



	}
}
