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

public class DataPhysApp : MonoBehaviour
{

	public GameObject expanDialSticksPrefab;
	public GUISkin guiSkin;
	private ExpanDialSticks expanDialSticks;
	private bool connected = false;
	public float EVENT_INTERVAL = 0.5f; // 0.2f;
	private float lastRising = 0f;

	private const float JOYSTICK_THRESHOLD = 10f;

	private DataPhysModel dataPhysModel;
	private DataSet dataSet;
	private bool DataPhysIsUpdated = false;
	private Vector2[,] directions;
	private int rotation = 0;
	private Queue<Vector3> errors;


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
		dataPhysModel = new DataPhysModel(expanDialSticks.NbRows, expanDialSticks.NbColumns);
		errors = new Queue<Vector3>();
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
	}

	private void HandleReachingChanged(object sender, ExpanDialStickEventArgs e)
	{

	}

	private void HandleHoldingChanged(object sender, ExpanDialStickEventArgs e)
	{

	}



	int PanUp(int yStep)
	{
		Debug.Log("PAN UP " + yStep + "TIMES");
		int ans = DataPhysModel.PAN_ERROR_NOT_FOUND;
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_UP, yStep);
				break;
			case DataPhysModel.Z_AXIS_90:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, yStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
			case DataPhysModel.Z_AXIS_180:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_DOWN, yStep);
				break;
			case DataPhysModel.Z_AXIS_270:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, yStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
		}
		DataPhysIsUpdated = false;
		return ans;
	}
	int PanDown(int yStep)
	{
		Debug.Log("PAN DOWN" + yStep + "TIMES");
		int ans = DataPhysModel.PAN_ERROR_NOT_FOUND;
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_DOWN, yStep);
				break;
			case DataPhysModel.Z_AXIS_90:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, yStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
			case DataPhysModel.Z_AXIS_180:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_UP, yStep);
				break;
			case DataPhysModel.Z_AXIS_270:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, yStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
		}
		DataPhysIsUpdated = false;
		return ans;
	}
	int PanLeft(int xStep)
	{
		Debug.Log("PAN LEFT" + xStep + "TIMES");
		int ans = DataPhysModel.PAN_ERROR_NOT_FOUND;
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, xStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
			case DataPhysModel.Z_AXIS_90:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_DOWN, xStep);
				break;
			case DataPhysModel.Z_AXIS_180:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, xStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
			case DataPhysModel.Z_AXIS_270:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_UP, xStep);
				break;
		}
		DataPhysIsUpdated = false;
		return ans;
	}
	int PanRight(int xStep)
	{
		Debug.Log("PAN RIGHT" + xStep + "TIMES");
		int ans = DataPhysModel.PAN_ERROR_NOT_FOUND;
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, xStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
			case DataPhysModel.Z_AXIS_90:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_UP, xStep);
				break;
			case DataPhysModel.Z_AXIS_180:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, xStep, DataPhysModel.Y_AXIS_IDLE, 0);
				break;
			case DataPhysModel.Z_AXIS_270:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_IDLE, 0, DataPhysModel.Y_AXIS_DOWN, xStep);
				break;
		}
		DataPhysIsUpdated = false;
		return ans;
	}

	int PanUpLeft(int xStep, int yStep)
	{
		Debug.Log("PAN UP LEFT" + xStep + ", " + yStep + " TIMES");
		int ans = DataPhysModel.PAN_ERROR_NOT_FOUND;
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, xStep, DataPhysModel.Y_AXIS_UP, yStep);
				break;
			case DataPhysModel.Z_AXIS_90:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, yStep, DataPhysModel.Y_AXIS_DOWN, xStep);
				break;
			case DataPhysModel.Z_AXIS_180:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, xStep, DataPhysModel.Y_AXIS_DOWN, yStep);
				break;
			case DataPhysModel.Z_AXIS_270:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, yStep, DataPhysModel.Y_AXIS_UP, yStep);
				break;
		}
		DataPhysIsUpdated = false;
		return ans;
	}

	int PanUpRight(int xStep, int yStep)
	{
		Debug.Log("PAN UP RIGHT" + xStep + ", " + yStep + " TIMES");
		int ans = DataPhysModel.PAN_ERROR_NOT_FOUND;
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, xStep, DataPhysModel.Y_AXIS_UP, yStep);
				break;
			case DataPhysModel.Z_AXIS_90:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, yStep, DataPhysModel.Y_AXIS_UP, xStep);
				break;
			case DataPhysModel.Z_AXIS_180:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, xStep,  DataPhysModel.Y_AXIS_DOWN, yStep);
				break;
			case DataPhysModel.Z_AXIS_270:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, yStep, DataPhysModel.Y_AXIS_DOWN, xStep);
				break;
		}
		DataPhysIsUpdated = false;
		return ans;
	}
	int PanDownLeft(int xStep, int yStep)
	{
		Debug.Log("PAN DOWN LEFT" + xStep + ", " + yStep + " TIMES");
		int ans = DataPhysModel.PAN_ERROR_NOT_FOUND;
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, xStep, DataPhysModel.Y_AXIS_DOWN, yStep);
				break;
			case DataPhysModel.Z_AXIS_90:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, yStep, DataPhysModel.Y_AXIS_DOWN, xStep);
				break;
			case DataPhysModel.Z_AXIS_180:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, xStep, DataPhysModel.Y_AXIS_UP, yStep);
				break;
			case DataPhysModel.Z_AXIS_270:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, yStep, DataPhysModel.Y_AXIS_UP, xStep);
				break;
		}
		DataPhysIsUpdated = false;
		return ans;
	}

	int PanDownRight(int xStep, int yStep)
	{
		Debug.Log("PAN UP RIGHT" + xStep + ", " + yStep + " TIMES");
		int ans = DataPhysModel.PAN_ERROR_NOT_FOUND;
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, xStep, DataPhysModel.Y_AXIS_DOWN, yStep);
				break;
			case DataPhysModel.Z_AXIS_90:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_RIGHT, yStep, DataPhysModel.Y_AXIS_UP, xStep);
				break;
			case DataPhysModel.Z_AXIS_180:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, xStep, DataPhysModel.Y_AXIS_UP, yStep);
				break;
			case DataPhysModel.Z_AXIS_270:
				ans = dataPhysModel.Pan(DataPhysModel.X_AXIS_LEFT, yStep, DataPhysModel.Y_AXIS_DOWN, xStep);
				break;
		}
		DataPhysIsUpdated = false;
		return ans;
	}
	int ZoomVerticalAxisIn(float xCenter, float yCenter)
	{
		Debug.Log("ZOOM VERTICAL AXIS IN");
		int ans = DataPhysModel.ZOOM_ERROR_NOT_FOUND;
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, xCenter, DataPhysModel.Y_AXIS_IN, yCenter);
				break;
			case DataPhysModel.Z_AXIS_90:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, yCenter, DataPhysModel.Y_AXIS_IDLE, xCenter);
				break;
			case DataPhysModel.Z_AXIS_180:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, xCenter, DataPhysModel.Y_AXIS_IN, yCenter);
				break;
			case DataPhysModel.Z_AXIS_270:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, yCenter, DataPhysModel.Y_AXIS_IDLE, xCenter);
				break;
		}
		DataPhysIsUpdated = false;
		return ans;
	}
	int ZoomVerticalAxisOut(float xCenter, float yCenter)
	{
		Debug.Log("ZOOM VERTICAL AXIS OUT");
		int ans = DataPhysModel.ZOOM_ERROR_NOT_FOUND;
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, xCenter, DataPhysModel.Y_AXIS_OUT, yCenter);
				break;
			case DataPhysModel.Z_AXIS_90:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, yCenter, DataPhysModel.Y_AXIS_IDLE, xCenter);
				break;
			case DataPhysModel.Z_AXIS_180:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, xCenter, DataPhysModel.Y_AXIS_OUT, yCenter);
				break;
			case DataPhysModel.Z_AXIS_270:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, yCenter, DataPhysModel.Y_AXIS_IDLE, xCenter);
				break;
		}
		DataPhysIsUpdated = false;
		return ans;
	}

	int ZoomHorizontalAxisIn(float xCenter, float yCenter)
	{
		Debug.Log("ZOOM HORIZONTAL AXIS IN");
		int ans = DataPhysModel.ZOOM_ERROR_NOT_FOUND;
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, xCenter, DataPhysModel.Y_AXIS_IDLE, yCenter);
				break;
			case DataPhysModel.Z_AXIS_90:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, yCenter, DataPhysModel.Y_AXIS_IN, xCenter);
				break;
			case DataPhysModel.Z_AXIS_180:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, xCenter, DataPhysModel.Y_AXIS_IDLE, yCenter);
				break;
			case DataPhysModel.Z_AXIS_270:
				ans = DataPhysModel.ZOOM_ERROR_NOT_FOUND;dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, yCenter, DataPhysModel.Y_AXIS_IN, xCenter);
			break;
		}
		DataPhysIsUpdated = false;
		return ans;
	}
	int ZoomHorizontalAxisOut(float xCenter, float yCenter)
	{
		Debug.Log("ZOOM HORIZONTAL AXIS OUT");
		int ans = DataPhysModel.ZOOM_ERROR_NOT_FOUND;
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, xCenter, DataPhysModel.Y_AXIS_IDLE, yCenter);
				break;
			case DataPhysModel.Z_AXIS_90:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, yCenter, DataPhysModel.Y_AXIS_OUT, xCenter);
				break;
			case DataPhysModel.Z_AXIS_180:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, xCenter, DataPhysModel.Y_AXIS_IDLE, yCenter);
				break;
			case DataPhysModel.Z_AXIS_270:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_IDLE, yCenter, DataPhysModel.Y_AXIS_OUT, xCenter);
				break;
		}
		DataPhysIsUpdated = false;
		return ans;
	}
	int ZoomBothIn(float xCenter, float yCenter)
	{
		Debug.Log("ZOOM IN");
		Debug.Log("xCenter: " + xCenter);
		Debug.Log("yCenter: " + yCenter);
		int ans = DataPhysModel.ZOOM_ERROR_NOT_FOUND;
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, xCenter, DataPhysModel.Y_AXIS_IN, yCenter);
				break;
			case DataPhysModel.Z_AXIS_90:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, yCenter, DataPhysModel.Y_AXIS_IN, expanDialSticks.NbRows - xCenter);
				break;
			case DataPhysModel.Z_AXIS_180:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, expanDialSticks.NbColumns - xCenter, DataPhysModel.Y_AXIS_IN, expanDialSticks.NbRows - yCenter);
				break;
			case DataPhysModel.Z_AXIS_270:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_IN, expanDialSticks.NbRows -  yCenter, DataPhysModel.Y_AXIS_IN, xCenter);
				break;
		}
		DataPhysIsUpdated = false;
		return ans;
	}
	int ZoomBothOut(float xCenter, float yCenter)
	{
		Debug.Log("ZOOM OUT");
		Debug.Log("xCenter: " + xCenter);
		Debug.Log("yCenter: " + yCenter);
		int ans = DataPhysModel.ZOOM_ERROR_NOT_FOUND;
		switch (dataSet.orientation)
		{
			case DataPhysModel.Z_AXIS_0:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, xCenter, DataPhysModel.Y_AXIS_OUT, yCenter);
				break;
			case DataPhysModel.Z_AXIS_90:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, yCenter, DataPhysModel.Y_AXIS_OUT, expanDialSticks.NbRows - xCenter);
				break;
			case DataPhysModel.Z_AXIS_180:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, expanDialSticks.NbColumns - xCenter, DataPhysModel.Y_AXIS_OUT, expanDialSticks.NbRows - yCenter);
				break;
			case DataPhysModel.Z_AXIS_270:
				ans = dataPhysModel.Zoom(DataPhysModel.X_AXIS_OUT, expanDialSticks.NbRows - yCenter, DataPhysModel.Y_AXIS_OUT,  xCenter);
				break;
		}
		DataPhysIsUpdated = false;
		return ans;
	}
	int RotateClockWise()
	{
		Debug.Log("ROTATE CW");
		int ans = dataPhysModel.Rotate(DataPhysModel.Z_AXIS_CW);
		DataPhysIsUpdated = false;
		return ans;
	}
	int RotateCounterClockWise()
	{
		Debug.Log("ROTATE CCW");
		int ans = dataPhysModel.Rotate(DataPhysModel.Z_AXIS_CCW);
		DataPhysIsUpdated = false;
		return ans;
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
						int ans = ZoomHorizontalAxisOut(yCenter, xCenter);
						if(ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(i, lastLeft, ct));
							errors.Enqueue(new Vector3(i, firstRight, ct));
						}
						goto LoopEnd;
					}
					if (lastRight < firstLeft) // ZOOM IN HORIZONTAL
					{
						float xCenter = i;
						float yCenter = lastRight + (firstLeft - lastRight) / 2f;
						int ans = ZoomHorizontalAxisIn(yCenter, xCenter);
						if (ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(i, lastRight, ct));
							errors.Enqueue(new Vector3(i, firstLeft, ct));
						}
						goto LoopEnd;
					}
				}
				if(firstRight > -1 && lastRight > -1) // MULTI PAN RIGHT
				{
					int ans = PanRight(Math.Abs(lastRight - firstRight) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(i, firstRight, ct));
						errors.Enqueue(new Vector3(i, lastRight, ct));
					}
					goto LoopEnd;
				}
				if (firstLeft > -1 && lastLeft > -1) // MULTI PAN LEFT
				{
					int ans = PanLeft(Math.Abs(lastLeft - firstLeft) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(i, firstLeft, ct));
						errors.Enqueue(new Vector3(i, lastLeft, ct));
					}
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
						int ans = ZoomVerticalAxisOut(yCenter, xCenter);
						if (ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(lastUp, j, ct));
							errors.Enqueue(new Vector3(firstDown, j, ct));
						}
						goto LoopEnd;
					}
					if (lastDown < firstUp) // ZOOM IN HORIZONTAL
					{
						float xCenter = lastDown + (firstUp - lastDown) / 2f;
						float yCenter = j;
						int ans = ZoomVerticalAxisIn(yCenter, xCenter);
						if (ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(lastDown, j, ct));
							errors.Enqueue(new Vector3(firstUp, j, ct));
						}
						goto LoopEnd;
					}
				}

				if (firstUp > -1 && lastUp > -1) // MULTI PAN UP
				{
					int ans = PanUp(Math.Abs(lastUp - firstUp) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(lastUp, j, ct));
						errors.Enqueue(new Vector3( firstUp, j, ct));
					}
					goto LoopEnd;
				}
				if (firstDown > -1 && lastDown > -1) // MULTI PAN DOWN
				{
					int ans = PanLeft(Math.Abs(lastDown - firstDown) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(lastDown, j, ct));
						errors.Enqueue(new Vector3(firstDown, j, ct));
					}
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
						int ans = ZoomBothOut(yCenter, xCenter);
						if (ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(lastDown, lastRight, ct));
							errors.Enqueue(new Vector3(firstUp, firstLeft, ct));
						}
						goto LoopEnd;
					}
					if (lastUp < firstDown && lastLeft < firstRight) // ZOOM IN 
					{
						float xCenter = lastUp + (firstDown - lastUp) / 2f;
						float yCenter = lastLeft + (firstRight - lastLeft) / 2f;
						int ans = ZoomBothIn(yCenter, xCenter);
						if (ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(lastUp, lastLeft, ct));
							errors.Enqueue(new Vector3(firstDown, firstRight, ct));
						}
						goto LoopEnd;
					}
				}

				if (firstUp > -1 && lastUp > -1 && firstRight > -1 && lastRight > -1) // MULTI PAN UP RIGHT
				{
					int ans = PanUpRight(Math.Abs(lastRight - firstRight) + 1, Math.Abs(lastUp - firstUp) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstUp, firstRight, ct));
						errors.Enqueue(new Vector3(lastUp, lastRight, ct));
					}
					goto LoopEnd;
				}
				if (firstUp > -1 && lastUp > -1 && firstLeft > -1 && lastLeft > -1) // MULTI PAN UP LEFT
				{
					int ans = PanUpLeft(Math.Abs(lastLeft - firstLeft) + 1, Math.Abs(lastUp - firstUp) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstUp, firstLeft, ct));
						errors.Enqueue(new Vector3(lastUp, lastLeft, ct));
					}
					goto LoopEnd;
				}

				if (firstDown > -1 && lastDown > -1 && firstRight > -1 && lastRight > -1) // MULTI PAN DOWN RIGHT
				{
					int ans = PanDownRight(Math.Abs(lastRight - firstRight) + 1, Math.Abs(lastDown - firstDown) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstDown, firstRight, ct));
						errors.Enqueue(new Vector3(lastDown, lastRight, ct));
					}
					goto LoopEnd;
				}
				if (firstDown > -1 && lastUp > -1 && firstLeft > -1 && lastLeft > -1) // MULTI PAN DOWN LEFT
				{
					int ans = PanDownLeft(Math.Abs(lastLeft - firstLeft) + 1, Math.Abs(lastDown - firstDown) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstDown, firstLeft, ct));
						errors.Enqueue(new Vector3(lastDown, lastLeft, ct));
					}
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
						int ans = ZoomBothOut(yCenter, xCenter);
						if (ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(lastUp, lastLeft, ct));
							errors.Enqueue(new Vector3(firstDown, firstRight, ct));
						}
						goto LoopEnd;
					}
					if (lastDown < firstUp && lastRight < firstLeft) // ZOOM IN 
					{
						float xCenter = lastDown + (firstUp - lastDown) / 2f;
						float yCenter = lastRight + (firstLeft - lastRight) / 2f;
						int ans = ZoomBothIn(yCenter, xCenter);
						if (ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(lastDown, lastRight, ct));
							errors.Enqueue(new Vector3(firstUp, firstLeft, ct));
						}
						goto LoopEnd;
					}
				}

				if (firstUp > -1 && lastUp > -1 && firstRight > -1 && lastRight > -1) // MULTI PAN UP RIGHT
				{
					int ans = PanUpRight(Math.Abs(lastRight - firstRight) + 1, Math.Abs(lastUp - firstUp) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstRight, firstUp, ct));
						errors.Enqueue(new Vector3(lastRight, lastUp, ct));
					}
					goto LoopEnd;
				}
				if (firstUp > -1 && lastUp > -1 && firstLeft > -1 && lastLeft > -1) // MULTI PAN UP LEFT
				{
					int ans = PanUpLeft(Math.Abs(lastLeft - firstLeft) + 1, Math.Abs(lastUp - firstUp) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstLeft, firstUp, ct));
						errors.Enqueue(new Vector3(lastLeft, lastUp, ct));
					}
					goto LoopEnd;
				}

				if (firstDown > -1 && lastDown > -1 && firstRight > -1 && lastRight > -1) // MULTI PAN DOWN RIGHT
				{
					int ans = PanDownRight(Math.Abs(lastRight - firstRight) + 1, Math.Abs(lastDown - firstDown) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstRight, firstDown, ct));
						errors.Enqueue(new Vector3(lastRight, lastDown, ct));
					}
					goto LoopEnd;
				}
				if (firstDown > -1 && lastUp > -1 && firstLeft > -1 && lastLeft > -1) // MULTI PAN DOWN LEFT
				{
					int ans = PanDownLeft(Math.Abs(lastLeft - firstLeft) + 1, Math.Abs(lastDown - firstDown) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstLeft, firstDown, ct));
						errors.Enqueue(new Vector3(lastLeft, lastDown, ct));
					}
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
							int ans = PanUpRight(1,1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
							goto LoopEnd;
						}
						else if (direction.x >= JOYSTICK_THRESHOLD && direction.y <= -JOYSTICK_THRESHOLD)
						{
							int ans = PanUpLeft(1, 1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
							goto LoopEnd;
						}
						else if (direction.x <= -JOYSTICK_THRESHOLD && direction.y >= JOYSTICK_THRESHOLD)
						{

							int ans = PanDownRight(1, 1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
							goto LoopEnd;
						}
						else if (direction.x <= -JOYSTICK_THRESHOLD && direction.y <= -JOYSTICK_THRESHOLD)
						{

							int ans = PanDownLeft(1, 1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
							goto LoopEnd;
						}
						else if (direction.x >= JOYSTICK_THRESHOLD)
						{
							int ans = PanUp(1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
							goto LoopEnd;
						}
						else if (direction.x <= -JOYSTICK_THRESHOLD)
						{
							int ans = PanDown(1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
							goto LoopEnd;
						}
						else if (direction.y >= JOYSTICK_THRESHOLD)
						{
							int ans = PanRight(1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
							goto LoopEnd;
						}
						else if (direction.y <= -JOYSTICK_THRESHOLD)
						{
							int ans = PanLeft(1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
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
				if (errors.Count > 0)
				{
					for (int i = 0; i < errors.Count; i++)
					{
						Vector3 err = errors.Dequeue();
						if (ct - err.z < 1f)
						{
							expanDialSticks[(int)err.x, (int)err.y].TargetColor = Color.red;
							expanDialSticks[(int)err.x, (int)err.y].TargetTextureChangeDuration = 0.1f;
							errors.Enqueue(err);
						}
					}
				}
				expanDialSticks.triggerTextureChange();
				expanDialSticks.triggerShapeChange();
				DataPhysIsUpdated = true;
			} else
			{
				if (errors.Count > 0)
				{
					for (int i = 0; i < errors.Count; i++)
					{
						Vector3 err = errors.Dequeue();
						int x = (int)err.x;
						int y = (int)err.y;
						if (ct - err.z < 1f)
						{
							expanDialSticks[x, y].TargetColor = Color.black;
							expanDialSticks[x, y].TargetTextureChangeDuration = 0.1f;
							errors.Enqueue(err);
						} else
						{

							float coeff = Mathf.InverseLerp(dataSet.minValue, dataSet.maxValue, dataSet.data[x, y]);
							expanDialSticks[x, y].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
							expanDialSticks[x, y].TargetTextureChangeDuration = 0.1f;
							errors.Enqueue(err);

						}
					}
					expanDialSticks.triggerTextureChange();
				}
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
