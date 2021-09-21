using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;

public class Control : MonoBehaviour
{
	// Start is called before the first frame update
	private UnityEngine.Video.VideoPlayer videoPlayer;
    private bool videoReady = false;
    private bool videoStarted = false;
    private GUIStyle currentStyle = null;

    public GameObject expanDialSticksPrefab;
    private ExpanDialSticks expanDialSticks;
    private bool connected = false;
    private int progressBarHeight = 5;
    private int progressBarWidth = 0;
    private Color progressBarColor = new Color(1f, 1f, 1f, 0.2f);

    public const string MQTT_CAMERA_RECORDER = "CAMERA_RECORDER";
    public const string MQTT_EMPATICA_RECORDER = "EMPATICA_RECORDER";
    public const string MQTT_SYSTEM_RECORDER = "SYSTEM_RECORDER";
    public const string CMD_START = "START";
    public const string CMD_STOP = "STOP";
    void Start()
    {   
        // Will attach a VideoPlayer to the main camera.
        GameObject camera = GameObject.Find("Main Camera");

        // VideoPlayer automatically targets the camera backplane when it is added
        // to a camera object, no need to change videoPlayer.targetCamera.
        videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();

        // Play on awake defaults to true. Set it to false to avoid the url set
        // below to auto-start playback since we're in Start().
        videoPlayer.playOnAwake = false;

        // By default, VideoPlayers added to a camera will use the far plane.
        // Let's target the near plane instead.
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;

        // This will cause our Scene to be visible through the video being played.
        videoPlayer.targetCameraAlpha = 1F;

        // Set the video to play. URL supports local absolute or relative paths.
        // Here, using absolute.
        UnityEngine.Video.VideoClip clip = Resources.Load<UnityEngine.Video.VideoClip>("relaxing-3min");

        videoPlayer.clip = clip;

        // Skip the first 100 frames.
        videoPlayer.frame = 0;

        // Restart from beginning when done.
        videoPlayer.isLooping = false;

        // Each time we reach the end, we slow down the playback by a factor of 10.
        videoPlayer.loopPointReached += EndReached;
        videoPlayer.prepareCompleted += PrepareCompleted;

        // Start playback. This means the VideoPlayer may have to prepare (reserve
        // resources, pre-load a few frames, etc.). To better control the delays
        // associated with this preparation one can use videoPlayer.Prepare() along with
        // its prepareCompleted event.
        videoReady = false;
        videoPlayer.Prepare();
        // Preparing MQTT broker

        expanDialSticks = expanDialSticksPrefab.GetComponent<ExpanDialSticks>();
        // Listen to events
        expanDialSticks.OnConnecting += HandleConnecting;
        expanDialSticks.OnConnected += HandleConnected;
        expanDialSticks.OnDisconnected += HandleDisconnected;

        connected = false;
        // Connection to MQTT Broker
        expanDialSticks.client_MqttConnect();
    }

    private void HandleConnecting(object sender, MqttConnectionEventArgs e)
    {
        Debug.Log("Application connecting to MQTT Broker @" + e.address + ":" + e.port + "...");
        connected = false;
    }

    private void HandleConnected(object sender, MqttConnectionEventArgs e)
    {
        Debug.Log("Application connected.");
        connected = true;

    }

    private void HandleDisconnected(object sender, MqttConnectionEventArgs e)
    {
        Debug.Log("Application disconnected.");
        connected = false;
    }


    void PrepareCompleted(UnityEngine.Video.VideoPlayer vp)
    {
        //Debug.Log("PrepareCompleted");
        videoReady = true;
    }
    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        //Debug.Log("EndReached");
        Quit();
        //Debug.Log(CMD_STOP);
        //expanDialSticks.client.Publish(MQTT_EMPATICA_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_STOP), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
        // STOP RECORDING
        //vp.playbackSpeed = vp.playbackSpeed / 10.0F;
    }
    private void OnDestroy()
    {
        Debug.Log(CMD_STOP);
        expanDialSticks.client.Publish(MQTT_EMPATICA_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_STOP), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
    }


    private void OnGUI()
    {
        if (connected && videoReady)
        {
			if (!videoPlayer.isPlaying)
            {
                GUI.color = Color.white;
                if (!videoStarted && GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 25, 100, 50), "START"))
                {

                    Debug.Log(CMD_START);
                    expanDialSticks.client.Publish(MQTT_EMPATICA_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_START), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                    videoPlayer.Play();
                    videoStarted = true;
                }
            } else
            {
                InitStyles();
                progressBarWidth = (int)((videoPlayer.frame / (double)videoPlayer.frameCount) * Screen.width);
                GUI.Box(new Rect(0, Screen.height - progressBarHeight, progressBarWidth, progressBarHeight), "", currentStyle);
            }
        } else
		{
            GUI.color = new Color(1f, 1f, 1f, Mathf.PingPong(Time.time, 1f));
            GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 25, 100, 50), "LOADING...");
        }
    }
    private void InitStyles()
    {
        if (currentStyle == null)
        {
            currentStyle = new GUIStyle(GUI.skin.box);
            currentStyle.normal.background = MakeTex(2, 2, progressBarColor);
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }


    // Update is called once per frame
    void Update()
    {
        if (connected)
        {
            if (videoReady && !videoPlayer.isPlaying && Input.GetKey(KeyCode.Space))
                {
                    Debug.Log(CMD_START);
                    expanDialSticks.client.Publish(MQTT_EMPATICA_RECORDER, System.Text.Encoding.UTF8.GetBytes(CMD_START), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                    videoPlayer.Play();
                    videoStarted = true;
                }

                if (Input.GetKey(KeyCode.Escape))
            {
                Quit();
            }
        }
    }

    void Quit()
    {
    #if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
    #else
						    Application.Quit();
    #endif
    }
}
