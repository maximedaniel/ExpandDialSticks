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


public class ObjectMotionControlScenario : MonoBehaviour
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
	private const int LEGEND_SIZE = 14;
    
    private const string CONTROL_TEXT = "<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Outline\"><size=10><color=\"black\"><voffset=0em>•";

	private ConcurrentQueue<string> users;
	private ConcurrentQueue<string> inputs;
	private ConcurrentQueue<Action> outputs;

    
	void EnqueueIO() 
	{
		inputs.Enqueue("4|5|" + CLICK);
		outputs.Enqueue( () => enableScanObject());
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => scan0HeightObject()))); 
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(2f, () => scan1HeightObject()))); 
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(3f, () => scan2HeightObject()))); 
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(4f, () => scan0WidthObject()))); 
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(5f, () => scan1WidthObject()))); 
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(6f, () => scan2WidthObject()))); 
		inputs.Enqueue("4|5|" + NONE); //inputs.Enqueue("4|5|" + CLICK);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(7f, () => detectObject()))); 
		inputs.Enqueue("4|5|" + CLICK);
		outputs.Enqueue( () => controlObject()); 
		inputs.Enqueue("4|5|" + TOP_BENDING);
		outputs.Enqueue( () => moveTop0Object());
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => moveTop1Object()))); 
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(2f, () => moveTop2Object()))); 
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(3f, () => moveTop3Object()))); 
        inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(4f, () => iddle0Object()))); 
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(5f, () => iddle1Object()))); 
		inputs.Enqueue("4|5|" + RIGHT_BENDING);
		outputs.Enqueue( () => moveRight0Object());
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => moveRight1Object()))); 
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(2f, () => moveRight2Object()))); 
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(3f, () => moveRight3Object()))); 
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(4f, () => iddle2Object()))); 
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(5f, () => iddle3Object()))); 
		inputs.Enqueue("4|5|" + PULL);
		outputs.Enqueue( () => moveUp0Object());
		
		inputs.Enqueue("4|5|" + LEFT_ROTATION);
		outputs.Enqueue( () => rotate0Object());
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(1f, () => rotate1Object()))); 
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(2f, () => rotate2Object()))); 
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(3f, () => rotate3Object()))); 
		inputs.Enqueue("4|5|" + NONE);
		outputs.Enqueue( () => StartCoroutine(CoroutineUtils.WaitAndDo(4f, () => rotate4Object()))); 
		inputs.Enqueue("4|5|" + PUSH);
		outputs.Enqueue( () => Reset());
	}


    void enableScanObject(){
        for(int i = 0; i < 5; i++){
                expanDialSticks[i, 0].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[i, 0].TargetTextureChangeDuration = 1f;
                expanDialSticks[i, 0].TargetPosition = 10;
                expanDialSticks[i, 0].TargetShapeChangeDuration = 1f;

                expanDialSticks[i, 5].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
                expanDialSticks[i, 5].TargetPosition = 10;
                expanDialSticks[i, 5].TargetShapeChangeDuration = 1f;
        }

        for(int j = 0; j < 6; j++){
                expanDialSticks[0, j].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[0, j].TargetTextureChangeDuration = 1f;
                expanDialSticks[0, j].TargetPosition = 10;
                expanDialSticks[0, j].TargetShapeChangeDuration = 1f;

                expanDialSticks[4, j].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[4, j].TargetTextureChangeDuration = 1f;
                expanDialSticks[4, j].TargetPosition = 10;
                expanDialSticks[4, j].TargetShapeChangeDuration = 1f;
        }

		string legend = "The system senses the size of the object through bending.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

     void scan0HeightObject(){
        for(int i = 0; i < 5; i++){
                expanDialSticks[i, 5].TargetColor = new Color(1f,1f,1f);
                expanDialSticks[i, 5].TargetTextureChangeDuration = 1f;
                expanDialSticks[i, 5].TargetPosition = 0;
                expanDialSticks[i, 5].TargetShapeChangeDuration = 1f;
        }
        
        for(int i = 0; i < 5; i++){
            
                expanDialSticks[i, 3].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[i, 3].TargetTextureChangeDuration = 1f;
                expanDialSticks[i, 3].TargetPosition = 5;
                expanDialSticks[i, 3].TargetShapeChangeDuration = 1f;

                expanDialSticks[i, 4].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[i, 4].TargetTextureChangeDuration = 1f;
                expanDialSticks[i, 4].TargetPosition = 15;
                expanDialSticks[i, 4].TargetShapeChangeDuration = 1f;
        }
        
        expanDialSticks[0, 3].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 10;
        expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[4, 3].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[4, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 3].TargetPosition = 10;
        expanDialSticks[4, 3].TargetShapeChangeDuration = 1f;
        
		string legend = "The system senses the size of the object through bending.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

     void scan1HeightObject(){
        for(int i = 0; i < 5; i++){
                expanDialSticks[i, 4].TargetColor = new Color(1f,1f,1f);
                expanDialSticks[i, 4].TargetTextureChangeDuration = 1f;
                expanDialSticks[i, 4].TargetPosition = 0;
                expanDialSticks[i, 4].TargetShapeChangeDuration = 1f;
        }
        
        for(int i = 0; i < 5; i++){
                expanDialSticks[i, 2].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[i, 2].TargetTextureChangeDuration = 1f;
                expanDialSticks[i, 2].TargetPosition = 5;
                expanDialSticks[i, 2].TargetShapeChangeDuration = 1f;

                expanDialSticks[i, 3].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[i, 3].TargetTextureChangeDuration = 1f;
                expanDialSticks[i, 3].TargetPosition = 15;
                expanDialSticks[i, 3].TargetShapeChangeDuration = 1f;
        }

        expanDialSticks[0, 2].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 10;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[4, 2].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[4, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 2].TargetPosition = 10;
        expanDialSticks[4, 2].TargetShapeChangeDuration = 1f;

        // green detection
        
        for(int i = 1; i < 4; i++){
            expanDialSticks[i, 0].TargetColor = new Color(0f,1f,0f);
            expanDialSticks[i, 0].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 0].TargetPosition = 10;
            expanDialSticks[i, 0].TargetShapeChangeDuration = 1f;

        }
        
		string legend = "The system senses the size of the object through bending.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    } 

    void scan2HeightObject(){

        for(int i = 1; i < 4; i++){
            expanDialSticks[i, 2].TargetColor = new Color(1f,1f,1f);
            expanDialSticks[i, 2].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 2].TargetPosition = 0;
            expanDialSticks[i, 2].TargetShapeChangeDuration = 1f;
            
            expanDialSticks[i, 3].TargetColor = new Color(0f,0f,0f);
            expanDialSticks[i, 3].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 3].TargetPosition = 10;
            expanDialSticks[i, 3].TargetShapeChangeDuration = 1f;

        }
        expanDialSticks[0, 3].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 10;
        expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[4, 3].TargetColor = new Color(0f,0f,0f);
        expanDialSticks[4, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 3].TargetPosition = 10;
        expanDialSticks[4, 3].TargetShapeChangeDuration = 1f;

		string legend = "The system senses the size of the object through bending.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void scan0WidthObject(){

        for(int j = 1; j < 3; j++){
            expanDialSticks[1, j].TargetColor = new Color(0f,0f,0f);
            expanDialSticks[1, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[1, j].TargetPosition = 5;
            expanDialSticks[1, j].TargetShapeChangeDuration = 1f;
        }
        
		string legend = "The system senses the size of the object through bending.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
    

    void scan1WidthObject(){

        for(int j = 0; j < 4; j++){
            expanDialSticks[0, j].TargetColor = new Color(1f,1f,1f);
            expanDialSticks[0, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[0, j].TargetPosition = 0;
            expanDialSticks[0, j].TargetShapeChangeDuration = 1f;

            expanDialSticks[1, j].TargetColor = new Color(0f,0f,0f);
            expanDialSticks[1, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[1, j].TargetPosition = 15;
            expanDialSticks[1, j].TargetShapeChangeDuration = 1f;
        }

        for(int j = 1; j < 3; j++){
            expanDialSticks[2, j].TargetColor = new Color(0f,0f,0f);
            expanDialSticks[2, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[2, j].TargetPosition = 5;
            expanDialSticks[2, j].TargetShapeChangeDuration = 1f;
        }
        
        

        // green detection
        
            expanDialSticks[4, 1].TargetColor = new Color(0f,1f,0f);
            expanDialSticks[4, 1].TargetTextureChangeDuration = 1f;
            expanDialSticks[4, 1].TargetPosition = 10;
            expanDialSticks[4, 1].TargetShapeChangeDuration = 1f;

            expanDialSticks[4, 2].TargetColor = new Color(0f,1f,0f);
            expanDialSticks[4, 2].TargetTextureChangeDuration = 1f;
            expanDialSticks[4, 2].TargetPosition = 10;
            expanDialSticks[4, 2].TargetShapeChangeDuration = 1f;
        
		string legend = "The system senses the size of the object through bending.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
    

    void scan2WidthObject(){

        for(int j = 0; j < 4; j++){
            expanDialSticks[1, j].TargetColor = new Color(0f,0f,0f);
            expanDialSticks[1, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[1, j].TargetPosition = 10;
            expanDialSticks[1, j].TargetShapeChangeDuration = 1f;
        }


        for(int j = 1; j < 3; j++){
            expanDialSticks[2, j].TargetColor = new Color(1f,1f,1f);
            expanDialSticks[2, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[2, j].TargetPosition = 0;
            expanDialSticks[2, j].TargetShapeChangeDuration = 1f;
        }
        
		string legend = "The system senses the size of the object through bending.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
    
    

    void detectObject(){
        for(int i = 2; i < 4; i++){
            for(int j = 0; j <4; j++){
                expanDialSticks[1, j].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[1, j].TargetTextureChangeDuration = 1f;
                expanDialSticks[1, j].TargetPosition = 10;
                expanDialSticks[1, j].TargetShapeChangeDuration = 1f;

                expanDialSticks[2, 0].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
                expanDialSticks[2, 0].TargetPosition = 10;
                expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

                expanDialSticks[3, 0].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
                expanDialSticks[3, 0].TargetPosition = 10;
                expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;
                
                expanDialSticks[2, 3].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
                expanDialSticks[2, 3].TargetPosition = 10;
                expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

                expanDialSticks[3, 3].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
                expanDialSticks[3, 3].TargetPosition = 10;
                expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;
                
                expanDialSticks[4, j].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[4, j].TargetTextureChangeDuration = 1f;
                expanDialSticks[4, j].TargetPosition = 10;
                expanDialSticks[4, j].TargetShapeChangeDuration = 1f;
            }
        }
        
        // object

        for(int i = 2; i < 4; i++){
            for(int j = 1; j <3; j++){
                expanDialSticks[i, j].TargetColor = new Color(0f,1f,0f);
                expanDialSticks[i, j].TargetTextureChangeDuration = 1f;
            }
        }
        
        
		string legend = "The system senses the size of the object through bending.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
    
    void constraintControlObject(){
        //constraint
        for(int i = 2; i < 4; i++){
            for(int j = 0; j <4; j++){
                expanDialSticks[1, j].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[1, j].TargetTextureChangeDuration = 1f;
                expanDialSticks[1, j].TargetPosition = 15;
                expanDialSticks[1, j].TargetShapeChangeDuration = 1f;

                expanDialSticks[2, 0].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
                expanDialSticks[2, 0].TargetPosition = 15;
                expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

                expanDialSticks[3, 0].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
                expanDialSticks[3, 0].TargetPosition = 15;
                expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;
                
                expanDialSticks[2, 3].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
                expanDialSticks[2, 3].TargetPosition = 15;
                expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

                expanDialSticks[3, 3].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
                expanDialSticks[3, 3].TargetPosition = 15;
                expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;
                
                expanDialSticks[4, j].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[4, j].TargetTextureChangeDuration = 1f;
                expanDialSticks[4, j].TargetPosition = 15;
                expanDialSticks[4, j].TargetShapeChangeDuration = 1f;
            }
        }
        
		string legend = "The user clicks a pin to call forth a controller.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void controlObject(){
         //constraint
        for(int i = 2; i < 4; i++){
            for(int j = 0; j <4; j++){
                expanDialSticks[1, j].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[1, j].TargetTextureChangeDuration = 1f;
                expanDialSticks[1, j].TargetPosition = 15;
                expanDialSticks[1, j].TargetShapeChangeDuration = 1f;

                expanDialSticks[2, 0].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
                expanDialSticks[2, 0].TargetPosition = 15;
                expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

                expanDialSticks[3, 0].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
                expanDialSticks[3, 0].TargetPosition = 15;
                expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;
                
                expanDialSticks[2, 3].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
                expanDialSticks[2, 3].TargetPosition = 15;
                expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

                expanDialSticks[3, 3].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
                expanDialSticks[3, 3].TargetPosition = 15;
                expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;
                
                expanDialSticks[4, j].TargetColor = new Color(0f,0f,0f);
                expanDialSticks[4, j].TargetTextureChangeDuration = 1f;
                expanDialSticks[4, j].TargetPosition = 15;
                expanDialSticks[4, j].TargetShapeChangeDuration = 1f;
            }
        }

        //green
        for(int i = 2; i < 4; i++){
            for(int j = 1; j <3; j++){
                expanDialSticks[i, j].TargetColor = new Color(0f,1f,0f);
                expanDialSticks[i, j].TargetTextureChangeDuration = 1f;
                expanDialSticks[i, j].TargetPosition = 5;
                expanDialSticks[i, j].TargetShapeChangeDuration = 1f;
            }
        }

        expanDialSticks[4, 5].TargetText = CONTROL_TEXT;
        expanDialSticks[4, 5].TargetColor = new Color(0f,0.6f,0f);
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 30;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;

        
		string legend = "The user clicks a pin to call forth a controller.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
void unconstraintControlObject(){
        //constraint
        for(int i = 2; i < 4; i++){
            for(int j = 0; j <4; j++){
                expanDialSticks[1, j].TargetColor = new Color(1f,1f,1f);
                expanDialSticks[1, j].TargetTextureChangeDuration = 1f;
                expanDialSticks[1, j].TargetPosition = 0;
                expanDialSticks[1, j].TargetShapeChangeDuration = 1f;

                expanDialSticks[2, 0].TargetColor = new Color(1f,1f,1f);
                expanDialSticks[2, 0].TargetTextureChangeDuration = 1f;
                expanDialSticks[2, 0].TargetPosition = 0;
                expanDialSticks[2, 0].TargetShapeChangeDuration = 1f;

                expanDialSticks[3, 0].TargetColor = new Color(1f,1f,1f);
                expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
                expanDialSticks[3, 0].TargetPosition = 0;
                expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;
                
                expanDialSticks[2, 3].TargetColor = new Color(1f,1f,1f);
                expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
                expanDialSticks[2, 3].TargetPosition = 0;
                expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

                expanDialSticks[3, 3].TargetColor = new Color(1f,1f,1f);
                expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
                expanDialSticks[3, 3].TargetPosition = 0;
                expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;
                
                expanDialSticks[4, j].TargetColor = new Color(1f,1f,1f);
                expanDialSticks[4, j].TargetTextureChangeDuration = 1f;
                expanDialSticks[4, j].TargetPosition = 0;
                expanDialSticks[4, j].TargetShapeChangeDuration = 1f;
            }
        }
        
        
		string legend = "The user clicks a pin to call forth a controller.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
    void moveTop0Object(){

        // Constraints 
        for(int j = 0; j <4; j++){
            expanDialSticks[0, j].TargetColor = new Color(0f, 0f, 0f);
            expanDialSticks[0, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[0, j].TargetPosition = 15;
            expanDialSticks[0, j].TargetShapeChangeDuration = 1f;
        }
        
        for(int i = 1; i <4; i++){
            expanDialSticks[i, 0].TargetColor = new Color(0f, 0f, 0f);
            expanDialSticks[i, 0].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 0].TargetPosition = 15;
            expanDialSticks[i, 0].TargetShapeChangeDuration = 1f;

            expanDialSticks[i, 3].TargetColor = new Color(0f, 0f, 0f);
            expanDialSticks[i, 3].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 3].TargetPosition = 15;
            expanDialSticks[i, 3].TargetShapeChangeDuration = 1f;
        }
        
        expanDialSticks[1, 1].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 1].TargetPosition = 0;
        expanDialSticks[1, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetColor = new Color(1f,1f,1f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 0;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;
        
		string legend = "The user bends the pin forward to move the object forward.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


    void moveTop1Object(){

        for(int j = 1; j <3; j++){
            expanDialSticks[4, j].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[4, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[4, j].TargetPosition = 15;
            expanDialSticks[4, j].TargetShapeChangeDuration = 1f;
        }
        
		string legend = "The user bends the pin forward to move the object forward.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

    }

    void moveTop2Object(){
        for(int j = 1; j <3; j++){
            expanDialSticks[4, j].TargetColor = new Color(1f, 1f, 1f);
            expanDialSticks[4, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[4, j].TargetPosition = 0;
            expanDialSticks[4, j].TargetShapeChangeDuration = 1f;

            expanDialSticks[3, j].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[3, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[3, j].TargetPosition = 15;
            expanDialSticks[3, j].TargetShapeChangeDuration = 1f;

            expanDialSticks[2, j].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[2, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[2, j].TargetPosition = 5;
            expanDialSticks[2, j].TargetShapeChangeDuration = 1f;
        }
        
		string legend = "The user bends the pin forward to move the object forward.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void moveTop3Object(){
        for(int j = 1; j <3; j++){
            expanDialSticks[3, j].TargetColor = new Color(1f, 1f, 1f);
            expanDialSticks[3, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[3, j].TargetPosition = 0;
            expanDialSticks[3, j].TargetShapeChangeDuration = 1f;

            expanDialSticks[2, j].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[2, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[2, j].TargetPosition = 15;
            expanDialSticks[2, j].TargetShapeChangeDuration = 1f;

            expanDialSticks[1, j].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[1, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[1, j].TargetPosition = 5;
            expanDialSticks[1, j].TargetShapeChangeDuration = 1f;
        }
        
		string legend = "The user bends the pin forward to move the object forward.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void iddle0Object(){
        expanDialSticks[3, 1].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 1].TargetPosition = 15;
        expanDialSticks[3, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 15;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;

        for(int j = 1; j <3; j++){
            expanDialSticks[2, j].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[2, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[2, j].TargetPosition = 5;
            expanDialSticks[2, j].TargetShapeChangeDuration = 1f;

        }
        
		string legend = "The user bends the pin forward to move the object forward.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void iddle1Object(){
        expanDialSticks[4, 0].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[4, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 0].TargetPosition = 0;
        expanDialSticks[4, 0].TargetShapeChangeDuration = 1f;

        expanDialSticks[4, 3].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[4, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 3].TargetPosition = 0;
        expanDialSticks[4, 3].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[3, 1].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 1].TargetPosition = 15;
        expanDialSticks[3, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 15;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;
        
		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

    }

    void moveRight0Object(){
        
        expanDialSticks[1, 3].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 0;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 0;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        // Constaints
        for(int j = 1; j <6; j++){
            expanDialSticks[0, j].TargetColor = new Color(0f, 0f, 0f);
            expanDialSticks[0, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[0, j].TargetPosition = 15;
            expanDialSticks[0, j].TargetShapeChangeDuration = 1f;
            
            expanDialSticks[3, j].TargetColor = new Color(0f, 0f, 0f);
            expanDialSticks[3, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[3, j].TargetPosition = 15;
            expanDialSticks[3, j].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks[1, 5].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 5].TargetPosition = 15;
        expanDialSticks[1, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 5].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 15;
        expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;
        
		string legend = "The user bends the pin right to move the object right.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

    }


    void moveRight1Object(){
        
        for(int i = 1; i <3; i++){
            expanDialSticks[i, 0].TargetColor = new Color(1f, 1f, 1f);
            expanDialSticks[i, 0].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 0].TargetPosition = 0;
            expanDialSticks[i, 0].TargetShapeChangeDuration = 1f;

            expanDialSticks[i, 1].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[i, 1].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 1].TargetPosition = 15;
            expanDialSticks[i, 1].TargetShapeChangeDuration = 1f;

            expanDialSticks[i, 2].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[i, 2].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 2].TargetPosition = 5;
            expanDialSticks[i, 2].TargetShapeChangeDuration = 1f;
        }
        
		string legend = "The user bends the pin right to move the object right.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

    }


    void moveRight2Object(){
        for(int i = 1; i <3; i++){
            expanDialSticks[i, 1].TargetColor = new Color(1f, 1f, 1f);
            expanDialSticks[i, 1].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 1].TargetPosition = 0;
            expanDialSticks[i, 1].TargetShapeChangeDuration = 1f;

            expanDialSticks[i, 2].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[i, 2].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 2].TargetPosition = 15;
            expanDialSticks[i, 2].TargetShapeChangeDuration = 1f;

            expanDialSticks[i, 3].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[i, 3].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 3].TargetPosition = 5;
            expanDialSticks[i, 3].TargetShapeChangeDuration = 1f;
        }
        
		string legend = "The user bends the pin right to move the object right.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

    }

    void moveRight3Object(){
        expanDialSticks[1, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 15;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 15;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;
        for(int i = 1; i <3; i++){
            expanDialSticks[i, 2].TargetColor = new Color(1f, 1f, 1f);
            expanDialSticks[i, 2].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 2].TargetPosition = 0;
            expanDialSticks[i, 2].TargetShapeChangeDuration = 1f;

            expanDialSticks[i, 3].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[i, 3].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 3].TargetPosition = 15;
            expanDialSticks[i, 3].TargetShapeChangeDuration = 1f;

            expanDialSticks[i, 4].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[i, 4].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 4].TargetPosition = 5;
            expanDialSticks[i, 4].TargetShapeChangeDuration = 1f;
        }
        
		string legend = "The user bends the pin right to move the object right.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void iddle2Object(){
        //constraints
        expanDialSticks[1, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 15;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 15;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;


        for(int i = 1; i <3; i++){
            expanDialSticks[i, 3].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[i, 3].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 3].TargetPosition = 5;
            expanDialSticks[i, 3].TargetShapeChangeDuration = 1f;
            
            expanDialSticks[i, 4].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[i, 4].TargetTextureChangeDuration = 1f;
            expanDialSticks[i, 4].TargetPosition = 5;
            expanDialSticks[i, 4].TargetShapeChangeDuration = 1f;
        }
        
		string legend = "The user bends the pin right to move the object right.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void iddle3Object(){
        
        //unconstraints
        expanDialSticks[0, 0].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[0, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 0].TargetPosition = 0;
        expanDialSticks[0, 0].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[0, 1].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[0, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 1].TargetPosition = 0;
        expanDialSticks[0, 1].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 0].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[3, 0].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 0].TargetPosition = 0;
        expanDialSticks[3, 0].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[3, 1].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[3, 1].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 1].TargetPosition = 0;
        expanDialSticks[3, 1].TargetShapeChangeDuration = 1f;

        /*for(int j = 1; j <6; j++){
            expanDialSticks[0, j].TargetColor = new Color(1f, 1f, 1f);
            expanDialSticks[0, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[0, j].TargetPosition = 0;
            expanDialSticks[0, j].TargetShapeChangeDuration = 1f;
            
            expanDialSticks[3, j].TargetColor = new Color(1f, 1f, 1f);
            expanDialSticks[3, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[3, j].TargetPosition = 0;
            expanDialSticks[3, j].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks[1, 5].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[1, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 5].TargetPosition = 0;
        expanDialSticks[1, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 5].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 0;
        expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;*/

        
		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }
     void constraintMoveUp0Object(){

        // left side
        expanDialSticks[0, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 25;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 25;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 25;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 25;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;


        // right side
        expanDialSticks[0, 5].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[0, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 5].TargetPosition = 25;
        expanDialSticks[0, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 5].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 5].TargetPosition = 25;
        expanDialSticks[1, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 5].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 25;
        expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 5].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 5].TargetPosition = 25;
        expanDialSticks[3, 5].TargetShapeChangeDuration = 1f;

        //top side

        expanDialSticks[0, 3].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 25;
        expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;
       
        expanDialSticks[0, 4].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[0, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 4].TargetPosition = 25;
        expanDialSticks[0, 4].TargetShapeChangeDuration = 1f;

        //bottom side

        expanDialSticks[3, 3].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 25;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;
       
        expanDialSticks[3, 4].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 25;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

		string legend = "The user pulls the pin up to move the object up.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void moveUp0Object(){
        //constraints
         // left side
        expanDialSticks[0, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 25;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 25;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 25;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 25;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;


        // right side
        expanDialSticks[0, 5].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[0, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 5].TargetPosition = 25;
        expanDialSticks[0, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 5].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 5].TargetPosition = 25;
        expanDialSticks[1, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 5].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 25;
        expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 5].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 5].TargetPosition = 25;
        expanDialSticks[3, 5].TargetShapeChangeDuration = 1f;

        //top side

        expanDialSticks[0, 3].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 25;
        expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;
       
        expanDialSticks[0, 4].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[0, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 4].TargetPosition = 25;
        expanDialSticks[0, 4].TargetShapeChangeDuration = 1f;

        //bottom side

        expanDialSticks[3, 3].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 25;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;
       
        expanDialSticks[3, 4].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 25;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

        // control
        expanDialSticks[1, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 15;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;

        
        expanDialSticks[1, 4].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 15;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 15;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

        
        expanDialSticks[2, 4].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 15;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        
		string legend = "The user pulls the pin up to move the object up.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

     void unconstraintMoveUp0Object(){

        // left side
        expanDialSticks[0, 2].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[0, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 2].TargetPosition = 0;
        expanDialSticks[0, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 0;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 0;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 2].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[3, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 2].TargetPosition = 0;
        expanDialSticks[3, 2].TargetShapeChangeDuration = 1f;


        // right side
        expanDialSticks[0, 5].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[0, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 5].TargetPosition = 0;
        expanDialSticks[0, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 5].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[1, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 5].TargetPosition = 0;
        expanDialSticks[1, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 5].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 0;
        expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[3, 5].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[3, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 5].TargetPosition = 0;
        expanDialSticks[3, 5].TargetShapeChangeDuration = 1f;

        //top side

        expanDialSticks[0, 3].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[0, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 3].TargetPosition = 0;
        expanDialSticks[0, 3].TargetShapeChangeDuration = 1f;
       
        expanDialSticks[0, 4].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[0, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[0, 4].TargetPosition = 0;
        expanDialSticks[0, 4].TargetShapeChangeDuration = 1f;

        //bottom side

        expanDialSticks[3, 3].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[3, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 3].TargetPosition = 0;
        expanDialSticks[3, 3].TargetShapeChangeDuration = 1f;
       
        expanDialSticks[3, 4].TargetColor = new Color(1f, 1f, 1f);
        expanDialSticks[3, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[3, 4].TargetPosition = 0;
        expanDialSticks[3, 4].TargetShapeChangeDuration = 1f;

		string legend = "The user pulls the pin up to move the object up.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


    void rotateObject(){
        // Constaints
        for(int j = 2; j <6; j++){
            expanDialSticks[0, j].TargetColor = new Color(0f, 0f, 0f);
            expanDialSticks[0, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[0, j].TargetPosition = 35;
            expanDialSticks[0, j].TargetShapeChangeDuration = 1f;
            
            expanDialSticks[3, j].TargetColor = new Color(0f, 0f, 0f);
            expanDialSticks[3, j].TargetTextureChangeDuration = 1f;
            expanDialSticks[3, j].TargetPosition = 35;
            expanDialSticks[3, j].TargetShapeChangeDuration = 1f;
        }
        expanDialSticks[1, 5].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 5].TargetPosition = 35;
        expanDialSticks[1, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[1, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[1, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 2].TargetPosition = 35;
        expanDialSticks[1, 2].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 5].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[2, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 5].TargetPosition = 35;
        expanDialSticks[2, 5].TargetShapeChangeDuration = 1f;

        expanDialSticks[2, 2].TargetColor = new Color(0f, 0f, 0f);
        expanDialSticks[2, 2].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 2].TargetPosition = 35;
        expanDialSticks[2, 2].TargetShapeChangeDuration = 1f;

        
		string legend = "The user rotates the pin to elevate the object circularly.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));


        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void rotate0Object(){
        expanDialSticks[1, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 25;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;
        
		string legend = "The user rotates the pin to elevate the object circularly.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void rotate1Object(){
        expanDialSticks[1, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 3].TargetPosition = 15;
        expanDialSticks[1, 3].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 25;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;

		string legend = "The user rotates the pin to start spinning the 'bottle'.";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void rotate2Object(){
        expanDialSticks[2, 3].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 3].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 3].TargetPosition = 15;
        expanDialSticks[2, 3].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[2, 4].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 25;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void rotate3Object(){
        expanDialSticks[2, 4].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[2, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[2, 4].TargetPosition = 15;
        expanDialSticks[2, 4].TargetShapeChangeDuration = 1f;
        
        expanDialSticks[1, 4].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 25;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;
        
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void rotate4Object(){
        expanDialSticks[1, 4].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 25;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void rotate5Object(){
        expanDialSticks[1, 4].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[1, 4].TargetTextureChangeDuration = 1f;
        expanDialSticks[1, 4].TargetPosition = 15;
        expanDialSticks[1, 4].TargetShapeChangeDuration = 1f;
        
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void exitObject(){
        ClearAllChange();
        
		string legend = "";
		expanDialSticks.setLeftBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 0f, 0f));
		expanDialSticks.setRightBorderText(TextAlignmentOptions.Center, LEGEND_SIZE, Color.black, legend, new Vector3(90f, 180f, 0f));

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }


    IEnumerator UserScenario(){
		while(outputs.Count > 0){
				Action output;
				while(!outputs.TryDequeue (out output));
				Debug.Log("output > " + output);
				output();
				yield return new WaitForSeconds(2f);
		}
    }

	void DequeueOutput(){
		Action action;
		while (!outputs.TryDequeue(out action));
		action();
	}

    void test(){
        ClearAllChange();
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


	/*IEnumerator UserScenario(){
		while(outputs.Count > 0){
				Action output;
				while(!outputs.TryDequeue (out output));
				Debug.Log("output > " + output);
				output();
				yield return new WaitForSeconds(1f);
		}
	}*/
	
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
                    // current
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
					};
				};
			}
        }
    }
}
