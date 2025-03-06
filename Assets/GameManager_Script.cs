using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

//struct ScorePopup
//{
//    public bool active;
//    public string text;
//    public float time;
//    public float timeLeft;
//    public Color color;
//    public ScorePopup(float _time)
//    {
//        active = false;
//        text = "";
//        time = _time;
//        timeLeft = 0.0f;
//        color = new Color(1, 1, 1, 1);
//    }
//}

public class GameManager_Script : MonoBehaviour
{
    enum ControlScheme
    {
        WASD_SPACE_E,
        ARROWKEYS_Z_SPACE,
    }
    
    ////static private ControlScheme controlScheme = ControlScheme.WASD_SPACE_E;

    //static public InteractableManager_Script InteractableManager;
    //static public CameraEffects_Script CameraEffectsManager;
    //static public ObjectiveBox_Script ObjectiveBox;
    //static public GameObject LevelClearScreen;

    //static private TMP_Text ScoreboxText;
    //static private int ScoreNumber;
    //private int ScoreDisplayed = 0;
    //static private Transform ScorePopupList;
    //static private ScorePopup[] ScorePopups; //Works like a queue

    //private GameObject SceneTransitionEffect;
    //public bool SceneTransitionStarted = false;
    //public bool SceneTransitionOnInit = true;
    //public float timeToSceneTransition = 0.8f;
    //private float timerSceneTransition = 0.0f;
    //[SerializeField]
    //private string NextScene = "TestScene";
    //static private GameObject DeathScreen;

    //static private string DefeatedByWho = "";
    //static private PlayerController_Script killedplayerreference;

    //static private List<GameObject> resetables;

    //[SerializeField]
    //private Material ScreenTransitionMaterial;
    //public void LoadNextScene()
    //{
    //    SceneManager.LoadScene(NextScene);
    //}
    public void LoadParticularScene(string scenename)
    {
        SceneManager.LoadScene(scenename);
    }
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    static public bool GetExitKey
    {
        get { return Input.GetKeyDown(KeyCode.Escape); }
    }
    static public bool GetEnterKey
    {
        get { return Input.GetKeyDown(KeyCode.Return); }
    }
    static public bool GetInteractKey
    {
        get { return Input.GetKeyDown(KeyCode.E); }
    }
    static public bool GetJumpKey
    {
        get { return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Z); }
    }
    static public bool GetJumpKeyDown
    {
        get { return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z); }
    }
    static public bool GetLeftKey
    {
        get { return Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A); }
    }
    static public bool GetRightKey
    {
        get { return Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D); }
    }
    static public bool GetUpKey
    {
        get { return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W); }
    }
    static public bool GetDownKey
    {
        get { return Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S); }
    }
    //static public KeyCode GetInteractKey
    //{
    //    get
    //    {
    //        switch (controlScheme)
    //        {
    //            case ControlScheme.WASD_SPACE_E: return KeyCode.E;
    //            case ControlScheme.ARROWKEYS_Z_SPACE: return KeyCode.Space;
    //            default: return KeyCode.Space;
    //        }
    //    }
    //}
    //static public KeyCode GetJumpKey
    //{
    //    get
    //    {
    //        switch (controlScheme)
    //        {
    //            case ControlScheme.WASD_SPACE_E: return KeyCode.Space;
    //            case ControlScheme.ARROWKEYS_Z_SPACE: return KeyCode.Z;
    //            default: return KeyCode.Space;
    //        }
    //    }
    //}
    //static public KeyCode GetUpKey
    //{
    //    get
    //    {
    //        switch (controlScheme)
    //        {
    //            case ControlScheme.WASD_SPACE_E: return KeyCode.W;
    //            case ControlScheme.ARROWKEYS_Z_SPACE: return KeyCode.UpArrow;
    //            default: return KeyCode.Space;
    //        }
    //    }
    //}
    //static public KeyCode GetDownKey
    //{
    //    get
    //    {
    //        switch (controlScheme)
    //        {
    //            case ControlScheme.WASD_SPACE_E: return KeyCode.S;
    //            case ControlScheme.ARROWKEYS_Z_SPACE: return KeyCode.DownArrow;
    //            default: return KeyCode.Space;
    //        }
    //    }
    //}
    //static public KeyCode GetLeftKey
    //{
    //    get
    //    {
    //        switch (controlScheme)
    //        {
    //            case ControlScheme.WASD_SPACE_E: return KeyCode.A;
    //            case ControlScheme.ARROWKEYS_Z_SPACE: return KeyCode.LeftArrow;
    //            default: return KeyCode.Space;
    //        }
    //    }
    //}
    //static public KeyCode GetRightKey
    //{
    //    get
    //    {
    //        switch (controlScheme)
    //        {
    //            case ControlScheme.WASD_SPACE_E: return KeyCode.D;
    //            case ControlScheme.ARROWKEYS_Z_SPACE: return KeyCode.RightArrow;
    //            default: return KeyCode.Space;
    //        }
    //    }
    //}
    //static public void AddScore(int score, string scoreReason)
    //{
    //    if (ScorePopups == null) return;

    //    ScoreNumber += score;

    //    bool newlyAddedScore = false;

    //    for (int i = 0; i < ScorePopups.Length; ++i)
    //    {
    //        if (ScorePopups[i].active == false)
    //        {
    //            ScorePopups[i].active = true;
    //            ScorePopups[i].text = "+" + score.ToString() + " " + scoreReason;
    //            ScorePopups[i].timeLeft = ScorePopups[i].time;
    //            //ScorePopups[i].color = Color.green * 0.75f;
    //            ScorePopupList.GetChild(i).GetComponent<TMP_Text>().text = ScorePopups[i].text;
    //            newlyAddedScore = true;
    //            break;
    //        }
    //    }

    //    if (newlyAddedScore == false)
    //    {
    //        ScorePopups[0] = ScorePopups[1];
    //        ScorePopupList.GetChild(0).GetComponent<TMP_Text>().text = ScorePopups[0].text;
    //        ScorePopups[1] = ScorePopups[2];
    //        ScorePopupList.GetChild(1).GetComponent<TMP_Text>().text = ScorePopups[1].text;
    //        ScorePopups[2].active = true;
    //        ScorePopups[2].text = "+" + score.ToString() + " " + scoreReason;
    //        ScorePopups[2].timeLeft = ScorePopups[2].time;
    //        //ScorePopups[2].color = Color.green * 0.75f;
    //        ScorePopupList.GetChild(2).GetComponent<TMP_Text>().text = ScorePopups[2].text;
    //    }
    //}
    //static public void DeductScore(int score, string scoreReason)
    //{
    //    ScoreNumber -= score;
    //    if (ScoreNumber < 0) ScoreNumber = 0;

    //    bool newlyAddedScore = false;

    //    for (int i = 0; i < ScorePopups.Length; ++i)
    //    {
    //        if (ScorePopups[i].active == false)
    //        {
    //            ScorePopups[i].active = true;
    //            ScorePopups[i].text = "-" + score.ToString() + " " + scoreReason;
    //            ScorePopups[i].timeLeft = ScorePopups[i].time;
    //            //ScorePopups[i].color = Color.red * 0.5f;
    //            ScorePopupList.GetChild(i).GetComponent<TMP_Text>().text = ScorePopups[i].text;
    //            newlyAddedScore = true;
    //            break;
    //        }
    //    }

    //    if (newlyAddedScore == false)
    //    {
    //        ScorePopups[0] = ScorePopups[1];
    //        ScorePopupList.GetChild(0).GetComponent<TMP_Text>().text = ScorePopups[0].text;
    //        ScorePopups[1] = ScorePopups[2];
    //        ScorePopupList.GetChild(1).GetComponent<TMP_Text>().text = ScorePopups[1].text;
    //        ScorePopups[2].active = true;
    //        ScorePopups[2].text = "-" + score.ToString() + " " + scoreReason;
    //        ScorePopups[2].timeLeft = ScorePopups[2].time;
    //        //ScorePopups[2].color = Color.red * 0.5f;
    //        ScorePopupList.GetChild(2).GetComponent<TMP_Text>().text = ScorePopups[2].text;
    //    }
    //}
    //static public void ResetAllThatObstacles()
    //{
    //    foreach (GameObject obstacle in resetables)
    //    {
    //        if (obstacle.activeSelf)
    //        {
    //            obstacle.GetComponent<Resetable>().ResetObstacle();
    //        }
    //    }
    //}
    //private void Awake()
    //{
    //    Application.targetFrameRate = 60;

    //    InteractableManager = FindObjectOfType<InteractableManager_Script>();
    //    CameraEffectsManager = FindObjectOfType<CameraEffects_Script>();
    //    ObjectiveBox = FindObjectOfType<ObjectiveBox_Script>();

    //    resetables = new List<GameObject>();
    //    foreach(GameObject everyobj in FindObjectsOfType<GameObject>())
    //    {
    //        if (everyobj.GetComponent<Resetable>() != null)
    //        {
    //            resetables.Add(everyobj);
    //        }
    //    }

    //    ScoreNumber = 0;

    //    GameObject Scorebox = GameObject.Find("Scorebox");
    //    if (Scorebox != null)
    //    {
    //        ScoreboxText = Scorebox.transform.GetChild(0).GetComponent<TMP_Text>();
    //        ScoreboxText.text = ScoreNumber.ToString();
    //    }

    //    GameObject ScorePopupListInit = GameObject.Find("ScorePopupList");
    //    if (ScorePopupListInit != null)
    //    {
    //        ScorePopupList = ScorePopupListInit.transform;
    //        ScorePopups = new ScorePopup[3] { new ScorePopup(2.0f), new ScorePopup(2.0f), new ScorePopup(2.0f) };
    //        for (int i = 0; i < ScorePopupList.childCount; ++i)
    //        {
    //            ScorePopupList.GetChild(i).GetComponent<TMP_Text>().text = "";
    //        }
    //    }

    //    LevelClearScreen = GameObject.Find("LevelClearScreen");
    //    if (LevelClearScreen != null)
    //    {
    //        LevelClearScreen.SetActive(false);
    //    }

    //    SceneTransitionEffect = GameObject.Find("ScreenTransitionEffect");
    //    DeathScreen = GameObject.Find("DeathScreen");
    //    if (DeathScreen != null)
    //    {
    //        DeathScreen.GetComponent<Image>().enabled = true;
    //        DeathScreen.SetActive(false);
    //    }
    //    //set to 0 if going in

    //    if (SceneTransitionEffect)
    //    {
    //        if (SceneTransitionEffect.GetComponent<MeshRenderer>())
    //            SceneTransitionEffect.GetComponent<MeshRenderer>().material = new Material(ScreenTransitionMaterial);
    //        else if (SceneTransitionEffect.GetComponent<Image>())
    //            SceneTransitionEffect.GetComponent<Image>().material = new Material(ScreenTransitionMaterial);
    //    }
    //    if (SceneTransitionOnInit)
    //    {
    //        SceneTransitionStarted = true;
    //        timerSceneTransition = timeToSceneTransition;
    //        if (SceneTransitionEffect.GetComponent<MeshRenderer>())
    //            SceneTransitionEffect.GetComponent<MeshRenderer>().material.SetFloat("_Progress", 1 - timerSceneTransition / timeToSceneTransition);
    //        else if (SceneTransitionEffect.GetComponent<Image>())
    //            SceneTransitionEffect.GetComponent<Image>().material.SetFloat("_Progress", 1 - timerSceneTransition / timeToSceneTransition);
    //    }
    //    else
    //    {
    //        if (SceneTransitionEffect.GetComponent<MeshRenderer>())
    //            SceneTransitionEffect.GetComponent<MeshRenderer>().material.SetFloat("_Progress", -1);
    //        else if (SceneTransitionEffect.GetComponent<Image>())
    //            SceneTransitionEffect.GetComponent<Image>().material.SetFloat("_Progress", -1);
    //    }
    //}
    //public static void ToggleDeathScreen(bool enable, PlayerController_Script refobj, string killedbywho = "")
    //{
    //    if (DeathScreen != null)
    //    {
    //        killedplayerreference = refobj.GetComponent<PlayerController_Script>();
    //        DeathScreen.SetActive(enable);
    //        if (killedbywho.CompareTo("") != 0)
    //        {
    //            //DefeatedByWho = " by " + killedbywho;
    //            DefeatedByWho = " by\n" + killedbywho;
    //        }
    //        else
    //        {
    //            DefeatedByWho = "";
    //        }

    //        //DeathScreen.transform.GetChild(0).GetComponent<TMP_Text>().text = "You were defeated" + DefeatedByWho + "!\n\nYou will respawn in " + killedplayerreference.GetRespawnTimer.ToString("0.0") + " seconds...";
    //        DeathScreen.transform.GetChild(0).GetComponent<TMP_Text>().text = "You were defeated" + DefeatedByWho + "!";
    //    }
    //}
    //private void Update()
    //{
    //    if (ScoreboxText != null)
    //    {
    //        if (ScoreDisplayed < ScoreNumber) //positive
    //        {
    //            ScoreDisplayed = (int)Mathf.Clamp(ScoreDisplayed + (int)Mathf.Clamp((ScoreNumber - ScoreDisplayed) * Time.deltaTime, 19, 999999)
    //                , 0, ScoreNumber);
    //        }
    //        else if (ScoreDisplayed > ScoreNumber) //negative
    //        {
    //            ScoreDisplayed = (int)Mathf.Clamp(ScoreDisplayed + (int)Mathf.Clamp((ScoreNumber - ScoreDisplayed) * Time.deltaTime, -999999, -19)
    //                , ScoreNumber, 999999999);
    //        }

    //        ScoreboxText.text = ScoreDisplayed.ToString();
    //        //ScoreboxText.text = ScoreNumber.ToString();
    //    }

    //    if (ScorePopupList != null)
    //    {
    //        for (int i = 0; i < ScorePopups.Length; ++i)
    //        {
    //            if (ScorePopups[i].active)
    //            {
    //                ScorePopups[i].timeLeft -= Time.deltaTime;
    //                if (ScorePopups[i].timeLeft <= 0.0f)
    //                {
    //                    ScorePopups[i].active = false;
    //                    for (int j = i; j < ScorePopups.Length; ++j)
    //                    {
    //                        if (j == ScorePopups.Length - 1)
    //                        {
    //                            ScorePopups[j].active = false;
    //                            break;
    //                        }

    //                        if (ScorePopups[j + 1].active)
    //                            ScorePopups[j] = ScorePopups[j + 1];
    //                        else
    //                        {
    //                            ScorePopups[j].active = false;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        for (int i = 0; i < ScorePopups.Length; ++i)
    //        {
    //            if (ScorePopups[i].active)
    //            {
    //                ScorePopupList.GetChild(i).GetComponent<TMP_Text>().text = ScorePopups[i].text;
    //                ScorePopupList.GetChild(i).GetComponent<TMP_Text>().color = new Color(ScorePopups[i].color.r, ScorePopups[i].color.g, ScorePopups[i].color.b, ScorePopups[i].timeLeft / ScorePopups[i].time);
    //            }
    //            else ScorePopupList.GetChild(i).GetComponent<TMP_Text>().color = new Color(ScorePopups[i].color.r, ScorePopups[i].color.g, ScorePopups[i].color.b, 0);
    //        }
    //    }

    //    if (killedplayerreference != null)
    //    {
    //        DeathScreen.transform.GetChild(0).GetComponent<TMP_Text>().text = "You were defeated" + DefeatedByWho + "!";
    //        //DeathScreen.transform.GetChild(0).GetComponent<TMP_Text>().text = "You were defeated" + DefeatedByWho + "!\n\nYou will respawn in " + killedplayerreference.GetRespawnTimer.ToString("0.0") + " seconds...";
    //    }
    //    if (SceneTransitionStarted)
    //    {
    //        timerSceneTransition = Mathf.Clamp(timerSceneTransition - Time.fixedDeltaTime, 0, timeToSceneTransition);
    //        if (SceneTransitionEffect == null) SceneTransitionEffect = GameObject.Find("ScreenTransitionEffect");
    //        if (SceneTransitionEffect != null)
    //        {
    //            float progressmeter = 0;
    //            if (SceneTransitionOnInit) progressmeter = 1 - timerSceneTransition / timeToSceneTransition;
    //            else progressmeter = 1 - timerSceneTransition / timeToSceneTransition - 1;
    //            if (SceneTransitionEffect.GetComponent<MeshRenderer>())
    //                SceneTransitionEffect.GetComponent<MeshRenderer>().material.SetFloat("_Progress", progressmeter);
    //            else if (SceneTransitionEffect.GetComponent<Image>())
    //                SceneTransitionEffect.GetComponent<Image>().material.SetFloat("_Progress", progressmeter);
    //        }
    //        if (timerSceneTransition <= 0.0f)
    //        {
    //            SceneTransitionStarted = false;
    //            if (SceneTransitionOnInit)
    //            {
    //                SceneTransitionOnInit = false;
    //            }
    //            else
    //            {
    //                if (LevelClearScreen == null)
    //                {
    //                    SceneManager.LoadScene(NextScene);
    //                }
    //                else
    //                {
    //                    LevelClearScreen.SetActive(true);
    //                    string rating = "Good Effort!";
    //                    int totalbottles = InteractableManager.NumberOfBottlesCollected;
    //                    switch (totalbottles)
    //                    {
    //                        case 0: rating = "Good Effort!"; break;
    //                        case 1: rating = "Nice Work!"; break;
    //                        case 2: rating = "Spectacular!"; break;
    //                        default: rating = "Amazingly Awesome!"; break;
    //                    }
    //                    FindObjectOfType<PlayerController_Script>().DisableControl = true;
    //                    InteractableManager.BottleDisplayer.transform.SetParent(LevelClearScreen.transform);
    //                    //InteractableManager.BottleDisplayer.GetComponent<RectTransform>().pivot = new Vector2(0, 1.4f);
    //                    //InteractableManager.BottleDisplayer.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
    //                    //InteractableManager.BottleDisplayer.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
    //                    //InteractableManager.BottleDisplayer.GetComponent<RectTransform>().anchoredPosition = new Vector2();
    //                    //InteractableManager.BottleDisplayer.GetComponent<RectTransform>().localScale *= 1.5f;
    //                    InteractableManager.BottleDisplayer.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
    //                    InteractableManager.BottleDisplayer.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
    //                    InteractableManager.BottleDisplayer.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
    //                    InteractableManager.BottleDisplayer.GetComponent<RectTransform>().anchoredPosition = new Vector2();
    //                    InteractableManager.BottleDisplayer.GetComponent<RectTransform>().localScale *= 1.5f;
    //                    foreach(Transform bottleimg in InteractableManager.BottleDisplayer.GetComponent<RectTransform>())
    //                    {
    //                        float bottlecount = InteractableManager.BottleDisplayer.GetComponent<RectTransform>().childCount; //0 17.5 35
    //                        bottleimg.GetComponent<RectTransform>().anchoredPosition = new Vector2(
    //                            bottleimg.GetSiblingIndex() * -InteractableManager_Script.BottleUIOffset + 0.5f * InteractableManager_Script.BottleUIOffset * (bottlecount - 1), 0);
    //                    }
    //                    LevelClearScreen.transform.GetChild(0).GetComponent<TMP_Text>().text = rating;
    //                    LevelClearScreen.transform.GetChild(2).GetComponent<TMP_Text>().text = "Score: " + ScoreNumber;
    //                }
    //            }
    //        }
    //    }
    //}
    //public void TransitionToNextScene()
    //{
    //    SceneTransitionStarted = true;
    //    timerSceneTransition = timeToSceneTransition;
    //}
}
