#define DEBUG

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

	/*public void setPositionByteArray(int[] positionArray)
	{
		SET.position = new sbyte[positionArray.Length];
		for (int i = 0; i < positionArray.Length; i++)
		{
			sbyte[] intBytes = Array.ConvertAll(BitConverter.GetBytes(positionArray[i]), b => unchecked((sbyte)b));
			if (BitConverter.IsLittleEndian) Array.Reverse(intBytes);
			SET.position[i] = intBytes[intBytes.Length - 1];
		}
	}
	public void setDurationByteArray(float[] durationArray)
	{
		SET.duration = new byte[durationArray.Length * sizeof(float)];
		for (int i = 0; i < durationArray.Length; i++)
		{
			byte[] floatBytes = BitConverter.GetBytes(durationArray[i]);
			if (BitConverter.IsLittleEndian) Array.Reverse(floatBytes);
			for (int j = 0; j < sizeof(float); j++) SET.duration[i * sizeof(float) + j] = floatBytes[j];
		}
	}
	public void setHoldingByte(bool[] holdingArray)
	{
		SET.holding = 0x00;
		for (int i = 0; i < holdingArray.Length; i++) SET.holding |= (byte)((holdingArray[i] ? 1 : 0) << (7 - i));
	}*/
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

	public float diameter = 4.0f;
	public float height = 10.0f;
	public float offset = 0.5f;


	public const string MQTT_BAD_VALUES = "bad values";
	public const string MQTT_WRONG_LENGTH = "wrong length";
	public const string MQTT_MISSING_KEY = "missing key";
	public const string MQTT_UNKNOWN_CMD = "unknown command";
	public const string MQTT_VALUE_ERROR = "json value error";
	public const string MQTT_SUCCESS = "success";

	public IPAddress BROKER_ADDRESS = IPAddress.Parse("192.168.0.10"); // "test.mosquitto.org";
	public int BROKER_PORT = 1883; // 8080; 
	public string MQTT_TOPIC = "ExpanDialSticks";
	public float MQTT_DELAY_RECONNECT = 5f; // 0.2f;
	public float MQTT_DELAY_AT_START = 2f; // 0.2f;
	public float MQTT_INTERVAL = 0.2f; // 0.2f;
	public int prevMillis = 0;
	public int currMillis = 0;
	public const int nbColumns = 6;
	public const int nbRows = 5;
	public float cameraDistanceFromMatrix = 30f;
	float xOffsetCamera = -3f; //5f

	public GameObject expanDialStickPrefab;
	public GUISkin guiSkin;
	public TextMeshPro textMeshPro;

	private Camera mainCamera;

	private MqttClient client;

	private GameObject[,] matrix = new GameObject[nbRows, nbColumns];
	private ExpanDialStickModel[,] modelMatrix = new ExpanDialStickModel[nbRows, nbColumns];

	private bool matrixMustBeUpdated = false;
	//private ExpanDialStickModel[,] expanDialSticks = new ExpanDialStickModel[nbRows, nbColumns];

	// Use this for initialization
	void Start () {

		// Init ExpanDialSticks Model and View
		for (int i = 0; i < nbRows; i++)
			for (int j = 0; j < nbColumns; j++)
			{
				// Model
				modelMatrix[i, j] = new ExpanDialStickModel();
				modelMatrix[i, j].setConstants(diameter, height, offset);
				modelMatrix[i, j].setIndexes(i, j);

				// View
				matrix[i, j] = Instantiate(expanDialStickPrefab);
				matrix[i, j].transform.parent = this.transform;
				matrix[i, j].GetComponent<ExpanDialStickView>().setConstants(diameter, height, offset);
				matrix[i, j].GetComponent<ExpanDialStickView>().setIndexes(i, j);
			}

		// Set camera
		mainCamera = Camera.main;
		mainCamera.enabled = true;
		mainCamera.pixelRect = new Rect(0, 0, 1920, 1080);
		Vector3 cameraPosition = new Vector3((nbRows - 1) * (diameter + offset) / 2 - xOffsetCamera, cameraDistanceFromMatrix, (nbColumns - 1) * (diameter + offset) / 2);
		mainCamera.transform.position = cameraPosition;

		Vector3 cameraLookAtPosition = cameraPosition - new Vector3(0f, cameraDistanceFromMatrix, 0f);
		mainCamera.transform.LookAt(cameraLookAtPosition);

		Vector3 targetOrientationPosition = new Vector3(0, cameraDistanceFromMatrix, (nbColumns - 1) * (diameter + offset) / 2);
		Vector3 targetOrientationDir = targetOrientationPosition - cameraPosition;
		float zAngle = Vector3.Angle(targetOrientationDir, Vector3.up);
		mainCamera.transform.Rotate(0f, 0f, zAngle, Space.Self);
		mainCamera.transform.Rotate(0f, 0f, 180f, Space.Self);

		// Border Quads
		GameObject topBorderBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
		topBorderBackground.transform.LookAt(Vector3.down);
		topBorderBackground.transform.position = new Vector3(-(diameter + offset), height/2, ((nbColumns - 1) * (diameter + offset) / 2));
		GameObject topBorderText = Instantiate(topBorderBackground);
		topBorderBackground.transform.localScale = new Vector3(diameter, nbColumns * (diameter + offset), 1f);
		topBorderText.transform.Rotate(new Vector3(0f, 0f, 90f), Space.Self);
		topBorderText.transform.position += new Vector3(0f, 1f, 0f);
		TextMeshPro topBorderTmp = topBorderText.AddComponent<TextMeshPro>();
		topBorderTmp.alignment = TextAlignmentOptions.Center;
		topBorderTmp.fontSize = 16;
		topBorderTmp.color = Color.black;
		topBorderTmp.text = "A simple line of text.";

		GameObject rightBorderBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
		rightBorderBackground.transform.LookAt(Vector3.down);
		rightBorderBackground.transform.position = new Vector3((nbRows - 1) * (diameter + offset) / 2, height/2, nbColumns * (diameter + offset));
		rightBorderBackground.transform.Rotate(new Vector3(0f, 0f, 90f), Space.Self);
		GameObject rightBorderText = Instantiate(rightBorderBackground);
		rightBorderBackground.transform.localScale = new Vector3(diameter, nbRows * (diameter + offset), 1f);

		GameObject bottomBorderBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
		bottomBorderBackground.transform.LookAt(Vector3.down); ;
		bottomBorderBackground.transform.position = new Vector3(nbRows * (diameter + offset), height/2, ((nbColumns - 1) * (diameter + offset) / 2));
		GameObject bottomBorderText = Instantiate(bottomBorderBackground);
		bottomBorderBackground.transform.localScale = new Vector3(diameter, nbColumns * (diameter + offset), 1f);
		bottomBorderText.transform.Rotate(new Vector3(0f, 0f, 90f), Space.Self);
		bottomBorderText.transform.position += new Vector3(0f, 1f, 0f);
		TextMeshPro bottomBorderTmp = bottomBorderText.AddComponent<TextMeshPro>();
		bottomBorderTmp.alignment = TextAlignmentOptions.Center;
		bottomBorderTmp.fontSize = 16;
		bottomBorderTmp.color = Color.black;
		bottomBorderTmp.text = "A simple line of text.";

		GameObject leftBorderBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
		leftBorderBackground.transform.LookAt(Vector3.down);
		leftBorderBackground.transform.position = new Vector3((nbRows - 1) * (diameter + offset) / 2, height/2, -(diameter + offset));
		GameObject leftBorderText = Instantiate(leftBorderBackground);
		leftBorderBackground.transform.Rotate(new Vector3(0f, 0f, -90f), Space.Self);
		leftBorderBackground.transform.localScale = new Vector3(diameter, nbRows * (diameter + offset), 1f);

		// Corner Quads

		GameObject topRightCornerQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		topRightCornerQuad.transform.localScale = new Vector3(diameter, diameter, 1f);
		topRightCornerQuad.transform.LookAt(Vector3.down);
		topRightCornerQuad.transform.position = new Vector3(-(diameter + offset), height/2, nbColumns * (diameter + offset));

		GameObject bottomRightCornerQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		bottomRightCornerQuad.transform.localScale = new Vector3(diameter, diameter, 1f);
		bottomRightCornerQuad.transform.LookAt(Vector3.down);
		bottomRightCornerQuad.transform.position = new Vector3(nbRows * (diameter + offset), height/2, nbColumns * (diameter + offset));

		GameObject bottomLeftCornerQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		bottomLeftCornerQuad.transform.localScale = new Vector3(diameter, diameter, 1f);
		bottomLeftCornerQuad.transform.LookAt(Vector3.down);
		bottomLeftCornerQuad.transform.position = new Vector3(nbRows * (diameter + offset), height/2, -(diameter + offset));

		GameObject topLeftCornerQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		topLeftCornerQuad.transform.localScale = new Vector3(diameter, diameter, 1f);
		topLeftCornerQuad.transform.LookAt(Vector3.down);
		topLeftCornerQuad.transform.position = new Vector3(-(diameter + offset), height/2, -(diameter + offset));

		client_MqttConnect();
	}


	void client_MqttConnect()
	{

		try
		{
			// create client instance 
			//client = new MqttClient(IPAddress.Parse("192.168.0.10"), 8080, false , null );
			client = new MqttClient(BROKER_ADDRESS, BROKER_PORT, false, null);

			// handle disconnect event
			client.MqttMsgDisconnected += client_MqttMsgDisconnected;

			// register to message received 
			client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

			string clientId = Guid.NewGuid().ToString();
			Debug.Log("Connecting to MQTT Broker @" + BROKER_ADDRESS + ":" + BROKER_PORT + " as " + clientId + " ...");
			client.Connect(clientId);
			Debug.Log("Connected.");


			// subscribe to the topic "/home/temperature" with QoS 2 
			client.Subscribe(new string[] { MQTT_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

			CancelInvoke("client_MqttConnect");
			InvokeRepeating("publishGetRequest", MQTT_DELAY_AT_START, MQTT_INTERVAL);

		}
		catch (Exception e0)
		{
			Debug.LogException(e0, this);
			Debug.Log("Trying to reconnect in  "  + MQTT_DELAY_RECONNECT + " secs...");
			//Invoke("client_MqttConnect", MQTT_DELAY_RECONNECT);

		}
	}

	void client_MqttMsgDisconnected(object sender, EventArgs e)
	{
		Debug.Log("Disconnected. Trying to reconnect in  " + MQTT_DELAY_RECONNECT + " secs...");
		//Invoke("client_MqttConnect", MQTT_DELAY_RECONNECT);

	}


	void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
	{
		try
		{
			#if DEBUG
				Debug.Log("Received: " + System.Text.Encoding.UTF8.GetString(e.Message));
			#endif

			GetAns gans = JsonUtility.FromJson<GetAns>(System.Text.Encoding.UTF8.GetString(e.Message));
			if (gans.GET != null && gans.ANS.status != null)
			{
				if (gans.ANS.status != MQTT_SUCCESS) Debug.LogWarning("GET -> " + gans.ANS.status);
				else {
					if(matrixMustBeUpdated == false){
						for (int i = 0; i < nbRows; i++)
						{
							for (int j = 0; j < nbColumns; j++)
							{
								modelMatrix[i, j].setStateTarget(
									gans.ANS.content.xAxisValue[i * nbColumns + j], // xAxisValue (-128, 127)
									gans.ANS.content.yAxisValue[i * nbColumns + j], // yAxisValue (-128, 127)
									gans.ANS.content.selectCountValue[i * nbColumns + j],  // selectCountValue (0, 255)
									gans.ANS.content.rotationValue[i * nbColumns + j],   // rotationValue (-128, 127)
									gans.ANS.content.positionValue[i * nbColumns + j], // positionValue (0, 40)
									(bool)(gans.ANS.content.reachingValue[i * nbColumns + j] == 1 ? true : false), // reachingValue (0, 1)
									(bool)(gans.ANS.content.holdingValue[i * nbColumns + j] == 1 ? true : false), // holdingValue (0, 1)
									MQTT_INTERVAL
									);
								
							}

						}
						matrixMustBeUpdated = true;
					}
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

	void publishGetRequest()
	{
		try { 
			// Create Get Request Object
			GetRequest greq = new GetRequest();
			// Convert it to JSON String
			string getJson = JsonUtility.ToJson(greq);
			// publish get request
			#if DEBUG
						Debug.Log("Sending...");
			#endif

			client.Publish(MQTT_TOPIC, System.Text.Encoding.UTF8.GetBytes(getJson), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);

			#if DEBUG
						Debug.Log("Sended: " + getJson);
			#endif

		}
		catch (Exception e2) {
			Debug.LogException(e2, this);
		}
	}

	void publishSetRequest(int[] position, float[] duration, int[] holding)
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

			#if DEBUG
						Debug.Log("Sending...");
			#endif

			// Publish it
			client.Publish(MQTT_TOPIC, System.Text.Encoding.UTF8.GetBytes(setJson), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);

			#if DEBUG
						Debug.Log("Sended: " + setJson);
			#endif
		}
		catch (Exception e3)
		{
			Debug.LogException(e3, this);
		}
	}

	void OnGUI()
	{
		GUI.skin = guiSkin;
		if (GUI.Button(new Rect(20, 40, 80, 20), "RESET"))
		{
			//this.publishSetRequest();
		}
	}

	// Update is called once per frame
	void Update () {
		// INPUT STATE
		if(matrixMustBeUpdated){	
			for (int i = 0; i < nbRows; i++)
			{
				for(int j = 0; j < nbColumns; j++)
				{
					matrix[i, j].GetComponent<ExpanDialStickView>().setStateTarget(
						modelMatrix[i, j].getTargetAxisX(),
						modelMatrix[i, j].getTargetAxisY(),
						modelMatrix[i, j].getTargetSelectCount(),
						modelMatrix[i, j].getTargetRotation(),
						modelMatrix[i, j].getTargetPosition(),
						modelMatrix[i, j].getTargetReaching(),
						modelMatrix[i, j].getTargetHolding(),
						modelMatrix[i, j].getTargetDuration()
					);
				}
			}
			matrixMustBeUpdated = false;
		}
		// INPUT EVENTS
		/*for (int i = 0; i < nbRows; i++)
		{
			for(int j = 0; j < nbColumns; j++)
			{
				float[] events = modelMatrix[i, j].readAndEraseStateDiffs();

				// !!! CAN HANDLE THE SAME EVENT ONLY ONE TIME
				if(events.Length > 6)
				{
					if (events[0] != 0f) //  X Axis events
					{
						Debug.Log("(" + i + ", " + j + ") X Axis Event.");
					}

					if (events[1] != 0f) //  Y Axis events
					{
						Debug.Log("(" + i + ", " + j + ") Y Axis Event.");
					}

					if (events[2] != 0f) // Select events
					{
						Debug.Log("(" + i + ", " + j + ") Select Event.");
					}

					if (events[3] != 0f) // Dial events
					{
						Debug.Log("(" + i + ", " + j + ") Dial Event.");
					}

					if (events[4] != 0f) // Encoder events
					{
						Debug.Log("(" + i + ", " + j + ") Encoder Event.");
					}

					if (events[5] != 0f) // Reaching events
					{
						Debug.Log("(" + i + ", " + j + ") Reaching Event.");
					}

					if (events[6] != 0f) // Holding events
					{
						Debug.Log("(" + i + ", " + j + ") Holding Event.");
					}
				}
			}
		}*/

		// PROCESS

		// OUTPUT
	}
}
