
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

public class XP2 : MonoBehaviour
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
	private float DISTRACTOR_INTERVAL = 0.5f; // 0.2f;
	private const float COMPLETION_INTERVAL = 0.5f; // 0.2f;
	private float currTime;
	private float prevMetricsTime;
	private float prevDistractorTime;

	private const int targetPos = 40;
	private const int distractorPos = 40;
	private const float shortShapeChangeDuration = 2f;
	private const float longShapeChangeDuration = 4f;
	private const float safetyDistance = 6f;


	private string stringParticipant = "";
	private int numeroParticipant = 0;
	private bool unknownParticipant = true;
	private bool training = false;
	private bool triggerNextTrial = true;
	private bool MetricsActive = false;

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
	private float aiguilleRotation = 90f;
	private float cadranRotation = 90f;
	private float speedRotation = 10f;


	private const float anglePerStep = 360f / 24f;
	public enum DirectionRotation { CW, CCW, IDDLE };
	private DirectionRotation directionRotation = DirectionRotation.CW;

	// MQTT VARIABLES
	public const string MQTT_SYSTEM_RECORDER = "SYSTEM_RECORDER";
	public const string CMD_START = "START";
	public const string CMD_STOP = "STOP";

	private List<ExpanDialSticks.SafetyOverlayMode> overlayModes;
	private ExpanDialSticks.SafetyOverlayMode currOverlayMode = ExpanDialSticks.SafetyOverlayMode.User;

	private Vector2Int LEFT = new Vector2Int(-1, 0);
	private Vector2Int LEFT_UP = new Vector2Int(-1, -1);
	private Vector2Int UP = new Vector2Int(0, -1);
	private Vector2Int RIGHT_UP = new Vector2Int(+1, -1);
	private Vector2Int RIGHT = new Vector2Int(+1, 0);
	private Vector2Int RIGHT_DOWN = new Vector2Int(+1, +1);
	private Vector2Int DOWN = new Vector2Int(0, +1);
	private Vector2Int LEFT_DOWN = new Vector2Int(-1, +1);



	public enum TaskMode { USER_INTERRUPT, SYSTEM_INTERRUPT };
	private TaskMode currTaskMode;

	private List<Vector2Int> candidates = new List<Vector2Int>();

	List<Tuple<Vector2Int, Vector2Int, List<Vector2Int>>> trials = new List<Tuple<Vector2Int, Vector2Int, List<Vector2Int>>>();
	private Tuple<Vector2Int, Vector2Int, List<Vector2Int>> currTrial;
	private int totalTrials;

	private Vector2Int prevSelectPosition;
	private Vector2Int currSelectPosition;

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
		currTaskMode = TaskMode.SYSTEM_INTERRUPT;
		candidates = new List<Vector2Int>();
		trials = new List<Tuple<Vector2Int, Vector2Int, List<Vector2Int>>>();
		prevSelectPosition = currSelectPosition = new Vector2Int(-1, -1);
		//GenerateTrials();
		MetricsActive = false;
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
		trials = new List<Tuple<Vector2Int, Vector2Int, List<Vector2Int>>>();
		// LEFT SIDE, DENSITY 1
		trials.Add(new Tuple<Vector2Int, Vector2Int, List<Vector2Int>>(new Vector2Int(4, 1), new Vector2Int(0, 1),new List<Vector2Int>
				{
								new Vector2Int(0,0),
								new Vector2Int(1,2),
								new Vector2Int(2,0),
								new Vector2Int(3,2),
								new Vector2Int(4,0)
				}
			));


		trials.Add(new Tuple<Vector2Int, Vector2Int, List<Vector2Int>>(new Vector2Int(4, 1), new Vector2Int(0, 1), new List<Vector2Int>
				{
								new Vector2Int(0,2),
								new Vector2Int(1,0),
								new Vector2Int(2,2),
								new Vector2Int(3,0),
								new Vector2Int(4,2)
				}
			));


		// RIGHT SIDE, DENSITY 1
		trials.Add(new Tuple<Vector2Int, Vector2Int, List<Vector2Int>>(new Vector2Int(4, 4), new Vector2Int(0, 4), new List<Vector2Int>
				{
								new Vector2Int(0,3),
								new Vector2Int(1,5),
								new Vector2Int(2,3),
								new Vector2Int(3,5),
								new Vector2Int(4,3)
				}
			));


		trials.Add(new Tuple<Vector2Int, Vector2Int, List<Vector2Int>>(new Vector2Int(4, 4), new Vector2Int(0, 4), new List<Vector2Int>
				{
								new Vector2Int(0,5),
								new Vector2Int(1,3),
								new Vector2Int(2,5),
								new Vector2Int(3,3),
								new Vector2Int(4,5)
				}
			));


		// LEFT SIDE, DENSITY 2
		trials.Add(new Tuple<Vector2Int, Vector2Int, List<Vector2Int>>(new Vector2Int(4, 1), new Vector2Int(0, 1), new List<Vector2Int>
				{
								new Vector2Int(0,0),
								new Vector2Int(0,1),
								new Vector2Int(1,0),
								new Vector2Int(1,2),
								new Vector2Int(2,1),
								new Vector2Int(2,2),
								new Vector2Int(3,0),
								new Vector2Int(3,2),
								new Vector2Int(4,0),
								new Vector2Int(4,1)
				}
			));

		trials.Add(new Tuple<Vector2Int, Vector2Int, List<Vector2Int>>(new Vector2Int(4, 1), new Vector2Int(0, 1), new List<Vector2Int>
				{
								new Vector2Int(0,1),
								new Vector2Int(0,2),
								new Vector2Int(1,0),
								new Vector2Int(1,2),
								new Vector2Int(2,0),
								new Vector2Int(2,1),
								new Vector2Int(3,0),
								new Vector2Int(3,2),
								new Vector2Int(4,1),
								new Vector2Int(4,2)
				}
			));

		// RIGHT SIDE, DENSITY 2
		trials.Add(new Tuple<Vector2Int, Vector2Int, List<Vector2Int>>(new Vector2Int(4, 4), new Vector2Int(0, 4), new List<Vector2Int>
				{
								new Vector2Int(0,3),
								new Vector2Int(0,4),
								new Vector2Int(1,3),
								new Vector2Int(1,5),
								new Vector2Int(2,4),
								new Vector2Int(2,5),
								new Vector2Int(3,3),
								new Vector2Int(3,5),
								new Vector2Int(4,3),
								new Vector2Int(4,4)
				}
			));

		trials.Add(new Tuple<Vector2Int, Vector2Int, List<Vector2Int>>(new Vector2Int(4, 4), new Vector2Int(0, 4), new List<Vector2Int>
				{
								new Vector2Int(0,4),
								new Vector2Int(0,5),
								new Vector2Int(1,3),
								new Vector2Int(1,5),
								new Vector2Int(2,3),
								new Vector2Int(2,4),
								new Vector2Int(3,3),
								new Vector2Int(3,5),
								new Vector2Int(4,4),
								new Vector2Int(4,5)
				}
			));

		// LEFT SIDE, DENSITY 2
		trials.Add(new Tuple<Vector2Int, Vector2Int, List<Vector2Int>>(new Vector2Int(4, 1), new Vector2Int(0, 1), new List<Vector2Int>
				{
								new Vector2Int(0,0),
								new Vector2Int(0,1),
								new Vector2Int(1,0),
								new Vector2Int(1,2),
								new Vector2Int(2,1),
								new Vector2Int(2,2),
								new Vector2Int(3,0),
								new Vector2Int(3,2),
								new Vector2Int(4,0),
								new Vector2Int(4,1)
				}
			));

		trials.Add(new Tuple<Vector2Int, Vector2Int, List<Vector2Int>>(new Vector2Int(4, 1), new Vector2Int(0, 1), new List<Vector2Int>
				{
								new Vector2Int(0,1),
								new Vector2Int(0,2),
								new Vector2Int(1,0),
								new Vector2Int(1,2),
								new Vector2Int(2,0),
								new Vector2Int(2,1),
								new Vector2Int(3,0),
								new Vector2Int(3,2),
								new Vector2Int(4,1),
								new Vector2Int(4,2)
				}
			));

		// LEFT SIDE, DENSITY 3
		trials.Add(new Tuple<Vector2Int, Vector2Int, List<Vector2Int>>(new Vector2Int(4, 4), new Vector2Int(0, 4), new List<Vector2Int>
				{
								new Vector2Int(0,3),
								new Vector2Int(0,4),
								new Vector2Int(0,5),
								new Vector2Int(1,3),
								new Vector2Int(1,4),
								new Vector2Int(1,5),
								new Vector2Int(2,3),
								new Vector2Int(2,4),
								new Vector2Int(2,5),
								new Vector2Int(3,3),
								new Vector2Int(3,4),
								new Vector2Int(3,5),
								new Vector2Int(4,3),
								new Vector2Int(4,4),
								new Vector2Int(4,5)
				}
			));

		trials.Add(new Tuple<Vector2Int, Vector2Int, List<Vector2Int>>(new Vector2Int(4, 4), new Vector2Int(0, 4), new List<Vector2Int>
				{
								new Vector2Int(0,3),
								new Vector2Int(0,4),
								new Vector2Int(0,5),
								new Vector2Int(1,3),
								new Vector2Int(1,4),
								new Vector2Int(1,5),
								new Vector2Int(2,3),
								new Vector2Int(2,4),
								new Vector2Int(2,5),
								new Vector2Int(3,3),
								new Vector2Int(3,4),
								new Vector2Int(3,5),
								new Vector2Int(4,3),
								new Vector2Int(4,4),
								new Vector2Int(4,5)
				}
			));

		// RIGHT SIDE, DENSITY 3
		trials.Add(new Tuple<Vector2Int, Vector2Int, List<Vector2Int>>(new Vector2Int(4, 1), new Vector2Int(0, 1), new List<Vector2Int>
				{
								new Vector2Int(0,0),
								new Vector2Int(0,1),
								new Vector2Int(0,2),
								new Vector2Int(1,0),
								new Vector2Int(1,1),
								new Vector2Int(1,2),
								new Vector2Int(2,0),
								new Vector2Int(2,1),
								new Vector2Int(2,2),
								new Vector2Int(3,0),
								new Vector2Int(3,1),
								new Vector2Int(3,2),
								new Vector2Int(4,0),
								new Vector2Int(4,1),
								new Vector2Int(4,2)
				}
			));

		trials.Add(new Tuple<Vector2Int, Vector2Int, List<Vector2Int>>(new Vector2Int(4, 1), new Vector2Int(0, 1), new List<Vector2Int>
				{
								new Vector2Int(0,0),
								new Vector2Int(0,1),
								new Vector2Int(0,2),
								new Vector2Int(1,0),
								new Vector2Int(1,1),
								new Vector2Int(1,2),
								new Vector2Int(2,0),
								new Vector2Int(2,1),
								new Vector2Int(2,2),
								new Vector2Int(3,0),
								new Vector2Int(3,1),
								new Vector2Int(3,2),
								new Vector2Int(4,0),
								new Vector2Int(4,1),
								new Vector2Int(4,2)
				}
			));

		totalTrials = trials.Count();
		ListExtension.Shuffle(trials);
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
		if (currSelectPosition == currTrial.Item1)
		{
			if (gaugeState == BEGIN_TARGET_START)
			{
				gaugeState = BEGIN_TARGET_TRIGGER;
				prevDistractorTime = currTime;
				DISTRACTOR_INTERVAL = Random.Range(5f, 10f);
			}
			float prevRotation = aiguilleRotation;
			aiguilleRotation -= e.diff * anglePerStep;
			aiguilleRotation = (aiguilleRotation + 360f) % 360f;
			string msg = "USER_ROTATION " + currSelectPosition.ToString() + " CADRAN " + cadranRotation + " AIGUILLE " + aiguilleRotation;
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
		if (currSelectPosition == currTrial.Item2)
		{
			if (gaugeState == END_TARGET_START)
			{
				gaugeState = END_TARGET_TRIGGER;
				prevDistractorTime = currTime;
				DISTRACTOR_INTERVAL = Random.Range(5f, 10f);
			}
			float prevRotation = aiguilleRotation;
			aiguilleRotation -= e.diff * anglePerStep;
			aiguilleRotation = (aiguilleRotation + 360f) % 360f;
			string msg = "USER_ROTATION " + currSelectPosition.ToString() + " CADRAN " + cadranRotation + " AIGUILLE " + aiguilleRotation;
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

			if (GUI.Button(new Rect(midX + 5, midY - 75, componentWidth, componentHeight), "USER Interrupt TRAINING"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.User;
				currTaskMode = TaskMode.USER_INTERRUPT;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | USER INTERRUPT TRAINING";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				GenerateTrials();
				training = true;
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY - 50, componentWidth, componentHeight), "USER Overlay | USER Interrupt"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.User;
				currTaskMode = TaskMode.USER_INTERRUPT;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | USER OVERLAY | USER INTERRUPT";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				GenerateTrials();
				training = false;
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY - 25, componentWidth, componentHeight), "SYSTEM Overlay | USER Interrupt"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.System;
				currTaskMode = TaskMode.USER_INTERRUPT;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | SYSTEM OVERLAY | USER INTERRUPT";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				GenerateTrials();
				training = false;
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY, componentWidth, componentHeight), "SYSTEM Interrupt TRAINING"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.User;
				currTaskMode = TaskMode.SYSTEM_INTERRUPT;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | SYSTEM INTERRUPT TRAINING";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				GenerateTrials();
				training = true;
				unknownParticipant = false;
			}


			if (GUI.Button(new Rect(midX + 5, midY + 25, componentWidth, componentHeight), "USER Overlay | SYSTEM Interrupt"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.User;
				currTaskMode = TaskMode.SYSTEM_INTERRUPT;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | USER OVERLAY | SYSTEM INTERRUPT";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				GenerateTrials();
				training = false;
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY + 50, componentWidth, componentHeight), "SYSTEM Overlay | SYSTEM Interrupt"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.System;
				currTaskMode = TaskMode.SYSTEM_INTERRUPT;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | SYSTEM OVERLAY | SYSTEM INTERRUPT";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				GenerateTrials();
				training = false;
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

				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontTexture = "projector";
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontRotation = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontChangeDuration = shapeChangeDuration;

				expanDialSticks.modelMatrix[i, j].TargetProjectorBackTexture = "projector";
				expanDialSticks.modelMatrix[i, j].TargetProjectorBackRotation = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorBackSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorBackChangeDuration = shapeChangeDuration;

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
		for (int i = target.x - 1; i <= target.x + 1; i++)
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

				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontTexture = "projector";
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontRotation = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontChangeDuration = duration;

				expanDialSticks.modelMatrix[i, j].TargetProjectorBackTexture = "projector";
				expanDialSticks.modelMatrix[i, j].TargetProjectorBackRotation = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorBackSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorBackChangeDuration = duration;

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

	private IEnumerator TriggerUserInterruptTask()
	{
		MetricsActive = false;
		// Fetch model
		int trialIndex = totalTrials - trials.Count();
		currTrial = trials.First();
		trials.RemoveAt(0);
		Vector2Int secondTarget = currTrial.Item2;
		List<Vector2Int> distractorList = currTrial.Item3;
		string startTrialMsg = "TRIAL_START " + trialIndex;
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(startTrialMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);


		// Reset Texture and Projector
		PrepareResetTextureAndProjector(0.1f);
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerProjectorChange();
		yield return new WaitForSeconds(0.1f);

		// Move up the target
		expanDialSticks.modelMatrix[secondTarget.x, secondTarget.y].TargetPosition = targetPos;
		expanDialSticks.modelMatrix[secondTarget.x, secondTarget.y].TargetShapeChangeDuration = shortShapeChangeDuration;
		expanDialSticks.triggerShapeChange();
		yield return new WaitForSeconds(shortShapeChangeDuration);

		// wait for target shape-change completion
		bool shapeChangeEnded = false;
		while (!shapeChangeEnded)
		{
			shapeChangeEnded = true;
			Debug.Log("waiting for shape-change to complete...");

			if (expanDialSticks.viewMatrix[secondTarget.x, secondTarget.y].CurrentReaching ||
					expanDialSticks.viewMatrix[secondTarget.x, secondTarget.y].CurrentPosition < targetPos - 1 ||
					expanDialSticks.viewMatrix[secondTarget.x, secondTarget.y].CurrentPosition > targetPos + 1) shapeChangeEnded = false;
			yield return new WaitForSeconds(COMPLETION_INTERVAL);
		}
		string targetMsg = "TARGET " + secondTarget.ToString();
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(targetMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

		// Trigger distractors
		if (!training)
		{
			String shapeChangeMsg = "SYSTEM_TRIGGER_SHAPE_CHANGE";
			foreach (Vector2Int distractor in distractorList)
			{
				shapeChangeMsg += " " + new Vector3Int(distractor.x, distractor.y, distractorPos).ToString();
				expanDialSticks.modelMatrix[distractor.x, distractor.y].TargetPosition = distractorPos;
				expanDialSticks.modelMatrix[distractor.x, distractor.y].TargetShapeChangeDuration = longShapeChangeDuration;
			}
			expanDialSticks.triggerShapeChange();
			expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(shapeChangeMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
			MetricsActive = true;
			yield return new WaitForSeconds(0.25f);
		}


		// Texture First Target
		// 24 position
		cadranRotation = Random.Range(0, 23) * anglePerStep;
		aiguilleRotation = cadranRotation + (-1 + Random.Range(0, 1) * 2) * anglePerStep * Random.Range(6, 12);
		directionRotation = aiguilleRotation > cadranRotation ? DirectionRotation.CCW : DirectionRotation.CW;
		ShowGaugeOnTarget(secondTarget, 0.1f);
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerProjectorChange();
		yield return new WaitForSeconds(0.1f);
		string taskStartMsg = "SYSTEM_TASK_START " + secondTarget.ToString() + " CADRAN " + cadranRotation + " AIGUILLE " + aiguilleRotation;
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(taskStartMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

		gaugeState = BEGIN_TARGET_START;
		// wait for start signal
		while (gaugeState != BEGIN_TARGET_STOP)
		{
			if (gaugeState == BEGIN_TARGET_TRIGGER)
			{
				if (aiguilleRotation >= (cadranRotation - anglePerStep / 2f) % 360f && aiguilleRotation <= (cadranRotation + anglePerStep / 2f) % 360f)
				{
					gaugeState = BEGIN_TARGET_STOP;
				}
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
				expanDialSticks.triggerProjectorChange();
			}
			yield return new WaitForSeconds(0.1f);
		}
		string taskEndMsg = "SYSTEM_TASK_END " + secondTarget.ToString() + " CADRAN " + cadranRotation + " AIGUILLE " + aiguilleRotation;
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(taskEndMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

		// wait for shape-change completion
		shapeChangeEnded = false;
		if (!training)
		{
			while (!shapeChangeEnded)
			{
				shapeChangeEnded = true;
				Debug.Log("waiting for shape-change to complete...");

				foreach (Vector2Int distractor in distractorList)
				{

					if (expanDialSticks.viewMatrix[distractor.x, distractor.y].CurrentReaching ||
						expanDialSticks.viewMatrix[distractor.x, distractor.y].CurrentPosition < distractorPos - 1 ||
						expanDialSticks.viewMatrix[distractor.x, distractor.y].CurrentPosition > distractorPos + 1) shapeChangeEnded = false;
				}
				yield return new WaitForSeconds(COMPLETION_INTERVAL);
			}
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
		string endTrialMsg = "TRIAL_END " + trialIndex;
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(endTrialMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		triggerNextTrial = true;
	}

	private IEnumerator TriggerSystemInterruptTask()
	{
		MetricsActive = false;
		String msg;
		// Fetch model
		int trialIndex = totalTrials - trials.Count();
		currTrial = trials.First();
		trials.RemoveAt(0);
		Vector2Int secondTarget = currTrial.Item2;
		List<Vector2Int> distractorList = currTrial.Item3;
		msg = "TRIAL_START " + trialIndex;
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);


		// Reset Texture and Projector
		PrepareResetTextureAndProjector(0.1f);
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerProjectorChange();
		yield return new WaitForSeconds(0.1f);

		// Move up the target
		expanDialSticks.modelMatrix[secondTarget.x, secondTarget.y].TargetPosition = targetPos;
		expanDialSticks.modelMatrix[secondTarget.x, secondTarget.y].TargetShapeChangeDuration = shortShapeChangeDuration;
		expanDialSticks.triggerShapeChange();
		yield return new WaitForSeconds(shortShapeChangeDuration);

		// wait for target shape-change completion
		bool shapeChangeEnded = false;
		while (!shapeChangeEnded)
		{
			shapeChangeEnded = true;
			Debug.Log("waiting for shape-change to complete...");

			if (expanDialSticks.viewMatrix[secondTarget.x, secondTarget.y].CurrentReaching ||
					expanDialSticks.viewMatrix[secondTarget.x, secondTarget.y].CurrentPosition < targetPos - 1 ||
					expanDialSticks.viewMatrix[secondTarget.x, secondTarget.y].CurrentPosition > targetPos + 1) shapeChangeEnded = false;
			yield return new WaitForSeconds(COMPLETION_INTERVAL);
		}

		msg = "TARGET " + secondTarget.ToString();
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);



		// Texture First Target
		// 24 position
		cadranRotation = Random.Range(0, 23) * anglePerStep;
		aiguilleRotation = cadranRotation + (-1 + Random.Range(0, 1) * 2) * anglePerStep;
		ShowGaugeOnTarget(secondTarget, 0.1f);
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerProjectorChange();
		yield return new WaitForSeconds(0.1f);
		msg = "USER_TASK_START " + secondTarget.ToString() + " CADRAN " + cadranRotation + " AIGUILLE " + aiguilleRotation;
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

		gaugeState = BEGIN_TARGET_START;
		ListExtension.Shuffle(distractorList);
		int distractorIndex = 0;
		int distractorLength = distractorList.Count();

		// wait for start signal
		while (gaugeState != BEGIN_TARGET_STOP)
		{
			if(gaugeState == BEGIN_TARGET_TRIGGER)
			{
				if (distractorIndex < distractorLength && currTime - prevDistractorTime >= DISTRACTOR_INTERVAL)
				{
					// Trigger distractors
					Vector2Int distractor = distractorList[distractorIndex++];
					if (!training)
					{
						msg = "SYSTEM_TRIGGER_SHAPE_CHANGE";
						msg += " " + new Vector3Int(distractor.x, distractor.y, distractorPos).ToString();
						expanDialSticks.modelMatrix[distractor.x, distractor.y].TargetPosition = distractorPos;
						expanDialSticks.modelMatrix[distractor.x, distractor.y].TargetShapeChangeDuration = longShapeChangeDuration;
						expanDialSticks.triggerShapeChange();
						expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
						MetricsActive = true;
					}
					if (distractorIndex >= distractorLength) DISTRACTOR_INTERVAL = 5f;
					else DISTRACTOR_INTERVAL = Random.Range(0.5f, 1.5f);
					prevDistractorTime = currTime;
				}

				//Debug.Log("BEGIN_TARGET: " + (cadranRotation - anglePerStep / 2f) + " <= " + aiguilleRotation + " <=  " + (cadranRotation + anglePerStep / 2f));
				if (distractorIndex >= distractorLength && currTime - prevDistractorTime >= DISTRACTOR_INTERVAL)
				{
					if (aiguilleRotation >= (cadranRotation - anglePerStep / 2f) % 360f && aiguilleRotation <= (cadranRotation + anglePerStep / 2f) % 360f)
					{
						gaugeState = BEGIN_TARGET_STOP;
					}

				}
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
				expanDialSticks.triggerProjectorChange();
			}

			yield return new WaitForSeconds(0.1f);
		}

		msg = "USER_TASK_END " + secondTarget.ToString() + " CADRAN " + cadranRotation + " AIGUILLE " + aiguilleRotation;
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);


		// Reset texture and projector
		PrepareResetTextureAndProjector(0.1f);
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerProjectorChange();
		yield return new WaitForSeconds(0.1f);


		// wait for shape-change completion
		shapeChangeEnded = false;

		if (!training)
		{
			while (!shapeChangeEnded)
			{
				shapeChangeEnded = true;
				Debug.Log("waiting for shape-change to complete...");

				foreach (Vector2Int distractor in distractorList)
				{

					if (expanDialSticks.viewMatrix[distractor.x, distractor.y].CurrentReaching ||
						expanDialSticks.viewMatrix[distractor.x, distractor.y].CurrentPosition < distractorPos - 1 ||
						expanDialSticks.viewMatrix[distractor.x, distractor.y].CurrentPosition > distractorPos + 1) shapeChangeEnded = false;
				}
				yield return new WaitForSeconds(COMPLETION_INTERVAL);
			}
		}

		
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

		msg = "TRIAL_END " + trialIndex;
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		triggerNextTrial = true;
	}
	private void TriggerTrial()
	{
		Debug.Log("TriggerTrial()");
		if (trials.Count() > 0)
		{

			switch (currTaskMode)
			{
				case TaskMode.USER_INTERRUPT:
					StartCoroutine(TriggerUserInterruptTask());
					break;
				case TaskMode.SYSTEM_INTERRUPT:
					StartCoroutine(TriggerSystemInterruptTask());
					break;
				default:
					break;
			}

			/*MetricsActive = false;
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
			// wait for start signal
			while (gaugeState != BEGIN_TARGET_STOP)
			{

				//Debug.Log("BEGIN_TARGET: " + (cadranRotation - anglePerStep / 2f) + " <= " + aiguilleRotation + " <=  " + (cadranRotation + anglePerStep / 2f));
				if (aiguilleRotation >= (cadranRotation - anglePerStep / 2f) % 360f && aiguilleRotation <= (cadranRotation + anglePerStep / 2f) % 360f)
				{

					// Trigger distractors
					String shapeChangeMsg = "SYSTEM_TRIGGER_SHAPE_CHANGE";
					foreach (Vector2Int distractor in currDistractors)
					{
						shapeChangeMsg += " " + new Vector3Int(distractor.x, distractor.y, distractorPos).ToString();
						expanDialSticks.modelMatrix[distractor.x, distractor.y].TargetPosition = distractorPos;
						expanDialSticks.modelMatrix[distractor.x, distractor.y].TargetShapeChangeDuration = longShapeChangeDuration;
					}
					expanDialSticks.triggerShapeChange();
					expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(shapeChangeMsg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

					MetricsActive = true;
					gaugeState = BEGIN_TARGET_STOP;
				}
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
				//Debug.Log("END_TARGET: " + (cadranRotation - anglePerStep / 2f) + " <= " + aiguilleRotation + " <=  " + (cadranRotation + anglePerStep / 2f));
				if (aiguilleRotation >= (cadranRotation - anglePerStep / 2f) % 360f && aiguilleRotation <= (cadranRotation + anglePerStep / 2f) % 360f)
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
						expanDialSticks.viewMatrix[distractor.x, distractor.y].CurrentPosition < distractorPos - 1 ||
						expanDialSticks.viewMatrix[distractor.x, distractor.y].CurrentPosition > distractorPos + 1) shapeChangeEnded = false;
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
			*/

		}
		else
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

					expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
					expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = 0f;
					expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetPlaneColor = Color.black;

					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontTexture = "LightCadran";
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontRotation = cadranRotation;
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontSize = 0.02f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontColor = Color.blue;

					expanDialSticks.modelMatrix[i, j].TargetProjectorBackTexture = "aiguille";
					expanDialSticks.modelMatrix[i, j].TargetProjectorBackRotation = aiguilleRotation;
					expanDialSticks.modelMatrix[i, j].TargetProjectorBackSize = 0.02f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorBackColor = Color.white;
				}

				else
				{
					expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
					expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = 0f;
					expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetPlaneColor = Color.black;

					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontTexture = "projector";
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontRotation = cadranRotation;
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontColor = Color.white;

					expanDialSticks.modelMatrix[i, j].TargetProjectorBackTexture = "projector";
					expanDialSticks.modelMatrix[i, j].TargetProjectorBackRotation = aiguilleRotation;
					expanDialSticks.modelMatrix[i, j].TargetProjectorBackSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorBackColor = Color.blue;
				}

				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontChangeDuration = duration;
				expanDialSticks.modelMatrix[i, j].TargetProjectorBackChangeDuration = duration;
			}
		}
		string participantNumber = "<pos=0%><b>P" + numeroParticipant + "</b>";
		string trialProgress = "<pos=90%><b>" + trials.Count() + "/" + totalTrials + "</b>";
		string legend = participantNumber + trialProgress;
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 0.1f, Color.white, legend, new Vector3(90f, -90f, 0f));
		expanDialSticks.setBorderBackground(Color.black);
	}

	void ShowDotOnTarget(Vector2Int target, float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.black; //Color.green;
				if (i == target.x && j == target.y)
				{

					expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
					expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = 0f;
					expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetPlaneColor = Color.black;

					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontTexture = "projector";
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontRotation = cadranRotation;
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontColor = Color.white;

					expanDialSticks.modelMatrix[i, j].TargetProjectorBackTexture = "dot";
					expanDialSticks.modelMatrix[i, j].TargetProjectorBackRotation = 0f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorBackSize = 0.02f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorBackColor = Color.white;
				}

				else
				{
					expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
					expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = 0f;
					expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetPlaneColor = Color.black;

					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontTexture = "projector";
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontRotation = cadranRotation;
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorFrontColor = Color.white;

					expanDialSticks.modelMatrix[i, j].TargetProjectorBackTexture = "projector";
					expanDialSticks.modelMatrix[i, j].TargetProjectorBackRotation = aiguilleRotation;
					expanDialSticks.modelMatrix[i, j].TargetProjectorBackSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorBackColor = Color.blue;
				}

				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
				expanDialSticks.modelMatrix[i, j].TargetProjectorFrontChangeDuration = duration;
				expanDialSticks.modelMatrix[i, j].TargetProjectorBackChangeDuration = duration;
			}
		}
		string participantNumber = "<pos=0%><b>P" + numeroParticipant + "</b>";
		string trialProgress = "<pos=90%><b>" + trials.Count() + "/" + totalTrials + "</b>";
		string legend = participantNumber + trialProgress;
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 0.1f, Color.white, legend, new Vector3(90f, -90f, 0f));
		expanDialSticks.setBorderBackground(Color.black);
	}


	void MoveAiguilleCadran(Vector2Int target, float duration)
	{
		expanDialSticks.modelMatrix[target.x, target.y].TargetProjectorBackRotation = aiguilleRotation;
		expanDialSticks.modelMatrix[target.x, target.y].TargetProjectorBackChangeDuration = duration;
		expanDialSticks.modelMatrix[target.x, target.y].TargetProjectorFrontRotation = cadranRotation;
		expanDialSticks.modelMatrix[target.x, target.y].TargetProjectorFrontChangeDuration = duration;
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
	void Update()
	{
		// check if ExpanDialSticks is connected
		if (connected && !unknownParticipant)
		{
			currTime = Time.time;

			if (triggerNextTrial == true)
			{
				triggerNextTrial = false;
				TriggerTrial();
			}
			if (MetricsActive && currTime - prevMetricsTime >= LOG_INTERVAL)
			{
				LogMetrics();
				prevMetricsTime = currTime;
			}
			/*if (currTime - prevRandomTextureTime >= 5f)
			{
				RandomColor();
				prevRandomTextureTime = currTime;
			}*/

			if (Input.GetKey("escape"))
			{
				Quit();
			}

			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				if (gaugeState == BEGIN_TARGET_START || gaugeState == BEGIN_TARGET_TRIGGER || gaugeState == BEGIN_TARGET_STOP)
				{
					HandleRotationChanged(new object(), new ExpanDialStickEventArgs(DateTime.Now, currTrial.Item1.x, currTrial.Item1.y, 0, 1, 1));
				}
				if (gaugeState == END_TARGET_START || gaugeState == END_TARGET_TRIGGER || gaugeState == END_TARGET_STOP)
				{
					HandleRotationChanged(new object(), new ExpanDialStickEventArgs(DateTime.Now, currTrial.Item2.x, currTrial.Item2.y, 0, 1, 1));
				}
				//currentRotation += anglePerStep;
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				if (gaugeState == BEGIN_TARGET_START || gaugeState == BEGIN_TARGET_TRIGGER || gaugeState == BEGIN_TARGET_STOP)
				{
					HandleRotationChanged(new object(), new ExpanDialStickEventArgs(DateTime.Now, currTrial.Item1.x, currTrial.Item1.y, 1, 0, -1));
				}
				if (gaugeState == END_TARGET_START || gaugeState == END_TARGET_TRIGGER || gaugeState == END_TARGET_STOP)
				{ 
					HandleRotationChanged(new object(), new ExpanDialStickEventArgs(DateTime.Now, currTrial.Item2.x, currTrial.Item2.y, 1, 0, -1));
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
				reachingString += (expanDialSticks.viewMatrix[i, j].CurrentReaching ? 1 : 0) + " ";
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