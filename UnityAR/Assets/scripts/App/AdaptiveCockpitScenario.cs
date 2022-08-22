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


public class AdaptiveCockpitScenario : MonoBehaviour
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
    
	private const float JOYSTICK_THRESHOLD = 25f;
	private const float LEGEND_SIZE = 0.16f;
    private const float TEXT_SIZE = 2f;
    private const float TRIGGER_DELAY = 6f;
    public float FONT_SIZE_H0 = 0.10f;
    public float LINE_HEIGHT_H0 = 0.12f;
    public float FONT_SIZE_H1 = 0.08f;
    public float LINE_HEIGHT_H1 = 0.10f;
    public float FONT_SIZE_H2 = 0.06f;
    public float LINE_HEIGHT_H2 = 0.08f;
    public float FONT_SIZE_H3 = 0.04f;
    public float LINE_HEIGHT_H3 = 0.06f;
    public float FONT_SIZE_H4 = 0.02f;
    public float LINE_HEIGHT_H4 = 0.04f;
    public float LINE_HEIGHT_MISC1 = 0.6f;
    public float LINE_HEIGHT_MISC2 = 0.4f;
    public float LINE_HEIGHT_MISC3 = 0.2f;

    public Mesh _separationMesh;
    public Material _separationMat;
    private Matrix4x4[] _separationMatrices;
    private Vector4[] _separationColors, _separationFirstOutlineColors, _separationSecondOutlineColors, _separationThirdOutlineColors;
    private float[] _separationFirstOutlineWidths, _separationSecondOutlineWidths, _separationThirdOutlineWidths;
    private int _separationIndex = 0;


    private ConcurrentQueue<string> users;
	private ConcurrentQueue<string> inputs;
	private ConcurrentQueue<Action> outputs;
	private float prevTime;
    private bool inited = false;
    private bool triggered = false;
    private bool started = false;


    void OnGUI()
    {
        // Make a text field that modifies stringToEdit.
        float midX = Screen.width / 2.0f;
        float midY = Screen.height / 2.0f;
        float componentHeight = 20;
        float componentWidth = 250;

        if (connected && !started)
        {

            if (GUI.Button(new Rect(midX + 5, midY - 50, componentWidth, componentHeight), "UNSAFE"))
            {
                expanDialSticks.SetOverlayMode(ExpanDialSticks.SafetyOverlayMode.None);
                expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.None);
                started = true;
            }

            if (GUI.Button(new Rect(midX + 5, midY - 25, componentWidth, componentHeight), "INCOHERENT"))
            {
                expanDialSticks.SetOverlayMode(ExpanDialSticks.SafetyOverlayMode.None);
                expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.SpeedAndSeparationMonitoring);
                started = true;
            }

            if (GUI.Button(new Rect(midX + 5, midY, componentWidth, componentHeight), "ALL"))
            {
                expanDialSticks.SetOverlayMode(ExpanDialSticks.SafetyOverlayMode.User);
                expanDialSticks.SetSafetyMode(ExpanDialSticks.SafetyMotionMode.SpeedAndSeparationMonitoring);
                started = true;
            }

        }

    }

    void EnqueueIO() 
	{
        /*
        // Rover Single
        inputs.Enqueue("4|4|" + CLICK);
        outputs.Enqueue(() => displayRoverControlSingle());
        // One DiaStick + One Throttle
        inputs.Enqueue("4|4|" + CLICK);
        outputs.Enqueue(() => displayDroneControlMode());
        // One DiaStick + One Big Throttle
        inputs.Enqueue("4|4|" + CLICK);
        outputs.Enqueue(() => displayDroneControlBigHand());
        // Reset cockpit
        inputs.Enqueue("4|4|" + CLICK);
        outputs.Enqueue(() => Reset());
        */
        // Rover start
        inputs.Enqueue("4|4|" + CLICK);
        outputs.Enqueue(() => displayDroneControlBigHand());
        // Rover start
        inputs.Enqueue("4|4|" + CLICK);
        outputs.Enqueue(() => displayRoverControlStart());

        // Rover Single
        inputs.Enqueue("4|4|" + CLICK);
        outputs.Enqueue(() => displayRoverControlSingle());

        // Rover Dual
        inputs.Enqueue("4|4|" + CLICK);
        outputs.Enqueue(() => displayRoverControlDual());

        // Rover Single
        inputs.Enqueue("4|4|" + CLICK);
        outputs.Enqueue(() => displayRoverControlSingle());

        // Rover stop
        inputs.Enqueue("4|4|" + CLICK);
        outputs.Enqueue(() => displayRoverControlStart());

        /*
        // Rover start
        inputs.Enqueue("4|4|" + CLICK);
		outputs.Enqueue( () => displayRoverControlStart());

        // Rover Single
		inputs.Enqueue("4|4|" + CLICK);
		outputs.Enqueue( () => displayRoverControlSingle());

        // Rover Dual
		inputs.Enqueue("4|4|" + CLICK);
		outputs.Enqueue( () => displayRoverControlDual());

        // Rover stop
		inputs.Enqueue("4|4|" + CLICK);
		outputs.Enqueue( () => displayRoverControlStart());

        // Taking off
		inputs.Enqueue("4|4|" + CLICK);
		outputs.Enqueue( () => displayDroneControlTakeOff());

        // One ExpanDialStick
		inputs.Enqueue("4|4|" + CLICK);
		outputs.Enqueue( () => displayDroneControlDefault());

        // One DiaStick + One Throttle
		inputs.Enqueue("4|4|" + CLICK);
		outputs.Enqueue( () => displayDroneControlMode());

        // One DiaStick + One Big Throttle
		inputs.Enqueue("4|4|" + CLICK);
		outputs.Enqueue( () => displayDroneControlBigHand());

        // One Big Throttle + One DiaStick
		inputs.Enqueue("4|4|" + CLICK);
		outputs.Enqueue( () => displayDroneControlLeftHand());

        // AR to GUI
		inputs.Enqueue("4|4|" + CLICK);
		outputs.Enqueue( () => displayDroneControlGUI());

        // Multi-User (Drone/Rover Collaboration)
		inputs.Enqueue("4|4|" + CLICK);
		outputs.Enqueue( () => displayDroneControlMultiUser());
        
		inputs.Enqueue("4|4|" + CLICK);
		outputs.Enqueue( () => Reset());*/

    }
	void DequeueOutput(){
		Action action;
		while (!outputs.TryDequeue(out action));
		action();
	}
    // Rover Control

    void displayRoverControlStart(){
        ClearAllChange();
        // Pitch/Roll/Yaw/Throlle/On/Pull/push
        // Button on/off
        expanDialSticks.triggerProjectorChange();
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		string legend = "Starting the rover with a button.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));
    }
    
    void displayRoverControlSingle(){
        ClearAllChange();

        // Pitch/Roll/Yaw/Throlle/On/Pull/push
        // Button on/off

        // Single Controller
        expanDialSticks[2, 1].TargetText = "Joystick";
        expanDialSticks[2, 1].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 1].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 40;
		expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        // Constraints
        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 0].TargetTextSize= TEXT_SIZE;
		expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 10;
		expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 2].TargetTextSize= TEXT_SIZE;
		expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 10;
		expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 0].TargetText = "";
        expanDialSticks[3, 0].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 0].TargetTextSize= TEXT_SIZE;
		expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 0].TargetPosition = 10;
		expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 2].TargetTextSize= TEXT_SIZE;
		expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 10;
		expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        // display rover mode menu
        /*expanDialSticks[4, 5].TargetColor = Color.white; //Color.green;
        expanDialSticks[4, 5].TargetProjectorFrontTexture = "rover_mode_1";
        expanDialSticks[4, 5].TargetProjectorFrontRotation = 90f;
        expanDialSticks[4, 5].TargetProjectorFrontSize = 0.02f;
        expanDialSticks[4, 5].TargetProjectorFrontColor = Color.white;
        expanDialSticks[4, 5].TargetProjectorFrontChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 5;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;*/


        /*expanDialSticks[4, 5].TargetText = "Mode 2";
        expanDialSticks[4, 5].TargetColor = new Color(0.66f, 0.66f, 0.66f);
        expanDialSticks[4, 5].TargetTextSize = TEXT_SIZE;
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 5;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;*/

        // Stop button
        /* expanDialSticks[4, 5].TargetText = "Stop";
         expanDialSticks[4, 5].TargetColor = new Color(1f, 0f, 0f);
         expanDialSticks[4, 5].TargetTextSize= TEXT_SIZE;
         expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
         expanDialSticks[4, 5].TargetPosition = 5;
         expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;*/
        expanDialSticks.triggerProjectorChange();
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		string legend = "<b>Rover</b> Control Mode";
		expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, -90f, 0f));
        }

    void displayRoverControlDual(){
        ClearAllChange();

        // First Controller
        expanDialSticks[2, 1].TargetText = "Throttle"; // "▲\n\n▼";
        expanDialSticks[2, 1].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 1].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 40;
		expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        // First Contraints
        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 0].TargetTextSize= TEXT_SIZE;
		expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 10;
		expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 2].TargetTextSize= TEXT_SIZE;
		expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 10;
		expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 0].TargetText = "";
        expanDialSticks[2, 0].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[2, 0].TargetTextSize= TEXT_SIZE;
		expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 10;
		expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[2, 2].TargetTextSize= TEXT_SIZE;
		expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
		expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;
        

        expanDialSticks[3, 0].TargetText = "";
        expanDialSticks[3, 0].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 0].TargetTextSize= TEXT_SIZE;
		expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 0].TargetPosition = 10;
		expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 2].TargetTextSize= TEXT_SIZE;
		expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 10;
		expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;


        // Second Controller
        expanDialSticks[2, 4].TargetText = "◄    ►";
        expanDialSticks[2, 4].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 4].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 40;
		expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;


        // Constraints
        expanDialSticks[1, 3].TargetText = "";
        expanDialSticks[1, 3].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 3].TargetTextSize= TEXT_SIZE;
		expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 10;
		expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 4].TargetTextSize= TEXT_SIZE;
		expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 10;
		expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[1, 5].TargetText = "";
        expanDialSticks[1, 5].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 5].TargetTextSize= TEXT_SIZE;
		expanDialSticks[1, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 5].TargetPosition = 10;
		expanDialSticks[1, 5].TargetShapeChangeDuration = 1f;


        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 3].TargetTextSize= TEXT_SIZE;
		expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 10;
		expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 4].TargetTextSize= TEXT_SIZE;
		expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 10;
		expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[3, 5].TargetText = "";
        expanDialSticks[3, 5].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 5].TargetTextSize= TEXT_SIZE;
		expanDialSticks[3, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 5].TargetPosition = 10;
		expanDialSticks[3, 5].TargetShapeChangeDuration = 1f;


        // Stop button
        /* expanDialSticks[4, 5].TargetText = "Stop";
         expanDialSticks[4, 5].TargetColor = new Color(1f, 0f, 0f);
         expanDialSticks[4, 5].TargetTextSize= TEXT_SIZE;
         expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
         expanDialSticks[4, 5].TargetPosition = 5;
         expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;*/

        // display rover mode menu
        expanDialSticks[4, 5].TargetColor = Color.white; //Color.green;
        expanDialSticks[4, 5].TargetProjectorFrontTexture = "rover_mode_2";
        expanDialSticks[4, 5].TargetProjectorFrontRotation = 90f;
        expanDialSticks[4, 5].TargetProjectorFrontSize = 0.02f;
        expanDialSticks[4, 5].TargetProjectorFrontColor = Color.white;
        expanDialSticks[4, 5].TargetProjectorFrontChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 5;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;

        /*expanDialSticks[4, 5].TargetText = "Mode 2";
        expanDialSticks[4, 5].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[4, 5].TargetTextSize = TEXT_SIZE;
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 5;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;*/
        expanDialSticks.triggerProjectorChange();
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		string legend = "Controlling a rover using two controllers.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));
    }
    

	// Drone Controle Mode 1
	void displayDroneControlTakeOff(){
        ClearAllChange();
        // Pitch/Roll/Yaw/Throlle/On/Pull/push
        // Button on/off
        expanDialSticks[4, 5].TargetText = "Take Off";
        expanDialSticks[4, 5].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[4, 5].TargetTextSize= TEXT_SIZE;
		expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 5;
		expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		string legend = "Taking off the drone with a button.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));
	}
    void displayDroneControlDefault(){
        // Pitch/Roll/Yaw/Throlle/On/Pull/push
        expanDialSticks[2, 3].TargetText = "▲\n◄ ↕ ►\n▼";
        expanDialSticks[2, 3].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 3].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 40;
		expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;
        // Button on/off
        expanDialSticks[4, 5].TargetText = "Landing";
        expanDialSticks[4, 5].TargetColor = new Color(1f, 0f, 0f);
        expanDialSticks[4, 5].TargetTextSize= TEXT_SIZE;
		expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 5;
		expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		string legend = "Controlling a drone using a single controller.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));
	}
    
    void displayDroneControlMode(){
        // Pitch/Roll/Yaw/Throlle/On/Pull/push
        expanDialSticks[2, 3].TargetText = "";
        expanDialSticks[2, 3].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 3].TargetColor = new Color(1f, 1f, 1f);
		expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 0;
		expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetText = "▲\n◄    ►\n▼";
        expanDialSticks[2, 1].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 1].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 40;
		expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "↕";
        expanDialSticks[2, 4].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 4].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 40;
		expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;
        // Constraints Left
        expanDialSticks[1, 3].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 10;
		expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 10;
		expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 3].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 10;
		expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        // Constraints Right
        expanDialSticks[1, 5].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[1, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 5].TargetPosition = 10;
		expanDialSticks[1, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 5].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 10;
		expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[3, 5].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[3, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 5].TargetPosition = 10;
		expanDialSticks[3, 5].TargetShapeChangeDuration = 1f;

        // Remove old constraints

        expanDialSticks[1, 0].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 0;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 0].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 0].TargetPosition = 0;
        expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		string legend = "Controlling a drone using two controllers.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));
	}
    
    
    void displayDroneControlBigHand(){
        // Pitch/Roll/Yaw/Throlle/On/Pull/push


        // Reposition DialStick
        expanDialSticks[2, 1].TargetText = "";
        expanDialSticks[2, 1].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 1].TargetColor = new Color(1f, 1f, 1f);
		expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 0;
		expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 0].TargetText = "Joystick";
        expanDialSticks[2, 0].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 0].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 40;
		expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;
        
        
        // Reposition Throttle

        expanDialSticks[2, 3].TargetText = "Large";
        expanDialSticks[2, 3].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 3].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 40;
		expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "throttle";
        expanDialSticks[2, 4].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 4].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 40;
		expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        // Constraints Left
        // Remove old left constraints
        expanDialSticks[1, 0].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 0;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 0].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 0].TargetPosition = 0;
        expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetColor = new Color(1f, 1f, 1f);
		expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 0;
		expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 3].TargetColor = new Color(1f, 1f, 1f);
		expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 0;
		expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        // Adding new left constraints
        expanDialSticks[1, 2].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 10;
		expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 2].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
		expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[3, 2].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 10;
		expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;
        

        // Constraints Right
        expanDialSticks[1, 5].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[1, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 5].TargetPosition = 10;
		expanDialSticks[1, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 5].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 10;
		expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[3, 5].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[3, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 5].TargetPosition = 10;
		expanDialSticks[3, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		string legend = "<b>Drone</b> Control Mode";
        expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, -90f, 0f));
        //expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		//expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));
	}
    
    void displayDroneControlLeftHand(){
        // Pitch/Roll/Yaw/Throlle/On/Pull/push

        
        ClearAllChange();
        
        // Reposition Landing Button
        expanDialSticks[4, 0].TargetText = "Landing";
        expanDialSticks[4, 0].TargetColor = new Color(1f, 0f, 0f);
        expanDialSticks[4, 0].TargetTextSize= TEXT_SIZE;
		expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 5;
		expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

        // Reposition DialStick
        expanDialSticks[2, 5].TargetText = "▲\n◄    ►\n▼";
        expanDialSticks[2, 5].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 5].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 40;
		expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;

        
        // Reposition Throttle
        expanDialSticks[2, 1].TargetText = "↕";
        expanDialSticks[2, 1].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 1].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 40;
		expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = "↕";
        expanDialSticks[2, 2].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 2].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 40;
		expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        // Constraints Left

        // Adding new left constraints
        expanDialSticks[1, 0].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 10;
		expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 0].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 10;
		expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[3, 0].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 0].TargetPosition = 10;
		expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;
        

        // Constraints Right
        expanDialSticks[1, 3].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 10;
		expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 10;
		expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[3, 3].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 10;
		expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		string legend = "Controlling a drone using a left-handed cockpit.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));
	}

    void displayDroneControlGUI(){
        // Pitch/Roll/Yaw/Throlle/On/Pull/push
        ClearAllChange();
                
        // Reposition Landing Button
        expanDialSticks[4, 0].TargetText = "Landing";
        expanDialSticks[4, 0].TargetColor = new Color(1f, 0f, 0f);
        expanDialSticks[4, 0].TargetTextSize= TEXT_SIZE;
		expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 5;
		expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

        // Reposition DialStick
        expanDialSticks[4, 5].TargetText = "▲\n◄    ►\n▼";
        expanDialSticks[4, 5].TargetTextSize= TEXT_SIZE;
        expanDialSticks[4, 5].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 40;
		expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;

        // Reposition Throttle
        expanDialSticks[4, 1].TargetText = "↕";
        expanDialSticks[4, 1].TargetTextSize= TEXT_SIZE;
        expanDialSticks[4, 1].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[4, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 1].TargetPosition = 40;
		expanDialSticks[4, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 2].TargetText = "↕";
        expanDialSticks[4, 2].TargetTextSize= TEXT_SIZE;
        expanDialSticks[4, 2].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[4, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 2].TargetPosition = 40;
		expanDialSticks[4, 2].TargetShapeChangeDuration = 1f;

        // Constraints Right
        expanDialSticks[4, 3].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[4, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 3].TargetPosition = 10;
		expanDialSticks[4, 3].TargetShapeChangeDuration = 1f;

        // Contraints for smartphones
        for (int j = 1; j < 5; j++)
        {
            expanDialSticks[0, j].TargetColor = new Color(0f, 0f, 0f);
            expanDialSticks[0, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[0, j].TargetPosition = 20;
            expanDialSticks[0, j].TargetShapeChangeDuration = 1f;
            
            expanDialSticks[1, j].TargetColor = new Color(0f, 0f, 0f);
            expanDialSticks[1, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[1, j].TargetPosition = 0;
            expanDialSticks[1, j].TargetShapeChangeDuration = 1f;

           /* expanDialSticks[2, j].TargetColor = new Color(0f, 0f, 0f);
            expanDialSticks[2, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[2, j].TargetPosition = 5;
            expanDialSticks[2, j].TargetShapeChangeDuration = 1f;*/
        }

        expanDialSticks[4, 3].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[4, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 3].TargetPosition = 10;
		expanDialSticks[4, 3].TargetShapeChangeDuration = 1f;



        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		string legend = "Controlling a drone displayed on a flat screen.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));
	}

    void displayDroneControlMultiUser(){
        // Pitch/Roll/Yaw/Throlle/On/Pull/push
        ClearAllChange();
                
        // Reposition Landing Button
        expanDialSticks[4, 0].TargetText = "Landing";
        expanDialSticks[4, 0].TargetColor = new Color(1f, 0f, 0f);
        expanDialSticks[4, 0].TargetTextSize= TEXT_SIZE;
		expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 5;
		expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

        // Reposition DialStick
        expanDialSticks[4, 5].TargetText = "▲\n◄    ►\n▼";
        expanDialSticks[4, 5].TargetTextSize= TEXT_SIZE;
        expanDialSticks[4, 5].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 40;
		expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;

        // Reposition Throttle
        expanDialSticks[4, 1].TargetText = "↕";
        expanDialSticks[4, 1].TargetTextSize= TEXT_SIZE;
        expanDialSticks[4, 1].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[4, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 1].TargetPosition = 40;
		expanDialSticks[4, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 2].TargetText = "↕";
        expanDialSticks[4, 2].TargetTextSize= TEXT_SIZE;
        expanDialSticks[4, 2].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[4, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 2].TargetPosition = 40;
		expanDialSticks[4, 2].TargetShapeChangeDuration = 1f;

        // Constraints Right
        expanDialSticks[4, 3].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[4, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 3].TargetPosition = 10;
		expanDialSticks[4, 3].TargetShapeChangeDuration = 1f;

        // Contraints for smartphones
        for (int j = 1; j < 5; j++)
        {
            expanDialSticks[0, j].TargetColor = new Color(1f, 1f, 1f);
            expanDialSticks[0, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[0, j].TargetPosition = 0;
            expanDialSticks[0, j].TargetShapeChangeDuration = 1f;
            
            expanDialSticks[1, j].TargetColor = new Color(1f, 1f, 1f);
            expanDialSticks[1, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[1, j].TargetPosition = 0;
            expanDialSticks[1, j].TargetShapeChangeDuration = 1f;

           /* expanDialSticks[2, j].TargetColor = new Color(1f, 1f, 1f);
            expanDialSticks[2, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[2, j].TargetPosition = 0;
            expanDialSticks[2, j].TargetShapeChangeDuration = 1f;*/
        }

        // Rover Control
        
        expanDialSticks[1, 4].TargetText = "▲\n◄    ►\n▼";
        expanDialSticks[1, 4].TargetTextSize= TEXT_SIZE;
        expanDialSticks[1, 4].TargetColor = new Color(0f, 1f, 0f);
		expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 40;
		expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        // Add Diagonal Constraints
        expanDialSticks[0, 3].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 10;
		expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 5].TargetText = "Stop";
        expanDialSticks[0, 5].TargetColor = new Color(1f, 0f, 0f);
        expanDialSticks[0, 5].TargetTextSize= TEXT_SIZE;
		expanDialSticks[0, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 5].TargetPosition = 5;
		expanDialSticks[0, 5].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 3].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 10;
		expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 5].TargetColor = new Color(0f, 0f, 0f);
		expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 10;
		expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		string legend = "Two users controlling a drone and rover in parallel.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));
	}

	void displayDroneControlLanding(){

        ClearAllChange();

        expanDialSticks[2, 1].TargetText = "";
        expanDialSticks[2, 1].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 1].TargetColor = new Color(1f, 1f, 1f);
		expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 0;
		expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetTextSize= TEXT_SIZE;
        expanDialSticks[2, 4].TargetColor = new Color(1f, 1f, 1f);
		expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 0;
		expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;


        expanDialSticks[4, 5].TargetText = "Take Off";
        expanDialSticks[4, 5].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[4, 5].TargetTextSize= TEXT_SIZE;
		expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 5;
		expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		string legend = "Drone Control Mode 1";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));
	}

    void ClearAllChange(){
        for(int i =0; i < expanDialSticks.NbRows; i++){
            for(int j =0; j < expanDialSticks.NbColumns; j++){
                expanDialSticks[i, j].TargetTextAlignment = TextAlignmentOptions.Center;
                expanDialSticks[i, j].TargetTextSize= TEXT_SIZE;
                expanDialSticks[i, j].TargetTextColor = new Color(0f, 0f, 0f);
                expanDialSticks[i, j].TargetText = "";
                expanDialSticks[i, j].TargetColor = new Color(1f, 1f, 1f);
                expanDialSticks[i, j].TargetPlaneTexture = "default";
                expanDialSticks[i, j].TargetPlaneRotation = 0f;
                expanDialSticks[i, j].TargetTextureChangeDuration = 1f;

                expanDialSticks[i, j].TargetProjectorBackTexture = "projector";
                expanDialSticks[i, j].TargetProjectorBackRotation = 0f;
                expanDialSticks[i, j].TargetProjectorBackSize = 0.0f;
                expanDialSticks[i, j].TargetProjectorBackColor = Color.white;
                expanDialSticks[i, j].TargetProjectorBackChangeDuration = 1f;

                expanDialSticks[i, j].TargetProjectorFrontTexture = "projector";
                expanDialSticks[i, j].TargetProjectorFrontRotation = 0f;
                expanDialSticks[i, j].TargetProjectorFrontSize = 0.0f;
                expanDialSticks[i, j].TargetProjectorFrontColor = Color.white;
                expanDialSticks[i, j].TargetProjectorFrontChangeDuration = 1f;

                expanDialSticks[i, j].TargetPosition = 0;
                expanDialSticks[i, j].TargetHolding = false;
                expanDialSticks[i, j].TargetShapeChangeDuration = 1f;
            }
        }
    }
    
    void Reset(){
        ClearAllChange();
        expanDialSticks.triggerProjectorChange();
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
        started = false;
        triggered = false;
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
        //string nextInput;
        //while(!inputs.TryPeek (out nextInput));
		//expanDialSticks.setBottomBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, nextInput, new Vector3(90f, -90f, 0f));
                           
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
        if(triggered == false)
		{

            if (e.diff > 0 && e.next >= JOYSTICK_THRESHOLD )
            {
                Debug.Log("HandleXAxisChanged -> (" + e.i + "|" + e.j + "|" + e.prev + "|" + e.next + "|" + e.diff + ")");
                prevTime = Time.time;
                triggered = true;
            }
            if (e.diff < 0 && e.next <= -JOYSTICK_THRESHOLD)
            {
                Debug.Log("HandleXAxisChanged -> (" + e.i + "|" + e.j + "|" + e.prev + "|" + e.next + "|" + e.diff + ")");
                prevTime = Time.time;
                triggered = true;
            }
        }

    }

	private void HandleYAxisChanged(object sender, ExpanDialStickEventArgs e)
	{
        if (triggered == false)
        {

            if (e.diff > 0 && e.next >= JOYSTICK_THRESHOLD)
            {
                Debug.Log("HandleYAxisChanged -> (" + e.i + "|" + e.j + "|" + e.prev + "|" + e.next + "|" + e.diff + ")");
                prevTime = Time.time;
                triggered = true;
            }
            if (e.diff < 0 && e.next <= -JOYSTICK_THRESHOLD)
            {
                Debug.Log("HandleYAxisChanged -> (" + e.i + "|" + e.j + "|" + e.prev + "|" + e.next + "|" + e.diff + ")");
                prevTime = Time.time;
                triggered = true;
            }
        }
    }

	private void HandleClickChanged(object sender, ExpanDialStickEventArgs e)
	{
		/*Debug.Log("HandleClickChanged -> (" + e.i + "|" + e.j + "|" + e.diff + ")");
		users.Enqueue(e.i + "|" + e.j + "|" + CLICK);*/
	}

	private void HandleRotationChanged(object sender, ExpanDialStickEventArgs e)
	{
        //Debug.Log("HandleRotationChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
        if (e.diff > 0) NextScenarioOutput();
	}

	private void HandlePositionChanged(object sender, ExpanDialStickEventArgs e)
	{
		//Debug.Log("HandlePositionChanged -> (" + e.i + '|' + e.j + '|' + e.diff + ")");
		/*if(e.diff > 0) users.Enqueue(e.i + "|" + e.j + "|" + PULL);
		else users.Enqueue(e.i + "|" + e.j + "|" + PUSH);*/

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
	
    void NextScenarioOutput()
	{
        if (inputs.Count > 0)
        {
            string input;
            while (!inputs.TryPeek(out input)) ;
            Debug.Log("input > " + input);
            users.Enqueue(input);
        }
    }
	void Update () {
		// check if ExpanDialSticks is connected
		if(connected && started){
			float currTime = Time.time;
			if (!inited)
			{
                displayDroneControlBigHand();
                inited = true;
			}
			if (Input.GetKey("escape"))
            {
                Application.Quit();
            }
			
            if (Input.GetKeyDown("n")) 
            {
                prevTime = Time.time;
                triggered = true;
            }
            if (triggered && currTime - prevTime > TRIGGER_DELAY)
            {
                displayRoverControlSingle();
                triggered = false;

            }

        }
    }
}
