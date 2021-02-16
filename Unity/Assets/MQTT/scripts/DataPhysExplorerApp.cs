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


public class PhysBarModel{
	private PhysBarChartModel chart;
	private string label;
	private DateTime xTime;
	private string xSpace;
	private int yDatum;
	private int timeScale;
	private int spaceScale;
	private float datumScale;
	private int datumType;
	private int xMode;

	public PhysBarChartModel Chart{ get => chart; set => chart = value;}
	public string Label{ get => label; set => label = value;}
	public DateTime Time{ get => xTime; set => xTime = value;}
	public string Space{ get => xSpace; set => xSpace = value;}
	public int Datum{ get => yDatum; set => yDatum = value;}
	public int TimeScale{ get => timeScale; set => timeScale = value;}
	public int SpaceScale{ get => spaceScale; set => spaceScale = value;}
	public float DatumScale{ get => datumScale; set => datumScale = value;}
	public int DatumType{ get => datumType; set => datumType = value;}
	public int Mode{ get => xMode; set => xMode = value;}

	/*public PhysBarModel(string label, DateTime xTime, string xSpace, int yDatum, int datumType, int timeScale, int spaceScale, int datumScale){
		this.label = label;
		this.xTime = xTime;
		this.xSpace = xSpace;
		this.yDatum = yDatum;
		this.datumType = datumType;
		this.timeScale = timeScale;
		this.spaceScale = spaceScale;
		this.datumScale = datumScale;
	}*/
}

public class PhysBarChartModel{
	private List<PhysBarModel> bars;
	private List<string> labels;
	private List<DateTime> xTimes;
	private List<string> xSpaces;
	private List<int> yData;
	private int timeScale;
	private int spaceScale;
	private float dataScale;
	private int dataType;
	private int xMode;

	public List<PhysBarModel> Bars{ get => bars; set => bars = value;}
	public List<string> Labels{ get => labels; set => labels = value;}
	public List<DateTime> Times{ get => xTimes; set => xTimes = value;}
	public List<string> Spaces{ get => xSpaces; set => xSpaces = value;}
	public List<int> Data{ get => yData; set => yData = value;}
	public int TimeScale{ get => timeScale; set => timeScale = value;}
	public int SpaceScale{ get => spaceScale; set => spaceScale = value;}
	public float DataScale{ get => dataScale; set => dataScale = value;}
	public int DataType{ get => dataType; set => dataType = value;}
	public int Mode{ get => xMode; set => xMode = value;}
	
	/*public PhysBarChartModel(List<string> labels, List<DateTime> xTimes, List<string> xSpaces, List<int> yData, int dataType, int timeScale, int spaceScale, int dataScale){
		this.labels = labels;
		this.xTimes = xTimes;
		this.xSpaces = xSpaces;
		this.yData = yData;
		this.dataType = dataType;
		this.timeScale = timeScale;
		this.spaceScale = spaceScale;
		this.dataScale = dataScale;
	}*/
}

public class DataPhysExplorerApp : MonoBehaviour
{

    public GameObject expanDialSticksPrefab;
	private ExpanDialSticks expanDialSticks;
	private bool connected = false;

	// Axis Mode
	private const int TIME_MODE = 0;
	private const int SPACE_MODE = 1;

	// Data type indexes
	private const int PRODUCTION_DATA = 0;
	private const int STORAGE_DATA = 1;
	private const int CONSUMPTION_DATA= 2;

	// X axis
	private const int MONTH_SCALE = 0;
	private const int DAY_SCALE = 1;
	private const int HOUR_SCALE = 2;
	
	private const int NO_SPACE_SCALE = 3;
	private const int BUILDING_SCALE = 4;
	private const int ROOM_SCALE = 5;
	private const int DEVICE_SCALE = 6;

	private string[] BUILDING_NAMES = {"ESTIA1", "ESTIA2", "ESTIA3"};
	private string[] ROOM_NAMES = {"Halle", "Amphi1", "Amphi2"};
	private string[] DEVICE_NAMES = {"Light", "Heating", "Power Supply"};


	// Y axis
	private int ONE_TO_ONE_SCALE = 0;

	private Dictionary<string, List<int>> data;
	
	private DateTime defaultDateTime = new DateTime(2021, 1, 1, 0, 0, 0);
	private int defaultTimeXScale = MONTH_SCALE;
	private int defaultSpaceXScale = BUILDING_SCALE;
	private float defaultYScale = 1f;
	private int defaultDataType = CONSUMPTION_DATA;
	private int defaultXMode = TIME_MODE;


	// Interactions states
	private const int START = 0;
	private const int IN_PROGRESS = 1;
	private const int END = 2;

	private bool triggerChange = false;

	private List<ExpanDialStickModel> errors;
	private bool errorAnimating = false;
	private Dictionary<ExpanDialStickModel, int> clicks;
	private bool clickRendering = false;
	

	// Model mapping
	private ConcurrentDictionary<ExpanDialStickModel, PhysBarModel> pinsToBars;
	private ConcurrentDictionary<PhysBarModel, ExpanDialStickModel> barsToPins;

	private ConcurrentDictionary<PhysBarChartModel, int> stateBarCharts;

	// Event mapping
	private ConcurrentQueue<PhysBarChartModel> renderBarCharts;
	private ConcurrentQueue<PhysBarModel> renderBars;
	private ConcurrentQueue<PhysBarChartModel> changeDataTypes;

	private bool dialRendering = false;

	private CultureInfo en = CultureInfo.CreateSpecificCulture("en-US");

	private IEnumerator coroutine;

	IEnumerator UserScenario() 
	{
		yield return new WaitForSeconds(3f);
		// Rotate Right -> (0, 2)
		expanDialSticks[0, 2].TargetRotation += 1;
		expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;
		expanDialSticks.triggerShapeChange();
		yield return new WaitForSeconds(1f);
		expanDialSticks[0, 2].TargetRotation += 1;
		expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;
		expanDialSticks.triggerShapeChange();
		yield return new WaitForSeconds(1f);
		expanDialSticks[0, 2].TargetRotation += 1;
		expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;
		expanDialSticks.triggerShapeChange();
	}
	
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
		changeDataTypes = new ConcurrentQueue<PhysBarChartModel>();
		errors = new List<ExpanDialStickModel>();

		pinsToBars = new ConcurrentDictionary<ExpanDialStickModel, PhysBarModel>();
		barsToPins = new ConcurrentDictionary<PhysBarModel, ExpanDialStickModel>();
		stateBarCharts = new ConcurrentDictionary<PhysBarChartModel, int>();
		renderBarCharts = new ConcurrentQueue<PhysBarChartModel>();
		renderBars = new ConcurrentQueue<PhysBarModel>();


		data = new Dictionary<string, List<int>>();	
		// Create Fake Data
		
		DateTime prevDateTime = defaultDateTime.AddHours(-1);
		DateTime currDateTime = defaultDateTime;

		do {
			// MONTH SCALE DATA
			if(prevDateTime.Month != currDateTime.Month){
				data.Add(currDateTime.ToString("MM"),
					new List<int>()
					{
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40)
					}
				);
				// BUILDING SCALE
				for(int i = 0; i < BUILDING_NAMES.Length; i++){
					data.Add(currDateTime.ToString( "MM") + "|" + BUILDING_NAMES[i], 
						new List<int>()
						{
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40)
						}
					);
				}
				
				// ROOM SCALE
				for(int i = 0; i < ROOM_NAMES.Length; i++){
					data.Add(currDateTime.ToString( "MM") + "|" + BUILDING_NAMES[i] + "-" + ROOM_NAMES[i], 
						new List<int>()
						{
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40)
						}
					);
				}

				// DEVICE SCALE
				for(int i = 0; i < ROOM_NAMES.Length; i++){
					data.Add(currDateTime.ToString( "MM") + "|" + BUILDING_NAMES[i] + "-" + ROOM_NAMES[i]+ "-" + DEVICE_NAMES[i], 
						new List<int>()
						{
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40)
						}
					);
				}

			// DAY SCALE DATA
			} else if (prevDateTime.Day != currDateTime.Day){
				data.Add(currDateTime.ToString( "MM-dd"), 
						new List<int>()
						{
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40)
						}
				);
				// BUILDING SCALE
				for(int i = 0; i < BUILDING_NAMES.Length; i++){
					data.Add(currDateTime.ToString( "MM-dd") + "|" + BUILDING_NAMES[i], 
						new List<int>()
						{
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40)
						}
					);
				}
				
				// ROOM SCALE
				for(int i = 0; i < ROOM_NAMES.Length; i++){
					data.Add(currDateTime.ToString( "MM-dd") + "|" + BUILDING_NAMES[i] + "-" + ROOM_NAMES[i], 
						new List<int>()
						{
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40)
						}
					);
				}

				// DEVICE SCALE
				for(int i = 0; i < ROOM_NAMES.Length; i++){
					data.Add(currDateTime.ToString( "MM-dd") + "|" + BUILDING_NAMES[i] + "-" + ROOM_NAMES[i]+ "-" + DEVICE_NAMES[i], 
						new List<int>()
						{
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40),
							UnityEngine.Random.Range(1, 40)
						}
					);
				}	
			} 
			// HOUR SCALE
			data.Add(currDateTime.ToString( "MM-dd-HH"), 
					new List<int>()
					{
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40)
					}
			);
			// BUILDING SCALE
			for(int i = 0; i < BUILDING_NAMES.Length; i++){
				data.Add(currDateTime.ToString( "MM-dd-HH") + "|" + BUILDING_NAMES[i], 
					new List<int>()
					{
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40)
					}
				);
			}
			
			// ROOM SCALE
			for(int i = 0; i < ROOM_NAMES.Length; i++){
				data.Add(currDateTime.ToString( "MM-dd-HH") + "|" + BUILDING_NAMES[i] + "|" + ROOM_NAMES[i], 
					new List<int>()
					{
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40)
					}
				);
			}

			// DEVICE SCALE
			for(int i = 0; i < ROOM_NAMES.Length; i++){
				data.Add(currDateTime.ToString( "MM-dd-HH") + "|" + BUILDING_NAMES[i] + "|" + ROOM_NAMES[i]+ "|" + DEVICE_NAMES[i], 
					new List<int>()
					{
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40),
						UnityEngine.Random.Range(1, 40)
					}
				);
			}	
			
			prevDateTime = currDateTime;
		}
		while((currDateTime = currDateTime.AddHours(1)).Year <= defaultDateTime.Year);

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
		DefaultChart();
		coroutine = UserScenario();
        StartCoroutine(coroutine);
		//ClickOnTwoPins();

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
		// Look For simple click on two pins
		// Create Chart
		Debug.Log("HandleClickChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
		// Enable clicking only near reset position and at holding position
		if(clicks.ContainsKey(expanDialSticks[e.i,e.j])){
			clicks.Remove(expanDialSticks[e.i,e.j]);
		} else {
			clicks.Add(expanDialSticks[e.i,e.j], START);
		}
	}

	private void HandleRotationChanged(object sender, ExpanDialStickEventArgs e)
	{
		Debug.Log("HandleRotationChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
		if(e.diff > 0){ //turn right then change Data Type
			if(pinsToBars.ContainsKey(expanDialSticks[e.i,e.j])){
				PhysBarModel bar = pinsToBars[expanDialSticks[e.i,e.j]];
				bar.DatumType = ((bar.DatumType + 1) % 3);
				UpdateBarChartFromSelectedBar(bar.Chart, bar);
				renderBarCharts.Enqueue(bar.Chart);
			}
		} else { // turn left then change Time to Space 

		}
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
	
	private void UpdateBarChartFromSelectedBar(PhysBarChartModel chart, PhysBarModel selectedBar)
	{
		// Mode changed
		if(chart.Mode != selectedBar.Mode){ 
		}
		DateTime xDateTimeValue = chart.Times[0];

		// Update Chart from selected bar
		chart.DataType = selectedBar.DatumType;
		chart.TimeScale = selectedBar.TimeScale;
		chart.SpaceScale = selectedBar.SpaceScale;
		chart.DataScale = selectedBar.DatumScale;
		chart.Mode = selectedBar.Mode;

		List<string> labels = new List<string>();
		List<DateTime> xTimes = new List<DateTime>();
		List<string> xSpaces = new List<string>();
		List<int> yData = new List<int>();


		for (int i = 0;  i < chart.Bars.Count; i++){
			PhysBarModel bar = chart.Bars[i];
			string label = "";
			DateTime xTime = DateTime.Now;
			string xSpace = "";
			int yDatum = 0;

			switch(chart.TimeScale){
				case MONTH_SCALE:
				xTime = xDateTimeValue;
				xSpace = "ESTIA1";
				yDatum = data[xDateTimeValue.ToString( "MM")][chart.DataType];
				label = xDateTimeValue.ToString("yyyy MMMM", en) + "\n<i>" + xSpace + "</i>\n<b>" + yDatum + "Wh</b>";
				if(chart.Mode == TIME_MODE) xDateTimeValue = xDateTimeValue.AddMonths(1);
				break;

				case DAY_SCALE:
				xTime = xDateTimeValue;
				xSpace = "ESTIA1";
				yDatum = data[xDateTimeValue.ToString("MM dd")][chart.DataType];
				label = xDateTimeValue.ToString("MMMM dd", en) + "\n<i>" + xSpace + "</i>\n<b>" + yDatum + "Wh</b>";
				if(chart.Mode == TIME_MODE) xDateTimeValue = xDateTimeValue.AddDays(1);
				break;

				case HOUR_SCALE:
				xTime = xDateTimeValue;
				xSpace = "ESTIA1";
				yDatum = data[xDateTimeValue.ToString("MM dd HH")][chart.DataType];
				label = xDateTimeValue.ToString("hh:mm tt", en) + "\n<i>" + xSpace + "</i>\n <b>" + yDatum + "Wh</b>";
				if(chart.Mode == TIME_MODE) xDateTimeValue = xDateTimeValue.AddHours(1);
				break;

				default:
				break;
			}
			labels.Add(label);
			xTimes.Add(xTime);
			xSpaces.Add(xSpace);
			yData.Add(yDatum);

			// fill bar
			bar.Label = label;
			bar.Time =  xTime;
			bar.Space = xSpace;
			bar.Datum = yDatum;
			bar.DatumType = chart.DataType;
			bar.TimeScale = chart.TimeScale;
			bar.SpaceScale = chart.SpaceScale;
			bar.DatumScale = chart.DataScale;
		}
		chart.Labels = labels;
		chart.Times =  xTimes;
		chart.Spaces = xSpaces;
		chart.Data = yData;

	}


	private void DefaultChart(){
		List<PhysBarModel> bars = new List<PhysBarModel>();
		List<string> labels = new List<string>();
		List<DateTime> xTimes = new List<DateTime>();
		List<string> xSpaces = new List<string>();
		List<int> yData = new List<int>();
		
		DateTime xDateTimeValue = defaultDateTime;

		PhysBarChartModel chart = new PhysBarChartModel();
		for (int j = 0;  j < expanDialSticks.NbColumns; j++){
			PhysBarModel bar = new PhysBarModel();
			string label = "";
			DateTime xTime = DateTime.Now;
			string xSpace = "";
			int yDatum = 0;

			switch(defaultTimeXScale){
				case MONTH_SCALE:
				xTime = xDateTimeValue;
				xSpace = "ESTIA1";
				yDatum = data[xDateTimeValue.ToString( "MM")][defaultDataType];
				label = xDateTimeValue.ToString("yyyy MMMM", en) + "\n<i>" + xSpace + "</i>\n<b>" + yDatum + "Wh</b>";
				xDateTimeValue = xDateTimeValue.AddMonths(1);
				break;

				case DAY_SCALE:
				xTime = xDateTimeValue;
				xSpace = "ESTIA1";
				yDatum = data[xDateTimeValue.ToString("MM dd")][defaultDataType];
				label = xDateTimeValue.ToString("MMMM dd", en) + "\n<i>" + xSpace + "</i>\n<b>" + yDatum + "Wh</b>";
				xDateTimeValue = xDateTimeValue.AddDays(1);
				break;

				case HOUR_SCALE:
				xTime = xDateTimeValue;
				xSpace = "ESTIA1";
				yDatum = data[xDateTimeValue.ToString("MM dd HH")][defaultDataType];
				label = xDateTimeValue.ToString("hh:mm tt", en) + "\n<i>" + xSpace + "</i>\n <b>" + yDatum + "Wh</b>";
				xDateTimeValue = xDateTimeValue.AddHours(1);
				break;

				default:
				break;
			}
			// fill bar
			bar.Chart = chart;
			bar.Label = label;
			bar.Time =  xDateTimeValue;
			bar.Space = xSpace;
			bar.Datum = yDatum;
			bar.DatumType = defaultDataType;
			bar.TimeScale = defaultTimeXScale;
			bar.SpaceScale = defaultSpaceXScale;
			bar.DatumScale = defaultYScale;
			bar.Mode = defaultXMode;

			while(!pinsToBars.TryAdd(expanDialSticks[0, j], bar));
			while(!barsToPins.TryAdd(bar, expanDialSticks[0, j]));

			bars.Add(bar);
			labels.Add(label);
			xTimes.Add(xTime);
			xSpaces.Add(xSpace);
			yData.Add(yDatum);
		}

		// Fill Chart
		chart.Bars = bars;
		chart.Labels = labels;
		chart.Times =  xTimes;
		chart.Spaces = xSpaces;
		chart.Data = yData;
		chart.DataType = defaultDataType;
		chart.TimeScale = defaultTimeXScale;
		chart.SpaceScale = defaultSpaceXScale;
		chart.DataScale = defaultYScale;
		chart.Mode = defaultXMode;

		renderBarCharts.Enqueue(chart);
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
			/*List<ExpanDialStickModel> clickKeys = new List<ExpanDialStickModel>(clicks.Keys);
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
			}*/
			/*List<ExpanDialStickModel> keys = new List<ExpanDialStickModel>(changeDataTypes.Keys);

			foreach(ExpanDialStickModel expanDialStick in keys){
				if(changeDataTypes[expanDialStick] == START){
					triggerChange = true;
					expanDialStick.TargetColor = Color.black;
					expanDialStick.TargetTextureChangeDuration = 0.25f;
					clicks[expanDialStick] = IN_PROGRESS;
				} 
			}
			changeDataTypes.Clear();*/

			/*while(changeDataTypes.Count > 0){
				ExpanDialStickModel pin;
				while(!changeDataTypes.TryDequeue(out pin));
				PhysBarModel bar = pinsToBars[pin];
				bar.dataType = ((bar.dataType + 1) % 3);
			}*/

			// Change Data Type of One pin

			// Render Bar
			while(renderBars.Count > 0){
				PhysBarModel bar;
				while(!renderBars.TryDequeue(out bar));
				triggerChange = true;
				ExpanDialStickModel pin = barsToPins[bar];
				float coeff = bar.Datum/40f;
				Color targetColor = Color.white;
				switch(bar.DatumType){
					case PRODUCTION_DATA:
					targetColor = new Color(1f - coeff, 1f, 1f - coeff);
					break;
					case STORAGE_DATA:
					targetColor = new Color(1f , 1f-coeff, 1f-coeff);
					break;
					case CONSUMPTION_DATA:
					targetColor = new Color(1f - coeff, 1f-coeff, 1f);
					break;
					default:
					break;
				}
				// Texture
				pin.TargetColor = targetColor;
				pin.TargetText = bar.Label;
				pin.TargetTextureChangeDuration = 0.25f;
			}

			// Render Bar Chart
			while(renderBarCharts.Count > 0){
				PhysBarChartModel chart;
				while(!renderBarCharts.TryDequeue(out chart));
				triggerChange = true;
				for(int i = 0; i < chart.Bars.Count; i++){
					ExpanDialStickModel pin = barsToPins[chart.Bars[i]];
			
					float coeff = chart.Data[i]/40f;
					Color targetColor = Color.white;
					switch(chart.DataType){
						case PRODUCTION_DATA:
						targetColor = new Color(1f - coeff, 1f, 1f - coeff);
						break;
						case STORAGE_DATA:
						targetColor = new Color(1f , 1f-coeff, 1f-coeff);
						break;
						case CONSUMPTION_DATA:
						targetColor = new Color(1f - coeff, 1f-coeff, 1f);
						break;
						default:
						break;
					}
					// Texture
					pin.TargetColor = targetColor;
					pin.TargetText = chart.Labels[i];
					pin.TargetTextureChangeDuration = 0.25f;
					// Shape
					pin.TargetPosition = (sbyte)chart.Data[i];
					pin.TargetHolding = false;
					pin.TargetShapeChangeDuration = 1f;
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
