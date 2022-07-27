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


public class ArcadeGameApp : MonoBehaviour
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

    private volatile List<Vector2> snakeBones;
    private volatile List<Vector2> candies;
    private volatile string snakeDirection;
    private volatile string nextDirection;
    private volatile bool gameover;
    private bool initialized;

    void MoveSnake()
    {
        // snakehead 
        Vector2 currHead = snakeBones[snakeBones.Count - 1];
        Vector2 nextHead = new Vector2(currHead.x, currHead.y);

        // move Snake
        switch (snakeDirection)
        {
            case TOP_BENDING:
                nextHead.x -= 1;
                break;
            case BOTTOM_BENDING:
                nextHead.x += 1;
                break;
            case LEFT_BENDING:
                nextHead.y -= 1;
                break;
            case RIGHT_BENDING:
                nextHead.y += 1;
                break;
        }
        Debug.Log(currHead);
        Debug.Log(nextHead);
        // check for border game over
        if (nextHead.x >= expanDialSticks.NbRows || nextHead.x <= 0 || nextHead.y >= expanDialSticks.NbColumns || nextHead.y <= 0)
        {
            gameover = true;
        }
        // check for body game over
        foreach (Vector2 snakeBone in snakeBones)
		{
            if (nextHead == snakeBone) gameover = true;

        }

        // remove candy
        if (candies.Count > 0)
        {
            if (nextHead == candies[0])
            {
                candies.RemoveAt(0);
            }
        }
        // remove tail and add nextHead
        snakeBones.RemoveAt(0);
        snakeBones.Add(nextHead);

    }

    void DrawSnake() { 
        // Clear Display
        ClearAllChange();

        // draw control
        expanDialSticks[4, 5].TargetText = "Move";
        expanDialSticks[4, 5].TargetColor = new Color(0f, 0f, 1f);
        expanDialSticks[4, 5].TargetTextureChangeDuration = 1f;
        expanDialSticks[4, 5].TargetPosition = 40;
        expanDialSticks[4, 5].TargetShapeChangeDuration = 1f;

        // draw bones
        for (int i = 0; i < snakeBones.Count - 1; i++)
		{
            Vector2 snakeBone = snakeBones[i];
            int xBone = (int)snakeBone.x;
            int yBone = (int)snakeBone.y;
            expanDialSticks[xBone, yBone].TargetColor = new Color(0f, 1f, 0f);
            expanDialSticks[xBone, yBone].TargetTextureChangeDuration = 1f;
            expanDialSticks[xBone, yBone].TargetPosition = 5;
            expanDialSticks[xBone, yBone].TargetShapeChangeDuration = 1f;
        }
        // draw snakehead
        Vector2 snakeHead = snakeBones[snakeBones.Count - 1];
        int xHead = (int)snakeHead.x;
        int yHead = (int)snakeHead.y;
        expanDialSticks[xHead, yHead].TargetText = "";
        expanDialSticks[xHead, yHead].TargetColor = new Color(0f, 1f, 0f);
        expanDialSticks[xHead, yHead].TargetPlaneColor = new Color(1f, 1f, 1f);
        expanDialSticks[xHead, yHead].TargetPlaneSize = 0.8f;
        expanDialSticks[xHead, yHead].TargetPlaneRotation = 90f;// OK
        expanDialSticks[xHead, yHead].TargetTextureChangeDuration = 0.1f;
        expanDialSticks[xHead, yHead].TargetPosition = 5;
        expanDialSticks[xHead, yHead].TargetShapeChangeDuration = 1f;

        // move Snake
        switch (snakeDirection)
        {
            case TOP_BENDING:
                expanDialSticks[xHead, yHead].TargetPlaneTexture = "snakehead-up";
                break;
            case BOTTOM_BENDING:
                expanDialSticks[xHead, yHead].TargetPlaneTexture = "snakehead-down";
                break;
            case LEFT_BENDING:
                expanDialSticks[xHead, yHead].TargetPlaneTexture = "snakehead-left"; 
                break;
            case RIGHT_BENDING:
                expanDialSticks[xHead, yHead].TargetPlaneTexture = "snakehead-right";
                break;
        }

        // draw candy
        if(candies.Count > 0)
        {
            Vector2 candy = candies[0];
            int xCandy = (int)candy.x;
            int yCandy = (int)candy.y;
            expanDialSticks[xCandy, yCandy].TargetColor = new Color(1f, 0.4f, 0.7f);
            expanDialSticks[xHead, yHead].TargetPlaneColor = new Color(1f, 1f, 1f);
            expanDialSticks[xCandy, yCandy].TargetPlaneTexture = "candy";
            expanDialSticks[xHead, yHead].TargetPlaneSize = 0.8f;
            expanDialSticks[xHead, yHead].TargetPlaneRotation = 90f;// OK
            expanDialSticks[xCandy, yCandy].TargetTextureChangeDuration = 1f;
            expanDialSticks[xCandy, yCandy].TargetPosition = 5;
            expanDialSticks[xCandy, yCandy].TargetShapeChangeDuration = 1f;
        }

        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();
    }

    void ClearAllChange()
    {
        for (int i = 0; i < expanDialSticks.NbRows; i++)
        {
            for (int j = 0; j < expanDialSticks.NbColumns; j++)
            {
                expanDialSticks[i, j].TargetTextAlignment = TextAlignmentOptions.Center;
                expanDialSticks[i, j].TargetTextSize = 2f;
                expanDialSticks[i, j].TargetTextColor = new Color(0f, 0f, 0f);
                expanDialSticks[i, j].TargetText = "";
                expanDialSticks[i, j].TargetColor = new Color(1f, 1f, 1f);
                expanDialSticks[i, j].TargetPlaneTexture = "default";
                expanDialSticks[i, j].TargetPlaneSize = 0.8f;
                expanDialSticks[i, j].TargetPlaneRotation = 90f;
                expanDialSticks[i, j].TargetTextureChangeDuration = 0.1f;

                expanDialSticks[i, j].TargetPosition = 0;
                expanDialSticks[i, j].TargetHolding = false;
                expanDialSticks[i, j].TargetShapeChangeDuration = 1f;
            }
        }
    }

    void ResetShape()
    {
        ClearAllChange();
        expanDialSticks.triggerTextureChange();
        expanDialSticks.triggerShapeChange();

    }
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

        snakeBones = new List<Vector2>()
        {
            new Vector2(1, 3),
            new Vector2(1, 2),
            new Vector2(1, 1),
        };
        candies = new List<Vector2>()
        {
            new Vector2(2, 1),
            new Vector2(3, 4)
        };
        gameover = false;
        snakeDirection = NONE;
        nextDirection = NONE;


        // Connection to MQTT Broker
        connected = false;
        initialized = false;
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
    }

    private void HandleDisconnected(object sender, MqttConnectionEventArgs e)
    {
        Debug.Log("ExpanDialSticks disconnected.");
        connected = false;
    }

    private void HandleXAxisChanged(object sender, ExpanDialStickEventArgs e)
    {
        //Debug.Log("HandleXAxisChanged -> (" + e.i + "|" + e.j + "|" + e.prev + "|" + e.next  + "|" + e.diff + ")");
        if (e.diff > 0 && e.next >= JOYSTICK_THRESHOLD) nextDirection = BOTTOM_BENDING;
        if (e.diff < 0 && e.next <= -JOYSTICK_THRESHOLD) nextDirection = TOP_BENDING;
    }

    private void HandleYAxisChanged(object sender, ExpanDialStickEventArgs e)
    {
        //Debug.Log("HandleYAxisChanged -> (" + e.i + "|" + e.j + "|" + e.prev + "|" + e.next  + "|" + e.diff + ")");
        if (e.diff > 0 && e.next >= JOYSTICK_THRESHOLD) nextDirection = LEFT_BENDING;
        if (e.diff < 0 && e.next <= -JOYSTICK_THRESHOLD) nextDirection = RIGHT_BENDING;
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
    IEnumerator SnakeGameCoroutine()
    {
        while (!gameover)
        {
            MoveSnake();
            DrawSnake();
            yield return new WaitForSeconds(1f);
        }
        ResetShape();
        yield return new WaitForSeconds(1f);
    }


    void Update()
    {
        // check if ExpanDialSticks is connected
        if (connected)
        {
			if (!initialized)
			{
                DrawSnake();
                initialized = true;

            }
            if (Input.GetKey("escape"))
            {
                Application.Quit();
            }

            if (Input.GetKey(KeyCode.Z))
            {
                nextDirection = TOP_BENDING;
            }

            if (Input.GetKey(KeyCode.S))
            {
                nextDirection = BOTTOM_BENDING;
            }

            if (Input.GetKey(KeyCode.D))
            {
                nextDirection = RIGHT_BENDING;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                nextDirection = LEFT_BENDING;
            }

            if (Input.GetKey("escape"))
            {
                Application.Quit();
            }

            if (snakeDirection == NONE && nextDirection != NONE)
            {
                snakeDirection = nextDirection;
                StartCoroutine("SnakeGameCoroutine");
            } else
            {
                snakeDirection = nextDirection;
            }
        }
    }
}
