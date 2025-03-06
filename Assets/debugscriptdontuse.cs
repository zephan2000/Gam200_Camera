using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class debugscriptdontuse : MonoBehaviour
{
    private void Awake()
    {
        Material mat = GetComponent<Image>().material;
        GetComponent<Image>().material = new Material(mat);
    }
    private void Update()
    {
        //GetComponent<Image>().materialForRendering.SetFloat("_FillProgress", 0.5f * Mathf.Sin(Time.time * 2)+0.5f);
    }
}
