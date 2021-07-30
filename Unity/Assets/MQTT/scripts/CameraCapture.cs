// Set an off-center projection, where perspective's vanishing
// point is not necessarily in the center of the screen.
//
// left/right/top/bottom define near plane size, i.e.
// how offset are corners of camera's near plane.
// Tweak the values and you can see camera's frustum change.


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.IO;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using TMPro;
using System;


public class CameraCapture : MonoBehaviour
{
    public int fileCounter;
    public KeyCode screenshotKey;
    public const string IMAGE_FULL_PATH = "C:\\Users\\m.daniel\\Pictures\\";
    public Camera _camera;
	private MqttClient client;
    //private bool toCapture = false;
    
	public IPAddress BROKER_ADDRESS = IPAddress.Parse("127.0.0.1"); // IPAddress.Parse("192.168.0.10"); "test.mosquitto.org";
	public int BROKER_PORT = 1883; // 8080; 
	private const string MQTT_LEAPMOTION_TOPIC = "LeapMotion";
	public float MQTT_DELAY_RECONNECT = 5f; // 0.2f;
	public float MQTT_DELAY_AT_START = 5f; // 0.2f;
	public float MQTT_INTERVAL = 0.2f; // 0.2f;
	public float EVENT_INTERVAL = 0.5f; // 0.2f;

    private ConcurrentStack<Texture2D> textures;

    private IEnumerator coroutine;

    public void Start(){
        textures = new ConcurrentStack<Texture2D>();
        client_MqttConnect();
        coroutine = RenderAndSendTexture();
        StartCoroutine(coroutine);
    }

    public IEnumerator RenderAndSendTexture()
    {
        while (true)
        {
            // Wait until all rendering + UI is done.
            yield return new WaitForEndOfFrame();
            // Create a texture the size of the screen, RGB24 format
            int width = Screen.width;
            int height = Screen.height;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            // Read screen contents into the texture
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            Destroy(tex);
            try { 
                client.Publish(MQTT_LEAPMOTION_TOPIC, bytes, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
            }
            catch (Exception e2) {
                Debug.LogException(e2, this);
            }
            yield return new WaitForSeconds(MQTT_INTERVAL);
        }
    }
    /*
    public void sendCameraImage(){
        Texture2D tex;
        
        if (!textures.TryPop(out tex))
        {
            //Debug.Log("Failed to pop a texture.");
        }
        else if (tex != null)
        {
            //Debug.Log("Popped a texture.");
            byte[] bytes = tex.EncodeToPNG();
            Destroy(tex);
            try { 
                client.Publish(MQTT_LEAPMOTION_TOPIC, bytes, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
            }
            catch (Exception e2) {
                Debug.LogException(e2, this);
            }
        }
        textures.Clear();
    }*/

    public void Update(){
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(screenshotKey))
        {
            //this.toCapture = true;
        }

    }

    public void client_MqttConnect()
	{
		try
		{
			// create client instance 
			//client = new MqttClient("test.mosquitto.org", 1883, false , null );
			client = new MqttClient(BROKER_ADDRESS, BROKER_PORT, false, null);

			// handle disconnect event
			client.MqttMsgDisconnected += client_MqttMsgDisconnected;

			// register to message received 
			//client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

			string clientId = Guid.NewGuid().ToString();
			
			Debug.Log("Connecting to MQTT Broker @" + BROKER_ADDRESS + ":" + BROKER_PORT + " as " + clientId + " ...");

            client.Connect(clientId);
            Debug.Log("Connected.");
            
            // subscribe to the topic "/home/temperature" with QoS 2 
            //client.Subscribe(new string[] { MQTT_LEAPMOTION_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE  });
            CancelInvoke("client_MqttConnect");
            //InvokeRepeating("sendCameraImage", MQTT_DELAY_AT_START, MQTT_INTERVAL);
		}
		catch (Exception e0)
		{
			Debug.LogException(e0, this);
			Debug.Log("Trying to reconnect in  "  + MQTT_DELAY_RECONNECT + " secs...");
			//Invoke("client_MqttConnect", MQTT_DELAY_RECONNECT);
		}
	}

	private void client_MqttMsgDisconnected(object sender, EventArgs e)
	{
		Debug.Log("Disconnected. Trying to reconnect in  " + MQTT_DELAY_RECONNECT + " secs...");
		//Invoke("client_MqttConnect", MQTT_DELAY_RECONNECT);
	}

	private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
	{
		try
		{
            byte[] bytes = e.Message;
            // For testing purposes, also write to a file in the project folder
            //File.WriteAllBytes(IMAGE_FULL_PATH + "SavedScreen.png", bytes);
		}
		catch (Exception e1)
		{
			Debug.LogException(e1, this);
		}
	}
}