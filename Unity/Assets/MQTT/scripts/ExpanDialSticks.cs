#define DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;

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

public class ExpanDialStick 
{

	public const float diameter = 4.0f;
	public const float height = 10.0f;
	public const float offset = 0.3f;

	public bool animated;
	public GameObject gameObject;
	public int i;
	public int j;

	public float xAxisCurrent;
	public float xAxisTarget;

	public float yAxisCurrent;
	public float yAxisTarget;

	public float selectCountCurrent;
	public float selectCountTarget;

	public float rotationCurrent;
	public float rotationTarget;

	public float positionCurrent;
	public float positionTarget;
	public float duration;


	public float reachingCurrent;
	public float reachingTarget;

	public float holdingCurrent;
	public float holdingTarget;

	public ExpanDialStick(int i, int j, Material transparentMaterial)
	{
		this.animated = false;
		this.i = i;
		this.j = j;
		this.xAxisCurrent = this.xAxisTarget = 0f;
		this.yAxisCurrent = this.yAxisTarget = 0f;
		this.selectCountCurrent = this.selectCountTarget =  0f;
		this.rotationCurrent = this.rotationTarget = 0f;
		this.positionCurrent = this.positionTarget = 0f;
		this.reachingCurrent = reachingTarget = 0f;
		this.holdingCurrent = holdingTarget = 0f;
		this.duration = 0f;

		this.gameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		this.gameObject.transform.GetComponent<MeshRenderer>().material = transparentMaterial;
		this.gameObject.transform.position = new Vector3(i * diameter + offset, this.positionCurrent, j * diameter + offset);
		this.gameObject.transform.localScale = new Vector3(diameter, height / 2, diameter);
	}
	public void set(sbyte xAxisTarget, sbyte yAxisTarget, byte selectCountTarget, sbyte rotationTarget, sbyte positionTarget, bool reachingTarget, bool holdingTarget, float duration)
	{

		float xAxisTargetNormal = Mathf.InverseLerp(-128, 127, xAxisTarget);
		this.xAxisTarget = Mathf.Lerp(-45f, 45f, xAxisTargetNormal);

		float yAxisTargetNormal = Mathf.InverseLerp(-128, 127, yAxisTarget);
		this.yAxisTarget = Mathf.Lerp(-45f, 45f, yAxisTargetNormal);

		this.selectCountTarget = Mathf.InverseLerp(0, 255, selectCountTarget);

		float rotationTargetNormal = Mathf.InverseLerp(-128, 127, rotationTarget);
		this.rotationTarget = Mathf.Lerp(-360f * 8, 360f * 8, rotationTargetNormal) % 360f;

		float positionTargetNormal = Mathf.InverseLerp(0, 40, positionTarget);
		this.positionTarget = Mathf.Lerp(0f, 10f, positionTargetNormal);

		this.reachingTarget = reachingTarget ? 1f : 0f;

		this.holdingTarget = holdingTarget ? 1f : 0f;

		this.duration = duration;

		animated = true;
	}

	public void animate()
	{
		if (animated)
		{
			if (this.duration > 0f)
			{


				this.xAxisCurrent += (this.xAxisTarget - this.xAxisCurrent) / this.duration * Time.deltaTime;
				this.yAxisCurrent += (this.yAxisTarget - this.yAxisCurrent) / this.duration * Time.deltaTime;
				this.selectCountCurrent += (this.selectCountTarget - this.selectCountCurrent) / this.duration * Time.deltaTime;
				this.rotationCurrent += (this.rotationTarget - this.rotationCurrent) / this.duration * Time.deltaTime;
				this.positionCurrent += (this.positionTarget - this.positionCurrent) / this.duration * Time.deltaTime;
				this.reachingCurrent += (this.reachingTarget - this.reachingCurrent) / this.duration * Time.deltaTime;
				this.holdingCurrent += (this.holdingTarget - this.holdingCurrent) / this.duration * Time.deltaTime;

				this.gameObject.transform.position = new Vector3(i * diameter + offset, this.positionCurrent, j * diameter + offset);
				this.gameObject.transform.rotation = Quaternion.identity;
				this.gameObject.transform.RotateAround(this.gameObject.transform.position - new Vector3(0f, height / 2, 0f), Vector3.up, (float)this.rotationCurrent);
				this.gameObject.transform.RotateAround(this.gameObject.transform.position - new Vector3(0f, height / 2, 0f), Vector3.left, (float)this.yAxisCurrent);
				this.gameObject.transform.RotateAround(this.gameObject.transform.position - new Vector3(0f, height / 2, 0f), Vector3.back, (float)this.xAxisCurrent);
				this.gameObject.transform.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f - this.holdingCurrent, 1.0f - this.holdingCurrent, 1.0f - this.reachingCurrent);

				this.duration -= Time.deltaTime;
			} 
			else animated = false;
		}
		
	}
}

public class ExpanDialSticks : MonoBehaviour
{
	public const string MQTT_BAD_VALUES = "bad values";
	public const string MQTT_WRONG_LENGTH = "wrong length";
	public const string MQTT_MISSING_KEY = "missing key";
	public const string MQTT_UNKNOWN_CMD = "unknown command";
	public const string MQTT_VALUE_ERROR = "json value error";
	public const string MQTT_SUCCESS = "success";

	public IPAddress BROKER_ADDRESS = IPAddress.Parse("192.168.0.10"); // "test.mosquitto.org";
	public int BROKER_PORT = 1883; // 8080; 
	public string MQTT_TOPIC = "ExpanDialSticks";
	public const float MQTT_DELAY_RECONNECT = 5f; // 0.2f;
	public const float MQTT_DELAY_AT_START = 2f; // 0.2f;
	public const float MQTT_INTERVAL = 0.2f; // 0.2f;
	public int prevMillis = 0;
	public int currMillis = 0;
	public const int nbColumns = 6;
	public const int nbRows = 5;

	public Material transparentMaterial;

	private MqttClient client;
	private ExpanDialStick [,] expanDialSticks = new ExpanDialStick[nbRows, nbColumns];

	// Use this for initialization
	void Start () {

		// Init ExpanDialSticks Matrix
		for (int i = 0; i < nbRows; i++)
			for (int j = 0; j < nbColumns; j++)
				expanDialSticks[i, j] = new ExpanDialStick(i, j, transparentMaterial);

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
			Invoke("client_MqttConnect", MQTT_DELAY_RECONNECT);

		}
	}

	void client_MqttMsgDisconnected(object sender, EventArgs e)
	{
		Debug.Log("Disconnected. Trying to reconnect in  " + MQTT_DELAY_RECONNECT + " secs...");
		Invoke("client_MqttConnect", MQTT_DELAY_RECONNECT);

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
				for (int i = 0; i < nbRows; i++)
				{
					for (int j = 0; j < nbColumns; j++)
					{
						expanDialSticks[i, j].set(
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


	void publishSetRequest()
	{
		try {
			int[] position = new int[nbRows * nbColumns];
			for (int i = 0; i < position.Length; i++) position[i] = -1;
			float[] duration = new float[nbRows * nbColumns];
			for (int i = 0; i < duration.Length; i++) duration[i] = 1f;
			int[] holding = new int[nbRows * nbColumns];
			for (int i = 0; i < holding.Length; i++) holding[i] = 0;

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
		catch (Exception e4) {
			Debug.LogException(e4, this);
		}
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(20, 40, 80, 20), "RESET"))
		{
			this.publishSetRequest();
		}
	}

	// Update is called once per frame
	void Update () {
		for (int i = 0; i < nbRows; i++)
		{
			for (int j = 0; j < nbColumns; j++)
			{
				expanDialSticks[i, j].animate();
			}

		}
	}
}
