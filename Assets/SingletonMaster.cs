using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SingletonMaster : MonoBehaviour
{
    [SerializeField]
    private AudioClip BGM_Menu;
    [SerializeField]
    private AudioClip BGM_Intro;
    [SerializeField]
    private AudioClip BGM_Game;
    [SerializeField]
    private AudioSource MusicPlayer;

    public float InternalVolume = 0.0f;
    public float CurrentVolume = 0.0f;
    public float VolumeChangeTime = 2.0f;

    private static IEnumerator SceneTransCoro = null;

    private static SingletonMaster _instance;

    public static SingletonMaster Instance
    {
        get
        {
            return _instance;
        }
    }
    public bool IsTransitioning
    {
        get { return SceneTransCoro != null; }
    }
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (SceneManager.GetActiveScene().name == "LevelTestScene")
            {
                MusicPlayer.clip = BGM_Intro;
                InternalVolume = 1.0f;
                MusicPlayer.Play();
            }
            else if (SceneManager.GetActiveScene().name == "MainMenuScene")
            {
                MusicPlayer.clip = BGM_Menu;
                InternalVolume = 1.0f;
                MusicPlayer.Play();
            }
        }
    }
    public void SetMusicVolume(float _vol)
    {
        InternalVolume = _vol;
    }
    public void SetMusicTrack_Menu()
    {
        MusicPlayer.clip = BGM_Menu;
        MusicPlayer.Play();
    }
    public void SetMusicTrack_Intro()
    {
        MusicPlayer.clip = BGM_Intro;
        MusicPlayer.Play();
    }
    public void SetMusicTrack_Game()
    {
        MusicPlayer.clip = BGM_Game;
        MusicPlayer.Play();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(InternalVolume - CurrentVolume) > (1.0f / VolumeChangeTime) * Time.unscaledDeltaTime)
        {
            CurrentVolume += ((InternalVolume - CurrentVolume) / Mathf.Abs((CurrentVolume - InternalVolume))) * (1.0f / VolumeChangeTime) * Time.unscaledDeltaTime;
        }
        else
        {
            CurrentVolume = InternalVolume;
        }
        MusicPlayer.volume = Mathf.Pow(CurrentVolume, 1.7f) * 0.25f;
    }
    IEnumerator Coroutine_SceneChange(string _scenename, bool _introversion = false)
    {
        float dura = 0.3f;
        float ogsoundchangetime = VolumeChangeTime;
        VolumeChangeTime = 0.5f;
        Image img = GetComponentInChildren<Image>();

        if (_introversion)
        {
            float eyedura = 0.3f;
            FindObjectOfType<MainMenuSelect>().SpreadOut(0.55f);
            yield return new WaitForSecondsRealtime(0.55f * 1.1f);
            SpriteRenderer[] bigshots = GameObject.Find("bgEye").GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer rend in bigshots)
            {
                rend.material.SetFloat("alphaMag", 0);
                rend.material.SetFloat("_alphaMultiplier", 0);
            }
            for (float coro = 0.0f; coro < 1.0f; coro += (1.0f / eyedura) * Time.unscaledDeltaTime)
            {
                foreach (SpriteRenderer rend in bigshots)
                {
                    rend.material.SetFloat("alphaMag", Mathf.Pow(coro, 1.4f));
                    rend.material.SetFloat("_alphaMultiplier", Mathf.Pow(coro, 1.4f));
                }
                yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
            }
            foreach (SpriteRenderer rend in bigshots)
            {
                rend.material.SetFloat("alphaMag", 1);
                rend.material.SetFloat("_alphaMultiplier", 1);
            }
            yield return new WaitForSecondsRealtime(dura);
        }

        img.material.SetFloat("_Progress", 1);
        SingletonMaster.Instance.SetMusicVolume(0);
        for (float coro = 0.0f; coro < 1.0f; coro += (1.0f / dura) * Time.unscaledDeltaTime)
        {
            img.material.SetFloat("_Progress", 1 - Mathf.Pow(coro, 1.1f));
            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        }
        img.material.SetFloat("_Progress", 0);

        yield return new WaitForSecondsRealtime(0.2f);

        bool thetest = false;
        SceneManager.LoadSceneAsync(_scenename).completed += (loaded) =>
        {
            thetest = true;
        };

        while (!thetest) yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        yield return new WaitForSecondsRealtime(0.5f);

        img.material.SetFloat("_Progress", 0);
        VolumeChangeTime = ogsoundchangetime;
        SingletonMaster.Instance.SetMusicVolume(1);
        if (_scenename == "LevelTestScene")
        {
            SetMusicTrack_Intro();
        }
        else if (_scenename == "MainMenuScene")
        {
            SetMusicTrack_Menu();
        }
        for (float coro = 0.0f; coro < 1.0f; coro += (1.0f / dura) * Time.unscaledDeltaTime)
        {
            img.material.SetFloat("_Progress", Mathf.Pow(coro, 1.1f));
            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        }
        img.material.SetFloat("_Progress", 1);

        Time.timeScale = 1;

        IEnumerator dupe = SceneTransCoro;
        SceneTransCoro = null;
        StopCoroutine(dupe);

        yield break;
    }
    public void ChangeScenes(string _scenename, bool _introversion = false)
    {
        if (SceneTransCoro == null)
        {
            SceneTransCoro = Coroutine_SceneChange(_scenename, _introversion);
            StartCoroutine(SceneTransCoro);
        }
    }
}
