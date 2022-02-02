
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
	public GameObject safeGuardPrefab;
	private MyCapsuleHand leftHand;
	private MyCapsuleHand rightHand;
	private SafeGuard safeGuard;
	private ExpanDialSticks expanDialSticks;
	private bool connected;

	private const float LOG_INTERVAL = 0.2f; // 0.2f;
	private const float COMPLETION_INTERVAL = 0.5f; // 0.2f;
	private float currTime;
	private float prevTime;
	private float prevRandomTextureTime;
	private float prevMetricsTime;
	private float prevDistractorTime;
	private float distractorTriggerDelay = 2f;

	private const int resetPos = 0;
	private const int minPos = 0;
	private const int maxPos = 20;
	private const int targetPos = 30;
	private const float shortShapeChangeDuration = 2f;
	private const float longShapeChangeDuration = 4f;
	private float shapeChangeWaitFor = 3f;
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
	private bool MetricsActive = false;
	private bool targetAroundIsSelected = false;

	// GAUGE GAME VARIABLES
	private Vector2[] gaugePositions;

	private int gaugeIndex;
	private const int GAUGE_TO_APPEAR = 0;
	private const int GAUGE_APPEARING = 1;
	private const int GAUGE_APPEARED = 2;
	private const int GAUGE_STARTED = 3;
	private const int LANDSCAPE_IS_CHANGING = 4;

	private int gaugeState = GAUGE_TO_APPEAR;
	private const sbyte gaugeHeight = 20;
	private float aiguilleRotation = 90f;
	private float cadranRotation = 90f;
	private float speedRotation = 1f;

	private float directionTime = 0f;
	private float directionDuration = 3f;
	private float startGameTime = 0f;

	private const float initGameDuration = 20f;
	private float gameDuration = Mathf.Infinity;
	private float motionDuration = 10f;

	private const float anglePerStep = 360f / 24f;
	private float startRotation = 90f - anglePerStep;
	public enum DirectionRotation { CW, CCW, IDDLE };
	private DirectionRotation directionRotation = DirectionRotation.CW;

	// MQTT VARIABLES
	public const string MQTT_SYSTEM_RECORDER = "SYSTEM_RECORDER";
	public const string CMD_START = "START";
	public const string CMD_STOP = "STOP";

	private List<ExpanDialSticks.SafetyOverlayMode> overlayModes;
	private ExpanDialSticks.SafetyOverlayMode currOverlayMode = ExpanDialSticks.SafetyOverlayMode.User;

	private Vector2Int TOP_LEFT_CORNER = new Vector2Int(1, 1);
	private Vector2Int TOP_RIGHT_CORNER = new Vector2Int(1, 4);
	private Vector2Int BOT_LEFT_CORNER = new Vector2Int(3, 1);
	private Vector2Int BOT_RIGHT_CORNER = new Vector2Int(3, 4);

	private Vector2Int LEFT = new Vector2Int(-1, 0);
	private Vector2Int LEFT_UP = new Vector2Int(-1, -1);
	private Vector2Int UP = new Vector2Int(0, -1);
	private Vector2Int RIGHT_UP = new Vector2Int(+1, -1);
	private Vector2Int RIGHT = new Vector2Int(+1, 0);
	private Vector2Int RIGHT_DOWN = new Vector2Int(+1, +1);
	private Vector2Int DOWN = new Vector2Int(0, +1);
	private Vector2Int LEFT_DOWN = new Vector2Int(-1, +1);
	private Vector2Int CENTER = new Vector2Int(0, 0);
	private List<Vector2Int> DELTAS = new List<Vector2Int>();



	private List<List<Vector2Int>> distractors = new List<List<Vector2Int>>();
	private int currDistractorIndex;
	private List<Vector2Int> currDistractor;

	public enum TaskMode { SC_UNDER, SC_BETWEEN, SC_AROUND };
	private TaskMode currTaskMode;

	private List<Vector2Int> candidates = new List<Vector2Int>();

	private List<Vector2Int> targets = new List<Vector2Int>();
	private int currTargetIndex;
	private Vector2Int currTarget;

	private Vector2Int prevSelectPosition;
	private Vector2Int currSelectPosition;

	private int currTrial = 0;
	public int nbTrials = 5;

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
		expanDialSticks.OnRotationChanged += HandleRotationChanged;

		currOverlayMode = ExpanDialSticks.SafetyOverlayMode.User;
		currTaskMode = TaskMode.SC_AROUND;
		candidates = new List<Vector2Int>();
		targets = new List<Vector2Int>();
		currTargetIndex = -1;
		currTarget = new Vector2Int(1, 1);
		distractors = new List< List < Vector2Int >> ();
		currDistractorIndex = -1;
		currDistractor = new List<Vector2Int>();
		prevSelectPosition = currSelectPosition = new Vector2Int(-1, -1);
		DELTAS = new List<Vector2Int>() {LEFT, LEFT_UP, UP, RIGHT_UP, RIGHT, RIGHT_DOWN, DOWN, LEFT_DOWN};
		GenerateTrials();
		MetricsActive = false;
		targetAroundIsSelected = false;
		triggerNextTrial = true;
		connected = false;
		/*for(float i = 0; i < 1.0f; i += 0.1f)
		{
			Debug.Log("Exponential(" + i + ") = > " + Transition.Exponential(i));
		}*/
		expanDialSticks.client_MqttConnect();

	}
	public void GenerateTrials()
	{
		for (int i = 1; i < 2; i++)
		{
			for (int j = 1; j < expanDialSticks.NbColumns - 1; j++)
			{
				candidates.Add(new Vector2Int(i, j));
			}
		}

		/*for(int i = 0; i < nbPins.Length; i++)
		{
			int nbPin = nbPins[i];
			for (int j = 0; j < nbTrials; j++)
			{
				ListExtension.Shuffle(DELTAS);
				distractors.Add(DELTAS.GetRange(0, nbPin));
			}
		}*/

		distractors.Add(new List<Vector2Int> { UP, RIGHT, DOWN });
		distractors.Add(new List<Vector2Int> { RIGHT, DOWN, LEFT });
		distractors.Add(new List<Vector2Int> { DOWN, LEFT, UP });

		distractors.Add(new List<Vector2Int> { LEFT_UP, RIGHT_UP, RIGHT_DOWN });
		distractors.Add(new List<Vector2Int> { RIGHT_UP, RIGHT_DOWN, LEFT_DOWN });
		distractors.Add(new List<Vector2Int> { RIGHT_DOWN, LEFT_DOWN, LEFT_UP });
		distractors.Add(new List<Vector2Int> { LEFT_DOWN, LEFT_UP, RIGHT_UP });

		distractors.Add(new List<Vector2Int> { LEFT_DOWN, LEFT_UP, RIGHT });
		distractors.Add(new List<Vector2Int> { LEFT_UP, RIGHT_UP, DOWN });
		distractors.Add(new List<Vector2Int> { RIGHT_UP, RIGHT_DOWN, LEFT });
		distractors.Add(new List<Vector2Int> { RIGHT_DOWN, LEFT_DOWN, UP });
		/*for (int i = 0; i < nbTrials; i++)
		{
			targets.Add(candidates[i % candidates.Count()]);
		}
		ListExtension.Shuffle(targets);*/



		ListExtension.Shuffle(distractors);


		for (int i = 0; i < distractors.Count(); i++)
		{
			//Debug.Log(i % candidates.Count());
			//Debug.Log(candidates[i % candidates.Count()]);
			targets.Add(candidates[i % candidates.Count()]);
		}
		ListExtension.Shuffle(targets);
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

	private void HandleRotationChanged(object sender, ExpanDialStickEventArgs e)
	{
		currSelectPosition = new Vector2Int(e.i, e.j);
			if(currTarget == currSelectPosition)
			{
				float prevRotation = aiguilleRotation;
				aiguilleRotation += e.diff * anglePerStep;
				string msg = "";
				msg += "USER_ROTATION " + prevRotation + " " + aiguilleRotation;
				//Debug.Log(msg);
				if (gaugeState == GAUGE_APPEARED)
				{
					startGameTime = Time.time;
					motionDuration = Random.Range(5f, initGameDuration - 5f);
					gameDuration = initGameDuration;
					gaugeState = GAUGE_STARTED;
				}
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

			switch (currTaskMode)
				{
					case TaskMode.SC_AROUND:
						
					break;
					case TaskMode.SC_BETWEEN:
					break;
					case TaskMode.SC_UNDER:
					break;
					default:
					break;
				}
				
			}
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


			if (GUI.Button(new Rect(midX + 5, midY - 50, componentWidth, componentHeight), "USER Overlay | SC Around"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.User;
				currTaskMode = TaskMode.SC_AROUND;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | USER OVERLAY | SC AROUND";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				unknownParticipant = false;
			}
			if (GUI.Button(new Rect(midX + 5, midY - 25, componentWidth, componentHeight), "USER Overlay | SC Under"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.User;
				currTaskMode = TaskMode.SC_UNDER;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | SYSTEM OVERLAY | SC UNDER";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				unknownParticipant = false;
			}
			if (GUI.Button(new Rect(midX + 5, midY, componentWidth, componentHeight), "USER Overlay | SC Between"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.User;
				currTaskMode = TaskMode.SC_BETWEEN;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | SYSTEM OVERLAY | SC BETWEEN";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				unknownParticipant = false;
			}

			/*if (GUI.Button(new Rect(midX + 5, midY, componentWidth, componentHeight), "SYSTEM Overlay | SC Around"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.System;
				currTaskMode = TaskMode.SC_AROUND;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | SYSTEM OVERLAY | SC AROUND";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				unknownParticipant = false;
			}*/

			/*if (GUI.Button(new Rect(midX + 5, midY - 50, componentWidth, componentHeight), "SYSTEM Overlay | SC Between"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.System;
				currTaskMode = TaskMode.SC_BETWEEN;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | SYSTEM OVERLAY | SC BETWEEN";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				unknownParticipant = false;
			}*/
			/*if (GUI.Button(new Rect(midX + 5, midY + 25, componentWidth, componentHeight), "SYSTEM Overlay | SC Under"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.System;
				currTaskMode = TaskMode.SC_UNDER;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | SYSTEM OVERLAY | SC UNDER";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				unknownParticipant = false;
			}*/


		}

	}

	private void DebugInSitu(string message, Color textColor, Color backgroundColor)
	{
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 0.1f, textColor, message, new Vector3(90f, -90f, 0f));
		expanDialSticks.setBorderBackground(backgroundColor);
		expanDialSticks.triggerTextureChange();
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

	private void PrepareResetShapeChange(float shapeChangeDuration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetPosition = 0; // (sbyte)Random.Range(minPos, maxPos);
				expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = shapeChangeDuration;

				expanDialSticks.modelMatrix[i, j].TargetColor = Color.black;
				expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
				expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = 0f;
				expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = shapeChangeDuration;

				expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "projector";
				expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = shapeChangeDuration;

			}
		}
	}

	private void PrepareShapeChangeAtTarget(float shapeChangeDuration)
	{
		expanDialSticks.modelMatrix[currTarget.x, currTarget.y].TargetPosition = targetPos;
		expanDialSticks.modelMatrix[currTarget.x, currTarget.y].TargetShapeChangeDuration = shapeChangeDuration;
	}
	private void PrepareShapeChangeAroundTarget(float shapeChangeDuration)
	{
		for(int i = currTarget.x - 1; i <= currTarget.x + 1; i++)
		{
			for (int j = currTarget.y - 1; j <= currTarget.y + 1; j++)
			{
				if (currTarget.x != i || currTarget.y != j)
				{
					expanDialSticks.modelMatrix[i, j].TargetPosition = targetPos;
					expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = shapeChangeDuration;
				}
			}
		}
	}
	private IEnumerator AroundShapeChangeTask()
	{
		// Reset Shape
		targetAroundIsSelected = false;
		MetricsActive = false;

		// Reset Texture
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.black;
				expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
				expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = 0f;
				expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = 0.1f;

				expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "projector";
				expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = 0.1f;

			}
		}
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerProjectorChange();
		yield return new WaitForSeconds(0.1f);

		// Generate Target candidates
		List<Vector2Int> targetCandidates = new List<Vector2Int> {new Vector2Int(1,1), new Vector2Int(1, 4), new Vector2Int(3, 1), new Vector2Int(3, 4)};
		ListExtension.Shuffle(targetCandidates);
		// Remove prev target and unsafe ones from candidates
		targetCandidates.RemoveAll(targetCandidate => targetCandidate == currTarget || expanDialSticks.modelMatrix[targetCandidate.x, targetCandidate.y].CurrentProximity == 1f);
		
		if (targetCandidates.Count() >  0)
		{
			// Get safest target and distractors
			Vector2Int nextTarget = new Vector2Int(-1, -1);
			List<Vector2Int> nextDistractors = new List<Vector2Int>();
			int maxSafeCount = 0;
			for (int i = 0; i < targetCandidates.Count(); i++)
			{

				List<Vector2Int> distractorCandidates = new List<Vector2Int>() {
					targetCandidates[i] + LEFT,
					targetCandidates[i] + LEFT_UP,
					targetCandidates[i] + UP,
					targetCandidates[i] + RIGHT_UP,
					targetCandidates[i] + RIGHT,
					targetCandidates[i] + RIGHT_DOWN,
					targetCandidates[i] + DOWN,
					targetCandidates[i] + LEFT_DOWN
				};
				distractorCandidates.RemoveAll(distractorCandidate => expanDialSticks.modelMatrix[distractorCandidate.x, distractorCandidate.y].CurrentProximity == 1f);
				if (distractorCandidates.Count() > maxSafeCount)
				{
					maxSafeCount = distractorCandidates.Count();
					nextTarget = targetCandidates[i];
					nextDistractors = distractorCandidates;
				}
			}
			//Debug.Log("nextTarget => " + nextTarget);
			// Check for target with less occlusion
			currTarget = nextTarget;

			// Texture change target
			cadranRotation = aiguilleRotation = startRotation;
			ShowGaugeOnTarget(0.1f);
			expanDialSticks.triggerTextureChange();
			expanDialSticks.triggerProjectorChange();
			yield return new WaitForSeconds(0.1f);

			// Reset Shape except target
			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					if(nextTarget.x != i || nextTarget.y != j)
					{
						expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
						expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = longShapeChangeDuration;
					}
				}
			}

			// Shape distractors
			String shapeChangeMsg = "SYSTEM_TRIGGER_SHAPE_CHANGE";
			foreach (Vector2Int distractor in nextDistractors)
			{
				shapeChangeMsg += " " + new Vector3Int(distractor.x, distractor.y, targetPos).ToString();
				expanDialSticks.modelMatrix[distractor.x, distractor.y].TargetPosition = targetPos;
				expanDialSticks.modelMatrix[distractor.x, distractor.y].TargetShapeChangeDuration = longShapeChangeDuration;
			}
			expanDialSticks.triggerShapeChange();
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(shapeChangeMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
			MetricsActive = true;
			// Wait for Gauge Game End
			// Loop Until Start
			gaugeState = GAUGE_APPEARED;
			bool finished = false;
			// Game
			Debug.Log("Waiting for unpause motion to finish...");
			while (!finished)
			{

				if (gaugeState == GAUGE_STARTED) // User started task
				{
					// Move it
					float prevRotation = cadranRotation;
					switch (directionRotation)
					{
						case DirectionRotation.CW:
							cadranRotation += speedRotation * Time.deltaTime;
							break;
						case DirectionRotation.CCW:
							cadranRotation -= speedRotation * Time.deltaTime;
							break;
						default:
							break;
					}
					MoveAiguilleCadran(0.1f);
					expanDialSticks.triggerTextureChange();
					expanDialSticks.triggerProjectorChange();

					//string msg = "SYSTEM_ROTATION " + prevRotation + " " + cadranRotation;
					if ((int)prevRotation != (int)cadranRotation)
					{
						//Debug.Log(msg);
					}
					// wait for unpause motion to finish
					//DebugInSitu("Waiting for safe shape-change to complete(" + count + ")...", Color.black, Color.white);
					finished = true;
					for (int i = 0; i < expanDialSticks.NbRows; i++)
						for (int j = 0; j < expanDialSticks.NbColumns; j++)
							if (expanDialSticks.modelMatrix[i, j].CurrentReaching) finished = false;
				}
				yield return new WaitForSeconds(0.1f);
			}
			Debug.Log("Unpause motion finished!");
			currTrial++;
		} else
		{

			yield return new WaitForSeconds(COMPLETION_INTERVAL);
		}
		triggerNextTrial = true;
	}
	private IEnumerator UnderShapeChangeTask()
	{
		// Reset Shape
		targetAroundIsSelected = false;
		MetricsActive = false;

		// Reset Texture
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.black;
				expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
				expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = 0f;
				expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = 0.1f;

				expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "projector";
				expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = 0.1f;

			}
		}
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerProjectorChange();
		yield return new WaitForSeconds(0.1f);

		// Generate Target candidates
		List<Vector2Int> targetCandidates = new List<Vector2Int> { new Vector2Int(1, 1), new Vector2Int(1, 4), new Vector2Int(3, 1), new Vector2Int(3, 4) };
		ListExtension.Shuffle(targetCandidates);
		// Remove prev target and unsafe ones from candidates
		targetCandidates.RemoveAll(targetCandidate => targetCandidate == currTarget || expanDialSticks.modelMatrix[targetCandidate.x, targetCandidate.y].CurrentProximity == 1f);

		if (targetCandidates.Count() > 0)
		{
			// Get safest target and distractors
			Vector2Int nextTarget = new Vector2Int(-1, -1);
			List<Vector2Int> nextDistractors = new List<Vector2Int>();
			int maxSafeCount = 0;
			for (int i = 0; i < targetCandidates.Count(); i++)
			{

				List<Vector2Int> distractorCandidates = new List<Vector2Int>() {
					targetCandidates[i] + LEFT,
					targetCandidates[i] + LEFT_UP,
					targetCandidates[i] + UP,
					targetCandidates[i] + RIGHT_UP,
					targetCandidates[i] + RIGHT,
					targetCandidates[i] + RIGHT_DOWN,
					targetCandidates[i] + DOWN,
					targetCandidates[i] + LEFT_DOWN
				};
				distractorCandidates.RemoveAll(distractorCandidate => expanDialSticks.modelMatrix[distractorCandidate.x, distractorCandidate.y].CurrentProximity == 1f);
				if (distractorCandidates.Count() > maxSafeCount)
				{
					maxSafeCount = distractorCandidates.Count();
					nextTarget = targetCandidates[i];
					nextDistractors = distractorCandidates;
				}
			}
			//Debug.Log("nextTarget => " + nextTarget);
			// Check for target with less occlusion
			currTarget = nextTarget;

			// Texture change target
			cadranRotation = aiguilleRotation = startRotation;
			ShowGaugeOnTarget(0.1f);
			expanDialSticks.triggerTextureChange();
			expanDialSticks.triggerProjectorChange();
			yield return new WaitForSeconds(0.1f);

			MetricsActive = true;
			// Wait for Gauge Game End
			// Loop Until Start
			gaugeState = GAUGE_APPEARED;
			bool endedUpMove = false;
			bool endedDownMove = false;
			// Game
			Debug.Log("Waiting for unpause motion to finish...");
			while (!endedUpMove || !endedDownMove)
			{

				if (gaugeState == GAUGE_STARTED) // User started task
				{
					// Move it
					float prevRotation = cadranRotation;
					switch (directionRotation)
					{
						case DirectionRotation.CW:
							cadranRotation += speedRotation * Time.deltaTime;
							break;
						case DirectionRotation.CCW:
							cadranRotation -= speedRotation * Time.deltaTime;
							break;
						default:
							break;
					}
					MoveAiguilleCadran(0.1f);
					expanDialSticks.triggerTextureChange();
					expanDialSticks.triggerProjectorChange();

					//string msg = "SYSTEM_ROTATION " + prevRotation + " " + cadranRotation;
					if ((int)prevRotation != (int)cadranRotation)
					{
						//Debug.Log(msg);
					}
					if (!endedUpMove)
					{
						// wait for previous motion to finish
						bool nextEndedUpMove = true;
						for (int i = 0; i < expanDialSticks.NbRows; i++)
							for (int j = 0; j < expanDialSticks.NbColumns; j++)
								if (expanDialSticks.viewMatrix[i, j].CurrentReaching) nextEndedUpMove = false;

						// then trigger reset
						if (nextEndedUpMove)
						{
							Debug.Log("Up ended!");
							// Reset Shape except target
							for (int i = 0; i < expanDialSticks.NbRows; i++)
							{
								for (int j = 0; j < expanDialSticks.NbColumns; j++)
								{
									if (nextTarget.x != i || nextTarget.y != j)
									{
										expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
										expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = longShapeChangeDuration;
									}
								}
							}
							Debug.Log("Trigger Shape Reset!");
							expanDialSticks.triggerShapeChange();
							endedUpMove = nextEndedUpMove;
						}
					} else
					{
						// wait for previous motion to finish
						bool nextEndedDownMove = true;
						for (int i = 0; i < expanDialSticks.NbRows; i++)
							for (int j = 0; j < expanDialSticks.NbColumns; j++)
								if (expanDialSticks.viewMatrix[i, j].CurrentReaching) nextEndedDownMove = false;

						if (nextEndedDownMove)
						{
							Debug.Log("Down ended!");
							endedDownMove = nextEndedDownMove;
						}
					}

				}
				yield return new WaitForSeconds(0.1f);
			}
			Debug.Log("Unpause motion finished!");


			// Shape distractors
			String shapeChangeMsg = "SYSTEM_TRIGGER_SHAPE_CHANGE";
			foreach (Vector2Int distractor in nextDistractors)
			{
				shapeChangeMsg += " " + new Vector3Int(distractor.x, distractor.y, targetPos).ToString();
				expanDialSticks.modelMatrix[distractor.x, distractor.y].TargetPosition = targetPos;
				expanDialSticks.modelMatrix[distractor.x, distractor.y].TargetShapeChangeDuration = longShapeChangeDuration;
			}
			expanDialSticks.triggerShapeChange();
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(shapeChangeMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

			// wait for unpause motion to finish
			/*Debug.Log("Waiting for unpause motion to finish...");
			bool safeShapeChangeCompleted = false;
			int count = 0;
			while (!safeShapeChangeCompleted)
			{
				DebugInSitu("Waiting for safe shape-change to complete(" + count + ")...", Color.black, Color.white);
				safeShapeChangeCompleted = true;
				for (int i = 0; i < expanDialSticks.NbRows; i++)
					for (int j = 0; j < expanDialSticks.NbColumns; j++)
						if (expanDialSticks.modelMatrix[i, j].CurrentReaching)
						{
							safeShapeChangeCompleted = false;
						}
				count++;
				yield return new WaitForSeconds(COMPLETION_INTERVAL);
			}
			Debug.Log("Unpause motion finished!");*/
			currTrial++;
		}
		else
		{

			yield return new WaitForSeconds(COMPLETION_INTERVAL);
		}
		triggerNextTrial = true;

	}

	private IEnumerator BetweenShapeChangeTask()
	{

		// Reset Shape
		targetAroundIsSelected = false;
		MetricsActive = false;

		// Reset Texture
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.black;
				expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
				expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = 0f;
				expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = 0.1f;

				expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "projector";
				expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = 0.1f;

			}
		}
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerProjectorChange();
		yield return new WaitForSeconds(0.1f);

		// Generate Target candidates
		//Debug.Log("currTarget=>" + currTarget);
		List <Vector2Int> targetCandidates = new List<Vector2Int> { TOP_LEFT_CORNER, TOP_RIGHT_CORNER, BOT_LEFT_CORNER, BOT_RIGHT_CORNER };
		ListExtension.Shuffle(targetCandidates);
		// Remove prev target and unsafe ones from candidates
		targetCandidates.RemoveAll(targetCandidate => targetCandidate == currTarget || expanDialSticks.modelMatrix[targetCandidate.x, targetCandidate.y].CurrentProximity == 1f);
		if (targetCandidates.Count() > 0)
		{
			Vector2Int nextTarget = targetCandidates[0];
			List<Vector2Int> nextDistractors = new List<Vector2Int>();

			// diagonale 1 
			if (currTarget == BOT_LEFT_CORNER && nextTarget == TOP_RIGHT_CORNER)
			{
				for (int i = 0; i < expanDialSticks.NbRows; i++)
				{
						nextDistractors.Add(new Vector2Int(i, i+1));
				}
			}
			if (currTarget == TOP_RIGHT_CORNER && nextTarget == BOT_LEFT_CORNER)
			{
				for (int i = 0; i < expanDialSticks.NbRows; i++)
				{
					nextDistractors.Add(new Vector2Int(i, i));
				}
			}

			// diagonale 2
			if (currTarget == TOP_LEFT_CORNER && nextTarget == BOT_RIGHT_CORNER)
			{
				for (int i = 0; i < expanDialSticks.NbRows; i++)
				{
					nextDistractors.Add(new Vector2Int(i, (expanDialSticks.NbColumns- 1) - i));
				}
			}
			if (currTarget == BOT_RIGHT_CORNER && nextTarget == TOP_LEFT_CORNER)
			{
				for (int i = 0; i < expanDialSticks.NbRows; i++)
				{
					nextDistractors.Add(new Vector2Int(i, (expanDialSticks.NbColumns - 1) - (i + 1) ));
				}
			}

			if ((currTarget == TOP_LEFT_CORNER && nextTarget == BOT_RIGHT_CORNER) || (nextTarget == TOP_LEFT_CORNER && currTarget == BOT_RIGHT_CORNER))
			{
				for (int i = 0; i < expanDialSticks.NbRows; i++)
				{
					for (int j = (expanDialSticks.NbColumns - 1 - (i + 1)); j <= (expanDialSticks.NbColumns - 1 - i); j++)
					{
						nextDistractors.Add(new Vector2Int(i, j));
					}
				}
			}
			// row 1
			if ((currTarget == TOP_LEFT_CORNER && nextTarget == BOT_LEFT_CORNER) || (nextTarget == TOP_LEFT_CORNER && currTarget == BOT_LEFT_CORNER))
			{

					for (int j = 0; j < expanDialSticks.NbColumns; j++)
					{
						nextDistractors.Add(new Vector2Int(2, j));
					}
			}
			if ((currTarget == TOP_RIGHT_CORNER && nextTarget == BOT_RIGHT_CORNER) || (nextTarget == TOP_RIGHT_CORNER && currTarget == BOT_RIGHT_CORNER))
			{

				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					nextDistractors.Add(new Vector2Int(2, j));
				}
			}

			if ((currTarget == TOP_LEFT_CORNER && nextTarget == TOP_RIGHT_CORNER) || (nextTarget == TOP_LEFT_CORNER && currTarget == TOP_RIGHT_CORNER))
			{

				for (int i = 0; i < expanDialSticks.NbRows; i++)
				{
					nextDistractors.Add(new Vector2Int(i, 2));
					nextDistractors.Add(new Vector2Int(i, 3));
				}
			}
			if ((currTarget == BOT_LEFT_CORNER && nextTarget == BOT_RIGHT_CORNER) || (nextTarget == BOT_LEFT_CORNER && currTarget == BOT_RIGHT_CORNER))
			{

				for (int i = 0; i < expanDialSticks.NbRows; i++)
				{
					nextDistractors.Add(new Vector2Int(i, 2));
					nextDistractors.Add(new Vector2Int(i, 3));
				}
			}

			// Check for target with less occlusion
			currTarget = nextTarget;

			// Texture change target
			cadranRotation = aiguilleRotation = startRotation;
			ShowGaugeOnTarget(0.1f);
			expanDialSticks.triggerTextureChange();
			expanDialSticks.triggerProjectorChange();
			yield return new WaitForSeconds(0.1f);

			// Reset Shape except target
			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					if (nextTarget.x != i || nextTarget.y != j)
					{
						expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
						expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = longShapeChangeDuration;
					}
				}
			}

			// Shape distractors
			String shapeChangeMsg = "SYSTEM_TRIGGER_SHAPE_CHANGE";
			foreach (Vector2Int distractor in nextDistractors)
			{
				shapeChangeMsg += " " + new Vector3Int(distractor.x, distractor.y, targetPos).ToString();
				expanDialSticks.modelMatrix[distractor.x, distractor.y].TargetPosition = targetPos;
				expanDialSticks.modelMatrix[distractor.x, distractor.y].TargetShapeChangeDuration = longShapeChangeDuration;
			}
			expanDialSticks.triggerShapeChange();
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(shapeChangeMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
			MetricsActive = true;
			// Wait for Gauge Game End
			// Loop Until Start
			gaugeState = GAUGE_APPEARED;
			bool finished = false;
			// Game
			Debug.Log("Waiting for unpause motion to finish...");
			while (!finished)
			{

				if (gaugeState == GAUGE_STARTED) // User started task
				{
					// Move it
					float prevRotation = cadranRotation;
					switch (directionRotation)
					{
						case DirectionRotation.CW:
							cadranRotation += speedRotation * Time.deltaTime;
							break;
						case DirectionRotation.CCW:
							cadranRotation -= speedRotation * Time.deltaTime;
							break;
						default:
							break;
					}
					MoveAiguilleCadran(0.1f);
					expanDialSticks.triggerTextureChange();
					expanDialSticks.triggerProjectorChange();

					//string msg = "SYSTEM_ROTATION " + prevRotation + " " + cadranRotation;
					if ((int)prevRotation != (int)cadranRotation)
					{
						//Debug.Log(msg);
					}
					// wait for unpause motion to finish
					//DebugInSitu("Waiting for safe shape-change to complete(" + count + ")...", Color.black, Color.white);
					finished = true;
					for (int i = 0; i < expanDialSticks.NbRows; i++)
						for (int j = 0; j < expanDialSticks.NbColumns; j++)
							if (expanDialSticks.modelMatrix[i, j].CurrentReaching) finished = false;
				}
				yield return new WaitForSeconds(0.1f);
			}
			Debug.Log("Unpause motion finished!");
			currTrial++;

		}
		else
		{

			yield return new WaitForSeconds(COMPLETION_INTERVAL);
		}
		triggerNextTrial = true;
	}
	private void TriggerTrial()
	{
		Debug.Log("TriggerTrial()");
		if(currTrial < nbTrials)
		{
			switch (currTaskMode)
			{
				case TaskMode.SC_AROUND:
					//Debug.Log("SC AROUND");
					StartCoroutine(AroundShapeChangeTask());
					break;


				case TaskMode.SC_UNDER:
					//Debug.Log("SC UNDER");
					StartCoroutine(UnderShapeChangeTask());
					break;

				case TaskMode.SC_BETWEEN:
					//Debug.Log("SC BETWEEN");
					StartCoroutine(BetweenShapeChangeTask());
					break;
				default:
					break;
			}

		} else
		{
			Quit();
		}
	}


	void ShowGaugeOnTarget(float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.black; //Color.green;
				if (i == currTarget.x && j == currTarget.y)
				{
					expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "LightCadran";
					expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = cadranRotation;
					expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0.6f;
					expanDialSticks.modelMatrix[i, j].TargetPlaneColor = Color.red;

					expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "aiguille";
					expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = aiguilleRotation;
					expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 0.02f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorColor = Color.white;
				}

				else
				{
					expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
					expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = cadranRotation;
					expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetPlaneColor = Color.black;

					expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "projector";
					expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = aiguilleRotation;
					expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorColor = Color.white;
				}

				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
				expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = duration;
			}
		}
		string participantNumber = "<pos=0%><b>P" + numeroParticipant + "</b>";
		string trialProgress = "<pos=90%><b>" + (currTargetIndex + 1) + "/" + targets.Count() + "</b>";
		string legend = participantNumber + trialProgress;
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 0.1f, Color.black, legend, new Vector3(90f, -90f, 0f));
		expanDialSticks.setBorderBackground(Color.white);
	}


	void MoveAiguilleCadran(float duration)
	{
		expanDialSticks.modelMatrix[currTarget.x, currTarget.y].TargetPlaneRotation = cadranRotation;
		expanDialSticks.modelMatrix[currTarget.x, currTarget.y].TargetTextureChangeDuration = duration;
		expanDialSticks.modelMatrix[currTarget.x, currTarget.y].TargetProjectorRotation = aiguilleRotation;
		expanDialSticks.modelMatrix[currTarget.x, currTarget.y].TargetProjectorChangeDuration = duration;
	}


	void Update()
	{
		// check if ExpanDialSticks is connected
		if (connected && !unknownParticipant)
		{
			currTime = Time.time;

			if(triggerNextTrial == true)
			{
				TriggerTrial();
				triggerNextTrial = false;
			}
			if (MetricsActive && currTime - prevMetricsTime >= LOG_INTERVAL)
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
				HandleRotationChanged(new object(), new ExpanDialStickEventArgs(DateTime.Now, currTarget.x, currTarget.y, 0, 1, 1));
				//currentRotation += anglePerStep;
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				HandleRotationChanged(new object(), new ExpanDialStickEventArgs(DateTime.Now, currTarget.x, currTarget.y, 1, 0, -1));
				//currentRotation -= anglePerStep;
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

		string positionString = "SYSTEM_POSITION ";
		string reachingString = "SYSTEM_MOTION ";
		string pauseString = "SYSTEM_STOP ";
		string leftHandString = "USER_LEFT_HAND " + leftHand.ToString();
		string rightHandString = "USER_RIGHT_HAND " + rightHand.ToString();

		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				positionString += expanDialSticks.viewMatrix[i, j].CurrentPosition + " ";
				reachingString += (expanDialSticks.viewMatrix[i, j].CurrentReaching?1:0) + " ";
				pauseString += expanDialSticks.viewMatrix[i, j].CurrentPaused + " ";
			}
		}

		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(positionString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(reachingString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(pauseString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(leftHandString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(rightHandString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
	}
}