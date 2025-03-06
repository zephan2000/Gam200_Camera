using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoveJumpDisplay_Script : MonoBehaviour
{
    public float alphachangeduration = 0.4f;
    private float currentAlpha = 0.0f;
    private Transform playerref;
    public float distthres = 7.0f;
    private void Awake()
    {
        GetComponent<TMP_Text>().color = GetComponent<TMP_Text>().color + new Color(0, 0, 0, currentAlpha - GetComponent<TMP_Text>().color.a);
    }
    string ArrowProcessing(string before)
    {
        if (before == "LEFTARROW") return "" + '\u2190';
        if (before == "RIGHTARROW") return "" + '\u2192';
        if (before == "UPARROW") return "" + '\u2191';
        if (before == "DOWNARROW") return "" + '\u2193';
        return before;
    }
    // Start is called before the first frame update
    void Start()
    {
        playerref = FindObjectOfType<PlayerBox_Script>().transform;
        string theleft = ControlGetter.GetControls()[ControlGetter.NameOf_ControlLeft].ToString().ToUpper();
        theleft = ArrowProcessing(theleft);
        string theright = ControlGetter.GetControls()[ControlGetter.NameOf_ControlRight].ToString().ToUpper();
        theright = ArrowProcessing(theright);
        string thejump = ControlGetter.GetControls()[ControlGetter.NameOf_ControlJump].ToString().ToUpper();
        thejump = ArrowProcessing(thejump);
        GetComponent<TMP_Text>().text = theleft + " / " + theright + " to Move\n\n" + thejump + " to Jump";
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
