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

public class XP2_P1BIS : MonoBehaviour
{

	// ExpanDialSticks Core
	public GameObject expanDialSticksPrefab;
	public GameObject capsuleHandLeftPrefab;
	public GameObject capsuleHandRightPrefab;
	private MyCapsuleHand leftHand;
	private MyCapsuleHand rightHand;
	private ExpanDialSticks expanDialSticks;
	private bool connected;

	private const float LOG_INTERVAL = 0.2f; // 0.2f;
	private float currTime;
	private float prevRandomTextureTime;
	private float prevMetricsTime;


	// generate all potential positions for selected candidates
	public int nbPositionCandidate = 3;
	public int stepPositionCandidate = 10;
	private List<int> positionCandidates;
	private const int minPos = 10;
	private const int maxPos = 20;
	private const float shapeChangeDuration = 2f;
	private const float safetyDistance = 6f;

	private string stringParticipant = "";
	private int numeroParticipant = 0;
	private bool unknownParticipant = true;

	public const string MQTT_EMPATICA_RECORDER = "EMPATICA_RECORDER";
	public const string MQTT_SYSTEM_RECORDER = "SYSTEM_RECORDER";
	public const string CMD_START = "START";
	public const string CMD_STOP = "STOP";

	private ExpanDialSticks.SafetyOverlayMode currOverlay;
	public int maxIconUnder = 3;
	private int currIconUnder;
	private int nbIconFactor;
	private const int nbRepeat = 5;
	private List<int> trials;
	private int nbTrials;
	private List<Vector2Int> candidates;

	private Vector3Int rightCandidate;
	private Vector3Int wrongCandidate;
	private List<Vector3Int> movingCandidates;
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

		// generate potential candidates
		candidates = new List<Vector2Int>(); //= Enumerable.Range(0, nbPins).ToList<int>();
											 // get pins inside matrix only
		for (int i = 1; i < expanDialSticks.NbRows - 1; i++)
		{
			for (int j = 1; j < expanDialSticks.NbColumns - 1; j++)
			{
				candidates.Add(new Vector2Int(i, j));
			}
		}
		movingCandidates = new List<Vector3Int>();
		// Generate potential positions for selected candidates
		positionCandidates = new List<int>();
		for (int i = 0; i < nbPositionCandidate; i++)
		{
			positionCandidates.Add(i * stepPositionCandidate);
		}

		// generate trials

		trials = new List<int>(); //= Enumerable.Range(0, nbPins).ToList<int>();
		for (int i = 0; i < maxIconUnder; i++)
		{
			for (int j = 0; j < nbRepeat; j++)
			{
				trials.Add(i);
			}
		}
		ListExtension.Shuffle(trials);
		trials.Insert(0, 0);
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

		if (toNextTrial == false)
		{
			//Debug.Log("target(" + e.i + ", " + e.j + ") == right(" + rightCandidate.x + ", " + rightCandidate.y + ")?");
			if (e.i == rightCandidate.x && e.j == rightCandidate.y) // right candidate
			{
				string payload = "USER_RIGHT_PIN " + e.i + " " + e.j + " " + expanDialSticks.modelMatrix[e.i, e.j].CurrentPosition;
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				//DebugInSitu("target("+e.i+", "+e.j+") == right("+ rightCandidate.x + ", "+ rightCandidate.y + ")", Color.black, Color.green);
				toNextTrial = true;

			}
			else // wrong candidate
			{

				string payload = "USER_WRONG_PIN " + e.i + " " + e.j + " " + expanDialSticks.modelMatrix[e.i, e.j].CurrentPosition;
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

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

	List<Vector3Int> FindAllUnsafesUnderDistance(float distance)
	{
		List<Vector3Int> positions = new List<Vector3Int>();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
				if (expanDialSticks.modelMatrix[i, j].CurrentDistance < distance)
				{
					positions.Add(new Vector3Int(i, j, expanDialSticks.modelMatrix[i, j].CurrentPosition));
				}

		return positions;
	}
	List<Vector3Int> FindAllSafesAboveDistance(float distance)
	{
		List<Vector3Int> positions = new List<Vector3Int>();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
				if (expanDialSticks.modelMatrix[i, j].CurrentDistance > distance)
				{
					positions.Add(new Vector3Int(i, j, expanDialSticks.modelMatrix[i, j].CurrentPosition));
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
				}
				else
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

			if (GUI.Button(new Rect(midX + 5, midY - 50, 150, componentHeight), "Training Overlay"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				Debug.Log("TRAINING");
				string identity = "USER_IDENTITY " + numeroParticipant + " TRAINING";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
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

			if (GUI.Button(new Rect(midX + 5, midY - 25, 150, componentHeight), "Edge Overlay"))
			{

				expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.SafetyRatedMonitoredStop);
				numeroParticipant = int.Parse(stringParticipant);
				Debug.Log("Start");

				currOverlay = ExpanDialSticks.SafetyOverlayMode.Edge;
				expanDialSticks.SetOverlayMode(currOverlay);
				expanDialSticks.triggerSafetyChange();

				string identity = "USER_IDENTITY " + numeroParticipant + " EDGE_OVERLAY";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY, 150, componentHeight), "Surface Overlay"))
			{

				expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.SafetyRatedMonitoredStop);
				numeroParticipant = int.Parse(stringParticipant);
				Debug.Log("Start");

				currOverlay = ExpanDialSticks.SafetyOverlayMode.Fill;
				expanDialSticks.SetOverlayMode(currOverlay);
				expanDialSticks.triggerSafetyChange();

				string identity = "USER_IDENTITY " + numeroParticipant + " SURFACE_OVERLAY";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				unknownParticipant = false;
			}
			if (GUI.Button(new Rect(midX + 5, midY + 25, 150, componentHeight), "Hull Overlay"))
			{

				expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.SafetyRatedMonitoredStop);
				numeroParticipant = int.Parse(stringParticipant);
				Debug.Log("Start");

				currOverlay = ExpanDialSticks.SafetyOverlayMode.Hull;
				expanDialSticks.SetOverlayMode(currOverlay);
				expanDialSticks.triggerSafetyChange();

				string identity = "USER_IDENTITY " + numeroParticipant + " HULL_OVERLAY";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				unknownParticipant = false;
			}
			if (GUI.Button(new Rect(midX + 5, midY + 50, 150, componentHeight), "Zone Overlay"))
			{

				expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.SafetyRatedMonitoredStop);
				numeroParticipant = int.Parse(stringParticipant);
				Debug.Log("Start");

				currOverlay = ExpanDialSticks.SafetyOverlayMode.Zone;
				expanDialSticks.SetOverlayMode(currOverlay);
				expanDialSticks.triggerSafetyChange();

				string identity = "USER_IDENTITY " + numeroParticipant + " ZONE_OVERLAY";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
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
			List<Vector3Int> unsafes;
			List<Vector3Int> safes;
			//Vector2Int rightCandidatePosition = new Vector2Int(rightCandidate.x, rightCandidate.y);
			bool candidatesFound = false;
			int iconFactorIndex = -1;
			while (!candidatesFound)
			{
				iconFactorIndex++;
				if (iconFactorIndex < trials.Count())
				{
					currIconUnder = trials[iconFactorIndex];
					Debug.Log("Looking for candidates (" + currIconUnder + ")...");

					// get all pins under and around user body
					unsafes = FindAllUnsafesUnderDistance(safetyDistance);
					safes = FindAllSafesAboveDistance(safetyDistance);

					// prevent same candidates
					unsafes.RemoveAll(x => movingCandidates.Contains(x));
					safes.RemoveAll(x => movingCandidates.Contains(x));
					// candidates to find under or above
					int nbUnderCandidates = currIconUnder;
					int nbAroundCandidates = maxIconUnder - currIconUnder;
					Predicate<Vector3Int> hasPotentialRightCandidate = candidate => Math.Abs(positionCandidates.Last() - candidate.z) >= stepPositionCandidate;
					List<Vector3Int> potentialRightCandidates = new List<Vector3Int>();
					potentialRightCandidates.AddRange(unsafes.FindAll(hasPotentialRightCandidate));
					potentialRightCandidates.AddRange(safes.FindAll(hasPotentialRightCandidate));
					// if there is enough candidates
					if (unsafes.Count() >= nbUnderCandidates && safes.Count() >= nbAroundCandidates && potentialRightCandidates.Count() > 0)
					{
						// Randomly select the right candidate
						rightCandidate = potentialRightCandidates[Random.Range(0, potentialRightCandidates.Count())];
						// Get all moving candidates
						movingCandidates = new List<Vector3Int>();
						ListExtension.Shuffle(unsafes);
						ListExtension.Shuffle(safes);
						bool isSafe = safes.Remove(rightCandidate);
						//Debug.Log("isSafe => " + isSafe);
						bool isUnsafe = unsafes.Remove(rightCandidate);
						//Debug.Log("isUnsafe => " + isUnsafe);
						// check if right candidates is safe or unsafe to not select him
						if (isSafe)
						{
							movingCandidates.AddRange(unsafes.GetRange(0, nbUnderCandidates));
							movingCandidates.AddRange(safes.GetRange(0, nbAroundCandidates - 1));
						}
						else if (isUnsafe)
						{
							movingCandidates.AddRange(unsafes.GetRange(0, nbUnderCandidates - 1));
							movingCandidates.AddRange(safes.GetRange(0, nbAroundCandidates));
						}
						// generate position for moving candidates
						for (int i = 0; i < movingCandidates.Count(); i++)
						{
							Vector3Int movingCandidate = movingCandidates[i];
							movingCandidate.z = Math.Abs(positionCandidates[0] - movingCandidate.z) > Math.Abs(positionCandidates[1] - movingCandidate.z) ? positionCandidates[0] : positionCandidates[1];
							movingCandidates[i] = movingCandidate;
						}
						//Debug.Log("movingCandidates.Count() => " + movingCandidates.Count());
						// Select a wrong candidate
						wrongCandidate = movingCandidates[Random.Range(0, movingCandidates.Count())];
						// Add the right candidate to moving candidate
						rightCandidate.z = positionCandidates.Last();
						movingCandidates.Add(rightCandidate);


						candidatesFound = true;
					}
					else
					{
						//What if?
					}
				}
				else // fails to find candidates for remaining icon factor
				{
					Debug.Log("Candidates Fail! Adding IconFactor.NoIconUnder.");
					iconFactorIndex = -1;
					trials.Insert(0, 0);
				}
			}
			Debug.Log("Candidates Successfully found!");
			trials.RemoveAt(iconFactorIndex);

			// Output Control
			List<int> randomIcons = new List<int>(); //= Enumerable.Range(0, nbPins).ToList<int>();
			for (int i = 1; i < 30; i++)
			{
				randomIcons.Add(i);
			}
			ListExtension.Shuffle(randomIcons);

			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					Vector3Int movingCandidate = movingCandidates.Find(l => (l.x == i && l.y == j));
					if (rightCandidate.x == i && rightCandidate.y == j)
					{
						Debug.Log("rightCandidate => " + rightCandidate);
						// Projector
						expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "icon0";
						// Shape
						expanDialSticks.modelMatrix[i, j].TargetPosition = (sbyte)rightCandidate.z;
						expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = shapeChangeDuration;
					}
					else if (wrongCandidate.x == i && wrongCandidate.y == j)
					{
						Debug.Log("wrongCandidate => " + wrongCandidate);
						// Projector
						expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "icon0";
						// Shape
						expanDialSticks.modelMatrix[i, j].TargetPosition = (sbyte)wrongCandidate.z;
						expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = shapeChangeDuration;
					}
					else if (movingCandidate != new Vector3Int())
					{
						Debug.Log("movingCandidate => " + movingCandidate);
						// Projector
						expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "icon" + randomIcons.First();
						randomIcons.RemoveAt(0);
						// Shape
						expanDialSticks.modelMatrix[i, j].TargetPosition = (sbyte)movingCandidate.z;
						expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = shapeChangeDuration;
					}
					else
					{
						// Projector
						//expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "default";
						expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "icon" + randomIcons.First();
						randomIcons.RemoveAt(0);
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
			string iconSituationMsg = "ICON_APPARATUS " + currIconUnder;
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(iconSituationMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
			string selectCandidateMsg = "SYSTEM_SELECT_PIN ";
			foreach (Vector3Int movingCandidate in movingCandidates)
			{
				selectCandidateMsg += "(" + movingCandidate.x + ", " + movingCandidate.y + ", " + movingCandidate.z + ") ";
			}
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(selectCandidateMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
			string rightCandidateMsg = "SYSTEM_RIGHT_PIN (" + rightCandidate.x + ", " + rightCandidate.y + ", " + rightCandidate.z + ")";
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(rightCandidateMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
			string wrongCandidateMsg = "SYSTEM_WRONG_PIN (" + wrongCandidate.x + ", " + wrongCandidate.y + ", " + wrongCandidate.z + ")";
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(wrongCandidateMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

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

			if (toNextTrial == true)
			{
				TriggerNextTrial();
				toNextTrial = false;
			}

			//CheckShapeChangeUnderBody();

			// random texture every 3 secondes
			if (currTime - prevRandomTextureTime >= 3f)
			{
				//Debug.Log("RandomTexture!");
				RandomColor();
				prevRandomTextureTime = currTime;
			}

			if (currTime - prevMetricsTime >= LOG_INTERVAL)
			{
				LogMetrics();
				prevMetricsTime = currTime;
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

	void LogMetrics()
	{

		//string colorString = "SYSTEM_COLOR ";
		string proximityString = "SYSTEM_PROXIMITY ";
		string positionString = "SYSTEM_POSITION ";
		string leftHandString = "USER_LEFT_HAND " + leftHand.ToString();
		string rightHandString = "USER_RIGHT_HAND " + rightHand.ToString();

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
	}
}
