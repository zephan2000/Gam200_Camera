using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu_Script : MonoBehaviour
{
    [SerializeField]
    private RectTransform ArrowReference;
    [SerializeField]
    private Transform SelectionOptions;
    public int pointer = 0;
    [SerializeField]
    private AudioSource SFX_ButtonHover;

    private bool ranonce = false;

    private KeyCode EnterKey;

    void MakeButtonNice(int _counter)
    {
        SelectionOptions.GetChild(_counter).GetComponent<TMP_Text>().fontStyle = FontStyles.Underline | FontStyles.UpperCase;
    }
    void MakeButtonNotSoNice(int _counter)
    {
        SelectionOptions.GetChild(_counter).GetComponent<TMP_Text>().fontStyle = FontStyles.Normal | FontStyles.UpperCase;
    }
    void ArrowUpdate()
    {
        ArrowReference.anchoredPosition = SelectionOptions.GetChild(pointer).GetComponent<RectTransform>().position + new Vector3(-950 + 50 + SelectionOptions.GetChild(pointer).GetComponent<RectTransform>().rect.width / 2, -520, 0);// + new Vector2(SelectionOptions.GetChild(pointer).GetComponent<RectTransform>().rect.width, 0);
    }
    private void Awake()
    {
        ArrowReference.GetComponent<Image>().material = new Material(ArrowReference.GetComponent<Image>().material);
        //EnterKey = ControlGetter.GetControls()[ControlGetter.NameOf_ControlJump];
        EnterKey = KeyCode.Return;
        for (int i = 0; i < SelectionOptions.childCount; ++i)
        {
            if (pointer == i) MakeButtonNice(i);
            else MakeButtonNotSoNice(i);
        }

        if (gameObject.scene.name == "PauseMenuScene")
        {
            if (FindObjectOfType<LevelManager_Script>().HasAbilityToRestart == false)
            {
                Destroy(SelectionOptions.GetChild(2).gameObject);
                Destroy(SelectionOptions.GetChild(1).gameObject);
                SelectionOptions.GetComponent<VerticalLayoutGroup>().spacing = -620;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (SingletonMaster.Instance.IsTransitioning) return;

        if (FindObjectOfType<LevelManager_Script>().GetActiveSceneName() != gameObject.scene.name)
        {
            ArrowReference.GetComponent<Image>().material.SetFloat("_Bounce", 0);
            return;
        }
        else
        {
            ArrowReference.GetComponent<Image>().material.SetFloat("_Bounce", 1);
        }

        if (ranonce)
        ArrowReference.gameObject.SetActive(true);

        int prevPointer = pointer;
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            pointer--;
            if (pointer < 0) pointer = SelectionOptions.childCount - 1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            pointer++;
            if (pointer >= SelectionOptions.childCount) pointer = 0;
        }
        if (pointer != prevPointer) SFX_ButtonHover.Play();

        ArrowUpdate();

        for (int i = 0; i < SelectionOptions.childCount; ++i)
        {
            if (pointer == i) MakeButtonNice(i);
            else MakeButtonNotSoNice(i);
        }

        if (Input.GetKeyDown(EnterKey) || Input.GetKeyDown(KeyCode.Return))
        {
            SelectionOptions.GetChild(pointer).GetComponent<ButtonInvokeGameManager_Script>().OnClick();
        }

        ranonce = true;
    }
}
