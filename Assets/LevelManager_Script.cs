using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager_Script : MonoBehaviour
{
    private int currentLevelPointer = 0;
    private PlayerBox_Script playerReference;
    public float ScreenTransitionPauseTime = 0.4f;
    private IEnumerator pauseCoroutine = null;
    private IEnumerator menuCoroutine = null;
    private IEnumerator confirmAMenuCoroutine = null;
    private IEnumerator confirmBMenuCoroutine = null;
    public string PauseMenuSceneName = "PauseMenuScene";
    public string ConfirmAMenuSceneName = "ConfirmAPauseMenuScene";
    public string ConfirmBMenuSceneName = "ConfirmBPauseMenuScene";
    [SerializeField]
    private AudioSource SFX_MenuSelectOption;
    [SerializeField]
    private AudioSource SFX_MenuOpen;
    private delegate void Notify();
    private event Notify Event_SoftReset;
    private event Notify Event_HardReset;
    [SerializeField]
    private Transform AspectRatioHolder;
    public float aspectratioCut = 132;
    public float aspectTransitionTime = 0.8f;
    public bool inCutscene = false;

    public bool HasAbilityToRestart = false;
    public DialogueObject[] CutsceneDialogueObjectList;

    [SerializeField]
    private GameObject LaserToKILL;

    public void AtLastPlayMusic()
    {
        SingletonMaster.Instance.SetMusicTrack_Game();
    }
    //IEnumerator MusicRiseCoro() //HAVE FADE OPTIONS WHEN YOU DO SINGLETON
    //{
    //    BGM_Game.volume = 0;
    //    for (float coro = 0; coro < 1.0f; coro += 0.5f * Time.deltaTime)
    //    {
    //        BGM_Game.volume = Mathf.Pow(coro, 1.7f);
    //        yield return new WaitForSeconds(Time.deltaTime);
    //    }
    //    BGM_Game.volume = 1;
    //    yield break;
    //}
    public int GetCurrentLevelPointer
    {
        get { return currentLevelPointer; }
    }
    public Transform GetCurrentLevelHolder
    {
        get { return transform.GetChild(currentLevelPointer); }
    }
    public Transform GetCurrentLevelCheckpoint
    {
        get { return transform.GetChild(currentLevelPointer).GetChild(0); }
    }
    public Bounds GetCurrentLevelBoundary
    {
        get { return transform.GetChild(currentLevelPointer).GetChild(1).GetComponent<BoxCollider2D>().bounds; }
    }
    public Transform GetCurrentLevelObjects
    {
        get { return transform.GetChild(currentLevelPointer).GetChild(2); }
    }

    private KeyCode QuickResetKey;
    private void Awake()
    {
        playerReference = GameObject.FindObjectOfType<PlayerBox_Script>();
        pauseCoroutine = null;

        SystemNotification_Script systemnotiftext = FindObjectOfType<SystemNotification_Script>();
        Event_HardReset += systemnotiftext.OnHardReset;
        Event_SoftReset += systemnotiftext.OnSoftReset;

        QuickResetKey = ControlGetter.GetControls()[ControlGetter.NameOf_ControlQuickreset];
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    void ResetPlayerPos(bool dopause = true)
    {
        playerReference.ResetUponRevive(GetCurrentLevelCheckpoint.position);
        if (dopause)
        DoPauseCoroutine(ScreenTransitionPauseTime);

        for (int i = 0; i < transform.childCount; ++i)
        {
            foreach (Transform trans in transform.GetChild(i).GetChild(2))
            {
                if (trans.GetComponent<Key_Script>())
                {
                    trans.GetComponent<Key_Script>().Reset();
                }

                if (trans.GetComponent<KeyWall_Script>())
                {
                    if (i >= currentLevelPointer)
                        trans.GetComponent<KeyWall_Script>().CloseKeyWall();
                    else trans.GetComponent<KeyWall_Script>().OpenKeyWall();
                }
            }
        }
    }
    public void PlayerAskToReset()
    {
        Event_SoftReset.Invoke();
        ResetPlayerPos();
    }
    public float PauseMenuSpeedMultiplier
    {
        get
        {
            if (menuCoroutine != null && !inCutscene) return 0.0f;
            else return 1.0f;
        }
    }
    public bool IsInPauseMenu
    {
        get
        {
            if (menuCoroutine != null) return true;
            else return false;
        }
    }
    IEnumerator Coroutine_PauseScreenTransition(float _timer)
    {
        Time.timeScale = 0;
        for (float coro = 0; coro <= 1.0f; coro += Time.unscaledDeltaTime * (1.0f / _timer))
        {
            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        }
        Time.timeScale = 1;
        IEnumerator currentCoroToNuke = pauseCoroutine;
        pauseCoroutine = null;
        StopCoroutine(currentCoroToNuke);
        yield break;
    }
    IEnumerator Coroutine_Menu()
    {
        SceneManager.LoadSceneAsync(PauseMenuSceneName, LoadSceneMode.Additive).completed += (loadedscene) =>
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(PauseMenuSceneName));
        };
        Time.timeScale = 0;
        while (true)
        {

            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);

            if (Input.GetKeyDown(KeyCode.Escape) && confirmAMenuCoroutine == null && confirmBMenuCoroutine == null)
            {
                EndMenuCoroutine();
                break;
            }
        }
        yield break;
    }
    IEnumerator Coroutine_ConfirmA()
    {
        SceneManager.LoadSceneAsync(ConfirmAMenuSceneName, LoadSceneMode.Additive).completed += (loadedscene) =>
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(ConfirmAMenuSceneName));
        };
        while (true)
        {

            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime); //JUST IN CASE
                EndConfirmAMenuCoroutine();
                break;
            }
        }
        yield break;
    }
    IEnumerator Coroutine_ConfirmB()
    {
        SceneManager.LoadSceneAsync(ConfirmBMenuSceneName, LoadSceneMode.Additive).completed += (loadedscene) =>
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(ConfirmBMenuSceneName));
        };
        while (true)
        {

            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime); //JUST IN CASE
                EndConfirmBMenuCoroutine();
                break;
            }
        }
        yield break;
    }
    IEnumerator AspectRatioStart()
    {
        RectTransform topborder = AspectRatioHolder.GetChild(0).GetComponent<RectTransform>();
        RectTransform bottomborder = AspectRatioHolder.GetChild(1).GetComponent<RectTransform>();
        topborder.GetComponent<Image>().enabled = true;
        topborder.anchoredPosition = new Vector3(0, aspectratioCut, 0);
        bottomborder.anchoredPosition = new Vector3(0, -aspectratioCut, 0);
        bottomborder.GetComponent<Image>().enabled = true;

        for (float coro = 0; coro < 1.0f; coro += Time.deltaTime * (1 / aspectTransitionTime))
        {
            topborder.anchoredPosition = new Vector3(0, (1 - coro) * aspectratioCut, 0);
            bottomborder.anchoredPosition = new Vector3(0, (1 - coro) * -aspectratioCut, 0);
            yield return new WaitForSeconds(Time.deltaTime);
        }

        topborder.anchoredPosition = new Vector3(0, 0, 0);
        bottomborder.anchoredPosition = new Vector3(0, 0, 0);

        yield break;
    }
    IEnumerator AspectRatioEnd()
    {
        RectTransform topborder = AspectRatioHolder.GetChild(0).GetComponent<RectTransform>();
        RectTransform bottomborder = AspectRatioHolder.GetChild(1).GetComponent<RectTransform>();
        topborder.anchoredPosition = new Vector3(0, 0, 0);
        bottomborder.anchoredPosition = new Vector3(0, 0, 0);

        for (float coro = 0; coro < 1.0f; coro += Time.deltaTime * (1 / aspectTransitionTime))
        {
            topborder.anchoredPosition = new Vector3(0, coro * aspectratioCut, 0);
            bottomborder.anchoredPosition = new Vector3(0, coro * -aspectratioCut, 0);
            yield return new WaitForSeconds(Time.deltaTime);
        }

        topborder.anchoredPosition = new Vector3(0, aspectratioCut, 0);
        bottomborder.anchoredPosition = new Vector3(0, -aspectratioCut, 0);

        yield break;
    }
    IEnumerator Coroutine_FirstCutscene()
    {
        EyeScript theEye = FindObjectOfType<EyeScript>();
        if (theEye == null) yield break;
        string[] thelines = new string[] {
            CutsceneDialogueObjectList[0].dialogueText,
            CutsceneDialogueObjectList[1].dialogueText, //dont end previous audio if they are connected
            CutsceneDialogueObjectList[2].dialogueText
        };
        //float time0 = theEye.GetDialogueTimeNoBase(thelines[0]) + theEye.postTalkingTime;
        //float time1 = theEye.GetDialogueTimeNoBase(thelines[1]) + theEye.postTalkingTime;
        //float time2 = theEye.GetDialogueTimeNoBase(thelines[2]) + theEye.postTalkingTime;
        float time0 = theEye.GetDialogueTimeNoBase(CutsceneDialogueObjectList[0]) + theEye.postTalkingTime;
        float time1 = theEye.GetDialogueTimeNoBase(CutsceneDialogueObjectList[1]) + theEye.postTalkingTime;
        float time2 = theEye.GetDialogueTimeNoBase(CutsceneDialogueObjectList[2]) + theEye.postTalkingTime;

        SingletonMaster.Instance.SetMusicVolume(0);

        //LOWER THE CURTAINS
        StartCoroutine(AspectRatioStart());

        GameObject fakelaser = Instantiate(LaserToKILL);
        fakelaser.transform.position = fakelaser.transform.position + new Vector3(playerReference.transform.position.x - fakelaser.transform.position.x, 0, -7.0f);

        foreach (SpriteRenderer rend in fakelaser.GetComponentsInChildren<SpriteRenderer>())
        {
            rend.material.SetColor("_NEOColor", new Color(1, 1, 1, 1));
        }

        bool haskill = false;
        for (float coro = 0; coro < 1.0f; coro += (1.0f / 0.075f) * Time.deltaTime)
        {
            if (!haskill && coro >= 0.5f)
            {
                haskill = true;
                playerReference.SupremeDeath();
            }
            foreach(SpriteRenderer rend in fakelaser.GetComponentsInChildren<SpriteRenderer>())
            {
                rend.material.SetFloat("_NEOValue", coro);
                rend.material.SetFloat("_alphaMag", coro);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }

        for (float coro = 0; coro < 1.0f; coro += (1.0f / 0.2f) * Time.deltaTime)
        {
            foreach (SpriteRenderer rend in fakelaser.GetComponentsInChildren<SpriteRenderer>())
            {
                rend.material.SetFloat("_NEOValue", 1 - coro);
                rend.material.SetFloat("_alphaMag", 1 - coro);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        Destroy(fakelaser);

        //WITH ZEPHAN'S
        //AtLastPlayMusic();

        //START SPEAKING, AND DO THINGS ACCORDING TO THE TIME
        theEye.InitiateDialogue(new DialogueObject[] { CutsceneDialogueObjectList[0] });
        yield return new WaitForSeconds(time0 * 0.6f);

        //REVIVE THE HOMIES WITHOUT PAUSE, EXAGGERATE, AND PLAY MUSIC
        playerReference.ReviveSupreme();
        Event_SoftReset.Invoke();
        playerReference.ResetUponReviveSupreme(GetCurrentLevelCheckpoint.position, time0 * 0.4f + time1 * 0.2f);

        yield return new WaitForSeconds(time0 * 0.4f);

        //AtLastPlayMusic(); //WITH VUK's A
        //ResetPlayerPos(false);

        for (int i = 0; i < transform.childCount; ++i)
        {
            foreach (Transform trans in transform.GetChild(i).GetChild(2))
            {
                if (trans.GetComponent<Key_Script>())
                {
                    trans.GetComponent<Key_Script>().Reset();
                }

                if (trans.GetComponent<KeyWall_Script>())
                {
                    if (i >= currentLevelPointer)
                        trans.GetComponent<KeyWall_Script>().CloseKeyWall();
                    else trans.GetComponent<KeyWall_Script>().OpenKeyWall();
                }
            }
        }

        HasAbilityToRestart = true;

        theEye.InitiateDialogue(new DialogueObject[] { CutsceneDialogueObjectList[1], CutsceneDialogueObjectList[2]});

        yield return new WaitForSeconds(0.5f);

        //VUK B
        SingletonMaster.Instance.SetMusicVolume(1);
        AtLastPlayMusic();

        yield return new WaitForSeconds(time1 + theEye.postTalkingTime + time2 + theEye.postTalkingTime);

        //LIFT THE CURTAINS
        StartCoroutine(AspectRatioEnd());

        inCutscene = false;
        IEnumerator tempcoro = menuCoroutine;
        menuCoroutine = null;
        StopCoroutine(tempcoro);
        yield break;
    }
    public void InitializeFirstCutscene()
    {
        //occupy menu coroutine
        if (menuCoroutine != null)
        {
            StopCoroutine(menuCoroutine);
            menuCoroutine = null;
        }
        inCutscene = true;
        menuCoroutine = Coroutine_FirstCutscene();
        StartCoroutine(menuCoroutine);
    }
    void EndMenuCoroutine()
    {
        SingletonMaster.Instance.SetMusicVolume(1);
        SFX_MenuSelectOption.Play();

        if (confirmAMenuCoroutine != null)
        {
            SceneManager.UnloadSceneAsync(ConfirmAMenuSceneName);
            StopCoroutine(confirmAMenuCoroutine);
            confirmAMenuCoroutine = null;
        }
        if (confirmBMenuCoroutine != null)
        {
            SceneManager.UnloadSceneAsync(ConfirmBMenuSceneName);
            StopCoroutine(confirmBMenuCoroutine);
            confirmBMenuCoroutine = null;
        }

        SceneManager.UnloadSceneAsync(PauseMenuSceneName);
        Time.timeScale = 1;
        StopCoroutine(menuCoroutine);
        menuCoroutine = null;
    }
    void EndConfirmAMenuCoroutine()
    {
        SFX_MenuSelectOption.Play();

        SceneManager.UnloadSceneAsync(ConfirmAMenuSceneName);
        StopCoroutine(confirmAMenuCoroutine);
        confirmAMenuCoroutine = null;
    }
    void EndConfirmBMenuCoroutine()
    {
        SFX_MenuSelectOption.Play();

        SceneManager.UnloadSceneAsync(ConfirmBMenuSceneName);
        StopCoroutine(confirmBMenuCoroutine);
        confirmBMenuCoroutine = null;
    }
    public void PauseDebug_Resume()
    {
        EndMenuCoroutine();
    }
    public void PauseDebug_EndConfirmA()
    {
        EndConfirmAMenuCoroutine();
    }
    public void PauseDebug_EndConfirmB()
    {
        EndConfirmBMenuCoroutine();
    }
    public string GetActiveSceneName()
    {
        if (confirmAMenuCoroutine != null) return ConfirmAMenuSceneName;
        if (confirmBMenuCoroutine != null) return ConfirmBMenuSceneName;
        if (menuCoroutine != null && !inCutscene) return PauseMenuSceneName;
        return "";
    }
    public void PauseDebug_LaunchConfirmA()
    {
        SFX_MenuOpen.Play();

        if (confirmAMenuCoroutine != null)
        {
            SceneManager.UnloadSceneAsync(ConfirmAMenuSceneName);
            StopCoroutine(confirmAMenuCoroutine);
            confirmAMenuCoroutine = null;
        }

        confirmAMenuCoroutine = Coroutine_ConfirmA();
        StartCoroutine(confirmAMenuCoroutine);
    }
    public void PauseDebug_LaunchConfirmB()
    {
        SFX_MenuOpen.Play();

        if (confirmBMenuCoroutine != null)
        {
            SceneManager.UnloadSceneAsync(ConfirmBMenuSceneName);
            StopCoroutine(confirmBMenuCoroutine);
            confirmBMenuCoroutine = null;
        }

        confirmBMenuCoroutine = Coroutine_ConfirmB();
        StartCoroutine(confirmBMenuCoroutine);
    }
    public void PauseDebug_RestartSoft()
    {
        EndMenuCoroutine();
        Event_SoftReset.Invoke();
        ResetPlayerPos();
    }
    public void PauseDebug_RestartHard()
    {
        EndMenuCoroutine();
        SingletonMaster.Instance.CurrentVolume = 0.3f;
        SingletonMaster.Instance.SetMusicTrack_Intro();
        currentLevelPointer = 0;
        Event_HardReset.Invoke();
        HasAbilityToRestart = false;
        ResetPlayerPos(false);
        Camera.main.GetComponent<CameraScript>().HardReset();
        FindObjectOfType<EyeScript>().ResetLaserDeathCounters();
        FindObjectOfType<EyeScript>().ForceEndDialogue();
        FindObjectOfType<EyeScript>().overallAlpha = 0;
        for (int i = 0; i < transform.childCount; ++i)
        {
            if (transform.GetChild(i).GetComponent<DialogueTriggerOnce_Script>())
            {
                transform.GetChild(i).GetComponent<DialogueTriggerOnce_Script>().cantrigger = true;
            }
            if (transform.GetChild(i).GetComponent<VictoryTriggerOnce_Script>())
            {
                transform.GetChild(i).GetComponent<VictoryTriggerOnce_Script>().cantrigger = true;
            }

            foreach (Transform trans in transform.GetChild(i).GetChild(2))
            {
                if (trans.GetComponent<OneTimeDeathScript>())
                {
                    trans.GetComponent<OneTimeDeathScript>().cankill = true;
                }
                if (trans.GetComponent<DialogueTriggerOnce_Script>())
                {
                    trans.GetComponent<DialogueTriggerOnce_Script>().cantrigger = true;
                }
                if (trans.GetComponent<EyeInitTrigger_Script>())
                {
                    trans.GetComponent<EyeInitTrigger_Script>().cantrigger = true;
                }
            }
        }
    }
    public void RestartHardPlain()
    {
        currentLevelPointer = 0;
        SingletonMaster.Instance.CurrentVolume = 0.3f;
        SingletonMaster.Instance.SetMusicTrack_Intro();
        Event_HardReset.Invoke();
        HasAbilityToRestart = false;
        ResetPlayerPos(false);
        Camera.main.GetComponent<CameraScript>().HardReset();
        FindObjectOfType<EyeScript>().ResetLaserDeathCounters();
        FindObjectOfType<EyeScript>().ForceEndDialogue();
        FindObjectOfType<EyeScript>().overallAlpha = 0;
        for (int i = 0; i < transform.childCount; ++i)
        {
            if (transform.GetChild(i).GetComponent<DialogueTriggerOnce_Script>())
            {
                transform.GetChild(i).GetComponent<DialogueTriggerOnce_Script>().cantrigger = true;
            }
            if (transform.GetChild(i).GetComponent<VictoryTriggerOnce_Script>())
            {
                transform.GetChild(i).GetComponent<VictoryTriggerOnce_Script>().cantrigger = true;
            }


            foreach (Transform trans in transform.GetChild(i).GetChild(2))
            {
                if (trans.GetComponent<OneTimeDeathScript>())
                {
                    trans.GetComponent<OneTimeDeathScript>().cankill = true;
                }
                if (trans.GetComponent<DialogueTriggerOnce_Script>())
                {
                    trans.GetComponent<DialogueTriggerOnce_Script>().cantrigger = true;
                }
                if (trans.GetComponent<EyeInitTrigger_Script>())
                {
                    trans.GetComponent<EyeInitTrigger_Script>().cantrigger = true;
                }
            }
        }
    }
    public void PauseDebug_ExitToMenu()
    {
        //inCutscene = true;
        SFX_MenuSelectOption.Play();
        //Time.timeScale = 1;
        SingletonMaster.Instance.ChangeScenes("MainMenuScene");
    }
    public void Force_ExitToMenu() //debug
    {
        //inCutscene = true;
        SingletonMaster.Instance.ChangeScenes("MainMenuScene");
    }
    void DoPauseCoroutine(float _pauseTime)
    {
        if (pauseCoroutine != null)
        {
            StopCoroutine(pauseCoroutine);
            pauseCoroutine = null;
        }

        pauseCoroutine = Coroutine_PauseScreenTransition(_pauseTime);
        StartCoroutine(pauseCoroutine);
    }
    void DoMenuCoroutine()
    {
        SingletonMaster.Instance.SetMusicVolume(0.5f);
        SFX_MenuOpen.Play();

        if (menuCoroutine != null)
        {
            StopCoroutine(menuCoroutine);
            menuCoroutine = null;
        }

        menuCoroutine = Coroutine_Menu();
        StartCoroutine(menuCoroutine);
    }
    // Update is called once per frame
    void Update()
    {
        if (SingletonMaster.Instance.IsTransitioning) return;

        if (pauseCoroutine == null && menuCoroutine == null && playerReference.isInDeadState == false)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DoMenuCoroutine();
            }

            if (HasAbilityToRestart && Input.GetKeyDown(QuickResetKey))
            {
                Event_SoftReset.Invoke();
                ResetPlayerPos();
            }
        }

        //for (int i = currentLevelPointer + 1; i < transform.childCount; ++i)
        //{
        //    if (transform.GetChild(i).GetChild(1).GetComponent<BoxCollider2D>().bounds.Contains(playerReference.transform.position))
        //    {
        //        currentLevelPointer = i;
        //        DoPauseCoroutine(ScreenTransitionPauseTime);
        //    }
        //}
        for (int i = 0; i < transform.childCount; ++i)
        {
            if (i == currentLevelPointer) continue;

            if (transform.GetChild(i).GetChild(1).GetComponent<BoxCollider2D>().bounds.Contains(playerReference.transform.position))
            {
                if (transform.GetChild(i).GetComponent<DialogueTriggerOnce_Script>()
                    && transform.GetChild(i).GetComponent<DialogueTriggerOnce_Script>().cantrigger)
                {
                    transform.GetChild(i).GetComponent<DialogueTriggerOnce_Script>().SendDialogue();
                }
                else if (transform.GetChild(i).GetComponent<VictoryTriggerOnce_Script>()
                    && transform.GetChild(i).GetComponent<VictoryTriggerOnce_Script>().cantrigger)
                {
                    transform.GetChild(i).GetComponent<VictoryTriggerOnce_Script>().NotifyVictory();
                }

                currentLevelPointer = i;
                DoPauseCoroutine(ScreenTransitionPauseTime);
            }
        }
    }
    public bool KeyIsCollectedSomewhere()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            foreach (Transform trans in transform.GetChild(i).GetChild(2))
            {
                if (trans.GetComponent<Key_Script>() && trans.GetComponent<Key_Script>().state == KeyState.COLLECTEDHOVER)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public void TurnAllCollectedKeysUsed()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            foreach (Transform trans in transform.GetChild(i).GetChild(2))
            {
                if (trans.GetComponent<Key_Script>() && trans.GetComponent<Key_Script>().state == KeyState.COLLECTEDHOVER)
                {
                    trans.GetComponent<Key_Script>().Used();
                }
            }
        }
    }
}
