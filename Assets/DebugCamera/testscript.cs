using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testscript : MonoBehaviour
{
    [SerializeField]
    private RenderTexture rendertexture; //ground cam
    private Texture2D renderreader;

    public int textureSize = 512;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    PauseToggle();
        //}

        //if (Input.GetKeyDown(KeyCode.O))
        //{
        //    Snap();
        //}
    }
    public void PauseToggle()
    {
        if (Time.timeScale > 0.5f) Time.timeScale = 0;
        else Time.timeScale = 1;
    }
    public void Snap()
    {
        RenderTexture mRt = new RenderTexture(rendertexture.width, rendertexture.height, rendertexture.depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        mRt.antiAliasing = rendertexture.antiAliasing;
        renderreader = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
        GetComponent<Camera>().targetTexture = mRt;
        GetComponent<Camera>().Render();
        RenderTexture.active = mRt;
        renderreader.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
        renderreader.Apply();
        SaveTexture(renderreader);
    }
    private void SaveTexture(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/RenderOutput";
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }
        System.IO.File.WriteAllBytes(dirPath + "/R_" + DateTime.Now.Ticks + ".png", bytes);
        Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + dirPath);
    }
}
