﻿#define DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using TMPro;
using System;
using Leap.Unity;

public class MqttConnectionEventArgs : EventArgs
{
	public IPAddress address;
	public int port;
	public MqttConnectionEventArgs(IPAddress address, int port){
		this.address = address;
		this.port = port;
	}
}

public class ExpanDialStickEventArgs : EventArgs
{
	public DateTime t;
	public int i, j;
	public float prev;
	public float next;
	public float diff;
	public ExpanDialStickEventArgs(DateTime t, int i, int j, float prev, float next, float diff){
		this.t = t;
		this.i = i;
		this.j = j;
		this.prev = prev;
		this.next = next;
		this.diff = diff;
	}
}

[Serializable]
public class GetRequest
{
	public byte[] GET;

}

[Serializable]
public class GetAns
{
	[Serializable]
	public class Content
	{
		public sbyte[] xAxisValue;
		public sbyte[] yAxisValue;
		public byte[] selectCountValue;
		public sbyte[] rotationValue;
		public sbyte[] positionValue;
		public byte[] reachingValue;
		public byte[] holdingValue;
	}

	[Serializable]
	public class Answer
	{
		public string status;
		public Content content;
	}

	public byte[] GET;
	public Answer ANS;


}

[Serializable]
public class SetRequest
{
	[Serializable]
	public class Content
	{
		public int[] position;
		public float[] duration;
		public int[] holding;
	}

	public Content SET;

	public SetRequest()
	{
		SET = new Content();
	}
}

[Serializable]
public class SetAns
{
	[Serializable]
	public class Content
	{
		public int[] position;
		public float[] duration;
		public int[] holding;
	}

	[Serializable]
	public class Answer
	{
		public string status;
		public int[] content;
	}

	public Content SET;
	public Answer ANS;


}

public class ExpanDialSticks : MonoBehaviour
{
	public SafeGuard safeGuard;
	public bool safeGuardOn = true;
	public Material borderMaterial;
	public bool SIMULATION = true;
	public float diameter = 0.06f; // divide by 100
	public float height = 0.1f;  // divide by 100
	public float offset = 0.001f;  // divide by 100
	public float borderOffset = 2.0f;

	//public GameObject capsuleHandLeftPrefab;
	//public GameObject capsuleHandRightPrefab;
	public MyCapsuleHand leftHand;
	public MyCapsuleHand rightHand;
	public enum SafetyMotionMode {SafetyRatedMonitoredStop, SpeedAndSeparationMonitoring};
	public SafetyMotionMode safetyMotionMode = SafetyMotionMode.SafetyRatedMonitoredStop;
	public enum SafetyOverlayMode {None, Edge, Fill, Hull, Zone};
	public SafetyOverlayMode safetyOverlayMode = SafetyOverlayMode.Edge;
	private int nbSeparationLevels = 1;


	public const float JOYSTICK_THRESHOLD = 10f;

	public const string MQTT_BAD_VALUES = "bad values";
	public const string MQTT_WRONG_LENGTH = "wrong length";
	public const string MQTT_MISSING_KEY = "missing key";
	public const string MQTT_UNKNOWN_CMD = "unknown command";
	public const string MQTT_VALUE_ERROR = "json value error";
	public const string MQTT_SUCCESS = "success";

	public IPAddress EXPANDIALSTICKS_BROKER_ADDRESS = IPAddress.Parse("192.168.0.10"); // "test.mosquitto.org";
	public IPAddress LOCALHOST_BROKER_ADDRESS = IPAddress.Parse("127.0.0.1"); // "test.mosquitto.org";
	private IPAddress BROKER_ADDRESS; // "test.mosquitto.org";

	public int BROKER_PORT = 1883; // 8080; 
	public string MQTT_TOPIC = "ExpanDialSticks";
	public float MQTT_DELAY_RECONNECT = 5f; // 0.2f;
	public float MQTT_DELAY_AT_START = 2f; // 0.2f;
	public float MQTT_INTERVAL = 0.25f; // 0.2f;
	public float EVENT_INTERVAL = 0.25f; // 0.2f;
	public const int nbColumns = 6;
	public const int nbRows = 5;
	float LeapMotionDistanceFromMatrix = 0.4f;
	float SARCameraDistanceFromMatrix = 0.7f;
	private const float maxSpeed = 20f; // 40f; 20f; 13.33f; // pos/seconds 1s, 2s, 3s



	public GameObject expanDialStickPrefab;

	public GameObject leapMotionObject;
	public Camera SARCamera;

	public MqttClient client;

	public GameObject[,] gameObjectMatrix = new GameObject[nbRows, nbColumns];
	public ExpanDialStickCollision[,] collisionMatrix = new ExpanDialStickCollision [nbRows, nbColumns];
	public ExpanDialStickView[,] viewMatrix = new ExpanDialStickView[nbRows, nbColumns];
	public ExpanDialStickModel[,] modelMatrix = new ExpanDialStickModel[nbRows, nbColumns];

    private GameObject topBorderText;
	private Vector3 textRotationTop;
	private TextMeshPro textMeshTop;
	private TextAlignmentOptions textAlignmentTop;
	private int textSizeTop;
	private Color textColorTop;
	private string textTop;
	private GameObject topBorderBackground;
	private Color backgroundColorTop;

    private GameObject bottomBorderText;
	private Vector3 textRotationBottom;
	private TextMeshPro textMeshBottom;
	private TextAlignmentOptions textAlignmentBottom;
	private int textSizeBottom;
	private Color textColorBottom;
	private string textBottom;
	private GameObject bottomBorderBackground;
	private Color backgroundColorBottom;

	private GameObject leftBorderText;
	private Vector3 textRotationLeft;
	private TextMeshPro textMeshLeft;
	private TextAlignmentOptions textAlignmentLeft;
	private int textSizeLeft;
	private Color textColorLeft;
	private string textLeft;
	private GameObject leftBorderBackground;
	private Color backgroundColorLeft;

	private GameObject rightBorderText;
	private Vector3 textRotationRight;
	private TextMeshPro textMeshRight;
	private TextAlignmentOptions textAlignmentRight;
	private int textSizeRight;
	private Color textColorRight;
	private string textRight;
	private GameObject rightBorderBackground;
	private Color backgroundColorRight;


	private GameObject rightCornerText;
	private Vector3 textRotationRightCorner;
	private TextMeshPro textMeshRightCorner;
	private TextAlignmentOptions textAlignmentRightCorner;
	private int textSizeRightCorner;
	private Color textColorRightCorner;
	private string textRightCorner;
	private GameObject rightCornerBackground;
	private Color backgroundColorCornerRight;



	private GameObject leftCornerText;
	private Vector3 textRotationLeftCorner;
	private TextMeshPro textMeshLeftCorner;
	private TextAlignmentOptions textAlignmentLeftCorner;
	private int textSizeLeftCorner;
	private Color textColorLeftCorner;
	private string textLeftCorner;
	private GameObject leftCornerBackground;
	private Color backgroundColorCornerLeft;

	private bool shapeChanging = false;
	private bool textureChanging = false;
	private bool projectorChanging = false;
	private bool safetyChanging = false;

	// Shape Change
	/*int[] positions = new int[nbRows * nbColumns];
	int[] holdings = new int[nbRows * nbColumns];
	float[] shapeChangeDurations = new float[nbRows * nbColumns];
	bool[] shapeChanging = new bool[nbRows * nbColumns];*/
	// Color Change
	/*Color[] colors = new Color[nbRows * nbColumns];
	float[] colorChangeDurations = new float[nbRows * nbColumns];
	bool[] colorChanging = new bool[nbRows * nbColumns];*/
	// Text Change
	TextAlignmentOptions[] textAlignments = new TextAlignmentOptions[nbRows * nbColumns];
	Color[] textColors = new Color[nbRows * nbColumns];
	float[] textSizes = new float[nbRows * nbColumns];
	string[] texts = new string[nbRows * nbColumns];
	bool[] textChanging = new bool[nbRows * nbColumns];

	// Create EventHandlers
	public event EventHandler<MqttConnectionEventArgs> OnConnecting = (sender, e) => {};
	public event EventHandler<MqttConnectionEventArgs> OnConnected = (sender, e) => {};
	public event EventHandler<MqttConnectionEventArgs> OnDisconnected = (sender, e) => {};
	public event EventHandler<ExpanDialStickEventArgs> OnXAxisChanged = (sender, e) => {};
	public event EventHandler<ExpanDialStickEventArgs> OnYAxisChanged = (sender, e) => {};
	public event EventHandler<ExpanDialStickEventArgs> OnClickChanged = (sender, e) => {};
	public event EventHandler<ExpanDialStickEventArgs> OnRotationChanged = (sender, e) => {};
	public event EventHandler<ExpanDialStickEventArgs> OnPositionChanged = (sender, e) => {};
	public event EventHandler<ExpanDialStickEventArgs> onHoldingChanged = (sender, e) => {};
	public event EventHandler<ExpanDialStickEventArgs> onReachingChanged = (sender, e) => {};
	public event EventHandler<ExpanDialStickEventArgs> onProximityChanged = (sender, e) => { };
	public event EventHandler<ExpanDialStickEventArgs> onSeparationLevelChanged = (sender, e) => { };

	public int NbRows{
		get => nbRows;
	}

	public int NbColumns{
		get => nbColumns;
	}
	public float DistanceFromSARCamera
	{
		get => SARCameraDistanceFromMatrix;
	}
	public float DistanceFromLMCamera
	{
		get => LeapMotionDistanceFromMatrix;
	}
	public float BorderOffset
	{
		get => borderOffset;
	}

	public SafetyMotionMode getSafetyMode()
	{
		return safetyMotionMode;
	}

	public void SetSafetyMode(SafetyMotionMode motionMode)
	{
		safetyMotionMode = motionMode;
		switch (safetyMotionMode)
		{
			case SafetyMotionMode.SafetyRatedMonitoredStop:
				nbSeparationLevels = 2;
				break;
			case SafetyMotionMode.SpeedAndSeparationMonitoring:
				nbSeparationLevels = 4;
				break;
			default:
				break;
		}
		for (int i = 0; i < nbRows; i++)
			for (int j = 0; j < nbColumns; j++)
			{
				// view
				viewMatrix[i, j].NbSeparationLevels = nbSeparationLevels;

				// collision
				collisionMatrix[i, j].NbSeparationLevels = nbSeparationLevels;
			}
	}
	public SafetyOverlayMode getOverlayMode()
	{
		return safetyOverlayMode;
	}
	public void SetOverlayMode(SafetyOverlayMode overlayMode)
	{
		safetyOverlayMode = overlayMode;
		switch (safetyOverlayMode)
		{
			case SafetyOverlayMode.Edge:
				safeGuard.setOverlayMode(SafeGuard.SafetyOverlayMode.Dot, SafeGuard.SemioticMode.None, SafeGuard.FeedbackMode.State);

				break;
			case SafetyOverlayMode.Fill:
				safeGuard.setOverlayMode(SafeGuard.SafetyOverlayMode.Surface, SafeGuard.SemioticMode.Icon, SafeGuard.FeedbackMode.Intent);
				break;
			case SafetyOverlayMode.Hull:
				safeGuard.setOverlayMode(SafeGuard.SafetyOverlayMode.Hull, SafeGuard.SemioticMode.Icon, SafeGuard.FeedbackMode.Intent);
				break;
			case SafetyOverlayMode.Zone:
				safeGuard.setOverlayMode(SafeGuard.SafetyOverlayMode.Zone, SafeGuard.SemioticMode.Icon, SafeGuard.FeedbackMode.Intent);
				break;
			default:
				break;
		}
	}

	void OnGUI()
	{
		/*if (GUI.Button(new Rect(10, 10, 50, 50), buttonTexture))
		{
			for (int i = 0; i < nbRows; i++)
			{ 
				for (int j = 0; j < nbColumns; j++)
				{
					collisionMatrix[i, j].EnableCollision();
				}
			}
		}*/
	}
	// Use this for initialization
	void Start () {
		if(leftHand == null || rightHand == null)
		{
			Debug.Log("Could not found left and/or right hand capsule.");
			Quit();
		}
		shapeChanging = textureChanging = safetyChanging = false;
		/*for(int i = 0; i < nbRows * nbColumns; i++) {
			this.positions[i] = 0;
			this.holdings[i] = 0;
			this.shapeChangeDurations[i] = 0f;
			this.shapeChanging[i] = false;
			this.colors[i] = new Color();
			this.colorChangeDurations[i] = 0f;
			this.colorChanging[i] = false;
		}*/

		// Init ExpanDialSticks Model and View
		switch (safetyMotionMode)
		{
			case SafetyMotionMode.SafetyRatedMonitoredStop:
				nbSeparationLevels = 2;
				break;
			case SafetyMotionMode.SpeedAndSeparationMonitoring:
				nbSeparationLevels = 4;
				break;
			default:
				break;

		}
		for (int i = 0; i < nbRows; i++)
			for (int j = 0; j < nbColumns; j++)
			{
				
				// Model
				modelMatrix[i, j] = new ExpanDialStickModel();
				modelMatrix[i, j].Row = i;
				modelMatrix[i, j].Column = j;
				modelMatrix[i, j].Diameter = diameter;
				modelMatrix[i, j].Height = height;
				modelMatrix[i, j].Offset = offset;
				modelMatrix[i, j].Init = false;
				
				// GameObject
				gameObjectMatrix[i, j] = Instantiate(expanDialStickPrefab);
				gameObjectMatrix[i, j].transform.parent = this.transform;
				
				// view
				viewMatrix[i, j] = gameObjectMatrix[i, j].GetComponent<ExpanDialStickView>();
				viewMatrix[i, j].Row = i;
				viewMatrix[i, j].Column = j;
				viewMatrix[i, j].Diameter = diameter;
				viewMatrix[i, j].Height = height;
				viewMatrix[i, j].Offset = offset;
				viewMatrix[i, j].NbSeparationLevels = nbSeparationLevels;

				// collision
				collisionMatrix[i, j] = gameObjectMatrix[i, j].GetComponent<ExpanDialStickCollision>();
				collisionMatrix[i, j].Row = i;
				collisionMatrix[i, j].Column = j;
				collisionMatrix[i, j].Diameter = diameter;
				collisionMatrix[i, j].Height = height;
				collisionMatrix[i, j].Offset = offset;
				collisionMatrix[i, j].NbSeparationLevels = nbSeparationLevels;
				collisionMatrix[i, j].RightHand = rightHand;
				collisionMatrix[i, j].LeftHand = leftHand;
				//collisionMatrix[i, j].EnableCollision();
			}
		// Set camera
		//SARCamera = Camera.main;
		// Leap Motion Camera
		leapMotionObject.transform.localPosition = new Vector3( ((nbRows - 1) * diameter + offset)/2f + (diameter)/2f, LeapMotionDistanceFromMatrix, ((nbColumns - 1) * diameter + offset)/ 2f);
		leapMotionObject.transform.localScale = new Vector3(1.4f, 1f, 1.4f);
		// SAR Camera
		SARCamera.enabled = true;
		SARCamera.pixelRect = new Rect(0, 0, 1920, 1080);
		// (nbRows - 1) * (diameter + offset)
		Vector3 cameraPosition = new Vector3(-(diameter/2 + offset), SARCameraDistanceFromMatrix, (nbColumns - 1) * (diameter + offset) / 2);
		SARCamera.transform.localPosition = cameraPosition;

		Vector3 cameraLookAtPosition = cameraPosition - new Vector3(0f, SARCameraDistanceFromMatrix, 0f);
		SARCamera.transform.LookAt(cameraLookAtPosition);
		SARCamera.transform.eulerAngles += new Vector3(0f, 90f, 0f);

		// Safety Guardian Initialization

		/*SafeGuardGo.transform.position = new Vector3(((nbRows - 1) * (diameter + offset)) / 2f + (diameter - borderOffset)/2f, cameraDistanceFromMatrix, ((nbColumns - 1) * (diameter + offset))/2f);
		Vector3 safeCameraLookAtPosition = SafeGuardGo.transform.position - new Vector3(0f, cameraDistanceFromMatrix, 0f);
		SafeGuardGo.transform.LookAt(safeCameraLookAtPosition);
		SafeGuardGo.transform.eulerAngles += new Vector3(0f, 90f, 0f);
		Camera safeCamera = SafeGuardGo.GetComponent<Camera>();
		safeCamera.orthographic = true;
		safeCamera.orthographicSize = (nbRows * (diameter + offset) + 2 * offset + 2 * borderOffset + (diameter - borderOffset))/2f;
		// Add projector
		Projector safeGuardProjector = SafeGuardGo.GetComponent<Projector>();
		safeGuardProjector.orthographic = true;
		safeGuardProjector.orthographicSize = safeCamera.orthographicSize;*/

		/*Vector3 targetOrientationPosition = new Vector3(0, cameraDistanceFromMatrix, (nbColumns - 1) * (diameter + offset) / 2);
		Vector3 targetOrientationDir = targetOrientationPosition - cameraPosition;
		float zAngle = Vector3.Angle(targetOrientationDir, Vector3.up);
		SARCamera.transform.Rotate(0f, 0f, zAngle, Space.Self);
		SARCamera.transform.Rotate(0f, 0f, 180f, Space.Self);*/

		// Border Quads
		/*GameObject topBorderBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
		topBorderBackground.transform.LookAt(Vector3.down);
		topBorderBackground.transform.position = new Vector3(-(diameter + offset + borderOffset/2), height/2, ((nbColumns - 1) * (diameter + offset) / 2));
		topBorderText = Instantiate(topBorderBackground);
		topBorderBackground.transform.localScale = new Vector3(diameter - borderOffset, nbColumns * (diameter + offset) + 2*offset + 1.5f*borderOffset, 1f);
		topBorderText.transform.Rotate(new Vector3(0f, 0f, 90f), Space.Self);
		topBorderText.transform.position += new Vector3(0f, 1f, 0f);

		textMeshTop = topBorderText.AddComponent<TextMeshPro>();
		topBorderText.GetComponent<RectTransform>().sizeDelta = new Vector2(nbColumns * diameter, diameter);
		textMeshTop.alignment = textAlignmentTop = TextAlignmentOptions.Center;
		textMeshTop.fontSize = textSizeTop = 16;
		textMeshTop.color = textColorTop =  Color.black;
		textMeshTop.text = textTop = "";
		textRotationTop = new Vector3(90f, 90f, 0f);*/
		backgroundColorBottom = backgroundColorTop = backgroundColorLeft = backgroundColorRight = Color.white;

		rightBorderBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
		rightBorderBackground.transform.parent = this.transform;
		rightBorderBackground.GetComponent<Renderer>().material = borderMaterial;
		rightBorderBackground.transform.LookAt(Vector3.down);
		rightBorderBackground.transform.localPosition = new Vector3((nbRows - 1) * (diameter + offset) / 2, height/2, nbColumns * (diameter + offset) + borderOffset);
		rightBorderBackground.transform.Rotate(new Vector3(0f, 0f, 90f), Space.Self);
		rightBorderText = Instantiate(rightBorderBackground);
		rightBorderText.transform.parent = this.transform;
		rightBorderBackground.transform.localScale = new Vector3((diameter + borderOffset/2), nbRows * (diameter + offset) + 2*offset + 2*borderOffset, 1f);
		rightBorderText.transform.localPosition += new Vector3(0f, 0.001f, 0f);
		//rightBorderText.transform.Rotate(new Vector3(0f, 0f, 90f), Space.Self);
		textMeshRight = rightBorderText.AddComponent<TextMeshPro>();
		rightBorderText.GetComponent<RectTransform>().sizeDelta = new Vector2(nbRows * diameter, diameter);
		textMeshRight.alignment  = textAlignmentRight = TextAlignmentOptions.Center;
		textMeshRight.fontSize = textSizeRight = 16;
		textMeshRight.color = textColorRight = Color.black;
		textMeshRight.text = textRight = "";
		textRotationRight = new Vector3(90f, 0f, 0f);

		bottomBorderBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
		bottomBorderBackground.transform.parent = this.transform;
		bottomBorderBackground.GetComponent<Renderer>().material = borderMaterial;
		bottomBorderBackground.transform.LookAt(Vector3.down); ;
		bottomBorderBackground.transform.localPosition = new Vector3(nbRows * (diameter + offset) + borderOffset/2, height/2, ((nbColumns - 1) * (diameter + offset) / 2));
		bottomBorderText = Instantiate(bottomBorderBackground);
		bottomBorderText.transform.parent = this.transform;
		bottomBorderBackground.transform.localScale = new Vector3(diameter  - borderOffset, nbColumns * (diameter + offset) + 2*offset + 1.5f * borderOffset, 1f);
		//bottomBorderText.transform.Rotate(new Vector3(0f, 0f, 90f), Space.Self);
		bottomBorderText.transform.localPosition += new Vector3(0f, 0.001f, 0f);
		textMeshBottom = bottomBorderText.AddComponent<TextMeshPro>();
		bottomBorderText.GetComponent<RectTransform>().sizeDelta = new Vector2(nbColumns * diameter, diameter  - borderOffset);
		textMeshBottom.alignment = textAlignmentBottom = TextAlignmentOptions.Center;
		textMeshBottom.fontSize = textSizeBottom = 16;
		textMeshBottom.color = textColorBottom = Color.black;
		textMeshBottom.text = textBottom = "";
		textRotationBottom = new Vector3(90f, 0f, 0f);
		

		leftBorderBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
		leftBorderBackground.transform.parent = this.transform;
		leftBorderBackground.GetComponent<Renderer>().material = borderMaterial;
		leftBorderBackground.transform.LookAt(Vector3.down);
		leftBorderBackground.transform.localPosition = new Vector3((nbRows - 1) * (diameter + offset) / 2, height/2, -(diameter + offset + borderOffset));
		leftBorderText = Instantiate(leftBorderBackground);
		leftBorderText.transform.parent = this.transform;
		leftBorderBackground.transform.Rotate(new Vector3(0f, 0f, -90f), Space.Self);
		leftBorderBackground.transform.localScale = new Vector3((diameter + borderOffset/2), nbRows * (diameter + offset) + 2*offset + 2*borderOffset, 1f);
		leftBorderText.transform.localPosition += new Vector3(0f, 0.001f, 0f);
		textMeshLeft = leftBorderText.AddComponent<TextMeshPro>();
		leftBorderText.GetComponent<RectTransform>().sizeDelta = new Vector2(nbRows * diameter, diameter);
		textMeshLeft.alignment = textAlignmentLeft = TextAlignmentOptions.Center;
		textMeshLeft.fontSize = textSizeLeft = 16;
		textMeshLeft.color = textColorLeft = Color.black;
		textMeshLeft.text = textLeft = "";
		textRotationLeft = new Vector3(90f, 0f, 0f);

		// Corner Quads

		backgroundColorCornerLeft = backgroundColorCornerRight = Color.white;

		rightCornerBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
		rightCornerBackground.transform.parent = this.transform;
		rightCornerBackground.GetComponent<Renderer>().material = borderMaterial;
		rightCornerBackground.transform.localScale = new Vector3(diameter  - borderOffset, diameter  + borderOffset/2, 1f);
		rightCornerBackground.transform.LookAt(Vector3.down);
		rightCornerBackground.transform.localPosition = new Vector3(nbRows * (diameter + offset) + borderOffset/2, height/2,  nbColumns * (diameter + offset) + borderOffset);
		rightCornerText = Instantiate(rightCornerBackground);
		rightCornerText.transform.parent = this.transform;
		rightCornerText.transform.localPosition += new Vector3(0f, 0.001f, 0f);
		rightCornerText.transform.localScale = new Vector3(1, 1, 1);
		textMeshRightCorner = rightCornerText.AddComponent<TextMeshPro>();
		rightCornerText.GetComponent<RectTransform>().sizeDelta = new Vector2(nbRows * diameter, diameter);

		textMeshRightCorner.alignment = textAlignmentRightCorner = TextAlignmentOptions.Center;
		textMeshRightCorner.fontSize = textSizeRightCorner = 16;
		textMeshRightCorner.color = textColorRightCorner = Color.black;
		textMeshRightCorner.text = textRightCorner = "";
		textRotationRightCorner = new Vector3(90f, 0f, 0f);



		leftCornerBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
		leftCornerBackground.transform.parent = this.transform;
		leftCornerBackground.GetComponent<Renderer>().material = borderMaterial;
		leftCornerBackground.transform.localScale = new Vector3(diameter  - borderOffset, diameter  + borderOffset/2, 1f);
		leftCornerBackground.transform.LookAt(Vector3.down);
		leftCornerBackground.transform.localPosition = new Vector3(nbRows * (diameter + offset) + borderOffset/2, height/2, -(diameter + offset + borderOffset));
		leftCornerText = Instantiate(leftCornerBackground);
		leftCornerText.transform.parent = this.transform;
		leftCornerText.transform.localPosition += new Vector3(0f, 0.001f, 0f);
		leftCornerText.transform.localScale = new Vector3(1, 1, 1);
		textMeshLeftCorner = leftCornerText.AddComponent<TextMeshPro>();
		leftCornerText.GetComponent<RectTransform>().sizeDelta = new Vector2(nbRows * diameter, diameter);
		textMeshLeftCorner.alignment = textAlignmentLeftCorner = TextAlignmentOptions.Center;
		textMeshLeftCorner.fontSize = textSizeLeftCorner = 16;
		textMeshLeftCorner.color = textColorLeftCorner = Color.black;
		textMeshLeftCorner.text = textLeftCorner = "";
		textRotationLeftCorner = new Vector3(90f, 0f, 0f);
		/*GameObject topLeftCornerQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		topLeftCornerQuad.transform.localScale = new Vector3(diameter, diameter, 1f);
		topLeftCornerQuad.transform.LookAt(Vector3.down);
		topLeftCornerQuad.transform.position = new Vector3(-(diameter + offset), height/2, -(diameter + offset));*/

		//client_MqttConnect();
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

	public void client_MqttConnect()
	{

		try
		{
			// Connecting to ExpanDialSticks MQTT Broker
			BROKER_ADDRESS = EXPANDIALSTICKS_BROKER_ADDRESS;
			client = new MqttClient(BROKER_ADDRESS, BROKER_PORT, false, null);
			client.MqttMsgDisconnected += client_MqttMsgDisconnected;
			client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

			string clientId = Guid.NewGuid().ToString();

			OnConnecting(this, new MqttConnectionEventArgs(BROKER_ADDRESS, BROKER_PORT));
			client.Connect(clientId);
			client.Subscribe(new string[] { MQTT_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
			triggerShapeReset();
			//CancelInvoke("client_MqttConnect");
			InvokeRepeating("publishGetRequest", MQTT_DELAY_AT_START, MQTT_INTERVAL);
			

			OnConnected(this, new MqttConnectionEventArgs(BROKER_ADDRESS, BROKER_PORT));

		}
		catch (Exception)
		{

			BROKER_ADDRESS = LOCALHOST_BROKER_ADDRESS;

			try
			{
				// Connecting to localhosy MQTT Broker
				client = new MqttClient(BROKER_ADDRESS, BROKER_PORT, false, null);
				client.MqttMsgDisconnected += client_MqttMsgDisconnected;
				client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

				string clientId = Guid.NewGuid().ToString();

				OnConnecting(this, new MqttConnectionEventArgs(BROKER_ADDRESS, BROKER_PORT));
				client.Connect(clientId);
				client.Subscribe(new string[] { MQTT_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
				triggerShapeReset();
				//CancelInvoke("client_MqttConnect");
				InvokeRepeating("publishGetRequest", MQTT_DELAY_AT_START, MQTT_INTERVAL);
				//}

				OnConnected(this, new MqttConnectionEventArgs(BROKER_ADDRESS, BROKER_PORT));
			}
			catch (Exception)
			{
				Quit();
			}
		}
	}

	private void client_MqttMsgDisconnected(object sender, EventArgs e)
	{
		
		OnDisconnected(this,  new MqttConnectionEventArgs(BROKER_ADDRESS, BROKER_PORT));
		//Debug.Log("Disconnected. Trying to reconnect in  " + MQTT_DELAY_RECONNECT + " secs...");
		//Invoke("client_MqttConnect", MQTT_DELAY_RECONNECT);

	}

	private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
	{
		try
		{
			/*#if DEBUG
			Debug.Log("Received: " + System.Text.Encoding.UTF8.GetString(e.Message));
				
			#endif*/
			GetAns gans = JsonUtility.FromJson<GetAns>(System.Text.Encoding.UTF8.GetString(e.Message));
			if (gans.GET != null && gans.ANS.status != null)
			{
				// Prepare Safety Response

				int[] positions = new int[nbRows * nbColumns];
				float[] durations = new float[nbRows * nbColumns];
				int[] holdings = new int[nbRows * nbColumns];
				bool safe = true;

				if (gans.ANS.status != MQTT_SUCCESS) Debug.LogWarning("GET -> " + gans.ANS.status);
				else {
					// IF PREVIOUS SHAPE FRAME HAS BEEN HANDLE THEN PROCEED WITH NEXT
					if(shapeChanging == false){
						for (int i = 0; i < nbRows; i++)
						{
							for (int j = 0; j < nbColumns; j++)
							{
								float prevProximity = modelMatrix[i, j].CurrentProximity;
								float nextProximity = collisionMatrix[i, j].Proximity();
								float distanceFromBody = collisionMatrix[i, j].Distance();
								int currSeparationLevel = collisionMatrix[i, j].SeparationLevel();

								modelMatrix[i, j].setShapeChangeCurrent(
									gans.ANS.content.xAxisValue[i * nbColumns + j], // xAxisValue (-128, 127)
									gans.ANS.content.yAxisValue[i * nbColumns + j], // yAxisValue (-128, 127)
									gans.ANS.content.selectCountValue[i * nbColumns + j],  // selectCountValue (0, 255)
									gans.ANS.content.rotationValue[i * nbColumns + j],   // rotationValue (-128, 127)
									gans.ANS.content.positionValue[i * nbColumns + j], // positionValue (0, 40)
									(bool)(gans.ANS.content.reachingValue[i * nbColumns + j] == 1 ? true : false), // reachingValue (0, 1)
									(bool)(gans.ANS.content.holdingValue[i * nbColumns + j] == 1 ? true : false), // holdingValue (0, 1)
									currSeparationLevel,
									nextProximity,
									distanceFromBody,
									modelMatrix[i, j].CurrentPaused,
									MQTT_INTERVAL
									);

								if (safeGuardOn)
								{
									// doing nothing for each pin
									positions[i * nbColumns + j] = modelMatrix[i, j].CurrentPosition;
									holdings[i * nbColumns + j] = modelMatrix[i, j].CurrentHolding ? 1 : 0;
									durations[i * nbColumns + j] = 0f;

									// handle proximity
									if (nextProximity >= 1f) // PIN MUST STOP
									{
										if (modelMatrix[i, j].CurrentReaching) // PIN IS INDEED MOVING 
										{
											if (modelMatrix[i, j].CurrentPaused == 0) // PIN IS NOT ALREADY BEING PAUSED
											{
												Debug.Log("modelMatrix[" + i + "," + j + "] pause towards " + modelMatrix[i, j].TargetPosition + "!");
												modelMatrix[i, j].CurrentPaused = modelMatrix[i, j].TargetPosition - modelMatrix[i, j].CurrentPosition;
												positions[i * nbColumns + j] = modelMatrix[i, j].CurrentPosition;
												holdings[i * nbColumns + j] = 0;
												durations[i * nbColumns + j] = 0.1f;
												safe = false;
											}
										}
									}
									else
									{
										// PIN IS FREE
										// PIN HAS BEEN PAUSED THEN START IT
										if (!modelMatrix[i, j].CurrentReaching)
										{
											if (modelMatrix[i, j].CurrentPaused != 0)
											{
												modelMatrix[i, j].CurrentPaused = 0;
												positions[i * nbColumns + j] = modelMatrix[i, j].TargetPosition;
												holdings[i * nbColumns + j] = modelMatrix[i, j].TargetHolding ? 1 : 0;

												//float minShapeChangeDuration = 1f; // 20 pos per sec
												//durations[i * nbColumns + j] = minShapeChangeDuration + (nextProximity * 3f);

												float safetySpeed = maxSpeed * (1f - modelMatrix[i, j].CurrentProximity); // 20 pos per sec max
												float distance = Math.Abs(modelMatrix[i, j].TargetPosition - modelMatrix[i, j].CurrentPosition);
												float safetyDuration = Math.Max(distance / safetySpeed, 0.1f);
												durations[i * nbColumns + j] = safetyDuration;
												Debug.Log("modelMatrix[" + i + "," + j + "] unpause from " + modelMatrix[i, j].TargetPosition
													+ " to " + modelMatrix[i, j].CurrentPosition + " in " + safetyDuration + "s!");
												safe = false;
											}
										}
										if (modelMatrix[i, j].CurrentReaching) // PIN IS INDEED MOVING 
										{
											if (nextProximity != prevProximity) // SPEED UP
											{
												positions[i * nbColumns + j] = modelMatrix[i, j].TargetPosition;
												holdings[i * nbColumns + j] = modelMatrix[i, j].TargetHolding ? 1 : 0;
												float safetySpeed = maxSpeed * (1f - modelMatrix[i, j].CurrentProximity); // 20 pos per sec max
												float distance = Math.Abs(modelMatrix[i, j].TargetPosition - modelMatrix[i, j].CurrentPosition);
												float safetyDuration = Math.Max(distance / safetySpeed, 0.1f);
												durations[i * nbColumns + j] = safetyDuration;

												Debug.Log("modelMatrix[" + i + "," + j + "] change speed from " + modelMatrix[i, j].TargetPosition
													+ " to " + modelMatrix[i, j].CurrentPosition + " in " + safetyDuration  + "s!");
												//float minShapeChangeDuration = 1f; // 20 pos per sec
												//durations[i * nbColumns + j] = minShapeChangeDuration + (nextProximity * 3f);
												safe = false;
											}
										}
									}
								}

							}
						}
						shapeChanging = true;
					}
				}
				// unsafety detected then command related pins to stop!
				if (!safe)
				{
					publishSetRequest(positions, durations, holdings);
				}
				return;
			}
			SetAns sans = JsonUtility.FromJson<SetAns>(System.Text.Encoding.UTF8.GetString(e.Message));
			if (sans.SET != null && sans.ANS.status != null)
			{
				if (sans.ANS.status != MQTT_SUCCESS) Debug.LogWarning("SET -> " + sans.ANS.status);
				return;
			}

		}
		catch (Exception e1)
		{
			Debug.LogException(e1, this);
		}
	}

	private void publishGetRequest()
	{
		try { 
			// Create Get Request Object
			GetRequest greq = new GetRequest();
			// Convert it to JSON String
			string getJson = JsonUtility.ToJson(greq);
			// publish get request
			/*#if DEBUG
						Debug.Log("Sending...");
			#endif*/

			client.Publish(MQTT_TOPIC, System.Text.Encoding.UTF8.GetBytes(getJson), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true); // ! MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE

			/*#if DEBUG
						Debug.Log("Sended: " + getJson);
			#endif*/

		}
		catch (Exception e2) {
			Debug.LogException(e2, this);
		}
	}
	
	private void publishSetRequest(int[] position, float[] duration, int[] holding)
	{
		try
		{
			// Create Set Request Object
			SetRequest sreq = new SetRequest();

			// Fill it
			sreq.SET.position = position;
			sreq.SET.duration = duration;
			sreq.SET.holding = holding;

			// Convert it to JSON String
			string setJson = JsonUtility.ToJson(sreq);

			/*#if DEBUG
						Debug.Log("Sending...");
			#endif*/

			// Publish it
			client.Publish(MQTT_TOPIC, System.Text.Encoding.UTF8.GetBytes(setJson), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true); // ! MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE

			/*#if DEBUG
						Debug.Log("Sended: " + setJson);
			#endif*/
		}
		catch (Exception e3)
		{
			Debug.LogException(e3, this);
		}
	}
	public void setBottomBorderText(TextAlignmentOptions textAlignment, int textSize, Color textColor, string text, Vector3 textRotation){
		this.textAlignmentBottom = textAlignment;
		this.textSizeBottom = textSize;
		this.textColorBottom = textColor;
		this.textBottom = text;
		this.textRotationBottom = textRotation;
	}

	public void setRightBorderText(TextAlignmentOptions textAlignment, int textSize, Color textColor, string text, Vector3 textRotation){
		this.textAlignmentRight = textAlignment;
		this.textSizeRight = textSize;
		this.textColorRight = textColor;
		this.textRight = text;
		this.textRotationRight = textRotation;
	}

	public void setRightCornerText(TextAlignmentOptions textAlignment, int textSize, Color textColor, string text, Vector3 textRotation)
	{
		this.textAlignmentRightCorner = textAlignment;
		this.textSizeRightCorner = textSize;
		this.textColorRightCorner = textColor;
		this.textRightCorner = text;
		this.textRotationRightCorner = textRotation;
	}

	public void setLeftBorderText(TextAlignmentOptions textAlignment, int textSize, Color textColor, string text, Vector3 textRotation){
		this.textAlignmentLeft = textAlignment;
		this.textSizeLeft = textSize;
		this.textColorLeft = textColor;
		this.textLeft = text;
		this.textRotationLeft = textRotation;
	}

	public void setLeftCornerText(TextAlignmentOptions textAlignment, int textSize, Color textColor, string text, Vector3 textRotation)
	{
		this.textAlignmentLeftCorner = textAlignment;
		this.textSizeLeftCorner = textSize;
		this.textColorLeftCorner = textColor;
		this.textLeftCorner = text;
		this.textRotationLeftCorner = textRotation;
	}

	public void setBottomBorderBackground(Color backgroundColor)
	{
		this.backgroundColorBottom = backgroundColor;
	}

	public void setRightBorderBackground(Color backgroundColor)
	{
		this.backgroundColorRight = backgroundColor;
	}
	public void setLeftBorderBackground(Color backgroundColor)
	{
		this.backgroundColorLeft = backgroundColor;
	}
	public void setLeftCornerBackground(Color backgroundColor)
	{
		this.backgroundColorCornerLeft = backgroundColor;
	}
	public void setRightCornerBackground(Color backgroundColor)
	{
		this.backgroundColorCornerRight = backgroundColor;
	}
	public void setBorderBackground(Color backgroundColor)
	{
		this.setBottomBorderBackground(backgroundColor);
		this.setLeftBorderBackground(backgroundColor);
		this.setRightBorderBackground(backgroundColor);
		this.setLeftCornerBackground(backgroundColor);
		this.setRightCornerBackground(backgroundColor);
	}




	public void triggerShapeChange(){
		if(SIMULATION) {
			// set target shape to current 
			for (int i = 0; i < nbRows; i++)
			{
				for(int j = 0; j < nbColumns; j++)
				{
					bool reaching = modelMatrix[i, j].TargetShapeChangeDuration > 0f &&  (modelMatrix[i, j].CurrentPosition != modelMatrix[i, j].TargetPosition);
					modelMatrix[i, j].setShapeChangeCurrent(
						modelMatrix[i, j].TargetAxisX,
						modelMatrix[i, j].TargetAxisY,
						modelMatrix[i, j].TargetSelectCount,
						modelMatrix[i, j].TargetRotation,
						modelMatrix[i, j].TargetPosition,
						reaching,
						modelMatrix[i, j].TargetHolding,
						modelMatrix[i, j].TargetSeparationLevel,
						modelMatrix[i, j].TargetProximity,
						modelMatrix[i, j].TargetDistance,
						0,
						modelMatrix[i, j].TargetShapeChangeDuration
					);
					modelMatrix[i, j].TargetShapeChangeDuration = 0f;
				}
			}
			shapeChanging = true;
		} else {
			// Publish_Command()
			int[] positions = new int[nbRows * nbColumns];
			float[] durations = new float[nbRows * nbColumns];
			int[] holdings = new int[nbRows * nbColumns];
			for (int i = 0; i < nbRows; i++)
			{
				for(int j = 0; j < nbColumns; j++)
				{
					positions[i * nbColumns + j] = modelMatrix[i, j].TargetPosition;
					holdings[i * nbColumns + j] = modelMatrix[i, j].TargetHolding ? 1 : 0;
					modelMatrix[i, j].CurrentProximity = collisionMatrix[i, j].Proximity();
					if (safeGuardOn)
					{
						if (modelMatrix[i, j].CurrentProximity < 1f)
						{
						float safetySpeed = maxSpeed * (1f - modelMatrix[i, j].CurrentProximity); // 20 pos per sec max
						float distance = Math.Abs(modelMatrix[i, j].TargetPosition - modelMatrix[i, j].CurrentPosition);
						float safetyDuration = Math.Max(distance / safetySpeed, 0.1f);
						durations[i * nbColumns + j] = Math.Max(safetyDuration, modelMatrix[i, j].TargetShapeChangeDuration);
						} else {
								Debug.Log("modelMatrix[" + i + "," + j + "] pause at start!");
								modelMatrix[i, j].CurrentPaused = modelMatrix[i, j].TargetPosition - modelMatrix[i, j].CurrentPosition;
								Debug.Log("modelMatrix[i, j].CurrentPaused: " + modelMatrix[i, j].CurrentPaused);
								durations[i * nbColumns + j] = 0f;
						}
					}
					else
					{
						durations[i * nbColumns + j] = modelMatrix[i, j].TargetShapeChangeDuration;
					}

					// Reset TargetShapeChangeDuration to zero to prevent previous animations
					modelMatrix[i, j].TargetShapeChangeDuration = 0f;
				}
			}
			 publishSetRequest(positions, durations, holdings);
		}
	}
	public void triggerShapeReset(){
		// Publish_Command()
		int[] positions = new int[nbRows * nbColumns];
		float[] durations = new float[nbRows * nbColumns];
		int[] holdings = new int[nbRows * nbColumns];
		for (int i = 0; i < nbRows; i++)
		{
			for(int j = 0; j < nbColumns; j++)
			{
				positions [i * nbColumns + j] = 0;
				durations [i * nbColumns + j] = 2f;
				holdings [i * nbColumns + j] = 0;
			}
		}
			publishSetRequest(positions, durations, holdings);
	}

	public void triggerSafetyChange()
	{
		// set target texture to current
		for (int i = 0; i < nbRows; i++)
		{
			for (int j = 0; j < nbColumns; j++)
			{
				modelMatrix[i, j].setSafetyChange(modelMatrix[i, j].CurrentFeedForwarded);
			}
		}
		safetyChanging = true;
	}

	public void triggerProjectorChange()
	{
		// set target texture to current
		for (int i = 0; i < nbRows; i++)
		{
			for (int j = 0; j < nbColumns; j++)
			{
				modelMatrix[i, j].setProjectorChangeCurrent(
					modelMatrix[i, j].TargetProjectorColor,
					modelMatrix[i, j].TargetProjectorTexture,
					modelMatrix[i, j].TargetProjectorOffset,
					modelMatrix[i, j].TargetProjectorRotation,
					modelMatrix[i, j].TargetProjectorSize,
					modelMatrix[i, j].TargetProjectorChangeDuration
				);
				modelMatrix[i, j].TargetProjectorChangeDuration = 0f;
			}
		}
		projectorChanging = true;

	}
	public void triggerTextureChange(){
		// set target texture to current
		for (int i = 0; i < nbRows; i++)
		{
			for(int j = 0; j < nbColumns; j++)
			{
				modelMatrix[i, j].setTextChangeCurrent(
					modelMatrix[i, j].TargetColor,
					modelMatrix[i, j].TargetTextAlignment,
					modelMatrix[i, j].TargetTextSize,
					modelMatrix[i, j].TargetTextRotation,
					modelMatrix[i, j].TargetTextColor,
					modelMatrix[i, j].TargetText,
					modelMatrix[i, j].TargetTextureChangeDuration
				);
				modelMatrix[i, j].setTextureChangeCurrent(
					modelMatrix[i, j].TargetColor,
					modelMatrix[i, j].TargetPlaneTexture,
					modelMatrix[i, j].TargetPlaneColor,
					modelMatrix[i, j].TargetPlaneSize,
					modelMatrix[i, j].TargetPlaneOffset,
					modelMatrix[i, j].TargetPlaneRotation,
					modelMatrix[i, j].TargetTextureChangeDuration
				);
				modelMatrix[i, j].TargetTextureChangeDuration = 0f;
				
				/*modelMatrix[i, j].setProjectorChangeCurrent(
					modelMatrix[i, j].TargetProjectorColor,
					modelMatrix[i, j].TargetProjectorTexture,
					modelMatrix[i, j].TargetProjectorOffset,
					modelMatrix[i, j].TargetProjectorRotation,
					modelMatrix[i, j].TargetProjectorSizeTargetProjectorSize,
					modelMatrix[i, j].TargetProjectorChangeDuration
				);
				modelMatrix[i, j].TargetProjectorChangeDuration = 0f;*/
			}
		}
		textureChanging = true;
	}
	
	public ExpanDialStickModel this [int i, int j]{
		get => modelMatrix[i, j];
    	set => modelMatrix[i, j] = value;
	}

	public void simulateSecureShapeChange()
	{
		for (int i = 0; i < nbRows; i++)
		{
			for (int j = 0; j < nbColumns; j++)
			{

				float prevProximity = modelMatrix[i, j].CurrentProximity;
				float nextProximity = collisionMatrix[i, j].Proximity();
				float currDistance = collisionMatrix[i, j].Distance();
				int currSeparationLevel = collisionMatrix[i, j].SeparationLevel();
				viewMatrix[i, j].CurrentProximity = modelMatrix[i, j].CurrentProximity = nextProximity;
				viewMatrix[i, j].CurrentDistance = modelMatrix[i, j].CurrentDistance = currDistance;
				viewMatrix[i, j].CurrentSeparationLevel = modelMatrix[i, j].CurrentSeparationLevel = currSeparationLevel;
				viewMatrix[i, j].CurrentReaching = modelMatrix[i, j].CurrentReaching = modelMatrix[i, j].CurrentPosition != viewMatrix[i, j].CurrentPosition;
				if (safeGuardOn)
				{
					if (nextProximity >= 1f)
					{
						if (modelMatrix[i, j].CurrentReaching) // PIN IS INDEED MOVING 
						{
							if (modelMatrix[i, j].CurrentPaused == 0)
							{
								modelMatrix[i, j].setShapeChangeCurrent(
									modelMatrix[i, j].CurrentAxisX,
									modelMatrix[i, j].CurrentAxisY,
									modelMatrix[i, j].CurrentSelectCount,
									modelMatrix[i, j].CurrentRotation,
									viewMatrix[i, j].CurrentPosition, // set to view current pos
									false,
									modelMatrix[i, j].CurrentHolding,
									modelMatrix[i, j].CurrentSeparationLevel,
									modelMatrix[i, j].CurrentProximity,
									modelMatrix[i, j].CurrentDistance,
									modelMatrix[i, j].CurrentPosition-viewMatrix[i, j].CurrentPosition,
									0.1f // very fast
								);
								shapeChanging = true;
							}
						}
					}
					else
					{
						if (!modelMatrix[i, j].CurrentReaching) // PIN IS NOT MOVING 
						{
							if (modelMatrix[i, j].CurrentPaused != 0)
							{

								float safetySpeed = maxSpeed * (1f - modelMatrix[i, j].CurrentProximity); // 20 pos per sec max
								float distance = Math.Abs(modelMatrix[i, j].TargetPosition - viewMatrix[i, j].CurrentPosition);
								float safetyDuration = Math.Max(distance / safetySpeed, 0.1f);
								modelMatrix[i, j].setShapeChangeCurrent(
										modelMatrix[i, j].CurrentAxisX,
										modelMatrix[i, j].CurrentAxisY,
										modelMatrix[i, j].CurrentSelectCount,
										modelMatrix[i, j].CurrentRotation,
										modelMatrix[i, j].TargetPosition, // set to model target pos
										true,
										modelMatrix[i, j].TargetHolding,
										modelMatrix[i, j].CurrentSeparationLevel,
										modelMatrix[i, j].CurrentProximity,
										modelMatrix[i, j].CurrentDistance,
										0,
										safetyDuration //  fast
									);
								shapeChanging = true;
							}
						}
						if (modelMatrix[i, j].CurrentReaching) // PIN IS INDEED MOVING 
						{
							if (prevProximity != nextProximity)
							{
								float safetySpeed = maxSpeed * (1f - modelMatrix[i, j].CurrentProximity); // 20 pos per sec max
								float distance = Math.Abs(modelMatrix[i, j].TargetPosition - viewMatrix[i, j].CurrentPosition);
								float safetyDuration = Math.Max(distance / safetySpeed, 0.1f);
								modelMatrix[i, j].setShapeChangeCurrent(
									modelMatrix[i, j].CurrentAxisX,
									modelMatrix[i, j].CurrentAxisY,
									modelMatrix[i, j].CurrentSelectCount,
									modelMatrix[i, j].CurrentRotation,
									modelMatrix[i, j].TargetPosition, // set to view current pos
									true,
									modelMatrix[i, j].TargetHolding,
									modelMatrix[i, j].CurrentSeparationLevel,
									modelMatrix[i, j].CurrentProximity,
									modelMatrix[i, j].CurrentDistance,
									modelMatrix[i, j].CurrentPaused,
									safetyDuration //  fast
								);
								shapeChanging = true;
							}
						}
					}
				}
			}
		}
	}

	// Update is called once per frame
	void Update () {
		// CHECK FOR COLLISION AND SECURE SHAPE-CHANGE ACCORDINGLY
		if (SIMULATION)
		{
			simulateSecureShapeChange();
		}

		// UPDATE VIEW FROM MODEL
		if (shapeChanging){
			DateTime t = DateTime.Now;
			for (int i = 0; i < nbRows; i++)
			{
				for(int j = 0; j < nbColumns; j++)
				{
					viewMatrix[i, j].setShapeChangeTarget(
						modelMatrix[i, j].CurrentAxisX,
						modelMatrix[i, j].CurrentAxisY,
						modelMatrix[i, j].CurrentSelectCount,
						modelMatrix[i, j].CurrentRotation,
						modelMatrix[i, j].CurrentPosition,
						modelMatrix[i, j].CurrentReaching,
						modelMatrix[i, j].CurrentHolding,
						modelMatrix[i, j].CurrentSeparationLevel,
						modelMatrix[i, j].CurrentProximity,
						modelMatrix[i, j].CurrentDistance,
						modelMatrix[i, j].CurrentPaused,
						modelMatrix[i, j].CurrentShapeChangeDuration
					);
					// READ EVENT
					float[] events = modelMatrix[i, j].readAndEraseShapeDiffs();
					// !!! CAN HANDLE THE SAME EVENT ONLY ONE TIME
					if (events.Length > 8)
					{
						if (events[0] != 0f) //  X Axis events
						{   // Trigger event
							OnXAxisChanged(this, new ExpanDialStickEventArgs(t, i, j, modelMatrix[i, j].CurrentAxisX - events[0], modelMatrix[i, j].CurrentAxisX, events[0]));
							//Debug.Log("(" + i + ", " + j + ") X Axis Event.");
						}

						if (events[1] != 0f) //  Y Axis events
						{
							OnYAxisChanged(this, new ExpanDialStickEventArgs(t, i, j, modelMatrix[i, j].CurrentAxisY - events[1], modelMatrix[i, j].CurrentAxisY, events[1]));
							//Debug.Log("(" + i + ", " + j + ") Y Axis Event.");
						}
						if (events[2] != 0f && (events[4] == 0f || (events[4] != 0f && modelMatrix[i, j].CurrentHolding))) // Click events
						{
							OnClickChanged(this, new ExpanDialStickEventArgs(t, i, j, modelMatrix[i, j].CurrentSelectCount - events[2], modelMatrix[i, j].CurrentSelectCount, events[2]));
							//Debug.Log("(" + i + ", " + j + ") Select Event.");
						}
						if (events[3] != 0f) // Rotation events
						{
							OnRotationChanged(this, new ExpanDialStickEventArgs(t, i, j, modelMatrix[i, j].CurrentRotation - events[3], modelMatrix[i, j].CurrentRotation, events[3]));
							//Debug.Log("(" + i + ", " + j + ") Dial Event.");
						}

						if (events[4] != 0f && !modelMatrix[i, j].CurrentHolding && !modelMatrix[i, j].CurrentReaching) // Push/Pull events
						{
							OnPositionChanged(this, new ExpanDialStickEventArgs(t, i, j, modelMatrix[i, j].CurrentPosition - events[4], modelMatrix[i, j].CurrentPosition, events[4]));
							//Debug.Log("(" + i + ", " + j + ") Encoder Event.");
						}


						if (events[5] != 0f) // Reaching events
						{
							onReachingChanged(this, new ExpanDialStickEventArgs(t, i, j, Convert.ToSingle(modelMatrix[i, j].CurrentReaching) - events[5], Convert.ToSingle(modelMatrix[i, j].CurrentReaching), events[5]));
							//Debug.Log("(" + i + ", " + j + ") Reaching Event.");
						}

						if (events[6] != 0f) // Holding events
						{
							onHoldingChanged(this, new ExpanDialStickEventArgs(t, i, j, Convert.ToSingle(modelMatrix[i, j].CurrentHolding) - events[6], Convert.ToSingle(modelMatrix[i, j].CurrentHolding), events[6]));
							//Debug.Log("(" + i + ", " + j + ") Holding Event.");
						}
						if (events[7] != 0f) // Proximity events
						{
							onSeparationLevelChanged(this, new ExpanDialStickEventArgs(t, i, j, Convert.ToSingle(modelMatrix[i, j].CurrentSeparationLevel) - events[7], Convert.ToSingle(modelMatrix[i, j].CurrentSeparationLevel), events[7]));
							//Debug.Log("(" + i + ", " + j + ") Holding Event.");
						}

						if (events[8] != 0f) // Proximity events
						{
							onProximityChanged(this, new ExpanDialStickEventArgs(t, i, j, Convert.ToSingle(modelMatrix[i, j].CurrentProximity) - events[8], Convert.ToSingle(modelMatrix[i, j].CurrentProximity), events[8]));
							//Debug.Log("(" + i + ", " + j + ") Holding Event.");
						}
					}

				}
			}
			shapeChanging = false;
		}
		
		if(textureChanging){
			for (int i = 0; i < nbRows; i++)
			{
				for(int j = 0; j < nbColumns; j++)
				{
					viewMatrix[i, j].setTextChangeTarget(
						modelMatrix[i, j].CurrentColor,
						modelMatrix[i, j].CurrentTextAlignment,
						modelMatrix[i, j].CurrentTextSize,
						modelMatrix[i, j].CurrentTextRotation,
						modelMatrix[i, j].CurrentTextColor,
						modelMatrix[i, j].CurrentText,
						modelMatrix[i, j].CurrentTextureChangeDuration
					);
					viewMatrix[i, j].setTextureChangeTarget(
						modelMatrix[i, j].CurrentColor,
						modelMatrix[i, j].CurrentPlaneTexture,
						modelMatrix[i, j].CurrentPlaneColor,
						modelMatrix[i, j].CurrentPlaneSize,
						modelMatrix[i, j].CurrentPlaneOffset,
						modelMatrix[i, j].CurrentPlaneRotation,
						modelMatrix[i, j].CurrentTextureChangeDuration
					);
					/*viewMatrix[i, j].setProjectorChangeTarget(
						modelMatrix[i, j].CurrentProjectorColor,
						modelMatrix[i, j].CurrentProjectorTexture,
						modelMatrix[i, j].CurrentProjectorOffset,
						modelMatrix[i, j].CurrentProjectorRotation,
						modelMatrix[i, j].CurrentProjectorSize,
						modelMatrix[i, j].CurrentProjectorChangeDuration
					);*/
				}
			}
			textureChanging = false;
		}
		if (projectorChanging)
		{
			for (int i = 0; i < nbRows; i++)
			{
				for (int j = 0; j < nbColumns; j++)
				{
					viewMatrix[i, j].setProjectorChangeTarget(
						modelMatrix[i, j].CurrentProjectorColor,
						modelMatrix[i, j].CurrentProjectorTexture,
						modelMatrix[i, j].CurrentProjectorOffset,
						modelMatrix[i, j].CurrentProjectorRotation,
						modelMatrix[i, j].CurrentProjectorSize,
						modelMatrix[i, j].CurrentProjectorChangeDuration
					);
				}
			}
			projectorChanging = false;
		}

		if (safetyChanging)
		{

			for (int i = 0; i < nbRows; i++)
			{
				for (int j = 0; j < nbColumns; j++)
				{
					viewMatrix[i, j].setSafetyChange(
						modelMatrix[i, j].CurrentFeedForwarded
					);
				}
			}
			safetyChanging = false;
		}

		// RENDER


		/*textMeshTop.alignment  = textAlignmentTop;
		textMeshTop.fontSize = textSizeTop;
		textMeshTop.color = textColorTop;
		textMeshTop.text = textTop;*/
		// topBorderText.transform.eulerAngles = textRotationTop; 
		textMeshBottom.alignment  = textAlignmentBottom;
		textMeshBottom.fontSize = textSizeBottom;
		textMeshBottom.color = textColorBottom;
		textMeshBottom.text = textBottom;
		bottomBorderText.transform.localEulerAngles = textRotationBottom;


		textMeshRight.alignment  = textAlignmentRight;
		textMeshRight.fontSize = textSizeRight;
		textMeshRight.color = textColorRight;
		textMeshRight.text = textRight;
		rightBorderText.transform.localEulerAngles = textRotationRight;

		textMeshRightCorner.alignment = textAlignmentRightCorner;
		textMeshRightCorner.fontSize = textSizeRightCorner;
		textMeshRightCorner.color = textColorRightCorner;
		textMeshRightCorner.text = textRightCorner;
		rightCornerText.transform.localEulerAngles = textRotationRightCorner;

		textMeshLeft.alignment  = textAlignmentLeft;
		textMeshLeft.fontSize = textSizeLeft;
		textMeshLeft.color = textColorLeft;
		textMeshLeft.text = textLeft;
		leftBorderText.transform.localEulerAngles = textRotationLeft;

		textMeshLeftCorner.alignment = textAlignmentLeftCorner;
		textMeshLeftCorner.fontSize = textSizeLeftCorner;
		textMeshLeftCorner.color = textColorLeftCorner;
		textMeshLeftCorner.text = textLeftCorner;
		leftCornerText.transform.localEulerAngles = textRotationLeftCorner;

		bottomBorderBackground.GetComponent<Renderer>().material.color = backgroundColorBottom;
		rightBorderBackground.GetComponent<Renderer>().material.color = backgroundColorRight;
		leftBorderBackground.GetComponent<Renderer>().material.color = backgroundColorLeft;
		leftCornerBackground.GetComponent<Renderer>().material.color = backgroundColorCornerLeft;
		rightCornerBackground.GetComponent<Renderer>().material.color = backgroundColorCornerRight;

	}
}
