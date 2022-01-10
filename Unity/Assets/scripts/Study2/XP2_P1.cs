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

	private const float LOG_INTERVAL = 0.2f; // 0.2f;
	private float currTime;
	private float prevRandomTextureTime;
	private float prevMetricsTime;

	private const int minPos = 0;
	private const int maxPos = 20;
	private const int targetPos = 30;
	private const float shapeChangeDuration = 2f;
	private const float safetyDistance = 6f;


	private bool newOverlay = false;
	private bool landscapeGenerated = false;
	private string stringParticipant = "";
	private int numeroParticipant = 0;
	private bool unknownParticipant = true;
	private bool overlayAppeared = false;

	public const string MQTT_SYSTEM_RECORDER = "SYSTEM_RECORDER";
	public const string CMD_START = "START";
	public const string CMD_STOP = "STOP";


	public enum Difficulty { Easy, Medium, Hard};
	private List<ExpanDialSticks.SafetyOverlayMode> overlays;
	private ExpanDialSticks.SafetyOverlayMode currOverlay;
	private List<Difficulty> difficulties;
	private Difficulty currDifficulty;
	private Dictionary<Difficulty, List<List<int>>> trials;
	private List<int> currSubChanges;
	private Vector2Int target;
	private List<Vector2Int> candidates;
	private List<Vector3Int> diffCandidates;
	private List<Vector3Int> orderedCandidates;
	private Vector3Int prevOrderedCandidate;
	private bool toNextTrial = true;
	private int currTrial = 0;
	private int nbTrials = int.MaxValue;

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
		orderedCandidates = new List<Vector3Int>();
		prevOrderedCandidate = new Vector3Int(-1,-1,-1);
		candidates = new List<Vector2Int>(); //= Enumerable.Range(0, nbPins).ToList<int>();
											 // get pins inside matrix only
		for (int i = 1; i < expanDialSticks.NbRows - 1; i++)
		{
			for (int j = 1; j < expanDialSticks.NbColumns - 1; j++)
			{
				candidates.Add(new Vector2Int(i, j));
			}
		}
		// Generate trials
		overlays = new List<ExpanDialSticks.SafetyOverlayMode> { ExpanDialSticks.SafetyOverlayMode.Hull, ExpanDialSticks.SafetyOverlayMode.Zone, ExpanDialSticks.SafetyOverlayMode.Edge, ExpanDialSticks.SafetyOverlayMode.Fill};
		currOverlay = ExpanDialSticks.SafetyOverlayMode.None;
		difficulties = new List<Difficulty> { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };
		trials = new Dictionary<Difficulty, List<List<int>>>();
		nbTrials = 0;
		foreach (Difficulty difficulty in difficulties)
		{
			List<List<int>> changes = new List<List<int>>();
			switch (difficulty)
			{
				case Difficulty.Easy:
					for(int i = 0; i < overlays.Count(); i++)
					{
						List<List<int>> overlayChanges = new List<List<int>>();
						overlayChanges.Add(new List<int> { (int)overlays[i], 40, 27, 13 }); ;
						overlayChanges.Add(new List<int> { (int)overlays[i], 27, 13, -13 });
						overlayChanges.Add(new List<int> { (int)overlays[i], 13, -13, -27 });
						overlayChanges.Add(new List<int> { (int)overlays[i], -13, -27, -40 });
						ListExtension.Shuffle(overlayChanges);
						changes.AddRange(overlayChanges);
						nbTrials += 4;
					}
					break;
				case Difficulty.Medium:
					for (int i = 0; i < overlays.Count(); i++)
					{
						List<List<int>> overlayChanges = new List<List<int>>();
						changes.Add(new List<int> { (int)overlays[i], 40, 32, 24, 16, 8 });
						changes.Add(new List<int> { (int)overlays[i], 24, 16, 8, -8, -16 });
						changes.Add(new List<int> { (int)overlays[i], 16, 8, -8, -16, -24 });
						changes.Add(new List<int> { (int)overlays[i], -8, -16, -24, -32, -40 });
						ListExtension.Shuffle(overlayChanges);
						changes.AddRange(overlayChanges);
						nbTrials += 4;
					}
					break;
				case Difficulty.Hard:
					for (int i = 0; i < overlays.Count(); i++)
					{
						List<List<int>> overlayChanges = new List<List<int>>();
						changes.Add(new List<int> { (int)overlays[i], 40, 37, 34, 32, 29, 26, 24 });
						changes.Add(new List<int> { (int)overlays[i], 10, 8, 5, 3, -3, -5, -8 });
						changes.Add(new List<int> { (int)overlays[i], 8, 5, 3, -3, -5, -8, -10 });
						changes.Add(new List<int> { (int)overlays[i], -24, -26, -29, -32, -34, -37, -40 });
						ListExtension.Shuffle(overlayChanges);
						changes.AddRange(overlayChanges);
						nbTrials += 4;
					}
					break;
			}
			trials.Add(difficulty, changes);
		}
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
		Vector3Int currOrderedCandidate = new Vector3Int(e.i, e.j, 0);
		if (!overlayAppeared)
		{
			if (currOrderedCandidate.x == target.x && currOrderedCandidate.y == target.y)
			{
				Debug.Log("TargetRotated!");
				string targetCandidateMsg = "USER_TARGET_ROTATION " + target;
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(targetCandidateMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				TriggerOverlay(); 
				target = new Vector2Int(-1, -1); 
				
				
			}
		} else
		{
			if(currOrderedCandidate != prevOrderedCandidate)
			{
				Vector3Int foundCandidate = diffCandidates.Find(diffCandidate => diffCandidate.x == e.i && diffCandidate.y == e.j);
				if (foundCandidate != Vector3Int.zero)
				{
					Debug.Log("Right Candidate Rotated => " + currOrderedCandidate);
					string rightCandidateMsg = "USER_RIGHT_ROTATION " + foundCandidate;
					expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(rightCandidateMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
					diffCandidates.Remove(foundCandidate);
					UpdateOverlay();
					if (diffCandidates.Count() == 0)
					{
						toNextTrial = true;
					}

				}
				else
				{
					Debug.Log("Wrong Candidate Rotated => " + currOrderedCandidate);
					string wrongCandidateMsg = "USER_WRONG_ROTATION " + currOrderedCandidate;
					expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(wrongCandidateMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

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

		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_STOP), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
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
		// Make a text field that modifies stringToEdit.
		float midX = Screen.width / 2.0f;
		float midY = Screen.height / 2.0f;
		float componentHeight = 20;

		if (newOverlay)
		{
			string buttonLabel = "START " + currDifficulty.ToString().ToUpper() + " | " + currOverlay.ToString().ToUpper();
			if (GUI.Button(new Rect(midX - 75, midY, 150, componentHeight), buttonLabel))
			{
				newOverlay = false;
				toNextTrial = true;
				landscapeGenerated = false;
			}
		}

		if (unknownParticipant)
		{
			stringParticipant = GUI.TextField(new Rect(midX - 55, midY, 50, componentHeight), stringParticipant, 25);

			if (GUI.Button(new Rect(midX + 5, midY, 150, componentHeight), "START"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				int overlaySplitIndex = numeroParticipant % overlays.Count();
				List<ExpanDialSticks.SafetyOverlayMode> nextOverlays = overlays.GetRange(0, overlaySplitIndex);
				List<ExpanDialSticks.SafetyOverlayMode> prevOverlays = overlays.GetRange(overlaySplitIndex, overlays.Count() - overlaySplitIndex);

				overlays = new List<ExpanDialSticks.SafetyOverlayMode>();
				overlays.AddRange(prevOverlays);
				overlays.AddRange(nextOverlays);
				Debug.Log(overlays.ToArrayString());
				unknownParticipant = false;
			}
				/*if (GUI.Button(new Rect(midX + 5, midY - 50, 150, componentHeight), "Training Overlay"))
				{
					training = true;
					numeroParticipant = int.Parse(stringParticipant);
					Debug.Log("TRAINING");
					string identity = "USER_IDENTITY " + numeroParticipant + " TRAINING";
					expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
					unknownParticipant = false;
				}

				if (GUI.Button(new Rect(midX + 5, midY - 25, 150, componentHeight), "Edge Overlay"))
				{

					expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.SafetyRatedMonitoredStop);
					training = false;
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
					training = false;
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
					training = false;
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
					training = false;
					numeroParticipant = int.Parse(stringParticipant);
					Debug.Log("Start");

					currOverlay = ExpanDialSticks.SafetyOverlayMode.Zone;
					expanDialSticks.SetOverlayMode(currOverlay);
					expanDialSticks.triggerSafetyChange();

					string identity = "USER_IDENTITY " + numeroParticipant + " ZONE_OVERLAY";
					expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
					unknownParticipant = false;
				}*/
			}

	}

	private void DebugInSitu(string message, Color textColor, Color backgroundColor)
	{
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 16, textColor, message, new Vector3(90f, -90f, 0f));
		expanDialSticks.setBorderBackground(backgroundColor);
		expanDialSticks.triggerTextureChange();
	}
	private List<Vector2Int> FindAllCandidatesAroundTarget(Vector2Int target)
	{

		List<Vector2Int> candidatesAround = new List<Vector2Int>();
		for (int i = Math.Max(0, target.x - 1); i < Math.Min(expanDialSticks.NbRows - 1, target.x + 1); i++)
		{
			for (int j = Math.Max(0, target.y - 1); j < Math.Min(expanDialSticks.NbColumns - 1, target.y+1); j++)
			{
				candidatesAround.Add(new Vector2Int(i, j));
			}
		}
		return candidatesAround;
	}
	private void TriggerOverlay()
	{
		List<Vector2Int> unsafeCandidates = FindAllCandidatesAroundTarget(target);//FindAllUnsafesUnderDistance(safetyDistance);
		if (currSubChanges.Count() <= unsafeCandidates.Count())
		{
			ListExtension.Shuffle(unsafeCandidates);
			diffCandidates = new List<Vector3Int>();
			for(int i = 0; i < currSubChanges.Count(); i++)
				diffCandidates.Add(new Vector3Int(unsafeCandidates[i].x, unsafeCandidates[i].y, currSubChanges[i]));

			// Output Control
			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					expanDialSticks.modelMatrix[i, j].CurrentFeedForwarded = 0;
				}
			}

			foreach (Vector3Int diffCandidate in diffCandidates)
			{

				expanDialSticks.modelMatrix[diffCandidate.x, diffCandidate.y].CurrentFeedForwarded = diffCandidate.z;
			}

			expanDialSticks.triggerSafetyChange();
			leftHand.Freeze();
			rightHand.Freeze();
			DisplayInstructions("Tourner les cylindres arrêtés <b>du plus descendant au plus ascendant</b>.");

			string shapeChangeMsg = "SHAPE_CHANGE [";
			foreach (Vector3Int diffCandidate in diffCandidates)
			{
				shapeChangeMsg += " "+ diffCandidate.ToString() + " ";
			}
			shapeChangeMsg += "]";
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(shapeChangeMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

			LogMetrics();
			overlayAppeared = true;
		} else
		{
			DisplayInstructions("Veuillez réessayer.");
		}
	}
	private void ResetDisplay()
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				//Projector
				expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "default";
				expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = 90f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 2f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = 0.1f;
				//Texture
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = 0.1f;
				//Shape
				//expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
				//expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = shapeChangeDuration;
			}
		}
		expanDialSticks.triggerProjectorChange();
		expanDialSticks.triggerTextureChange();
		//expanDialSticks.triggerShapeChange();

	}
	private void UpdateOverlay()
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].CurrentFeedForwarded = 0;
			}
		}

		foreach (Vector3Int diffCandidate in diffCandidates)
		{

			expanDialSticks.modelMatrix[diffCandidate.x, diffCandidate.y].CurrentFeedForwarded = diffCandidate.z;
		}

		expanDialSticks.triggerSafetyChange();
	}

	private void DisplayInstructions(string instructions)
	{
		string participantNumber = "P" + numeroParticipant;
		string trialProgress =  currTrial + "/" + nbTrials;

		expanDialSticks.setBorderBackground(Color.white);
		expanDialSticks.setLeftCornerText(TextAlignmentOptions.Center, 12, Color.black, participantNumber, new Vector3(90f, -90f, 0f));
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 12, Color.black, instructions, new Vector3(90f, -90f, 0f));
		expanDialSticks.setRightCornerText(TextAlignmentOptions.Center, 12, Color.black, trialProgress, new Vector3(90f, -90f, 0f));

		//expanDialSticks.triggerTextureChange();
	}

	private void TriggerNextTrial()
	{
		if (!landscapeGenerated)
		{

			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					expanDialSticks.modelMatrix[i, j].TargetPosition = (sbyte)Random.Range(minPos, maxPos);
					expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = shapeChangeDuration;
				}
			}
			landscapeGenerated = true;
		}
		if (trials.Count() > 0)
		{
			// Unfreeze hand tracking
			leftHand.Unfreeze();
			rightHand.Unfreeze();
			// Extract difficulty and changes
			KeyValuePair<Difficulty, List<List<int>>> difficultyChanges = trials.First();
			currDifficulty = difficultyChanges.Key;
			Debug.Log("currDifficulty -> " + currDifficulty);
			List<List<int>> changes = difficultyChanges.Value;
			currSubChanges = changes.First();
			ExpanDialSticks.SafetyOverlayMode nextOverlay = (ExpanDialSticks.SafetyOverlayMode)currSubChanges.First();
			// new overlay detected
			if(currOverlay != nextOverlay)
			{
				currOverlay = nextOverlay;
				expanDialSticks.SetOverlayMode(currOverlay);
				ResetDisplay();
				newOverlay = true;
				toNextTrial = false;
				return;
			}
			// current trial;
			currTrial++;
			changes.RemoveAt(0);
			currSubChanges.RemoveAt(0);
			ListExtension.Shuffle(currSubChanges);
			Debug.Log("currSubChanges -> " + currSubChanges);

			// delete difficulty or if any reinsert remaining changes
			if (changes.Count() > 0)
			{
				trials[currDifficulty] = changes;
			} else
			{
				trials.Remove(currDifficulty);
			}
			// get a target for user
			target = candidates[Random.Range(0, candidates.Count())];
			// Output Control
			List<int> randomIcons = new List<int>(); //= Enumerable.Range(0, nbPins).ToList<int>();
			for (int i = 1; i < 30; i++)
				randomIcons.Add(i);
			ListExtension.Shuffle(randomIcons);

			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					if(target.x == i && target.y == j)
					{
						expanDialSticks.modelMatrix[i, j].TargetProjectorTexture ="icon0";
					} else
					{
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

			DisplayInstructions("Tourner le cylindre avec l'icône <b>Avion</b>");

			string trialMsg = "PARTICIPANT " + numeroParticipant + " DIFFICULTY " + currDifficulty + " OVERLAY " + currOverlay + " TARGET (" + target.x + ", " + target.y + ") ";
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(trialMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

			toNextTrial = false;
			overlayAppeared = false;
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
			}
			if (!newOverlay)
			{
				// random texture every 3 secondes
				if (currTime - prevRandomTextureTime >= 3f)
				{
					//Debug.Log("RandomTexture!");
					RandomColor();
					prevRandomTextureTime = currTime;
				}

				/*if (currTime - prevMetricsTime >= LOG_INTERVAL)
				{
					LogMetrics();
					prevMetricsTime = currTime;
				}*/

				if (Input.GetKey("escape"))
				{
					Quit();
				}

				if (Input.GetKeyDown(KeyCode.RightArrow))
				{
					HandleRotationChanged(this, new ExpanDialStickEventArgs(DateTime.Now, target.x, target.y, 0, 10, 10));
				}
				if (Input.GetKeyDown(KeyCode.UpArrow))
				{
					if (diffCandidates.Count() > 0)
					{
						Vector3Int selectedCandidate = diffCandidates.First();
						HandleRotationChanged(this, new ExpanDialStickEventArgs(DateTime.Now, selectedCandidate.x, selectedCandidate.y, 0, 10, 10));
					}
				}
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
