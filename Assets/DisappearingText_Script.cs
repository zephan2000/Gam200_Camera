using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisappearingText_Script : MonoBehaviour
{
    public float alphachangeduration = 0.4f;
    private float currentAlpha = 0.0f;
    private Transform playerref;
    public float distthres = 7.0f;
    private void Awake()
    {
        GetComponent<TMP_Text>().color = GetComponent<TMP_Text>().color + new Color(0, 0, 0, currentAlpha - GetComponent<TMP_Text>().color.a);
    }
    // Start is called before the first frame update
    void Start()
    {
        playerref = FindObjectOfType<PlayerBox_Script>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(playerref.position.x - transform.position.x) <= distthres)
        {
            currentAlpha = Mathf.Clamp(currentAlpha + Time.deltaTime * (1.0f / alphachangeduration), 0, 1);
        }
        else
        {
            currentAlpha = Mathf.Clamp(currentAlpha - Time.deltaTime * (1.0f / alphachangeduration), 0, 1);
        }

        GetComponent<TMP_Text>().color = GetComponent<TMP_Text>().color + new Color(0, 0, 0, currentAlpha - GetComponent<TMP_Text>().color.a);
    }
}
