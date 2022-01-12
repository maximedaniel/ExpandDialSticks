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
	public GUISkin guiSkin;
	private ExpanDialSticks expanDialSticks;
	private bool connected = false;

	private CultureInfo en = CultureInfo.CreateSpecificCulture("en-US");

	private IEnumerator coroutine;

    private int TimeChartSize = 5;
    private int[] TimeChartRows = new int[]{0, 1, 2, 3, 4};
    private int[] TimeChartColumns = new int[]{0, 0, 0, 0, 0};

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
		outputs.Enqueue( () => displayESTIA1ConsumptionMonthByMonth());
		// 2. Rotate right to switch from consumption to storage data
		inputs.Enqueue("2|0|" + RIGHT_ROTATION);
		outputs.Enqueue( () => displayESTIA1StorageMonthByMonth());
		// 3. Rotate right to switch from storage to production data
		inputs.Enqueue("2|0|" + RIGHT_ROTATION);
		outputs.Enqueue( () => displayESTIA1ProductionMonthByMonth());
		// 4. Rotate ESTIA1 left to switch from month-mode to building-mode
		inputs.Enqueue("2|0|" + LEFT_ROTATION);
		outputs.Enqueue( () => displayMarchProductionBuildingByBuilding());
		// 5. Rotate ESTIA5 left to switch from building-mode to month-mode
		inputs.Enqueue("4|0|" + LEFT_ROTATION);
		outputs.Enqueue( () => displayESTIA3ProductionMonthByMonth());
		// 6. Bend left one time to navigate past month
		inputs.Enqueue("4|0|" + BOTTOM_BENDING);
		outputs.Enqueue( () => displayESTIA3ProductionPastOneMonth());
		// 8. Bend left a second time to navigate past months
		inputs.Enqueue("4|0|" + BOTTOM_BENDING);
		outputs.Enqueue( () => displayESTIA3ProductionPastTwoMonths());
		// 8. Bend right two pins to navigate future months
		inputs.Enqueue("1|0|" + TOP_BENDING);
		outputs.Enqueue( () => displayESTIA3ProductionMonthByMonth());
		// 9. Bend two pins outside January month pin to zoom into days 
		inputs.Enqueue("2|0|" + BOTTOM_BENDING);
		outputs.Enqueue( () => displayESTIA3ProductionJanDayByDay());
		// 10. Bend two extreme pins outside to zoom out from Jan. days to months  
		inputs.Enqueue("0|0|" + TOP_BENDING);
		outputs.Enqueue( () => displayESTIA3ProductionMonthByMonth());
		// 11. Bend perpendicalar January month pin to show subscale chart of Jan. days production.
		inputs.Enqueue("1|0|" + RIGHT_BENDING);
		outputs.Enqueue( () => subDisplayESTIA3ProductionJanDayByDay());
		// 12. Click to lock ESTIA3 Jan. month.
		inputs.Enqueue("1|0|" + PUSH);
		outputs.Enqueue( () => displayESTIA3ProductionMonthByMonthLocked());
		// 13. Bend right one time to navigate future month.
		inputs.Enqueue("4|0|" + BOTTOM_BENDING);
		outputs.Enqueue( () => displayESTIA3ProductionMonthByMonthAfterLock());
		// 14. Rotate right to switch from prod to conso data
		inputs.Enqueue("2|0|" + RIGHT_ROTATION);
		outputs.Enqueue( () => displayESTIA3ConsumptionMonthByMonthAfterLock());
		// 15. Bend perpendicalar January month pin to show subscale chart of Jan. days consumption.
		inputs.Enqueue("2|0|" + RIGHT_BENDING);
		outputs.Enqueue( () => subDisplayESTIA3ConsumptionJanDayByDay());
		// DONE Compare same space over time for two differents data type.

		//  Rotate ESTIA3 Jan Conso left to switch from month-mode to building-mode
		inputs.Enqueue("2|0|" + LEFT_ROTATION);
		outputs.Enqueue(() => displayAndSubDisplayConsumptionJanBuildingByBuilding());

		//  Rotate ESTIA3 Jan Prod left to switch from month-mode to building-mode
		inputs.Enqueue("1|0|" + LEFT_ROTATION);
		outputs.Enqueue( () => subDisplayESTIA3ProductionJanRoomByRoom());

		// DONE Compare same time over space for two differents data type.
		
		// Bend right to move ESTIA2 after ESTIA3
		inputs.Enqueue("4|0|" + BOTTOM_BENDING);
		outputs.Enqueue( () => displayAndSubDisplayConsumptionJanBuildingByBuildingAfterOneBuilding());

		// Bend down ESTIA3 to hide subchart
		inputs.Enqueue("3|0|" + LEFT_BENDING);
		outputs.Enqueue( () => hideSubDisplayConsumptionJanBuildingByBuildingAfterOneBuilding());

		//  Rotate ESTIA2 right to switch from conso to stor
		inputs.Enqueue("2|0|" + RIGHT_ROTATION);
		outputs.Enqueue( () => displayStorageJanBuildingByBuilding());

		//  Rotate ESTIA2 right to switch from stor to prod
		inputs.Enqueue("2|0|" + RIGHT_ROTATION);
		outputs.Enqueue( () => displayProductionJanBuildingByBuilding());

		// Bend Top ESTIA2 to show subcharts
		inputs.Enqueue("2|0|" + RIGHT_BENDING);
		outputs.Enqueue( () => subDisplayESTIA2ProductionJanRoomByRoom());
		// Compare same data type over time for two different spaces.

		// Bend Left subcharts are manipulable as charts
		inputs.Enqueue("2|5|" + RIGHT_BENDING);
		outputs.Enqueue( () => subDisplayESTIA2ESTIA3ProductionJanRoomByRoomByOneRoom());

		inputs.Enqueue("4|5|" + CLICK);
		outputs.Enqueue( () => Reset());
		
	}
	void DequeueOutput(){
		Action action;
		while (!outputs.TryDequeue(out action));
		action();
	}

	// 1.
	void displayESTIA1ConsumptionMonthByMonth(){
        string[] xTimes = new string[]{"Jan. 2021", "Feb. 2021", "Mar. 2021", "Apr. 2021", "May 2021"};
        string[] xSpaces = new string[]{"ESTIA1", "ESTIA1", "ESTIA1", "ESTIA1", "ESTIA1"};
        sbyte[] yData = new sbyte[]{12, 18, 25, 10, 20};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i];
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		/*string legend = "<line-height=1em><voffset=-8><size=32><color=\"green\">•</voffset><size=8><color=\"black\">energy production\n"
		+ "<voffset=-8><size=32><color=\"orange\">•</voffset><size=8><color=\"black\">energy storage\n"
		+ "<voffset=-8><size=32><color=\"blue\">•</voffset><size=8><color=\"black\">energy consumption\n";*/
		string labels = "<size=7><pos=3%>Past month <pos=85%>Next month\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFFF>■ <color=#000000FF>Energy Consumption<pos=30%><alpha=#FF>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900AA>■ <color=#000000AA>Energy Storage<pos=30%><alpha=#AA>Space<pos=50%><alpha=#AA>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00AA>■ <color=#000000AA>Energy Production<pos=50%><alpha=#FF>Months<pos=75%><alpha=#FF>Buildings ► ESTIA1\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 180f, 0f));
	}
	// 2.
	void displayESTIA1StorageMonthByMonth(){
        string[] xTimes = new string[]{"Jan. 2021", "Feb. 2021", "Mar. 2021", "Apr. 2021", "May 2021"};
        string[] xSpaces = new string[]{"ESTIA1", "ESTIA1", "ESTIA1", "ESTIA1", "ESTIA1"};
        sbyte[] yData = new sbyte[]{18, 10, 10, 18, 10};
									

        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i];
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f, 1f - (coeff * 0.33f), 1f - coeff);
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();		
		string labels = "<size=7><pos=3%>Past month <pos=85%>Next month\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFAA>■ <color=#000000AA>Energy Consumption<pos=30%><alpha=#FF>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900FF>■ <color=#000000FF>Energy Storage<pos=30%><alpha=#AA>Space<pos=50%><alpha=#AA>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00AA>■ <color=#000000AA>Energy Production<pos=50%><alpha=#FF>Months<pos=75%><alpha=#FF>Buildings ► ESTIA1\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 180f, 0f));
	}
	// 3.
	void displayESTIA1ProductionMonthByMonth(){
        string[] xTimes = new string[]{"Jan. 2021", "Feb. 2021", "Mar. 2021", "Apr. 2021", "May 2021"};
        string[] xSpaces = new string[]{"ESTIA1", "ESTIA1", "ESTIA1", "ESTIA1", "ESTIA1"};
        sbyte[] yData = new sbyte[]{20, 10, 25, 18, 12};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i];
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		string labels = "<size=7><pos=3%>Past month <pos=85%>Next month\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFAA>■ <color=#000000AA>Energy Consumption<pos=30%><alpha=#FF>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900AA>■ <color=#000000AA>Energy Storage<pos=30%><alpha=#AA>Space<pos=50%><alpha=#AA>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00FF>■ <color=#000000FF>Energy Production<pos=50%><alpha=#FF>Months<pos=75%><alpha=#FF>Buildings ► ESTIA1\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 180f, 0f));
	}
	// 4.
	void displayMarchProductionBuildingByBuilding(){
		        string[] xTimes = new string[]{"Mar. 2021", "Mar. 2021", "Mar. 2021", "Mar. 2021", "Mar. 2021"};
        string[] xSpaces = new string[]{"ESTIA4", "ESTIA5", "ESTIA1", "ESTIA2", "ESTIA3"};
        sbyte[] yData = new sbyte[]{12, 5, 30, 18, 25};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i];
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		/*string legend = "<line-height=1em><voffset=-8><size=32><color=\"green\">•</voffset><size=8><color=\"black\">energy production\n"
		+ "<voffset=-8><size=32><color=\"orange\">•</voffset><size=8><color=\"black\">energy storage\n"
		+ "<voffset=-8><size=32><color=\"blue\">•</voffset><size=8><color=\"black\">energy consumption\n";*/
		string labels = "<size=7><pos=3%>Past building <pos=85%>Next building\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFAA>■ <color=#000000AA>Energy Consumption<pos=30%><alpha=#AA>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900AA>■ <color=#000000AA>Energy Storage<pos=30%><alpha=#FF>Space<pos=50%><alpha=#AA>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00FF>■ <color=#000000FF>Energy Production<pos=50%><alpha=#FF>Months ► Mar. 2021<pos=75%><alpha=#FF>Buildings\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 180f, 0f));
	}
	// 5.
	void displayESTIA3ProductionMonthByMonth(){
        string[] xTimes = new string[]{"Dec. 2020", "Jan. 2021", "Feb. 2021", "Apr. 2021", "Mar. 2021"};
        string[] xSpaces = new string[]{"ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3"};
        sbyte[] yData = new sbyte[]{19, 15, 17, 22, 25};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i];
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		
		string labels = "<size=7><pos=3%>Past month <pos=85%>Next month\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFAA>■ <color=#000000AA>Energy Consumption<pos=30%><alpha=#FF>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900AA>■ <color=#000000AA>Energy Storage<pos=30%><alpha=#AA>Space<pos=50%><alpha=#AA>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00FF>■ <color=#000000FF>Energy Production<pos=50%><alpha=#FF>Months<pos=75%><alpha=#FF>Buildings ► ESTIA3\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 180f, 0f));
	}
	
	// 6.
	void displayESTIA3ProductionPastOneMonth(){
        string[] xTimes = new string[]{"Nov. 2020", "Dec. 2020", "Jan. 2021", "Feb. 2021", "Mar. 2021"};
        string[] xSpaces = new string[]{"ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3"};
        sbyte[] yData = new sbyte[]{25, 19, 15, 17, 22};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i];
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		
		string labels = "<size=7><pos=3%>Past month <pos=85%>Next month\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFAA>■ <color=#000000AA>Energy Consumption<pos=30%><alpha=#FF>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900AA>■ <color=#000000AA>Energy Storage<pos=30%><alpha=#AA>Space<pos=50%><alpha=#AA>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00FF>■ <color=#000000FF>Energy Production<pos=50%><alpha=#FF>Months<pos=75%><alpha=#FF>Buildings ► ESTIA3\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 180f, 0f));
	}
	// 7.
	void displayESTIA3ProductionPastTwoMonths(){
        string[] xTimes = new string[]{"Oct. 2020", "Nov. 2020", "Dec. 2020", "Jan. 2021", "Feb. 2021"};
        string[] xSpaces = new string[]{"ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3"};
        sbyte[] yData = new sbyte[]{26, 25, 19, 15, 17};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i];
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		
		string labels = "<size=7><pos=3%>Past month <pos=85%>Next month\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFAA>■ <color=#000000AA>Energy Consumption<pos=30%><alpha=#FF>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900AA>■ <color=#000000AA>Energy Storage<pos=30%><alpha=#AA>Space<pos=50%><alpha=#AA>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00FF>■ <color=#000000FF>Energy Production<pos=50%><alpha=#FF>Months<pos=75%><alpha=#FF>Buildings ► ESTIA3\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 180f, 0f));
	}
	// 8.
	void displayESTIA3ProductionJanDayByDay(){
        string[] xTimes = new string[]{"1 Jan. 2021", "2 Jan. 2021", "3 Jan. 2021", "4 Jan. 2021", "5 Jan. 2021"};
        string[] xSpaces = new string[]{"ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3"};
        sbyte[] yData = new sbyte[]{5, 20, 10, 15, 10};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " kWh</b>\n" + xSpaces[i];
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		
		string labels = "<size=7><pos=3%>Past day <pos=85%>Next day\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFAA>■ <color=#000000AA>Energy Consumption<pos=30%><alpha=#FF>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900AA>■ <color=#000000AA>Energy Storage<pos=30%><alpha=#AA>Space<pos=50%><alpha=#FF>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00FF>■ <color=#000000FF>Energy Production<pos=50%><alpha=#AA>Months<pos=75%><alpha=#FF>Buildings ► ESTIA3\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 180f, 0f));
	}
	// 9.
	void subDisplayESTIA3ProductionJanDayByDay(){
    	int SubChartSize = 4;
		int[] SubChartRows = new int[]{1, 1, 1, 1};
		int[] SubChartColumns = new int[]{2, 3, 4, 5};
        string[] xTimes = new string[]{"1 Jan. 2021", "2 Jan. 2021", "3 Jan. 2021", "4 Jan. 2021", "5 Jan. 2021"};
        string[] xSpaces = new string[]{"ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3"};
        sbyte[] yData = new sbyte[]{5, 20, 10, 15, 10};
        for(int i = 0; i < SubChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " kWh</b>\n" + xSpaces[i];
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}
	// 10.
	void displayESTIA3ProductionMonthByMonthLocked(){
		int lockIndex = 1;
        string[] xTimes = new string[]{"Dec. 2020", "Jan. 2021", "Feb. 2021", "Apr. 2021", "Mar. 2021"};
        string[] xSpaces = new string[]{"ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3"};
        sbyte[] yData = new sbyte[]{19, 15, 17, 22, 25};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yData[i]/40f;
			if(i == lockIndex) expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = "▼\n" + xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i] + "\n▲";
           	else expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i];
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

		string labels = "<size=7><pos=3%>Past month <pos=85%>Next month\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFAA>■ <color=#000000AA>Energy Consumption<pos=30%><alpha=#FF>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900AA>■ <color=#000000AA>Energy Storage<pos=30%><alpha=#AA>Space<pos=50%><alpha=#AA>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00FF>■ <color=#000000FF>Energy Production<pos=50%><alpha=#FF>Months<pos=75%><alpha=#FF>Buildings ► ESTIA3\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 180f, 0f));
	}
	// 11.
	void displayESTIA3ProductionMonthByMonthAfterLock(){
		int lockIndex = 1;
        string[] xTimes = new string[]{"Dec. 2020", "Jan. 2021", "Jan. 2021", "Feb. 2021", "Apr. 2021"};
        string[] xSpaces = new string[]{"ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3"};
        sbyte[] yData = new sbyte[]{19, 15, 15, 17, 22};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yData[i]/40f;
			if(i == lockIndex) expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = "▼\n" + xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i] + "\n▲";
           	else expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i];
            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		
		string labels = "<size=7><pos=3%>Past month <pos=85%>Next month\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFAA>■ <color=#000000AA>Energy Consumption<pos=30%><alpha=#FF>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900AA>■ <color=#000000AA>Energy Storage<pos=30%><alpha=#AA>Space<pos=50%><alpha=#AA>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00FF>■ <color=#000000FF>Energy Production<pos=50%><alpha=#FF>Months<pos=75%><alpha=#FF>Buildings ► ESTIA3\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 180f, 0f));
	}
	// 12.
	void displayESTIA3ConsumptionMonthByMonthAfterLock(){
		
		int lockIndex = 1;
        string[] xTimes = new string[]{"Dec. 2020", "Jan. 2021", "Jan. 2021", "Feb. 2021", "Apr. 2021"};
        string[] xSpaces = new string[]{"ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3"};
        sbyte[] yData = new sbyte[]{32, 15, 17, 22, 27};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yData[i]/40f;
			if(i == lockIndex){
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = "▼\n" + xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i] + "\n▲";
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
			} else {
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i];
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
			}
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		string labels = "<size=7><pos=3%>Past month <pos=85%>Next month\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFFF>■ <color=#000000FF>Energy Consumption<pos=30%><alpha=#FF>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900AA>■ <color=#000000AA>Energy Storage<pos=30%><alpha=#AA>Space<pos=50%><alpha=#AA>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00AA>■ <color=#000000AA>Energy Production<pos=50%><alpha=#FF>Months<pos=75%><alpha=#FF>Buildings ► ESTIA3\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 180f, 0f));

	}
	// 13. 
	void subDisplayESTIA3ConsumptionJanDayByDay(){
    	int SubChartSize = 4;
		int[] SubChartRows = new int[]{2, 2, 2, 2};
		int[] SubChartColumns = new int[]{2, 3, 4, 5};
        string[] xTimes = new string[]{"1 Jan. 2021", "2 Jan. 2021", "3 Jan. 2021", "4 Jan. 2021", "5 Jan. 2021"};
        string[] xSpaces = new string[]{"ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3", "ESTIA3"};
        sbyte[] yData = new sbyte[]{6, 25, 15, 5, 5};
        for(int i = 0; i < SubChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " kWh</b>\n" + xSpaces[i];
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	// 14.
	void subDisplayESTIA3ProductionJanRoomByRoom(){
    	int SubChartSize = 4;
		int[] SubChartRows = new int[]{1, 1, 1, 1};
		int[] SubChartColumns = new int[]{2, 3, 4, 5};
        string[] xTimes = new string[]{"Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021"};
        string[] xSpaces = new string[]{"Hall", "Amphitheatre", "Classrooms", "FabLab", "Cafeteria"};
        sbyte[] yData = new sbyte[]{10, 5, 10, 20, 15};
        for(int i = 0; i < SubChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " kWh</b>\n" + xSpaces[i];
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	// 15.
	void displayAndSubDisplayConsumptionJanBuildingByBuilding(){
		int lockIndex = 1;
        string[] xTimes = new string[]{"Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021"};
        string[] xSpaces = new string[]{"ESTIA2", "ESTIA3", "ESTIA3", "ESTIA4", "ESTIA5"};
        sbyte[] yData = new sbyte[]{28, 15, 17, 22, 12};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yData[i]/40f;
			if(i == lockIndex){
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = "▼\n" + xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i] + "\n▲";
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
			} else {
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i];
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
			}
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }

		string labels = "<size=7><pos=3%>Past building <pos=85%>Next building\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFFF>■ <color=#000000FF>Energy Consumption<pos=30%><alpha=#AA>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900AA>■ <color=#000000AA>Energy Storage<pos=30%><alpha=#FF>Space<pos=50%><alpha=#AA>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00AA>■ <color=#000000AA>Energy Production<pos=50%><alpha=#FF>Months ► Jan. 2021<pos=75%><alpha=#FF>Buildings\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 180f, 0f));

    	int SubChartSize = 4;
		int[] SubChartRows = new int[]{2, 2, 2, 2};
		int[] SubChartColumns = new int[]{2, 3, 4, 5};
        xTimes = new string[]{"Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021"};
        xSpaces = new string[]{"Hall", "Amphitheatre", "Classrooms", "FabLab", "Cafeteria"};
        yData = new sbyte[]{5, 8, 15, 6, 25};
        for(int i = 0; i < SubChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " kWh</b>\n" + xSpaces[i];
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}
	
	// 16.
	void displayAndSubDisplayConsumptionJanBuildingByBuildingAfterOneBuilding(){
		int lockIndex = 1;
        string[] xTimes = new string[]{"Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021"};
        string[] xSpaces = new string[]{"ESTIA1", "ESTIA3", "ESTIA2", "ESTIA3", "ESTIA4"};
        sbyte[] yData = new sbyte[]{5, 15, 28, 17, 22};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yData[i]/40f;
			if(i == lockIndex){
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = "▼\n" + xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i] + "\n▲";
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
			} else {
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i];
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
			}
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
		
		string labels = "<size=7><pos=3%>Past building <pos=85%>Next building\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFFF>■ <color=#000000FF>Energy Consumption<pos=30%><alpha=#AA>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900AA>■ <color=#000000AA>Energy Storage<pos=30%><alpha=#FF>Space<pos=50%><alpha=#AA>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00AA>■ <color=#000000AA>Energy Production<pos=50%><alpha=#FF>Months ► Jan. 2021<pos=75%><alpha=#FF>Buildings\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 180f, 0f));

		// hide subchart
    	int SubChartSize = 4;
		int[] SubChartRows = new int[]{2, 2, 2, 2};
		int[] SubChartColumns = new int[]{2, 3, 4, 5};
		for(int i = 0; i < SubChartSize; i++){
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetText = "";
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetColor = new Color(1f, 1f, 1f);
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetPosition = 0;
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
    	SubChartSize = 4;
		SubChartRows = new int[]{3, 3, 3, 3};
		SubChartColumns = new int[]{2, 3, 4, 5};
        xTimes = new string[]{"Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021"};
        xSpaces = new string[]{"Hall", "Amphitheatre", "Classrooms", "FabLab", "Cafeteria"};
        yData = new sbyte[]{5, 8, 15, 6, 25};
        for(int i = 0; i < SubChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " kWh</b>\n" + xSpaces[i];
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetColor = new Color(1f - coeff, 1f - coeff, 1f);
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}


	// 17.
	void hideSubDisplayConsumptionJanBuildingByBuildingAfterOneBuilding(){
		// hide subchart
    	int SubChartSize = 4;
		int [] SubChartRows = new int[]{3, 3, 3, 3};
		int[] SubChartColumns = new int[]{2, 3, 4, 5};
		for(int i = 0; i < SubChartSize; i++){
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetText = "";
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetColor = new Color(1f, 1f, 1f);
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetPosition = 0;
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	// 18.
	void displayStorageJanBuildingByBuilding(){
		int lockIndex = 1;
        string[] xTimes = new string[]{"Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021"};
        string[] xSpaces = new string[]{"ESTIA1", "ESTIA3", "ESTIA2", "ESTIA3", "ESTIA4"};
        sbyte[] yData = new sbyte[]{18, 15, 28, 22, 15};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yData[i]/40f;
			if(i == lockIndex){
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = "▼\n" + xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i] + "\n▲";
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
			} else {
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i];
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f, 1f - (coeff * 0.33f), 1f - coeff);;
			}
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }

		string labels = "<size=7><pos=3%>Past building <pos=85%>Next building\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFAA>■ <color=#000000AA>Energy Consumption<pos=30%><alpha=#AA>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900FF>■ <color=#000000FF>Energy Storage<pos=30%><alpha=#FF>Space<pos=50%><alpha=#AA>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00AA>■ <color=#000000AA>Energy Production<pos=50%><alpha=#FF>Months ► Jan. 2021<pos=75%><alpha=#FF>Buildings\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	// 19.
	void displayProductionJanBuildingByBuilding(){
		int lockIndex = 1;
        string[] xTimes = new string[]{"Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021"};
        string[] xSpaces = new string[]{"ESTIA1", "ESTIA3", "ESTIA2", "ESTIA3", "ESTIA4"};
        sbyte[] yData = new sbyte[]{10, 15, 16, 15, 25};
        for(int i = 0; i < TimeChartSize; i++){
            float coeff = yData[i]/40f;
			if(i == lockIndex){
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = "▼\n" + xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i] + "\n▲";
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
			} else {
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " MWh</b>\n" + xSpaces[i];
				expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);;
			}
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[TimeChartRows[i], TimeChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
		/*string legend = "<line-height=1em><voffset=-8><size=32><color=\"green\">•</voffset><size=8><color=\"black\">energy production\n"
		+ "<voffset=-8><size=32><color=\"orange\">•</voffset><size=8><color=\"black\">energy storage\n"
		+ "<voffset=-8><size=32><color=\"blue\">•</voffset><size=8><color=\"black\">energy consumption\n";*/
		string labels = "<size=7><pos=3%>Past building <pos=85%>Next building\n";
		string direction = "<line-height=10><size=9>◄────────────────────────────────────────────►\n";
		string titles = "<b><line-height=10><size=10><pos=0%>Data Type<pos=30%>Mode<pos=50%>Time Scale<pos=75%>Space Scale</b>\n";
		string firstOption = "<line-height=7><size=7><pos=0%><color=#0000FFAA>■ <color=#000000AA>Energy Consumption<pos=30%><alpha=#AA>Time<pos=50%><alpha=#AA>Hours<pos=75%><alpha=#AA>Devices\n";
		string secondOption = "<line-height=7><size=7><pos=0%><color=#FF9900AA>■ <color=#000000AA>Energy Storage<pos=30%><alpha=#FF>Space<pos=50%><alpha=#AA>Days<pos=75%><alpha=#AA>Rooms\n";
		string thirdOption = "<line-height=7><size=7><pos=0%><color=#00FF00FF>■ <color=#000000FF>Energy Production<pos=50%><alpha=#FF>Months ► Jan. 2021<pos=75%><alpha=#FF>Buildings\n";
		string legend = labels + direction + titles +  firstOption + secondOption + thirdOption;
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.TopLeft, 16, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	// 20.
	void subDisplayESTIA2ProductionJanRoomByRoom(){
    	int SubChartSize = 4;
		int[] SubChartRows = new int[]{2, 2, 2, 2};
		int[] SubChartColumns = new int[]{2, 3, 4, 5};
        string[] xTimes = new string[]{"Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021"};
        string[] xSpaces = new string[]{"Hall", "Amphitheatre", "Classrooms", "FabLab", "Cafeteria"};
        sbyte[] yData = new sbyte[]{8, 7, 13, 18, 0};
        for(int i = 0; i < SubChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " kWh</b>\n" + xSpaces[i];
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

	}

	void subDisplayESTIA2ESTIA3ProductionJanRoomByRoomByOneRoom(){
		
    	int SubChartSize = 4;
		int[] SubChartRows = new int[]{1, 1, 1, 1};
		int[] SubChartColumns = new int[]{2, 3, 4, 5};
        string[] xTimes = new string[]{"Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021"};
        string[] xSpaces = new string[]{"Cafeteria", "Hall", "Amphitheatre", "Classrooms", "FabLab"};
        sbyte[] yData = new sbyte[]{15, 10, 5, 10, 20};
        for(int i = 0; i < SubChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " kWh</b>\n" + xSpaces[i];
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetShapeChangeDuration = 1f;
        };

    	SubChartSize = 4;
		SubChartRows = new int[]{2, 2, 2, 2};
		SubChartColumns = new int[]{2, 3, 4, 5};
        xTimes = new string[]{"Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021", "Jan. 2021"};
        xSpaces = new string[]{"Cafeteria", "Hall", "Amphitheatre", "Classrooms", "FabLab"};
        yData = new sbyte[]{15, 8, 7, 13, 18};
        for(int i = 0; i < SubChartSize; i++){
            float coeff = yData[i]/40f;
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetText = xTimes[i] + "\n<b>" + yData[i] + " kWh</b>\n" + xSpaces[i];
            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetColor = new Color(1f - coeff, 1f, 1f - coeff);
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetTextureChangeDuration = 1f;

            expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetPosition = yData[i];
		    expanDialSticks[SubChartRows[i], SubChartColumns[i]].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

	}

	void ClearAllChange(){
        for(int i =0; i < expanDialSticks.NbRows; i++){
            for(int j =0; j < expanDialSticks.NbColumns; j++){
                expanDialSticks[i, j].TargetTextAlignment = TextAlignmentOptions.Center;
                expanDialSticks[i, j].TargetTextSize = 2f;
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
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 8, Color.black, nextInput, new Vector3(90f, -90f, 0f));
                           
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
							expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 8, Color.black, nextInput, new Vector3(90f, -90f, 0f));
                            if(nextInput.EndsWith(NONE)) users.Enqueue(nextInput);
                        } else {
							expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, 8, Color.black, "", new Vector3(90f, -90f, 0f));
						}
                        return;
					}
				}
			}
        }
    }
}
