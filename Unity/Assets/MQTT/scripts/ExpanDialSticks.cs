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
	public int i, j;
	public float diff;
	public ExpanDialStickEventArgs(int i, int j, float diff){
		this.i = i;
		this.j = j;
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

	public bool SIMULATION = true;
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
	public float MQTT_DELAY_AT_START = 5f; // 0.2f;
	public float MQTT_INTERVAL = 0.2f; // 0.2f;
	public float EVENT_INTERVAL = 0.5f; // 0.2f;
	public float prevEventTimeCheck = 0f;
	public float currEventTimeCheck = 0f;
	public const int nbColumns = 6;
	public const int nbRows = 5;
	public float cameraDistanceFromMatrix = 30f;
	float xOffsetCamera = -3f; //5f

	public GameObject expanDialStickPrefab;
	public GUISkin guiSkin;

	private Camera mainCamera;

	private MqttClient client;

	private GameObject[,] gameObjectMatrix = new GameObject[nbRows, nbColumns];
	private ExpanDialStickView[,] viewMatrix = new ExpanDialStickView[nbRows, nbColumns];
	private ExpanDialStickModel[,] modelMatrix = new ExpanDialStickModel[nbRows, nbColumns];
	
    private GameObject topBorderText;
	private Vector3 textRotationTop;
	public TextMeshPro textMeshTop;
	private TextAlignmentOptions textAlignmentTop;
	private int textSizeTop;
	private Color textColorTop;
	private string textTop;

    private GameObject bottomBorderText;
	private Vector3 textRotationBottom;
	public TextMeshPro textMeshBottom;
	private TextAlignmentOptions textAlignmentBottom;
	private int textSizeBottom;
	private Color textColorBottom;
	private string textBottom;

    private GameObject leftBorderText;
	private Vector3 textRotationLeft;
	public TextMeshPro textMeshLeft;
	private TextAlignmentOptions textAlignmentLeft;
	private int textSizeLeft;
	private Color textColorLeft;
	private string textLeft;
	
    private GameObject rightBorderText;
	private Vector3 textRotationRight;
	public TextMeshPro textMeshRight;
	private TextAlignmentOptions textAlignmentRight;
	private int textSizeRight;
	private Color textColorRight;
	private string textRight;

	private bool shapeChanging = false;
	private bool textureChanging = false;

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
	
	public int NbRows{
		get => nbRows;
	}

	public int NbColumns{
		get => nbColumns;
	}

	// Use this for initialization
	void Start () {
		shapeChanging = textureChanging = false;
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
		topBorderText = Instantiate(topBorderBackground);
		topBorderBackground.transform.localScale = new Vector3(diameter, nbColumns * (diameter + offset), 1f);
		topBorderText.transform.Rotate(new Vector3(0f, 0f, 90f), Space.Self);
		topBorderText.transform.position += new Vector3(0f, 1f, 0f);

		textMeshTop = topBorderText.AddComponent<TextMeshPro>();
		textMeshTop.alignment = textAlignmentTop = TextAlignmentOptions.Center;
		textMeshTop.fontSize = textSizeTop = 16;
		textMeshTop.color = textColorTop =  Color.black;
		textMeshTop.text = textTop = "";
		textRotationTop = new Vector3(90f, 0f, 0f);

		GameObject rightBorderBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
		rightBorderBackground.transform.LookAt(Vector3.down);
		rightBorderBackground.transform.position = new Vector3((nbRows - 1) * (diameter + offset) / 2, height/2, nbColumns * (diameter + offset));
		rightBorderBackground.transform.Rotate(new Vector3(0f, 0f, 90f), Space.Self);
		rightBorderText = Instantiate(rightBorderBackground);
		rightBorderBackground.transform.localScale = new Vector3(diameter, nbRows * (diameter + offset), 1f);
		//rightBorderText.transform.Rotate(new Vector3(0f, 0f, 90f), Space.Self);
		textMeshRight = rightBorderText.AddComponent<TextMeshPro>();
		textMeshRight.alignment  = textAlignmentRight = TextAlignmentOptions.Center;
		textMeshRight.fontSize = textSizeRight = 16;
		textMeshRight.color = textColorRight = Color.black;
		textMeshRight.text = textRight = "";
		textRotationRight = new Vector3(90f, 0f, 0f);

		GameObject bottomBorderBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
		bottomBorderBackground.transform.LookAt(Vector3.down); ;
		bottomBorderBackground.transform.position = new Vector3(nbRows * (diameter + offset), height/2, ((nbColumns - 1) * (diameter + offset) / 2));
		bottomBorderText = Instantiate(bottomBorderBackground);
		bottomBorderBackground.transform.localScale = new Vector3(diameter, nbColumns * (diameter + offset), 1f);
		//bottomBorderText.transform.Rotate(new Vector3(0f, 0f, 90f), Space.Self);
		bottomBorderText.transform.position += new Vector3(0f, 1f, 0f);
		textMeshBottom = bottomBorderText.AddComponent<TextMeshPro>();
		textMeshBottom.alignment = textAlignmentBottom = TextAlignmentOptions.Center;
		textMeshBottom.fontSize = textSizeBottom = 16;
		textMeshBottom.color = textColorBottom = Color.black;
		textMeshBottom.text = textBottom = "";
		textRotationBottom = new Vector3(90f, 0f, 0f);
		

		GameObject leftBorderBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
		leftBorderBackground.transform.LookAt(Vector3.down);
		leftBorderBackground.transform.position = new Vector3((nbRows - 1) * (diameter + offset) / 2, height/2, -(diameter + offset));
		leftBorderText = Instantiate(leftBorderBackground);
		leftBorderBackground.transform.Rotate(new Vector3(0f, 0f, -90f), Space.Self);
		leftBorderBackground.transform.localScale = new Vector3(diameter, nbRows * (diameter + offset), 1f);

		textMeshLeft = leftBorderText.AddComponent<TextMeshPro>();
		textMeshLeft.alignment = textAlignmentLeft = TextAlignmentOptions.Center;
		textMeshLeft.fontSize = textSizeLeft = 16;
		textMeshLeft.color = textColorLeft = Color.black;
		textMeshLeft.text = textLeft = "";
		textRotationLeft = new Vector3(90f, 0f, 0f);

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

		//client_MqttConnect();
	}

	public void client_MqttConnect()
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
			
			OnConnecting(this,  new MqttConnectionEventArgs(BROKER_ADDRESS, BROKER_PORT));
			//Debug.Log("Connecting to MQTT Broker @" + BROKER_ADDRESS + ":" + BROKER_PORT + " as " + clientId + " ...");
			if(!SIMULATION){
				client.Connect(clientId);
				//Debug.Log("Connected.");
				
				// subscribe to the topic "/home/temperature" with QoS 2 
				client.Subscribe(new string[] { MQTT_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
				triggerShapeReset();
				CancelInvoke("client_MqttConnect");
				InvokeRepeating("publishGetRequest", MQTT_DELAY_AT_START, MQTT_INTERVAL);
			}
			
			OnConnected(this,  new MqttConnectionEventArgs(BROKER_ADDRESS, BROKER_PORT));

		}
		catch (Exception e0)
		{
			
			OnDisconnected(this,  new MqttConnectionEventArgs(BROKER_ADDRESS, BROKER_PORT));
			Debug.LogException(e0, this);
			//Debug.Log("Trying to reconnect in  "  + MQTT_DELAY_RECONNECT + " secs...");
			//Invoke("client_MqttConnect", MQTT_DELAY_RECONNECT);

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
				if (gans.ANS.status != MQTT_SUCCESS) Debug.LogWarning("GET -> " + gans.ANS.status);
				else {
					if(shapeChanging == false){
						for (int i = 0; i < nbRows; i++)
						{
							for (int j = 0; j < nbColumns; j++)
							{
								modelMatrix[i, j].setShapeChangeCurrent(
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
						shapeChanging = true;
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

			client.Publish(MQTT_TOPIC, System.Text.Encoding.UTF8.GetBytes(getJson), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);

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
			client.Publish(MQTT_TOPIC, System.Text.Encoding.UTF8.GetBytes(setJson), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);

			/*#if DEBUG
						Debug.Log("Sended: " + setJson);
			#endif*/
		}
		catch (Exception e3)
		{
			Debug.LogException(e3, this);
		}
	}

	/*void OnGUI()
	{
		GUI.skin = guiSkin;
		if (GUI.Button(new Rect(20, 40, 80, 20), "RESET"))
		{
			//this.publishSetRequest();
		}
	}*/
	
	/*public void prepareTextChange(int i, int j, TextAlignmentOptions textAlignment, float textSize, Color textColor, string text)
	{	
		this.textAlignments[i * nbColumns + j] = textAlignment;
		this.textSizes[i * nbColumns + j] = textSize;
		this.textColors[i * nbColumns + j] = textColor;
		this.texts[i * nbColumns + j] = text;
		this.textChanging[i * nbColumns + j]  = true;
	}


	public void executeTextChange()
	{	
		for (int i = 0; i < nbRows; i++){
			for (int j = 0; j < nbColumns; j++){
					if(this.textChanging[i * nbColumns + j]){
						this.modelMatrix[i, j].setText(
							textAlignments[i * nbColumns + j], 
							textSizes[i * nbColumns + j], 
							textColors[i * nbColumns + j], 
							texts[i * nbColumns + j]
						);
						this.viewMatrix[i, j].setText(
							textAlignments[i * nbColumns + j], 
							textSizes[i * nbColumns + j], 
							textColors[i * nbColumns + j], 
							texts[i * nbColumns + j]
						);
					}
			}
		}
		
		for(int i = 0; i < nbRows * nbColumns; i++) {
			this.textAlignments[i] = 0;
			this.textSizes[i] = 0;
			this.textColors[i] = new Color();
			this.texts[i] = "";
			this.textChanging[i]  = false;
		}
	}

	public void prepareColorChange(int i, int j, Color color, float duration)
	{	
		
		this.colors[i * nbColumns + j] = color;
		this.colorChangeDurations[i * nbColumns + j] = duration;
		this.colorChanging[i * nbColumns + j]  = true;
	}

	public void executeColorChange()
	{	
		for (int i = 0; i < nbRows; i++){
			for (int j = 0; j < nbColumns; j++){
					if(this.colorChanging[i * nbColumns + j]){
						this.modelMatrix[i, j].setColorTarget(this.colors[i * nbColumns + j], this.colorChangeDurations[i * nbColumns + j]);
						this.viewMatrix[i, j].setColorTarget(
										this.modelMatrix[i, j].getTargetColor(),
										this.modelMatrix[i, j].getTargetColorDuration()
						);
					}
			}
		}
		
		for(int i = 0; i < nbRows * nbColumns; i++) {
			this.colors[i] = new Color();
			this.colorChangeDurations[i] = 0f;
			this.colorChanging[i] = false;
		}
	}
	
	public void prepareShapeChange(int i, int j, int position, int holding, float duration)
	{
		// publish ?
		this.positions[i * nbColumns + j] = position;
		this.holdings[i * nbColumns + j] = holding;
		this.shapeChangeDurations[i * nbColumns + j] = duration;
		this.shapeChanging[i * nbColumns + j] = true;
	}
	
	public void executeShapeChange(){
		if(SIMULATION){
			for (int i = 0; i < nbRows; i++)
			{
				for(int j = 0; j < nbColumns; j++)
				{
					if(this.shapeChanging[i * nbColumns + j]){
						this.modelMatrix[i, j].setStateTarget(
							modelMatrix[i, j].getTargetAxisX(),
							modelMatrix[i, j].getTargetAxisY(),
							modelMatrix[i, j].getTargetSelectCount(),
							modelMatrix[i, j].getTargetRotation(),
							(sbyte)this.positions[i * nbColumns + j],
							modelMatrix[i, j].getTargetReaching(),
							this.holdings[i * nbColumns + j] > 0 ? true : false,
							this.shapeChangeDurations[i * nbColumns + j]
							);
						viewMatrix[i, j].setStateTarget(
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
			}
		}
		else {
			publishSetRequest(this.positions, this.shapeChangeDurations, this.holdings);
		}

		for(int i = 0; i < nbRows * nbColumns; i++) {
			this.positions[i] = 0;
			this.holdings[i] = 0;
			this.shapeChangeDurations[i] = 0f;
			this.shapeChanging[i] = false;
		}
	}*/

	public void setTopBorderText(TextAlignmentOptions textAlignment, int textSize, Color textColor, string text, Vector3 textRotation){         
		this.textAlignmentTop = textAlignment;
		this.textSizeTop = textSize;
		this.textColorTop = textColor;
		this.textTop = text;
		this.textRotationTop = textRotation;
	}
	
	public void setBottomBorderText(TextAlignmentOptions textAlignment, int textSize, Color textColor, string text, Vector3 textRotation){
		Debug.Log("setBottomBorderText -> " + text);
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
	
	public void setLeftBorderText(TextAlignmentOptions textAlignment, int textSize, Color textColor, string text, Vector3 textRotation){
		this.textAlignmentLeft = textAlignment;
		this.textSizeLeft = textSize;
		this.textColorLeft = textColor;
		this.textLeft = text;
		this.textRotationLeft = textRotation;
	}

	public void triggerShapeChange(){
		if(SIMULATION) {
			// set target shape to current 
			for (int i = 0; i < nbRows; i++)
			{
				for(int j = 0; j < nbColumns; j++)
				{
					modelMatrix[i, j].setShapeChangeCurrent(
						modelMatrix[i, j].TargetAxisX,
						modelMatrix[i, j].TargetAxisY,
						modelMatrix[i, j].TargetSelectCount,
						modelMatrix[i, j].TargetRotation,
						modelMatrix[i, j].TargetPosition,
						modelMatrix[i, j].TargetReaching,
						modelMatrix[i, j].TargetHolding,
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
					positions [i * nbColumns + j] = modelMatrix[i, j].TargetPosition;
					durations [i * nbColumns + j] = modelMatrix[i, j].TargetShapeChangeDuration;
					holdings [i * nbColumns + j] = modelMatrix[i, j].TargetHolding ? 1 : 0;
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
				durations [i * nbColumns + j] = 1f;
				holdings [i * nbColumns + j] = 0;
			}
		}
			publishSetRequest(positions, durations, holdings);
	}
	
	public void triggerTextureChange(){
		// set target texture to current
		for (int i = 0; i < nbRows; i++)
		{
			for(int j = 0; j < nbColumns; j++)
			{
				modelMatrix[i, j].setTextureChangeCurrent(
					modelMatrix[i, j].TargetColor,
					modelMatrix[i, j].TargetTextAlignment,
					modelMatrix[i, j].TargetTextSize,
					modelMatrix[i, j].TargetTextColor,
					modelMatrix[i, j].TargetText,
					modelMatrix[i, j].TargetTextureChangeDuration
				);
					modelMatrix[i, j].TargetTextureChangeDuration = 0f;
			}
		}
		textureChanging = true;
	}
	
	public ExpanDialStickModel this [int i, int j]{
		get => modelMatrix[i, j];
    	set => modelMatrix[i, j] = value;
	}
	// Update is called once per frame
	void Update () {
		// UPDATE VIEW FROM MODEL
		if(shapeChanging){	
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
						modelMatrix[i, j].CurrentShapeChangeDuration
					);
				}
			}
			shapeChanging = false;
		}
		
		if(textureChanging){
			for (int i = 0; i < nbRows; i++)
			{
				for(int j = 0; j < nbColumns; j++)
				{
					viewMatrix[i, j].setTextureChangeTarget(
						modelMatrix[i, j].CurrentColor,
						modelMatrix[i, j].CurrentTextAlignment,
						modelMatrix[i, j].CurrentTextSize,
						modelMatrix[i, j].CurrentTextColor,
						modelMatrix[i, j].CurrentText,
						modelMatrix[i, j].CurrentTextureChangeDuration
					);
				}
			}
			textureChanging = false;
		}	
		// SCAN EVENTS FROM VIEW
		if((currEventTimeCheck += Time.deltaTime) - prevEventTimeCheck > EVENT_INTERVAL){
			for (int i = 0; i < nbRows; i++)
			{
				for(int j = 0; j < nbColumns; j++)
				{
					float[] events = modelMatrix[i, j].readAndEraseShapeDiffs();

					// !!! CAN HANDLE THE SAME EVENT ONLY ONE TIME
					if(events.Length > 6)
					{
						if (events[0] != 0f) //  X Axis events
						{	// Trigger event
							OnXAxisChanged(this,  new ExpanDialStickEventArgs(i, j, events[0]));
							//Debug.Log("(" + i + ", " + j + ") X Axis Event.");
						}

						if (events[1] != 0f) //  Y Axis events
						{
							OnYAxisChanged(this,  new ExpanDialStickEventArgs(i, j, events[1]));
							//Debug.Log("(" + i + ", " + j + ") Y Axis Event.");
						}
						if (events[3] != 0f) // Rotation events
						{
							OnRotationChanged(this,  new ExpanDialStickEventArgs(i, j, events[3]));
							//Debug.Log("(" + i + ", " + j + ") Dial Event.");
						}

						if (events[4] != 0f && !modelMatrix[i, j].CurrentHolding && !modelMatrix[i, j].CurrentReaching) // Push/Pull events
						{
							OnPositionChanged(this,  new ExpanDialStickEventArgs(i, j, events[4]));
							//Debug.Log("(" + i + ", " + j + ") Encoder Event.");
						}

						if (events[2] != 0f && (events[4] == 0f || (events[4] != 0f && modelMatrix[i, j].CurrentHolding) )) // Click events
						{
							OnClickChanged(this,  new ExpanDialStickEventArgs(i, j, events[2]));
							//Debug.Log("(" + i + ", " + j + ") Select Event.");
						}



						if (events[5] != 0f) // Reaching events
						{
							onReachingChanged(this,  new ExpanDialStickEventArgs(i, j, events[5]));
							//Debug.Log("(" + i + ", " + j + ") Reaching Event.");
						}

						if (events[6] != 0f) // Holding events
						{
							onHoldingChanged(this,  new ExpanDialStickEventArgs(i, j, events[6]));
							//Debug.Log("(" + i + ", " + j + ") Holding Event.");
						}
					}
				}
			}
			prevEventTimeCheck = currEventTimeCheck;
		}

		// PROCESS

		// RENDER
		

		textMeshTop.alignment  = textAlignmentTop;
		textMeshTop.fontSize = textSizeTop;
		textMeshTop.color = textColorTop;
		textMeshTop.text = textTop;
		topBorderText.transform.eulerAngles = textRotationTop; 

		textMeshBottom.alignment  = textAlignmentBottom;
		textMeshBottom.fontSize = textSizeBottom;
		textMeshBottom.color = textColorBottom;
		textMeshBottom.text = textBottom;
		bottomBorderText.transform.eulerAngles = textRotationBottom; 

		textMeshRight.alignment  = textAlignmentRight;
		textMeshRight.fontSize = textSizeRight;
		textMeshRight.color = textColorRight;
		textMeshRight.text = textRight;
		rightBorderText.transform.eulerAngles = textRotationRight; 
		
		textMeshLeft.alignment  = textAlignmentLeft;
		textMeshLeft.fontSize = textSizeLeft;
		textMeshLeft.color = textColorLeft;
		textMeshLeft.text = textLeft;
		leftBorderText.transform.eulerAngles = textRotationLeft; 
		
	}
}
