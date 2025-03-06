using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShrinkTutorialDisplay : MonoBehaviour
{
    public float alphachangeduration = 0.4f;
    private float currentAlpha = 0.0f;
    private Transform playerref;
    public float distthres = 10.0f;
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
        string theshrink = ControlGetter.GetControls()[ControlGetter.NameOf_ControlShrink].ToString().ToUpper();
        theshrink = ArrowProcessing(theshrink);
        string thejump = ControlGetter.GetControls()[ControlGetter.NameOf_ControlJump].ToString().ToUpper();
        thejump = ArrowProcessing(thejump);
        GetComponent<TMP_Text>().text = "Hold " + theshrink + " to Shrink\n\nPress " + thejump + " + " + theshrink + " together to jump higher.";
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
