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


public class AidedDesignScenario : MonoBehaviour
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
	private const int LEGEND_SIZE = 16;


    private const string CONTROL_TEXT = "<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Outline\"><size=10><color=\"black\"><voffset=0em>•";

	private ConcurrentQueue<string> users;
	private ConcurrentQueue<string> inputs;
	private ConcurrentQueue<Action> outputs;

	void EnqueueIO() 
	{
        
        // Sketching Rack
		inputs.Enqueue("4|0|" + LEFT_BENDING);
		outputs.Enqueue( () => selectRackLinePins());
		inputs.Enqueue("3|1|" + CLICK);
		outputs.Enqueue( () => selectAllRackPins());
		inputs.Enqueue("2|2|" + CLICK); // First Click
		outputs.Enqueue( () => selectAllRackPins());
		inputs.Enqueue("2|2|" + CLICK); // Second Click
		outputs.Enqueue( () => controlAllRackPins());
		inputs.Enqueue("2|2|" + LEFT_ROTATION);
		outputs.Enqueue( () => rotateAllRackPins());
		inputs.Enqueue("2|2|" + RIGHT_BENDING);
		outputs.Enqueue( () => translateAllRackPins());
		inputs.Enqueue("2|2|" + PULL);
		outputs.Enqueue( () => extrudeAllRackPins());
		inputs.Enqueue("2|2|" + PUSH); // First Click
		outputs.Enqueue( () => extrudeAllRackPins());
		inputs.Enqueue("2|2|" + PUSH); // Second Click
		outputs.Enqueue( () => validAllRackPins());

        // Sketching Gear
		inputs.Enqueue("3|0|" + CLICK);
		outputs.Enqueue( () => selectTwoCornerGearPins());
		inputs.Enqueue("3|2|" + CLICK);
		outputs.Enqueue( () => selectFourCornerGearPins());
		inputs.Enqueue("2|1|" + CLICK); 
		outputs.Enqueue( () => selectOneCenterGearPins());
		inputs.Enqueue("3|2|" + CLICK); // First Click
		outputs.Enqueue( () => selectOneCenterGearPins());
		inputs.Enqueue("3|2|" + CLICK); // Second Click
		outputs.Enqueue( () => controlScaleFirstAllGearPins());
		inputs.Enqueue("1|0|" + CLICK); // First Click
		outputs.Enqueue( () => controlScaleFirstAllGearPins());
		inputs.Enqueue("1|0|" + CLICK); // Second Click
		outputs.Enqueue( () => controlScaleSecondAllGearPins());
		inputs.Enqueue("3|2|" + RIGHT_BENDING);
		outputs.Enqueue( () => scaleUpAllGearPins());
		inputs.Enqueue("3|2|" + LEFT_BENDING);
		outputs.Enqueue( () => scaleDownAllGearPins());
		inputs.Enqueue("1|0|" + PUSH); // First Click
		outputs.Enqueue( () => scaleDownAllGearPins());
		inputs.Enqueue("1|0|" + PUSH); // Second Click
		outputs.Enqueue( () => unControlFirstGearPins());
		inputs.Enqueue("3|2|" + PUSH); // First Click
		outputs.Enqueue( () => unControlFirstGearPins());
		inputs.Enqueue("3|2|" + PUSH); // Second Click
		outputs.Enqueue( () => unControlSecondGearPins());
        
		inputs.Enqueue("4|1|" + CLICK); // First Click
		outputs.Enqueue( () => unControlSecondGearPins());
		inputs.Enqueue("4|1|" + CLICK); // Second Click
		outputs.Enqueue( () => controlAllGearPins());
		inputs.Enqueue("2|1|" + CLICK); // First Click
		outputs.Enqueue( () => controlAllGearPins());
		inputs.Enqueue("2|1|" + CLICK); // Second Click
		outputs.Enqueue( () => anchorAllGearPins());
		inputs.Enqueue("4|1|" + PULL);
		outputs.Enqueue( () => extrudeAllGearPins());
		inputs.Enqueue("4|1|" + RIGHT_ROTATION);
		outputs.Enqueue( () => rotateAllGearPins());
		inputs.Enqueue("4|1|" + RIGHT_BENDING);
		outputs.Enqueue( () => translateFirstAllGearPins());
		inputs.Enqueue("4|1|" + RIGHT_BENDING);
		outputs.Enqueue( () => translateSecondAllGearPins());
		inputs.Enqueue("2|3|" + PUSH); // First Click
		outputs.Enqueue( () => translateSecondAllGearPins());
		inputs.Enqueue("2|3|" + PUSH); // Second Click
		outputs.Enqueue( () => unAnchorAllGearPins());
		inputs.Enqueue("4|1|" + PUSH); // First Click
		outputs.Enqueue( () => unAnchorAllGearPins());
		inputs.Enqueue("4|1|" + PUSH); // Second Click
		outputs.Enqueue( () => validAllGearPins());

        // Animate Gear & Rack
		inputs.Enqueue("2|3|" + PUSH); // First Click
		outputs.Enqueue( () => validAllGearPins());
		inputs.Enqueue("2|3|" + PUSH); // Second Click
		outputs.Enqueue( () => controlGearPart());
		inputs.Enqueue("2|3|" + LEFT_ROTATION);
		outputs.Enqueue( () => rotateGearPart());
		inputs.Enqueue("2|3|" + LEFT_ROTATION);
		outputs.Enqueue( () => rotateAgainGearPart());
        
        // Sketch Second Part
		inputs.Enqueue("3|5|" + CLICK);
		outputs.Enqueue( () => selectSecondRackPins());

		inputs.Enqueue("4|5|" + CLICK); // First Click
		outputs.Enqueue( () => selectSecondRackPins());

		inputs.Enqueue("4|5|" + CLICK); // Second Click
		outputs.Enqueue( () => controlSecondRackPins());

	    inputs.Enqueue("4|5|" + PULL);
		outputs.Enqueue( () => extrudeSecondRackPins());
		inputs.Enqueue("4|5|" + PUSH); // First Click
		outputs.Enqueue( () => extrudeSecondRackPins());
		inputs.Enqueue("4|5|"+ PUSH);  // Second Click
		outputs.Enqueue( () => validSecondRackPins());
        
        // Merge two parts
		inputs.Enqueue("4|5|"+ PUSH);  // First Click
		outputs.Enqueue( () => validSecondRackPins());
		inputs.Enqueue("4|5|" + PUSH); // Second Click
		outputs.Enqueue( () => controlFirstSecondRackParts());
		inputs.Enqueue("4|5|" + TOP_BENDING);
		outputs.Enqueue( () => mergeFirstAndSecondRackParts());

        // Animate
		inputs.Enqueue("2|3|" + RIGHT_ROTATION);
		outputs.Enqueue( () => rotateRightGearPart());
		inputs.Enqueue("2|3|" + RIGHT_ROTATION);
		outputs.Enqueue( () => rotateRightAgainGearPart());
        inputs.Enqueue("2|3|" + RIGHT_ROTATION);
		outputs.Enqueue( () => rotateRightAgainAgainGearPart());
        inputs.Enqueue("2|3|" + RIGHT_ROTATION);
		outputs.Enqueue( () => rotateRightAgainAgainAgainGearPart());

		inputs.Enqueue("4|0|" + CLICK);
		outputs.Enqueue( () => Reset());


	}

	void DequeueOutput(){
		Action action;
		while (!outputs.TryDequeue(out action));
		action();
	}
    // Rover Control
    void selectRackLinePins(){
        for (int j = 0; j < 5; j++){
            expanDialSticks[4, j].TargetText = "";
            expanDialSticks[4, j].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[4, j].TargetTextSize = 2f;
            expanDialSticks[4, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[4, j].TargetPosition = 0;
            expanDialSticks[4, j].TargetShapeChangeDuration = 1f;
        }
        
		string legend = "...or click pins to select them.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
        
    }

    void selectAllRackPins(){
        
        expanDialSticks[3, 1].TargetText = "";
        expanDialSticks[3, 1].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 1].TargetTextSize = 2f;
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 1].TargetPosition = 0;
        expanDialSticks[3, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 3].TargetTextSize = 2f;
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 0;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

		string legend = "Double-click a pin to activate a controller.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void controlAllRackPins(){
        expanDialSticks[2, 2].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 2].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[2, 2].TargetTextSize = 2f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        
		string legend = "Rotate the controller to rotate the selection.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


    void rotateAllRackPins(){
        ClearAllChange();
        // Pitch/Roll/Yaw/Throlle/On/Pull/push
        // Button on/off
        for (int i = 0; i < 5; i++){
            expanDialSticks[i, 4].TargetText = "";
            expanDialSticks[i, 4].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[i, 4].TargetTextSize = 2f;
            expanDialSticks[i, 4].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 4].TargetPosition = 0;
        }
        
        expanDialSticks[1, 3].TargetText = "";
        expanDialSticks[1, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 3].TargetTextSize = 2f;
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 0;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 3].TargetTextSize = 2f;
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 0;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 2].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[2, 2].TargetTextSize = 2f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

		string legend = "Bend the controller to translate the selection.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
    
    void translateAllRackPins(){
        ClearAllChange();
        // Pitch/Roll/Yaw/Throlle/On/Pull/push
        // Button on/off
        for (int i = 0; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 0;
        }
        
        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 4].TargetTextSize = 2f;
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 0;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 0;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 2].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[2, 2].TargetTextSize = 2f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

		string legend = "Pull the controller to extrude the selection.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void extrudeAllRackPins(){
        ClearAllChange();
        // Pitch/Roll/Yaw/Throlle/On/Pull/push
        // Button on/off
        for (int i = 0; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
        }
        
        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 4].TargetTextSize = 2f;
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 2].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[2, 2].TargetTextSize = 2f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 15;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

		string legend = "Double-click the controller to validate the part.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
    
    void validAllRackPins(){
        ClearAllChange();
        // Pitch/Roll/Yaw/Throlle/On/Pull/push
        // Button on/off
        for (int i = 0; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
        }
        
        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[1, 4].TargetTextSize = 2f;
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void selectTwoCornerGearPins(){

        expanDialSticks[3, 0].TargetText = "";
        expanDialSticks[3, 0].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 0].TargetTextSize = 2f;
        expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 0].TargetPosition = 0;
        expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 2].TargetTextSize = 2f;
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 0;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

    }

    void selectFourCornerGearPins(){
        
        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 0].TargetTextSize = 2f;
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 0;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 2].TargetTextSize = 2f;
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 0;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;


		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

    }
    void selectOneCenterGearPins(){

        expanDialSticks[2, 1].TargetText = "";
        expanDialSticks[2, 1].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 1].TargetTextSize = 2f;
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 0;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));
        

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
    void controlScaleFirstAllGearPins(){

        expanDialSticks[3, 2].TargetText = CONTROL_TEXT;
        expanDialSticks[3, 2].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[3, 2].TargetTextSize = 2f;
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 10;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

		string legend = "Double-click another pin to activate a second controller.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


    void controlScaleSecondAllGearPins(){
        
        expanDialSticks[1, 0].TargetText = CONTROL_TEXT;
        expanDialSticks[1, 0].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[1, 0].TargetTextSize = 2f;
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 10;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;
        


		string legend = "Bend both controllers outside to scale up the selection.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void scaleUpAllGearPins(){

        expanDialSticks[4, 3].TargetText = "";
        expanDialSticks[4, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[4, 3].TargetTextSize = 2f;
        expanDialSticks[4, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 3].TargetPosition = 0;
        expanDialSticks[4, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 3].TargetText = "";
        expanDialSticks[0, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[0, 3].TargetTextSize = 2f;
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 0;
        expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 1].TargetText = "";
        expanDialSticks[3, 1].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 1].TargetTextSize = 2f;
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 1].TargetPosition = 0;
        expanDialSticks[3, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 1].TargetTextSize = 2f;
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 0;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 0].TargetText = "";
        expanDialSticks[2, 0].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 0].TargetTextSize = 2f;
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 0;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 2].TargetTextSize = 2f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 0;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

		string legend = "Bend both controllers inside to scale down the selection.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void scaleDownAllGearPins(){

        expanDialSticks[4, 3].TargetText = "";
        expanDialSticks[4, 3].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[4, 3].TargetTextSize = 2f;
        expanDialSticks[4, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 3].TargetPosition = 0;
        expanDialSticks[4, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 3].TargetText = "";
        expanDialSticks[0, 3].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[0, 3].TargetTextSize = 2f;
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 0;
        expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 1].TargetText = "";
        expanDialSticks[3, 1].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[3, 1].TargetTextSize = 2f;
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 1].TargetPosition = 0;
        expanDialSticks[3, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[1, 1].TargetTextSize = 2f;
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 0;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 0].TargetText = "";
        expanDialSticks[2, 0].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[2, 0].TargetTextSize = 2f;
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 0;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[2, 2].TargetTextSize = 2f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 0;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        
        expanDialSticks[1, 0].TargetText = CONTROL_TEXT;
        expanDialSticks[1, 0].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[1, 0].TargetTextSize = 2f;
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 10;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


    void unControlFirstGearPins(){
        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 0].TargetTextSize = 2f;
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 0;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 2].TargetText = CONTROL_TEXT;
        expanDialSticks[3, 2].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[3, 2].TargetTextSize = 2f;
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 10;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;


		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


    void unControlSecondGearPins(){
        
        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 2].TargetTextSize = 2f;
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 0;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));
        
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void controlAllGearPins(){
        expanDialSticks[4, 1].TargetText = CONTROL_TEXT;
        expanDialSticks[4, 1].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[4, 1].TargetTextSize = 2f;
        expanDialSticks[4, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 1].TargetPosition = 10;
        expanDialSticks[4, 1].TargetShapeChangeDuration = 1f;
        
		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void anchorAllGearPins(){
        expanDialSticks[2, 1].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 1].TargetColor =  new Color(0f, 0.66f, 0f);
        expanDialSticks[2, 1].TargetTextSize = 2f;
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 10;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

		string legend = "Use the second controller as an anchor.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


    void extrudeAllGearPins(){
        ClearAllChange();
        
        for (int i = 0; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
        }
        
        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[1, 4].TargetTextSize = 2f;
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 0].TargetText = "";
        expanDialSticks[3, 0].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 0].TargetTextSize = 2f;
        expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 0].TargetPosition = 5;
        expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 0].TargetTextSize = 2f;
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 10;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 1].TargetColor =  new Color(0f, 0.66f, 0f);
        expanDialSticks[2, 1].TargetTextSize = 2f;
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 20;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 2].TargetTextSize = 2f;
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 10;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 2].TargetTextSize = 2f;
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 10;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 1].TargetText = CONTROL_TEXT;
        expanDialSticks[4, 1].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[4, 1].TargetTextSize = 2f;
        expanDialSticks[4, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 1].TargetPosition = 20;
        expanDialSticks[4, 1].TargetShapeChangeDuration = 1f;

		string legend = "Use the second controller as an anchor.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void rotateAllGearPins(){
        ClearAllChange();
        
        for (int i = 0; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
        }
        
        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[1, 4].TargetTextSize = 2f;
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        // Gear
        expanDialSticks[3, 1].TargetText = "";
        expanDialSticks[3, 1].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 1].TargetTextSize = 2f;
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 1].TargetPosition = 10;
        expanDialSticks[3, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 1].TargetTextSize = 2f;
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 10;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 1].TargetColor =  new Color(0f, 0.66f, 0f);
        expanDialSticks[2, 1].TargetTextSize = 2f;
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 20;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 0].TargetText = "";
        expanDialSticks[2, 0].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 0].TargetTextSize = 2f;
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 10;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 2].TargetTextSize = 2f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 1].TargetText = CONTROL_TEXT;
        expanDialSticks[4, 1].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[4, 1].TargetTextSize = 2f;
        expanDialSticks[4, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 1].TargetPosition = 20;
        expanDialSticks[4, 1].TargetShapeChangeDuration = 1f;

		string legend = "Use the second controller as an anchor.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void translateFirstAllGearPins(){
        ClearAllChange();
        
        for (int i = 0; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
        }
        
        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[1, 4].TargetTextSize = 2f;
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        // Gear
        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 2].TargetTextSize = 2f;
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 10;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 2].TargetTextSize = 2f;
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 10;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 2].TargetColor =  new Color(0f, 0.66f, 0f);
        expanDialSticks[2, 2].TargetTextSize = 2f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 20;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 1].TargetText = "";
        expanDialSticks[2, 1].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 1].TargetTextSize = 2f;
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 10;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = "";
        expanDialSticks[2, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 3].TargetTextSize = 2f;
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 10;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 1].TargetText = CONTROL_TEXT;
        expanDialSticks[4, 1].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[4, 1].TargetTextSize = 2f;
        expanDialSticks[4, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 1].TargetPosition = 20;
        expanDialSticks[4, 1].TargetShapeChangeDuration = 1f;

		string legend = "Use the second controller as an anchor.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
    

    void translateSecondAllGearPins(){
        ClearAllChange();
        
        for (int i = 0; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
        }
        
        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[1, 4].TargetTextSize = 2f;
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        // Gear
        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 3].TargetTextSize = 2f;
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 10;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetText = "";
        expanDialSticks[1, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 3].TargetTextSize = 2f;
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 10;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 3].TargetColor =  new Color(0f, 0.66f, 0f);
        expanDialSticks[2, 3].TargetTextSize = 2f;
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 20;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 2].TargetTextSize = 2f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 4].TargetTextSize = 2f;
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 10;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 1].TargetText = CONTROL_TEXT;
        expanDialSticks[4, 1].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[4, 1].TargetTextSize = 2f;
        expanDialSticks[4, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 1].TargetPosition = 20;
        expanDialSticks[4, 1].TargetShapeChangeDuration = 1f;


		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    
    void unAnchorAllGearPins(){
        ClearAllChange();
        
        for (int i = 0; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
        }
        
        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[1, 4].TargetTextSize = 2f;
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        // Gear
        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 3].TargetTextSize = 2f;
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 10;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetText = "";
        expanDialSticks[1, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 3].TargetTextSize = 2f;
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 10;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = "";
        expanDialSticks[2, 3].TargetColor =  new Color(0f, 1f, 0f);
        expanDialSticks[2, 3].TargetTextSize = 2f;
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 10;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 2].TargetTextSize = 2f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 4].TargetTextSize = 2f;
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 10;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 1].TargetText = CONTROL_TEXT;
        expanDialSticks[4, 1].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[4, 1].TargetTextSize = 2f;
        expanDialSticks[4, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 1].TargetPosition = 20;
        expanDialSticks[4, 1].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

     void validAllGearPins(){
        ClearAllChange();
        
        for (int i = 0; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
        }
        
        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[1, 4].TargetTextSize = 2f;
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        // Gear
        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[3, 3].TargetTextSize = 2f;
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 10;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetText = "";
        expanDialSticks[1, 3].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[1, 3].TargetTextSize = 2f;
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 10;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = "";
        expanDialSticks[2, 3].TargetColor =  new Color(1f, 1f, 0f);
        expanDialSticks[2, 3].TargetTextSize = 2f;
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 10;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[2, 2].TargetTextSize = 2f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[2, 4].TargetTextSize = 2f;
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 10;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

		string legend = "Animate parts in direct contact.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

     void controlGearPart(){
        ClearAllChange();
        
        for (int i = 0; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
        }
        
        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[1, 4].TargetTextSize = 2f;
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        // Gear
        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[3, 3].TargetTextSize = 2f;
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 10;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetText = "";
        expanDialSticks[1, 3].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[1, 3].TargetTextSize = 2f;
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 10;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 3].TargetColor =  new Color(0.66f, 0.66f, 0f);
        expanDialSticks[2, 3].TargetTextSize = 2f;
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 20;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[2, 2].TargetTextSize = 2f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[2, 4].TargetTextSize = 2f;
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 10;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

		string legend = "Animate parts in direct contact.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
    

     void rotateGearPart(){
        ClearAllChange();
        
        for (int i = 0; i < 4; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
        }
        
        expanDialSticks[0, 4].TargetText = "";
        expanDialSticks[0, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[0, 4].TargetTextSize = 2f;
        expanDialSticks[0, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 4].TargetPosition = 5;
        expanDialSticks[0, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[2, 4].TargetTextSize = 2f;
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 5;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        // Gear
        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[3, 2].TargetTextSize = 2f;
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 10;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 10;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[1, 2].TargetTextSize = 2f;
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 10;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[1, 4].TargetTextSize = 2f;
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 10;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;


        expanDialSticks[2, 3].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 3].TargetColor =  new Color(0.66f, 0.66f, 0f);
        expanDialSticks[2, 3].TargetTextSize = 2f;
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 20;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

		string legend = "Animate parts in direct contact.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


     void rotateAgainGearPart(){
        ClearAllChange();
        
        for (int i = 0; i < 3; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
        }

        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[1, 4].TargetTextSize = 2f;
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        // Gear
        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[3, 3].TargetTextSize = 2f;
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 10;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetText = "";
        expanDialSticks[1, 3].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[1, 3].TargetTextSize = 2f;
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 10;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[2, 2].TargetTextSize = 2f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[2, 4].TargetTextSize = 2f;
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 10;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 3].TargetColor =  new Color(0.66f, 0.66f, 0f);
        expanDialSticks[2, 3].TargetTextSize = 2f;
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 20;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
    

     void selectSecondRackPins(){
        // First Rack
        for (int i = 3; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 0;
            expanDialSticks[i, 5].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 0;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
    

     void controlSecondRackPins(){
        expanDialSticks[4, 5].TargetText = CONTROL_TEXT;
        expanDialSticks[4, 5].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[4, 5].TargetTextSize = 2f;
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 10;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
    

     void extrudeSecondRackPins(){
        // First Rack
        for (int i = 3; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
            expanDialSticks[i, 5].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        
        expanDialSticks[4, 5].TargetText = CONTROL_TEXT;
        expanDialSticks[4, 5].TargetColor = new Color(0f, 0.66f, 0f);
        expanDialSticks[4, 5].TargetTextSize = 2f;
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 15;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
    

     void validSecondRackPins(){
        // First Rack
        for (int i = 3; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0f, 1f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
            expanDialSticks[i, 5].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0f, 1f, 1f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void controlFirstSecondRackParts(){
        expanDialSticks[4, 5].TargetText = CONTROL_TEXT;
        expanDialSticks[4, 5].TargetColor = new Color(0f, 0.66f, 0.66f);
        expanDialSticks[4, 5].TargetTextSize = 2f;
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 15;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[0, 5].TargetText = CONTROL_TEXT;
        expanDialSticks[0, 5].TargetColor = new Color(0.3f, 0.3f, 0.66f);
        expanDialSticks[0, 5].TargetTextSize = 2f;
        expanDialSticks[0, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 5].TargetPosition = 15;
        expanDialSticks[0, 5].TargetShapeChangeDuration = 1f;

		string legend = "Bend both controllers towards each other to merge parts.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void mergeFirstAndSecondRackParts(){
        // First Rack
        for (int i = 3; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
            expanDialSticks[i, 5].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[4, 5].TargetText = "";
        expanDialSticks[4, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[4, 5].TargetTextSize = 2f;
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 5;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[0, 5].TargetText = "";
        expanDialSticks[0, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[0, 5].TargetTextSize = 2f;
        expanDialSticks[0, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 5].TargetPosition = 5;
        expanDialSticks[0, 5].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

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

    void rotateRightGearPart(){
        ClearAllChange();
        
        for (int i = 0; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
        }
        
        expanDialSticks[0, 4].TargetText = "";
        expanDialSticks[0, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[0, 4].TargetTextSize = 2f;
        expanDialSticks[0, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 4].TargetPosition = 5;
        expanDialSticks[0, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[2, 4].TargetTextSize = 2f;
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 5;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 4].TargetText = "";
        expanDialSticks[4, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[4, 4].TargetTextSize = 2f;
        expanDialSticks[4, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 4].TargetPosition = 5;
        expanDialSticks[4, 4].TargetShapeChangeDuration = 1f;

        // Gear
        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[3, 2].TargetTextSize = 2f;
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 10;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 10;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[1, 2].TargetTextSize = 2f;
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 10;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[1, 4].TargetTextSize = 2f;
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 10;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;


        expanDialSticks[2, 3].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 3].TargetColor =  new Color(0.66f, 0.66f, 0f);
        expanDialSticks[2, 3].TargetTextSize = 2f;
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 20;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    
    void rotateRightAgainGearPart(){
        ClearAllChange();
        
        for (int i = 0; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
        }
        
        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[1, 4].TargetTextSize = 2f;
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        // Gear
        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[3, 3].TargetTextSize = 2f;
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 10;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetText = "";
        expanDialSticks[1, 3].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[1, 3].TargetTextSize = 2f;
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 10;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[2, 2].TargetTextSize = 2f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[2, 4].TargetTextSize = 2f;
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 10;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 3].TargetColor =  new Color(0.66f, 0.66f, 0f);
        expanDialSticks[2, 3].TargetTextSize = 2f;
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 20;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void rotateRightAgainAgainGearPart(){
        ClearAllChange();
        
        for (int i = 1; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
        }

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[2, 4].TargetTextSize = 2f;
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 5;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 4].TargetText = "";
        expanDialSticks[4, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[4, 4].TargetTextSize = 2f;
        expanDialSticks[4, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 4].TargetPosition = 5;
        expanDialSticks[4, 4].TargetShapeChangeDuration = 1f;

        // Gear
        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[3, 2].TargetTextSize = 2f;
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 10;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 10;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[1, 2].TargetTextSize = 2f;
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 10;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[1, 4].TargetTextSize = 2f;
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 10;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;


        expanDialSticks[2, 3].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 3].TargetColor =  new Color(0.66f, 0.66f, 0f);
        expanDialSticks[2, 3].TargetTextSize = 2f;
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 20;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    

    void rotateRightAgainAgainAgainGearPart(){
        ClearAllChange();
        
        for (int i = 2; i < 5; i++){
            expanDialSticks[i, 5].TargetText = "";
            expanDialSticks[i, 5].TargetColor = new Color(0.3f, 0.3f, 1f);
            expanDialSticks[i, 5].TargetTextSize = 2f;
            expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 5].TargetPosition = 5;
        }

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0.3f, 0.3f, 1f);
        expanDialSticks[3, 4].TargetTextSize = 2f;
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        // Gear
        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[3, 3].TargetTextSize = 2f;
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 10;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetText = "";
        expanDialSticks[1, 3].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[1, 3].TargetTextSize = 2f;
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 10;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[2, 2].TargetTextSize = 2f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetColor = new Color(1f, 1f, 0f);
        expanDialSticks[2, 4].TargetTextSize = 2f;
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 10;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = CONTROL_TEXT;
        expanDialSticks[2, 3].TargetColor =  new Color(0.66f, 0.66f, 0f);
        expanDialSticks[2, 3].TargetTextSize = 2f;
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 20;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void Reset(){
        ClearAllChange();
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


		outputs = new ConcurrentQueue<Action>();
		inputs = new ConcurrentQueue<string>();
		users = new ConcurrentQueue<string>();
		EnqueueIO();

		// Connection to MQTT Broker
		connected = false;
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
                           
		string legend = "Bend pins to select them...";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

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
		Debug.Log("HandlePositionChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
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
