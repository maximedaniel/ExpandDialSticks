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
	private const sbyte minPos = 0;
	private const sbyte maxPos = 10;

	private int nbTrials = 3;
	private int[] amountFactor = new int[] { 3, 6, 9 };
	private ExpanDialSticks.SafetyOverlayMode[] overlayFactor = new ExpanDialSticks.SafetyOverlayMode[] {
		ExpanDialSticks.SafetyOverlayMode.MotionTrajectoryZone,
		ExpanDialSticks.SafetyOverlayMode.MotionTrajectoryHull,
		ExpanDialSticks.SafetyOverlayMode.MotionTrajectoryFill,
		ExpanDialSticks.SafetyOverlayMode.MotionZoneEdge
	};
	private int currIndex;
	private List<int> targets;
	private int currTarget;
	private List<int> amounts;
	private int currAmount;
	private ExpanDialSticks.SafetyOverlayMode currOverlay;
	private List<Vector2Int> pauses;
	//private List<int> unpauses;
	//private List<sbyte> displacements;
	//private List<int> selects;


	private float shapeChangeWaitFor = 3f;
	private bool toNextTarget = true;


	private bool training = false;
	private string stringParticipant = "";
	private int numeroParticipant = 0;
	private bool unknownParticipant = true;


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

		int nbTargets = nbTrials * amountFactor.Length;
		targets = new List<int>();
		amounts = new List<int>();

		List<int> randomPins = new List<int>(); //= Enumerable.Range(0, nbPins).ToList<int>();
		// get pins inside matrix only
		for(int i = 1; i < expanDialSticks.NbRows-1; i++)
		{
			for (int j = 1; j < expanDialSticks.NbColumns - 1; j++)
			{
				randomPins.Add(i * expanDialSticks.NbColumns + j);
			}
		}
		int nbPins = randomPins.Count();

		for (int j = 0; j < amountFactor.Length; j++)
		{
			for (int w = 0; w < nbTrials; w++)
			{
				int z = (j * nbTrials) + w;
				amounts.Add(amountFactor[j]);
				if (z % nbPins == 0) randomPins = Shuffle(randomPins);
				targets.Add(randomPins[z % nbPins]);
			}
		}

		currIndex = 0;
		currTarget = targets[currIndex];
		currAmount = amounts[currIndex];
		currOverlay = ExpanDialSticks.SafetyOverlayMode.MotionZoneEdge;
		pauses = new List<Vector2Int>();
		toNextTarget = true;
		/*unpauses = new List<int>();
		displacements = new List<sbyte>();
		selects = new List<int>();*/

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
		//RandomShape();
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
		int select = e.i * ExpanDialSticks.nbColumns + e.j;
		//Debug.Lo
		// if rotate is not paused and is the target
		if (select == currTarget)
		{
			Debug.Log("Right Target : " + select + " == " + currTarget);
			TriggerShapeChangeUnderBody();
			toNextTarget = true;
		}
		else
		{

			Debug.Log("Wrong Target : " + select + " != "  + currTarget);
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

	List<int> FindAllUnsafesAtDistance(float distance)
	{
		List<int> unsafePositions = new List<int>();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
				if (expanDialSticks.modelMatrix[i, j].CurrentProximity == 1f && expanDialSticks.modelMatrix[i, j].CurrentDistance < distance)
				{
					unsafePositions.Add(i * expanDialSticks.NbColumns + j);
				}

		return unsafePositions;
	}

	List<int> FindAllUnsafes()
	{
		List<int> unsafePositions = new List<int>();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
				if (expanDialSticks.modelMatrix[i, j].CurrentProximity == 1f)
				{
					unsafePositions.Add(i * expanDialSticks.NbColumns + j);
				}
		return unsafePositions;
	}
	List<int> FindAllSafes()
	{
		List<int> safePositions = new List<int>();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
				if (expanDialSticks.modelMatrix[i, j].CurrentProximity < 1f)
				{
					safePositions.Add(i * expanDialSticks.NbColumns + j);
				}
		return safePositions;
	}

	public List<int> Shuffle(List<int> array)
	{
		for (int i = 0; i < array.Count(); i++)
		{
			int rnd = Random.Range(0, array.Count());
			int temp = array[rnd];
			array[rnd] = array[i];
			array[i] = temp;
		}
		return array;
	}
	public List<sbyte> Swap(List<sbyte> array, int index0, int index1)
	{
		if (0 <= index0 && index0 < array.Count() && 0 <= index1 && index1 < array.Count())
		{
			sbyte tmp = array[index0];
			array[index0] = array[index1];
			array[index0] = tmp;
		}
		return array;
	}
	public List<int> Swap(List<int> array, int index0, int index1)
	{
		if (0 <= index0 && index0 < array.Count() && 0 <= index1 && index1 < array.Count())
		{
			int tmp = array[index0];
			array[index0] = array[index1];
			array[index0] = tmp;
		}
		return array;
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



	void RandomShape()
	{

		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
					sbyte randomPosition = (sbyte)Random.Range(10, 30);
					expanDialSticks.modelMatrix[i, j].TargetPosition = randomPosition;
					expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = 2f;
			}
		}
		expanDialSticks.triggerShapeChange();
	}
	/*
	IEnumerator NextTarget()
	{
		Debug.Log("WaitForNoHandPresence...");
		bool handIsPresent = true;
		float waitingSince = 0f;
		float waitingCount = 1;
		List<int> allUnsafes = new List<int>();

		while (handIsPresent)
		{
			if (!leftHand.IsActive() && !rightHand.IsActive())
			{
				allUnsafes = FindAllUnsafes();
				if (allUnsafes.Count() == 0)
				{
					handIsPresent = false;
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
		for (int i = 0; i < unpauses.Count(); i++)
		{
			int x = unpauses[i] / expanDialSticks.NbColumns;
			int y = unpauses[i] % expanDialSticks.NbColumns;
			// Motion On
			// ERROR : WRONG DISPALCEMENT APPLIED
			expanDialSticks.modelMatrix[x, y].TargetPosition = (sbyte)(expanDialSticks.modelMatrix[x, y].CurrentPosition + displacements[i]);
			expanDialSticks.modelMatrix[x, y].TargetShapeChangeDuration = 2f;
		}
		expanDialSticks.triggerShapeChange();
		Debug.Log("TriggerShapeChange!");
		yield return new WaitForSeconds(2f);
		if (currIndex < targets.Count())
		{

			currTarget = targets[currIndex];
			currAmount = amounts[currIndex];
			currOverlay = overlays[currIndex];
			currIndex += 1;
			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					if (currTarget == i * expanDialSticks.NbColumns + j)
					{
						expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "icon0";
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
			expanDialSticks.triggerProjectorChange();
			Debug.Log("ShowNextTarget!");
		}
		else
		{
			Debug.Log("NoNextTarget!");
			Quit();
		}
		waitingForNoHand = false;

	}*/

	List<Vector3> FindSafes()
	{
		List<Vector3> safePositions = new List<Vector3>();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
				if (expanDialSticks.modelMatrix[i, j].CurrentProximity < 1f)
				{
					safePositions.Add(new Vector3(i, j, expanDialSticks.modelMatrix[i, j].CurrentProximity));
				}

		return safePositions;
	}

	
	void CheckShapeChangeUnderBody()
	{
			// move unpaused pins
			List<Vector2Int> unpauses = new List<Vector2Int>();
			for (int i = 0; i < pauses.Count(); i++)
			{
				Vector2Int pause = pauses[i];
				int x = pause.x / expanDialSticks.NbColumns;
				int y = pause.x % expanDialSticks.NbColumns;
				int displacement = pause.y;

				if (expanDialSticks.modelMatrix[x, y].CurrentProximity < 1f && displacement != 0)
				{
					// unpaused index
					unpauses.Add(pause);
					// Motion On
					expanDialSticks.modelMatrix[x, y].TargetPosition = (sbyte)(expanDialSticks.modelMatrix[x, y].CurrentPosition + displacement);
					expanDialSticks.modelMatrix[x, y].TargetShapeChangeDuration = 2f;
					// Overlay Off
					//expanDialSticks.modelMatrix[x, y].CurrentFeedForwarded = 0;
				}
				pauses.RemoveAll(lambda => unpauses.Contains(lambda));
			}
			if (unpauses.Count() > 0)
			{
				expanDialSticks.triggerShapeChange();
				//expanDialSticks.triggerSafetyChange();
			}
	}

	void TriggerNextTarget()
	{
		Debug.Log("TriggerNextTarget!");
		if (currIndex < targets.Count())
		{
			currTarget = targets[currIndex];
			currAmount = amounts[currIndex];
			currIndex += 1;
			Debug.Log("currTarget:"+ currTarget);
			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					if (currTarget == i * expanDialSticks.NbColumns + j)
					{

						//Debug.Log("TargetProjectorTexture:icon0");
						expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "icon0";
					}
					else
					{
						//Debug.Log("TargetProjectorTexture:default");
						expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "default";
					}
					expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = 90f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 2f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = 0.1f;
				}
			}
			Debug.Log("TriggerProjectorChange!");
			expanDialSticks.triggerProjectorChange();
		} else
		{
			Quit();
		}
	}
	void TriggerShapeChangeUnderBody()
	{

		Debug.Log("TriggerShapeChangeUnderBody!");
		// get all unsafe pins
		List<int> unsafes = FindAllUnsafesAtDistance(6f);
	
			if (unsafes.Count() > 0)
			{
				// shuffle unsafe pins
				unsafes = Shuffle(unsafes);
				// select first N unsafe pins to move and pause
				int startIndex = ((unsafes.Count() < currAmount) ? unsafes.Count() : currAmount) - 1;
				unsafes.RemoveRange(startIndex, unsafes.Count() - startIndex);
				// pause next pins
				for (int i = 0; i < unsafes.Count(); i++)
				{
					int x = unsafes[i] / expanDialSticks.NbColumns;
					int y = unsafes[i] % expanDialSticks.NbColumns;
					int currentPosition = expanDialSticks.modelMatrix[x, y].CurrentPosition;
					List<int> randomPositions = Enumerable.Range(minPos, maxPos).Where(randomPos => randomPos != currentPosition).ToList<int>();
					int randomPosition = randomPositions[Random.Range(0, randomPositions.Count())];
					sbyte displacement = (sbyte)(randomPosition - currentPosition);
					//expanDialSticks.modelMatrix[x, y].CurrentFeedForwarded = displacement; 
					expanDialSticks.modelMatrix[x, y].TargetPosition = (sbyte)randomPosition;
					expanDialSticks.modelMatrix[x, y].TargetShapeChangeDuration = 2f;
					pauses.Add(new Vector2Int(unsafes[i], displacement));
				}
				// trigger shape-change
				expanDialSticks.triggerShapeChange();

		} else // LeapMotion failed to track user hands !!
			{
				// add to end queue for retry
				targets.Add(currTarget);
				amounts.Add(currAmount);
		}
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

			stringParticipant = GUI.TextField(new Rect(midX - 55, midY, 50, componentHeight), stringParticipant, 25);

			if (GUI.Button(new Rect(midX + 5, midY - 50, 150, componentHeight), "Training"))
			{
				training = true;
				numeroParticipant = int.Parse(stringParticipant);
				Debug.Log("TRAINING");
				// init trials
				/*InitTrials();
				moleIndex = -1;
				moleState = MOLE_TO_APPEAR;

				currTime = LOG_INTERVAL;
				prevTime = 0f;
				string identity = "USER_IDENTITY " + numeroParticipant + " USER_TRIGGERED TRAINING";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				*/
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY - 25, 150, componentHeight), "Motion Zone Edge"))
			{

				expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.SafetyRatedMonitoredStop);
				training = false;
				numeroParticipant = int.Parse(stringParticipant);
				Debug.Log("Start");

				currOverlay = ExpanDialSticks.SafetyOverlayMode.MotionZoneEdge;
				expanDialSticks.SetOverlayMode(currOverlay);
				expanDialSticks.triggerSafetyChange();

				//string identity = "USER_IDENTITY " + numeroParticipant + " USER_TRIGGERED SMS TRIAL";
				//expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY, 150, componentHeight), "Motion Trajectory Fill"))
			{

				expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.SafetyRatedMonitoredStop);
				training = false;
				numeroParticipant = int.Parse(stringParticipant);
				Debug.Log("Start");

				currOverlay = ExpanDialSticks.SafetyOverlayMode.MotionTrajectoryFill;
				expanDialSticks.SetOverlayMode(currOverlay);
				expanDialSticks.triggerSafetyChange();

				//string identity = "USER_IDENTITY " + numeroParticipant + " USER_TRIGGERED SMS TRIAL";
				//expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				unknownParticipant = false;
			}
			if (GUI.Button(new Rect(midX + 5, midY + 25, 150, componentHeight), "Motion Trajectory Hull"))
			{

				expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.SafetyRatedMonitoredStop);
				training = false;
				numeroParticipant = int.Parse(stringParticipant);
				Debug.Log("Start");

				currOverlay = ExpanDialSticks.SafetyOverlayMode.MotionTrajectoryHull;
				expanDialSticks.SetOverlayMode(currOverlay);
				expanDialSticks.triggerSafetyChange();

				//string identity = "USER_IDENTITY " + numeroParticipant + " USER_TRIGGERED SMS TRIAL";
				//expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				unknownParticipant = false;
			}
			if (GUI.Button(new Rect(midX + 5, midY + 50, 150, componentHeight), "Motion Trajectory Zone"))
			{

				expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.SafetyRatedMonitoredStop);
				training = false;
				numeroParticipant = int.Parse(stringParticipant);
				Debug.Log("Start");

				currOverlay = ExpanDialSticks.SafetyOverlayMode.MotionTrajectoryZone;
				expanDialSticks.SetOverlayMode(currOverlay);
				expanDialSticks.triggerSafetyChange();

				//string identity = "USER_IDENTITY " + numeroParticipant + " USER_TRIGGERED SMS TRIAL";
				//expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				unknownParticipant = false;
			}
		}
	}

	void Update()
	{
		// check if ExpanDialSticks is connected
		if (connected && !unknownParticipant)
		{
			currTime = Time.time;

			if(toNextTarget == true)
			{
				TriggerNextTarget();
				toNextTarget = false;
			}

			//CheckShapeChangeUnderBody();

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
					int i = (int)(currTarget / ExpanDialSticks.nbColumns);
					int j = (int)(currTarget % ExpanDialSticks.nbColumns);
					HandleRotationChanged(this, new ExpanDialStickEventArgs(DateTime.Now, i, j, 0, 10, 10));
			}
		}
	}
}
