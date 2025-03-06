using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewUIImage : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Image>().material = new Material(GetComponent<Image>().material);
    }
}
