using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GiveShaderUnscaledTime : MonoBehaviour
{
    private Material shadermaterial;
    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<SpriteRenderer>())
        {
            shadermaterial = GetComponent<SpriteRenderer>().material;
        }
        else if (GetComponent<MeshRenderer>())
        {
            shadermaterial = GetComponent<MeshRenderer>().material;
        }
        else if (GetComponent<Image>())
        {
            shadermaterial = GetComponent<Image>().material;
        }
        shadermaterial.SetFloat("_UTime", Time.unscaledTime);
    }

    // Update is called once per frame
    void Update()
    {
        shadermaterial.SetFloat("_UTime", Time.unscaledTime);
    }
}
