using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInvokeGameManager_Script : MonoBehaviour
{
    public string methodName;
    public void OnClick()
    {
        if (FindObjectOfType<LevelManager_Script>())
        {
            FindObjectOfType<LevelManager_Script>().Invoke(methodName, 0.0f);
        }
    }
}
