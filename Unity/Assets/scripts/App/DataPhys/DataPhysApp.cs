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

public class Manipulation
{
	public const int NONE = 0;
	public const int PAN_LEFT = 1;
	public const int PAN_RIGHT = 2;
	public const int PAN_UP = 3;
	public const int PAN_DOWN = 4;
	public const int PAN_UP_LEFT = 5;
	public const int PAN_UP_RIGHT = 6;
	public const int PAN_DOWN_LEFT = 7;
	public const int PAN_DOWN_RIGHT = 8;
	public const int ZOOM_HORIZONTAL_IN = 9;
	public const int ZOOM_HORIZONTAL_OUT = 10;
	public const int ZOOM_VERTICAL_IN = 11;
	public const int ZOOM_VERTICAL_OUT = 12;
	public const int ZOOM_DIAGONAL_IN = 13;
	public const int ZOOM_DIAGONAL_OUT = 14;
	public const int ROTATE_CW = 15;
	public const int ROTATE_CCW = 16;
	public Vector2 p0 { get; set; }
	public Vector2 p1 { get; set; }
	public float dt { get; set; }
	public int type { get; set; }
	public Manipulation(int type, Vector2 p0, Vector2 p1, float dt)
	{
		this.type = type;
		this.p0 = p0;
		this.p1 = p1;
		this.dt = dt;
	}

}
public class DataPhysTransition
{


}
public class DataPhysApp : MonoBehaviour
{

	public GameObject expanDialSticksPrefab;
	public GUISkin guiSkin;
	private ExpanDialSticks expanDialSticks;
	private bool connected = false;
	public float EVENT_INTERVAL = 0.5f; // 0.2f;
	private float lastRising = 0f;

	private const float JOYSTICK_THRESHOLD = 10f;
	private const float TRANSITION_DELAY = 0.25f;


	private DataPhysModel dataPhysModel;
	private DataSet dataSet;
	private bool dataTransition = true;
	private Vector2[,] directions;
	private int rotation = 0;
	private Queue<Vector3> errors;
	private Manipulation manipulation;

	private const int RIGHT = 0x0001;
	private const int LEFT = 0x0010;
	private const int UP = 0x0100;
	private const int DOWN = 0x1000;
	private Coroutine DataTransitionHandler = null;

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
	public void ViewFromDatum(int row, int column)
	{
		float datum = (column < dataSet.data.GetLength(0) && row < dataSet.data.GetLength(1)) ? dataSet.data[column, row] : 0f;
		float coeff = Mathf.InverseLerp(dataSet.minValue, dataSet.maxValue, datum);
		sbyte height = Convert.ToSByte((int)Mathf.Lerp(0f, 40f, coeff));
		float textRotation = dataSet.orientation * 90f;

		expanDialSticks[row, column].TargetText = "<b>" + (int)datum + " Wh </b>";//xLabel + "\n<b>" + yLabel;
		expanDialSticks[row, column].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
		expanDialSticks[row, column].TargetTextRotation = textRotation;
		expanDialSticks[row, column].TargetTextureChangeDuration = 1f;

		expanDialSticks[row, column].TargetPosition = height;
		expanDialSticks[row, column].TargetShapeChangeDuration = 1f;
	}
	public void LegendFromLabels()
	{
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

	IEnumerator TriggerTransition()
	{
		if(manipulation != null)
		{
			switch (manipulation.type)
			{
				case Manipulation.PAN_RIGHT:
					for (int i = 0; i < expanDialSticks.NbColumns; i++)
					{
						for (int j = 0; j < expanDialSticks.NbRows; j++)
						{
							ViewFromDatum(j, i);
						}
						expanDialSticks.triggerTextureChange();
						expanDialSticks.triggerShapeChange();
						yield return new WaitForSeconds(TRANSITION_DELAY);
					}
					break;
				case Manipulation.PAN_LEFT:
					for (int i = 0; i < expanDialSticks.NbColumns; i++)
					{
						for (int j = 0; j < expanDialSticks.NbRows; j++)
						{
							ViewFromDatum(expanDialSticks.NbRows - 1 - j, i);
						}
						expanDialSticks.triggerTextureChange();
						expanDialSticks.triggerShapeChange();
						yield return new WaitForSeconds(TRANSITION_DELAY);
					}
					break;
				case Manipulation.PAN_DOWN:
					for (int j = 0; j < expanDialSticks.NbRows; j++)
					{
						for (int i = 0; i < expanDialSticks.NbColumns; i++)
						{
							ViewFromDatum(j, i);
						}
						expanDialSticks.triggerTextureChange();
						expanDialSticks.triggerShapeChange();
						yield return new WaitForSeconds(TRANSITION_DELAY);
					}
					break;
				case Manipulation.PAN_UP:
					for (int j = 0; j < expanDialSticks.NbRows; j++)
					{
						for (int i = 0; i < expanDialSticks.NbColumns; i++)
						{
							ViewFromDatum(j, expanDialSticks.NbColumns - 1 - i);
						}
						expanDialSticks.triggerTextureChange();
						expanDialSticks.triggerShapeChange();
						yield return new WaitForSeconds(TRANSITION_DELAY);
					}
					break;

				case Manipulation.PAN_DOWN_RIGHT:
					for (int h = 0; h < expanDialSticks.NbRows + expanDialSticks.NbColumns; h++)
					{
						for (int w = 0; w < h; w++)
						{
							int j = h - w;
							int i = w;
							if (i < expanDialSticks.NbColumns && j < expanDialSticks.NbRows)
							{
								ViewFromDatum(j, i);
							}
						}
						expanDialSticks.triggerTextureChange();
						expanDialSticks.triggerShapeChange();
						yield return new WaitForSeconds(TRANSITION_DELAY);
					}
					break;
				case Manipulation.PAN_UP_RIGHT:
					for (int h = 0; h < expanDialSticks.NbRows + expanDialSticks.NbColumns; h++)
					{
						for (int w = 0; w < h; w++)
						{
							int j = h - w;
							int i = w;
							if (i < expanDialSticks.NbColumns && j < expanDialSticks.NbRows)
							{
								ViewFromDatum(expanDialSticks.NbRows - 1 - j, i);
							}
						}
						expanDialSticks.triggerTextureChange();
						expanDialSticks.triggerShapeChange();
						yield return new WaitForSeconds(TRANSITION_DELAY);
					}
					break;
				case Manipulation.PAN_DOWN_LEFT:
					for (int h = 0; h < expanDialSticks.NbRows + expanDialSticks.NbColumns; h++)
					{
						for (int w = 0; w < h; w++)
						{
							int j = h - w;
							int i = w;
							if (i < expanDialSticks.NbColumns && j < expanDialSticks.NbRows)
							{
								ViewFromDatum(j, expanDialSticks.NbColumns - 1 - i);
							}
						}
						expanDialSticks.triggerTextureChange();
						expanDialSticks.triggerShapeChange();
						yield return new WaitForSeconds(TRANSITION_DELAY);
					}
					break;
				case Manipulation.PAN_UP_LEFT:
					for (int h = 0; h < expanDialSticks.NbRows + expanDialSticks.NbColumns; h++)
					{
						for (int w = 0; w < h; w++)
						{
							int j = h - w;
							int i = w;
							if (i < expanDialSticks.NbColumns && j < expanDialSticks.NbRows)
							{
								ViewFromDatum(expanDialSticks.NbRows - 1 - j, expanDialSticks.NbColumns - 1 - i);
							}
						}
						expanDialSticks.triggerTextureChange();
						expanDialSticks.triggerShapeChange();
						yield return new WaitForSeconds(TRANSITION_DELAY);
					}
					break;
				default:
					for (int i = 0; i < expanDialSticks.NbColumns; i++)
					{
						for (int j = 0; j < expanDialSticks.NbRows; j++)
						{
							ViewFromDatum(j, i);
						}
					}
					expanDialSticks.triggerTextureChange();
					expanDialSticks.triggerShapeChange();
				break;
			}
		} else { 
			for (int i = 0; i < expanDialSticks.NbColumns; i++)
			{
				for (int j = 0; j < expanDialSticks.NbRows; j++)
				{
					ViewFromDatum(j, i);
				}
			}
			expanDialSticks.triggerTextureChange();
			expanDialSticks.triggerShapeChange();
		}
		yield break;
	}

	private void ViewFromDataSet()
	{
		dataSet = dataPhysModel.DataSet();
		LegendFromLabels();

		for (int i = 0; i < expanDialSticks.NbColumns; i++)
		{
			for (int j = 0; j < expanDialSticks.NbRows; j++)
			{
				ViewFromDatum(j, i);
			}
		}

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
		dataTransition = true;
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
		dataTransition = true;
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
		dataTransition = true;
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
		dataTransition = true;
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
		dataTransition = true;
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
		dataTransition = true;
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
		dataTransition = true;
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
		dataTransition = true;
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
		dataTransition = true;
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
		dataTransition = true;
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
		dataTransition = true;
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
		dataTransition = true;
		return ans;
	}
	int ZoomBothIn(float xCenter, float yCenter)
	{
		Debug.Log("ZOOM IN");
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
		dataTransition = true;
		return ans;
	}
	int ZoomBothOut(float xCenter, float yCenter)
	{
		Debug.Log("ZOOM OUT");
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
		dataTransition = true;
		return ans;
	}
	int RotateClockWise()
	{
		Debug.Log("ROTATE CW");
		int ans = dataPhysModel.Rotate(DataPhysModel.Z_AXIS_CW);
		dataTransition = true;
		return ans;
	}
	int RotateCounterClockWise()
	{
		Debug.Log("ROTATE CCW");
		int ans = dataPhysModel.Rotate(DataPhysModel.Z_AXIS_CCW);
		dataTransition = true;
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
						Debug.Log("ZOOM OUT HORIZONTAL FOUND!");
						float xCenter = i;
						float yCenter = lastLeft + (firstRight - lastLeft) / 2f;
						int ans = ZoomHorizontalAxisOut(yCenter, xCenter);
						if(ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(i, lastLeft, ct));
							errors.Enqueue(new Vector3(i, firstRight, ct));
						} else
						{
							manipulation = new Manipulation(Manipulation.ZOOM_HORIZONTAL_OUT, new Vector2(i, lastLeft), new Vector2(i, firstRight), ct);
						}
						goto LoopEnd;
					}
					if (lastRight < firstLeft) // ZOOM IN HORIZONTAL
					{
						Debug.Log("ZOOM IN HORIZONTAL FOUND!");
						float xCenter = i;
						float yCenter = lastRight + (firstLeft - lastRight) / 2f;
						int ans = ZoomHorizontalAxisIn(yCenter, xCenter);
						if (ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(i, lastRight, ct));
							errors.Enqueue(new Vector3(i, firstLeft, ct));
						}
						else
						{
							manipulation = new Manipulation(Manipulation.ZOOM_HORIZONTAL_IN, new Vector2(i, lastRight), new Vector2(i, firstLeft), ct);
						}
						goto LoopEnd;
					}
				}
				if(firstRight > -1 && lastRight > -1 && firstRight != lastRight) // MULTI PAN RIGHT
				{
					//Debug.Log("MULTI PAN RIGHT FOUND!");
					int ans = PanRight(Math.Abs(lastRight - firstRight) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(i, firstRight, ct));
						errors.Enqueue(new Vector3(i, lastRight, ct));
					}
					else
					{
						manipulation = new Manipulation(Manipulation.PAN_RIGHT, new Vector2(i, firstRight), new Vector2(i, lastRight), ct);
					}
					goto LoopEnd;
				}
				if (firstLeft > -1 && lastLeft > -1 && firstLeft != lastLeft) // MULTI PAN LEFT
				{
					//Debug.Log("MULTI PAN LEFT FOUND!");
					int ans = PanLeft(Math.Abs(lastLeft - firstLeft) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(i, firstLeft, ct));
						errors.Enqueue(new Vector3(i, lastLeft, ct));
					}
					else
					{
						manipulation = new Manipulation(Manipulation.PAN_LEFT, new Vector2(i, firstLeft), new Vector2(i, lastLeft), ct);
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
					if (lastUp < firstDown) // ZOOM OUT VERTICAL
					{
						//Debug.Log("ZOOM VERTICAL OUT FOUND!");
						float xCenter = lastUp + (firstDown - lastUp) / 2f;
						float yCenter = j;
						int ans = ZoomVerticalAxisOut(yCenter, xCenter);
						if (ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(lastUp, j, ct));
							errors.Enqueue(new Vector3(firstDown, j, ct));
						}
						else
						{
							manipulation = new Manipulation(Manipulation.ZOOM_VERTICAL_OUT, new Vector2(lastUp, j), new Vector2(firstDown, j), ct);
						}
						goto LoopEnd;
					}
					if (lastDown < firstUp) // ZOOM IN HORIZONTAL
					{
						//Debug.Log("ZOOM VERTICAL IN FOUND!");
						float xCenter = lastDown + (firstUp - lastDown) / 2f;
						float yCenter = j;
						int ans = ZoomVerticalAxisIn(yCenter, xCenter);
						if (ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(lastDown, j, ct));
							errors.Enqueue(new Vector3(firstUp, j, ct));
						}
						else
						{
							manipulation = new Manipulation(Manipulation.ZOOM_VERTICAL_IN, new Vector2(lastDown, j), new Vector2(firstUp, j), ct);
						}
						goto LoopEnd;
					}
				}

				if (firstUp > -1 && lastUp > -1 && firstUp != lastUp) // MULTI PAN UP
				{
					//Debug.Log("MULTI PAN UP FOUND!");
					int ans = PanUp(Math.Abs(lastUp - firstUp) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(lastUp, j, ct));
						errors.Enqueue(new Vector3(firstUp, j, ct));
					}
					else
					{
						manipulation = new Manipulation(Manipulation.PAN_UP, new Vector2(lastUp, j), new Vector2(firstUp, j), ct);
					}
					goto LoopEnd;
				}
				if (firstDown > -1 && lastDown > -1 && firstDown != lastDown) // MULTI PAN DOWN
				{
					//Debug.Log("MULTI PAN DOWN FOUND!");
					int ans = PanDown(Math.Abs(lastDown - firstDown) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(lastDown, j, ct));
						errors.Enqueue(new Vector3(firstDown, j, ct));
					}
					else
					{
						manipulation = new Manipulation(Manipulation.PAN_DOWN, new Vector2(lastDown, j), new Vector2(firstDown, j), ct);
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
						//Debug.Log("ZOOM DIAGONAL OUT FOUND!");
						float xCenter = lastDown + (firstUp - lastDown) / 2f;
						float yCenter = lastRight + (firstLeft - lastRight) / 2f;
						int ans = ZoomBothOut(yCenter, xCenter);
						if (ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(lastDown, lastRight, ct));
							errors.Enqueue(new Vector3(firstUp, firstLeft, ct));
						}
						else
						{
							manipulation = new Manipulation(Manipulation.ZOOM_DIAGONAL_OUT, new Vector2(lastDown, lastRight), new Vector2(firstUp, firstLeft), ct);
						}
						goto LoopEnd;
					}
					if (lastUp < firstDown && lastLeft < firstRight) // ZOOM IN 
					{
						//Debug.Log("ZOOM DIAGONAL IN FOUND!");
						float xCenter = lastUp + (firstDown - lastUp) / 2f;
						float yCenter = lastLeft + (firstRight - lastLeft) / 2f;
						int ans = ZoomBothIn(yCenter, xCenter);
						if (ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(lastUp, lastLeft, ct));
							errors.Enqueue(new Vector3(firstDown, firstRight, ct));
						}
						else
						{
							manipulation = new Manipulation(Manipulation.ZOOM_DIAGONAL_IN, new Vector2(lastUp, lastLeft), new Vector2(firstDown, firstRight), ct);
						}
						goto LoopEnd;
					}
				}

				if (firstUp > -1 && lastUp > -1 &&  firstRight > -1 && lastRight > -1) // MULTI PAN UP RIGHT
				{
					//Debug.Log("MULTI PAN UP RIGHT FOUND!");
					int ans = PanUpRight(Math.Abs(lastRight - firstRight) + 1, Math.Abs(lastUp - firstUp) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstUp, firstRight, ct));
						errors.Enqueue(new Vector3(lastUp, lastRight, ct));
					}
					else
					{
						manipulation = new Manipulation(Manipulation.PAN_UP_RIGHT, new Vector2(firstUp, firstRight), new Vector2(lastUp, lastRight), ct);
					}
					goto LoopEnd;
				}
				if (firstUp > -1 && lastUp > -1 && firstLeft > -1 && lastLeft > -1) // MULTI PAN UP LEFT
				{
					//Debug.Log("MULTI PAN UP LEFT FOUND!");
					int ans = PanUpLeft(Math.Abs(lastLeft - firstLeft) + 1, Math.Abs(lastUp - firstUp) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstUp, firstLeft, ct));
						errors.Enqueue(new Vector3(lastUp, lastLeft, ct));
					}
					else
					{
						manipulation = new Manipulation(Manipulation.PAN_UP_LEFT, new Vector2(firstUp, firstLeft), new Vector2(lastUp, lastLeft), ct);
					}
					goto LoopEnd;
				}

				if (firstDown > -1 && lastDown > -1 && firstRight > -1 && lastRight > -1) // MULTI PAN DOWN RIGHT
				{
					//Debug.Log("MULTI PAN DOWN RIGHT FOUND!");
					int ans = PanDownRight(Math.Abs(lastRight - firstRight) + 1, Math.Abs(lastDown - firstDown) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstDown, firstRight, ct));
						errors.Enqueue(new Vector3(lastDown, lastRight, ct));
					}
					else
					{
						manipulation = new Manipulation(Manipulation.PAN_DOWN_RIGHT, new Vector2(firstDown, firstRight), new Vector2(lastDown, lastRight), ct);
					}
					goto LoopEnd;
				}
				if (firstDown > -1 && lastUp > -1 && firstLeft > -1 && lastLeft > -1) // MULTI PAN DOWN LEFT
				{
					//Debug.Log("MULTI PAN DOWN LEFT FOUND!");
					int ans = PanDownLeft(Math.Abs(lastLeft - firstLeft) + 1, Math.Abs(lastDown - firstDown) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstDown, firstLeft, ct));
						errors.Enqueue(new Vector3(lastDown, lastLeft, ct));
					}
					else
					{
						manipulation = new Manipulation(Manipulation.PAN_DOWN_LEFT, new Vector2(firstDown, firstLeft), new Vector2(lastDown, lastLeft), ct);
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
						//Debug.Log("ZOOM DIAGONAL OUT FOUND!");
						float xCenter = lastUp + (firstDown - lastUp) / 2f;
						float yCenter = lastLeft + (firstRight - lastLeft) / 2f;
						int ans = ZoomBothOut(yCenter, xCenter);
						if (ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(lastUp, lastLeft, ct));
							errors.Enqueue(new Vector3(firstDown, firstRight, ct));
						}
						else
						{
							manipulation = new Manipulation(Manipulation.ZOOM_DIAGONAL_OUT, new Vector2(lastUp, lastLeft), new Vector2(firstDown, firstRight), ct);
						}
						goto LoopEnd;
					}
					if (lastDown < firstUp && lastRight < firstLeft) // ZOOM IN 
					{
						//Debug.Log("ZOOM DIAGONAL IN FOUND!");
						float xCenter = lastDown + (firstUp - lastDown) / 2f;
						float yCenter = lastRight + (firstLeft - lastRight) / 2f;
						int ans = ZoomBothIn(yCenter, xCenter);
						if (ans == DataPhysModel.ZOOM_ERROR_NOT_FOUND || ans == DataPhysModel.ZOOM_ERROR_OUT_OF_BOUNDS)
						{
							errors.Enqueue(new Vector3(lastDown, lastRight, ct));
							errors.Enqueue(new Vector3(firstUp, firstLeft, ct));
						}
						else
						{
							manipulation = new Manipulation(Manipulation.ZOOM_DIAGONAL_IN, new Vector2(lastDown, lastRight), new Vector2(firstUp, firstLeft), ct);
						}
						goto LoopEnd;
					}
				}

				if (firstUp > -1 && lastUp > -1 && firstRight > -1 && lastRight > -1) // MULTI PAN UP RIGHT
				{
					//Debug.Log("MULTI PAN UP RIGHT FOUND!");
					int ans = PanUpRight(Math.Abs(lastRight - firstRight) + 1, Math.Abs(lastUp - firstUp) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstRight, firstUp, ct));
						errors.Enqueue(new Vector3(lastRight, lastUp, ct));
					}
					else
					{
						manipulation = new Manipulation(Manipulation.PAN_UP_RIGHT, new Vector2(firstRight, firstUp), new Vector2(lastRight, lastUp), ct);
					}
					goto LoopEnd;
				}
				if (firstUp > -1 && lastUp > -1 && firstLeft > -1 && lastLeft > -1) // MULTI PAN UP LEFT
				{
					//Debug.Log("MULTI PAN UP LEFT FOUND!");
					int ans = PanUpLeft(Math.Abs(lastLeft - firstLeft) + 1, Math.Abs(lastUp - firstUp) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstLeft, firstUp, ct));
						errors.Enqueue(new Vector3(lastLeft, lastUp, ct));
					}
					else
					{
						manipulation = new Manipulation(Manipulation.PAN_UP_LEFT, new Vector2(firstLeft, firstUp), new Vector2(lastLeft, lastUp), ct);
					}
					goto LoopEnd;
				}

				if (firstDown > -1 && lastDown > -1 && firstRight > -1 && lastRight > -1) // MULTI PAN DOWN RIGHT
				{
					//Debug.Log("MULTI PAN DOWN RIGHT FOUND!");
					int ans = PanDownRight(Math.Abs(lastRight - firstRight) + 1, Math.Abs(lastDown - firstDown) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstRight, firstDown, ct));
						errors.Enqueue(new Vector3(lastRight, lastDown, ct));
					}
					else
					{
						manipulation = new Manipulation(Manipulation.PAN_DOWN_RIGHT, new Vector2(firstRight, firstDown), new Vector2(lastRight, lastDown), ct);
					}
					goto LoopEnd;
				}
				if (firstDown > -1 && lastUp > -1 && firstLeft > -1 && lastLeft > -1) // MULTI PAN DOWN LEFT
				{
					//Debug.Log("MULTI PAN DOWN LEFT FOUND!");
					int ans = PanDownLeft(Math.Abs(lastLeft - firstLeft) + 1, Math.Abs(lastDown - firstDown) + 1);
					if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
					{
						errors.Enqueue(new Vector3(firstLeft, firstDown, ct));
						errors.Enqueue(new Vector3(lastLeft, lastDown, ct));
					}
					else
					{
						manipulation = new Manipulation(Manipulation.PAN_DOWN_LEFT, new Vector2(firstLeft, firstDown), new Vector2(lastLeft, lastDown), ct);
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
							//Debug.Log("PAN UP RIGHT FOUND!");
							int ans = PanUpRight(1,1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
							else
							{
								manipulation = new Manipulation(Manipulation.PAN_UP_RIGHT, new Vector2(i, j), Vector2.zero, ct);
							}
							goto LoopEnd;
						}
						else if (direction.x >= JOYSTICK_THRESHOLD && direction.y <= -JOYSTICK_THRESHOLD)
						{
							//Debug.Log("PAN UP LEFT FOUND!");
							int ans = PanUpLeft(1, 1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
							else
							{
								manipulation = new Manipulation(Manipulation.PAN_UP_LEFT, new Vector2(i, j), Vector2.zero, ct);
							}
							goto LoopEnd;
						}
						else if (direction.x <= -JOYSTICK_THRESHOLD && direction.y >= JOYSTICK_THRESHOLD)
						{
							//Debug.Log("PAN DOWN RIGHT FOUND!");
							int ans = PanDownRight(1, 1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
							else
							{
								manipulation = new Manipulation(Manipulation.PAN_DOWN_RIGHT, new Vector2(i, j), Vector2.zero, ct);
							}
							goto LoopEnd;
						}
						else if (direction.x <= -JOYSTICK_THRESHOLD && direction.y <= -JOYSTICK_THRESHOLD)
						{
							//Debug.Log("PAN DOWN LEFT FOUND!");
							int ans = PanDownLeft(1, 1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
							else
							{
								manipulation = new Manipulation(Manipulation.PAN_DOWN_LEFT, new Vector2(i, j), Vector2.zero, ct);
							}
							goto LoopEnd;
						}
						else if (direction.x >= JOYSTICK_THRESHOLD)
						{
							//Debug.Log("PAN UP FOUND!");
							int ans = PanUp(1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
							else
							{
								manipulation = new Manipulation(Manipulation.PAN_UP, new Vector2(i, j), Vector2.zero, ct);
							}
							goto LoopEnd;
						}
						else if (direction.x <= -JOYSTICK_THRESHOLD)
						{
							//Debug.Log("PAN DOWN FOUND!");
							int ans = PanDown(1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
							else
							{
								manipulation = new Manipulation(Manipulation.PAN_DOWN, new Vector2(i, j), Vector2.zero, ct);
							}
							goto LoopEnd;
						}
						else if (direction.y >= JOYSTICK_THRESHOLD)
						{
							//Debug.Log("PAN RIGHT FOUND!");
							int ans = PanRight(1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
							else
							{
								manipulation = new Manipulation(Manipulation.PAN_RIGHT, new Vector2(i, j), Vector2.zero, ct);
							}
							goto LoopEnd;
						}
						else if (direction.y <= -JOYSTICK_THRESHOLD)
						{
							//Debug.Log("PAN LEFT FOUND!");
							int ans = PanLeft(1);
							if (ans == DataPhysModel.PAN_ERROR_NOT_FOUND || ans == DataPhysModel.PAN_ERROR_OUT_OF_BOUNDS)
							{
								errors.Enqueue(new Vector3(i, j, ct));
							}
							else
							{
								manipulation = new Manipulation(Manipulation.PAN_LEFT, new Vector2(i, j), Vector2.zero, ct);
							}
							goto LoopEnd;
						}

					}
				}
			}
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
			/*if (dataTransition)
			{
				StopCoroutine(TriggerTransition());
				// update dataset
				dataSet = dataPhysModel.DataSet();
				// set legend texture
				LegendFromLabels();
				// start bar chart shape & texture
				DataTransitionHandler = StartCoroutine(TriggerTransition());
				dataTransition = false;
			}*/
			
			if (dataTransition)
			{
				ViewFromDataSet();

				// trigger texture animation coroutines
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
							float coeff = Mathf.InverseLerp(dataSet.minValue, dataSet.maxValue, dataSet.data[y, x]);
							expanDialSticks[x, y].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
							expanDialSticks[x, y].TargetTextureChangeDuration = 0.1f;

						}
					}
				};
				expanDialSticks.triggerTextureChange();
				expanDialSticks.triggerShapeChange();
				dataTransition = false;
			}
			else
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
							float coeff = Mathf.InverseLerp(dataSet.minValue, dataSet.maxValue, dataSet.data[y, x]);
							expanDialSticks[x, y].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
							expanDialSticks[x, y].TargetTextureChangeDuration = 0.1f;

						}
					}
					expanDialSticks.triggerTextureChange();
				}
			}
			

			// ZOOM IN TOP LEFT TO BOTTOM RIGHT
			if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{

				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, 0, -JOYSTICK_THRESHOLD, -JOYSTICK_THRESHOLD);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, 0, JOYSTICK_THRESHOLD, JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, 0, JOYSTICK_THRESHOLD, JOYSTICK_THRESHOLD);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, 0, -JOYSTICK_THRESHOLD, -JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);

			}
			if (Input.GetKeyUp(KeyCode.KeypadPlus))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, -JOYSTICK_THRESHOLD, 0, JOYSTICK_THRESHOLD);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, JOYSTICK_THRESHOLD, 0, -JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, JOYSTICK_THRESHOLD, 0, -JOYSTICK_THRESHOLD);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, -JOYSTICK_THRESHOLD, 0, JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);
			}


			// ZOOM OUT TOP LEFT TO BOTTOM RIGHT
			if (Input.GetKeyDown(KeyCode.KeypadMinus))
			{

				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, 0, JOYSTICK_THRESHOLD, JOYSTICK_THRESHOLD);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, 0, -JOYSTICK_THRESHOLD, -JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, 0, -JOYSTICK_THRESHOLD, -JOYSTICK_THRESHOLD);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, 0, JOYSTICK_THRESHOLD, JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);

			}
			if (Input.GetKeyUp(KeyCode.KeypadMinus))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, JOYSTICK_THRESHOLD, 0, -JOYSTICK_THRESHOLD);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 3, 0, -JOYSTICK_THRESHOLD, 0, JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, -JOYSTICK_THRESHOLD, 0, JOYSTICK_THRESHOLD);
				HandleXAxisChanged(new object(), e);

				e = new ExpanDialStickEventArgs(DateTime.Now, 4, 1, JOYSTICK_THRESHOLD, 0, -JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);
			}

			// UP
			if (Input.GetKeyDown(KeyCode.Z))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 0, 2, 0, JOYSTICK_THRESHOLD, JOYSTICK_THRESHOLD);
				HandleXAxisChanged(new object(), e);
			}
			if (Input.GetKeyUp(KeyCode.Z))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 0, 2, JOYSTICK_THRESHOLD, 0, -JOYSTICK_THRESHOLD);
				HandleXAxisChanged(new object(), e);
			}

			// DOWN
			if (Input.GetKeyDown(KeyCode.S))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 4, 2, 0, -JOYSTICK_THRESHOLD, -JOYSTICK_THRESHOLD);
				HandleXAxisChanged(new object(), e);
			}
			if (Input.GetKeyUp(KeyCode.S))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 4, 2, -JOYSTICK_THRESHOLD, 0, JOYSTICK_THRESHOLD);
				HandleXAxisChanged(new object(), e);
			}

			// LEFT
			if (Input.GetKeyDown(KeyCode.Q))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 2, 0, 0, -JOYSTICK_THRESHOLD, -JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);
				/*e = new ExpanDialStickEventArgs(DateTime.Now, 2, 5, 0, -JOYSTICK_THRESHOLD, -JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);*/
			}
			if (Input.GetKeyUp(KeyCode.Q))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 2, 0, -JOYSTICK_THRESHOLD, 0, JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);
				/*e = new ExpanDialStickEventArgs(DateTime.Now, 2, 5, -JOYSTICK_THRESHOLD, 0, JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);*/
			}

			// RIGHT
			if (Input.GetKeyDown(KeyCode.D))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 2, 5, 0, JOYSTICK_THRESHOLD, JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);
				/*e = new ExpanDialStickEventArgs(DateTime.Now, 2, 0, 0, JOYSTICK_THRESHOLD, JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);*/
			}
			if (Input.GetKeyUp(KeyCode.D))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 2, 5, JOYSTICK_THRESHOLD, 0, -JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);
				/*e = new ExpanDialStickEventArgs(DateTime.Now, 2, 0, JOYSTICK_THRESHOLD, 0, -JOYSTICK_THRESHOLD);
				HandleYAxisChanged(new object(), e);*/
			}

			if (Input.GetKeyDown(KeyCode.A))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 2, 5, 0, -JOYSTICK_THRESHOLD, -JOYSTICK_THRESHOLD);
				HandleRotationChanged(new object(), e);
			}
			if (Input.GetKeyDown(KeyCode.E))
			{
				ExpanDialStickEventArgs e = new ExpanDialStickEventArgs(DateTime.Now, 2, 5, 0, JOYSTICK_THRESHOLD, JOYSTICK_THRESHOLD);
				HandleRotationChanged(new object(), e);
			}
		}
	}
}
