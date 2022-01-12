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


public class DataPhysStudy : MonoBehaviour
{

    public GameObject expanDialSticksPrefab;
	public GUISkin guiSkin;
	private ExpanDialSticks expanDialSticks;
	private bool connected = false;

	private CultureInfo en = CultureInfo.CreateSpecificCulture("en-US");

	private IEnumerator coroutine;

    //private int TimeChartSize = 5;
    //private int[] TimeChartRows = new int[]{0, 1, 2, 3, 4};
    //private int[] TimeChartColumns = new int[]{0, 0, 0, 0, 0};

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
		outputs.Enqueue( () => display0());
		inputs.Enqueue("2|0|" + CLICK);
		outputs.Enqueue( () => display1());
		inputs.Enqueue("4|5|" + CLICK);
		outputs.Enqueue( () => Reset());
		
	}
	void DequeueOutput(){
		Action action;
		while (!outputs.TryDequeue(out action));
		action();
	}

	// 1.
	void display0(){
        ClearAllChange();
        string[] xTimes = new string[]{"Jan. 2021", "Feb. 2021", "Mar. 2021", "Apr. 2021", "May 2021"};
        string[] xSpaces = new string[]{"ESTIA1", "ESTIA2", "ESTIA3", "ESTIA4", "ESTIA5", "ESTIA6"};
        
        sbyte[] yData = new sbyte[]{12, 18, 25, 10, 20};
        string xAxisLegend = "<size=7><line-height=60>";
        for (int i = 0; i < xTimes.Length; i++)
        {
            xAxisLegend += xTimes[i] + "\n";
        }
        string yAxisLegend = "<size=7>";
        for (int i = 0; i < xSpaces.Length; i++)
        {
            yAxisLegend += xSpaces[i] + "<space=38>";
        }
        
        for(int i = 0; i < expanDialSticks.NbRows; i++){
            for(int j = 0; j < expanDialSticks.NbColumns; j++){
                int datum =  UnityEngine.Random.Range(0, 40);
                float coeff = datum/40f;
                expanDialSticks[i, j].TargetText = "<b>" + datum + " MWh</b>";
                expanDialSticks[i, j].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
                expanDialSticks[i, j].TargetTextureChangeDuration = 1f;

                expanDialSticks[i, j].TargetPosition = (sbyte)datum;
                expanDialSticks[i, j].TargetShapeChangeDuration = 1f;
            }
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		/*string legend = "<line-height=1em><voffset=-8><size=32><color=\"green\">•</voffset><size=8><color=\"black\">energy production\n"
		+ "<voffset=-8><size=32><color=\"orange\">•</voffset><size=8><color=\"black\">energy storage\n"
		+ "<voffset=-8><size=32><color=\"blue\">•</voffset><size=8><color=\"black\">energy consumption\n";*/
        
		/*string labels = "<size=7><pos=3%>Past month <pos=85%>Next month\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFFF>■ <color=#000000FF>Energy Consumption<pos=30%><alpha=#FF>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900AA>■ <color=#000000AA>Energy Storage<pos=30%><alpha=#AA>Space<pos=50%><alpha=#AA>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00AA>■ <color=#000000AA>Energy Production<pos=50%><alpha=#FF>Months<pos=75%><alpha=#FF>Buildings ► ESTIA1\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;*/
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, 16, Color.black, xAxisLegend, new Vector3(90f, -90f, 0f));
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 8, Color.black, yAxisLegend, new Vector3(90f, -90f, 0f));
        //expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, 16, Color.black, legend, new Vector3(90f, 90f, 0f));
	}


void display1(){
        ClearAllChange();
        string[] xTimes = new string[]{"Jan. 2021", "Feb. 2021", "Mar. 2021", "Apr. 2021", "May 2021"};
        string[] xSpaces = new string[]{"ESTIA1", "ESTIA2", "ESTIA3", "ESTIA4", "ESTIA5", "ESTIA6"};
        
        sbyte[] yData = new sbyte[]{12, 18, 25, 10, 20};
        string xAxisLegend = "<size=7><line-height=60>";
        for (int i = 0; i < xTimes.Length; i++)
        {
            xAxisLegend += xTimes[i] + "\n";
        }
        string yAxisLegend = "<size=7>";
        for (int i = 0; i < xSpaces.Length; i++)
        {
            yAxisLegend += xSpaces[i] + "<space=38>";
        }
        
        for(int i = 0; i < expanDialSticks.NbRows-1; i++){
            for(int j = 1; j < expanDialSticks.NbColumns; j++){
                int datum =  UnityEngine.Random.Range(0, 40);
                float coeff = datum/40f;
                expanDialSticks[i, j].TargetText = "<b>" + datum + " MWh</b>";
                expanDialSticks[i, j].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
                expanDialSticks[i, j].TargetTextureChangeDuration = 1f;

                expanDialSticks[i, j].TargetPosition = (sbyte)datum;
                expanDialSticks[i, j].TargetShapeChangeDuration = 1f;
            }
        }
        // x Axis Controller
         for(int i = 0; i < expanDialSticks.NbRows -1; i++){
                if( i == 0)  expanDialSticks[i, 0].TargetText = "<size=2>▲<size=1>\n" + xTimes[i];
                else if (i == expanDialSticks.NbRows -2) expanDialSticks[i, 0].TargetText = "<size=1>" + xTimes[i] + "\n<size=2>▼";
                else expanDialSticks[i, 0].TargetText = xTimes[i];
                expanDialSticks[i, 0].TargetColor = new Color(0.6f, 0.6f, 0.6f);
                expanDialSticks[i, 0].TargetTextureChangeDuration = 1f;
                expanDialSticks[i, 0].TargetPosition = (sbyte)10;
                expanDialSticks[i, 0].TargetShapeChangeDuration = 1f;
        }
        // y Axis Controller
         for(int j = 1; j < expanDialSticks.NbColumns; j++){
                //expanDialSticks[i, j].TargetText = "<b>" + datum + " MWh</b>";
                expanDialSticks[expanDialSticks.NbRows-1, j].TargetText = xSpaces[j];
                expanDialSticks[expanDialSticks.NbRows-1, j].TargetColor = new Color(0.6f, 0.6f, 0.6f);
                expanDialSticks[expanDialSticks.NbRows-1, j].TargetTextureChangeDuration = 1f;
                expanDialSticks[expanDialSticks.NbRows-1, j].TargetPosition = (sbyte)10;
                expanDialSticks[expanDialSticks.NbRows-1, j].TargetShapeChangeDuration = 1f;
        }


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, 16, Color.black, xAxisLegend, new Vector3(90f, -90f, 0f));
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 8, Color.black, yAxisLegend, new Vector3(90f, -90f, 0f));
	}

	void ClearAllChange(){
        for(int i =0; i < expanDialSticks.NbRows; i++){
            for(int j =0; j < expanDialSticks.NbColumns; j++){
                expanDialSticks[i, j].TargetTextAlignment = TextAlignmentOptions.Center;
                expanDialSticks[i, j].TargetTextSize = 1f;
                expanDialSticks[i, j].TargetTextColor = new Color(0f, 0f, 0f);
                expanDialSticks[i, j].TargetText = "";
                expanDialSticks[i, j].TargetColor = new Color(1f, 1f, 1f);
                expanDialSticks[i, j].TargetPlaneTexture = "default";
                expanDialSticks[i, j].TargetPlaneRotation = 0f;
                expanDialSticks[i, j].TargetTextureChangeDuration = 1f;

                expanDialSticks[i, j].TargetPosition = 0;
                expanDialSticks[i, j].TargetHolding = false;
                expanDialSticks[i, j].TargetShapeChangeDuration = 1f;
            }
        }
    }

	void Reset(){
        ClearAllChange();
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, "", new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, "", new Vector3(90f, 180f, 0f));
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

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
        
        // Display Next Input
        string nextInput;
        while(!inputs.TryPeek (out nextInput));
		//expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 8, Color.black, nextInput, new Vector3(90f, -90f, 0f));
                           
		//coroutine = UserScenario();
        //StartCoroutine(coroutine);

	}

	private void HandleDisconnected(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("ExpanDialSticks disconnected.");
		connected = false;
	}

	private void HandleXAxisChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandleXAxisChanged -> (" + e.i + "|" + e.j + "|" + e.prev + "|" + e.next  + "|" + e.diff + ")");
		if(e.diff > 0 && e.next >= JOYSTICK_THRESHOLD) users.Enqueue(e.i + "|" + e.j + "|" + BOTTOM_BENDING);
		if(e.diff < 0 && e.next <= -JOYSTICK_THRESHOLD) users.Enqueue(e.i + "|" + e.j + "|" + TOP_BENDING);
	}

	private void HandleYAxisChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandleYAxisChanged -> (" + e.i + "|" + e.j + "|" + e.prev + "|" + e.next  + "|" + e.diff + ")");
		if(e.diff > 0 && e.next >= JOYSTICK_THRESHOLD) users.Enqueue(e.i + "|" + e.j + "|" + LEFT_BENDING);
		if(e.diff < 0 && e.next <= -JOYSTICK_THRESHOLD) users.Enqueue(e.i + "|" + e.j + "|" + RIGHT_BENDING);
	}

	private void HandleClickChanged(object sender, ExpanDialStickEventArgs e)
	{
		Debug.Log("HandleClickChanged -> (" + e.i + "|" + e.j + "|" + e.diff + ")");
		users.Enqueue(e.i + "|" + e.j + "|" + CLICK);
	}

	private void HandleRotationChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandleRotationChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
		if(e.diff > 0) users.Enqueue(e.i + "|" + e.j + "|" + LEFT_ROTATION);
		else users.Enqueue(e.i + "|" + e.j + "|" + RIGHT_ROTATION);
	}

	private void HandlePositionChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandlePositionChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
		if(e.diff > 0) users.Enqueue(e.i + "|" + e.j + "|" + PULL);
		else users.Enqueue(e.i + "|" + e.j + "|" + PUSH);

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
					Debug.Log(input + " vs " + user);
					if(user == input) {

						while(!inputs.TryDequeue (out input));
						this.DequeueOutput();
						if(inputs.Count > 0){
                            string nextInput;
                            while(!inputs.TryPeek (out nextInput));
							// Display next Input
							//expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 8, Color.black, nextInput, new Vector3(90f, -90f, 0f));
                            if(nextInput.EndsWith(NONE)) users.Enqueue(nextInput);
                        } else {
							//expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 8, Color.black, "", new Vector3(90f, -90f, 0f));
						}
                        return;
					}
				}
			}
        }
    }
}
