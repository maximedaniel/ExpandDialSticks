
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
	private const int BEGIN_TARGET_START = 5;
	private const int BEGIN_TARGET_TRIGGER = 6;
	private const int BEGIN_TARGET_STOP = 7;
	private const int END_TARGET_START = 8;
	private const int END_TARGET_TRIGGER = 9;
	private const int END_TARGET_STOP = 10;

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



	private List<List<Vector2Int>> distractorsList = new List<List<Vector2Int>>();
	private int currDistractorIndex;
	private List<Vector2Int> currDistractors;

	public enum TaskMode { SC_UNDER, SC_BETWEEN, SC_AROUND };
	private TaskMode currTaskMode;

	private List<Vector2Int> candidates = new List<Vector2Int>();

	List<Tuple<Vector2Int, Vector2Int>> targetPairs = new List<Tuple<Vector2Int, Vector2Int>>();
	private int currTargetPairIndex;
	private Tuple<Vector2Int, Vector2Int> currTargetPair;
	private int totalTargetPairs;

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
		targetPairs = new List<Tuple<Vector2Int, Vector2Int>>();
		currTargetPairIndex = -1;
		currTargetPair = new Tuple<Vector2Int, Vector2Int>(new Vector2Int(-1, -1), new Vector2Int(-1, -1));
		distractorsList = new List< List < Vector2Int >> ();
		currDistractorIndex = -1;
		currDistractors = new List<Vector2Int>();
		prevSelectPosition = currSelectPosition = new Vector2Int(-1, -1);
		DELTAS = new List<Vector2Int>() {LEFT, LEFT_UP, UP, RIGHT_UP, RIGHT, RIGHT_DOWN, DOWN, LEFT_DOWN};
		//GenerateTrials();
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
		targetPairs = new List<Tuple<Vector2Int, Vector2Int>>();
		distractorsList = new List<List<Vector2Int>>();
		switch (currTaskMode)
		{
			case TaskMode.SC_AROUND:
				List<Vector2Int> targetCandidates = new List<Vector2Int> { new Vector2Int(1, 1), new Vector2Int(1, 4), new Vector2Int(3, 1), new Vector2Int(3, 4) };
				for (int i = 0; i < targetCandidates.Count(); i++)
				{
					Vector2Int firstTarget = targetCandidates[i];
					for (int j = 0; j < targetCandidates.Count(); j++)
					{
						Vector2Int secondTarget = targetCandidates[j];
						if (i != j)
						{
							targetPairs.Add(new Tuple<Vector2Int, Vector2Int>(firstTarget, secondTarget));
							distractorsList.Add(
								new List<Vector2Int>
								{
										secondTarget + LEFT,
										secondTarget + LEFT_UP,
										secondTarget + UP,
										secondTarget  + RIGHT_UP,
										secondTarget + RIGHT,
										secondTarget + RIGHT_DOWN,
										secondTarget + DOWN,
										secondTarget + LEFT_DOWN
								}
							);
						}
					}
				}
				break;
			case TaskMode.SC_UNDER:
				targetCandidates = new List<Vector2Int> { new Vector2Int(1, 1), new Vector2Int(1, 4), new Vector2Int(3, 1), new Vector2Int(3, 4) };
				targetPairs = new List<Tuple<Vector2Int, Vector2Int>>();
				for (int i = 0; i < targetCandidates.Count(); i++)
				{
					Vector2Int firstTarget = targetCandidates[i];
					for (int j = 0; j < targetCandidates.Count(); j++)
					{
						Vector2Int secondTarget = targetCandidates[j];
						if (i != j)
						{
							targetPairs.Add(new Tuple<Vector2Int, Vector2Int>(firstTarget, secondTarget));
							distractorsList.Add(
								new List<Vector2Int>
								{
										firstTarget + LEFT,
										firstTarget + LEFT_UP,
										firstTarget + UP,
										firstTarget  + RIGHT_UP,
										firstTarget + RIGHT,
										firstTarget + RIGHT_DOWN,
										firstTarget + DOWN,
										firstTarget + LEFT_DOWN
								}
							);
						}
					}
				}
				break;
			case TaskMode.SC_BETWEEN:
				targetPairs = new List<Tuple<Vector2Int, Vector2Int>>();
				// TOP_LEFT -> BOT_LEFT
				targetPairs.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(0,1), new Vector2Int(4, 1)));
				distractorsList.Add(
								new List<Vector2Int>
								{
										new Vector2Int(2,1) + LEFT,
										new Vector2Int(2,1) + LEFT_UP,
										new Vector2Int(2,1) + UP,
										new Vector2Int(2,1)  + RIGHT_UP,
										new Vector2Int(2,1) + RIGHT,
										new Vector2Int(2,1) + RIGHT_DOWN,
										new Vector2Int(2,1) + DOWN,
										new Vector2Int(2,1) + LEFT_DOWN
								}
							);
				// BOT_LEFT -> TOP_LEFT
				targetPairs.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(4, 1), new Vector2Int(0, 1)));
				distractorsList.Add(distractorsList.Last());

				// TOP_RIGHT -> BOT_RIGHT
				targetPairs.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(0, 4), new Vector2Int(4, 4)));
				distractorsList.Add(
								new List<Vector2Int>
								{
										new Vector2Int(2,4) + LEFT,
										new Vector2Int(2,4) + LEFT_UP,
										new Vector2Int(2,4) + UP,
										new Vector2Int(2,4)  + RIGHT_UP,
										new Vector2Int(2,4) + RIGHT,
										new Vector2Int(2,4) + RIGHT_DOWN,
										new Vector2Int(2,4) + DOWN,
										new Vector2Int(2,4) + LEFT_DOWN
								}
							);
				// BOT_RIGHT -> TOP_RIGHT
				targetPairs.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(4, 4), new Vector2Int(0, 4)));
				distractorsList.Add(distractorsList.Last());



				// TOP_LEFT -> TOP_RIGHT
				targetPairs.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(1, 0), new Vector2Int(1, 4)));
				distractorsList.Add(
								new List<Vector2Int>
								{
										new Vector2Int(1,2) + LEFT,
										new Vector2Int(1,2) + LEFT_UP,
										new Vector2Int(1,2) + UP,
										new Vector2Int(1,2)  + RIGHT_UP,
										new Vector2Int(1,2) + RIGHT,
										new Vector2Int(1,2) + RIGHT_DOWN,
										new Vector2Int(1,2) + DOWN,
										new Vector2Int(1,2) + LEFT_DOWN
								}
							);
				// TOP_RIGHT -> TOP_LEFT
				targetPairs.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(1, 4), new Vector2Int(1, 0)));
				distractorsList.Add(distractorsList.Last());

				// BOT_LEFT -> BOT_RIGHT
				targetPairs.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(3, 0), new Vector2Int(3, 4)));
				distractorsList.Add(
								new List<Vector2Int>
								{
										new Vector2Int(3,2) + LEFT,
										new Vector2Int(3,2) + LEFT_UP,
										new Vector2Int(3,2) + UP,
										new Vector2Int(3,2)  + RIGHT_UP,
										new Vector2Int(3,2) + RIGHT,
										new Vector2Int(3,2) + RIGHT_DOWN,
										new Vector2Int(3,2) + DOWN,
										new Vector2Int(3,2) + LEFT_DOWN
								}
							);
				// BOT_RIGHT -> BOT_LEFT
				targetPairs.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(3, 4), new Vector2Int(3, 0)));
				distractorsList.Add(distractorsList.Last());

				// TOP_LEFT -> BOT_RIGHT
				targetPairs.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(0, 1), new Vector2Int(4, 5)));
				distractorsList.Add(
								new List<Vector2Int>
								{
										new Vector2Int(2,3) + LEFT,
										new Vector2Int(2,3) + LEFT_UP,
										new Vector2Int(2,3) + UP,
										new Vector2Int(2,3)  + RIGHT_UP,
										new Vector2Int(2,3) + RIGHT,
										new Vector2Int(2,3) + RIGHT_DOWN,
										new Vector2Int(2,3) + DOWN,
										new Vector2Int(2,3) + LEFT_DOWN
								}
							);
				// BOT_RIGHT -> BOT_LEFT
				targetPairs.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(4, 5), new Vector2Int(0, 1)));
				distractorsList.Add(distractorsList.Last());

				// TOP_RIGHT -> BOT_LEFT
				targetPairs.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(0, 4), new Vector2Int(4, 0)));
				distractorsList.Add(
								new List<Vector2Int>
								{
										new Vector2Int(2,2) + LEFT,
										new Vector2Int(2,2) + LEFT_UP,
										new Vector2Int(2,2) + UP,
										new Vector2Int(2,2)  + RIGHT_UP,
										new Vector2Int(2,2) + RIGHT,
										new Vector2Int(2,2) + RIGHT_DOWN,
										new Vector2Int(2,2) + DOWN,
										new Vector2Int(2,2) + LEFT_DOWN
								}
							);
				// BOT_LEFT -> TOP_RIGHT
				targetPairs.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(4, 0), new Vector2Int(0, 4)));
				distractorsList.Add(distractorsList.Last());
				break;
			default:
				break;
		}

		totalTargetPairs = targetPairs.Count();
		/*for (int i = 1; i < 2; i++)
		{
			for (int j = 1; j < expanDialSticks.NbColumns - 1; j++)
			{
				candidates.Add(new Vector2Int(i, j));
			}
		}*/

		/*for(int i = 0; i < nbPins.Length; i++)
		{
			int nbPin = nbPins[i];
			for (int j = 0; j < nbTrials; j++)
			{
				ListExtension.Shuffle(DELTAS);
				distractors.Add(DELTAS.GetRange(0, nbPin));
			}
		}*/

		/*distractors.Add(new List<Vector2Int> { UP, RIGHT, DOWN });
		distractors.Add(new List<Vector2Int> { RIGHT, DOWN, LEFT });
		distractors.Add(new List<Vector2Int> { DOWN, LEFT, UP });

		distractors.Add(new List<Vector2Int> { LEFT_UP, RIGHT_UP, RIGHT_DOWN });
		distractors.Add(new List<Vector2Int> { RIGHT_UP, RIGHT_DOWN, LEFT_DOWN });
		distractors.Add(new List<Vector2Int> { RIGHT_DOWN, LEFT_DOWN, LEFT_UP });
		distractors.Add(new List<Vector2Int> { LEFT_DOWN, LEFT_UP, RIGHT_UP });

		distractors.Add(new List<Vector2Int> { LEFT_DOWN, LEFT_UP, RIGHT });
		distractors.Add(new List<Vector2Int> { LEFT_UP, RIGHT_UP, DOWN });
		distractors.Add(new List<Vector2Int> { RIGHT_UP, RIGHT_DOWN, LEFT });
		distractors.Add(new List<Vector2Int> { RIGHT_DOWN, LEFT_DOWN, UP });*/
		/*for (int i = 0; i < nbTrials; i++)
		{
			targets.Add(candidates[i % candidates.Count()]);
		}
		ListExtension.Shuffle(targets);*/



		//ListExtension.Shuffle(distractors);


		/*for (int i = 0; i < distractors.Count(); i++)
		{
			//Debug.Log(i % candidates.Count());
			//Debug.Log(candidates[i % candidates.Count()]);
			targets.Add(candidates[i % candidates.Count()]);
		}
		ListExtension.Shuffle(targets);*/
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
		if (currSelectPosition == currTargetPair.Item1)
		{
			if (gaugeState == BEGIN_TARGET_START)
			{
				gaugeState = BEGIN_TARGET_TRIGGER;
			}
			float prevRotation = aiguilleRotation;
			aiguilleRotation -= e.diff * anglePerStep;
			aiguilleRotation = aiguilleRotation % 360f;
			string msg = "";
			msg += "USER_ROTATION " + prevRotation + " " + aiguilleRotation;
			//Debug.Log(msg);
			/*if (gaugeState == GAUGE_APPEARED)
			{
				startGameTime = Time.time;
				motionDuration = Random.Range(5f, initGameDuration - 5f);
				gameDuration = initGameDuration;
				gaugeState = GAUGE_STARTED;
			}*/
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		}
		if (currSelectPosition == currTargetPair.Item2)
		{
			if (gaugeState == END_TARGET_START)
			{
				gaugeState = END_TARGET_TRIGGER;
			}
			float prevRotation = aiguilleRotation;
			aiguilleRotation -= e.diff * anglePerStep;
			aiguilleRotation = aiguilleRotation % 360f;
			string msg = "";
			msg += "USER_ROTATION " + prevRotation + " " + aiguilleRotation;
			//Debug.Log(msg);
			/*if (gaugeState == GAUGE_APPEARED)
			{
				startGameTime = Time.time;
				motionDuration = Random.Range(5f, initGameDuration - 5f);
				gameDuration = initGameDuration;
				gaugeState = GAUGE_STARTED;
			}*/
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
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
				GenerateTrials();
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY - 25, componentWidth, componentHeight), "SYSTEM Overlay | SC Around"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.System;
				currTaskMode = TaskMode.SC_AROUND;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | SYSTEM OVERLAY | SC AROUND";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				GenerateTrials();
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY, componentWidth, componentHeight), "USER Overlay | SC Under"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.User;
				currTaskMode = TaskMode.SC_UNDER;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | USER OVERLAY | SC UNDER";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				GenerateTrials();
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY + 25, componentWidth, componentHeight), "SYSTEM Overlay | SC Under"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.System;
				currTaskMode = TaskMode.SC_UNDER;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | SYSTEM OVERLAY | SC UNDER";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				GenerateTrials();
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY + 50, componentWidth, componentHeight), "USER Overlay | SC Between"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.User;
				currTaskMode = TaskMode.SC_BETWEEN;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | USER OVERLAY | SC BETWEEN";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				GenerateTrials();
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY + 75, componentWidth, componentHeight), "SYSTEM Overlay | SC Between"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.System;
				currTaskMode = TaskMode.SC_BETWEEN;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | SYSTEM OVERLAY | SC BETWEEN";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				GenerateTrials();
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

	private void PrepareShapeChangeAtTarget(Vector2Int target, float shapeChangeDuration)
	{
		expanDialSticks.modelMatrix[target.x, target.y].TargetPosition = targetPos;
		expanDialSticks.modelMatrix[target.x, target.y].TargetShapeChangeDuration = shapeChangeDuration;
	}
	private void PrepareShapeChangeAroundTarget(Vector2Int target, float shapeChangeDuration)
	{
		for(int i = target.x - 1; i <= target.x + 1; i++)
		{
			for (int j = target.y - 1; j <= target.y + 1; j++)
			{
				if (target.x != i || target.y != j)
				{
					expanDialSticks.modelMatrix[i, j].TargetPosition = targetPos;
					expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = shapeChangeDuration;
				}
			}
		}
	}
	private void PrepareResetTextureAndProjector(float duration)
	{   
		// Reset Texture
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.black;
				expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
				expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = 0f;
				expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;

				expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "projector";
				expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = duration;

			}
		}

	}
	private void PrepareResetShape(float duration)
	{
		// Reset Texture
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
				expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = duration;

			}
		}

	}
	private IEnumerator TriggerTrial()
	{
		Debug.Log("TriggerTrial()");
		if(targetPairs.Count() > 0)
		{

			MetricsActive = false;
			// Fetch model
			int trialIndex = totalTargetPairs - targetPairs.Count();
			currTargetPair = targetPairs.First();
			targetPairs.RemoveAt(0);
			Vector2Int firstTarget = currTargetPair.Item1;
			Vector2Int secondTarget = currTargetPair.Item2;
			currDistractors = distractorsList.First();
			distractorsList.RemoveAt(0);
			string startTrialMsg = "TRIAL_STARTED " + trialIndex;
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(startTrialMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);


			// Reset Texture and Projector
			PrepareResetTextureAndProjector(0.1f);
			expanDialSticks.triggerTextureChange();
			expanDialSticks.triggerProjectorChange();
			yield return new WaitForSeconds(0.1f);

			// Move up the two targets
			expanDialSticks.modelMatrix[firstTarget.x, firstTarget.y].TargetPosition = targetPos;
			expanDialSticks.modelMatrix[firstTarget.x, firstTarget.y].TargetShapeChangeDuration = shortShapeChangeDuration;
			expanDialSticks.modelMatrix[secondTarget.x, secondTarget.y].TargetPosition = targetPos;
			expanDialSticks.modelMatrix[secondTarget.x, secondTarget.y].TargetShapeChangeDuration = shortShapeChangeDuration;
			expanDialSticks.triggerShapeChange();
			yield return new WaitForSeconds(shortShapeChangeDuration);

			// Texture First Target
			// 24 position
			cadranRotation = Random.Range(0, 23) * anglePerStep;
			aiguilleRotation = cadranRotation + (-1 + Random.Range(0, 1) * 2) * anglePerStep;
			ShowGaugeOnTarget(firstTarget, 0.1f);
			expanDialSticks.triggerTextureChange();
			expanDialSticks.triggerProjectorChange();
			yield return new WaitForSeconds(0.1f);
			string firstTargetMsg = "FIRST_TARGET " + firstTarget.ToString() + " CADRAN " + cadranRotation + " AIGUILLE " + aiguilleRotation;
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(firstTargetMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

			gaugeState = BEGIN_TARGET_START;
			bool distractorsTriggered = false;
			// wait for start signal
			while (gaugeState != BEGIN_TARGET_STOP)
			{
				/*if(!distractorsTriggered && gaugeState == BEGIN_TARGET_TRIGGER)
				{
					distractorsTriggered = true;
				}*/
				if (aiguilleRotation >= (cadranRotation - anglePerStep / 2f) % 360f && aiguilleRotation <= (cadranRotation + anglePerStep / 2f) % 360f)
				{

					// Trigger distractors
					String shapeChangeMsg = "SYSTEM_TRIGGER_SHAPE_CHANGE";
					foreach (Vector2Int distractor in currDistractors)
					{
						shapeChangeMsg += " " + new Vector3Int(distractor.x, distractor.y, targetPos).ToString();
						expanDialSticks.modelMatrix[distractor.x, distractor.y].TargetPosition = targetPos;
						expanDialSticks.modelMatrix[distractor.x, distractor.y].TargetShapeChangeDuration = longShapeChangeDuration;
					}
					expanDialSticks.triggerShapeChange();
					expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(shapeChangeMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

					MetricsActive = true;
					gaugeState = BEGIN_TARGET_STOP;
				}
				//Debug.Log((cadranRotation - anglePerStep / 2f) + " <= " + aiguilleRotation + " <=  " + (cadranRotation + anglePerStep / 2f));
				// Move it
				float prevRotation = cadranRotation;
				switch (directionRotation)
				{
					case DirectionRotation.CW:
						cadranRotation += speedRotation * 0.1f;
						break;
					case DirectionRotation.CCW:
						cadranRotation -= speedRotation * 0.1f;
						break;
					default:
						break;
				}
				cadranRotation = cadranRotation % 360f;
				MoveAiguilleCadran(firstTarget, 0.1f);
				expanDialSticks.triggerTextureChange();
				expanDialSticks.triggerProjectorChange();
				yield return new WaitForSeconds(0.1f);
			}

			// Texture Second Target
			cadranRotation = Random.Range(0, 23) * anglePerStep;
			aiguilleRotation = cadranRotation + 12f * anglePerStep;
			ShowGaugeOnTarget(secondTarget, 0.1f);
			expanDialSticks.triggerTextureChange();
			expanDialSticks.triggerProjectorChange();
			string secondTargetMsg = "SECOND_TARGET " + secondTarget.ToString() + " CADRAN " + cadranRotation + " AIGUILLE " + aiguilleRotation;
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(secondTargetMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
			yield return new WaitForSeconds(0.1f);
			gaugeState = END_TARGET_START;


			// wait for end signal
			while (gaugeState != END_TARGET_STOP)
			{
				if (aiguilleRotation >= (cadranRotation - anglePerStep / 2f)%360f && aiguilleRotation <= (cadranRotation + anglePerStep / 2f)%360f)
				{
					gaugeState = END_TARGET_STOP;
				}
				//Debug.Log((cadranRotation - anglePerStep / 2f) + " <= " + aiguilleRotation + " <=  " + (cadranRotation + anglePerStep / 2f));
				// Move it
				float prevRotation = cadranRotation;
				switch (directionRotation)
				{
					case DirectionRotation.CW:
						cadranRotation += speedRotation * 0.1f;
						break;
					case DirectionRotation.CCW:
						cadranRotation -= speedRotation * 0.1f;
						break;
					default:
						break;
				}
				cadranRotation = cadranRotation % 360f;
				MoveAiguilleCadran(secondTarget, 0.1f);
				expanDialSticks.triggerTextureChange();
				expanDialSticks.triggerProjectorChange();
				yield return new WaitForSeconds(0.1f);
			}

			// Reset Texture and Projector
			PrepareResetTextureAndProjector(0.1f);
			expanDialSticks.triggerTextureChange();
			expanDialSticks.triggerProjectorChange();
			yield return new WaitForSeconds(0.1f);

			// wait for shape-change completion
			bool shapeChangeEnded = false;
			while (!shapeChangeEnded)
			{
				shapeChangeEnded = true;
				Debug.Log("waiting for shape-change to complete...");

				foreach (Vector2Int distractor in currDistractors)
				{

					if (expanDialSticks.viewMatrix[distractor.x, distractor.y].CurrentReaching || 
						expanDialSticks.viewMatrix[distractor.x, distractor.y].CurrentPosition < targetPos - 1 ||
						expanDialSticks.viewMatrix[distractor.x, distractor.y].CurrentPosition > targetPos + 1) shapeChangeEnded = false;
				}
				yield return new WaitForSeconds(COMPLETION_INTERVAL);
			}

			// Trigger texture and shape reset
			PrepareResetTextureAndProjector(0.1f);
			expanDialSticks.triggerTextureChange();
			expanDialSticks.triggerProjectorChange();
			yield return new WaitForSeconds(0.1f);
			PrepareResetShape(shortShapeChangeDuration);
			expanDialSticks.triggerShapeChange();
			yield return new WaitForSeconds(shortShapeChangeDuration);

			// Wait for shape-change completion
			bool resetEnded = false;
			while (!resetEnded)
			{
				resetEnded = true;
				Debug.Log("waiting for reset to complete...");
				for (int i = 0; i < expanDialSticks.NbRows; i++)
					for (int j = 0; j < expanDialSticks.NbColumns; j++)
						if (expanDialSticks.viewMatrix[i, j].CurrentReaching || expanDialSticks.viewMatrix[i, j].CurrentPosition != 0) resetEnded = false;
				yield return new WaitForSeconds(COMPLETION_INTERVAL);
			}
			MetricsActive = false;

			string endTrialMsg = "TRIAL_ENDED " + trialIndex;
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(endTrialMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
			triggerNextTrial = true;

		} else
		{
			Quit();
		}
	}


	void ShowGaugeOnTarget(Vector2Int target, float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.black; //Color.green;
				if (i == target.x && j == target.y)
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
		string trialProgress = "<pos=90%><b>" + targetPairs.Count() + "/" + totalTargetPairs + "</b>";
		string legend = participantNumber + trialProgress;
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 0.1f, Color.black, legend, new Vector3(90f, -90f, 0f));
		expanDialSticks.setBorderBackground(Color.white);
	}


	void MoveAiguilleCadran(Vector2Int target, float duration)
	{
		expanDialSticks.modelMatrix[target.x, target.y].TargetPlaneRotation = cadranRotation;
		expanDialSticks.modelMatrix[target.x, target.y].TargetTextureChangeDuration = duration;
		expanDialSticks.modelMatrix[target.x, target.y].TargetProjectorRotation = aiguilleRotation;
		expanDialSticks.modelMatrix[target.x, target.y].TargetProjectorChangeDuration = duration;
	}


	void Update()
	{
		// check if ExpanDialSticks is connected
		if (connected && !unknownParticipant)
		{
			currTime = Time.time;

			if(triggerNextTrial == true)
			{
				triggerNextTrial = false;
				StartCoroutine(TriggerTrial());
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
				if (gaugeState == BEGIN_TARGET_START || gaugeState == BEGIN_TARGET_TRIGGER || gaugeState == BEGIN_TARGET_STOP)
				{
					HandleRotationChanged(new object(), new ExpanDialStickEventArgs(DateTime.Now, currTargetPair.Item1.x, currTargetPair.Item1.y, 0, 1, 1));
				}
				if (gaugeState == END_TARGET_START || gaugeState == END_TARGET_TRIGGER || gaugeState == END_TARGET_STOP)
				{
					HandleRotationChanged(new object(), new ExpanDialStickEventArgs(DateTime.Now, currTargetPair.Item2.x, currTargetPair.Item2.y, 0, 1, 1));
				}
				//currentRotation += anglePerStep;
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				if (gaugeState == BEGIN_TARGET_START ||  gaugeState == BEGIN_TARGET_TRIGGER || gaugeState == BEGIN_TARGET_STOP)
				{
					HandleRotationChanged(new object(), new ExpanDialStickEventArgs(DateTime.Now, currTargetPair.Item1.x, currTargetPair.Item1.y, 1, 0, -1));
				}
				if (gaugeState == END_TARGET_START || gaugeState == END_TARGET_TRIGGER || gaugeState == END_TARGET_STOP)
				{
					HandleRotationChanged(new object(), new ExpanDialStickEventArgs(DateTime.Now, currTargetPair.Item2.x, currTargetPair.Item2.y, 1, 0, -1));
				}
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