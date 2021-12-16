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

public static class ListExtension
{
	public static System.Random rng = new System.Random();

	public static void Shuffle<T>(this IList<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = rng.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}
}
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

	private float currTime;
	private float prevRandomTextureTime;


	private const int minPos = 10;
	private const int maxPos = 20;
	private const float shapeChangeDuration = 2f;
	private const float safetyDistance = 6f;

	private bool training = false;
	private string stringParticipant = "";
	private int numeroParticipant = 0;
	private bool unknownParticipant = true;

	private ExpanDialSticks.SafetyOverlayMode currOverlay;
	private enum IconFactor { TwoIconsUnder, OneIconUnder, NoIconUnder};
	private IconFactor currIconFactor;
	private int nbIconFactor;
	private const int nbRepeat = 3;
	private const int nbChange = 3;
	private List<IconFactor> trials;
	private int nbTrials;
	private List<Vector2Int> candidates;
	private Vector3Int rightCandidate;
	private Vector3Int wrongCandidate;
	private bool toNextTrial = true;

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

		// generate candidates
		candidates = new List<Vector2Int>(); //= Enumerable.Range(0, nbPins).ToList<int>();
												// get pins inside matrix only
		for (int i = 1; i < expanDialSticks.NbRows - 1; i++)
		{
			for (int j = 1; j < expanDialSticks.NbColumns - 1; j++)
			{
				candidates.Add(new Vector2Int(i,j));
			}
		}

		// generate trials
		nbIconFactor = Enum.GetNames(typeof(IconFactor)).Length;

		trials = new List<IconFactor>(); //= Enumerable.Range(0, nbPins).ToList<int>();
		for (int i = 0; i < nbIconFactor; i++)
		{
			for(int j = 0; j < nbRepeat; j++)
			{
				trials.Add((IconFactor)i);
			}
		}
		ListExtension.Shuffle(trials);
		trials.Insert(0, IconFactor.NoIconUnder);
		nbTrials = trials.Count();
		toNextTrial = true;
	}


	private void HandleConnecting(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("Application connecting to MQTT Broker @" + e.address + ":" + e.port + "...");
		connected = false;
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

		DebugInSitu("target(" + e.i + ", " + e.j + ") == right(" + rightCandidate.x + ", " + rightCandidate.y + ")", Color.black, Color.white);
		if (toNextTrial == false)
		{
			
			if (e.i == rightCandidate.x && e.j == rightCandidate.y) // right candidate
			{
				//DebugInSitu("target("+e.i+", "+e.j+") == right("+ rightCandidate.x + ", "+ rightCandidate.y + ")", Color.black, Color.green);
				toNextTrial = true;

			}
			else // wrong candidate
			{

				//DebugInSitu("target(" + e.i + ", " + e.j + ") != right(" + rightCandidate.x + ", " + rightCandidate.y + ")", Color.black, Color.red);
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

	List<Vector2Int> FindAllUnsafesUnderDistance(float distance)
	{
		List<Vector2Int> positions = new List<Vector2Int>();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
				if (expanDialSticks.modelMatrix[i, j].CurrentDistance < distance)
				{
					positions.Add(new Vector2Int(i, j));
				}

		return positions;
	}
	List<Vector2Int> FindAllSafesAboveDistance(float distance)
	{
		List<Vector2Int> positions = new List<Vector2Int>();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
				if (expanDialSticks.modelMatrix[i, j].CurrentDistance > distance)
				{
					positions.Add(new Vector2Int(i, j));
				}

		return positions;
	}
	List<Vector2Int> FindAllUnsafes()
	{
		List<Vector2Int> unsafePositions = new List<Vector2Int>();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
				if (expanDialSticks.modelMatrix[i, j].CurrentProximity == 1f)
				{
					unsafePositions.Add(new Vector2Int(i, j));
				}
		return unsafePositions;
	}
	List<Vector2Int> FindAllSafes()
	{
		List<Vector2Int> safePositions = new List<Vector2Int>();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
				if (expanDialSticks.modelMatrix[i, j].CurrentProximity < 1f)
				{
					safePositions.Add(new Vector2Int(i, j));
				}
		return safePositions;
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

	private void DebugInSitu(string message, Color textColor, Color backgroundColor)
	{
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 16, textColor, message, new Vector3(90f, -90f, 0f));
		expanDialSticks.setBorderBackground(backgroundColor);
		expanDialSticks.triggerTextureChange();
	}
	private void TriggerNextTrial()
	{
		if (trials.Count() > 0)
		{
			List<Vector2Int> unsafes;
			List<Vector2Int> safes;
			Vector2Int rightCandidatePos = new Vector2Int(rightCandidate.x, rightCandidate.y);
			Vector2Int wrongCandidatePos = new Vector2Int(wrongCandidate.x, wrongCandidate.y);
			bool candidatesFound = false;
			int iconFactorIndex = -1;
			while (!candidatesFound)
			{
				iconFactorIndex++;
				if(iconFactorIndex < trials.Count()) 
				{
					currIconFactor = trials[iconFactorIndex];
					Debug.Log("Looking for candidates (" + currIconFactor+")...");
					switch (currIconFactor)
					{
						case IconFactor.TwoIconsUnder:
							// get all pins under user body
							unsafes = FindAllUnsafesUnderDistance(safetyDistance);
							// prevent same candidates
							unsafes.Remove(rightCandidatePos);
							unsafes.Remove(wrongCandidatePos);
							// if there is remaining candidates
							if (unsafes.Count() > 1)
							{
								ListExtension.Shuffle(unsafes);
								rightCandidate = new Vector3Int(unsafes[0].x, unsafes[0].y, maxPos);
								wrongCandidate = new Vector3Int(unsafes[1].x, unsafes[1].y, minPos);
								candidatesFound = true;
							}
							else
							{
								//What if?
							}
							break;
						case IconFactor.OneIconUnder:
							// get all pins under user body
							unsafes = FindAllUnsafesUnderDistance(safetyDistance);
							// prevent same candidates
							unsafes.Remove(rightCandidatePos);
							unsafes.Remove(wrongCandidatePos);
							// get all pins not under user body
							safes = FindAllSafesAboveDistance(safetyDistance);
							// prevent same candidates
							safes.Remove(rightCandidatePos);
							safes.Remove(wrongCandidatePos);
							// if there is remaining candidates
							if (unsafes.Count() > 0 && safes.Count() > 0)
							{
								rightCandidate = new Vector3Int(unsafes[0].x, unsafes[0].y, maxPos);
								wrongCandidate = new Vector3Int(safes[0].x, safes[0].y, minPos);
								candidatesFound = true;
							}
							else
							{
								//What if?
							}

							break;
						case IconFactor.NoIconUnder:
							// get all pins not under user body
							safes = FindAllSafesAboveDistance(safetyDistance);
							// prevent same candidates
							safes.Remove(rightCandidatePos);
							safes.Remove(wrongCandidatePos);
							// if there is remaining candidates
							if (safes.Count() > 1)
							{
								ListExtension.Shuffle(safes);
								rightCandidate = new Vector3Int(safes[0].x, safes[0].y, maxPos);
								wrongCandidate = new Vector3Int(safes[1].x, safes[1].y, minPos);
								candidatesFound = true;
							}
							else
							{
								//What if?
							}
							break;
					}
				} else // fails to find candidates for remaining icon factor
				{
					Debug.Log("Candidates Fail! Adding IconFactor.NoIconUnder.");
					iconFactorIndex = -1;
					trials.Insert(0, IconFactor.NoIconUnder);
				}

			}
			Debug.Log("Candidates Success!");
			trials.RemoveAt(iconFactorIndex);


			// Output Control
			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					if (rightCandidate.x == i && rightCandidate.y == j)
					{
						// Projector
						expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "icon0";
						// Shape
						expanDialSticks.modelMatrix[i, j].TargetPosition = (sbyte)rightCandidate.z;
						expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = shapeChangeDuration;
					}
					else if (wrongCandidate.x == i && wrongCandidate.y == j)
					{
						// Projector
						expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "icon0";
						// Shape
						expanDialSticks.modelMatrix[i, j].TargetPosition = (sbyte)wrongCandidate.z;
						expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = shapeChangeDuration;
					}
					else
					{
						// Projector
						expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "default";
					}
					// Projector
					expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = 90f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 2f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = 0.1f;
				}
			}
			expanDialSticks.triggerProjectorChange();
			expanDialSticks.triggerShapeChange();

			string participantNumber = "<pos=0%><b>P" + numeroParticipant + "</b>";
			string trialProgress = "<pos=90%><b>" + trials.Count() + "/" + nbTrials + "</b>";
			string legend = participantNumber + trialProgress;
			expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 16, Color.black, legend, new Vector3(90f, -90f, 0f));
			expanDialSticks.setBorderBackground(Color.white);
			expanDialSticks.triggerTextureChange();

		}
		else
		{
			Quit();
		}
	}

	void Update()
	{
		// check if ExpanDialSticks is connected
		if (connected && !unknownParticipant)
		{
			currTime = Time.time;

			if(toNextTrial == true)
			{
				TriggerNextTrial();
				toNextTrial = false;
			}

			//CheckShapeChangeUnderBody();

			// random texture every 3 secondes
			if (currTime - prevRandomTextureTime >= 3f)
			{
				Debug.Log("RandomTexture!");
				RandomColor();
				prevRandomTextureTime = currTime;
			}

			if (Input.GetKey("escape"))
			{
				Quit();
			}

			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
					HandleRotationChanged(this, new ExpanDialStickEventArgs(DateTime.Now, rightCandidate.x, rightCandidate.y, 0, 10, 10));
			}
		}
	}
}
