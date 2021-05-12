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

public class BarEvent
{
	public DateTime time;
	public Vector2 direction;

	public BarEvent(DateTime time, Vector2 direction)
	{
		this.time = time;
		this.direction = direction;
	}
}

public class DataPhysApp : MonoBehaviour
{

	public GameObject expanDialSticksPrefab;
	public GUISkin guiSkin;
	private ExpanDialSticks expanDialSticks;
	private bool connected = false;

	private CultureInfo en = CultureInfo.CreateSpecificCulture("en-US");

	private IEnumerator coroutine;

	public float EVENT_INTERVAL = 0.5f; // 0.2f;
	//private float prevEventTimeCheck = 0f;
	//private float currEventTimeCheck = 0f;

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

	private float lastRising = 0f;
	private const int IDLE = 0x0000;
	private const int RIGHT_RISING = 0x0001;
	private const int RIGHT_FALLING = 0x1110;
	private const int LEFT_RISING = 0x0010;
	private const int LEFT_FALLING = 0x1101;
	private const int TOP_RISING = 0x0100;
	private const int TOP_FALLING = 0x1011;
	private const int BOTTOM_RISING = 0x1000;
	private const int BOTTOM_FALLING = 0x0111;

	private const float JOYSTICK_THRESHOLD = 10f;

	private DataPhysModel dataPhysModel;
	private DataSet dataSet;
	private bool DataPhysIsUpdated = false;
	private Vector2[,] directions;
	private int rotation = 0;
	List<BarEvent> inputs;


	void Start()
	{
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

		directions = new Vector2[expanDialSticks.NbRows, expanDialSticks.NbColumns];
		for (int i = 0; i < directions.GetLength(0); i++)
			for (int j = 0; j < directions.GetLength(1); j++)
				directions[i, j] = Vector2.zero;
		inputs = new List<BarEvent>();
		dataPhysModel = new DataPhysModel(expanDialSticks.NbRows, expanDialSticks.NbColumns);
		// Connection to MQTT Broker
		expanDialSticks.client_MqttConnect();
	}

	private void ViewFromDataSet()
	{
		dataSet = dataPhysModel.DataSet();
		//int widthSize = Math.Min(dataSet.data.GetLength(0), expanDialSticks.NbColumns);
		//Debug.Log(widthSize);
		//int heightSize = Math.Min(dataSet.data.GetLength(1), expanDialSticks.NbRows);
		//Debug.Log(heightSize);
		for (int i = 0; i < expanDialSticks.NbColumns; i++)
		{
			for (int j = 0; j < expanDialSticks.NbRows; j++)
			{
				float datum = (i < dataSet.data.GetLength(0) && j < dataSet.data.GetLength(1)) ? dataSet.data[i, j] : 0f;
				float coeff = Mathf.InverseLerp(dataSet.minValue, dataSet.maxValue, datum);
				sbyte height = Convert.ToSByte((int)Mathf.Lerp(0f, 40f, coeff));
				float textRotation = dataSet.orientation * 90f;

				expanDialSticks[j, i].TargetText = "<b>" + (int)datum + " Wh </b>";//xLabel + "\n<b>" + yLabel;
				expanDialSticks[j, i].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
				expanDialSticks[j, i].TargetTextRotation = textRotation;
				expanDialSticks[j, i].TargetTextureChangeDuration = 1f;

				expanDialSticks[j, i].TargetPosition = height;
				expanDialSticks[j, i].TargetShapeChangeDuration = 1f;
			}
		}
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerShapeChange();

		string xLegendFirst = "<line-height=6><size=4>";
		string xLegendSecond = "<line-height=8><size=6>";
		string xLegendThird = "<line-height=10><size=8>";

		for (int i = 0; i < dataSet.xLabels.Length; i++)
		{
			string xLabel = dataSet.xLabels[i];
			string[] subLabels = xLabel.Split('\n');
			int pos = (int)((i / (float)dataSet.xLabels.Length) * 100f);
			switch (subLabels.Length)
			{
				case 1:
					xLegendThird += "<pos=" + pos + "%>" + subLabels[0];
					break;
				case 2:
					xLegendThird += "<pos=" + pos + "%>" + subLabels[1];
					xLegendSecond += "<pos=" + pos + "%>" + subLabels[0];
					break;
				case 3:
					xLegendThird += "<pos=" + pos + "%>" + subLabels[2];
					xLegendSecond += "<pos=" + pos + "%>" + subLabels[1];
					xLegendFirst += "<pos=" + pos + "%>" + subLabels[0];
					break;
				default:
					xLegendThird += "<pos=" + pos + "%>" + subLabels[0];
				break;
			}
		}
		string xLegend = "";
		if (xLegendFirst != "<line-height=6><size=4>") xLegend += xLegendFirst + "\n";
		if (xLegendSecond != "<line-height=8><size=6>") xLegend += xLegendSecond + "\n";
		if (xLegendThird != "<line-height=10><size=8>") xLegend += xLegendThird + "\n";

		
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 16, Color.black, xLegend, new Vector3(90f, -90f, 0f));

		string yLegend = "";

		for (int i = 0; i < dataSet.yLabels.Length; i++)
		{
			string yLabel = dataSet.yLabels[i];
			string[] subLabels = yLabel.Split('\n');
			switch (subLabels.Length)
			{
				case 1:
				yLegend += "<line-height=10><size=8>" + subLabels[0] + "<line-height=60>\n";
				break;
				case 2:
				yLegend += "<line-height=8><size=6>" + subLabels[0] + "\n";
				yLegend += "<line-height=10><size=8>" + subLabels[1] + "\n";
				yLegend += "<line-height=42>\n";
				break;
				case 3:
				yLegend += "<line-height=6><size=4>" + subLabels[0] + "\n";
				yLegend += "<line-height=8><size=6>" + subLabels[1] + "\n";
				yLegend += "<line-height=10><size=8>" + subLabels[2] + "\n";
				yLegend += "<line-height=36>\n";
				break;
				default:
					yLegend += "<line-height=10>" + subLabels[0] + "<line-height=60>\n";
				break;
			}
		}
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, 16, Color.black, yLegend, new Vector3(90f, -90f, 0f));
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
		directions[e.i, e.j].x = (e.next >= JOYSTICK_THRESHOLD || e.next <= -JOYSTICK_THRESHOLD) ? e.next : 0f;
		if (e.prev < JOYSTICK_THRESHOLD && e.next >= JOYSTICK_THRESHOLD)
		{
			Debug.Log("[" + e.i + ", " + e.j + "] X-AXIS UP RAISING");
			lastRising = Time.time;
		}
		if (e.prev > -JOYSTICK_THRESHOLD && e.next <= -JOYSTICK_THRESHOLD)
		{
			Debug.Log("[" + e.i + ", " + e.j + "] X-AXIS DOWN RAISING");
			lastRising = Time.time;
		}
		if (e.prev >= JOYSTICK_THRESHOLD && e.next < JOYSTICK_THRESHOLD)
		{
			Debug.Log("[" + e.i + ", " + e.j + "] X-AXIS UP FALLING");
		}
		if (e.prev <= -JOYSTICK_THRESHOLD && e.next > -JOYSTICK_THRESHOLD)
		{
			Debug.Log("[" + e.i + ", " + e.j + "] X-AXIS DOWN FALLING");
		}
	}

	private void HandleYAxisChanged(object sender, ExpanDialStickEventArgs e)
	{
		directions[e.i, e.j].y = (e.next >= JOYSTICK_THRESHOLD || e.next <= -JOYSTICK_THRESHOLD)? e.next : 0f;
		if (e.prev < JOYSTICK_THRESHOLD && e.next >= JOYSTICK_THRESHOLD)
		{
			Debug.Log("[" + e.i + ", " + e.j + "] Y-AXIS RIGHT RAISING");
			lastRising = Time.time;
		}
		if (e.prev > -JOYSTICK_THRESHOLD && e.next <= -JOYSTICK_THRESHOLD)
		{
			Debug.Log("[" + e.i + ", " + e.j + "] Y-AXIS LEFT RAISING");
			lastRising = Time.time;
		}
		if (e.prev >= JOYSTICK_THRESHOLD && e.next < JOYSTICK_THRESHOLD)
		{
			Debug.Log("[" + e.i + ", " + e.j + "] Y-AXIS RIGHT FALLING");
		}
		if (e.prev <= -JOYSTICK_THRESHOLD && e.next > -JOYSTICK_THRESHOLD)
		{
			Debug.Log("[" + e.i + ", " + e.j + "] Y-AXIS LEFT FALLING");
		}
	}

	private void HandleClickChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandleClickChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
		/*users.Enqueue(e.i + '|' + e.j + '|' + CLICK);*/
	}

	private void HandleRotationChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandleRotationChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
		if (e.diff > 0)
		{

			Debug.Log("[" + e.i + ", " + e.j + "] Z-AXIS CLOCKWISE ROTATION");
			rotation = 1;
			lastRising = Time.time;
		}
		else
		{
			Debug.Log("[" + e.i + ", " + e.j + "] Z-AXIS COUNTERCLOCKWISE ROTATION");
			rotation = -1; 
			lastRising = Time.time;
		}

	}

	private void HandlePositionChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandlePositionChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
		/*if (e.diff > 0) users.Enqueue(e.i + '|' + e.j + '|' + PULL);
		else users.Enqueue(e.i + '|' + e.j + '|' + PUSH);*/

	}

	private void HandleReachingChanged(object sender, ExpanDialStickEventArgs e)
	{

	}

	private void HandleHoldingChanged(object sender, ExpanDialStickEventArgs e)
	{

	}

	

	void PanUp(int yStep)
	{
		Debug.Log("PAN UP " + yStep + "TIMES");
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_UP, yStep);
				break;
			case DataPhysModel.Z_AXIS_90:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, yStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
			case DataPhysModel.Z_AXIS_180:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_DOWN, yStep);
				break;
			case DataPhysModel.Z_AXIS_270:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, yStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
		}
		DataPhysIsUpdated = false;
	}
	void PanDown(int yStep)
	{
		Debug.Log("PAN DOWN" + yStep + "TIMES");
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_DOWN, yStep);
				break;
			case DataPhysModel.Z_AXIS_90:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, yStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
			case DataPhysModel.Z_AXIS_180:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_UP, yStep);
				break;
			case DataPhysModel.Z_AXIS_270:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, yStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
		}
		DataPhysIsUpdated = false;
	}
	void PanLeft(int xStep)
	{
		Debug.Log("PAN LEFT" + xStep + "TIMES");
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, xStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
			case DataPhysModel.Z_AXIS_90:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_DOWN, xStep);
				break;
			case DataPhysModel.Z_AXIS_180:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, xStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
			case DataPhysModel.Z_AXIS_270:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_UP, xStep);
				break;
		}
		DataPhysIsUpdated = false;
	}
	void PanRight(int xStep)
	{
		Debug.Log("PAN RIGHT" + xStep + "TIMES");
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, xStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
			case DataPhysModel.Z_AXIS_90:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_UP, xStep);
				break;
			case DataPhysModel.Z_AXIS_180:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, xStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
			case DataPhysModel.Z_AXIS_270:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_DOWN, xStep);
				break;
		}
		DataPhysIsUpdated = false;
	}

	void PanUpLeft(int xStep, int yStep)
	{
		Debug.Log("PAN UP LEFT" + xStep + ", " + yStep + " TIMES");
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, xStep, DataPhysModel.Y_AXIS_UP, yStep);
				break;
			case DataPhysModel.Z_AXIS_90:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, yStep, DataPhysModel.Y_AXIS_DOWN, xStep);
				break;
			case DataPhysModel.Z_AXIS_180:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, xStep, DataPhysModel.Y_AXIS_DOWN, yStep);
				break;
			case DataPhysModel.Z_AXIS_270:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, yStep, DataPhysModel.Y_AXIS_UP, yStep);
				break;
		}
		DataPhysIsUpdated = false;
	}

	void PanUpRight(int xStep, int yStep)
	{
		Debug.Log("PAN UP RIGHT" + xStep + ", " + yStep + " TIMES");
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, xStep, DataPhysModel.Y_AXIS_UP, yStep);
				break;
			case DataPhysModel.Z_AXIS_90:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, yStep, DataPhysModel.Y_AXIS_UP, xStep);
				break;
			case DataPhysModel.Z_AXIS_180:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, xStep,  DataPhysModel.Y_AXIS_DOWN, yStep);
				break;
			case DataPhysModel.Z_AXIS_270:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, yStep, DataPhysModel.Y_AXIS_DOWN, xStep);
				break;
		}
		DataPhysIsUpdated = false;
	}
	void PanDownLeft(int xStep, int yStep)
	{
		Debug.Log("PAN DOWN LEFT" + xStep + ", " + yStep + " TIMES");
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, xStep, DataPhysModel.Y_AXIS_DOWN, yStep);
				break;
			case DataPhysModel.Z_AXIS_90:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, yStep, DataPhysModel.Y_AXIS_DOWN, xStep);
				break;
			case DataPhysModel.Z_AXIS_180:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, xStep, DataPhysModel.Y_AXIS_UP, yStep);
				break;
			case DataPhysModel.Z_AXIS_270:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, yStep, DataPhysModel.Y_AXIS_UP, xStep);
				break;
		}
		DataPhysIsUpdated = false;
	}

	void PanDownRight(int xStep, int yStep)
	{
		Debug.Log("PAN UP RIGHT" + xStep + ", " + yStep + " TIMES");
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, xStep, DataPhysModel.Y_AXIS_DOWN, yStep);
				break;
			case DataPhysModel.Z_AXIS_90:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, yStep, DataPhysModel.Y_AXIS_UP, xStep);
				break;
			case DataPhysModel.Z_AXIS_180:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, xStep, DataPhysModel.Y_AXIS_UP, yStep);
				break;
			case DataPhysModel.Z_AXIS_270:
				dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, yStep, DataPhysModel.Y_AXIS_DOWN, xStep);
				break;
		}
		DataPhysIsUpdated = false;
	}
	void ZoomVerticalAxisIn(float xCenter, float yCenter)
	{
		Debug.Log("ZOOM VERTICAL AXIS IN");
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, xCenter, DataPhysModel.Y_AXIS_IN, yCenter);
				break;
			case DataPhysModel.Z_AXIS_90:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, yCenter, DataPhysModel.Y_AXIS_IDLE, xCenter);
				break;
			case DataPhysModel.Z_AXIS_180:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, xCenter, DataPhysModel.Y_AXIS_IN, yCenter);
				break;
			case DataPhysModel.Z_AXIS_270:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, yCenter, DataPhysModel.Y_AXIS_IDLE, xCenter);
				break;
		}
		DataPhysIsUpdated = false;
	}
	void ZoomVerticalAxisOut(float xCenter, float yCenter)
	{
		Debug.Log("ZOOM VERTICAL AXIS OUT");
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, xCenter, DataPhysModel.Y_AXIS_OUT, yCenter);
				break;
			case DataPhysModel.Z_AXIS_90:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, yCenter, DataPhysModel.Y_AXIS_IDLE, xCenter);
				break;
			case DataPhysModel.Z_AXIS_180:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, xCenter, DataPhysModel.Y_AXIS_OUT, yCenter);
				break;
			case DataPhysModel.Z_AXIS_270:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, yCenter, DataPhysModel.Y_AXIS_IDLE, xCenter);
				break;
		}
		DataPhysIsUpdated = false;
	}

	void ZoomHorizontalAxisIn(float xCenter, float yCenter)
	{
		Debug.Log("ZOOM HORIZONTAL AXIS IN");
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, xCenter, DataPhysModel.Y_AXIS_IDLE, yCenter);
				break;
			case DataPhysModel.Z_AXIS_90:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, yCenter, DataPhysModel.Y_AXIS_IN, xCenter);
				break;
			case DataPhysModel.Z_AXIS_180:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, xCenter, DataPhysModel.Y_AXIS_IDLE, yCenter);
				break;
			case DataPhysModel.Z_AXIS_270:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, yCenter, DataPhysModel.Y_AXIS_IN, xCenter);
				break;
		}
		DataPhysIsUpdated = false;
	}
	void ZoomHorizontalAxisOut(float xCenter, float yCenter)
	{
		Debug.Log("ZOOM HORIZONTAL AXIS OUT");
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, xCenter, DataPhysModel.Y_AXIS_IDLE, yCenter);
				break;
			case DataPhysModel.Z_AXIS_90:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, yCenter, DataPhysModel.Y_AXIS_OUT, xCenter);
				break;
			case DataPhysModel.Z_AXIS_180:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, xCenter, DataPhysModel.Y_AXIS_IDLE, yCenter);
				break;
			case DataPhysModel.Z_AXIS_270:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, yCenter, DataPhysModel.Y_AXIS_OUT, xCenter);
				break;
		}
		DataPhysIsUpdated = false;
	}
	void ZoomBothIn(float xCenter, float yCenter)
	{
		Debug.Log("ZOOM IN");
		Debug.Log("xCenter: " + xCenter);
		Debug.Log("yCenter: " + yCenter);
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, xCenter, DataPhysModel.Y_AXIS_IN, yCenter);
				break;
			case DataPhysModel.Z_AXIS_90:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, yCenter, DataPhysModel.Y_AXIS_IN, expanDialSticks.NbRows - xCenter);
				break;
			case DataPhysModel.Z_AXIS_180:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, expanDialSticks.NbColumns - xCenter, DataPhysModel.Y_AXIS_IN, expanDialSticks.NbRows - yCenter);
				break;
			case DataPhysModel.Z_AXIS_270:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, expanDialSticks.NbRows -  yCenter, DataPhysModel.Y_AXIS_IN, xCenter);
				break;
		}
		DataPhysIsUpdated = false;
	}
	void ZoomBothOut(float xCenter, float yCenter)
	{
		Debug.Log("ZOOM OUT");
		Debug.Log("xCenter: " + xCenter);
		Debug.Log("yCenter: " + yCenter);
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, xCenter, DataPhysModel.Y_AXIS_OUT, yCenter);
				break;
			case DataPhysModel.Z_AXIS_90:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, yCenter, DataPhysModel.Y_AXIS_OUT, expanDialSticks.NbRows - xCenter);
				break;
			case DataPhysModel.Z_AXIS_180:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, expanDialSticks.NbColumns - xCenter, DataPhysModel.Y_AXIS_OUT, expanDialSticks.NbRows - yCenter);
				break;
			case DataPhysModel.Z_AXIS_270:
				dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, expanDialSticks.NbRows - yCenter, DataPhysModel.Y_AXIS_OUT,  xCenter);
				break;
		}
		DataPhysIsUpdated = false;
	}
	void RotateClockWise()
	{
		Debug.Log("ROTATE CW");
		dataPhysModel.Rotate(DataPhysModel.Z_AXIS_CW);
		DataPhysIsUpdated = false;
	}
	void RotateCounterClockWise()
	{
		Debug.Log("ROTATE CCW");
		dataPhysModel.Rotate(DataPhysModel.Z_AXIS_CCW);
		DataPhysIsUpdated = false;
	}

void Update()
	{
		
		float ct = Time.time;
		if(ct  - lastRising > expanDialSticks.MQTT_INTERVAL + 0.1f) // TREAT EVENTS	
		{
			if(rotation > 0)
			{
				RotateClockWise();
				rotation = 0;
			}
			if (rotation < 0)
			{
				RotateCounterClockWise();
				rotation = 0;
			}

			// CHECK FOR HORIZONTAL ZOOM OR PAN
			for (int i = 0; i < directions.GetLength(0); i++)
			{
				int firstLeft = -1;
				int firstRight = -1;
				int lastLeft = -1;
				int lastRight = -1;
				for (int j = 0; j < directions.GetLength(1); j++)
				{
					Vector2 currDirection = directions[i, j];
					if (currDirection.y >= JOYSTICK_THRESHOLD && currDirection.x == 0f)
					{
						firstRight = (firstRight == -1) ? j : firstRight;
						lastRight = j;
					}
					if (currDirection.y <= -JOYSTICK_THRESHOLD && currDirection.x == 0f)
					{
						firstLeft = (firstLeft == -1) ? j : firstLeft;
						lastLeft = j;
					}
				}
				if(firstRight > -1 && firstLeft > -1)
				{
					if (lastLeft < firstRight) // ZOOM OUT HORIZONTAL
					{
						float xCenter = i;
						float yCenter = lastLeft + (firstRight - lastLeft) / 2f;
						ZoomHorizontalAxisOut(yCenter, xCenter);
						goto LoopEnd;
					}
					if (lastRight < firstLeft) // ZOOM IN HORIZONTAL
					{
						float xCenter = i;
						float yCenter = lastRight + (firstLeft - lastRight) / 2f;
						ZoomHorizontalAxisIn(yCenter, xCenter);
						goto LoopEnd;
					}
				}
				if(firstRight > -1 && lastRight > -1) // MULTI PAN RIGHT
				{
					PanRight(Math.Abs(lastRight - firstRight) + 1);
					goto LoopEnd;
				}
				if (firstLeft > -1 && lastLeft > -1) // MULTI PAN LEFT
				{
					PanLeft(Math.Abs(lastLeft - firstLeft) + 1);
					goto LoopEnd;
				}
			}
			// CHECK FOR VERTICAL ZOOM
			for (int j = 0; j < directions.GetLength(1); j++)
			{
				int firstDown = -1;
				int firstUp = -1;
				int lastDown = -1;
				int lastUp = -1;
				for (int i = 0; i < directions.GetLength(0); i++)
				{
					Vector2 currDirection = directions[i, j];
					if (currDirection.x >= JOYSTICK_THRESHOLD && currDirection.y == 0f)
					{
						firstUp = (firstUp == -1) ? i : firstUp;
						lastUp = i;
					}
					if (currDirection.x <= -JOYSTICK_THRESHOLD && currDirection.y == 0f)
					{
						firstDown = (firstDown == -1) ? i : firstDown;
						lastDown = i;
					}
				}
				if(firstUp > -1 && firstDown > -1)
				{
					if (lastUp < firstDown) // ZOOM OUT HORIZONTAL
					{
						float xCenter = lastUp + (firstDown - lastUp) / 2f;
						float yCenter = j;
						ZoomVerticalAxisOut(yCenter, xCenter);
						goto LoopEnd;
					}
					if (lastDown < firstUp) // ZOOM IN HORIZONTAL
					{
						float xCenter = lastDown + (firstUp - lastDown) / 2f;
						float yCenter = j;
						ZoomVerticalAxisIn(yCenter, xCenter);
						goto LoopEnd;
					}
				}

				if (firstUp > -1 && lastUp > -1) // MULTI PAN UP
				{
					PanUp(Math.Abs(lastUp - firstUp) + 1);
					goto LoopEnd;
				}
				if (firstDown > -1 && lastDown > -1) // MULTI PAN DOWN
				{
					PanLeft(Math.Abs(lastDown - firstDown) + 1);
					goto LoopEnd;
				}
			}

			// CHECK FOR DIAGONAL ZOOM
			for (int k = 0; k <= directions.GetLength(0) + directions.GetLength(1) - 2; k++)
			{

				int firstUp = -1;
				int firstDown = -1;
				int firstLeft = -1;
				int firstRight = -1;
				int lastUp = -1;
				int lastDown = -1;
				int lastLeft = -1;
				int lastRight = -1;

				// CHECK FOR BOTTOM_LEFT to TOP_RIGHT DIAGONAL
				for (int j = 0; j <= k; j++)
				{
					int i = k - j;

					if (i < directions.GetLength(0) && j < directions.GetLength(1))
					{

						Vector2 currDirection = directions[i, j];
						if (currDirection.x >= JOYSTICK_THRESHOLD && currDirection.y >= JOYSTICK_THRESHOLD)
						{
							firstUp = (firstUp == -1) ? i : firstUp;
							lastUp = i;
							firstRight = (firstRight == -1) ? j : firstRight;
							lastRight = j;
						}
						if (currDirection.x <= -JOYSTICK_THRESHOLD && currDirection.y <= -JOYSTICK_THRESHOLD)
						{
							firstDown = (firstDown == -1) ? i : firstDown;
							lastDown = i;
							firstLeft = (firstLeft == -1) ? j : firstLeft;
							lastLeft = j;
						}
					}
				}

				if (firstUp > -1 && firstDown > -1 && firstLeft > -1 && firstRight > -1)
				{

					if (lastDown < firstUp && lastRight < firstLeft) // ZOOM OUT 
					{
						float xCenter = lastDown + (firstUp - lastDown) / 2f;
						float yCenter = lastRight + (firstLeft - lastRight) / 2f;
						ZoomBothOut(yCenter, xCenter);
						goto LoopEnd;
					}
					if (lastUp < firstDown && lastLeft < firstRight) // ZOOM IN 
					{
						float xCenter = lastUp + (firstDown - lastUp) / 2f;
						float yCenter = lastLeft + (firstRight - lastLeft) / 2f;
						ZoomBothIn(yCenter, xCenter);
						goto LoopEnd;
					}
				}

				if (firstUp > -1 && lastUp > -1 && firstRight > -1 && lastRight > -1) // MULTI PAN UP RIGHT
				{
					PanUpRight(Math.Abs(lastRight - firstRight) + 1, Math.Abs(lastUp - firstUp) + 1);
					goto LoopEnd;
				}
				if (firstUp > -1 && lastUp > -1 && firstLeft > -1 && lastLeft > -1) // MULTI PAN UP LEFT
				{
					PanUpLeft(Math.Abs(lastLeft - firstLeft) + 1, Math.Abs(lastUp - firstUp) + 1);
					goto LoopEnd;
				}

				if (firstDown > -1 && lastDown > -1 && firstRight > -1 && lastRight > -1) // MULTI PAN DOWN RIGHT
				{
					PanDownRight(Math.Abs(lastRight - firstRight) + 1, Math.Abs(lastDown - firstDown) + 1);
					goto LoopEnd;
				}
				if (firstDown > -1 && lastUp > -1 && firstLeft > -1 && lastLeft > -1) // MULTI PAN DOWN LEFT
				{
					PanDownLeft(Math.Abs(lastLeft - firstLeft) + 1, Math.Abs(lastDown - firstDown) + 1);
					goto LoopEnd;
				}

				firstUp = -1;
				firstDown = -1;
				firstLeft = -1;
				firstRight = -1;
				lastUp = -1;
				lastDown = -1;
				lastLeft = -1;
				lastRight = -1;

				// CHECK FOR TOP_LEFT to BOTTOM_RIGHT DIAGONAL
				for (int j = 0; j <= k; j++)
				{
					int i = k - j;

					if (i < directions.GetLength(0) && j < directions.GetLength(1))
					{

						i = (directions.GetLength(0) - 1) - i;
						Vector2 currDirection = directions[i, j];
						if (currDirection.x >= JOYSTICK_THRESHOLD && currDirection.y <=- JOYSTICK_THRESHOLD)
						{
							firstUp = (firstUp == -1) ? i : firstUp;
							lastUp = i;
							firstLeft = (firstLeft == -1) ? j : firstLeft;
							lastLeft = j;
						}
						if (currDirection.x <= -JOYSTICK_THRESHOLD && currDirection.y >= JOYSTICK_THRESHOLD)
						{
							firstDown = (firstDown == -1) ? i : firstDown;
							lastDown = i;
							firstRight = (firstRight == -1) ? j : firstRight;
							lastRight = j;
						}
					}
				}

				if (firstUp > -1 && firstDown > -1 && firstLeft > -1 && firstRight > -1)
				{
					if (lastUp < firstDown && lastLeft < firstRight) // ZOOM OUT 
					{
						float xCenter = lastUp + (firstDown - lastUp) / 2f;
						float yCenter = lastLeft + (firstRight - lastLeft) / 2f;
						ZoomBothOut(yCenter, xCenter);
						goto LoopEnd;
					}
					if (lastDown < firstUp && lastRight < firstLeft) // ZOOM IN 
					{
						float xCenter = lastDown + (firstUp - lastDown) / 2f;
						float yCenter = lastRight + (firstLeft - lastRight) / 2f;
						ZoomBothIn(yCenter, xCenter);
						goto LoopEnd;
					}
				}

				if (firstUp > -1 && lastUp > -1 && firstRight > -1 && lastRight > -1) // MULTI PAN UP RIGHT
				{
					PanUpRight(Math.Abs(lastRight - firstRight) + 1, Math.Abs(lastUp - firstUp) + 1);
					goto LoopEnd;
				}
				if (firstUp > -1 && lastUp > -1 && firstLeft > -1 && lastLeft > -1) // MULTI PAN UP LEFT
				{
					PanUpLeft(Math.Abs(lastLeft - firstLeft) + 1, Math.Abs(lastUp - firstUp) + 1);
					goto LoopEnd;
				}

				if (firstDown > -1 && lastDown > -1 && firstRight > -1 && lastRight > -1) // MULTI PAN DOWN RIGHT
				{
					PanDownRight(Math.Abs(lastRight - firstRight) + 1, Math.Abs(lastDown - firstDown) + 1);
					goto LoopEnd;
				}
				if (firstDown > -1 && lastUp > -1 && firstLeft > -1 && lastLeft > -1) // MULTI PAN DOWN LEFT
				{
					PanDownLeft(Math.Abs(lastLeft - firstLeft) + 1, Math.Abs(lastDown - firstDown) + 1);
					goto LoopEnd;
				}
			}
			// CHECK FOR SIMPLE XY PANNING
			for (int i = 0; i < directions.GetLength(0); i++)
			{
				for (int j = 0; j < directions.GetLength(1); j++)
				{
					Vector2 direction = directions[i, j];
					if (direction != Vector2.zero)
					{
						if (direction.x >= JOYSTICK_THRESHOLD && direction.y >= JOYSTICK_THRESHOLD)
						{
							PanUpRight(1,1);
							goto LoopEnd;
						}
						else if (direction.x >= JOYSTICK_THRESHOLD && direction.y <= -JOYSTICK_THRESHOLD)
						{
							PanUpLeft(1, 1);
							goto LoopEnd;
						}
						else if (direction.x <= -JOYSTICK_THRESHOLD && direction.y >= JOYSTICK_THRESHOLD)
						{

							PanDownRight(1, 1);
							goto LoopEnd;
						}
						else if (direction.x <= -JOYSTICK_THRESHOLD && direction.y <= -JOYSTICK_THRESHOLD)
						{

							PanDownLeft(1, 1);
							goto LoopEnd;
						}
						else if (direction.x >= JOYSTICK_THRESHOLD)
						{
							PanUp(1);
							goto LoopEnd;
						}
						else if (direction.x <= -JOYSTICK_THRESHOLD)
						{
							PanDown(1);
							goto LoopEnd;
						}
						else if (direction.y >= JOYSTICK_THRESHOLD)
						{
							PanRight(1);
							goto LoopEnd;
						}
						else if (direction.y <= -JOYSTICK_THRESHOLD)
						{
							PanLeft(1);
							goto LoopEnd;
						}

					}
				}
			}

			// CHECK FOR ROTATION

			// FOUND XY AXIS ZOOM
			// FOUND X AXIS ZOOM
			// FOUND Y AXIS ZOOM
			// FOUND XY AXIS PAN 
			// FOUND X AXIS PAN
			// FOUND Y AXIS PAN
			LoopEnd:
				lastRising = ct + 1f;
		}

		// check if ExpanDialSticks is connected
		if (connected)
		{
			if (Input.GetKey("escape"))
			{
				Application.Quit();
			}
			if (!DataPhysIsUpdated)
			{
				ViewFromDataSet();
				DataPhysIsUpdated = true;
			}

			// ZOOM IN TOP LEFT TO BOTTOM RIGHT
			if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{

				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, 0, -10, -10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, 0, 10, 10);
				HandleYAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, 0, 10, 10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, 0, -10, -10);
				HandleYAxisChanged(new object(), e);

			}
			if (Input.GetKeyUp(KeyCode.KeypadPlus))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, -10, 0, 10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, 10, 0, -10);
				HandleYAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, 10, 0, -10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, -10, 0, 10);
				HandleYAxisChanged(new object(), e);
			}

			// ZOOM IN BOTTOM LEFT TO TOP RIGHT
			/*if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 0, 4, 0, 10, 10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 0, 4, 0, 10, 10);
				HandleYAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 0, 0, -10, -10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 0, 0, -10, -10);
				HandleYAxisChanged(new object(), e);
			}

			if (Input.GetKeyUp(KeyCode.KeypadPlus))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 0, 4, 10, 0, -10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 0, 4, 10, 0, -10);
				HandleYAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 0, -10, 0, 10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 0, -10, 0, 10);
				HandleYAxisChanged(new object(), e);
			}*/

			// ZOOM OUT TOP LEFT TO BOTTOM RIGHT
			if (Input.GetKeyDown(KeyCode.KeypadMinus))
			{

				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, 0, 10, 10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, 0, -10, -10);
				HandleYAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, 0, -10, -10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, 0, 10, 10);
				HandleYAxisChanged(new object(), e);

			}
			if (Input.GetKeyUp(KeyCode.KeypadMinus))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, 10, 0, -10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, -10, 0, 10);
				HandleYAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, -10, 0, 10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, 10, 0, -10);
				HandleYAxisChanged(new object(), e);
			}

			// ZOOM OUT BOTTOM LEFT TO TOP RIGHT
			/*if (Input.GetKeyDown(KeyCode.KeypadMinus))
			{

				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 0, 4, 0, -10, -10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 0, 4, 0, -10, -10);
				HandleYAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 0, 0, 10, 10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 0, 0, 10, 10);
				HandleYAxisChanged(new object(), e);

			}
			if (Input.GetKeyUp(KeyCode.KeypadMinus))
			{

				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 0, 4, -10, 0, 10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 0, 4, -10, 0, 10);
				HandleYAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 0, 10, 0, -10);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 0, 10, 0, -10);
				HandleYAxisChanged(new object(), e);
			}*/


			// UP
			if (Input.GetKeyDown(KeyCode.Z))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 0, 2, 0, 10, 10);
				HandleXAxisChanged(new object(), e);
			}
			if (Input.GetKeyUp(KeyCode.Z))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 0, 2, 10, 0, -10);
				HandleXAxisChanged(new object(), e);
			}

			// DOWN
			if (Input.GetKeyDown(KeyCode.S))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 4, 2, 0, -10, -10);
				HandleXAxisChanged(new object(), e);
			}
			if (Input.GetKeyUp(KeyCode.S))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 4, 2, -10, 0, 10);
				HandleXAxisChanged(new object(), e);
			}

			// LEFT
			if (Input.GetKeyDown(KeyCode.Q))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 2, 0, 0, -10, -10);
				HandleYAxisChanged(new object(), e);
				/*e = new ExpanDialStickEventArgs(DateTime.Now, 2, 5, 0, -10, -10);
				HandleYAxisChanged(new object(), e);*/
			}
			if (Input.GetKeyUp(KeyCode.Q))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 2, 0, -10, 0, 10);
				HandleYAxisChanged(new object(), e);
				/*e = new ExpanDialStickEventArgs(DateTime.Now, 2, 5, -10, 0, 10);
				HandleYAxisChanged(new object(), e);*/
			}

			// RIGHT
			if (Input.GetKeyDown(KeyCode.D))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 2, 5, 0, 10, 10);
				HandleYAxisChanged(new object(), e);
				/*e = new ExpanDialStickEventArgs(DateTime.Now, 2, 0, 0, 10, 10);
				HandleYAxisChanged(new object(), e);*/
			}
			if (Input.GetKeyUp(KeyCode.D))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 2, 5, 10, 0, -10);
				HandleYAxisChanged(new object(), e);
				/*e = new ExpanDialStickEventArgs(DateTime.Now, 2, 0, 10, 0, -10);
				HandleYAxisChanged(new object(), e);*/
			}

			if (Input.GetKeyDown(KeyCode.A))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 2, 5, 0, -10, -10);
				HandleRotationChanged(new object(), e);
			}
			if (Input.GetKeyDown(KeyCode.E))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 2, 5, 0, 10, 10);
				HandleRotationChanged(new object(), e);
			}
		}
	}
}
