
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

	private const int minPos = 0;
	private const int maxPos = 20;
	private const int targetPos = 30;
	private const float shortShapeChangeDuration = 2f;
	private const float longShapeChangeDuration = 6f;
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
	public enum TaskMode { System, User };
	private TaskMode currTaskMode;

	private List<Vector2Int> candidates = new List<Vector2Int>();

	private List<Vector2Int> targets = new List<Vector2Int>();
	private int currTargetIndex;
	private Vector2Int currTarget;

	private Vector2Int prevSelectPosition;
	private Vector2Int currSelectPosition;


	public int nbTrials = 12;

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

		currOverlayMode = ExpanDialSticks.SafetyOverlayMode.User;
		currTaskMode = TaskMode.User;
		candidates = new List<Vector2Int>();
		targets = new List<Vector2Int>();
		currTargetIndex = -1;
		currTarget = new Vector2Int(-1, -1);
		prevSelectPosition = currSelectPosition = new Vector2Int(-1, -1);
		GenerateTrials();
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
		for (int i = 0; i < nbTrials; i++)
		{
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
			if(currTarget == currSelectPosition)
			{
				float prevRotation = aiguilleRotation;
				aiguilleRotation += e.diff * anglePerStep;
				string msg = "";
				msg += "USER_ROTATION " + prevRotation + " " + aiguilleRotation;
				Debug.Log(msg);
				if (gaugeState == GAUGE_APPEARED)
				{
					startGameTime = Time.time;
					motionDuration = Random.Range(5f, initGameDuration - 5f);
					gameDuration = initGameDuration;
					gaugeState = GAUGE_STARTED;
				}
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
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


			if (GUI.Button(new Rect(midX + 5, midY - 50, componentWidth, componentHeight), "USER Overlay | USER Interrupt"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.User;
				currTaskMode = TaskMode.User;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | USER OVERLAY | USER INTERRUPT";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				unknownParticipant = false;
			}
			if (GUI.Button(new Rect(midX + 5, midY - 25, componentWidth, componentHeight), "USER Overlay | SYSTEM Interrupt"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.User;
				currTaskMode = TaskMode.System;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | SYSTEM OVERLAY | SYSTEM INTERRUPT";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY, componentWidth, componentHeight), "SYSTEM Overlay | USER Interrupt"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.System;
				currTaskMode = TaskMode.User;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | SYSTEM OVERLAY | USER INTERRUPT";
				expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes(identity), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
				Debug.Log(identity);
				unknownParticipant = false;
			}

			if (GUI.Button(new Rect(midX + 5, midY + 25, componentWidth, componentHeight), "SYSTEM Overlay | SYSTEM Interrupt"))
			{
				numeroParticipant = int.Parse(stringParticipant);
				currOverlayMode = ExpanDialSticks.SafetyOverlayMode.System;
				currTaskMode = TaskMode.User;
				expanDialSticks.SetOverlayMode(currOverlayMode);
				string identity = "USER_IDENTITY " + numeroParticipant + " | SYSTEM OVERLAY | SYSTEM INTERRUPT";
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

				expanDialSticks.modelMatrix[i, j].TargetColor = Color.white;
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

	private IEnumerator TriggerSystemTask()
	{

		// Reset Shape
		PrepareResetShapeChange(shortShapeChangeDuration);
		expanDialSticks.triggerShapeChange();
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerProjectorChange();
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("SYSTEM_RESET_SHAPE"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		yield return new WaitForSeconds(shortShapeChangeDuration);

		// Shape Distractors
		List<Vector2Int> distractors = new List<Vector2Int>();
		for (int i = currTarget.x - 1; i <= currTarget.x + 1; i++)
		{
			for (int j = currTarget.y - 1; j <= currTarget.y + 1; j++)
			{
					distractors.Add(new Vector2Int(i, j));
					expanDialSticks.modelMatrix[i, j].TargetPosition = targetPos;
					expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = longShapeChangeDuration;
			}
		}
		expanDialSticks.triggerShapeChange();
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("SYSTEM_TRIGGER_SHAPE_CHANGE"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

		yield return new WaitForSeconds(longShapeChangeDuration);

		// Wait for Shape-Change to Complete
		bool shapeChangeCompleted = false;
		int count = 0;
		while (!shapeChangeCompleted)
		{
			DebugInSitu("Waiting for shape-change to complete(" + count + ")...", Color.black, Color.white);
			shapeChangeCompleted = true;
			foreach (Vector2Int distractor in distractors)
			{
				sbyte currDistractorPos = expanDialSticks.viewMatrix[distractor.x, distractor.y].CurrentPosition;
				//Debug.Log(distractor + " => " + currDistractorPos);
				if (currDistractorPos < targetPos - 1 || currDistractorPos > targetPos + 1) // cannot get exact position, add tolerance (problem with Arduino Driver)
				{
					shapeChangeCompleted = false;
				}
			}
			count++;
			yield return new WaitForSeconds(COMPLETION_INTERVAL);
		}
		PrepareResetShapeChange(shortShapeChangeDuration);
		expanDialSticks.triggerShapeChange();
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerProjectorChange();
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("SYSTEM_TRIGGER_UNSHAPE_CHANGE"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

		yield return new WaitForSeconds(shortShapeChangeDuration);
		triggerNextTrial = true;
	}
	private IEnumerator TriggerUserTask()
	{
		// Trigger Shape Reset
		PrepareResetShapeChange(shortShapeChangeDuration);
		expanDialSticks.triggerShapeChange();
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerProjectorChange();
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("SYSTEM_RESET_SHAPE"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

		yield return new WaitForSeconds(shortShapeChangeDuration);
		// Wait for completion
		bool wait = true;
		while (wait)
		{
			wait = false;
			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					if (expanDialSticks.viewMatrix[i, j].CurrentPosition != 0) wait = true;
				}
			}
			yield return new WaitForSeconds(COMPLETION_INTERVAL);
		}

		// Trigger Target Shape
		PrepareShapeChangeAtTarget(shortShapeChangeDuration);
		expanDialSticks.triggerShapeChange();
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("SYSTEM_SHAPE_TARGET"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		yield return new WaitForSeconds(shortShapeChangeDuration);
		// Wait for completion
		wait = true;
		while (wait)
		{
			wait = (expanDialSticks.viewMatrix[currTarget.x, currTarget.y].CurrentPosition < targetPos-1 || expanDialSticks.viewMatrix[currTarget.x, currTarget.y].CurrentPosition > targetPos + 1);
			yield return new WaitForSeconds(COMPLETION_INTERVAL);
		}

		cadranRotation = aiguilleRotation = startRotation;
		ShowGaugeOnTarget(0.1f);
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerProjectorChange();
		expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("SYSTEM_TEXTURE_TARGET"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
		yield return new WaitForSeconds(shortShapeChangeDuration);
		
		// Loop Until Start
		gaugeState = GAUGE_APPEARED;
		bool finished = false;
		// Game
		while (!finished) {

			if (gaugeState == GAUGE_STARTED) // User started task
			{
				if (Time.time - startGameTime >= gameDuration) // Game has ended
				{
					Debug.Log("Game has ended.");
					gaugeState = GAUGE_APPEARING;
					gameDuration = Mathf.Infinity;
					finished = true;
				}

				if (Time.time - startGameTime >= motionDuration) // Distractor trigger
				{
					//gaugeState = LANDSCAPE_IS_CHANGING;
					if (training)
					{
						//StartCoroutine(FakeEarthquake());
					}
					else
					{

						PrepareShapeChangeAroundTarget(shortShapeChangeDuration);
						expanDialSticks.triggerShapeChange();
						expanDialSticks.client.Publish(MQTT_SYSTEM_RECORDER, System.Text.Encoding.UTF8.GetBytes("SYSTEM_TRIGGER_SHAPE_CHANGE"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

					}
					motionDuration = Mathf.Infinity;
				}
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
				string msg = "SYSTEM_ROTATION " + prevRotation + " " + cadranRotation;
				if ((int)prevRotation != (int)cadranRotation)
				{
					//Debug.Log(msg);
				}
				if (Time.time - directionTime >= directionDuration)
				{
					Debug.Log("Distractor has been updated.");
					int nbDirections = Enum.GetNames(typeof(DirectionRotation)).Length - 1; // without IDDLE
					directionRotation = (DirectionRotation)UnityEngine.Random.Range(0, nbDirections);
					speedRotation = UnityEngine.Random.Range(5f, 15f);
					directionDuration = UnityEngine.Random.Range(3f, 9f);
					directionTime = Time.time;
				}
				MoveAiguilleCadran(0.1f);
				expanDialSticks.triggerTextureChange();
				expanDialSticks.triggerProjectorChange();
			}
			yield return new WaitForSeconds(0.1f);
		}
		// Wait for distractor completion
		wait = true;
		while (wait)
		{
			wait = false;
			for (int i = currTarget.x - 1; i <= currTarget.x + 1; i++)
			{
				for (int j = currTarget.y - 1; j <= currTarget.y + 1; j++)
				{
					if (currTarget.x != i || currTarget.y != j)
					{
						wait = (expanDialSticks.viewMatrix[i, j].CurrentPosition < targetPos - 1 || expanDialSticks.viewMatrix[i, j].CurrentPosition > targetPos + 1);

					}
				}
			}
			yield return new WaitForSeconds(COMPLETION_INTERVAL);
		}
		triggerNextTrial = true;

		PrepareResetShapeChange(shortShapeChangeDuration);
		expanDialSticks.triggerShapeChange();
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerProjectorChange();
		yield return new WaitForSeconds(shortShapeChangeDuration);
		// Wait for completion
		wait = true;
		while (wait)
		{
			wait = false;
			for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					if (expanDialSticks.viewMatrix[i, j].CurrentPosition != 0) wait = true;
				}
			}
			yield return new WaitForSeconds(COMPLETION_INTERVAL);
		}
		triggerNextTrial = true;

	}

	private void TriggerTarget()
	{
		Debug.Log("TriggerTarget()");
		currTargetIndex++;
		if(currTargetIndex < targets.Count())
		{
			// Select Target
			currTarget = targets[currTargetIndex];
			Debug.Log("currTargetPosition=>" + currTarget);
			// Random Shape Change + Target
			// Random Shape
			/*for (int i = 0; i < expanDialSticks.NbRows; i++)
			{
				for (int j = 0; j < expanDialSticks.NbColumns; j++)
				{
					// Shape
					if (i == currTarget.x && j == currTarget.y)
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
			Debug.Log("triggerShapeChange()");*/

			switch (currTaskMode)
			{
				case TaskMode.User:
					Debug.Log("USER INTERRUPTION");
					StartCoroutine(TriggerSystemTask());
					break;


				case TaskMode.System:
					Debug.Log("SYSTEM INTERRUPTION");
					StartCoroutine(TriggerUserTask());
					break;
				default:
					break;
			}

		} else
		{
			Quit();
		}
	}
	void AllBlack(float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
				expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = aiguilleRotation;

				expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "projector";
				expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = cadranRotation;
				expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 0f;
				expanDialSticks.modelMatrix[i, j].TargetProjectorColor = Color.white;
				expanDialSticks.modelMatrix[i, j].TargetProjectorChangeDuration = duration;

				expanDialSticks.modelMatrix[i, j].TargetColor = Color.black;
				expanDialSticks.modelMatrix[i, j].TargetTextureChangeDuration = duration;
			}
		}
		expanDialSticks.setBorderBackground(Color.black);
		expanDialSticks.triggerTextureChange();
	}
	bool IsGaugeUp()
	{
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		return !expanDialSticks.viewMatrix[(int)gaugePosition.x, (int)gaugePosition.y].CurrentReaching && expanDialSticks.viewMatrix[(int)gaugePosition.x, (int)gaugePosition.y].CurrentPosition >= gaugeHeight -1 && expanDialSticks.viewMatrix[(int)gaugePosition.x, (int)gaugePosition.y].CurrentPosition <= gaugeHeight + 1;
	
	}

	void GaugeUp(float duration)
	{
		Vector2 gaugePosition = gaugePositions[gaugeIndex];
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{

				if (i == (int)gaugePosition.x && j == (int)gaugePosition.y)
				{
					//Debug.Log("GaugeUp -> gauge Index: " + gaugePosition + " (" + (count++) + ")");
					expanDialSticks.modelMatrix[i, j].TargetPosition = gaugeHeight;
				}
				else
					expanDialSticks.modelMatrix[i, j].TargetPosition = 0;
				expanDialSticks.modelMatrix[i, j].TargetShapeChangeDuration = duration;
			}
		}
		expanDialSticks.triggerShapeChange();
	}


	void ShowGaugeOnTarget(float duration)
	{
		for (int i = 0; i < expanDialSticks.NbRows; i++)
		{
			for (int j = 0; j < expanDialSticks.NbColumns; j++)
			{
				expanDialSticks.modelMatrix[i, j].TargetColor = Color.white; //Color.green;
				if (i == currTarget.x && j == currTarget.y)
				{
					expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "LightCadran";
					expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = cadranRotation;
					expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0.6f;
					expanDialSticks.modelMatrix[i, j].TargetPlaneColor = Color.red;

					expanDialSticks.modelMatrix[i, j].TargetProjectorTexture = "aiguille";
					expanDialSticks.modelMatrix[i, j].TargetProjectorRotation = aiguilleRotation;
					expanDialSticks.modelMatrix[i, j].TargetProjectorSize = 0.02f;
					expanDialSticks.modelMatrix[i, j].TargetProjectorColor = Color.black;
				}

				else
				{
					expanDialSticks.modelMatrix[i, j].TargetPlaneTexture = "default";
					expanDialSticks.modelMatrix[i, j].TargetPlaneRotation = cadranRotation;
					expanDialSticks.modelMatrix[i, j].TargetPlaneSize = 0f;
					expanDialSticks.modelMatrix[i, j].TargetPlaneColor = Color.white;

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
				TriggerTarget();
				triggerNextTrial = false;
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
				reachingString += expanDialSticks.viewMatrix[i, j].CurrentReaching + " ";
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