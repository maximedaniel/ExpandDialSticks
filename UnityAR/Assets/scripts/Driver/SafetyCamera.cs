using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafetyCamera : MonoBehaviour
{

    private Camera cam;
    private Projector proj;
    private RenderTexture rt;
   // public GameObject DebugTextureObject;
    private int rtSize = 1024;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        proj = GetComponent<Projector>();
        rt = new RenderTexture(rtSize, rtSize, 16, RenderTextureFormat.ARGB32);
        rt.Create();
        cam.targetTexture = rt;
        proj.material.mainTexture = rt;
        /*if(DebugTextureObject != null)
        {
            DebugTextureObject.GetComponent<MeshRenderer>().material.mainTexture = rt;
        }*/
    }

    public void TakeCameraShot()
    {
        Texture2D shot = new Texture2D(rtSize, rtSize, TextureFormat.ARGB32, false);
        RenderTexture.active = rt;
        // false, meaning no need for mipmaps
        shot.ReadPixels(new Rect(0, 0, rtSize, rtSize), 0, 0);
        shot.Apply();
       RenderTexture.active = null; //can help avoid errors 
       // cam.targetTexture = null;
        byte[] bytes;
        bytes = shot.EncodeToPNG();
        Debug.Log(bytes);
        string path = Application.dataPath + "/Saves/SavedTexture.png";
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log("Saved to " + path);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            TakeCameraShot();
        }
    }
}
