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


public class DataPhysScenario : MonoBehaviour
{

    public GameObject expanDialSticksPrefab;
	private ExpanDialSticks expanDialSticks;
	private bool connected = false;

	private CultureInfo en = CultureInfo.CreateSpecificCulture("en-US");

	private IEnumerator coroutine;

    private int TimeChartSize = 5;
    private int[] TimeChartRows = new int[]{0, 1, 2, 3, 4};
    private int[] TimeChartColumns = new int[]{0, 0, 0, 0, 0};
    private int SpaceChartSize = 4;
    private int[] SpaceChartRows = new int[]{2, 2, 2, 2};
    private int[] SpaceChartColumns = new int[]{2, 3, 4, 5};


	IEnumerator UserScenario() 
	{
		
	   	// Task -> Compare ESTIA1 and ESTIA2 production for each hour of 14th february
		// Start at ESTIA3 rooms consumption for each month of 2020
		yield return new WaitForSeconds(2f);
		displayTimeConsumptionChart();
        // Rotate right to switch from cons to prod passing by stor, end at prod
		yield return new WaitForSeconds(2f);
		displayTimeStorageChart();
        // Rotate left to switch from time to space
		// Dual Bend outside to scale up from rooms to buildings
		// Bend left until ESTIA1 is in the middle
        // Rotate left to switch from space to time
		// Bend left until Feb is in the middle
		// Dual Bend outside Feb to scale down from months to days of Feb
		// Bend left until 14th Feb is in the middle
		// Bend perpendical to show subchart of ESTIA1 production hour per hour
		// Click ESTIA1 to lock it
        // Rotate left to switch from time to space
		// Bend left until ESTIA2 is beside ESTIA1
        // Rotate left to switch from space to time
		// Bend perpendical to show subchart of ESTIA2 production hour per hour
		// Click ESTIA2 to lock it
		// Dual bend subcharts of ESTIA1 and ESTIA2 to move by one hour
	}
	void displayTimeConsumptionChart(){
        string[] xLabels = new string[]{"1 January", "2 January", "3 January", "4 January", "5 January"};
        sbyte[] yValue = new sbyte[]{5, 10, 20, 30, 25};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yValue[i]/40f;
            Debug.Log(coeff);
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xLabels[i] + "\n" + yValue[i] + "Wh</b>";
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yValue[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    } 
	void displayTimeConsumptionChartPastOneDay(){
        string[] xLabels = new string[]{"31 December", "1 January", "2 January", "3 January", "4 January"};
        sbyte[] yValue = new sbyte[]{40, 5, 10, 20, 30};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yValue[i]/40f;
            Debug.Log(coeff);
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xLabels[i] + "\n" + yValue[i] + "Wh</b>";
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yValue[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    } 
    
	void displayTimeConsumptionChartPastTwoDays(){
        string[] xLabels = new string[]{"29 December", "30 December", "31 December", "1 January", "2 January"};
        sbyte[] yValue = new sbyte[]{20, 25, 40, 5, 10};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yValue[i]/40f;
            Debug.Log(coeff);
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xLabels[i] + "\n" + yValue[i] + "Wh</b>";
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yValue[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    } 
    void displaySpaceConsumptionChart(){
        string[] xLabels = new string[]{"Building 1", "Building 2", "Building 3", "Building 4"};
        sbyte[] yValue = new sbyte[]{20, 5, 10, 5};
        for(int i = 0; i < SpaceChartSize; i++){
            float coeff = yValue[i]/40f;
            Debug.Log(coeff);
            expanDialSticks[SpaceChartRows[i], SpaceChartColumns[i]].TargetText = xLabels[i] + "\n" + yValue[i] + "Wh</b>";
            expanDialSticks[SpaceChartRows[i], SpaceChartColumns[i]].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
		    expanDialSticks[SpaceChartRows[i], SpaceChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[SpaceChartRows[i], SpaceChartColumns[i]].TargetPosition = yValue[i];
		    expanDialSticks[SpaceChartRows[i], SpaceChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    } 

	void displayTimeProductionChart(){
        string[] xLabels = new string[]{"1 January", "2 January", "3 January", "4 January", "5 January"};
        sbyte[] yValue = new sbyte[]{25, 30, 20, 10, 5};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yValue[i]/40f;
            Debug.Log(coeff);
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xLabels[i] + "\n" + yValue[i] + "Wh</b>";
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yValue[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    } 
	void displayTimeStorageChart(){
        string[] xLabels = new string[]{"1 January", "2 January", "3 January", "4 January", "5 January"};
        sbyte[] yValue = new sbyte[]{5, 25, 40, 30, 10};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yValue[i]/40f;
            Debug.Log(coeff);
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xLabels[i] + "\n" + yValue[i] + "Wh</b>";
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f, 1f - (coeff * (1f-0.64f)), 1f - coeff);
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yValue[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
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
		coroutine = UserScenario();
        StartCoroutine(coroutine);

	}

	private void HandleDisconnected(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("ExpanDialSticks disconnected.");
		connected = false;
	}

	private void HandleXAxisChanged(object sender, ExpanDialStickEventArgs e)
	{
		Debug.Log("HandleXAxisChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
	}

	private void HandleYAxisChanged(object sender, ExpanDialStickEventArgs e)
	{
		Debug.Log("HandleYAxisChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
	}

	private void HandleClickChanged(object sender, ExpanDialStickEventArgs e)
	{
		Debug.Log("HandleClickChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
	}

	private void HandleRotationChanged(object sender, ExpanDialStickEventArgs e)
	{
		Debug.Log("HandleRotationChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
	}

	private void HandlePositionChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandlePositionChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");

	}

	private void HandleReachingChanged(object sender, ExpanDialStickEventArgs e)
	{

	}

	private void HandleHoldingChanged(object sender, ExpanDialStickEventArgs e)
	{

	}

	void Update () {
		// check if ExpanDialSticks is connected
		if(connected){
        }
    }
}
