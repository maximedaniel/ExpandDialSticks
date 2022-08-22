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


public class ArcadeGameScenario : MonoBehaviour
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
		/*inputs.Enqueue("4|0|" + CLICK);
		outputs.Enqueue( () => initMenu());
		inputs.Enqueue("2|2|" + LEFT_ROTATION);
		outputs.Enqueue( () => firstMoveMenu());
		inputs.Enqueue("2|2|" + PUSH);
		outputs.Enqueue( () => startPongGameSettings());
		inputs.Enqueue("2|2|" + RIGHT_ROTATION);
		outputs.Enqueue( () => changePongGamePlayerColor());
		inputs.Enqueue("2|2|" + TOP_BENDING);
		outputs.Enqueue( () => setFirstBadPongPlayer());
		inputs.Enqueue("0|0|" + PUSH);
		outputs.Enqueue( () => cancelFirstBadPongPlayer());
		inputs.Enqueue("2|2|" + BOTTOM_BENDING);
		outputs.Enqueue( () => setFirstPongPlayer());
		inputs.Enqueue("2|2|" + TOP_BENDING);
		outputs.Enqueue( () => setSecondPongPlayer());
		inputs.Enqueue("2|2|" + TOP_BENDING);
		outputs.Enqueue( () => setThirdPongPlayer());
		inputs.Enqueue("2|2|" + BOTTOM_BENDING);
		outputs.Enqueue( () => setFourthPongPlayer());
		inputs.Enqueue("2|2|" + PUSH);
		outputs.Enqueue( () => readyPongGame());
		inputs.Enqueue("2|2|" + PUSH);
		outputs.Enqueue( () => startPongGame());
		inputs.Enqueue("4|0|" + LEFT_ROTATION);
		outputs.Enqueue( () => player1FirstMove());
		inputs.Enqueue("4|0|" + RIGHT_ROTATION);
		outputs.Enqueue( () => player1SecondMove());
		inputs.Enqueue("4|0|" + RIGHT_ROTATION);
		outputs.Enqueue( () => player1ThirdMove());
		inputs.Enqueue("4|0|" + LEFT_BENDING);
		outputs.Enqueue( () => ballFirstMotion());
		inputs.Enqueue("4|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => ballSecondMotion())));
		inputs.Enqueue("4|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(2f, () => ballThirdMotion())));
		inputs.Enqueue("4|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(3f, () => ballFourthMotion())));
		inputs.Enqueue("4|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(4f, () => ballFifthMotion())));
		inputs.Enqueue("4|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(5f, () => ballSixthMotion())));
		inputs.Enqueue("4|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(6f, () => ballSeventhMotion())));
		inputs.Enqueue("4|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(7f, () => ballEighthMotion())));
		inputs.Enqueue("4|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(8f, () => ballNinthMotion())));
		inputs.Enqueue("4|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(9f, () => ballTenthMotion())));
		inputs.Enqueue("4|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(10f, () => player3Lost())));
		inputs.Enqueue("4|5|" + PUSH);
		outputs.Enqueue( () => player4Pause());
		inputs.Enqueue("2|2|" + RIGHT_ROTATION);
		outputs.Enqueue( () => startPongGame());
		inputs.Enqueue("4|5|" + PUSH);
		outputs.Enqueue( () => player4PauseAgain());
		inputs.Enqueue("2|2|" + PULL);
		outputs.Enqueue( () => Player4QuitToPongSettings());
		inputs.Enqueue("2|2|" + PULL);
		outputs.Enqueue( () => Player4QuitToGameMenu());*/
        /*
		// Tetris
		inputs.Enqueue("4|0|" + CLICK);
		outputs.Enqueue( () => Tetris0());
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => Tetris1())));
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(2f, () => Tetris2())));
		inputs.Enqueue("4|5|" + LEFT_BENDING);
		outputs.Enqueue( () => Tetris3());
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => Tetris4())));
		inputs.Enqueue("4|0|" + LEFT_ROTATION);
		outputs.Enqueue( ()  => Tetris5());
		inputs.Enqueue("4|0|" + LEFT_ROTATION);
		outputs.Enqueue( () => Tetris6());
		inputs.Enqueue("4|5|" + BOTTOM_BENDING);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => Tetris7())));
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(2f, () => Tetris8())));
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(3f, () => Tetris9())));
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(4f, () => Tetris10())));
        
		// PacMan
		inputs.Enqueue("4|0|" + CLICK);
		outputs.Enqueue( () => PacMan0());
		inputs.Enqueue("4|5|" + RIGHT_BENDING);
		outputs.Enqueue( () => PacMan1());
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => PacMan2())));
		inputs.Enqueue("4|5|" + BOTTOM_BENDING);
		outputs.Enqueue(() => PacMan3());
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => PacMan4())));
		inputs.Enqueue("4|5|" + RIGHT_BENDING);
		outputs.Enqueue( () => PacMan5());
		inputs.Enqueue("4|5|" + LEFT_BENDING);
		outputs.Enqueue( () => PacMan6());
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => PacMan7())));
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(2f, () => PacMan8())));
		inputs.Enqueue("4|5|" + BOTTOM_BENDING);
		outputs.Enqueue( () => PacMan9());
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => PacMan10())));
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(2f, () => PacMan11())));
        */
        // Snake
		inputs.Enqueue("4|0|" + CLICK);
		outputs.Enqueue( () => Snake0());
		inputs.Enqueue("4|5|" + RIGHT_BENDING);
		outputs.Enqueue( () => Snake1());
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => Snake2())));
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(2f, () => Snake3())));
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(3f, () => Snake4())));
		inputs.Enqueue("4|5|" + TOP_BENDING);
		outputs.Enqueue( () => Snake5());
		inputs.Enqueue("4|5|" + RIGHT_BENDING);
		outputs.Enqueue( () => Snake6());
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => Snake7())));
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(2f, () => Snake8())));
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(3f, () => Snake9())));
		inputs.Enqueue("4|5|" + BOTTOM_BENDING);
		outputs.Enqueue( () => Snake10());
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => Snake11())));
		inputs.Enqueue("4|5|" + RIGHT_BENDING);
		outputs.Enqueue( () => Snake12());
        inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => Snake13())));
		inputs.Enqueue("4|5|" + TOP_BENDING);
		outputs.Enqueue( () => Snake14());
		inputs.Enqueue("4|5|" + LEFT_BENDING);
		outputs.Enqueue( () => Snake15());
		inputs.Enqueue("4|0|" + PUSH);
		outputs.Enqueue( () => Snake16());
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => Snake17())));
		inputs.Enqueue("4|5|" + TOP_BENDING);
		outputs.Enqueue( () => Snake18());
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => Snake19())));
		inputs.Enqueue("4|5|" + RIGHT_BENDING);
		outputs.Enqueue( () => Snake20());
		inputs.Enqueue("2|0|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => Snake21())));
        
        inputs.Enqueue("4|0|" + CLICK);
		outputs.Enqueue( () => Reset());
	}

	void DequeueOutput(){
		Action action;
		while (!outputs.TryDequeue(out action));
		action();
	}

    void initMenu(){
        ClearAllChange();
        expanDialSticks[2, 0].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 0].TargetPlaneTexture = "pong";
        expanDialSticks[2, 0].TargetPlaneRotation = 90f;
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 0;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 2].TargetPlaneTexture = "pacman";
        expanDialSticks[2, 2].TargetPlaneRotation = 90f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 40;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[2, 4].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 4].TargetPlaneTexture = "tetris";
        expanDialSticks[2, 4].TargetPlaneRotation = 90f;
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 0;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[2, 5].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 5].TargetPlaneTexture = "invader";
        expanDialSticks[2, 5].TargetPlaneRotation = 0f;
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 0;
        expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;

		// Select Square

        expanDialSticks[1, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 5;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 1].TargetPosition = 5;
        expanDialSticks[3, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 5;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 5;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 5;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;
		

        expanDialSticks[1, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 5;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[3, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 5;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        
		string legend = "Rotate to navigate the game list.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

	void firstMoveMenu(){
        expanDialSticks[2, 0].TargetPlaneTexture = "snake";
        expanDialSticks[2, 0].TargetPlaneRotation = 90f;
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;

        expanDialSticks[2, 2].TargetPlaneTexture = "pong";
        expanDialSticks[2, 2].TargetPlaneRotation = 90f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
		
        expanDialSticks[2, 4].TargetPlaneTexture = "pacman";
        expanDialSticks[2, 5].TargetPlaneRotation = 90f;
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
		
        expanDialSticks[2, 5].TargetPlaneTexture = "tetris";
        expanDialSticks[2, 5].TargetPlaneRotation = 90f;
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        
        
		string legend = "Push to open game settings.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
	
	void startPongGameSettings(){
		
        ClearAllChange();
        expanDialSticks[2, 2].TargetColor = new Color(0.3f,0.3f,0.3f);
        expanDialSticks[2, 2].TargetPlaneTexture = "pong";
        expanDialSticks[2, 2].TargetPlaneRotation = 90f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 35;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

		// Select Square

        expanDialSticks[1, 1].TargetColor = new Color(1f,0f,0f);
        expanDialSticks[1, 1].TargetText = "P1?";
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 0;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 5;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 1].TargetColor = new Color(1f,0f,0f);
        expanDialSticks[3, 1].TargetText = "P1?";
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 1].TargetPosition = 0;
        expanDialSticks[3, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetColor = new Color(1f,0f,0f);
        expanDialSticks[1, 3].TargetText = "P1?";
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 0;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 5;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 3].TargetColor = new Color(1f,0f,0f);
        expanDialSticks[3, 3].TargetText = "P1?";
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 0;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;
		

        expanDialSticks[1, 2].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 5;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[3, 2].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 5;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

		string legend = "Rotate to change player color.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

	void changePongGamePlayerColor(){
		expanDialSticks[1, 1].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;

        expanDialSticks[3, 1].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;

        expanDialSticks[1, 3].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;

        expanDialSticks[3, 3].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;


		string legend = "Bend to distribute player controller.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	void setFirstBadPongPlayer(){
        expanDialSticks[0, 0].TargetText = "P1";
        expanDialSticks[0, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 20;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();


	}

	void cancelFirstBadPongPlayer(){
        expanDialSticks[0, 0].TargetText = "";
        expanDialSticks[0, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 0;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[1, 1].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[1, 1].TargetText = "P1?";
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 0;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;



		string legend = "Push to cancel player controller.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
		
	}


	void setFirstPongPlayer(){
		
        expanDialSticks[3, 1].TargetText = "";
        expanDialSticks[3, 1].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 1].TargetPosition = 5;
        expanDialSticks[3, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 0].TargetText = "P1";
        expanDialSticks[4, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 20;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetText = "P2?";
        expanDialSticks[1, 3].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;

        expanDialSticks[1, 1].TargetText = "P2?";
        expanDialSticks[1, 1].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;

        expanDialSticks[3, 3].TargetText = "P2?";
        expanDialSticks[3, 3].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	void setSecondPongPlayer(){

        expanDialSticks[1, 3].TargetText = "";
        expanDialSticks[1, 3].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 5;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[0, 5].TargetText = "P2";
        expanDialSticks[0, 5].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[0, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 5].TargetPosition = 20;
        expanDialSticks[0, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetText = "P3?";
        expanDialSticks[1, 1].TargetColor = new Color(1f,0f,1f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;

        expanDialSticks[3, 3].TargetText = "P3?";
        expanDialSticks[3, 3].TargetColor = new Color(1f,0f,1f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
		
		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));
	
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}
	

	void setThirdPongPlayer(){

        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[0, 0].TargetText = "P3";
        expanDialSticks[0, 0].TargetColor = new Color(1f,0f,1f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 20;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;


        expanDialSticks[3, 3].TargetText = "P4?";
        expanDialSticks[3, 3].TargetColor = new Color(1f,0.5f,0f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
	
		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}
	void setFourthPongPlayer(){

        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 5;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[4, 5].TargetText = "P4";
        expanDialSticks[4, 5].TargetColor = new Color(1f,0.5f,0f);
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 20;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;
	
		string legend = "Push to launch the game.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

void readyPongGame(){
		
        ClearAllChange();
		
        expanDialSticks[2, 2].TargetColor = new Color(0.6f,0.6f,0.6f);
        expanDialSticks[2, 2].TargetPlaneTexture = "pong";
        expanDialSticks[2, 2].TargetPlaneRotation = 90f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 0].TargetText = "P1";
        expanDialSticks[4, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 20;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[2, 0].TargetPlaneTexture = "barBall";
        expanDialSticks[2, 0].TargetPlaneRotation = 180f;
        expanDialSticks[2, 0].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 5;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[0, 5].TargetText = "P2";
        expanDialSticks[0, 5].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[0, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 5].TargetPosition = 20;
        expanDialSticks[0, 5].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[2, 5].TargetPlaneTexture = "bar";
        expanDialSticks[2, 5].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 5;
        expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[0, 0].TargetText = "P3";
        expanDialSticks[0, 0].TargetColor = new Color(1f,0f,1f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 20;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[0, 2].TargetPlaneTexture = "bar";
        expanDialSticks[0, 2].TargetPlaneRotation = 90f;
        expanDialSticks[0, 2].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 5;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[4, 5].TargetText = "P4";
        expanDialSticks[4, 5].TargetColor = new Color(1f,0.5f,0f);
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 20;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 3].TargetPlaneTexture = "bar";
        expanDialSticks[4, 3].TargetPlaneRotation = 90f;
        expanDialSticks[4, 3].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[4, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 3].TargetPosition = 5;
        expanDialSticks[4, 3].TargetShapeChangeDuration = 1f;
		
		string legend = "Push to start the game.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
 }


void startPongGame(){
		
        ClearAllChange();

        expanDialSticks[4, 0].TargetText = "P1";
        expanDialSticks[4, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 20;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[2, 0].TargetPlaneTexture = "barBall";
        expanDialSticks[2, 0].TargetPlaneRotation = 180f;
        expanDialSticks[2, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 5;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[0, 5].TargetText = "P2";
        expanDialSticks[0, 5].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[0, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 5].TargetPosition = 20;
        expanDialSticks[0, 5].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[2, 5].TargetPlaneTexture = "bar";
        expanDialSticks[2, 5].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 5;
        expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[0, 0].TargetText = "P3";
        expanDialSticks[0, 0].TargetColor = new Color(1f,0f,1f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 20;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[0, 2].TargetPlaneTexture = "bar";
        expanDialSticks[0, 2].TargetPlaneRotation = 90f;
        expanDialSticks[0, 2].TargetColor = new Color(1f,0f,1f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 5;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[4, 5].TargetText = "P4";
        expanDialSticks[4, 5].TargetColor = new Color(1f,0.5f,0f);
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 20;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 3].TargetPlaneTexture = "bar";
        expanDialSticks[4, 3].TargetPlaneRotation = 90f;
        expanDialSticks[4, 3].TargetColor = new Color(1f,0.5f,0f);
        expanDialSticks[4, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 3].TargetPosition = 5;
        expanDialSticks[4, 3].TargetShapeChangeDuration = 1f;
		
		
		string legend = "Bend to launch the ball and rotate to move the bar.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
 }


void player1FirstMove(){
	
        expanDialSticks[4, 0].TargetText = "P1";
        expanDialSticks[4, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 20;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[2, 0].TargetPlaneTexture = "default";
        expanDialSticks[2, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 0;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[2, 1].TargetPlaneTexture = "default";
        expanDialSticks[2, 1].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;

        expanDialSticks[1, 0].TargetPlaneTexture = "barBall";
        expanDialSticks[1, 0].TargetPlaneRotation = 180f;
        expanDialSticks[1, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 5;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;
		
		
		string legend = "Bend to launch the ball and rotate to move the bar.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


void player1SecondMove(){
	
        expanDialSticks[4, 0].TargetText = "P1";
        expanDialSticks[4, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 20;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[1, 0].TargetPlaneTexture = "default";
        expanDialSticks[1, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 0;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[1, 1].TargetPlaneTexture = "default";
        expanDialSticks[1, 1].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;

        expanDialSticks[2, 0].TargetPlaneTexture = "barBall";
        expanDialSticks[2, 0].TargetPlaneRotation = 180f;
        expanDialSticks[2, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 5;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;
		
		
		string legend = "Bend to launch the ball and rotate to move the bar.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


void player1ThirdMove(){
	
        expanDialSticks[4, 0].TargetText = "P1";
        expanDialSticks[4, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 20;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[2, 0].TargetPlaneTexture = "default";
        expanDialSticks[2, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 0;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[2, 1].TargetPlaneTexture = "default";
        expanDialSticks[2, 1].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;

        expanDialSticks[3, 0].TargetPlaneTexture = "barBall";
        expanDialSticks[3, 0].TargetPlaneRotation = 180f;
        expanDialSticks[3, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 0].TargetPosition = 5;
        expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;
		
		
		string legend = "Bend to launch the ball and rotate to move the bar.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));



        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


	void ballFirstMotion(){
        expanDialSticks[3, 0].TargetPlaneTexture = "bar";
        expanDialSticks[3, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;

        expanDialSticks[3, 1].TargetPlaneTexture = "ball";
        expanDialSticks[3, 1].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;
		
		
		string legend = "Bend to launch the ball and rotate to move the bar.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

	void ballSecondMotion(){
        expanDialSticks[3, 1].TargetPlaneTexture = "default";
        expanDialSticks[3, 1].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;

        expanDialSticks[3, 2].TargetPlaneTexture = "ball";
        expanDialSticks[3, 2].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
		
		
		string legend = "Bend to launch the ball and rotate to move the bar.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

	void ballThirdMotion(){
		

        expanDialSticks[3, 2].TargetPlaneTexture = "default";
        expanDialSticks[3, 2].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;

        expanDialSticks[3, 3].TargetPlaneTexture = "ball";
        expanDialSticks[3, 3].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
		
		string legend = "Bend to launch the ball and rotate to move the bar.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
	void ballFourthMotion(){
        expanDialSticks[3, 3].TargetPlaneTexture = "default";
        expanDialSticks[3, 3].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;

        expanDialSticks[3, 4].TargetPlaneTexture = "ball";
        expanDialSticks[3, 4].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;

        expanDialSticks[2, 5].TargetPlaneTexture = "default";
        expanDialSticks[2, 5].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 0;
        expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[3, 5].TargetPlaneTexture = "bar";
        expanDialSticks[3, 5].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[3, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 5].TargetPosition = 5;
        expanDialSticks[3, 5].TargetShapeChangeDuration = 1f;
		
		string legend = "Bend to launch the ball and rotate to move the bar.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
	
	void ballFifthMotion(){
        expanDialSticks[3, 4].TargetPlaneTexture = "default";
        expanDialSticks[3, 4].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
		
        expanDialSticks[3, 5].TargetPlaneTexture = "barBall";
        expanDialSticks[3, 5].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[3, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 5].TargetPosition = 5;
        expanDialSticks[3, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 3].TargetPlaneTexture = "default";
        expanDialSticks[4, 3].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[4, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 3].TargetPosition = 0;
        expanDialSticks[4, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 4].TargetPlaneTexture = "bar";
        expanDialSticks[4, 4].TargetPlaneRotation = 90f;
        expanDialSticks[4, 4].TargetColor = new Color(1f,0.5f,0f);
        expanDialSticks[4, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 4].TargetPosition = 5;
        expanDialSticks[4, 4].TargetShapeChangeDuration = 1f;

		
		string legend = "Bend to launch the ball and rotate to move the bar.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

	
	void ballSixthMotion(){
        expanDialSticks[3, 4].TargetPlaneTexture = "default";
        expanDialSticks[3, 4].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
		
        expanDialSticks[3, 5].TargetPlaneTexture = "bar";
        expanDialSticks[3, 5].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[3, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 5].TargetPosition = 5;
        expanDialSticks[3, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 4].TargetPlaneTexture = "barBall";
        expanDialSticks[4, 4].TargetPlaneRotation = 90f;
        expanDialSticks[4, 4].TargetColor = new Color(1f,0.5f,0f);
        expanDialSticks[4, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 4].TargetPosition = 5;
        expanDialSticks[4, 4].TargetShapeChangeDuration = 1f;

		
		string legend = "Bend to launch the ball and rotate to move the bar.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

	void ballSeventhMotion(){
        expanDialSticks[4, 4].TargetPlaneTexture = "bar";
        expanDialSticks[4, 4].TargetPlaneRotation = 90f;
        expanDialSticks[4, 4].TargetColor = new Color(1f,0.5f,0f);
        expanDialSticks[4, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 4].TargetPosition = 5;
        expanDialSticks[4, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetPlaneTexture = "ball";
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;

		
		string legend = "Bend to launch the ball and rotate to move the bar.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
	
	void ballEighthMotion(){

        expanDialSticks[3, 4].TargetPlaneTexture = "default";
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
		
        expanDialSticks[2, 4].TargetPlaneTexture = "ball";
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;

        expanDialSticks[0, 2].TargetPlaneTexture = "default";
        expanDialSticks[0, 2].TargetPlaneRotation = 0f;
        expanDialSticks[0, 2].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 0;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 1].TargetPlaneTexture = "bar";
        expanDialSticks[0, 1].TargetPlaneRotation = 90f;
        expanDialSticks[0, 1].TargetColor = new Color(1f,0f,1f);
        expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 1].TargetPosition = 5;
        expanDialSticks[0, 1].TargetShapeChangeDuration = 1f;
		
		
		string legend = "Bend to launch the ball and rotate to move the bar.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
	
	void ballNinthMotion(){

        expanDialSticks[2, 4].TargetPlaneTexture = "default";
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
		
        expanDialSticks[1, 4].TargetPlaneTexture = "ball";
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
		
        expanDialSticks[0, 1].TargetPlaneTexture = "default";
        expanDialSticks[0, 1].TargetPlaneRotation = 0f;
        expanDialSticks[0, 1].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 1].TargetPosition = 0;
        expanDialSticks[0, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 2].TargetPlaneTexture = "bar";
        expanDialSticks[0, 2].TargetPlaneRotation = 90f;
        expanDialSticks[0, 2].TargetColor = new Color(1f,0f,1f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 5;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;
		
		
		string legend = "Bend to launch the ball and rotate to move the bar.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
	void ballTenthMotion(){

        expanDialSticks[1, 4].TargetPlaneTexture = "default";
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
		
        expanDialSticks[0, 4].TargetPlaneTexture = "ball";
        expanDialSticks[0, 4].TargetTextureChangeDuration = 1f;
		
        expanDialSticks[0, 2].TargetPlaneTexture = "default";
        expanDialSticks[0, 2].TargetPlaneRotation = 0f;
        expanDialSticks[0, 2].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 0;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 3].TargetPlaneTexture = "bar";
        expanDialSticks[0, 3].TargetPlaneRotation = 90f;
        expanDialSticks[0, 3].TargetColor = new Color(1f,0f,1f);
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 5;
        expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;
		
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
	
	void player3Lost(){
        expanDialSticks[0, 3].TargetPlaneTexture = "default";
        expanDialSticks[0, 3].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 0;
        expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[0, 0].TargetText = "";
        expanDialSticks[0, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 0;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[0, 4].TargetPlaneTexture = "default";
        expanDialSticks[0, 4].TargetTextureChangeDuration = 1f;

        expanDialSticks[4, 4].TargetPlaneTexture = "barBall";
        expanDialSticks[4, 4].TargetPlaneRotation = 90f;
        expanDialSticks[4, 4].TargetColor = new Color(1f,0.5f,0f);
        expanDialSticks[4, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 4].TargetPosition = 5;
        expanDialSticks[4, 4].TargetShapeChangeDuration = 1f;

        for (int i = 0; i < 5; i++)
        {
        expanDialSticks[0, i].TargetColor = new Color(0f, 0f,0f);
        expanDialSticks[0, i].TargetTextureChangeDuration = 1f;
        }

		string legend = "Push to pause the game.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

	void player4Pause(){
        expanDialSticks[2, 2].TargetColor = new Color(0.6f,0.6f,0.6f);
        expanDialSticks[2, 2].TargetPlaneTexture = "pong";
        expanDialSticks[2, 2].TargetPlaneRotation = 90f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 4].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[4, 4].TargetTextureChangeDuration = 1f;
		
        expanDialSticks[3, 0].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;

        expanDialSticks[3, 5].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[3, 5].TargetTextureChangeDuration = 1f;
		

		string legend = "Rotate to restart the game.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
	
	void player4PauseAgain(){
        expanDialSticks[2, 2].TargetColor = new Color(0.6f,0.6f,0.6f);
        expanDialSticks[2, 2].TargetPlaneTexture = "pong";
        expanDialSticks[2, 2].TargetPlaneRotation = 90f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 2].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
		
        expanDialSticks[2, 0].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;

        expanDialSticks[4, 3].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[4, 3].TargetTextureChangeDuration = 1f;
		
        expanDialSticks[2, 5].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
		
		string legend = "Pull to quit game.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
	
	void Player4QuitToPongSettings(){

        ClearAllChange();
        expanDialSticks[2, 2].TargetColor = new Color(0.3f,0.3f,0.3f);
        expanDialSticks[2, 2].TargetPlaneTexture = "pong";
        expanDialSticks[2, 2].TargetPlaneRotation = 90f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 35;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

		// Player Pins
        expanDialSticks[4, 0].TargetText = "P1";
        expanDialSticks[4, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 20;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[0, 5].TargetText = "P2";
        expanDialSticks[0, 5].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[0, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 5].TargetPosition = 20;
        expanDialSticks[0, 5].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[0, 0].TargetText = "P3";
        expanDialSticks[0, 0].TargetColor = new Color(1f,0f,1f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 20;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[4, 5].TargetText = "P4";
        expanDialSticks[4, 5].TargetColor = new Color(1f,0.5f,0f);
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 20;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;

		// Select Square

        expanDialSticks[1, 1].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 5;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 1].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[3, 1].TargetText = "";
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 1].TargetPosition = 5;
        expanDialSticks[3, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[1, 3].TargetText = "";
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 5;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 5;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 3].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 5;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 5;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[3, 2].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 5;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;
		

		string legend = "Pull to quit game settings.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	void Player4QuitToGameMenu(){
        ClearAllChange();
        expanDialSticks[2, 0].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 0].TargetPlaneTexture = "snake";
        expanDialSticks[2, 0].TargetPlaneRotation = 90f;
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 0;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 2].TargetPlaneTexture = "pong";
        expanDialSticks[2, 2].TargetPlaneRotation = 90f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 40;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[2, 4].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 4].TargetPlaneTexture = "pacman";
        expanDialSticks[2, 4].TargetPlaneRotation = 90f;
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 0;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[2, 5].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[2, 5].TargetPlaneTexture = "tetris";
        expanDialSticks[2, 5].TargetPlaneRotation = 0f;
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 0;
        expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;

		// Select Square

        expanDialSticks[1, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 5;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 1].TargetPosition = 5;
        expanDialSticks[3, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 5;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 5;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 5;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;
		

        expanDialSticks[1, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 5;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[3, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 5;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 0].TargetPlaneTexture = "snake";
        expanDialSticks[2, 0].TargetPlaneRotation = 90f;
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;

        expanDialSticks[2, 2].TargetPlaneTexture = "pong";
        expanDialSticks[2, 2].TargetPlaneRotation = 90f;
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
		
        expanDialSticks[2, 4].TargetPlaneTexture = "pacman";
        expanDialSticks[2, 5].TargetPlaneRotation = 90f;
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
		
        expanDialSticks[2, 5].TargetPlaneTexture = "tetris";
        expanDialSticks[2, 5].TargetPlaneRotation = 90f;
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;

		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}
	
	
	void Tetris0(){
        ClearAllChange();
		
        expanDialSticks[4, 0].TargetPlaneTexture = "default";
        expanDialSticks[4, 0].TargetPlaneRotation = 0f;
        expanDialSticks[4, 0].TargetText = "Rotate";
        expanDialSticks[4, 0].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 20;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 5].TargetPlaneTexture = "default";
        expanDialSticks[4, 5].TargetPlaneRotation = 0f;
        expanDialSticks[4, 5].TargetText = "Move";
        expanDialSticks[4, 5].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 40;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;

		for(int j = 1; j < 5; j++){
			expanDialSticks[4, j].TargetColor = new Color(0f,0f,0f);
			expanDialSticks[4, j].TargetTextureChangeDuration = 1f;
		}
		// Parts
        expanDialSticks[3, 2].TargetColor = new Color(0f,0f,1f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 5;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 3].TargetColor = new Color(0f,0f,1f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 5;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[3, 4].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 5].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[3, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 5].TargetPosition = 5;
        expanDialSticks[3, 5].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[2, 4].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 5;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 5].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 5;
        expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	void Tetris1(){
        expanDialSticks[0, 3].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 5;
        expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}
	

	void Tetris2(){
        expanDialSticks[0, 2].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 5;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 3].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 5;
        expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 5;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}
	
	
	void Tetris3(){
        expanDialSticks[0, 2].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 0;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 3].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 0;
        expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 0;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[0, 1].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 1].TargetPosition = 5;
        expanDialSticks[0, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 2].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 5;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 5;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;
		

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	void Tetris4(){
        expanDialSticks[0, 1].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 1].TargetPosition = 0;
        expanDialSticks[0, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 2].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 0;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 0;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[0, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 5;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 1].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 1].TargetPosition = 5;
        expanDialSticks[0, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;
		

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	
	void Tetris5(){
        expanDialSticks[0, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 0;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 1].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 1].TargetPosition = 0;
        expanDialSticks[0, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 0;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[0, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 5;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 1].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 1].TargetPosition = 5;
        expanDialSticks[0, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 5;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;
		

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}
	


	void Tetris6(){
        expanDialSticks[0, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 0;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 1].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 1].TargetPosition = 0;
        expanDialSticks[0, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 0;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[0, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 5;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 5;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;
		

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	void Tetris7(){
        expanDialSticks[0, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 0;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 0;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 0;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[1, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 5;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 5;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 5;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}
	

	void Tetris8(){
        expanDialSticks[1, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 0;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 0;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 0;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[2, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 5;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 0].TargetPosition = 5;
        expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 1].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 1].TargetPosition = 5;
        expanDialSticks[3, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	void Tetris9(){
		for(int i = 0; i < 6; i++){
			expanDialSticks[3, i].TargetColor = new Color(1f,1f,1f);
			expanDialSticks[3, i].TargetTextureChangeDuration = 1f;
			expanDialSticks[3, i].TargetPosition = 0;
			expanDialSticks[3, i].TargetShapeChangeDuration = 1f;
		}
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	void Tetris10(){
        ClearAllChange();
		
        expanDialSticks[4, 0].TargetPlaneTexture = "default";
        expanDialSticks[4, 0].TargetPlaneRotation = 0f;
        expanDialSticks[4, 0].TargetText = "Rotate";
        expanDialSticks[4, 0].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 20;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 5].TargetPlaneTexture = "default";
        expanDialSticks[4, 5].TargetPlaneRotation = 0f;
        expanDialSticks[4, 5].TargetText = "Move";
        expanDialSticks[4, 5].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 40;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;

		for(int j = 1; j < 5; j++){
			expanDialSticks[4, j].TargetColor = new Color(0f,0f,0f);
			expanDialSticks[4, j].TargetTextureChangeDuration = 1f;
		}
		// Parts
		
        expanDialSticks[3, 4].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 5].TargetColor = new Color(0f,1f,1f);
        expanDialSticks[3, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 5].TargetPosition = 5;
        expanDialSticks[3, 5].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[3, 0].TargetColor = new Color(1f,1f,0f);
        expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 0].TargetPosition = 5;
        expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	

	void PacMan0(){
		ClearAllChange();
		// Controllers
        expanDialSticks[4, 5].TargetText = "0 pts";
        expanDialSticks[4, 5].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 40;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;

		// blocks
        expanDialSticks[1, 1].TargetColor = new Color(0.2f, 0.2f,0.2f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 10;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetColor = new Color(0.2f, 0.2f,0.2f);
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 10;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 3].TargetColor = new Color(0.2f, 0.2f,0.2f);
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 10;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 4].TargetColor = new Color(0.2f, 0.2f,0.2f);
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 10;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetColor = new Color(0.2f, 0.2f,0.2f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 10;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 1].TargetColor = new Color(0.2f, 0.2f,0.2f);
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 1].TargetPosition = 10;
        expanDialSticks[3, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 2].TargetColor = new Color(0.2f, 0.2f,0.2f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 10;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

		// Ennemi

        expanDialSticks[0, 0].TargetPlaneTexture = "pacman";
        expanDialSticks[0, 0].TargetPlaneRotation = 180f;
        expanDialSticks[0, 0].TargetColor = new Color(0f, 0f,0f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 5;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[0, 5].TargetPlaneTexture = "eyes";
        expanDialSticks[0, 5].TargetPlaneRotation = 90f;
        expanDialSticks[0, 5].TargetColor = new Color(0f, 0f, 1f);
        expanDialSticks[0, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 5].TargetPosition = 5;
        expanDialSticks[0, 5].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[4, 0].TargetPlaneTexture = "eyes";
        expanDialSticks[4, 0].TargetPlaneRotation = 180f;
        expanDialSticks[4, 0].TargetColor = new Color(1f, 0f, 0f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 5;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

		// Row 1
        expanDialSticks[0, 1].TargetPlaneTexture = "ball";
        expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPlaneTexture = "ball";
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPlaneTexture = "ball";
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 4].TargetPlaneTexture = "ball";
        expanDialSticks[0, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 4].TargetPlaneTexture = "ball";
        expanDialSticks[0, 4].TargetTextureChangeDuration = 1f;

		// Row 1
        expanDialSticks[1, 0].TargetPlaneTexture = "ball";
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPlaneTexture = "ball";
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 5].TargetPlaneTexture = "ball";
        expanDialSticks[1, 5].TargetTextureChangeDuration = 1f;
		
		// Row 2
        expanDialSticks[2, 0].TargetPlaneTexture = "ball";
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPlaneTexture = "ball";
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPlaneTexture = "ball";
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPlaneTexture = "bigBall";
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPlaneTexture = "ball";
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;


		// Row 3
        expanDialSticks[3, 0].TargetPlaneTexture = "ball";
        expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPlaneTexture = "ball";
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPlaneTexture = "ball";
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 5].TargetPlaneTexture = "ball";
        expanDialSticks[3, 5].TargetTextureChangeDuration = 1f;
		
		// Row 4
        expanDialSticks[4, 1].TargetPlaneTexture = "ball";
        expanDialSticks[4, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 2].TargetPlaneTexture = "ball";
        expanDialSticks[4, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 3].TargetPlaneTexture = "ball";
        expanDialSticks[4, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 4].TargetPlaneTexture = "ball";
        expanDialSticks[4, 4].TargetTextureChangeDuration = 1f;
		

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}
	void PacMan1(){
		
        expanDialSticks[4, 5].TargetText = "1 pts";

 		expanDialSticks[0, 0].TargetPlaneTexture = "default";
        expanDialSticks[0, 0].TargetPlaneRotation = 0f;
        expanDialSticks[0, 0].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 0;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;

 		expanDialSticks[0, 1].TargetPlaneTexture = "pacman";
        expanDialSticks[0, 1].TargetPlaneRotation = 90f;
        expanDialSticks[0, 1].TargetColor = new Color(0f, 0f,0f);
        expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 1].TargetPosition = 5;
        expanDialSticks[0, 1].TargetShapeChangeDuration = 1f;
		
 		expanDialSticks[0, 5].TargetPlaneTexture = "ball";
        expanDialSticks[0, 5].TargetPlaneRotation = 0f;
        expanDialSticks[0, 5].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[0, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 5].TargetPosition = 0;
        expanDialSticks[0, 5].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[0, 4].TargetPlaneTexture = "eyes";
        expanDialSticks[0, 4].TargetPlaneRotation = 90f;
        expanDialSticks[0, 4].TargetColor = new Color(0f, 0f, 1f);
        expanDialSticks[0, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 4].TargetPosition = 5;
        expanDialSticks[0, 4].TargetShapeChangeDuration = 1f;
		
		
 		expanDialSticks[4, 0].TargetPlaneTexture = "ball";
        expanDialSticks[4, 0].TargetPlaneRotation = 0f;
        expanDialSticks[4, 0].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 0;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;
		

        expanDialSticks[3, 0].TargetPlaneTexture = "eyes";
        expanDialSticks[3, 0].TargetPlaneRotation = 180f;
        expanDialSticks[3, 0].TargetColor = new Color(1f, 0f, 0f);
        expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 0].TargetPosition = 5;
        expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}
	
	void PacMan2(){
		
        expanDialSticks[4, 5].TargetText = "2 pts";

 		expanDialSticks[0, 1].TargetPlaneTexture = "default";
        expanDialSticks[0, 1].TargetPlaneRotation = 0f;
        expanDialSticks[0, 1].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 1].TargetPosition = 0;
        expanDialSticks[0, 1].TargetShapeChangeDuration = 1f;

 		expanDialSticks[0, 2].TargetPlaneTexture = "pacman";
        expanDialSticks[0, 2].TargetPlaneRotation = 90f;
        expanDialSticks[0, 2].TargetColor = new Color(0f, 0f,0f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 5;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;
		
 		expanDialSticks[0, 4].TargetPlaneTexture = "ball";
        expanDialSticks[0, 4].TargetPlaneRotation = 0f;
        expanDialSticks[0, 4].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[0, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 4].TargetPosition = 0;
        expanDialSticks[0, 4].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[0, 3].TargetPlaneTexture = "eyes";
        expanDialSticks[0, 3].TargetPlaneRotation = 90f;
        expanDialSticks[0, 3].TargetColor = new Color(0f, 0f, 1f);
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 5;
        expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;
		
		
 		expanDialSticks[3, 0].TargetPlaneTexture = "ball";
        expanDialSticks[3, 0].TargetPlaneRotation = 0f;
        expanDialSticks[3, 0].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 0].TargetPosition = 0;
        expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;
		

        expanDialSticks[2, 0].TargetPlaneTexture = "eyes";
        expanDialSticks[2, 0].TargetPlaneRotation = 180f;
        expanDialSticks[2, 0].TargetColor = new Color(1f, 0f, 0f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 5;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	void PacMan3(){
		
        expanDialSticks[4, 5].TargetText = "3 pts";

 		expanDialSticks[0, 2].TargetPlaneTexture = "default";
        expanDialSticks[0, 2].TargetPlaneRotation = 0f;
        expanDialSticks[0, 2].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 0;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;

 		expanDialSticks[1, 2].TargetPlaneTexture = "pacman";
        expanDialSticks[1, 2].TargetPlaneRotation = 180f;
        expanDialSticks[1, 2].TargetColor = new Color(0f, 0f,0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 5;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;
		
 		expanDialSticks[0, 3].TargetPlaneTexture = "ball";
        expanDialSticks[0, 3].TargetPlaneRotation = 0f;
        expanDialSticks[0, 3].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 0;
        expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[0, 2].TargetPlaneTexture = "eyes";
        expanDialSticks[0, 2].TargetPlaneRotation = 90f;
        expanDialSticks[0, 2].TargetColor = new Color(0f, 0f, 1f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 5;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;
		
		
 		expanDialSticks[2, 0].TargetPlaneTexture = "ball";
        expanDialSticks[2, 0].TargetPlaneRotation = 0f;
        expanDialSticks[2, 0].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 0;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;
		

        expanDialSticks[1, 0].TargetPlaneTexture = "eyes";
        expanDialSticks[1, 0].TargetPlaneRotation = 180f;
        expanDialSticks[1, 0].TargetColor = new Color(1f, 0f, 0f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 5;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}
	void PacMan4(){
		
        expanDialSticks[4, 5].TargetText = "4 pts";

 		expanDialSticks[1, 2].TargetPlaneTexture = "default";
        expanDialSticks[1, 2].TargetPlaneRotation = 0f;
        expanDialSticks[1, 2].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 0;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

 		expanDialSticks[2, 2].TargetPlaneTexture = "pacman";
        expanDialSticks[2, 2].TargetPlaneRotation = 180f;
        expanDialSticks[2, 2].TargetColor = new Color(0f, 0f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;
		
 		expanDialSticks[0, 2].TargetPlaneTexture = "default";
        expanDialSticks[0, 2].TargetPlaneRotation = 0f;
        expanDialSticks[0, 2].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 0;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[1, 2].TargetPlaneTexture = "eyes";
        expanDialSticks[1, 2].TargetPlaneRotation = 0f;
        expanDialSticks[1, 2].TargetColor = new Color(0f, 0f, 1f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 5;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;
		

 		expanDialSticks[1, 0].TargetPlaneTexture = "ball";
        expanDialSticks[1, 0].TargetPlaneRotation = 0f;
        expanDialSticks[1, 0].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 0;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;
		

        expanDialSticks[2, 0].TargetPlaneTexture = "eyes";
        expanDialSticks[2, 0].TargetPlaneRotation = 0f;
        expanDialSticks[2, 0].TargetColor = new Color(1f, 0f, 0f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 5;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	void PacMan5(){


        expanDialSticks[4, 5].TargetText = "5 pts";

 		expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 0;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

 		expanDialSticks[2, 3].TargetPlaneTexture = "pacman";
        expanDialSticks[2, 3].TargetPlaneRotation = 90f;
        expanDialSticks[2, 3].TargetColor = new Color(0f, 0f,0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 10;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;
		
 		expanDialSticks[1, 2].TargetPlaneTexture = "default";
        expanDialSticks[1, 2].TargetPlaneRotation = 0f;
        expanDialSticks[1, 2].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 0;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;
		
        expanDialSticks[2, 2].TargetPlaneTexture = "eyes";
        expanDialSticks[2, 2].TargetPlaneRotation = -90f;
        expanDialSticks[2, 2].TargetColor = new Color(0f, 0f, 1f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;
		

 		expanDialSticks[2, 0].TargetPlaneTexture = "ball";
        expanDialSticks[2, 0].TargetPlaneRotation = 0f;
        expanDialSticks[2, 0].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 0;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;
		

        expanDialSticks[2, 1].TargetPlaneTexture = "eyes";
        expanDialSticks[2, 1].TargetPlaneRotation = -90f;
        expanDialSticks[2, 1].TargetColor = new Color(1f, 0f, 0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 5;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

	}
	
	void PacMan6(){
		
		
        expanDialSticks[4, 5].TargetText = "15 pts";

 		expanDialSticks[2, 3].TargetPlaneTexture = "default";
        expanDialSticks[2, 3].TargetPlaneRotation = 0f;
        expanDialSticks[2, 3].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 0;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

 		expanDialSticks[2, 2].TargetPlaneTexture = "pacman";
        expanDialSticks[2, 2].TargetPlaneRotation = -90f;
        expanDialSticks[2, 2].TargetColor = new Color(0f, 0f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 10;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;
		
 		expanDialSticks[2, 0].TargetPlaneTexture = "ball";
        expanDialSticks[2, 0].TargetPlaneRotation = 0f;
        expanDialSticks[2, 0].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 0;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;
		

        expanDialSticks[2, 1].TargetPlaneTexture = "eyes";
        expanDialSticks[2, 1].TargetPlaneRotation = -90f;
        expanDialSticks[2, 1].TargetColor = new Color(1f, 0f, 0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 5;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}


	
	void PacMan7(){
		
        expanDialSticks[4, 5].TargetText = "25 pts";

 		expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 0;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

 		expanDialSticks[2, 1].TargetPlaneTexture = "pacman";
        expanDialSticks[2, 1].TargetPlaneRotation = -90f;
        expanDialSticks[2, 1].TargetColor = new Color(0f, 0f,0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 10;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	void PacMan8(){
		
        expanDialSticks[4, 5].TargetText = "26 pts";

 		expanDialSticks[2, 1].TargetPlaneTexture = "default";
        expanDialSticks[2, 1].TargetPlaneRotation = 0f;
        expanDialSticks[2, 1].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 0;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

 		expanDialSticks[2, 0].TargetPlaneTexture = "pacman";
        expanDialSticks[2, 0].TargetPlaneRotation = -90f;
        expanDialSticks[2, 0].TargetColor = new Color(0f, 0f,0f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 10;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	
	void PacMan9(){
		
        expanDialSticks[4, 5].TargetText = "27 pts";

 		expanDialSticks[2, 0].TargetPlaneTexture = "default";
        expanDialSticks[2, 0].TargetPlaneRotation = 0f;
        expanDialSticks[2, 0].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 0;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

 		expanDialSticks[3, 0].TargetPlaneTexture = "pacman";
        expanDialSticks[3, 0].TargetPlaneRotation = 180f;
        expanDialSticks[3, 0].TargetColor = new Color(0f, 0f,0f);
        expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 0].TargetPosition = 10;
        expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

	void PacMan10(){
		
        expanDialSticks[4, 5].TargetText = "28 pts";

 		expanDialSticks[3, 0].TargetPlaneTexture = "default";
        expanDialSticks[3, 0].TargetPlaneRotation = 0f;
        expanDialSticks[3, 0].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 0].TargetPosition = 0;
        expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;

 		expanDialSticks[4, 0].TargetPlaneTexture = "pacman";
        expanDialSticks[4, 0].TargetPlaneRotation = 180f;
        expanDialSticks[4, 0].TargetColor = new Color(0f, 0f,0f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 10;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

		
        expanDialSticks[0, 5].TargetPlaneTexture = "eyes";
        expanDialSticks[0, 5].TargetPlaneRotation = -90f;
        expanDialSticks[0, 5].TargetColor = new Color(1f, 0.5f, 0f);
        expanDialSticks[0, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 5].TargetPosition = 5;
        expanDialSticks[0, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}
	
	void PacMan11(){
		
        expanDialSticks[4, 5].TargetText = "29 pts";

 		expanDialSticks[4, 0].TargetPlaneTexture = "default";
        expanDialSticks[4, 0].TargetPlaneRotation = 0f;
        expanDialSticks[4, 0].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 0;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

 		expanDialSticks[4, 1].TargetPlaneTexture = "pacman";
        expanDialSticks[4, 1].TargetPlaneRotation = 90f;
        expanDialSticks[4, 1].TargetColor = new Color(0f, 0f,0f);
        expanDialSticks[4, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 1].TargetPosition = 10;
        expanDialSticks[4, 1].TargetShapeChangeDuration = 1f;

 		expanDialSticks[0, 5].TargetPlaneTexture = "ball";
        expanDialSticks[0, 5].TargetPlaneRotation = 0f;
        expanDialSticks[0, 5].TargetColor = new Color(1f, 1f,1f);
        expanDialSticks[0, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 5].TargetPosition = 0;
        expanDialSticks[0, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 4].TargetPlaneTexture = "eyes";
        expanDialSticks[0, 4].TargetPlaneRotation = -90f;
        expanDialSticks[0, 4].TargetColor = new Color(1f, 0.5f, 0f);
        expanDialSticks[0, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 4].TargetPosition = 5;
        expanDialSticks[0, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}


	void Snake0(){
		ClearAllChange();
		// Controllers
        
        expanDialSticks[4, 0].TargetText = "Jump";
        expanDialSticks[4, 0].TargetColor = new Color(0f,0.6f,0f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 5;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

        for (int i = 1; i < 5; i++)
        {
        
        expanDialSticks[4, i].TargetText = "";
        expanDialSticks[4, i].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[4, i].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, i].TargetPosition = 0;
        expanDialSticks[4, i].TargetShapeChangeDuration = 1f;
            
        }

        expanDialSticks[4, 5].TargetText = "Move";
        expanDialSticks[4, 5].TargetColor = new Color(0f,0.6f,0f);
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 40;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;

        // Candy

        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetPlaneTexture = "candies";
        expanDialSticks[1, 4].TargetColor = new Color(1f,0.4f,0.7f);
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        // Snake

        expanDialSticks[0, 0].TargetText = "";
        expanDialSticks[0, 0].TargetPlaneTexture = "default";
        expanDialSticks[0, 0].TargetPlaneRotation = 0f;
        expanDialSticks[0, 0].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 5;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetPlaneTexture = "default";
        expanDialSticks[1, 0].TargetPlaneRotation = 0f;
        expanDialSticks[1, 0].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 5;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 0].TargetText = "";
        expanDialSticks[2, 0].TargetPlaneTexture = "snakehead";
        expanDialSticks[2, 0].TargetPlaneRotation = 180f;
        expanDialSticks[2, 0].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 5;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
	}

    void Snake1(){

        expanDialSticks[0, 0].TargetText = "";
        expanDialSticks[0, 0].TargetPlaneTexture = "default";
        expanDialSticks[0, 0].TargetPlaneRotation = 0f;
        expanDialSticks[0, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 0;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetPlaneTexture = "default";
        expanDialSticks[1, 0].TargetPlaneRotation = 0f;
        expanDialSticks[1, 0].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 5;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 0].TargetText = "";
        expanDialSticks[2, 0].TargetPlaneTexture = "default";
        expanDialSticks[2, 0].TargetPlaneRotation = 0f;
        expanDialSticks[2, 0].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 5;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetText = "";
        expanDialSticks[2, 1].TargetPlaneTexture = "snakehead";
        expanDialSticks[2, 1].TargetPlaneRotation = 90f;
        expanDialSticks[2, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 5;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


    void Snake2(){

        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetPlaneTexture = "default";
        expanDialSticks[1, 0].TargetPlaneRotation = 0f;
        expanDialSticks[1, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 0;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 0].TargetText = "";
        expanDialSticks[2, 0].TargetPlaneTexture = "default";
        expanDialSticks[2, 0].TargetPlaneRotation = 0f;
        expanDialSticks[2, 0].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 5;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetText = "";
        expanDialSticks[2, 1].TargetPlaneTexture = "default";
        expanDialSticks[2, 1].TargetPlaneRotation = 0f;
        expanDialSticks[2, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 5;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "snakehead";
        expanDialSticks[2, 2].TargetPlaneRotation = 90f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    
    void Snake3(){

        expanDialSticks[2, 0].TargetText = "";
        expanDialSticks[2, 0].TargetPlaneTexture = "default";
        expanDialSticks[2, 0].TargetPlaneRotation = 0f;
        expanDialSticks[2, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 0].TargetPosition = 0;
        expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetText = "";
        expanDialSticks[2, 1].TargetPlaneTexture = "default";
        expanDialSticks[2, 1].TargetPlaneRotation = 0f;
        expanDialSticks[2, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 5;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = "";
        expanDialSticks[2, 3].TargetPlaneTexture = "snakehead";
        expanDialSticks[2, 3].TargetPlaneRotation = 90f;
        expanDialSticks[2, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 5;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void Snake4(){

        expanDialSticks[2, 1].TargetText = "";
        expanDialSticks[2, 1].TargetPlaneTexture = "default";
        expanDialSticks[2, 1].TargetPlaneRotation = 0f;
        expanDialSticks[2, 1].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 0;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = "";
        expanDialSticks[2, 3].TargetPlaneTexture = "default";
        expanDialSticks[2, 3].TargetPlaneRotation = 0f;
        expanDialSticks[2, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 5;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetPlaneTexture = "snakehead";
        expanDialSticks[2, 4].TargetPlaneRotation = 90f;
        expanDialSticks[2, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 5;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void Snake5(){ // Eat

        // Candy

        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetPlaneTexture = "candies";
        expanDialSticks[1, 0].TargetColor = new Color(1f,0.4f,0.7f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 5;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        // Snake

        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 0;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = "";
        expanDialSticks[2, 3].TargetPlaneTexture = "default";
        expanDialSticks[2, 3].TargetPlaneRotation = 0f;
        expanDialSticks[2, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 5;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetPlaneTexture = "default";
        expanDialSticks[2, 4].TargetPlaneRotation = 0f;
        expanDialSticks[2, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 5;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetPlaneTexture = "snakehead";
        expanDialSticks[1, 4].TargetPlaneRotation = 0f;
        expanDialSticks[1, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


    void Snake6(){

        // Snake

        expanDialSticks[2, 3].TargetText = "";
        expanDialSticks[2, 3].TargetPlaneTexture = "default";
        expanDialSticks[2, 3].TargetPlaneRotation = 0f;
        expanDialSticks[2, 3].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 0;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetPlaneTexture = "default";
        expanDialSticks[2, 4].TargetPlaneRotation = 0f;
        expanDialSticks[2, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 5;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetPlaneTexture = "candies";
        expanDialSticks[1, 4].TargetPlaneRotation = 0f;
        expanDialSticks[1, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 5].TargetText = "";
        expanDialSticks[1, 5].TargetPlaneTexture = "snakehead";
        expanDialSticks[1, 5].TargetPlaneRotation = 90f;
        expanDialSticks[1, 5].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 5].TargetPosition = 5;
        expanDialSticks[1, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void Snake7(){
        // Candy
        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetPlaneTexture = "candy";
        expanDialSticks[3, 3].TargetColor = new Color(1f,0.4f,0.7f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 5;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        // Snake
        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetPlaneTexture = "default";
        expanDialSticks[2, 4].TargetPlaneRotation = 0f;
        expanDialSticks[2, 4].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 0;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetPlaneTexture = "candies";
        expanDialSticks[1, 4].TargetPlaneRotation = 0f;
        expanDialSticks[1, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 5].TargetText = "";
        expanDialSticks[1, 5].TargetPlaneTexture = "default";
        expanDialSticks[1, 5].TargetPlaneRotation = 0f;
        expanDialSticks[1, 5].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 5].TargetPosition = 5;
        expanDialSticks[1, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetPlaneTexture = "snakehead";
        expanDialSticks[1, 0].TargetPlaneRotation = 90f;
        expanDialSticks[1, 0].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 5;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void Snake8(){


        // Snake
        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetPlaneTexture = "candy";
        expanDialSticks[1, 4].TargetPlaneRotation = 0f;
        expanDialSticks[1, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 5].TargetText = "";
        expanDialSticks[1, 5].TargetPlaneTexture = "default";
        expanDialSticks[1, 5].TargetPlaneRotation = 0f;
        expanDialSticks[1, 5].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 5].TargetPosition = 5;
        expanDialSticks[1, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetPlaneTexture = "candies";
        expanDialSticks[1, 0].TargetPlaneRotation = 0f;
        expanDialSticks[1, 0].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 5;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetPlaneTexture = "snakehead";
        expanDialSticks[1, 1].TargetPlaneRotation = 90f;
        expanDialSticks[1, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


 void Snake9(){

        // Snake
        
        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetPlaneTexture = "default";
        expanDialSticks[1, 4].TargetPlaneRotation = 0f;
        expanDialSticks[1, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 5;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 5].TargetText = "";
        expanDialSticks[1, 5].TargetPlaneTexture = "default";
        expanDialSticks[1, 5].TargetPlaneRotation = 0f;
        expanDialSticks[1, 5].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 5].TargetPosition = 5;
        expanDialSticks[1, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetPlaneTexture = "candies";
        expanDialSticks[1, 0].TargetPlaneRotation = 0f;
        expanDialSticks[1, 0].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 5;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetPlaneTexture = "default";
        expanDialSticks[1, 1].TargetPlaneRotation = 0f;
        expanDialSticks[1, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetPlaneTexture = "snakehead";
        expanDialSticks[1, 2].TargetPlaneRotation = 90f;
        expanDialSticks[1, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 5;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
}

void Snake10(){

        // Snake
        
        expanDialSticks[1, 4].TargetText = "";
        expanDialSticks[1, 4].TargetPlaneTexture = "default";
        expanDialSticks[1, 4].TargetPlaneRotation = 0f;
        expanDialSticks[1, 4].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 0;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetPlaneTexture = "candies";
        expanDialSticks[1, 0].TargetPlaneRotation = 0f;
        expanDialSticks[1, 0].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 5;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetPlaneTexture = "default";
        expanDialSticks[1, 1].TargetPlaneRotation = 0f;
        expanDialSticks[1, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetPlaneTexture = "default";
        expanDialSticks[1, 2].TargetPlaneRotation = 0f;
        expanDialSticks[1, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 5;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "snakehead";
        expanDialSticks[2, 2].TargetPlaneRotation =  180f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


    void Snake11(){
        // Snake
        
        expanDialSticks[1, 5].TargetText = "";
        expanDialSticks[1, 5].TargetPlaneTexture = "default";
        expanDialSticks[1, 5].TargetPlaneRotation = 0f;
        expanDialSticks[1, 5].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 5].TargetPosition = 0;
        expanDialSticks[1, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetPlaneTexture = "candies";
        expanDialSticks[1, 0].TargetPlaneRotation = 0f;
        expanDialSticks[1, 0].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 5;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetPlaneTexture = "default";
        expanDialSticks[1, 1].TargetPlaneRotation = 0f;
        expanDialSticks[1, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetPlaneTexture = "default";
        expanDialSticks[1, 2].TargetPlaneRotation = 0f;
        expanDialSticks[1, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 5;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetPlaneTexture = "snakehead";
        expanDialSticks[3, 2].TargetPlaneRotation =  180f;
        expanDialSticks[3, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 5;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void Snake12(){
        // Candy
        expanDialSticks[2, 1].TargetText = "";
        expanDialSticks[2, 1].TargetPlaneTexture = "candy";
        expanDialSticks[2, 1].TargetColor = new Color(1f,0.4f,0.7f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 5;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        // Snake
        
        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetPlaneTexture = "candy";
        expanDialSticks[1, 0].TargetPlaneRotation = 0f;
        expanDialSticks[1, 0].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 5;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetPlaneTexture = "default";
        expanDialSticks[1, 1].TargetPlaneRotation = 0f;
        expanDialSticks[1, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetPlaneTexture = "default";
        expanDialSticks[1, 2].TargetPlaneRotation = 0f;
        expanDialSticks[1, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 5;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetPlaneTexture = "default";
        expanDialSticks[3, 2].TargetPlaneRotation = 0f;
        expanDialSticks[3, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 5;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetPlaneTexture = "snakehead";
        expanDialSticks[3, 3].TargetPlaneRotation =  90f;
        expanDialSticks[3, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 5;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void Snake13(){

        // Snake
        
        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetPlaneTexture = "default";
        expanDialSticks[1, 0].TargetPlaneRotation = 0f;
        expanDialSticks[1, 0].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 5;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetPlaneTexture = "default";
        expanDialSticks[1, 1].TargetPlaneRotation = 0f;
        expanDialSticks[1, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetPlaneTexture = "default";
        expanDialSticks[1, 2].TargetPlaneRotation = 0f;
        expanDialSticks[1, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 5;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetPlaneTexture = "default";
        expanDialSticks[3, 2].TargetPlaneRotation = 0f;
        expanDialSticks[3, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 5;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetPlaneTexture = "candy";
        expanDialSticks[3, 3].TargetPlaneRotation = 0f;
        expanDialSticks[3, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 5;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetPlaneTexture = "snakehead";
        expanDialSticks[3, 4].TargetPlaneRotation =  90f;
        expanDialSticks[3, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void Snake14(){

        // Snake
        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetPlaneTexture = "default";
        expanDialSticks[1, 0].TargetPlaneRotation = 0f;
        expanDialSticks[1, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 0;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetPlaneTexture = "default";
        expanDialSticks[1, 2].TargetPlaneRotation = 0f;
        expanDialSticks[1, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 5;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;
   
        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetPlaneTexture = "default";
        expanDialSticks[3, 2].TargetPlaneRotation = 0f;
        expanDialSticks[3, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 5;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetPlaneTexture = "candy";
        expanDialSticks[3, 3].TargetPlaneRotation = 0f;
        expanDialSticks[3, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 5;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetPlaneTexture = "default";
        expanDialSticks[3, 4].TargetPlaneRotation = 0f;
        expanDialSticks[3, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetPlaneTexture = "snakehead";
        expanDialSticks[2, 4].TargetPlaneRotation =  0f;
        expanDialSticks[2, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 5;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


    void Snake15(){

        // Snake
        expanDialSticks[1, 0].TargetText = "";
        expanDialSticks[1, 0].TargetPlaneTexture = "default";
        expanDialSticks[1, 0].TargetPlaneRotation = 0f;
        expanDialSticks[1, 0].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 0].TargetPosition = 0;
        expanDialSticks[1, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetPlaneTexture = "default";
        expanDialSticks[1, 1].TargetPlaneRotation = 0f;
        expanDialSticks[1, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetPlaneTexture = "default";
        expanDialSticks[1, 2].TargetPlaneRotation = 0f;
        expanDialSticks[1, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 5;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;
   
        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetPlaneTexture = "default";
        expanDialSticks[3, 2].TargetPlaneRotation = 0f;
        expanDialSticks[3, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 5;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetPlaneTexture = "candy";
        expanDialSticks[3, 3].TargetPlaneRotation = 0f;
        expanDialSticks[3, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 5;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetPlaneTexture = "default";
        expanDialSticks[3, 4].TargetPlaneRotation = 0f;
        expanDialSticks[3, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetPlaneTexture = "default";
        expanDialSticks[2, 4].TargetPlaneRotation = 0f;
        expanDialSticks[2, 4].TargetColor = new Color(0f,0.6f,0f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 10;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 3].TargetText = "";
        expanDialSticks[2, 3].TargetPlaneTexture = "snakehead";
        expanDialSticks[2, 3].TargetPlaneRotation =  -90f;
        expanDialSticks[2, 3].TargetColor = new Color(0f,0.6f,0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 10;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

    }

    
    void Snake16(){
        // Up Controller
        expanDialSticks[4, 0].TargetText = "Jump";
        expanDialSticks[4, 0].TargetColor = new Color(0f,0.6f,0f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 5;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

        // Snake
        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetPlaneTexture = "default";
        expanDialSticks[1, 1].TargetPlaneRotation = 0f;
        expanDialSticks[1, 1].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 0;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetPlaneTexture = "default";
        expanDialSticks[1, 2].TargetPlaneRotation = 0f;
        expanDialSticks[1, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 5;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;
   
        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetPlaneTexture = "default";
        expanDialSticks[3, 2].TargetPlaneRotation = 0f;
        expanDialSticks[3, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 5;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetPlaneTexture = "candy";
        expanDialSticks[3, 3].TargetPlaneRotation = 0f;
        expanDialSticks[3, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 5;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetPlaneTexture = "default";
        expanDialSticks[3, 4].TargetPlaneRotation = 0f;
        expanDialSticks[3, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetPlaneTexture = "default";
        expanDialSticks[2, 4].TargetPlaneRotation = 0f;
        expanDialSticks[2, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 5;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = "";
        expanDialSticks[2, 3].TargetPlaneTexture = "default";
        expanDialSticks[2, 3].TargetPlaneRotation = 0f;
        expanDialSticks[2, 3].TargetColor = new Color(0f,0.6f,0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 10;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "snakehead";
        expanDialSticks[2, 2].TargetPlaneRotation =  -90f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,0.3f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 15;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void Snake17(){

        // Candy
        expanDialSticks[0, 4].TargetText = "";
        expanDialSticks[0, 4].TargetPlaneTexture = "candy";
        expanDialSticks[0, 4].TargetColor = new Color(1f,0.4f,0.7f);
        expanDialSticks[0, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 4].TargetPosition = 5;
        expanDialSticks[0, 4].TargetShapeChangeDuration = 1f;

        // Snake
        expanDialSticks[1, 2].TargetText = "";
        expanDialSticks[1, 2].TargetPlaneTexture = "default";
        expanDialSticks[1, 2].TargetPlaneRotation = 0f;
        expanDialSticks[1, 2].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 0;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetPlaneTexture = "default";
        expanDialSticks[3, 2].TargetPlaneRotation = 0f;
        expanDialSticks[3, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 5;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;
   
        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetPlaneTexture = "candy";
        expanDialSticks[3, 3].TargetPlaneRotation = 0f;
        expanDialSticks[3, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 5;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetPlaneTexture = "default";
        expanDialSticks[3, 4].TargetPlaneRotation = 0f;
        expanDialSticks[3, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetPlaneTexture = "default";
        expanDialSticks[2, 4].TargetPlaneRotation = 0f;
        expanDialSticks[2, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 5;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = "";
        expanDialSticks[2, 3].TargetPlaneTexture = "default";
        expanDialSticks[2, 3].TargetPlaneRotation = 0f;
        expanDialSticks[2, 3].TargetColor = new Color(0f,0.6f,0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 10;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,0.3f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 15;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 1].TargetText = "";
        expanDialSticks[2, 1].TargetPlaneTexture = "snakehead";
        expanDialSticks[2, 1].TargetPlaneRotation =  -90f;
        expanDialSticks[2, 1].TargetColor = new Color(0f,0.6f,0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 10;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void Snake18(){
        // Snake
        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 0;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetPlaneTexture = "default";
        expanDialSticks[3, 2].TargetPlaneRotation = 0f;
        expanDialSticks[3, 2].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 0;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetPlaneTexture = "candy";
        expanDialSticks[3, 3].TargetPlaneRotation = 0f;
        expanDialSticks[3, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 5;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;
   
        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetPlaneTexture = "default";
        expanDialSticks[3, 4].TargetPlaneRotation = 0f;
        expanDialSticks[3, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetPlaneTexture = "default";
        expanDialSticks[2, 4].TargetPlaneRotation = 0f;
        expanDialSticks[2, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 5;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = "";
        expanDialSticks[2, 3].TargetPlaneTexture = "default";
        expanDialSticks[2, 3].TargetPlaneRotation = 0f;
        expanDialSticks[2, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 5;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetText = "";
        expanDialSticks[2, 1].TargetPlaneTexture = "candy";
        expanDialSticks[2, 1].TargetPlaneRotation = 0f;
        expanDialSticks[2, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 5;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetPlaneTexture = "snakehead";
        expanDialSticks[1, 1].TargetPlaneRotation =  0f;
        expanDialSticks[1, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

void Snake19(){
        // Snake
        expanDialSticks[3, 2].TargetText = "";
        expanDialSticks[3, 2].TargetPlaneTexture = "default";
        expanDialSticks[3, 2].TargetPlaneRotation = 0f;
        expanDialSticks[3, 2].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 0;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetPlaneTexture = "default";
        expanDialSticks[3, 3].TargetPlaneRotation = 0f;
        expanDialSticks[3, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 5;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetPlaneTexture = "default";
        expanDialSticks[3, 4].TargetPlaneRotation = 0f;
        expanDialSticks[3, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;
   
        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetPlaneTexture = "default";
        expanDialSticks[2, 4].TargetPlaneRotation = 0f;
        expanDialSticks[2, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 5;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = "";
        expanDialSticks[2, 3].TargetPlaneTexture = "default";
        expanDialSticks[2, 3].TargetPlaneRotation = 0f;
        expanDialSticks[2, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 5;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetText = "";
        expanDialSticks[2, 1].TargetPlaneTexture = "candy";
        expanDialSticks[2, 1].TargetPlaneRotation = 0f;
        expanDialSticks[2, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 5;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetPlaneTexture = "default";
        expanDialSticks[1, 1].TargetPlaneRotation = 0f;
        expanDialSticks[1, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[0, 1].TargetText = "";
        expanDialSticks[0, 1].TargetPlaneTexture = "snakehead";
        expanDialSticks[0, 1].TargetPlaneRotation =  0f;
        expanDialSticks[0, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 1].TargetPosition = 5;
        expanDialSticks[0, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

void Snake20(){
        // Snake

        expanDialSticks[3, 3].TargetText = "";
        expanDialSticks[3, 3].TargetPlaneTexture = "default";
        expanDialSticks[3, 3].TargetPlaneRotation = 0f;
        expanDialSticks[3, 3].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 0;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetPlaneTexture = "default";
        expanDialSticks[3, 4].TargetPlaneRotation = 0f;
        expanDialSticks[3, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 5;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetPlaneTexture = "default";
        expanDialSticks[2, 4].TargetPlaneRotation = 0f;
        expanDialSticks[2, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 5;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;
   
        expanDialSticks[2, 3].TargetText = "";
        expanDialSticks[2, 3].TargetPlaneTexture = "default";
        expanDialSticks[2, 3].TargetPlaneRotation = 0f;
        expanDialSticks[2, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 5;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetText = "";
        expanDialSticks[2, 1].TargetPlaneTexture = "candy";
        expanDialSticks[2, 1].TargetPlaneRotation = 0f;
        expanDialSticks[2, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 5;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetPlaneTexture = "default";
        expanDialSticks[1, 1].TargetPlaneRotation = 0f;
        expanDialSticks[1, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 1].TargetText = "";
        expanDialSticks[0, 1].TargetPlaneTexture = "default";
        expanDialSticks[0, 1].TargetPlaneRotation = 0f;
        expanDialSticks[0, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 1].TargetPosition = 5;
        expanDialSticks[0, 1].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[0, 2].TargetText = "";
        expanDialSticks[0, 2].TargetPlaneTexture = "snakehead";
        expanDialSticks[0, 2].TargetPlaneRotation =  90f;
        expanDialSticks[0, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 5;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

void Snake21(){
        // Snake


        expanDialSticks[3, 4].TargetText = "";
        expanDialSticks[3, 4].TargetPlaneTexture = "default";
        expanDialSticks[3, 4].TargetPlaneRotation = 0f;
        expanDialSticks[3, 4].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 0;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetPlaneTexture = "default";
        expanDialSticks[2, 4].TargetPlaneRotation = 0f;
        expanDialSticks[2, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 5;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 4].TargetText = "";
        expanDialSticks[2, 4].TargetPlaneTexture = "default";
        expanDialSticks[2, 4].TargetPlaneRotation = 0f;
        expanDialSticks[2, 4].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 5;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetText = "";
        expanDialSticks[2, 3].TargetPlaneTexture = "default";
        expanDialSticks[2, 3].TargetPlaneRotation = 0f;
        expanDialSticks[2, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 5;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;
   
        expanDialSticks[2, 2].TargetText = "";
        expanDialSticks[2, 2].TargetPlaneTexture = "default";
        expanDialSticks[2, 2].TargetPlaneRotation = 0f;
        expanDialSticks[2, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 5;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 1].TargetText = "";
        expanDialSticks[2, 1].TargetPlaneTexture = "candy";
        expanDialSticks[2, 1].TargetPlaneRotation = 0f;
        expanDialSticks[2, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[2, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 1].TargetPosition = 5;
        expanDialSticks[2, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 1].TargetText = "";
        expanDialSticks[1, 1].TargetPlaneTexture = "default";
        expanDialSticks[1, 1].TargetPlaneRotation = 0f;
        expanDialSticks[1, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 5;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 1].TargetText = "";
        expanDialSticks[0, 1].TargetPlaneTexture = "default";
        expanDialSticks[0, 1].TargetPlaneRotation = 0f;
        expanDialSticks[0, 1].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 1].TargetPosition = 5;
        expanDialSticks[0, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[0, 2].TargetText = "";
        expanDialSticks[0, 2].TargetPlaneTexture = "default";
        expanDialSticks[0, 2].TargetPlaneRotation = 0f;
        expanDialSticks[0, 2].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 5;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[0, 3].TargetText = "";
        expanDialSticks[0, 3].TargetPlaneTexture = "snakehead";
        expanDialSticks[0, 3].TargetPlaneRotation =  90f;
        expanDialSticks[0, 3].TargetColor = new Color(0f,1f,0f);
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 5;
        expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;

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
