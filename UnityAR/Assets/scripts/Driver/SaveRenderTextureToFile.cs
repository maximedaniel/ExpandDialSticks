using UnityEngine;
using UnityEditor;

public class SaveRenderTextureToFile {

    public static int rtSize = 1024;
    [MenuItem("Test/Take Camera Shot")]
    public static void TakeCameraShot()
    {
        Camera cam = Selection.activeGameObject.GetComponent<Camera>();
        Debug.Log("Selected GameObject is " + Selection.activeGameObject.name + ".");

        RenderTexture rt = new RenderTexture(rtSize, rtSize, 16, RenderTextureFormat.ARGB32);
        rt.Create();
        cam.targetTexture = rt;
        cam.Render();


        Texture2D shot = new Texture2D(rtSize, rtSize, TextureFormat.ARGB32, false);
        RenderTexture.active = rt;
        // false, meaning no need for mipmaps
        shot.ReadPixels(new Rect(0, 0, rtSize, rtSize), 0, 0);
        shot.Apply();
        RenderTexture.active = null; //can help avoid errors 
        cam.targetTexture = null;
        byte[] bytes;
        bytes = shot.EncodeToPNG();
        Debug.Log(bytes);
        string path = Application.dataPath + "/Saves/SavedTexture.png";
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log("Saved to " + path);
    }


    // Validate the menu item defined by the function above.
    // The menu item will be disabled if this function returns false.
    [MenuItem("Test/Take Transform Shot", true)]
    static bool ValidateTakeCameraShot()
    {
        Camera cam = null;
        if (Selection.activeGameObject != null)
		{
            cam = Selection.activeGameObject.GetComponent<Camera>();
		}
        // Return false if no transform is selected.
        return cam != null;
    }
}
