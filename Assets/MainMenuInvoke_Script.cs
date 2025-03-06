using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public enum MainMenuInvokeTypes
{
    START,
    OPTION,
    QUIT,
    BACKTOMENU,
    PRESET,
    CONTROL_LEFT,
    CONTROL_RIGHT,
    CONTROL_JUMPENTER,
    CONTROL_SHRINK,
    CONTROL_QUICKRESET,
}

public class MainMenuInvoke_Script : MonoBehaviour
{
    public MainMenuInvokeTypes typeOfButton = MainMenuInvokeTypes.START;
    public void InvokeFunction()
    {
        switch(typeOfButton)
        {
            case MainMenuInvokeTypes.PRESET://NOT EXECUTED HERE ANYMORE
                print("why this playing bro");
                ControlGetter.CyclePresets();
                FindObjectOfType<MainMenuSelect>().Public_UpdateButtonText();
                break;
            case MainMenuInvokeTypes.CONTROL_LEFT:
                FindObjectOfType<MainMenuSelect>().DoNewInput(transform.GetChild(0).GetComponent<TMP_Text>(), ControlGetter.NameOf_ControlLeft);
                break;
            case MainMenuInvokeTypes.CONTROL_RIGHT:
                FindObjectOfType<MainMenuSelect>().DoNewInput(transform.GetChild(0).GetComponent<TMP_Text>(), ControlGetter.NameOf_ControlRight);
                break;
            case MainMenuInvokeTypes.CONTROL_JUMPENTER:
                FindObjectOfType<MainMenuSelect>().DoNewInput(transform.GetChild(0).GetComponent<TMP_Text>(), ControlGetter.NameOf_ControlJump);
                break;
            case MainMenuInvokeTypes.CONTROL_SHRINK:
                FindObjectOfType<MainMenuSelect>().DoNewInput(transform.GetChild(0).GetComponent<TMP_Text>(), ControlGetter.NameOf_ControlShrink);
                break;
            case MainMenuInvokeTypes.CONTROL_QUICKRESET:
                FindObjectOfType<MainMenuSelect>().DoNewInput(transform.GetChild(0).GetComponent<TMP_Text>(), ControlGetter.NameOf_ControlQuickreset);
                break;
            case MainMenuInvokeTypes.START:
                SingletonMaster.Instance.ChangeScenes("LevelTestScene", true);
                break;
            case MainMenuInvokeTypes.BACKTOMENU:
            case MainMenuInvokeTypes.OPTION:
                FindObjectOfType<MainMenuSelect>().DoTransition();
                break;
            case MainMenuInvokeTypes.QUIT:
                Application.Quit();
                break;
        }
    }
}
