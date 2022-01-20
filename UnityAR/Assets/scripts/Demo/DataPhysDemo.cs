
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
using Leap.Unity;
using Random = UnityEngine.Random;
using System.Linq;

public class DataPhysDemo : MonoBehaviour
{

	// ExpanDialSticks Core
	public GameObject expanDialSticksPrefab;
	public GameObject capsuleHandLeftPrefab;
	public GameObject capsuleHandRightPrefab;
	public GameObject safeGuardPrefab;
	private MyCapsuleHand leftHand;
	private MyCapsuleHand rightHand;
	private SafeGuard safeGuard;
	private ExpanDialSticks expanDialSticks;
	private bool connected;

	private DataSet dataSet;
	private DataPhysModel dataPhysModel;
	void Start()
	{
		leftHand = capsuleHandLeftPrefab.GetComponent<MyCapsuleHand>();
		rightHand = capsuleHandRightPrefab.GetComponent<MyCapsuleHand>();
		expanDialSticks = expanDialSticksPrefab.GetComponent<ExpanDialSticks>();
		safeGuard = safeGuardPrefab.GetComponent<SafeGuard>();
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

		dataPhysModel = new DataPhysModel(expanDialSticks.NbRows, expanDialSticks.NbColumns);
		expanDialSticks.client_MqttConnect();

	}

	private void HandleConnecting(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("Application connecting to MQTT Broker @" + e.address + ":" + e.port + "...");
		connected = false;
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
	private void HandleConnected(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("Application connected.");
		connected = true;
		ViewFromDataSet();
		expanDialSticks.triggerTextureChange();
		expanDialSticks.triggerShapeChange();

	}

	private void HandleDisconnected(object sender, MqttConnectionEventArgs e)
	{
		Debug.Log("Application disconnected.");
		connected = false;
	}

	private void HandleXAxisChanged(object sender, ExpanDialStickEventArgs e)
	{

	}

	private void HandleYAxisChanged(object sender, ExpanDialStickEventArgs e)
	{

	}

	private void HandleClickChanged(object sender, ExpanDialStickEventArgs e)
	{

	}

	private void HandleRotationChanged(object sender, ExpanDialStickEventArgs e)
	{
		
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



	void Update()
	{
		// check if ExpanDialSticks is connected
		if (connected)
		{
		}
	}

}