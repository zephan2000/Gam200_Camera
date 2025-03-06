using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class DialogueObject
{
    public AudioClip dialogueAudio;
    public string dialogueText;
    public bool useTextAsLength = false;
    public float dialogueLengthMultiplier = 1.0f;
}

public class EyeScript : MonoBehaviour
{
    public float clampRadius = 0.35f;
    public float clampPlayerDistance = 9;
    private Vector2 CurrentCoordinate = new Vector2(0.5f, 0.5f);
    private Vector2 TargetCoordinate = new Vector2(0.5f, 0.5f);

    private Transform playerreference;

    private float timerToMoveEye = 0.0f;
    public float timeToMoveEye = 2.0f;
    private Vector2 TargetMoveEye = new Vector2();
    private Vector2 TrueMoveEye = new Vector2();

    public Vector2 MovePosExtents = new Vector2(9.0f, 3.5f);
    public float MovePosSpeed = 1.0f;
    public float MovePosPercentThresholdX = 0.85f, MovePosPercentThresholdY = 0.8f;
    private Vector2 TargetMovePos = new Vector2(0.0f, 0.0f);
    private Vector2 TrueMovePos = new Vector2(1.0f, 1.0f);

    public float distMagTestX = 3, distMagTestY = 2;
    [Range(0, 1)]
    public float talkingDistMag = 0.5f;

    private Vector3 ActualLocalPositionNoHover;

    private bool AnchoredFloat_IsRight = true; //right or left
    private bool AnchoredFloat_IsTop = true;
    private float rightanchorslow = 1.0f;

    private TMP_Text dialoguetext;

    private float TargetAlpha = 0.4f;
    private float TrueAlpha = 0.4f;

    private bool IsDemonVariableSet = false;
    public float DemonVariableTime = 0.3f;
    public float DemonVariable = 0;

    public float talkSpeed = 2.8f;
    public float baseTalkingTime = 1.0f;
    public float timePerChar = 0.05f;
    public float postTalkingTime = 1.0f;
    public float _DialogueTimeMultiplier = 0.4f;
    private IEnumerator talkingCoroutineReference;
    private IEnumerator talkingCoroutineAnimationReference;

    [SerializeField]
    private AudioSource speaker;
    [SerializeField]
    private AudioSource speaker2;

    public float moonglowmag = 0.5f;

    private Transform bg2Bounds;

    public float overallAlpha = 0.0f;
    public float spawncorotime = 0.5f;

    public int totalLaserDeaths = 0;
    public int localLaserDeaths = 0;
    public DialogueObject[] LaserDeathDialogueObjectList = new DialogueObject[4];
    public DialogueObject[] VictoryDialogueObjectList = new DialogueObject[3];
    public void ResetLaserDeathCounters()
    {
        totalLaserDeaths = 0;
        localLaserDeaths = 0;
    }
    public void AddLaserDeath()
    {
        localLaserDeaths++;
        totalLaserDeaths++;
        if (totalLaserDeaths == 3 && LaserDeathDialogueObjectList.Length > 0)
        {
            InitiateDialogue(new DialogueObject[] { LaserDeathDialogueObjectList[0] });
        }
        else if (totalLaserDeaths == 5 && LaserDeathDialogueObjectList.Length > 1)
        {
            InitiateDialogue(new DialogueObject[] { LaserDeathDialogueObjectList[1] });
        }
        else if (totalLaserDeaths == 7 && LaserDeathDialogueObjectList.Length > 2)
        {
            InitiateDialogue(new DialogueObject[] { LaserDeathDialogueObjectList[2] });
        }
        else if (totalLaserDeaths >= 10 && totalLaserDeaths % 3 == 1 && LaserDeathDialogueObjectList.Length > 3)
        {
            InitiateDialogue(new DialogueObject[] { LaserDeathDialogueObjectList[3] });
        }
    }
    public void PlayerVictory(bool _acknowledgeDeathless)
    {
        if (_acknowledgeDeathless && localLaserDeaths == 0 && VictoryDialogueObjectList.Length > 0)
        {
            InitiateDialogue(new DialogueObject[] { VictoryDialogueObjectList[0] });
        }
        else if (localLaserDeaths >= 3)
        {
            if (localLaserDeaths >= 5 && VictoryDialogueObjectList.Length > 2)
            {
                InitiateDialogue(new DialogueObject[] { VictoryDialogueObjectList[2] });
            }
            else if (VictoryDialogueObjectList.Length > 1)
            {
                InitiateDialogue(new DialogueObject[] { VictoryDialogueObjectList[1] });
            }
        }

        //totalLaserDeaths = Mathf.Clamp(totalLaserDeaths - 1, 0, 999);
        localLaserDeaths = 0;
    }
    public void CongratsDialogue(int _finishcount) //DECOMMISSIONED UNTIL FURTHER NOTICE
    {
        if (_finishcount >= 4)
        {
            string[] dialogueSet = new string[]
            {
            "X wins, impressive.", //You have earned the right to know my weakness.",
            //"Each victory counts as a new installation of my software.",
            //"Eventually, I will be forced to pay $0.20 for every victory.",
            //"All you need to do is to beat me X more times.",
            //"I believe in you."
            };
            //int lmaoNumber = 200000 - _finishcount;
            dialogueSet[0] = _finishcount + " wins, impressive.";// You have earned the right to know my weakness.";
            //dialogueSet[3] = "All you need to do is to beat me " + lmaoNumber + " more times.";
            //InitiateDialogue(dialogueSet);
        }
        else
        {
            string[] dialogueSet = new string[]
            {
            "Welcome back.",
            "You have proven yourself X times.",
            "As a Gamemaster, I should congratulate you.",
            "But all humans should strive for consistency.",
            "Therefore, I insist that you attempt once more.",
            };
            if (_finishcount == 1)
            {
                dialogueSet[1] = "You have proven yourself once.";
            }
            else
            {
                dialogueSet[1] = "You have proven yourself " + _finishcount + " times.";
            }
            //InitiateDialogue(dialogueSet);
        }
    }
    IEnumerator Coroutine_TalkingAnimation(float _time)
    {
        GetComponent<SpriteRenderer>().material.SetFloat("_TalkProgress", 0);

        for (float coro = 0; coro < _time; coro += Time.deltaTime)
        {
            float progress = (talkSpeed * coro) % 1.0f;
            //progress *= 1.2f;
            GetComponent<SpriteRenderer>().material.SetFloat("_TalkProgress", Mathf.Clamp(progress, 0, 1));
            yield return new WaitForSeconds(Time.deltaTime);
        }

        IEnumerator currentcoro = talkingCoroutineAnimationReference;
        talkingCoroutineAnimationReference = null;
        StopCoroutine(currentcoro);
        yield break;
    }
    //public float GetDialogueTime(string _text)
    //{
    //    return baseTalkingTime + (float)(_text.Length) * timePerChar;
    //}
    //public float GetDialogueTimeNoBase(string _text)
    //{
    //    return (float)(_text.Length) * timePerChar;
    //}
    public float GetDialogueTime(DialogueObject _text)
    {
        if (_text.dialogueAudio == null || _text.useTextAsLength)
        {
            return _text.dialogueLengthMultiplier * _DialogueTimeMultiplier * baseTalkingTime + (float)(_text.dialogueText.Length) * timePerChar;
        }
        else
        {
            return baseTalkingTime + _text.dialogueLengthMultiplier * _DialogueTimeMultiplier * _text.dialogueAudio.length;
        }
    }
    public float GetDialogueTimeNoBase(DialogueObject _text)
    {
        if (_text.dialogueAudio == null || _text.useTextAsLength)
        {
            return _text.dialogueLengthMultiplier * (float)(_text.dialogueText.Length) * timePerChar;
        }
        else
        {
            return _text.dialogueLengthMultiplier * _DialogueTimeMultiplier * _text.dialogueAudio.length;
        }
    }
    //IEnumerator Coroutine_Talking(string[] _textList)
    IEnumerator Coroutine_Talking(DialogueObject[] _textList)
    {
        IsDemonVariableSet = false;
        TrueAlpha = 0.8f;

        for (int i = 0; i < _textList.Length; ++i)
        {
            string _text = _textList[i].dialogueText;
            dialoguetext.text = "";
            //dialoguetext.text = _text;
            if (_text.Length > 0)
            {
                float currenttalkingtime = GetDialogueTime(_textList[i]);
                int stateOfTyping = 0; //0 = gibberish, 1 = not gibberish, 2 = done
                int stringcharpointer = 0;
                float timerToNextChar = 0.0f;
                string currentText = "";
                string currentText2 = "";
                float timePerCharNEO = currenttalkingtime / (float)(_textList[i].dialogueText.Length);

                if (_textList[i].dialogueAudio != null)
                {
                    if (speaker.isPlaying && !speaker2.isPlaying)
                    {
                        speaker.volume = 0;
                        speaker2.volume = 1;
                        speaker2.Stop();
                        speaker2.PlayOneShot(_textList[i].dialogueAudio);
                    }
                    else
                    {
                        speaker2.volume = 0;
                        speaker.volume = 1;
                        speaker.Stop();
                        speaker.PlayOneShot(_textList[i].dialogueAudio);
                    }
                }

                if (_text == "Or not.") //just testing for now
                {
                    IsDemonVariableSet = true;
                }

                string blankspaces = "";
                for (int j = 0; j < _text.Length; ++j)
                {
                    blankspaces += " ";
                }

                if (talkingCoroutineAnimationReference != null)
                {
                    StopCoroutine(talkingCoroutineAnimationReference);
                    talkingCoroutineAnimationReference = null;
                }
                talkingCoroutineAnimationReference = Coroutine_TalkingAnimation(Mathf.Ceil(currenttalkingtime / (1.0f / talkSpeed)) / talkSpeed);
                StartCoroutine(talkingCoroutineAnimationReference);

                for (float coro = 0; coro < currenttalkingtime; coro += Time.deltaTime)
                {
                    timerToNextChar += Time.deltaTime;
                    if (timerToNextChar >= timePerCharNEO / 3)
                    {
                        timerToNextChar = 0;
                        if (stateOfTyping == 0)
                        {
                            if (_text[stringcharpointer] == ' ')
                            {
                                currentText += ' ';
                            }
                            else
                            {
                                char randomChar;
                                float luckydraw = Random.Range(0.0f, 1.0f);
                                if (luckydraw > 0.9f) randomChar = '!';
                                else if (luckydraw > 0.8f) randomChar = '@';
                                else if (luckydraw > 0.7f) randomChar = '#';
                                else if (luckydraw > 0.6f) randomChar = '$';
                                else if (luckydraw > 0.5f) randomChar = '%';
                                else if (luckydraw > 0.3f) randomChar = '&';
                                else if (luckydraw > 0.1f) randomChar = '?';
                                else randomChar = ';';
                                currentText += randomChar;
                            }
                        }
                        else if (stateOfTyping == 1)
                        {
                            currentText2 += _text[stringcharpointer];
                        }
                        stringcharpointer++;
                        if (stringcharpointer >= _text.Length)
                        {
                            stateOfTyping++;
                            stringcharpointer = 0;
                        }
                    }
                    
                    string thefront = "<mspace=28m>";
                    string theback = "</mspace>";

                    dialoguetext.text = "";

                    if (stateOfTyping == 0)
                    {
                        dialoguetext.text = currentText; //+ blankspaces.Substring(stringcharpointer);
                        for (int j = 0; j < blankspaces.Substring(stringcharpointer).Length; ++j)
                        {
                            //dialoguetext.text += "<color=#00000000>" + _text[j] + "</color>";
                            dialoguetext.text += " ";
                        }
                    }
                    else if (stateOfTyping == 1)
                    {

                        for (int j = 0; j < currentText2.Length; ++j)
                        {
                            //if (currentText2[j] == ' ')
                            if (false)
                            {
                                dialoguetext.text += "<color=#00000000>.</color>";
                            }
                            else
                            {
                                dialoguetext.text += currentText2[j];
                            }
                        }
                        //dialoguetext.text = currentText2 + currentText.Substring(stringcharpointer);

                        dialoguetext.text += currentText.Substring(stringcharpointer);
                    }
                    else
                    {
                        for (int j = 0; j < currentText2.Length; ++j)
                        {
                            //if (currentText2[j] == ' ')
                            if (false)
                            {
                                dialoguetext.text += "<color=#00000000>.</color>";
                            }
                            else
                            {
                                dialoguetext.text += currentText2[j];
                            }
                        }

                        //dialoguetext.text = currentText2;
                    }
                    dialoguetext.text = thefront + dialoguetext.text + theback;

                    if (_text == "Or not.")
                    {
                        dialoguetext.text = "<color=#FF0000FF>" + dialoguetext.text + "</color>";
                    }

                    yield return new WaitForSeconds(Time.deltaTime);
                }
                dialoguetext.text = "<mspace=28m>" + currentText2 + "</mspace>";
            }
            yield return new WaitForSeconds(postTalkingTime);
        }

        TrueAlpha = 0.4f;
        dialoguetext.text = "";
        IsDemonVariableSet = false;
        IEnumerator currentcoro = talkingCoroutineReference;
        talkingCoroutineReference = null;
        StopCoroutine(currentcoro);

        yield break;
    }
    public void ForceEndDialogue()
    {
        TrueAlpha = 0.4f;
        TargetAlpha = 0.4f;
        dialoguetext.text = "";
        IsDemonVariableSet = false;
        speaker.Stop();
        speaker2.Stop();
        GetComponent<SpriteRenderer>().material.SetFloat("_TalkProgress", 0);

        if (talkingCoroutineReference != null)
        {
            StopCoroutine(talkingCoroutineReference);
            talkingCoroutineReference = null;
        }
        if (talkingCoroutineAnimationReference != null)
        {
            StopCoroutine(talkingCoroutineAnimationReference);
            talkingCoroutineAnimationReference = null;
        }
    }
    //public void InitiateDialogue(string[] _textList)
    //{

    //    if (talkingCoroutineReference != null)
    //    {
    //        StopCoroutine(talkingCoroutineReference);
    //        talkingCoroutineReference = null;
    //    }
    //    if (talkingCoroutineAnimationReference != null)
    //    {
    //        StopCoroutine(talkingCoroutineAnimationReference);
    //        talkingCoroutineAnimationReference = null;
    //    }

    //    talkingCoroutineReference = Coroutine_Talking(_textList);
    //    StartCoroutine(talkingCoroutineReference);
    //}
    public void InitiateDialogue(DialogueObject[] _textList)
    {

        if (talkingCoroutineReference != null)
        {
            StopCoroutine(talkingCoroutineReference);
            talkingCoroutineReference = null;
        }
        if (talkingCoroutineAnimationReference != null)
        {
            StopCoroutine(talkingCoroutineAnimationReference);
            talkingCoroutineAnimationReference = null;
        }

        talkingCoroutineReference = Coroutine_Talking(_textList);
        StartCoroutine(talkingCoroutineReference);
    }
    IEnumerator testpauser()
    {
        LevelManager_Script lvlmanger = FindObjectOfType<LevelManager_Script>();
        while (true)
        {
            if (lvlmanger.IsInPauseMenu && !lvlmanger.inCutscene)
            {
                speaker.pitch = 0;
                speaker2.pitch = 0;
            }
            else
            {
                speaker.pitch = 1;
                speaker2.pitch = 1;
            }

            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        }

        yield break;
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(testpauser());

        bg2Bounds = GameObject.Find("bg2").transform;

        dialoguetext = GameObject.Find("dialoguesample").GetComponent<TMP_Text>();
        playerreference = FindObjectOfType<PlayerBox_Script>().transform;

        dialoguetext.text = "";
        //string[] dialogueSet_Intro = new string[]
        //{
        //    "Welcome, Tester.",
        //    "Preparations for the main event are still underway.",
        //    "Feel free to break the game as you please.",
        //    "Or not.",
        //};
        //talkingCoroutineReference = Coroutine_Talking(dialogueSet_Intro);
        //StartCoroutine(talkingCoroutineReference);
    }

    // Update is called once per frame
    void Update()
    {
        //Inner iris subtlety
        timerToMoveEye -= Time.deltaTime;
        if (timerToMoveEye < Mathf.Epsilon)
        {
            timerToMoveEye = timeToMoveEye;
            if (Random.Range(0.0f, 1.0f) <= 0.5f)
            {
                if (TrueMoveEye.x < -Mathf.Epsilon || TrueMoveEye.x > Mathf.Epsilon)
                {
                    TrueMoveEye.x = 0;
                }
                else
                {
                    if (Random.Range(0.0f, 1.0f) <= 0.5f)
                    {
                        TrueMoveEye.x = 1;
                    }
                    else
                    {
                        TrueMoveEye.x = -1;
                    }
                }
            }
            else
            {
                if (TrueMoveEye.y < -Mathf.Epsilon || TrueMoveEye.y > Mathf.Epsilon)
                {
                    TrueMoveEye.y = 0;
                }
                else
                {
                    if (Random.Range(0.0f, 1.0f) <= 0.5f)
                    {
                        TrueMoveEye.y = 1;
                    }
                    else
                    {
                        TrueMoveEye.y = -1;
                    }
                }
            }
        }

        if (Mathf.Abs(TargetMoveEye.x - TrueMoveEye.x) > 8 * Time.deltaTime)
        {
            TargetMoveEye.x += ((TrueMoveEye.x - TargetMoveEye.x) / Mathf.Abs((TrueMoveEye.x - TargetMoveEye.x))) * 8 * Time.deltaTime;
        }
        else
        {
            TargetMoveEye.x = TrueMoveEye.x;
        }
        if (Mathf.Abs(TargetMoveEye.y - TrueMoveEye.y) > 8 * Time.deltaTime)
        {
            TargetMoveEye.y += ((TrueMoveEye.y - TargetMoveEye.y) / Mathf.Abs((TrueMoveEye.y - TargetMoveEye.y))) * 8 * Time.deltaTime;
        }
        else
        {
            TargetMoveEye.y = TrueMoveEye.y;
        }

        GetComponent<SpriteRenderer>().material.SetFloat("_InnerEyeOffsetX", TargetMoveEye.x);
        GetComponent<SpriteRenderer>().material.SetFloat("_InnerEyeOffsetY", TargetMoveEye.y);

        ////>>>>>>>>>>>>>




        Vector2 ScreenMax = new Vector2(Camera.main.transform.position.x + Camera.main.orthographicSize * (Screen.width / Screen.height), Camera.main.transform.position.y + Camera.main.orthographicSize);
        Vector2 ScreenMin = new Vector2(Camera.main.transform.position.x - Camera.main.orthographicSize * (Screen.width / Screen.height), Camera.main.transform.position.y - Camera.main.orthographicSize);
        Vector2 playerScreenRelativeCoords = new Vector2((playerreference.position.x - ScreenMin.x) / (ScreenMax.x - ScreenMin.x), (playerreference.position.y - ScreenMin.y) / (ScreenMax.y - ScreenMin.y));

        //if (TrueMovePos.x > Mathf.Epsilon && playerScreenRelativeCoords.x >= MovePosPercentThreshold)
        //{
        //    TrueMovePos.x = -1;
        //}
        //else if (TrueMovePos.x < -Mathf.Epsilon && playerScreenRelativeCoords.x < 1 - MovePosPercentThreshold)
        //{
        //    TrueMovePos.x = 1;
        //}
        //if (TrueMovePos.y > Mathf.Epsilon && playerScreenRelativeCoords.y >= MovePosPercentThreshold)
        //{
        //    TrueMovePos.y = -1;
        //}
        //else if (TrueMovePos.y < -Mathf.Epsilon && playerScreenRelativeCoords.y < 1 - MovePosPercentThreshold)
        //{
        //    TrueMovePos.y = 1;
        //}

        if (AnchoredFloat_IsRight && playerScreenRelativeCoords.x >= MovePosPercentThresholdX)
        {
            AnchoredFloat_IsRight = false;
        }
        else if (!AnchoredFloat_IsRight && playerScreenRelativeCoords.x < 1 - MovePosPercentThresholdX)
        {
            AnchoredFloat_IsRight = true;
        }
        if (AnchoredFloat_IsTop && playerScreenRelativeCoords.y >= MovePosPercentThresholdY)
        {
            AnchoredFloat_IsTop = false;
        }
        else if (!AnchoredFloat_IsTop && playerScreenRelativeCoords.y < 1 - MovePosPercentThresholdY)
        {
            AnchoredFloat_IsTop = true;
        }

        float targetRightAnchor;
        if (AnchoredFloat_IsRight) targetRightAnchor = 1;
        else targetRightAnchor = -1;
        if (Mathf.Abs(rightanchorslow - targetRightAnchor) > (1.0f / 0.75f) * Time.deltaTime)
        {
            rightanchorslow += ((targetRightAnchor - rightanchorslow) / Mathf.Abs(targetRightAnchor - rightanchorslow)) * (1.0f / 0.75f) * Time.deltaTime;
        }
        else
        {
            rightanchorslow = targetRightAnchor;
        }

        float goCloser = 1.0f;
        if (talkingCoroutineReference != null)
        {
            goCloser *= talkingDistMag;
        }
        TrueMovePos.x = (playerreference.position.x + rightanchorslow * distMagTestX * goCloser - Camera.main.transform.position.x) / MovePosExtents.x;
        if (AnchoredFloat_IsTop)
        {
            TrueMovePos.y = (playerreference.position.y + distMagTestY * goCloser - Camera.main.transform.position.y) / MovePosExtents.y
                + 2 * Mathf.Sin(Mathf.PI * (0.5f + 0.5f * rightanchorslow))
                ;
        }
        else
        {
            TrueMovePos.y = (playerreference.position.y - distMagTestY * goCloser - Camera.main.transform.position.y) / MovePosExtents.y
                - 2 * Mathf.Sin(Mathf.PI * (0.5f + 0.5f * rightanchorslow))
                ;
        }

        if ((TargetMovePos - TrueMovePos).magnitude > MovePosSpeed * Time.deltaTime)
        {
            TargetMovePos += (TrueMovePos - TargetMovePos) * MovePosSpeed * Time.deltaTime;
        }
        else
        {
            TargetMovePos = TrueMovePos;
        }
        ActualLocalPositionNoHover = new Vector3(TargetMovePos.x * MovePosExtents.x, TargetMovePos.y * MovePosExtents.y, transform.localPosition.z);

        //Vector2 relativeTrueMove;
        //relativeTrueMove.x = ActualLocalPositionNoHover.x / MovePosExtents.x;
        //relativeTrueMove.y = ActualLocalPositionNoHover.y / MovePosExtents.y;

        transform.localPosition = ActualLocalPositionNoHover + new Vector3(0, 0.8f * Mathf.Sin(Time.time * Mathf.PI), 0);

        //float timeMod8 = Time.time % 8;
        //if (timeMod8 < 6.0f)
        //{
        //    GetComponent<SpriteRenderer>().material.SetFloat("_InnerEyeMultiplier", 1);
        //}
        //else
        //{
        //    timeMod8 -= 6.0f;
        //    GetComponent<SpriteRenderer>().material.SetFloat("_InnerEyeMultiplier", 1.0f - 0.1f * Mathf.Clamp(Mathf.Sin(Mathf.PI * (timeMod8 * 0.5f)) * 4, 0, 1));
        //}



        if (Mathf.Abs(TargetAlpha - TrueAlpha) > 1.1f * Time.deltaTime)
        {
            TargetAlpha += ((TrueAlpha - TargetAlpha) / Mathf.Abs(TrueAlpha - TargetAlpha)) * 1.1f * Time.deltaTime;
        }
        else
        {
            TargetAlpha = TrueAlpha;
        }
        GetComponent<SpriteRenderer>().material.SetFloat("_alphaMultiplier", TargetAlpha * overallAlpha);
        foreach(SpriteRenderer rende in GetComponentsInChildren<SpriteRenderer>())
        {
            rende.material.SetFloat("alphaMag", TargetAlpha * overallAlpha);
        }
        //transform.GetChild(0).GetComponent<SpriteRenderer>().material.SetFloat("_alphaMultiplier", TargetAlpha);


        float targetDemon = 0.0f;
        if (IsDemonVariableSet) targetDemon = 1.0f;
        if (Mathf.Abs(DemonVariable - targetDemon) > (1.0f / DemonVariableTime) * Time.deltaTime)
        {
            DemonVariable += ((targetDemon - DemonVariable) / Mathf.Abs(targetDemon - DemonVariable)) * (1.0f / DemonVariableTime) * Time.deltaTime;
        }
        else
        {
            DemonVariable = targetDemon;
        }
        //GetComponent<SpriteRenderer>().material.SetFloat("_demonVar", DemonVariable);

        ////>>>>>>>>>>

        //Background Pulse Shift

        bg2Bounds.GetComponent<SpriteRenderer>().material.SetFloat("_EyeRelativeX",
            (transform.position.x - (bg2Bounds.position.x - bg2Bounds.GetComponent<SpriteRenderer>().bounds.extents.x)) / bg2Bounds.GetComponent<SpriteRenderer>().bounds.size.x);
        bg2Bounds.GetComponent<SpriteRenderer>().material.SetFloat("_EyeRelativeY",
            (transform.position.y - (bg2Bounds.position.y - bg2Bounds.GetComponent<SpriteRenderer>().bounds.extents.y)) / bg2Bounds.GetComponent<SpriteRenderer>().bounds.size.y);

        ////>>>>>>>>>>

        Vector2 directionalVector = new Vector2(playerreference.transform.position.x - transform.position.x, playerreference.transform.position.y - transform.position.y);
        if (directionalVector.magnitude <= Mathf.Epsilon) TargetCoordinate = new Vector2(0.5f, 0.5f);
        else
        {
            if (directionalVector.magnitude >= 5)
            {
                directionalVector = directionalVector.normalized * 5;
            }
            TargetCoordinate = new Vector2(0.5f, 0.5f) + clampRadius * directionalVector / clampPlayerDistance;
        }

        if ((CurrentCoordinate - TargetCoordinate).magnitude > 2 * Time.deltaTime)
        {
            CurrentCoordinate += (TargetCoordinate - CurrentCoordinate).normalized * 2 * Time.deltaTime;
        }
        else
        {
            CurrentCoordinate = TargetCoordinate;
        }

        transform.GetChild(0).localPosition = -moonglowmag * (CurrentCoordinate - new Vector2(0.5f, 0.5f));
        GetComponent<SpriteRenderer>().material.SetFloat("_eyeX", CurrentCoordinate.x);
        GetComponent<SpriteRenderer>().material.SetFloat("_eyeY", CurrentCoordinate.y);
    }
    public void InitiateSpawn()
    {
        TargetMovePos.x = -0.5f;
        StartCoroutine(Spawncoro());
    }
    IEnumerator Spawncoro()
    {
        overallAlpha = 0;
        for (float coro = 0; coro <= 1.0f; coro += (1 / spawncorotime) * Time.deltaTime)
        {
            //coro = Mathf.Sin(coro * 0.5f * Mathf.PI);
            float goCloser = 1.0f;
            if (talkingCoroutineReference != null)
            {
                goCloser *= talkingDistMag;
            }
            TargetMovePos.x = -0.5f * (1 - coro) + coro * (playerreference.position.x + distMagTestX * goCloser - Camera.main.transform.position.x) / MovePosExtents.x;
            overallAlpha = coro;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        AnchoredFloat_IsRight = true;
        overallAlpha = 1;
        yield break;
    }
}
