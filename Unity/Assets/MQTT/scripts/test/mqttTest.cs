using UnityEngine;
using System.Collections;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;

using System;
/*

[Serializable]
public class GetRequest
{
	public byte[] GET;

}


[Serializable]
public class SetRequest
{
	[Serializable]
	public class Content
	{
		public byte[] position;
		public byte[] duration;
		public byte holding;
	}

	public Content SET;

	public SetRequest()
	{
		SET = new Content();
	}

	public void setPositionByteArray(int[] positionArray)
	{
		SET.position = new byte[positionArray.Length];
		for (int i =0; i < positionArray.Length; i++)
		{
			byte[] intBytes = BitConverter.GetBytes(positionArray[i]);
			if (BitConverter.IsLittleEndian) Array.Reverse(intBytes);
			SET.position[i] = intBytes[intBytes.Length-1];
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
		for (int i = 0; i < holdingArray.Length; i++) SET.holding |= (byte)( ( holdingArray[i] ? 1 : 0 ) << (7 - i) );
	}
}



public class mqttTest : MonoBehaviour {
	public string BROKER_ADDRESS = "test.mosquitto.org";
	public string MQTT_TOPIC = "ExpanDialSticks";
	public const int MAX_DIALSTICKS = 6;

	private MqttClient client;
	// Use this for initialization
	void Start () {
		// create client instance 
		//client = new MqttClient(IPAddress.Parse("192.168.0.10"), 8080, false , null );
		client = new MqttClient(BROKER_ADDRESS);

		// register to message received 
		client.MqttMsgPublishReceived += client_MqttMsgPublishReceived; 
		
		string clientId = Guid.NewGuid().ToString(); 
		client.Connect(clientId); 
		
		// subscribe to the topic "/home/temperature" with QoS 2 
		client.Subscribe(new string[] { MQTT_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE }); 

	}
	void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e) 
	{ 

		Debug.Log("Received: " + System.Text.Encoding.UTF8.GetString(e.Message)  );
	} 
	void publishGetRequest()
	{
		Debug.Log("sending...");
		// Create Get Request Object
		GetRequest greq = new GetRequest();
		// Convert it to JSON String
		string getJson = JsonUtility.ToJson(greq);
		client.Publish(MQTT_TOPIC, System.Text.Encoding.UTF8.GetBytes(getJson), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
		Debug.Log("sent");
	}
	void publishSetRequest()
	{
		int[] position = new int[MAX_DIALSTICKS] { 1, 15, 20, 30, 5, 18 };
		float[] duration = new float[MAX_DIALSTICKS] { 1.5f, 2.1f, 1f, 1.8f, 2.65f, 3f };
		bool[] holding = new bool[MAX_DIALSTICKS] { true, false, false, false, false, true };
		Debug.Log("sending...");
		// Create Set Request Object
		SetRequest sreq = new SetRequest();
		// Fill it
		sreq.setPositionByteArray(position);
		sreq.setDurationByteArray(duration);
		sreq.setHoldingByte(holding);
		// Convert it to JSON String
		string setJson = JsonUtility.ToJson(sreq);
		client.Publish(MQTT_TOPIC, System.Text.Encoding.UTF8.GetBytes(setJson), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
		Debug.Log("sent");
	}


	void OnGUI(){
		if ( GUI.Button (new Rect (20,40,80,20), "GET")) {
			this.publishGetRequest();
		}
		if (GUI.Button(new Rect(50, 40, 80, 20), "SET"))
		{
			this.publishSetRequest();
		}
	}
	// Update is called once per frame
	void Update () {



	}
}
*/