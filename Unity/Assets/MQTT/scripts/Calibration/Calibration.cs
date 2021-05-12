#define DEBUG

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


public class Calibration : MonoBehaviour
{

    public GameObject expanDialSticksPrefab;
	public GUISkin guiSkin;
	private ExpanDialSticks expanDialSticks;
	private bool connected = false;

	private CultureInfo en = CultureInfo.CreateSpecificCulture("en-US");

	private IEnumerator coroutine;

	private const string LEFT_BENDING = "LEFT_BENDING";
	private const string RIGHT_BENDING = "RIGHT_BENDING";
	private const string TOP_BENDING = "TOP_BENDING";
	private const string BOTTOM_BENDING = "BOTTOM_BENDING";
	private const string LEFT_ROTATION = "LEFT_ROTATION";
	private const string RIGHT_ROTATION = "RIGHT_ROTATION";
	private const string PULL = "PULL";
	private const string PUSH = "PUSH";
	private const string CLICK = "CLICK";
	private const string NONE = "NONE";

	private const float JOYSTICK_THRESHOLD = 10f;

	private ConcurrentQueue<string> users;
	private ConcurrentQueue<string> inputs;
	private ConcurrentQueue<Action> outputs;

	void EnqueueIO() 
	{
		// 1. Show ESTIA1 consumption month by month
		inputs.Enqueue("2|0|" + CLICK);
		outputs.Enqueue( () => calibrate0());
		inputs.Enqueue("2|0|" + CLICK);
		outputs.Enqueue( () => calibrate1());
		inputs.Enqueue("2|0|" + CLICK);
		outputs.Enqueue( () => calibrate2());
		
	}
    
    void calibrate0(){
        for (int i = 0; i < expanDialSticks.NbRows; i++)
        {
            for (int j = 0; j < expanDialSticks.NbColumns; j++)
            {
                
                expanDialSticks[i, j].TargetColor = new Color(1f,1f,1f);
                expanDialSticks[i, j].TargetPlaneTexture = "cross";
                expanDialSticks[i, j].TargetTextureChangeDuration = 1f;
		
            }
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void calibrate1(){
        
        for (int i = 0; i < expanDialSticks.NbRows; i++)
        {
            for (int j = 0; j < expanDialSticks.NbColumns; j++)
            {
                
                expanDialSticks[i, j].TargetText = "17 Jan. 2021\n<b>150 MWh</b>\nESTIA1";
                expanDialSticks[i, j].TargetColor = new Color(1f,1f,1f);
                expanDialSticks[i, j].TargetPlaneTexture = "default";
                expanDialSticks[i, j].TargetTextureChangeDuration = 1f;
		
            }
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

        
    }
    void calibrate2(){
		/*for (int i = 0; i < expanDialSticks.NbRows; i++)
        {
            for (int j = 0; j < expanDialSticks.NbColumns; j++)
            {
                expanDialSticks[i, j].TargetPosition = 40;
                expanDialSticks[i, j].TargetShapeChangeDuration = 3f;
		
            }
        }*/
		expanDialSticks[4, 0].TargetPosition = 20;
		expanDialSticks[4, 0].TargetShapeChangeDuration = 3f;
		expanDialSticks[4, 1].TargetPosition = 20;
		expanDialSticks[4, 1].TargetShapeChangeDuration = 3f;
		expanDialSticks[3, 0].TargetPosition = 20;
		expanDialSticks[3, 0].TargetShapeChangeDuration = 3f;
		expanDialSticks[3, 1].TargetPosition = 20;
		expanDialSticks[3, 1].TargetShapeChangeDuration = 3f;
		expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

        
    }

	void DequeueOutput(){
		Action action;
		while (!outputs.TryDequeue(out action));
		action();
	}


	void Start () {
		expanDialSticks = expanDialSticksPrefab.GetComponent<ExpanDialSticks>();
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

		connected = false;

		outputs = new ConcurrentQueue<Action>();
		inputs = new ConcurrentQueue<string>();
		users = new ConcurrentQueue<string>();
		EnqueueIO();
		// Connection to MQTT Broker
		expanDialSticks.client_MqttConnect();
	}

	private void HandleConnecting(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("ExpanDialSticks connecting to MQTT Broker @" + e.address + ":" + e.port);
		connected = false;
	}

	private void HandleConnected(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("ExpanDialSticks connected.");
		connected = true;

	}

	private void HandleDisconnected(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("ExpanDialSticks disconnected.");
		connected = false;
	}

	private void HandleXAxisChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandleXAxisChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
		if(e.diff > 0 && e.next >= JOYSTICK_THRESHOLD) users.Enqueue(e.i + '|' + e.j + '|' + TOP_BENDING);
		if(e.diff < 0 && e.next <= -JOYSTICK_THRESHOLD) users.Enqueue(e.i + '|' + e.j + '|' + BOTTOM_BENDING);
	}

	private void HandleYAxisChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandleYAxisChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
		if(e.diff > 0 && e.next >= JOYSTICK_THRESHOLD) users.Enqueue(e.i + '|' + e.j + '|' + RIGHT_BENDING);
		if(e.diff < 0 && e.next <= -JOYSTICK_THRESHOLD) users.Enqueue(e.i + '|' + e.j + '|' + LEFT_BENDING);
	}

	private void HandleClickChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandleClickChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
		users.Enqueue(e.i + '|' + e.j + '|' + CLICK);
	}

	private void HandleRotationChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandleRotationChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
		if(e.diff > 0) users.Enqueue(e.i + '|' + e.j + '|' + RIGHT_ROTATION);
		else users.Enqueue(e.i + '|' + e.j + '|' + LEFT_ROTATION);
	}

	private void HandlePositionChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandlePositionChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
		if(e.diff > 0) users.Enqueue(e.i + '|' + e.j + '|' + PULL);
		else users.Enqueue(e.i + '|' + e.j + '|' + PUSH);

	}

	private void HandleReachingChanged(object sender, ExpanDialStickEventArgs e)
	{

	}

	private void HandleHoldingChanged(object sender, ExpanDialStickEventArgs e)
	{

	}

	/*void OnGUI()
	{
		GUI.skin = guiSkin;
		if(outputs.Count > 0 && inputs.Count > 0){
			if (GUI.Button(new Rect(20, 40, 80, 20), "NEXT"))
			{
				string input;
				while(!inputs.TryPeek (out input));
				Debug.Log("input > " + input);
				users.Enqueue(input);
			}
		}
	}*/

	IEnumerator UserScenario(){
		while(outputs.Count > 0){
				Action output;
				while(!outputs.TryDequeue (out output));
				Debug.Log("output > " + output);
				output();
				yield return new WaitForSeconds(1f);
		}
	}

	void Update () {
		// check if ExpanDialSticks is connected
		if(connected){
			 if (Input.GetKey("escape"))
            {
                Application.Quit();
            }

            if (Input.GetKeyDown("s")) 
            {
                StartCoroutine("UserScenario");
            }

            if (Input.GetKeyDown("n")) 
            {
                if(inputs.Count > 0){
                    string input;
                    while(!inputs.TryPeek (out input));
                    Debug.Log("input > " + input);
                    users.Enqueue(input);
                }
            }

			if(inputs.Count > 0){
				string input;
				while(!inputs.TryPeek (out input));
				while(users.Count > 0){
					string user;
					while(!users.TryDequeue (out user));
					if(user == input) {
						while(!inputs.TryDequeue (out input));
						this.DequeueOutput();
                        if(inputs.Count > 0){
                            string nextInput;
                            while(!inputs.TryPeek (out nextInput));
                            if(nextInput.EndsWith(NONE)) users.Enqueue(nextInput);
                        }
                        return;
					}
				}
			}
        }
    }
}
