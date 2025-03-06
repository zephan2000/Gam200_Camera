using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuSelect : MonoBehaviour
{
    [SerializeField]
    private Transform SelectionOptionsA;
    [SerializeField]
    private Transform SelectionOptionsB;
    private bool IsModeB = false; //false means A
    public float pressTransitionTime = 0.3f;
    private float[] TrueValue_IsPressed, TargetValue_IsPressed;
    private float[] TrueValue_IsRed, TargetValue_IsRed;
    private int pointer = 0;
    [SerializeField]
    private AudioSource SFX_ButtonHover;
    private KeyCode EnterKey;
    void MakeButtonNice(int _counter)
    {
        TrueValue_IsPressed[_counter] = 1;
    }
    void MakeButtonNotSoNice(int _counter)
    {
        TrueValue_IsPressed[_counter] = 0;
    }
    void MakeButtonRed(int _counter)
    {
        TrueValue_IsRed[_counter] = 1;
    }
    void MakeButtonNotRed(int _counter)
    {
        TrueValue_IsRed[_counter] = 0;
    }
    private IEnumerator CoroTransition = null;
    private IEnumerator CoroAction_Transition(bool _toB)
    {
        float duration = 0.6f;
        for (float coro = 0; coro < 1.0f; coro += Time.deltaTime * (1 / duration))
        {
            if (coro >= 0.5f && _toB != IsModeB)
            {
                pointer = 0;
                IsModeB = !IsModeB;
                SelectionOptionsA.gameObject.SetActive(!IsModeB);
                SelectionOptionsB.gameObject.SetActive(IsModeB);
                if (!IsModeB)
                {
                    RespectiveInit(SelectionOptionsA.GetChild(0));
                }
                else
                {
                    RespectiveInit(SelectionOptionsB.GetChild(0));
                }
            }
            if (coro >= 0.5f)
            {
                transform.GetChild(0).GetComponent<Image>().materialForRendering.SetFloat("_FillProgress", (1.0f - coro) / 0.5f);
            }
            else
            {
                transform.GetChild(0).GetComponent<Image>().materialForRendering.SetFloat("_FillProgress", coro / 0.5f);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        transform.GetChild(0).GetComponent<Image>().materialForRendering.SetFloat("_FillProgress", 0);

        CoroTransition = null;
        yield break;
    }
    private IEnumerator CoroAction_ReplaceInput(TMP_Text textObj, string toreplace)
    {
        string helperogtext = SelectionOptionsB.GetChild(1).GetComponent<TMP_Text>().text;
        SelectionOptionsB.GetChild(1).GetComponent<TMP_Text>().text = "Allowed Inputs:\nARROW KEYS\nALPHABETS, NUMBERS\nSPACE, SHIFT";

        string ogtext = textObj.text;
        textObj.text = "Input Your Key";
        yield return new WaitForSeconds(Time.deltaTime);

        KeyCode thefabled;

        while (true)
        {
            if (!Input.anyKeyDown) yield return new WaitForSeconds(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                thefabled = KeyCode.LeftShift;
                break;
            }
            if (Input.GetKeyDown(KeyCode.RightShift))
            {
                thefabled = KeyCode.RightShift;
                break;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                thefabled = KeyCode.UpArrow;
                break;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                thefabled = KeyCode.DownArrow;
                break;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                thefabled = KeyCode.LeftArrow;
                break;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                thefabled = KeyCode.RightArrow;
                break;
            }

            string input = Input.inputString;
            if (input.Length == 1)
            {
                char thechar = input[0];
                if (
                    (thechar >= 'A' && thechar <= 'Z')
                    || (thechar >= 'a' && thechar <= 'z')
                    || (thechar >= '0' && thechar <= '9')
                    )
                {
                    if (thechar >= 'A' && thechar <= 'Z')
                    {
                        thechar += (char)32;
                    }
                    thefabled = (KeyCode)((int)thechar);
                    break;
                }
                if (thechar == ' ')
                {
                    thefabled = KeyCode.Space;
                    break;
                }
            }

            yield return new WaitForSeconds(Time.deltaTime);
        }

        ControlGetter.SwapKey(toreplace, thefabled);
        textObj.text = ogtext;
        UpdateButtonText();
        SelectionOptionsB.GetChild(1).GetComponent<TMP_Text>().text = helperogtext;
        CoroTransition = null;
        yield break;
    }
    public void DoNewInput(TMP_Text textObj, string toreplace) //name of control
    {
        if (CoroTransition != null) return;
        CoroTransition = CoroAction_ReplaceInput(textObj, toreplace);
        StartCoroutine(CoroTransition);
    }
    public void DoTransition()
    {
        if (CoroTransition != null) return; //lmao error
        CoroTransition = CoroAction_Transition(!IsModeB);
        StartCoroutine(CoroTransition);
    }
    private void Awake()
    {
        //EnterKey = ControlGetter.GetControls()[ControlGetter.NameOf_ControlJump];
        EnterKey = KeyCode.Return;

        SelectionOptionsA.gameObject.SetActive(true);
        SelectionOptionsB.gameObject.SetActive(false);

        if (!IsModeB)
        {
            RespectiveInit(SelectionOptionsA.GetChild(0));
        }
        else
        {
            RespectiveInit(SelectionOptionsB.GetChild(0));
        }
    }
    public void Public_UpdateButtonText()
    {
        UpdateButtonText();
    }
    void UpdateButtonText()
    {
        Dictionary<KeyCode, int> keyCounterDupe = new Dictionary<KeyCode, int>();
        Dictionary<string, KeyCode> currentControls = ControlGetter.GetControls();

        foreach(KeyValuePair<string, KeyCode> thepair in currentControls)
        {
            if (!keyCounterDupe.ContainsKey(thepair.Value)) keyCounterDupe.Add(thepair.Value, 0);
            keyCounterDupe[thepair.Value]++;
        }

        //EnterKey = currentControls[ControlGetter.NameOf_ControlJump];
        int thecounter = 0;
        foreach (Transform trans in SelectionOptionsB.GetChild(0))
        {
            if (trans.GetComponent<MainMenuInvoke_Script>().typeOfButton == MainMenuInvokeTypes.PRESET) //not done here anymore
            {
                trans.GetChild(0).GetComponent<TMP_Text>().text = "Preset: " + ControlGetter.GetCurrentPresetName();
            }
            else if (trans.GetComponent<MainMenuInvoke_Script>().typeOfButton != MainMenuInvokeTypes.BACKTOMENU)
            {
                string thename = "";
                KeyCode thecontrol = KeyCode.None;
                if (trans.GetComponent<MainMenuInvoke_Script>().typeOfButton == MainMenuInvokeTypes.CONTROL_LEFT)
                {
                    thename = ControlGetter.NameOf_ControlLeft;
                    thecontrol = currentControls[ControlGetter.NameOf_ControlLeft];
                }
                if (trans.GetComponent<MainMenuInvoke_Script>().typeOfButton == MainMenuInvokeTypes.CONTROL_RIGHT)
                {
                    thename = ControlGetter.NameOf_ControlRight;
                    thecontrol = currentControls[ControlGetter.NameOf_ControlRight];
                }
                if (trans.GetComponent<MainMenuInvoke_Script>().typeOfButton == MainMenuInvokeTypes.CONTROL_JUMPENTER)
                {
                    thename = ControlGetter.NameOf_ControlJump;
                    thecontrol = currentControls[ControlGetter.NameOf_ControlJump];
                }
                if (trans.GetComponent<MainMenuInvoke_Script>().typeOfButton == MainMenuInvokeTypes.CONTROL_SHRINK)
                {
                    thename = ControlGetter.NameOf_ControlShrink;
                    thecontrol = currentControls[ControlGetter.NameOf_ControlShrink];
                }
                if (trans.GetComponent<MainMenuInvoke_Script>().typeOfButton == MainMenuInvokeTypes.CONTROL_QUICKRESET)
                {
                    thename = ControlGetter.NameOf_ControlQuickreset;
                    thecontrol = currentControls[ControlGetter.NameOf_ControlQuickreset];
                }

                if(keyCounterDupe.ContainsKey(thecontrol) && keyCounterDupe[thecontrol] > 1)
                {
                    MakeButtonRed(thecounter);
                }
                else
                {
                    MakeButtonNotRed(thecounter);
                }

                trans.GetChild(0).GetComponent<TMP_Text>().text = thename + ": " + thecontrol;
            }
            thecounter++;
        }
    }
    void RespectiveInit(Transform trans)
    {

        TrueValue_IsPressed = new float[trans.childCount];
        TargetValue_IsPressed = new float[trans.childCount];
        TrueValue_IsRed = new float[trans.childCount];
        TargetValue_IsRed = new float[trans.childCount];
        for (int i = 0; i < trans.childCount; ++i)
        {
            Material theref = trans.GetChild(i).GetComponent<Image>().material;
            trans.GetChild(i).GetComponent<Image>().material = new Material(theref);

            if (pointer == i) MakeButtonNice(i);
            else MakeButtonNotSoNice(i);

            MakeButtonNotRed(i);

            TargetValue_IsPressed[i] = TrueValue_IsPressed[i];
            trans.GetChild(i).GetComponent<Image>().materialForRendering.SetFloat("_IsPressed", Mathf.Pow(TargetValue_IsPressed[i], 2));
            trans.GetChild(i).GetComponent<Image>().materialForRendering.SetFloat("_AlphaMod", 0.3f + 0.7f * TargetValue_IsPressed[i]);
            trans.GetChild(i).GetChild(0).GetComponent<TMP_Text>().color =
                trans.GetChild(i).GetChild(0).GetComponent<TMP_Text>().color
                - new Color(0, 0, 0, trans.GetChild(i).GetChild(0).GetComponent<TMP_Text>().color.a)
                + new Color(0, 0, 0, 0.7f + 0.3f * TargetValue_IsPressed[i])
                ;
        }

        if (IsModeB)
        {
            UpdateButtonText();
            SelectionOptionsB.GetChild(1).GetComponent<TMP_Text>().text = "Current Control Preset: " + ControlGetter.GetCurrentPresetName() + "\n\nPress 'ENTER' then Any key to rebind a selected key\npress 'ctrl' to toggle control presets";
        }
    }
    void RespectiveUpdateLoop(Transform trans)
    {
        int prevPointer = pointer;
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            pointer--;
            if (pointer < 0) pointer = trans.childCount - 1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            pointer++;
            if (pointer >= trans.childCount) pointer = 0;
        }
        if (pointer != prevPointer) SFX_ButtonHover.Play();

        if (Input.GetKeyDown(EnterKey) || Input.GetKeyDown(KeyCode.Return) /*&& pointer != 1*/)
        {
            trans.GetChild(pointer).GetComponent<MainMenuInvoke_Script>().InvokeFunction();
        }
    }
    void RespectiveButtonChange(Transform trans)
    {
        for (int i = 0; i < trans.childCount; ++i)
        {
            if (pointer == i) MakeButtonNice(i);
            else MakeButtonNotSoNice(i);


            if (Mathf.Abs(TargetValue_IsPressed[i] - TrueValue_IsPressed[i]) > (1.0f / pressTransitionTime) * Time.deltaTime)
            {
                TargetValue_IsPressed[i] += ((TrueValue_IsPressed[i] - TargetValue_IsPressed[i]) / Mathf.Abs((TrueValue_IsPressed[i] - TargetValue_IsPressed[i]))) * (1.0f / pressTransitionTime) * Time.deltaTime;
            }
            else
            {
                TargetValue_IsPressed[i] = TrueValue_IsPressed[i];
            }
            if (Mathf.Abs(TargetValue_IsRed[i] - TrueValue_IsRed[i]) > (1.0f / pressTransitionTime) * Time.deltaTime)
            {
                TargetValue_IsRed[i] += ((TrueValue_IsRed[i] - TargetValue_IsRed[i]) / Mathf.Abs((TrueValue_IsRed[i] - TargetValue_IsRed[i]))) * (1.0f / pressTransitionTime) * Time.deltaTime;
            }
            else
            {
                TargetValue_IsRed[i] = TrueValue_IsRed[i];
            }
            trans.GetChild(i).GetComponent<Image>().materialForRendering.SetFloat("_IsPressed", Mathf.Pow(TargetValue_IsPressed[i], 2));
            trans.GetChild(i).GetComponent<Image>().materialForRendering.SetFloat("_WarningProgress", Mathf.Pow(TargetValue_IsRed[i], 1.2f));
            trans.GetChild(i).GetComponent<Image>().materialForRendering.SetFloat("_AlphaMod", 0.3f + 0.7f * TargetValue_IsPressed[i]);
            trans.GetChild(i).GetChild(0).GetComponent<TMP_Text>().color =
                trans.GetChild(i).GetChild(0).GetComponent<TMP_Text>().color
                - new Color(0, 0, 0, trans.GetChild(i).GetChild(0).GetComponent<TMP_Text>().color.a)
                + new Color(0, 0, 0, 0.7f + 0.3f * TargetValue_IsPressed[i])
                ;
        }
    }
    IEnumerator Coroutine_SpreadOut(float _dura)
    {
        RectTransform elementR = SelectionOptionsA.GetChild(0).GetComponent<RectTransform>();
        RectTransform elementL1 = SelectionOptionsA.GetChild(1).GetComponent<RectTransform>();
        RectTransform elementL2 = SelectionOptionsA.GetChild(2).GetComponent<RectTransform>();
        Vector3 og = new Vector3(elementR.anchoredPosition.x, elementL1.anchoredPosition.x, elementL2.anchoredPosition.x);

        pointer = -999;

        for (float coro = 0; coro < 1; coro += (1 / _dura) * Time.deltaTime)
        {
            float truecoro = 0;
            if (coro <= 1.0f / 3.0f) truecoro = -75.0f * Mathf.Sin(0.5f * Mathf.PI * coro / (1.0f / 3.0f));
            else truecoro = -75.0f + 825.0f * Mathf.Pow((coro - 1.0f / 3.0f) / (2.0f / 3.0f), 2);

            elementR.anchoredPosition = new Vector2(og.x + truecoro * (750.0f/650.0f), elementR.anchoredPosition.y);
            elementL1.anchoredPosition = new Vector2(og.y - truecoro, elementL1.anchoredPosition.y);
            elementL2.anchoredPosition = new Vector2(og.z - truecoro, elementL2.anchoredPosition.y);
            RespectiveButtonChange(SelectionOptionsA.GetChild(0));
            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        }

        yield break;
    }
    public void SpreadOut(float _dura)
    {
        StartCoroutine(Coroutine_SpreadOut(_dura));
    }
    // Update is called once per frame
    void Update()
    {
        if (SingletonMaster.Instance.IsTransitioning) return;

        if (CoroTransition == null)
        {
            if (!IsModeB)
            {
                RespectiveUpdateLoop(SelectionOptionsA.GetChild(0));
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.LeftControl))
                {
                    ControlGetter.CyclePresets();
                    FindObjectOfType<MainMenuSelect>().Public_UpdateButtonText();
                }
                RespectiveUpdateLoop(SelectionOptionsB.GetChild(0));

                if (CoroTransition == null)
                    SelectionOptionsB.GetChild(1).GetComponent<TMP_Text>().text = "Current Control Preset: " + ControlGetter.GetCurrentPresetName() + "\n\nPress 'ENTER' then Any key to rebind a selected key\npress 'ctrl' to toggle control presets";

            }
        }

        if (!IsModeB)
            RespectiveButtonChange(SelectionOptionsA.GetChild(0));
        else
            RespectiveButtonChange(SelectionOptionsB.GetChild(0));
    }
}
