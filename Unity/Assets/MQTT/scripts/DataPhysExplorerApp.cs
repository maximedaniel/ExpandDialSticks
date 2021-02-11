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

public class PhysBarChart{
	List<int> rows;
	List<int> columns;
	List<string> xValues;
	List<int> yValues;
	public PhysBarChart(List<int> rows, List<int> columns, List<string> xValues, List<int> yValues){
		this.rows = rows;
		this.columns = columns;
		this.xValues = xValues;
		this.yValues = yValues;
	}

}

public class DataPhysExplorerApp : MonoBehaviour
{

    public GameObject expanDialSticksPrefab;
	private ExpanDialSticks expanDialSticks;
	private bool connected = false;

	// Data type indexes
	private const int PRODUCTION_INDEX = 0;
	private const int STORAGE_INDEX = 1;
	private const int CONSO_INDEX = 2;

	// X axis
	private const int MONTH_SCALE = 0;
	private const int DAY_SCALE = 1;
	private const int HOUR_SCALE = 2;
	
	private const int BUILDING_SCALE = 3;
	private const int ROOM_SCALE = 4;
	private const int DEVICE_SCALE = 5;

	private string[] BUILDING_NAMES = {"ESTIA1", "ESTIA2", "ESTIA3"};
	private string[] ROOM_NAMES = {"Halle", "Amphi1", "Amphi2"};
	private string[] DEVICE_NAMES = {"Light", "Heating", "Power Supply"};


	// Y axis
	private int ONE_TO_ONE_SCALE = 0;

	private Color PRODUCTION_COLOR = new Color(1f, 0f, 0f);
	private Color STORAGE_COLOR = new Color(1f, 0f, 0f);
	private Color CONSO_COLOR = new Color(0f, 0f, 1f);

	private Dictionary<string, (int, int, int)> data;
	
	private DateTime defaultDateTime = new DateTime(2021, 1, 1, 0, 0, 0);
	private int defaultXScale = MONTH_SCALE;
	private float defaultYScale = 1f;

	List<PhysBarChart> dataPhysList;

	// Interactions states
	private const int START = 0;
	private const int IN_PROGRESS = 1;
	private const int END = 2;

	private bool triggerChange = false;

	private List<ExpanDialStickModel> errors;
	private Dictionary<ExpanDialStickModel, int> clicks;
	private bool errorAnimating = false;
	private bool clickRendering = false;



	public static float RandomGaussian(float minValue = 0.0f, float maxValue = 1.0f)
	{
		float u, v, S;
	
		do
		{
			u = 2.0f * UnityEngine.Random.value - 1.0f;
			v = 2.0f * UnityEngine.Random.value - 1.0f;
			S = u * u + v * v;
		}
		while (S >= 1.0f);
	
		// Standard Normal Distribution
		float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
	
		// Normal Distribution centered between the min and max value
		// and clamped following the "three-sigma rule"
		float mean = (minValue + maxValue) / 2.0f;
		float sigma = (maxValue - mean) / 3.0f;
		return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
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

		clicks = new Dictionary<ExpanDialStickModel, int>();
		errors = new List<ExpanDialStickModel>();

		data = new Dictionary<string, (int, int, int)>();	
		// Create Fake Data
		
		DateTime prevDateTime = defaultDateTime;
		DateTime currDateTime = prevDateTime;

		do {
			// MONTH SCALE DATA
			if(prevDateTime.Month != currDateTime.Month){
				data.Add(currDateTime.ToString("MM"), (
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40)
					)
				);
				// BUILDING SCALE
				for(int i = 0; i < BUILDING_NAMES.Length; i++){
					data.Add(currDateTime.ToString("MM") + "|" + BUILDING_NAMES[i], (
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40)
						)
					);
				}
				
				// ROOM SCALE
				for(int i = 0; i < ROOM_NAMES.Length; i++){
					data.Add(currDateTime.ToString("MM") + "|" + BUILDING_NAMES[i] + "-" + ROOM_NAMES[i], (
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40)
						)
					);
				}

				// DEVICE SCALE
				for(int i = 0; i < ROOM_NAMES.Length; i++){
					data.Add(currDateTime.ToString("MM") + "|" + BUILDING_NAMES[i] + "-" + ROOM_NAMES[i]+ "-" + DEVICE_NAMES[i], (
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40)
						)
					);
				}

			// DAY SCALE DATA
			} else if (prevDateTime.Day != currDateTime.Day){
				data.Add(currDateTime.ToString("MM-dd"), (
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40)
					)
				);
				// BUILDING SCALE
				for(int i = 0; i < BUILDING_NAMES.Length; i++){
					data.Add(currDateTime.ToString("MM-dd") + "|" + BUILDING_NAMES[i], (
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40)
						)
					);
				}
				
				// ROOM SCALE
				for(int i = 0; i < ROOM_NAMES.Length; i++){
					data.Add(currDateTime.ToString("MM-dd") + "|" + BUILDING_NAMES[i] + "-" + ROOM_NAMES[i], (
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40)
						)
					);
				}

				// DEVICE SCALE
				for(int i = 0; i < ROOM_NAMES.Length; i++){
					data.Add(currDateTime.ToString("MM-dd") + "|" + BUILDING_NAMES[i] + "-" + ROOM_NAMES[i]+ "-" + DEVICE_NAMES[i], (
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40)
						)
					);
				}	
			} 
			// HOUR SCALE
			data.Add(currDateTime.ToString("MM-dd-HH"), (
					UnityEngine.Random.Range(1, 40),
					UnityEngine.Random.Range(1, 40),
					UnityEngine.Random.Range(1, 40)
				)
			);
			// BUILDING SCALE
			for(int i = 0; i < BUILDING_NAMES.Length; i++){
				data.Add(currDateTime.ToString("MM-dd-HH") + "|" + BUILDING_NAMES[i], (
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40)
					)
				);
			}
			
			// ROOM SCALE
			for(int i = 0; i < ROOM_NAMES.Length; i++){
				data.Add(currDateTime.ToString("MM-dd-HH") + "|" + BUILDING_NAMES[i] + "|" + ROOM_NAMES[i], (
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40)
					)
				);
			}

			// DEVICE SCALE
			for(int i = 0; i < ROOM_NAMES.Length; i++){
				data.Add(currDateTime.ToString("MM-dd-HH") + "|" + BUILDING_NAMES[i] + "|" + ROOM_NAMES[i]+ "|" + DEVICE_NAMES[i], (
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40)
					)
				);
			}	
			
			prevDateTime = currDateTime;
		}
		while((currDateTime = currDateTime.AddHours(1)).Year <= defaultDateTime.Year);

		expanDialSticks.client_MqttConnect();
 
	}
	private void CreateBarChart(int[] rows, int[] columns, string xValues, int yValues){

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
		//ClickOnTwoPins();

	}

	private void HandleDisconnected(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("ExpanDialSticks disconnected.");
		connected = false;
	}

	private void HandleXAxisChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandleXAxisChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
	}

	private void HandleYAxisChanged(object sender, ExpanDialStickEventArgs e)
	{

	}
	private void HandleClickChanged(object sender, ExpanDialStickEventArgs e)
	{
		// Look For simple click on two pins
		// Create Chart
		Debug.Log("HandleClickChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
		// Enable clicking only near reset position and at holding position
		if(clicks.ContainsKey(expanDialSticks[e.i,e.j])){
			clicks.Remove(expanDialSticks[e.i,e.j]);
		} else {
			clicks.Add(expanDialSticks[e.i,e.j], START);
		}
		/*Debug.Log("clicks.Count ->" + clicks.Count);

		// if two clicks then create Charts
		if(clicks.Count > 1){
			if((clicks[0].Row != clicks[1].Row) && (clicks[0].Column != clicks[1].Column)){
				Debug.Log("Non-axis Position!");
				errors = new List<ExpanDialStickModel>(clicks);
				return;
			}
			switch (defaultXScale)
			{
				case MONTH_SCALE:
				break;

				case DAY_SCALE:
				break;

				case HOUR_SCALE:
				break;

				default:
				break;
			}
			// get Data to display
		}*/
	}

	private void HandleRotationChanged(object sender, ExpanDialStickEventArgs e)
	{

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
	private void createBarChart(int rowStart, int colStart, int rowEnd, int colEnd, string xValues, int yValues){
		/*List<int> rows = new List<int>();
		if(rowStart > rowEnd) for(int i = rowStart; i <= rowEnd; i++)rows.Add(i);
		else for(int i = rowStart; i >= rowEnd; i--)rows.Add(i);
		List<int> columns = new List<int>();
		if(colStart > colEnd) for(int j = colStart; j <= rowEnd; j++)columns.Add(j);
		else for(int j = colStart; j >= colEnd; j--)columns.Add(j);*/

	}

	private void ClickOnTwoPins(){
		
		expanDialSticks[0, 1].TargetSelectCount += 1;
		expanDialSticks[0, 1].TargetShapeChangeDuration = 1f;
		expanDialSticks[0, 1].TargetText = "Clicked!";
		expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
		
		expanDialSticks[expanDialSticks.NbRows - 1, 0].TargetSelectCount += 1;
		expanDialSticks[expanDialSticks.NbRows - 1, 0].TargetShapeChangeDuration = 1f;
		expanDialSticks[expanDialSticks.NbRows - 1, 0].TargetText = "Clicked!";
		expanDialSticks[expanDialSticks.NbRows - 1, 0].TargetTextureChangeDuration = 1f;
		expanDialSticks.triggerShapeChange();
		expanDialSticks.triggerTextureChange();
	
		Debug.Log("ClickOnTwoPins() triggered.");
	}
	/*void RenderClicks(){
		if(clicks.Count > 0){
			if(!clickRendering){
			Debug.Log("RenderClicks() triggered.");
				foreach (var expandDialStick in clicks) {
					expandDialStick.TargetColor = Color.green;
					expandDialStick.TargetTextureChangeDuration = 0.25f;
					expanDialSticks.triggerTextureChange();
					expandDialStick.TargetPosition = 10;
					expandDialStick.TargetHolding = true;
					expandDialStick.TargetShapeChangeDuration = 1f;
					expanDialSticks.triggerShapeChange();
				}
				clickRendering = true;
				Invoke("RenderUnClicks", 2f);

			}
		}

	}

	void RenderUnClicks(){
		if(clicks.Count > 0){
			Debug.Log("RenderUnClicks() triggered.");
				foreach (var expandDialStick in clicks) {
					expandDialStick.TargetColor = Color.green;
					expandDialStick.TargetTextureChangeDuration = 0.25f;
					expanDialSticks.triggerTextureChange();
					expandDialStick.TargetPosition = expandDialStick.CurrentPosition;
					expandDialStick.TargetHolding = false;
					expandDialStick.TargetShapeChangeDuration = 1f;
					expanDialSticks.triggerShapeChange();
				}
		}
		clicks.Clear();
		clickRendering = false;
	}*/



	void RenderErrors(){
		if(errors.Count > 0){
			Debug.Log("RenderErrors() triggered.");

			if(!errorAnimating){
				foreach (var expandDialStick in errors) {
					expandDialStick.TargetColor = Color.red;
					expandDialStick.TargetTextureChangeDuration = 1f;
					expanDialSticks.triggerTextureChange();
				}
				errorAnimating = true;
			}
		}
		Invoke("ExitErrors", 3f);
	}

	void ExitErrors(){
		if(errors.Count > 0){
			if(errorAnimating){
				foreach (var expandDialStick in errors) {
					expandDialStick.TargetColor = Color.white;
					expandDialStick.TargetTextureChangeDuration = 1f;
					expanDialSticks.triggerTextureChange();
				}
				errors.Clear();
				errorAnimating = false;
			}
		}
	}


	void Update () {
		// check if ExpanDialSticks is connected
		if(connected){

			// Handle Error
			//RenderErrors();
			// Handle Clicks
			List<ExpanDialStickModel> clickKeys = new List<ExpanDialStickModel>(clicks.Keys);
			foreach(ExpanDialStickModel expanDialStick in clickKeys){
				if(clicks[expanDialStick] == START){
					triggerChange = true;
					expanDialStick.TargetColor = Color.green;
					expanDialStick.TargetTextureChangeDuration = 0.25f;
					
					expanDialStick.TargetPosition = 10;
					expanDialStick.TargetHolding = false;
					expanDialStick.TargetShapeChangeDuration = 1f;
					clicks[expanDialStick] = IN_PROGRESS;
				}
			}
			if(triggerChange){
				expanDialSticks.triggerTextureChange();
				expanDialSticks.triggerShapeChange();
				triggerChange = false;
			}
		}
    }
}
