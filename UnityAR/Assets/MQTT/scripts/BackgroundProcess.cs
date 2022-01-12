using UnityEngine;
using System.Threading;
using System.Collections;
using System.Timers;

public class BackgroundProcess: MonoBehaviour
{

    // Thread client_thread_;
    // private Object thisLock_ = new Object();
    // bool stop_thread_ = false;

    // void Start()
    // {
    //     Debug.Log("Start a request thread.");
    //     client_thread_ = new Thread(NetMQClient);
    //     client_thread_.Start();
    // }

    // // Client thread which does not block Update()
    // void NetMQClient()
    // {
    //     AsyncIO.ForceDotNet.Force();
    //     NetMQConfig.Cleanup();
    //     using (var client = new RequestSocket())
    //     {
    //         client.Connect("tcp://localhost:5555");
    //         for (int i = 0; i < 10; i++)
    //         {
    //             Debug.Log("Sending Hello");
    //             client.SendFrame("Hello");
    //             Thread.Sleep(100);
    //             //var message = client.ReceiveFrameString();
    //             //Debug.Log("Received " + message);
    //         }
    //     }

    //     // string topic = "KV6XML";
    //     // using (var subSocket = new SubscriberSocket())
    //     // {
    //     //     subSocket.Options.ReceiveHighWatermark = 1000;
    //     //     subSocket.Connect("tcp://localhost:5555");
    //     //     subSocket.Subscribe(topic);
    //     //     Debug.Log("Subscriber socket connecting...");
    //     //     while (!stop_thread_)
    //     //     {
    //     //         Debug.Log("waiting...?");
    //     //         //   string messageTopicReceived = subSocket.ReceiveFrameString();
    //     //         //   Debug.Log(messageTopicReceived);
    //     //         string messageReceived = subSocket.ReceiveFrameString();
    //     //         Debug.Log(messageReceived);
    //     //     }
    //     //     subSocket.Disconnect("tcp://172.20.20.42:10100");
    //     // }
    //     Debug.Log("ContextTerminate.");
    //     NetMQConfig.Cleanup();
    // }

    // void Update()
    // {
    //     /// Do normal Unity stuff
    // }

    // void OnApplicationQuit()
    // {
    //     lock (thisLock_)stop_thread_ = true;
    //     client_thread_.Abort();
    //     client_thread_.Join();
    //     Debug.Log("Quit the thread.");
    // }

}