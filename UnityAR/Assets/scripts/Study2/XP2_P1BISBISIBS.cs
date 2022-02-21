
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

public class XP2_P1BISBISBIS : MonoBehaviour
{

	// ExpanDialSticks Core
	public GameObject expanDialSticksPrefab;
	public GameObject capsuleHandLeftPrefab;
	public GameObject capsuleHandRightPrefab;
	public GameObject safeGuardPrefab;
	private MyCapsuleHand leftHand;
	private MyCapsuleHand rightHand;
	private SafeGuard safeGuard;
	private ExpanDialSticks expanDialSticks;
	private bool connected;

	private const float LOG_INTERVAL = 0.2f; // 0.2f;
	private const float COMPLETION_INTERVAL = 0.5f; // 0.2f;
	private float currTime;
	private float prevRandomTextureTime;
	private float prevMetricsTime;
	private float prevDistractorTime;
	private float distractorTriggerDelay = 2f;

	private const int minPos = 0;
	private const int maxPos = 20;
	private const int targetPos = 30;
	private const float shortShapeChangeDuration = 6f;
	private const float longShapeChangeDuration = 6f;
	private float shapeChangeDuration = 2f;
	private const float safetyDistance = 6f;


	private bool newOverlay = false;
	private bool landscapeGenerated = false;
	private string stringParticipant = "";
	private int numeroParticipant = 0;
	private bool unknownParticipant = true;
	private bool overlayAppeared = false;
	private bool training = false;
	private bool triggerNextTrial = true;
	private bool waitForNoHand = false;
	private bool autoDistractorTrigger = false;

	public const string MQTT_SYSTEM_RECORDER = "SYSTEM_RECORDER";
	public const string CMD_START = "START";
	public const string CMD_STOP = "STOP";


	private List<ExpanDialSticks.SafetyOverlayMode> overlays;
	private ExpanDialSticks.SafetyOverlayMode currOverlay = ExpanDialSticks.SafetyOverlayMode.User;

	private Vector2Int LEFT = new Vector2Int(-1, 0);
	private Vector2Int LEFT_UP = new Vector2Int(-1, -1);
	private Vector2Int UP = new Vector2Int(0, -1);
	private Vector2Int RIGHT_UP = new Vector2Int(+1, -1);
	private Vector2Int RIGHT = new Vector2Int(+1, 0);
	private Vector2Int RIGHT_DOWN = new Vector2Int(+1, +1);
	private Vector2Int DOWN = new Vector2Int(0, +1);
	private Vector2Int LEFT_DOWN = new Vector2Int(-1, +1);

	private List<List<Vector2Int>> distractorPositions = new List<List<Vector2Int>>();

	//public enum DistractorPosition { LEFT, UP, RIGHT, DOWN, UP_LEFT, UP_RIGHT, DOWN_RIGHT, DOWN_LEFT };
	private List<Vector2Int> currDistractorPositions;
	private int currDistractorIndex;
	private List<Vector2Int> targetPositions;
	private Vector2Int currTargetPosition;
	private int currTargetIndex;

	public enum TaskMode { System, User };
	private List<TaskMode> tasks;
	private TaskMode currTask = TaskMode.User;
	private Vector3 currTarget;
	private List<Vector2Int> candidates;

	private Vector2Int prevSelectPosition;
	private Vector2Int currSelectPosition;


	private const int nbTrials = 3;

	void Start()
	{
		leftHand = capsuleHandLeftPrefab.GetComponent<MyCapsuleHand>();
		rightHand = capsuleHandRightPrefab.GetComponent<MyCapsuleHand>();
		expanDialSticks = expanDialSticksPrefab.GetComponent<ExpanDialSticks>();
		safeGuard = safeGuardPrefab.GetComponent<SafeGuard>();
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

		candidates = new List<Vector2Int>(); //= Enumerable.Range(0, nbPins).ToList<int>();
											 // get pins inside matrix only
		for (int i = 1; i < expanDialSticks.NbRows - 1; i++)
		{
			for (int j = 1; j < expanDialSticks.NbColumns - 1; j++)
			{
				candidates.Add(new Vector2Int(i, j));
			}
		}
		distractorPositions = new List<List<Vector2Int>>();

		connected = false;
		training = false;
		currOverlay = ExpanDialSticks.SafetyOverlayMode.User;
		currTask = TaskMode.User;

		currDistractorIndex = -1;
		currDistractorPositions = new List<Vector2Int>();
		currTargetIndex = -1;
		currTargetPosition = new Vector2Int(-1, -1);
		targetPositions = new List<Vector2Int>();

		prevSelectPosition = currSelectPosition = new Vector2Int(-1, -1);

		GenerateTrials();
		triggerNextTrial = true;

		expanDialSticks.client_MqttConnect();

	}
	public void GenerateTrials()
	{
		distractorPositions.Add(new List<Vector2Int> { LEFT, UP, RIGHT });
		distractorPositions.Add(new List<Vector2Int> { UP, RIGHT, DOWN });
		distractorPositions.Add(new List<Vector2Int> { RIGHT, DOWN, LEFT });
		distractorPositions.Add(new List<Vector2Int> { DOWN, LEFT, UP });

		distractorPositions.Add(new List<Vector2Int> { LEFT_UP, RIGHT_UP, RIGHT_DOWN });
		distractorPositions.Add(new List<Vector2Int> { RIGHT_UP, RIGHT_DOWN, LEFT_DOWN });
		distractorPositions.Add(new List<Vector2Int> { RIGHT_DOWN, LEFT_DOWN, LEFT_UP });
		distractorPositions.Add(new List<Vector2Int> { LEFT_DOWN, LEFT_UP, RIGHT_UP });

		distractorPositions.Add(new List<Vector2Int> { LEFT_DOWN, LEFT_UP, RIGHT });
		distractorPositions.Add(new List<Vector2Int> { LEFT_UP, RIGHT_UP, DOWN });
		distractorPositions.Add(new List<Vector2Int> { RIGHT_UP, RIGHT_DOWN, LEFT });
		distractorPositions.Add(new List<Vector2Int> { RIGHT_DOWN, LEFT_DOWN, UP });

		ListExtension.Shuffle(distractorPositions);

		for (int i = 0; i < distractorPositions.Count(); i++)
		{
			targetPositions.Add(candidates[i % candidates.Count()]);
		}
		ListExtension.Shuffle(targetPositions);
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
		currSelectPosition = new Vector2Int(e.i, e.j);

		if (currSelectPosition != prevSelectPosition)
		{
			if (currTargetPosition == currSelectPosition)
			{
				StartCoroutine(TriggerDistractors());
			}
		}
		else
		{

		}
		prevSelectPosition = currSelectPosition;
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
		// Make a text field that modifies stringToEdit.
		float midX = Screen.width / 2.0f;
		float midY = Screen.height / 2.0f;
		float componentHeight = 20;
		float componentWidth = 250;

		if (unknownParticipant)
		{
			stringParticipant = GUI.TextField(new Rect(midX - 55, midY, 50, componentHeight), stringParticipant, 25);


			if (GUI.Button(new Rect(midX + 5, midY - 50, componentWidth, componentHeight), "USER Overlay | USER Task"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currTask = TaskMode.User;
				shapeChangeDuration = shortShapeChangeDuration;
				currOverlay = ExpanDialSticks.SafetyOverlayMode.User;
				expanDialSticks.SetOverlayMode(currOverlay);
				string identity = "USER_IDENTITY " + numeroParticipant + " | OVERLAY USER | TASK USER";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY - 25, componentWidth, componentHeight), "USER Overlay | SYSTEM Task"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlay = ExpanDialSticks.SafetyOverlayMode.User;
				currTask = TaskMode.System;
				shapeChangeDuration = longShapeChangeDuration;
				expanDialSticks.SetOverlayMode(currOverlay);
				string identity = "USER_IDENTITY " + numeroParticipant + " | OVERLAY USER | TASK SYSTEM";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY, componentWidth, componentHeight), "SYSTEM Overlay | USER Task"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlay = ExpanDialSticks.SafetyOverlayMode.System;
				currTask = TaskMode.User;
				shapeChangeDuration = shortShapeChangeDuration;
				expanDialSticks.SetOverlayMode(currOverlay);
				string identity = "USER_IDENTITY " + numeroParticipant + " | OVERLAY SYSTEM | TASK USER";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				unknownParticipant = false;
			}
			if (GUI.Button(new Rect(midX + 5, midY + 25, componentWidth, componentHeight), "SYSTEM Overlay | SYSTEM Task"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlay = ExpanDialSticks.SafetyOverlayMode.System;
				currTask = TaskMode.System;
				shapeChangeDuration = longShapeChangeDuration;
				expanDialSticks.SetOverlayMode(currOverlay);
				string identity = "USER_IDENTITY " + numeroParticipant + " | OVERLAY SYSTEM | TASK SYSTEM";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				unknownParticipant = false;
			}
		}

	}

	private void DebugInSitu(string message, Color textColor, Color backgroundColor)
	{
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 0.1f, textColor, message, new Vector3(90f, -90f, 0f));
		expanDialSticks.setBorderBackground(backgroundColor);
		expanDialSticks.triggerTextureChange();
	}
	private List<Vector2Int> FindAllCandidatesAroundTarget(Vector2Int target)
	{

		List<Vector2Int> candidatesAround = new List<Vector2Int>();
		for (int i = Math.Max(0, target.x - 1); i < Math.Min(expanDialSticks.NbRows - 1, target.x + 1); i++)
		{
			for (int j = Math.Max(0, target.y - 1); j < Math.Min(expanDialSticks.NbColumns - 1, target.y + 1); j++)
			{
				candidatesAround.Add(new Vector2Int(i, j));
			}
		}
		return candidatesAround;
	}
	private void TriggerOverlay()
	{

	}
	private void ResetDisplay()
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				//Projector
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontTexture = "default";
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontRotation = 90f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontSize = expanDialSticks.modelMatrix[i, j].Diameter / 3f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontChangeDuration = 0.1f;
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

	}

	private void DisplayInstructions(string instructions)
	{
		string participantNumber = "P" + numeroParticipant;
		//string trialProgress = currTrial + "/" + nbTrials;

		expanDialSticks.setBorderBackground(Color.white);
		expanDialSticks.setLeftCornerText(TextAlignmentOptions.Center, 12, Color.black, participantNumber, new Vector3(90f, -90f, 0f));
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 12, Color.black, instructions, new Vector3(90f, -90f, 0f));
		//expanDialSticks.setRightCornerText(TextAlignmentOptions.Center, 12, Color.black, trialProgress, new Vector3(90f, -90f, 0f));

		//expanDialSticks.triggerTextureChange();
	}

	private IEnumerator TriggerDistractors()
	{

		Debug.Log("TriggerDistractors()");
		currDistractorIndex++;
		if (currDistractorIndex < distractorPositions.Count())
		{
			currDistractorPositions = distractorPositions[currDistractorIndex];
			//Debug.Log(currDistractorPositions.ToArrayString<Vector2Int>());

			foreach (Vector2Int distractorDelta in currDistractorPositions)
			{
				Vector2Int distractorPosition = currTargetPosition + distractorDelta;
				expanDialSticks.modelMatrix[distractorPosition.x, distractorPosition.y].TargetPosition = (sbyte)targetPos;
				expanDialSticks.modelMatrix[distractorPosition.x, distractorPosition.y].TargetShapeChangeDuration = shapeChangeDuration;
			}

			expanDialSticks.triggerShapeChange();

			Debug.Log("triggerShapeChange()");
			// wait for complete shape-change
			yield return new WaitForSeconds(shapeChangeDuration);
			bool shapeChangeCompleted = false;
			int count = 0;
			while (!shapeChangeCompleted)
			{
				DebugInSitu("Waiting for shape-change to complete(" + count + ")...", Color.black, Color.white);
				shapeChangeCompleted = true;
				foreach (Vector2Int distractorDelta in currDistractorPositions)
				{
					Vector2Int distractorPosition = currTargetPosition + distractorDelta;
					sbyte currDistractorPos = expanDialSticks.modelMatrix[distractorPosition.x, distractorPosition.y].CurrentPosition;
					Debug.Log(distractorPosition + " => " + currDistractorPos);
					if (currDistractorPos < targetPos - 1 || currDistractorPos > targetPos + 1) // cannot get exact position, add tolerance (problem with Arduino Driver)
					{
						shapeChangeCompleted = false;
					}
				}
				count++;
				yield return new WaitForSeconds(COMPLETION_INTERVAL);
			}
			// reset shape-change
			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					expanDialSticks.modelMatrix[i, j].TargetPosition = 0; // (sbyte)Random.Range(minPos, maxPos);
					expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = shortShapeChangeDuration;
					expanDialSticks.modelMatrix[i, j].TargetColor = Color.black;
					expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = shortShapeChangeDuration;
				}
			}
			expanDialSticks.triggerShapeChange();
			expanDialSticks.triggerTextureChange();
			yield return new WaitForSeconds(shortShapeChangeDuration);
			// trigger next trial
			triggerNextTrial = true;

		}
		else
		{
			Quit();
		}
	}
	private void TriggerShapeChange()
	{

	}
	private void TriggerTarget()
	{
		Debug.Log("TriggerTarget()");
		currTargetIndex++;
		if (currTargetIndex < targetPositions.Count())
		{
			// Select Target
			currTargetPosition = targetPositions[currTargetIndex];

			Debug.Log("currTargetPosition=>" + currTargetPosition);
			// Random Shape Change + Target
			// Random Shape
			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					// Shape
					if (i == currTargetPosition.x && j == currTargetPosition.y)
					{
						expanDialSticks.modelMatrix[i, j].TargetPosition = (sbyte)targetPos;
					}
					else
					{
						expanDialSticks.modelMatrix[i, j].TargetPosition = 0; // (sbyte)Random.Range(minPos, maxPos);
					}
					expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = shortShapeChangeDuration;

					Color randomColor = Random.ColorHSV(0f, 1f, 0f, 1f, 0.5f, 1f);
					expanDialSticks.modelMatrix[i, j].TargetColor = randomColor;
					expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = shortShapeChangeDuration;
				}
			}
			expanDialSticks.triggerShapeChange();
			expanDialSticks.triggerTextureChange();
			Debug.Log("triggerShapeChange()");

			switch (currTask)
			{
				case TaskMode.User:
					Debug.Log("USER TASK");
					break;


				case TaskMode.System:
					Debug.Log("SYSTEM TASK");
					autoDistractorTrigger = true;
					prevDistractorTime = Time.time;
					distractorTriggerDelay = Random.Range(shapeChangeDuration + shapeChangeDuration, shapeChangeDuration + longShapeChangeDuration);
					break;
				default:
					break;
			}

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

			if (triggerNextTrial == true)
			{
				TriggerTarget();
				triggerNextTrial = false;
			}
			/*if (currTime - prevMetricsTime >= LOG_INTERVAL)
			{
				if (waitForNoHand)
				{
					if (FindAllUnsafes().Count() == 0)
					{
						waitForNoHand = false;
						triggerNextTrial = true;
					}

				}
				prevMetricsTime = currTime;
			}*/

			if (autoDistractorTrigger && currTime - prevDistractorTime >= distractorTriggerDelay)
			{
				Debug.Log("Trigger distractor");
				HandleRotationChanged(this, new ExpanDialStickEventArgs(DateTime.Now, currTargetPosition.x, currTargetPosition.y, 0, 10, 10));
				autoDistractorTrigger = false;
			}

			if (Input.GetKey("escape"))
			{
				Quit();
			}

			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				HandleRotationChanged(this, new ExpanDialStickEventArgs(DateTime.Now, currTargetPosition.x, currTargetPosition.y, 0, 10, 10));
			}
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				/*if (diffCandidates.Count() > 0)
				{
					Vector3Int selectedCandidate = diffCandidates.First();
					HandleRotationChanged(this, new ExpanDialStickEventArgs(DateTime.Now, selectedCandidate.x, selectedCandidate.y, 0, 10, 10));
				}*/
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