using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public enum KeyDirection
{
    DIR_UP,
    DIR_DOWN,
    DIR_LEFT,
    DIR_RIGHT
}
public class KeyCodeWielder
{
    public KeyCodeWielder(KeyCode defaultKeyCode = KeyCode.RightArrow)
    {
        keycodeListener = new List<KeyCode>();
        allHeldKeycodes = new List<KeyCode>();
        keycode = defaultKeyCode;
        timer = 0.0f;
        isActive = false;
    }
    public List<KeyCode> keycodeListener;
    public KeyCode keycode;
    public float timer;
    public bool isActive;
    public List<KeyCode> allHeldKeycodes;
}
public struct PredictedCollision
{
    public Vector3 trajectory;
    public float hitfraction;
    public GameObject hitobj;
    public PredictedCollision(Vector3 traj, float frac, GameObject _hitobj = null)
    {
        trajectory = traj;
        hitfraction = frac;
        hitobj = _hitobj;
    }
}

public class AttachedWind
{
    public Transform reference;
    public float progress = 0;
    public bool recentActivated = true;
    public AttachedWind(Transform ref_, float prog)
    {
        reference = ref_;
        progress = prog;
    }
}

public class PlayerBox_Script : MonoBehaviour
{

    public bool DEBUG_CAMERAMODEACTIVATE = false;

    //Any variables made public here are done so intentionally to be seen and edited within the Unity Editor, not in this script, so that anyone can tweak the Player's physics

    private List<AttachedWind> AttachedWindList = new List<AttachedWind>();
    public float windTransitionTime = 0.12f;
    public float windExitTime = 0.3f;
    public float windForce = 5.0f;

    public float speed = 10.0f; //Player's Movement Speed.
    public float jumpmultiplier = 10.0f; //How high will the Player be able to jump?

    private float initiatedJumpForce = 1.0f; //Player's Zoom Scale when they initialize a jump

    public float gravitationalforce = 9.8f; //Limit of the Gravitational Force.
    private float risinggravity = 0.0f; //The rising Gravitational Force that pulls the Player down when falling.
    public float gravityriserate = 2.0f; //How quickly will the Gravitational Force acting on the falling Player increase?

    [Range(0.0f, 0.2f)]
    public float zoomPercent = 0.1f;

    [Range(0.5f, 2.0f)]
    public float debugGravityPower = 1.0f;

    //public float ropeclimbspeed = 4.0f; //How quickly will the Player climb ropes?
    //public float ropedetachbuffertime = 0.3f; //When the Player jumps off a rope while holding the climbing button, they will not grab onto the rope again by accident for a brief period of time.
    //public float ropedetachbuffer = 0.0f; //Buffer time of not being able to grab a rope after jumping off a rope.
    //public Bounds ropeAttachedBoundary; //The boundaries of the rope that the Player is currently on.

    public bool IsGrounded = true; //Is the Player on the ground, or suspended mid-air?
    public bool IsJumping = false; //Is the Player in the process of jumping or not?
    //public bool IsRoping = false; //Is the Player climbing a rope or not? When the Player is climbing a rope, they would also not be Grounded or Jumping.
    private bool IsDashing = false;
    private bool CanDash = true;
    public float timetodash = 0.6f;
    private float timertodash = 0.0f;
    public float DashForce = 4.0f;
    private Vector3[] dashDirectionalVectors = new Vector3[2]; //MUST ALL BE NORMALIZED

    public float coyoteTimeMax = 0.25f;
    public float coyoteTimer = 0.0f;

    Vector3 currentvector;

    [SerializeField]
    private float TimeToGroundParticle = 0.2f;
    private float TimerToGroundParticle = 0.0f;
    [SerializeField]
    private GameObject GroundParticle_Prefab;
    [SerializeField]
    private GameObject JumpParticle_Jump;
    [SerializeField]
    private GameObject JumpParticle_String;
    [SerializeField]
    private GameObject JumpParticle_Land;
    [SerializeField]
    private GameObject PopVFX;

    public double timetogravity = 0.1f; //What is the minimum amount of time that the Player jumps for after their initial jump before they can fall? 
    private double timertogravity = 0.0f; //Likewise.
    public double timetojump = 0.5f; //What is the maximum amount of time that the Player can jump for after their initial jump? 
    private double timertojump = 0.0f; //Likewise.
    //public AudioSource jumpSoundEffect; //to play the sound effect when jumping
    //public AudioSource disintegrateSoundEffect;
    //public AudioSource jumpLandSoundEffect;
    //public AudioSource walkingSoundEffect;

    public float timeForGoingDownFallSFX = 0.8f; //pow 2
    private float timerForGoingDownFallSFX = 0.0f;

    //private Animator anim;

    public float InternalLeftRightVelocity = 0;
    public float InternalLeftRightBuildupRate = 3.0f;
    public float InternalLeftRightPower = 2.0f;

    private KeyCodeWielder mostRecentHeldDirection;
    public float moveInputBufferMax = 0.04f;
    //[SerializeField]
    //private RenderTexture rendertexture; //ground cam
    //private Texture2D renderreader;

    //[SerializeField]
    //private Color VignetteHurt;

    private Vector2 currentsurfacenormal; //When the Player is on the ground, this will determine the left-right directions that the Player will move towards in order to facilitate seamless slope movement.

    ////When the Player lands on a slope, this variable will help in verifying the steepness of that slope to determine if it counts as being on the ground or not. At 1.0f, only a completely flat platform facing upwards at 90 degrees will be treated as ground, which is quite strict. At 0.5f, all upward surfaces between 45 to 90 degrees will be considered as ground. It is not advisable to set this below 0.5f.
    ////Will explain more on this later, but basically, It verifies the Dot Product between the Normal Vector of the slope and the Normal Vector of an completely flat platform facing upwards at 90 degrees
    [Range(0.0f, 1.0f)]
    public float slopeSteepnessThreshold = 0.95f;

    ////This determines how far the Player's X-Position can be from a Rope's X-Position in order to grab onto it. The higher it is, the more lenient it becomes. It is preferable not to set this to 0.0f;
    //[Range(0.0f, 1.0f)]
    //public float ropeClimbOffset = 0.3f;

    private float lastfallingvalue = 0;

    private float BallProgress = 0.0f; //0 ~ 1

    public bool isGroundPoundEnabled = false;

    private Vector3 PreviousPosition;

    [SerializeField]
    private AudioSource SFX_Jump;
    [SerializeField]
    private AudioSource SFX_Fall;
    [SerializeField]
    private AudioSource SFX_CollectItem;
    [SerializeField]
    private AudioSource SFX_ChangeSize_Forward;
    [SerializeField]
    private AudioSource SFX_ChangeSize_Backward;
    [SerializeField]
    private AudioSource SFX_Landing;
    [SerializeField]
    private AudioSource SFX_DeathNormal;
    [SerializeField]
    private AudioSource SFX_DeathSupreme;
    [SerializeField]
    private AudioSource SFX_ReviveNormal;
    [SerializeField]
    private AudioSource SFX_ReviveSupreme;
    [SerializeField]
    private AudioSource UnlockSFX;

    [SerializeField]
    private GameObject DeathVFXPrefab;
    [SerializeField]
    private GameObject DeathSupremeVFXPrefab;
    [SerializeField]
    private GameObject ReviveSupremePrefab;
    [SerializeField]
    private GameObject SparkVFXPrefab;
    public int SparkAMT;

    private LevelManager_Script levelMangerReference;

    public bool isInDeadState = false;

    //private GameObject InteractableInFocus;

    //public float RespawnTime = 1.0f;
    //private float RespawnTimer = 0.0f;

    ////Needs proper speed transition, but just for now...
    //public bool RunningMode = false;
    //public float RunningSpeed = 8;

    //public bool IsDead = false;
    //public bool DisableControl = false; //For cutscenes, scene transition, death
    //public float timetoDisintengrate = 0.3f;
    //private float timerDisintegrate = 0.0f;
    //private ClampedFloatParameter HurtVignetteIntensityRef;
    //private bool PostDeathSequence = false; //showing cause of death

    //private bool lagkill = false;
    //private float lagkilltimer = 0.0f;

    //[SerializeField]
    //private GameObject DeathEffect;

    //[SerializeField]
    //private GameObject JumpParticle_Jump;
    //[SerializeField]
    //private GameObject JumpParticle_String;
    //[SerializeField]
    //private GameObject JumpParticle_Land;

    //[SerializeField]
    //private GameObject GroundParticle_Grass;
    //[SerializeField]
    //private GameObject GroundParticle_Leaf;
    //[SerializeField]
    //private GameObject GroundParticle_Wood;
    //[SerializeField]
    //private float TimeToGroundParticle = 0.4f;
    //private float TimerToGroundParticle = 0.0f;

    //private Vector3[] checkpoints;
    //private int checkpointcurrent = 0;

    ////[Range(0.0f, 4.0f)]
    //public float VineJumpoffBoost = 0.2f;
    //private bool VineJumpoffRight = true; //otherwise left
    //private float VineJumpoffTimer = 0.0f;
    //public float VineJumpoffTime = 0.5f;

    //public int ScoreDeductionPerDeath = 1000;

    //private string DeathKilledByWho = "";

    [Range(0.1f, 10.0f)]
    public float ZoomScale = 1.0f;

    public int ZoomLvlPointerStarting = 1;
    private int ZoomLvlPointer = 1;
    public float[] ZoomLvlStorage = new float[3] { 0.25f, 1.0f, 1.5f };
    private float ZoomScaleTarget = 1.0f;

    private bool disableGravity = false;

    private float PlayerToCameraRatio;

    public float ZoomSpeed = 5;

    private Dictionary<string, KeyCode> CurrentKeybindings;

    private bool GetJumpKey
    {
        get { return Input.GetKey(CurrentKeybindings[ControlGetter.NameOf_ControlJump]); }
    }
    private bool GetJumpKeyDown
    {
        get { return Input.GetKeyDown(CurrentKeybindings[ControlGetter.NameOf_ControlJump]); }
    }
    private bool GetLeftKey
    {
        get { return Input.GetKey(CurrentKeybindings[ControlGetter.NameOf_ControlLeft]); }
    }
    private bool GetRightKey
    {
        get { return Input.GetKey(CurrentKeybindings[ControlGetter.NameOf_ControlRight]); }
    }
    private KeyCode GetShrinkKeycode
    {
        get { return CurrentKeybindings[ControlGetter.NameOf_ControlShrink]; }
    }

    private Vector3 InitialPosition;
    [SerializeField]
    private Transform BodyHolder;
    [SerializeField]
    private Transform EyesHolder;
    public float turnBodyTime = 0.3f;
    private float eyeL_Local_Left = -0.772f;
    private float eyeL_Local_Right = -0.269f;
    private float eyeR_Local_Left = 0.269f;
    private float eyeR_Local_Right = 0.772f;
    private float eyeL_Internal = -0.269f;
    private float eyeL_Current = -0.269f;
    private float eyeR_Internal = 0.772f;
    private float eyeR_Current = 0.772f;
    private float bodyFacing_Internal = 1;
    private float bodyFacing_Current = 1;

    private float eyeHeight_Default = -0.188f;
    //private float eyeHeight_Top = 0;
    //private float eyeHeight_Bottom = -0.29f;
    private float eyeHeight_Top = -0.08f;
    private float eyeHeight_Bottom = -0.148f;
    private float eyeHeight_Internal = -0.188f;
    private float eyeHeight_Current = -0.188f;
    public float turnEyeHeightTime = 0.2f;

    private float morphBody_Progress_Internal = 0; //1 if mid-air, 0 otherwise
    private float morphBody_Progress_Current = 0;
    private float morphBody_VerticalS_Internal = 0; //1 if going upwards, 0 if going downwards
    private float morphBody_VerticalS_Current = 0;
    public float morphBodyTime = 0.2f;

    private float morphPlanX_Progress_Internal = 0;
    private float morphPlanX_Progress_Current = 0;
    private float morphPlanX_RunningTimer = 0;
    public float morphPlanX_Rate = 2;
    public float morphPlanXTime = 0.2f;

    private float idleBody_Progress_Internal = 0;
    private float idleBody_Progress_Current = 0;
    public float idleBody_RunningTimer = 0.0f;
    public float idleBodyTime = 0.2f;

    void SetSpriteFlip(bool right, bool immediate = false)
    {
        if (right)
        {
            eyeL_Internal = eyeL_Local_Right;
            eyeR_Internal = eyeR_Local_Right;
            bodyFacing_Internal = 1;
        }
        else
        {
            eyeL_Internal = eyeL_Local_Left;
            eyeR_Internal = eyeR_Local_Left;
            bodyFacing_Internal = 0;
        }

        //if (right) foreach (SpriteRenderer rend in transform.GetComponentsInChildren<SpriteRenderer>()) rend.flipX = false; else foreach (SpriteRenderer rend in transform.GetComponentsInChildren<SpriteRenderer>()) rend.flipX = true;
        if (immediate)
        {
            eyeL_Current = eyeL_Internal;
            eyeR_Current = eyeR_Internal;
            bodyFacing_Current = bodyFacing_Internal;
        }
    }

    void BallFunction()
    {
        if (IsGrounded) //drop it down NOW
        {
            BallProgress = Mathf.Clamp(BallProgress - (1.0f / 0.2f) * Time.deltaTime, 0, 1);
        }
        else if (!IsGrounded)
        {
            if (IsJumping) //jump (it WILL grow)
            {
                float newValue = 1 - ((float)timertojump / (float)timetojump);
                if (newValue > BallProgress)
                    BallProgress = newValue;
            }
            else //free fall (do nothing)
            {

            }
        }

        transform.GetComponentInChildren<SpriteRenderer>().material.SetFloat("_Progress", BallProgress);
    }

    private void Awake()
    {
        CurrentKeybindings = ControlGetter.GetControls();

        PlayerToCameraRatio = GetComponent<CircleCollider2D>().radius / Camera.main.orthographicSize;
        mostRecentHeldDirection = new KeyCodeWielder();
        mostRecentHeldDirection.keycodeListener.Add(CurrentKeybindings[ControlGetter.NameOf_ControlLeft]);
        mostRecentHeldDirection.keycodeListener.Add(CurrentKeybindings[ControlGetter.NameOf_ControlRight]);
        levelMangerReference = FindObjectOfType<LevelManager_Script>();
        InitialPosition = transform.position;
        ZoomLvlPointer = ZoomLvlPointerStarting;
        coyoteTimer = 0.0f;
        SetZoomScaleStats(ZoomLvlPointer);
        SFX_Fall.volume = 0;
    }
    float groundhitOffset = 0.005f;
    RaycastHit2D GetGroundHitLineLEFT
    {
        get
        {
            float therad = (GetComponent<CircleCollider2D>().bounds.max.y - GetComponent<CircleCollider2D>().bounds.min.y) / 2;
            return Physics2D.Raycast(transform.position + new Vector3(-therad, -therad - groundhitOffset, 0), Vector2.right, therad * 2, LayerMask.GetMask("Platform"));
        }
    }
    bool GroundLineRaycastLEFT
    {
        get
        {
            if (currentvector.y > Mathf.Epsilon) return false;
            RaycastHit2D hit = GetGroundHitLineLEFT;
            if (hit.collider == null) return false;
            RaycastHit2D easygroundhit = Physics2D.Raycast(transform.position, -Vector2.up, GetComponent<Collider2D>().bounds.size.y / 2 + 0.02f, LayerMask.GetMask("Platform"));
            if (easygroundhit == true) return true;

            float theheight = GetComponent<CircleCollider2D>().bounds.max.y - GetComponent<CircleCollider2D>().bounds.min.y;
            float teenyoffset = 0.01f;
            if (hit.point.x < transform.position.x) teenyoffset = -teenyoffset;
            RaycastHit2D emptySpaceChecker =
                Physics2D.Raycast(
                    new Vector3(hit.point.x + teenyoffset, 0.05f * theheight + GetComponent<CircleCollider2D>().bounds.min.y, 0),
                    Vector2.up,
                    theheight * 0.95f,
                    LayerMask.GetMask("Platform")
                    );

            return emptySpaceChecker.collider == null;
        }
    }
    //bool GroundLineRaycastRIGHT
    //{
    //    get
    //    {
    //        if (currentvector.y > Mathf.Epsilon) return false;
    //        RaycastHit2D hit = GetGroundHitLineRIGHT;
    //        if (hit.collider == null) return false;
    //        float theheight = GetComponent<CircleCollider2D>().bounds.max.y - GetComponent<CircleCollider2D>().bounds.min.y;
    //        RaycastHit2D emptySpaceChecker =
    //            Physics2D.Raycast(
    //                new Vector3(hit.point.x, 0.1f * theheight + GetComponent<CircleCollider2D>().bounds.min.y, 0),
    //                Vector2.up,
    //                theheight * 0.9f,
    //                LayerMask.GetMask("Platform")
    //                );

    //        return emptySpaceChecker.collider == null;
    //    }
    //}
    bool GroundLineRaycast
    {
        get
        {
            return GroundLineRaycastLEFT; //|| GroundLineRaycastRIGHT;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void UpdateAccordingToZoom()
    {
        float truescale = 0.4f + 0.6f * Mathf.SmoothStep(ZoomLvlStorage[0], ZoomLvlStorage[1], ZoomScale);

        float ogHeight = GetComponent<CircleCollider2D>().bounds.max.y - GetComponent<CircleCollider2D>().bounds.min.y;
        GetComponent<CircleCollider2D>().radius = //ZoomScale
            truescale
            * 0.5f;
        transform.GetChild(0).localScale = new Vector3(1, 1, 1) * truescale; //* ZoomScale;
        float newHeight = GetComponent<CircleCollider2D>().bounds.max.y - GetComponent<CircleCollider2D>().bounds.min.y;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, GetComponent<Collider2D>().bounds.size.y / 2 + 0.4f, LayerMask.GetMask("Platform"));
        //if (IsGrounded)

        if (IsGrounded && hit) //just in case
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + GetComponent<CircleCollider2D>().bounds.size.y / 2, transform.position.z);
        }
        else
        {
            transform.position += new Vector3(0, newHeight - ogHeight, 0);
        }

        //Camera.main.orthographicSize = GetComponent<CircleCollider2D>().radius / PlayerToCameraRatio;
        Camera.main.orthographicSize = ((Mathf.SmoothStep(ZoomLvlStorage[0], ZoomLvlStorage[1], ZoomScale) * zoomPercent + (1 - zoomPercent)) * 0.5f) / PlayerToCameraRatio;
    }

    float SmallBigZoomWeightMultiplier
    {
        get { return (1 - Mathf.Clamp((ZoomScale - ZoomLvlStorage[0]) / (ZoomLvlStorage[1] - ZoomLvlStorage[0]), 0, 1)); }
    }

    // Update is called once per frame
    void Update()
    {
        if (SingletonMaster.Instance.IsTransitioning)
        {
            if (!isInDeadState)
            {

                if (Mathf.Abs(ZoomScale - ZoomScaleTarget) > Mathf.Epsilon)
                {
                    float changeVal = ((ZoomScaleTarget - ZoomScale) / Mathf.Abs(ZoomScaleTarget - ZoomScale))
                        * Mathf.Clamp(Mathf.Abs(ZoomScaleTarget - ZoomScale), 0.1f, 1.0f) * ZoomSpeed * Time.deltaTime;

                    if (Mathf.Abs(changeVal) > Mathf.Abs(ZoomScaleTarget - ZoomScale))
                    {
                        ZoomScale = ZoomScaleTarget;
                    }
                    else
                        ZoomScale += changeVal;
                }
                float truescale = 0.4f + 0.6f * Mathf.SmoothStep(ZoomLvlStorage[0], ZoomLvlStorage[1], ZoomScale);

                float ogHeight = GetComponent<CircleCollider2D>().bounds.max.y - GetComponent<CircleCollider2D>().bounds.min.y;
                GetComponent<CircleCollider2D>().radius = //ZoomScale
                    truescale
                    * 0.5f;
                transform.GetChild(0).localScale = new Vector3(1, 1, 1) * truescale; //* ZoomScale;
                float newHeight = GetComponent<CircleCollider2D>().bounds.max.y - GetComponent<CircleCollider2D>().bounds.min.y;

                RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, GetComponent<Collider2D>().bounds.size.y / 2 + 0.4f, LayerMask.GetMask("Platform"));
                //if (IsGrounded)

                if (IsGrounded && hit) //just in case
                {
                    transform.position = new Vector3(transform.position.x, hit.point.y + GetComponent<CircleCollider2D>().bounds.size.y / 2, transform.position.z);
                }
                else
                {
                    transform.position += new Vector3(0, newHeight - ogHeight, 0);
                }

                //Camera.main.orthographicSize = GetComponent<CircleCollider2D>().radius / PlayerToCameraRatio;
                Camera.main.orthographicSize = ((Mathf.SmoothStep(ZoomLvlStorage[0], ZoomLvlStorage[1], ZoomScale) * zoomPercent + (1 - zoomPercent)) * 0.5f) / PlayerToCameraRatio;
                FallingFunction();
                CheckIfGrounded();
            }
            return;
        }

        if (DEBUG_CAMERAMODEACTIVATE)
        {
            //if (!IsGrounded)
            {
                if (Time.timeScale > 0.5f)
                    FindObjectOfType<testscript>().PauseToggle();
                else if (Input.GetKeyDown(KeyCode.O))
                {
                    FindObjectOfType<testscript>().Snap();
                }

            }

            if (Time.timeScale < 0.5f)
            {
                if (Input.GetKeyDown(KeyCode.P))
                {

                    FindObjectOfType<testscript>().PauseToggle();
                }
            }
        }

        bool _recentDirInput = false;
        foreach (KeyCode _keycode in mostRecentHeldDirection.keycodeListener)
        {
            if (!mostRecentHeldDirection.allHeldKeycodes.Contains(_keycode) && Input.GetKey(_keycode))
            {
                mostRecentHeldDirection.allHeldKeycodes.Insert(0, _keycode);
            }
            else if (mostRecentHeldDirection.allHeldKeycodes.Contains(_keycode) && !Input.GetKey(_keycode))
            {
                mostRecentHeldDirection.allHeldKeycodes.Remove(_keycode);
            }
        }
        if (mostRecentHeldDirection.allHeldKeycodes.Count > 0) //whoever's at 0 index is the most recent one
        {
            mostRecentHeldDirection.keycode = mostRecentHeldDirection.allHeldKeycodes[0];
            mostRecentHeldDirection.timer = moveInputBufferMax;
            mostRecentHeldDirection.isActive = true;
            _recentDirInput = true;
        }
        if (!_recentDirInput && mostRecentHeldDirection.isActive)
        {
            mostRecentHeldDirection.timer -= Time.unscaledDeltaTime;
            if (mostRecentHeldDirection.timer <= Mathf.Epsilon)
            {
                mostRecentHeldDirection.timer = 0.0f;
                mostRecentHeldDirection.isActive = false;
            }
        }
        if (!isInDeadState)
        {
            if (levelMangerReference.IsInPauseMenu)
            {
                SFX_Fall.volume = 0;
            }

            if (levelMangerReference.inCutscene && !disableGravity) //hardcode
            {
                FallingFunction();
                CheckIfGrounded();
            }

            float windY = 0.0f;

            if (!levelMangerReference.IsInPauseMenu)
            {
                if ((Input.GetKeyDown(GetShrinkKeycode) && ZoomLvlPointer == 1) || ((Input.GetKeyUp(GetShrinkKeycode) || !Input.GetKey(GetShrinkKeycode)) && ZoomLvlPointer == 0))
                {
                    if (ZoomLvlPointer > 0)
                    {
                        SFX_ChangeSize_Forward.Play();
                        ZoomLvlPointer--;
                        ZoomScaleTarget = ZoomLvlStorage[ZoomLvlPointer];

                        //if (PopVFX != null && IsJumping && (timetojump - timertojump) < timetogravity * 2)
                        //{
                        //    GameObject popvfx = Instantiate(PopVFX);
                        //    popvfx.transform.position = transform.position;
                        //}

                    }
                    else
                    {
                        SFX_ChangeSize_Backward.Play();
                        ZoomLvlPointer++;
                        ZoomScaleTarget = ZoomLvlStorage[ZoomLvlPointer];
                    }
                    //ZoomScale = Mathf.Clamp(ZoomScale - 5 * Time.deltaTime, 0.1f, 1);
                }

                if (Mathf.Abs(ZoomScale - ZoomScaleTarget) > Mathf.Epsilon)
                {
                    float changeVal = ((ZoomScaleTarget - ZoomScale) / Mathf.Abs(ZoomScaleTarget - ZoomScale))
                        * Mathf.Clamp(Mathf.Abs(ZoomScaleTarget - ZoomScale), 0.1f, 1.0f) * ZoomSpeed * Time.deltaTime;

                    if (Mathf.Abs(changeVal) > Mathf.Abs(ZoomScaleTarget - ZoomScale))
                    {
                        ZoomScale = ZoomScaleTarget;
                    }
                    else
                        ZoomScale += changeVal;
                }
                UpdateAccordingToZoom();

                Vector3 CurrentPreviousPosition = transform.position;

                {
                    if (IsDashing)
                    {
                        DashInput();
                    }
                    else if (!IsDashing)
                    {
                        FallingFunction();
                        MovementInput();
                    }

                    //coyote time
                    if (IsGrounded)
                    {
                        coyoteTimer = 0.0f;
                    }
                    else
                    {
                        //if (CurrentPreviousPosition.y > transform.position.y)
                        if (!IsJumping || AttachedWindList.Count == 0)
                        {
                            coyoteTimer = Mathf.Clamp(coyoteTimer + Time.deltaTime, 0, coyoteTimeMax);
                        }
                        else
                        {
                            coyoteTimer = coyoteTimeMax;
                        }
                    }

                    CheckIfGrounded();
                }


                currentvector = transform.position - CurrentPreviousPosition;
                if (IsGrounded && Vector3.SqrMagnitude(currentvector) > 0.00001f)
                {
                    TimerToGroundParticle -= Time.deltaTime;
                    if (TimerToGroundParticle <= 0.0f)
                    {
                        TimerToGroundParticle = TimeToGroundParticle;
                        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, GetComponent<Collider2D>().bounds.size.y / 2 + 0.4f, LayerMask.GetMask("Platform"));
                        if (hit)
                        {
                            GameObject obj;
                            obj = Instantiate(GroundParticle_Prefab);
                            obj.transform.position = transform.position + new Vector3(0, -GetComponent<Collider2D>().bounds.extents.y, -1);
                            obj.transform.localScale *= ZoomScale;
                            if (currentvector.x < 0.0f) obj.transform.Rotate(new Vector3(0, 1, 0), 180);
                        }
                    }
                }

                if (currentvector.y < -0.02f && !IsGrounded)
                {
                    timerForGoingDownFallSFX = Mathf.Clamp(timerForGoingDownFallSFX + Time.deltaTime, 0, timeForGoingDownFallSFX);
                }
                else
                {
                    timerForGoingDownFallSFX = Mathf.Clamp(timerForGoingDownFallSFX - 5 * Time.deltaTime, 0, timeForGoingDownFallSFX);
                }

                //Disabled falling sound
                //SFX_Fall.volume = ZoomScale * Mathf.Pow(timerForGoingDownFallSFX / timeForGoingDownFallSFX, 3);

                List<AttachedWind> windToDiscard = new List<AttachedWind>();
                for (int i = 0; i < AttachedWindList.Count; ++i)
                {
                    AttachedWind wind = AttachedWindList[i];

                    wind.progress = Mathf.Clamp(wind.progress, 0, SmallBigZoomWeightMultiplier);
                    wind.recentActivated = false;

                    if (Vector2.Dot(wind.reference.up, Vector2.up) > Mathf.Epsilon) IsGrounded = false;

                    //float trueWindForce = (1 - Mathf.Clamp((ZoomScale - ZoomLvlStorage[1])/(ZoomLvlStorage[2] - ZoomLvlStorage[1]), 0, 1)) * windForce
                    //    + 0.25f * (1 - Mathf.Clamp((ZoomScale - ZoomLvlStorage[0]) / (ZoomLvlStorage[1] - ZoomLvlStorage[0]), 0, 1)) * windForce;

                    float trueWindForce = windForce;

                    if (wind.reference.GetComponent<Wind_Script>())
                    {
                        trueWindForce *= wind.reference.GetComponent<Wind_Script>().WindMultiplier;
                    }

                    //trueWindForce *= SmallBigZoomWeightMultiplier;

                    float boostedWindProgress = 0.2f + 0.8f * wind.progress;

                    if (SmallBigZoomWeightMultiplier > Mathf.Epsilon)
                    {
                        PredictedCollision PredictedMovementVector = PredictCollisionTrajectory(wind.reference.up, boostedWindProgress * Time.deltaTime * trueWindForce, -1.1f, 1.1f, 0.01f);
                        transform.position += PredictedMovementVector.trajectory;
                        windY = PredictedMovementVector.trajectory.y;
                    }

                    if (wind.recentActivated)
                    {
                        wind.recentActivated = false;
                    }
                    else
                    {
                        float additionalWindDownMultiplier = 1;
                        //if (wind.reference.GetComponent<Wind_Script>())
                        //{
                        //    additionalWindDownMultiplier /= wind.reference.GetComponent<Wind_Script>().WindMultiplier;
                        //}

                        wind.progress = Mathf.Clamp(wind.progress - additionalWindDownMultiplier * Time.deltaTime * 1.0f / windExitTime, 0, 1.0f);
                        if (wind.progress <= Mathf.Epsilon)
                        {
                            windToDiscard.Add(wind);
                        }
                    }
                }
                foreach (AttachedWind wind in windToDiscard)
                {
                    AttachedWindList.Remove(wind);
                }

                if (!CanDash && !IsDashing && IsGrounded)
                {
                    CanDash = true;
                }

                BallFunction();

            }

            if (/*DisableControl ||*/ !(GetLeftKey || GetRightKey))
            {
                if (InternalLeftRightVelocity > Mathf.Epsilon)
                {
                    InternalLeftRightVelocity = Mathf.Clamp(InternalLeftRightVelocity - InternalLeftRightBuildupRate * Time.deltaTime, 0, 1);
                }
                else if (InternalLeftRightVelocity < -Mathf.Epsilon)
                {
                    InternalLeftRightVelocity = Mathf.Clamp(InternalLeftRightVelocity + InternalLeftRightBuildupRate * Time.deltaTime, -1, 0);
                }
            }

            if (Mathf.Abs(InternalLeftRightVelocity) > Mathf.Epsilon)
            {
                morphPlanX_Progress_Internal = 1;
            }
            else
            {
                morphPlanX_Progress_Internal = 0;
            }
            if (Mathf.Abs(morphPlanX_Progress_Current - morphPlanX_Progress_Internal) > (1.0f / morphPlanXTime) * Time.deltaTime)
                morphPlanX_Progress_Current += ((morphPlanX_Progress_Internal - morphPlanX_Progress_Current) / Mathf.Abs((morphPlanX_Progress_Internal - morphPlanX_Progress_Current))) * (1.0f / morphPlanXTime) * Time.deltaTime;
            else
                morphPlanX_Progress_Current = morphPlanX_Progress_Internal;

            if (morphPlanX_Progress_Current > Mathf.Epsilon)
            {
                morphPlanX_RunningTimer += Time.deltaTime;
            }
            else
            {
                morphPlanX_RunningTimer = 0;
            }

            if (Mathf.Abs(eyeL_Current - eyeL_Internal) > (1.0f / turnBodyTime) * Time.deltaTime)
                eyeL_Current += ((eyeL_Internal - eyeL_Current) / Mathf.Abs((eyeL_Internal - eyeL_Current))) * (1.0f / turnBodyTime) * Time.deltaTime;
            else
                eyeL_Current = eyeL_Internal;
            if (Mathf.Abs(eyeR_Current - eyeR_Internal) > (1.0f / turnBodyTime) * Time.deltaTime)
                eyeR_Current += ((eyeR_Internal - eyeR_Current) / Mathf.Abs((eyeR_Internal - eyeR_Current))) * (1.0f / turnBodyTime) * Time.deltaTime;
            else
                eyeR_Current = eyeR_Internal;
            if (Mathf.Abs(bodyFacing_Current - bodyFacing_Internal) > (1.0f / turnBodyTime) * Time.deltaTime)
                bodyFacing_Current += ((bodyFacing_Internal - bodyFacing_Current) / Mathf.Abs((bodyFacing_Internal - bodyFacing_Current))) * (1.0f / turnBodyTime) * Time.deltaTime;
            else
                bodyFacing_Current = bodyFacing_Internal;

            if (Mathf.Abs(InternalLeftRightVelocity) <= Mathf.Epsilon && IsGrounded)
            {
                idleBody_Progress_Internal = 1;
                idleBody_RunningTimer += Time.deltaTime;
            }
            else
            {
                idleBody_Progress_Internal = 0;
                idleBody_RunningTimer = 0;
            }

            if (Mathf.Abs(idleBody_Progress_Current - idleBody_Progress_Internal) > (1.0f / idleBodyTime) * Time.deltaTime)
                idleBody_Progress_Current += ((idleBody_Progress_Internal - idleBody_Progress_Current) / Mathf.Abs((idleBody_Progress_Internal - idleBody_Progress_Current))) * (1.0f / idleBodyTime) * Time.deltaTime;
            else
                idleBody_Progress_Current = idleBody_Progress_Internal;

            if (levelMangerReference.inCutscene)
            {
                eyeHeight_Internal = eyeHeight_Default;
                morphBody_Progress_Internal = 0;
                morphBody_VerticalS_Internal = 0;

                float addEyeIdle = 0.08f * idleBody_Progress_Current * (Mathf.Cos(idleBody_RunningTimer * 0.65f * Mathf.PI * 2 + Mathf.PI) * 0.5f + 0.5f);
                EyesHolder.GetChild(0).transform.localPosition = EyesHolder.GetChild(0).transform.localPosition + new Vector3(eyeL_Current - EyesHolder.GetChild(0).transform.localPosition.x, addEyeIdle + eyeHeight_Current - EyesHolder.GetChild(0).transform.localPosition.y, 0);
                EyesHolder.GetChild(1).transform.localPosition = EyesHolder.GetChild(1).transform.localPosition + new Vector3(eyeR_Current - EyesHolder.GetChild(1).transform.localPosition.x, addEyeIdle + eyeHeight_Current - EyesHolder.GetChild(1).transform.localPosition.y, 0);
                BodyHolder.GetComponent<SpriteRenderer>().material.SetFloat("_FlipRight", bodyFacing_Current);
                BodyHolder.GetComponent<SpriteRenderer>().material.SetFloat("_Progress", morphBody_Progress_Current);
                BodyHolder.GetComponent<SpriteRenderer>().material.SetFloat("_ProgressX", morphPlanX_Progress_Current * (0.5f + 0.25f *
                    Mathf.Cos(Mathf.Pow((morphPlanX_RunningTimer * morphPlanX_Rate) % 1.0f, 2) * Mathf.PI * 2)
                    ));
                BodyHolder.GetComponent<SpriteRenderer>().material.SetFloat("_VerticalStretch", morphBody_VerticalS_Current);
                BodyHolder.GetComponent<SpriteRenderer>().material.SetFloat("_IdleProgress", idleBody_Progress_Current);
                BodyHolder.GetComponent<SpriteRenderer>().material.SetFloat("_RunningTimer", idleBody_RunningTimer);
                glowmatselfref.material.SetFloat("_FlipRight", bodyFacing_Current);
                glowmatselfref.material.SetFloat("_Progress", morphBody_Progress_Current);
                glowmatselfref.material.SetFloat("_ProgressX", morphPlanX_Progress_Current * (0.5f + 0.25f *
                    Mathf.Cos(Mathf.Pow((morphPlanX_RunningTimer * morphPlanX_Rate) % 1.0f, 2) * Mathf.PI * 2)
                    ));
                glowmatselfref.material.SetFloat("_VerticalStretch", morphBody_VerticalS_Current);
                glowmatselfref.material.SetFloat("_IdleProgress", idleBody_Progress_Current);
                glowmatselfref.material.SetFloat("_RunningTimer", idleBody_RunningTimer);
            }
            else if (!IsGrounded && (IsJumping || (windY > Mathf.Epsilon && windY + lastfallingvalue > Mathf.Epsilon)))
            {
                eyeHeight_Internal = eyeHeight_Top;
                morphBody_Progress_Internal = 1;
                morphBody_VerticalS_Internal = 1;
            }
            else if (!IsGrounded)
            {
                eyeHeight_Internal = eyeHeight_Bottom;
                morphBody_Progress_Internal = 1;
                morphBody_VerticalS_Internal = 0;
            }
            else
            {
                eyeHeight_Internal = eyeHeight_Default;
                morphBody_Progress_Internal = 0;
                morphBody_VerticalS_Internal = 0;
            }

            if (Mathf.Abs(eyeHeight_Current - eyeHeight_Internal) > (1.0f / turnEyeHeightTime) * Time.deltaTime)
                eyeHeight_Current += ((eyeHeight_Internal - eyeHeight_Current) / Mathf.Abs((eyeHeight_Internal - eyeHeight_Current))) * (1.0f / turnEyeHeightTime) * Time.deltaTime;
            else
                eyeHeight_Current = eyeHeight_Internal;

            if (morphBody_Progress_Internal < 0.5f)
            {
                if (Mathf.Abs(morphBody_Progress_Current - morphBody_Progress_Internal) > (2.0f / morphBodyTime) * Time.deltaTime)
                    morphBody_Progress_Current += ((morphBody_Progress_Internal - morphBody_Progress_Current) / Mathf.Abs((morphBody_Progress_Internal - morphBody_Progress_Current))) * (2.0f / morphBodyTime) * Time.deltaTime;
                else
                    morphBody_Progress_Current = morphBody_Progress_Internal;
            }
            else
            {
                if (Mathf.Abs(morphBody_Progress_Current - morphBody_Progress_Internal) > (1.0f / morphBodyTime) * Time.deltaTime)
                    morphBody_Progress_Current += ((morphBody_Progress_Internal - morphBody_Progress_Current) / Mathf.Abs((morphBody_Progress_Internal - morphBody_Progress_Current))) * (1.0f / morphBodyTime) * Time.deltaTime;
                else
                    morphBody_Progress_Current = morphBody_Progress_Internal;
            }

            if (Mathf.Abs(morphBody_VerticalS_Current - morphBody_VerticalS_Internal) > (1.0f / morphBodyTime) * Time.deltaTime)
                morphBody_VerticalS_Current += ((morphBody_VerticalS_Internal - morphBody_VerticalS_Current) / Mathf.Abs((morphBody_VerticalS_Internal - morphBody_VerticalS_Current))) * (1.0f / morphBodyTime) * Time.deltaTime;
            else
                morphBody_VerticalS_Current = morphBody_VerticalS_Internal;

            if (!levelMangerReference.IsInPauseMenu)
            {
                float addEyeIdle = 0.08f * idleBody_Progress_Current * (Mathf.Cos(idleBody_RunningTimer * 0.65f * Mathf.PI * 2 + Mathf.PI) * 0.5f + 0.5f);
                EyesHolder.GetChild(0).transform.localPosition = EyesHolder.GetChild(0).transform.localPosition + new Vector3(eyeL_Current - EyesHolder.GetChild(0).transform.localPosition.x, addEyeIdle + eyeHeight_Current - EyesHolder.GetChild(0).transform.localPosition.y, 0);
                EyesHolder.GetChild(1).transform.localPosition = EyesHolder.GetChild(1).transform.localPosition + new Vector3(eyeR_Current - EyesHolder.GetChild(1).transform.localPosition.x, addEyeIdle + eyeHeight_Current - EyesHolder.GetChild(1).transform.localPosition.y, 0);
                BodyHolder.GetComponent<SpriteRenderer>().material.SetFloat("_FlipRight", bodyFacing_Current);
                BodyHolder.GetComponent<SpriteRenderer>().material.SetFloat("_Progress", morphBody_Progress_Current);
                BodyHolder.GetComponent<SpriteRenderer>().material.SetFloat("_ProgressX", morphPlanX_Progress_Current * (0.5f + 0.25f *
                    Mathf.Cos(Mathf.Pow((morphPlanX_RunningTimer * morphPlanX_Rate) % 1.0f, 2) * Mathf.PI * 2)
                    ));
                BodyHolder.GetComponent<SpriteRenderer>().material.SetFloat("_VerticalStretch", morphBody_VerticalS_Current);
                BodyHolder.GetComponent<SpriteRenderer>().material.SetFloat("_IdleProgress", idleBody_Progress_Current);
                BodyHolder.GetComponent<SpriteRenderer>().material.SetFloat("_RunningTimer", idleBody_RunningTimer);
                glowmatselfref.material.SetFloat("_FlipRight", bodyFacing_Current);
                glowmatselfref.material.SetFloat("_Progress", morphBody_Progress_Current);
                glowmatselfref.material.SetFloat("_ProgressX", morphPlanX_Progress_Current * (0.5f + 0.25f *
                    Mathf.Cos(Mathf.Pow((morphPlanX_RunningTimer * morphPlanX_Rate) % 1.0f, 2) * Mathf.PI * 2)
                    ));
                glowmatselfref.material.SetFloat("_VerticalStretch", morphBody_VerticalS_Current);
                glowmatselfref.material.SetFloat("_IdleProgress", idleBody_Progress_Current);
                glowmatselfref.material.SetFloat("_RunningTimer", idleBody_RunningTimer);
            }
        }

        if (DEBUG_CAMERAMODEACTIVATE)
        {
            if (Input.GetKeyUp(KeyCode.P))
            {
                FindObjectOfType<testscript>().PauseToggle();
            }
        }

        PreviousPosition = transform.position;
    }


    void FallingFunction() //If the Player is not on the ground, they are supposed to either be jumping or falling.
    {
        if (disableGravity) return;

        if (!IsGrounded)
        {
            timertogravity -= Time.deltaTime;
            if (timertogravity <= 0.0f && !IsJumping) //If the minimum time for a jump has elapsed, and the Player is no longer jumping, begin falling.
            {
                //The Gravitational Force pulling the Player down will increase up to a certain point.
                //if (risinggravity < gravitationalforce) risinggravity += gravityriserate * Time.deltaTime;
                //else risinggravity = gravitationalforce;
                //V2
                float lowestbidder = 1;
                for (int i = 0; i < AttachedWindList.Count; ++i)
                {
                    AttachedWind wind = AttachedWindList[i];
                    float progval = 1 - wind.progress * SmallBigZoomWeightMultiplier; //1 - wind.progress * (1 - Mathf.Clamp((ZoomScale - ZoomLvlStorage[0]) / (ZoomLvlStorage[1] - ZoomLvlStorage[0]), 0, 1));
                    if (progval < lowestbidder)
                    {
                        lowestbidder = progval;
                    }
                }

                float isGroundPoundEnabledMultiplier = 0.0f;
                if (isGroundPoundEnabled) isGroundPoundEnabledMultiplier = 1.0f;

                //additional weight
                float test2 = gravitationalforce;
                if (isGroundPoundEnabled)
                {
                    test2 *= lowestbidder;
                }
                float thecap = test2 + 2 * gravitationalforce * lowestbidder * isGroundPoundEnabledMultiplier * (Mathf.SmoothStep(ZoomLvlStorage[0], ZoomLvlStorage[1], ZoomScale));
                if (risinggravity < thecap) risinggravity += gravityriserate * Time.deltaTime;
                else risinggravity = thecap;

                //Falling cap for small is 33% of big(default)
                //float risingGravityToUse = Mathf.Clamp(risinggravity, 0, gravitationalforce - (2.0f / 3.0f) * gravitationalforce * (1 - Mathf.SmoothStep(ZoomLvlStorage[0], ZoomLvlStorage[1], ZoomScale)));
                float risingGravityToUse = risinggravity;

                //testing wind to cancel out, but quite buggy
                //V1
                //for (int i = 0; i < AttachedWindList.Count; ++i)
                //{
                //    AttachedWind wind = AttachedWindList[i];
                //    float trueWindForceMult = 1;

                //    if (wind.reference.GetComponent<Wind_Script>())
                //    {
                //        trueWindForceMult *= wind.reference.GetComponent<Wind_Script>().WindMultiplier;
                //    }
                //    trueWindForceMult *= (1 - Mathf.Clamp((ZoomScale - ZoomLvlStorage[0]) / (ZoomLvlStorage[1] - ZoomLvlStorage[0]), 0, 1));
                //    risinggravity = Mathf.Clamp(risinggravity - trueWindForceMult * wind.progress * gravityriserate * Time.deltaTime, 0, gravitationalforce);
                //}

                float zoomhi = Mathf.Pow(ZoomScale, debugGravityPower);

                //The Player's position will be affected by my custom physics in a downwards direction with their current Gravitational Force, whereby we do not want the Player to fall past platforms.
                //Since we only expect the Player to successfully land on suitable ground, the Player will become grounded if a collision trajectory is successfully predicted.
                //More on PredictCollisionTrajectory later...

                bool prevGrounded = IsGrounded;
                PredictedCollision fallingPredict = PredictCollisionTrajectory(new Vector3(0, -1, 0), lowestbidder * zoomhi * risingGravityToUse * Time.deltaTime, slopeSteepnessThreshold, 1.0f, 0.0f, true);
                transform.position += fallingPredict.trajectory;
                lastfallingvalue = fallingPredict.trajectory.y;

                if (CheckIfGroundedBoolean())
                {
                    if (risinggravity > 14.0f)
                    {
                        CreateJumpParticle_Land();
                    }
                    SFX_Landing.Play();
                    risinggravity = 0.0f;
                    TimerToGroundParticle = TimeToGroundParticle / 2;
                    IsGrounded = true;
                    currentsurfacenormal = new Vector2(0, 1); //hit.normal;
                }

                if (prevGrounded == false && IsGrounded == true)
                {
                    RaycastHit2D temphit = GetGroundHitLineLEFT;
                    if (temphit.collider != null) transform.position = new Vector3(transform.position.x, temphit.point.y + groundhitOffset + GetComponent<Collider2D>().bounds.size.y / 2, transform.position.z);
                }
                //else
                //{
                //    transform.position += fallingPredict.trajectory;
                //}
            }
        }
    }
    private float _playerspeed
    {
        get { /*if (RunningMode) return RunningSpeed; else*/ return speed; }
    }
    private float _deltaSpeedCap
    {
        get { return _playerspeed * Time.deltaTime; }
    }
    void DashInput()
    {
        Vector3 trueDashDirection = (dashDirectionalVectors[0] + dashDirectionalVectors[1]).normalized;
        InternalLeftRightVelocity = 0;

        PredictedCollision PredictedMovementVector = PredictCollisionTrajectory(trueDashDirection, ZoomScale * Time.deltaTime * DashForce, -slopeSteepnessThreshold, slopeSteepnessThreshold, 0.01f);

        transform.position += PredictedMovementVector.trajectory;

        if (PredictedMovementVector.trajectory.x < -Mathf.Epsilon)
        {
            SetSpriteFlip(false);
        }
        else if (PredictedMovementVector.trajectory.x > Mathf.Epsilon)
        {
            SetSpriteFlip(true);
        }
    }
    public void ResetDash()
    {
        CanDash = true;
    }
    void MovementInput()
    {
        float totalvelocity = _deltaSpeedCap;
        if (GetLeftKey)
        {
            //totalvelocity = -_deltaSpeedCap;

            if (InternalLeftRightVelocity > Mathf.Epsilon) InternalLeftRightVelocity = 0;

            InternalLeftRightVelocity = Mathf.Clamp(InternalLeftRightVelocity - InternalLeftRightBuildupRate * Time.deltaTime, -1, 1);
        }
        else if (GetRightKey)
        {
            //totalvelocity = _deltaSpeedCap;

            if (InternalLeftRightVelocity < -Mathf.Epsilon) InternalLeftRightVelocity = 0;

            InternalLeftRightVelocity = Mathf.Clamp(InternalLeftRightVelocity + InternalLeftRightBuildupRate * Time.deltaTime, -1, 1);
        }

        //if (Mathf.Abs(totalvelocity) > Mathf.Epsilon)
        //{
        //    InternalLeftRightVelocity = Mathf.Clamp(InternalLeftRightVelocity + InternalLeftRightBuildupRate * Time.deltaTime, 0, 1);
        //}
        //else
        //{
        //    InternalLeftRightVelocity = Mathf.Clamp(InternalLeftRightVelocity - InternalLeftRightBuildupRate * Time.deltaTime, 0, 1);
        //}
        float multsign = 0;
        if (Mathf.Abs(InternalLeftRightVelocity) > Mathf.Epsilon)
        {
            multsign = (InternalLeftRightVelocity / Mathf.Abs(InternalLeftRightVelocity));
        }
        totalvelocity *= multsign * (0.2f + 0.8f * Mathf.Pow(Mathf.Abs(InternalLeftRightVelocity), InternalLeftRightPower));

        PredictedCollision PredictedMovementVector = PredictCollisionTrajectory(GetMoveRightVector(), /*ZoomScale*/ (0.2f + 0.8f * ZoomScale) * totalvelocity, -slopeSteepnessThreshold, slopeSteepnessThreshold, 0.01f);

        transform.position += PredictedMovementVector.trajectory;

        if (totalvelocity < -Mathf.Epsilon)
        {
            SetSpriteFlip(false);
        }
        else if (totalvelocity > Mathf.Epsilon)
        {
            SetSpriteFlip(true);
        }

        if (IsJumping && !GetJumpKey && timertogravity <= 0.0f)
        {
            IsJumping = false;
            timertojump = 0.0f;
        }
        if (GetJumpKeyDown && (IsGrounded || coyoteTimer < coyoteTimeMax - Time.deltaTime)) //GetKeyDown -> Key has just been pressed down | GetKey -> Key is being held down
        {
            InitializeJump();

            float jumpOverall = (float)(timertojump / timetojump) * jumpmultiplier * initiatedJumpForce;

            PredictedMovementVector = PredictCollisionTrajectory(new Vector3(0, 1, 0), /*ZoomScale **/ jumpOverall * Time.deltaTime, -1.0f, -slopeSteepnessThreshold, 0.01f);
            transform.position += PredictedMovementVector.trajectory;
        }
        else if ((GetJumpKey || timertogravity > 0.0f) && IsJumping) //If Jumping can no longer be initialized, then this checks if the Player is currently in the process of Jumping.
        {
            if (timertojump > 0.0f) //As the player continues to be jumping for longer(relatively) periods of time, their jumping strength will decrease mid-air to facilitate a smooth transition to falling. This is not exactly the case if the Player chooses to let go the Jump Button mid-jump, although I am basing this off Hollow Knight's jumping physics.
            {
                float testval = ZoomScaleTarget;
                if (ZoomScaleTarget < 1.0f)
                {
                    testval *= 2;
                }
                else
                {
                    testval *= 1.5f;
                }

                timertojump -= Time.deltaTime * testval; //ZoomScale;

                float jumpOverall = (float)(timertojump / timetojump) * jumpmultiplier * initiatedJumpForce;

                PredictedMovementVector = PredictCollisionTrajectory(new Vector3(0, 1, 0), /*ZoomScale * */jumpOverall * Time.deltaTime, -1.0f, -slopeSteepnessThreshold, 0.01f);
                transform.position += PredictedMovementVector.trajectory;
            }
            if (timertojump <= 0.0f) //When the Player reaches the maximum threshold time for jumping, the Player is therefore no longer jumping.
            {
                IsJumping = false;
            }
        }
    }
    void InitializeJump(float mag = 1)
    {
        SFX_Jump.Play();
        CreateJumpParticle_String();
        CreateJumpParticle_Jump();
        IsGrounded = false;
        IsJumping = true;
        timertogravity = mag * timetogravity;
        timertojump = mag * timetojump;
        risinggravity = 0;
        initiatedJumpForce = ZoomScale;
        coyoteTimer = coyoteTimeMax;
    }
    void CheckIfGrounded() //If the Player is already assumed to be on the ground, then this method will serve to verify if that is really the case, most often caused by walking off platforms.
    {
        if (IsGrounded)
        {
            //RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, GetComponent<Collider2D>().bounds.size.y / 2 + 0.4f, LayerMask.GetMask("Platform"));
            //if (!hit)
            if (false)
            {
                SetNotGroundedMidair();
            }
            else if (true)
            {
                //if (Vector2.Dot(hit.normal, Vector2.up) < slopeSteepnessThreshold)
                if (false)
                {
                    SetNotGroundedMidair();
                }
                else
                {
                    //float supremeoffset = (GetComponent<CircleCollider2D>().radius) / Mathf.Cos(Mathf.Deg2Rad * Vector2.Angle(Vector2.up, hit.normal));
                    //if (transform.position.y - GetComponent<CircleCollider2D>().radius - supremeoffset - 0.01f <= hit.point.y)
                    if (true)
                    {
                        if (GroundLineRaycast)
                        {

                        }
                        else
                        {
                            SetNotGroundedMidair();
                        }
                    }
                    else
                    {
                        SetNotGroundedMidair();
                    }
                }
            }
        }
    }
    void SetNotGroundedMidair()
    {
        IsGrounded = false;
        ResetGravityJumpVariables();
    }
    bool CheckIfGroundedBoolean()
    {
        if (!IsGrounded)
        {
            //RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, GetComponent<Collider2D>().bounds.size.y / 2 + 0.4f, LayerMask.GetMask("Platform"));
            //if (!hit)
            if (false)
            {

            }
            else if (true)
            {
                //if (Vector2.Dot(hit.normal, Vector2.up) < 1.0f - Mathf.Epsilon)
                //{

                //}
                //else
                {
                    //float supremeoffset = (GetComponent<CircleCollider2D>().radius) / Mathf.Cos(Mathf.Deg2Rad * Vector2.Angle(Vector2.up, hit.normal));
                    //if (transform.position.y - GetComponent<CircleCollider2D>().radius - supremeoffset - 0.01f <= hit.point.y)
                    if (true)
                    {
                        if (GroundLineRaycast)
                        {
                            return true;
                        }
                        else
                        {

                        }
                    }
                    else
                    {

                    }
                }
            }
        }
        return false;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Wind" && SmallBigZoomWeightMultiplier > Mathf.Epsilon)
        {
            float additionalWindUpMultiplier = 1;
            //if (collision.gameObject.GetComponent<Wind_Script>())
            //{
            //    additionalWindUpMultiplier *= collision.gameObject.GetComponent<Wind_Script>().WindMultiplier;
            //}

            bool hasNotBeenAdded = true;
            for (int i = 0; i < AttachedWindList.Count; ++i)
            {
                if (AttachedWindList[i].reference == collision.gameObject.transform)
                {
                    AttachedWindList[i].progress = Mathf.Clamp(AttachedWindList[i].progress + additionalWindUpMultiplier * Time.deltaTime * 1.0f / windTransitionTime, 0, 1);
                    AttachedWindList[i].recentActivated = true;
                    hasNotBeenAdded = false;
                    break;
                }
            }
            if (hasNotBeenAdded)
            {
                AttachedWindList.Add(new AttachedWind(collision.gameObject.transform, Time.deltaTime * 1.0f / windTransitionTime));
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            if (collision.gameObject.GetComponent<KeyWall_Script>() && levelMangerReference.KeyIsCollectedSomewhere())
            {
                UnlockSFX.Play();
                collision.gameObject.GetComponent<KeyWall_Script>().OpenKeyWall();
                levelMangerReference.TurnAllCollectedKeysUsed();
            }
        }
    }
    void OnCollisionStay2D(Collision2D collision) //Despite my custom methods to predict collision, I left this here in case something messes up.
    {
        //This is mostly the standard way to detect collision in Unity, look up OnCollisionStay2D and its variants for more information.
        //If the Player's collider intersects with a Platform, and the Player is not technically on the ground, the Player will be set to be on the ground if the Platform is not too steep.
        if (collision.gameObject.tag == "Platform")
        {
            //if (collision.gameObject.GetComponent<KeyWall_Script>() && levelMangerReference.KeyIsCollectedSomewhere())
            //{
            //    UnlockSFX.Play();
            //    collision.gameObject.GetComponent<KeyWall_Script>().OpenKeyWall();
            //    levelMangerReference.TurnAllCollectedKeysUsed();
            //}
            //else //platform collision detection
            {
                if (false)
                {
                    float AABB_CircleRadius = GetComponent<CircleCollider2D>().bounds.size.y / 2;
                    float AABB_BoxLength = collision.gameObject.GetComponent<BoxCollider2D>().bounds.size.x;
                    float AABB_BoxHeight = collision.gameObject.GetComponent<BoxCollider2D>().bounds.size.y;
                    float tinyoffset = 0.00f;

                    Vector2 diagonalVector = collision.gameObject.GetComponent<BoxCollider2D>().bounds.extents.normalized;
                    Vector2 playerVector = (transform.position - collision.gameObject.transform.position).normalized;

                    float DotThreshold_Top = Vector2.Dot(new Vector2(0, 1), diagonalVector);
                    float DotThreshold_Right = Vector2.Dot(new Vector2(1, 0), diagonalVector);
                    float DotThreshold_Bottom = Vector2.Dot(new Vector2(0, -1), -diagonalVector);
                    float DotThreshold_Left = Vector2.Dot(new Vector2(-1, 0), -diagonalVector);

                    bool inTopSection = Vector2.Dot(new Vector2(0, 1), playerVector) > DotThreshold_Top;
                    bool inRightSection = Vector2.Dot(new Vector2(1, 0), playerVector) > DotThreshold_Right;
                    bool inBottomSection = Vector2.Dot(new Vector2(0, -1), playerVector) > DotThreshold_Bottom;
                    bool inLeftSection = Vector2.Dot(new Vector2(-1, 0), playerVector) > DotThreshold_Left;

                    if (inTopSection)
                    {
                        float metricA = transform.position.y - collision.gameObject.transform.position.y + tinyoffset;
                        float metricB = AABB_CircleRadius + AABB_BoxHeight / 2;
                        if (metricA < metricB)
                        {
                            transform.position += (metricB - metricA) * new Vector3(0, 1, 0);
                        }
                    }
                    else if (inBottomSection)
                    {
                        float metricA = collision.gameObject.transform.position.y - transform.position.y + tinyoffset;
                        float metricB = AABB_CircleRadius + AABB_BoxHeight / 2;
                        if (metricA < metricB)
                        {
                            transform.position -= (metricB - metricA) * new Vector3(0, 1, 0);
                        }
                    }
                    else if (inRightSection)
                    {
                        float metricA = transform.position.x - collision.gameObject.transform.position.x + tinyoffset;
                        float metricB = AABB_CircleRadius + AABB_BoxLength / 2;
                        if (metricA < metricB)
                        {
                            transform.position += (metricB - metricA) * new Vector3(1, 0, 0);
                        }
                    }
                    else if (inLeftSection)
                    {
                        float metricA = collision.gameObject.transform.position.x - transform.position.x + tinyoffset;
                        float metricB = AABB_CircleRadius + AABB_BoxLength / 2;
                        if (metricA < metricB)
                        {
                            transform.position -= (metricB - metricA) * new Vector3(1, 0, 0);
                        }
                    }
                }

                //END OF OLD SCHOOL STUFF

                if (!IsGrounded)
                {
                    if (Vector2.Dot(collision.GetContact(0).normal, Vector2.up) >= slopeSteepnessThreshold &&
                        collision.GetContact(0).point.y >= transform.position.y - GetComponent<Collider2D>().bounds.size.y / 2)
                    {
                        //CheckIfGrounded();
                        if (CheckIfGroundedBoolean())
                        {
                            IsGrounded = true;
                            transform.position = new Vector3(transform.position.x, collision.GetContact(0).point.y + GetComponent<Collider2D>().bounds.size.y / 2, transform.position.z);
                            currentsurfacenormal = collision.GetContact(0).normal;
                        }
                    }
                }
                if (IsJumping) //This is a special case. If the Player jumps into a ceiling platform that faces down enough, their process of Jumping will be cancelled. I might need to set timertogravity to 0.0f here also, but it still seems to work fine
                {
                    if (Vector2.Dot(collision.GetContact(0).normal, Vector2.up) < -slopeSteepnessThreshold)
                    {
                        IsJumping = false;
                    }
                }
            }
        }
    }

    Vector3 GetMoveRightVector() //As mentioned earlier, the Player's direction on the ground is determined by the steepness of the platform they are currently on.
    {
        if (!IsGrounded)
        {
            return new Vector3(1, 0, 0);
        }
        else //You probably will want to look up an image of what a Cross Product looks like, but this is basically a way to rotate the ground's Normal towards the right direction.
        {
            if (currentsurfacenormal.sqrMagnitude <= Mathf.Epsilon) currentsurfacenormal = new Vector2(0, 1);

            return Vector3.Cross(currentsurfacenormal, new Vector3(0, 0, 1));
        }
    }
    void ResetGravityJumpVariables()
    {
        risinggravity = 0;
        timertogravity = 0.0f;
        timertojump = 0.0f;
    }
    void CreateJumpParticle_String()
    {
        if (JumpParticle_String != null)
        {
            GameObject obj = Instantiate(JumpParticle_String, transform);
            obj.transform.localPosition = new Vector3(0, -GetComponent<Collider2D>().bounds.extents.y, -1);
            obj.transform.localScale *= ZoomScale;
        }
    }
    void CreateJumpParticle_Jump()
    {
        if (JumpParticle_Jump != null)
        {
            GameObject obj = Instantiate(JumpParticle_Jump);
            obj.transform.position = transform.position + new Vector3(0, -GetComponent<Collider2D>().bounds.extents.y, -1);
            obj.transform.localScale *= ZoomScale;
        }
    }
    void CreateJumpParticle_Land()
    {
        if (JumpParticle_Land != null)
        {
            GameObject obj = Instantiate(JumpParticle_Land);
            obj.transform.position = transform.position + new Vector3(0, -GetComponent<Collider2D>().bounds.extents.y, -1);
            obj.transform.localScale *= ZoomScale;
        }
    }

    //This function uses a special kind of raycast known as 'CapsuleCast' that predicts whether the Player will collide into a Platform during their next movement.
    //If the Player is predicted to collide into a Platform, using the raycast's provided information, the distance covered by their movement is reduced so that the Player does not clip into the platform.
    PredictedCollision PredictCollisionTrajectory(Vector3 direction, float _speed, float minNormalThreshold, float maxNormalThreshold, float bufferHit = 0.0f, bool GroundedIfHit = false)
    {
        Vector3 directionalVector = _speed * direction;
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, GetComponent<CircleCollider2D>().radius,
            direction, _speed, LayerMask.GetMask("Platform"));


        //print((hit == true) + " " + direction);

        if (hit.collider != null)
        {
            float dotpro = Vector2.Dot(hit.normal, Vector2.up);
            if (minNormalThreshold <= dotpro && dotpro <= maxNormalThreshold)
            {
                directionalVector *= Mathf.Clamp(hit.fraction, 0.0f, 1.0f);
                directionalVector -= directionalVector.normalized * bufferHit;

                //if (GroundedIfHit && CheckIfGroundedBoolean())
                //{
                //    if (risinggravity > 14.0f)
                //    {
                //        CreateJumpParticle_Land();
                //    }
                //    risinggravity = 0.0f;
                //    TimerToGroundParticle = TimeToGroundParticle / 2;
                //    IsGrounded = true;
                //    currentsurfacenormal = hit.normal;
                //}
                //else
                {
                    RaycastHit2D hit2 = Physics2D.Raycast(transform.position, -Vector2.up, GetComponent<Collider2D>().bounds.size.y / 2 + 0.25f, LayerMask.GetMask("Platform"));
                    if (!hit2)
                    {
                        if (hit.normal.x > Mathf.Epsilon)
                        {
                            directionalVector = Vector3.Cross(hit.normal, new Vector3(0, 0, 1)) * directionalVector.magnitude;
                        }
                        else if (hit.normal.x < -Mathf.Epsilon)
                        {
                            directionalVector = Vector3.Cross(hit.normal, new Vector3(0, 0, -1)) * directionalVector.magnitude;
                        }
                    }
                }
            }

        }
        float hitfraction = 1;
        if (hit.collider != null) hitfraction = hit.fraction;

        if (hit.collider != null) return new PredictedCollision(directionalVector, hitfraction, hit.transform.gameObject);
        else return new PredictedCollision(directionalVector, hitfraction);

    }
    public void SupremeDeath()
    {
        SFX_DeathSupreme.Play();

        GameObject death = Instantiate(DeathSupremeVFXPrefab);
        death.transform.position = transform.position + new Vector3(0, 0, death.transform.position.z - transform.position.z);
        for (int i = 0; i < (int)(SparkAMT * 1.25f); ++i)
        {
            float ang = (float)i * 360 / (float)SparkAMT + Random.Range(-180 / (float)SparkAMT * 2, 2 * 180 / (float)SparkAMT);
            GameObject spark = Instantiate(SparkVFXPrefab);
            spark.transform.position = transform.position + new Vector3(0, 0, death.transform.position.z - transform.position.z);
            spark.GetComponent<SparkScript>().Init(Quaternion.Euler(0, 0, ang) * new Vector3(1, 0, 0));
            spark.GetComponent<SparkScript>().speed *= Random.Range(0.95f, 1.2f);
        }
        isInDeadState = true;

        transform.GetChild(0).gameObject.SetActive(false);

        Camera.main.GetComponent<CameraScript>().DoScreenshake();

        //Invoke("KillTrue", 0.9f);
    }
    public void Kill(string killedbywho)
    {
        SFX_DeathNormal.Play();

        GameObject death = Instantiate(DeathVFXPrefab);
        death.transform.position = transform.position + new Vector3(0, 0, death.transform.position.z - transform.position.z);
        for (int i = 0; i < SparkAMT; ++i)
        {
            float ang = (float)i * 360 / (float)SparkAMT + Random.Range(-180 / (float)SparkAMT, 180 / (float)SparkAMT);
            GameObject spark = Instantiate(SparkVFXPrefab);
            spark.transform.position = transform.position + new Vector3(0, 0, death.transform.position.z - transform.position.z);
            spark.GetComponent<SparkScript>().Init(Quaternion.Euler(0, 0, ang) * new Vector3(1, 0, 0));
        }
        isInDeadState = true;

        transform.GetChild(0).gameObject.SetActive(false);

        if (killedbywho == "Laser")
        {
            FindObjectOfType<EyeScript>().AddLaserDeath();
        }

        Invoke("KillTrue", 0.9f);
    }
    void KillTrue()
    {
        isInDeadState = false;
        transform.GetChild(0).gameObject.SetActive(true);
        levelMangerReference.PlayerAskToReset();
    }
    public void ReviveSupreme()
    {
        isInDeadState = false;
        transform.GetChild(0).gameObject.SetActive(true);
    }
    void InvokeRespawn()
    {
        //PostDeathSequence = false;
        //Invoke(nameof(Respawn), timetoDisintengrate * 1.5f);
        //GameManager_Script.CameraEffectsManager.EyesToShut(0.0f);
    }
    [SerializeField]
    private SpriteRenderer glowmatselfref;
    [SerializeField]
    private GameObject respawnballprefab;
    private IEnumerator RespawningAnimSupreme(float _time)
    {
        disableGravity = true;

        transform.position += new Vector3(0, 1.75f, 0);

        Vector3 highpos = transform.position;

        Instantiate(respawnballprefab).transform.position = transform.position + new Vector3(0, 0, -transform.position.z);

        GameObject topRing = Instantiate(ReviveSupremePrefab);
        GameObject bottomRing = Instantiate(ReviveSupremePrefab);
        topRing.transform.position = highpos + new Vector3(0, -0.25f, 0);
        topRing.GetComponent<SpriteRenderer>().material.SetFloat("alphaMag", 0);
        bottomRing.transform.position = highpos + new Vector3(0, -0.25f, 0);
        bottomRing.GetComponent<SpriteRenderer>().material.SetFloat("alphaMag", 0);

        bool hasplayedA = false;
        bool hasplayedB = false;

        SFX_ReviveSupreme.Play();
        glowmatselfref.material.SetFloat("alphaMag", 0);
        SpriteRenderer r = transform.GetComponentInChildren<SpriteRenderer>();
        r.material.SetFloat("_RespawnProgress", 0);

        for (float coro = 0; coro < 1.0f; coro += (1.0f / _time) * Time.unscaledDeltaTime)
        {
            glowmatselfref.material.SetFloat("alphaMag", Mathf.Pow(coro, 2));
            r.material.SetFloat("_RespawnProgress", Mathf.Clamp(coro * 1.2f, 0, 1));
            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);

            //if (!hasplayedA && coro >= 0.2f)
            //{
            //    hasplayedA = true;
            //    SFX_ReviveSupreme.Play();
            //}
            if (!hasplayedB && coro >= 0.75f)
            {
                hasplayedB = true;
                SFX_ReviveNormal.Play();
            }

            float ringval = Mathf.Clamp(1.5f * Mathf.Sin(Mathf.PI * coro * 1.2f), 0, 1);
            topRing.transform.position = highpos + new Vector3(0, -0.25f + 1.5f * ringval, 0);
            topRing.GetComponent<SpriteRenderer>().material.SetFloat("alphaMag", ringval);
            bottomRing.transform.position = highpos + new Vector3(0, -0.25f - 1.5f * ringval, 0);
            bottomRing.GetComponent<SpriteRenderer>().material.SetFloat("alphaMag", ringval);
        }

        glowmatselfref.material.SetFloat("alphaMag", 1);
        r.material.SetFloat("_RespawnProgress", 1);


        Destroy(topRing);
        Destroy(bottomRing);

        disableGravity = false;

        yield break;
    }
    private IEnumerator RespawningAnim(float _time)
    {
        SFX_ReviveNormal.Play();
        Instantiate(respawnballprefab).transform.position = transform.position + new Vector3(0, 0, -transform.position.z);

        glowmatselfref.material.SetFloat("alphaMag", 0);
        SpriteRenderer r = transform.GetComponentInChildren<SpriteRenderer>();
        r.material.SetFloat("_RespawnProgress", 0);

        for (float coro = 0; coro < 1.0f; coro += (1.0f / _time) * Time.unscaledDeltaTime)
        {
            glowmatselfref.material.SetFloat("alphaMag", Mathf.Pow(coro, 2));
            r.material.SetFloat("_RespawnProgress", Mathf.Clamp(coro * 1.2f, 0, 1));
            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        }

        glowmatselfref.material.SetFloat("alphaMag", 1);
        r.material.SetFloat("_RespawnProgress", 1);

        yield break;
    }
    void Respawn()
    {
        //GameManager_Script.ResetAllThatObstacles();
        //Vector3 nearestpoint = checkpoints[checkpointcurrent];
        //DisableControl = false;
        //transform.position = new Vector3(nearestpoint.x, nearestpoint.y, -0.2f);
        //ResetUponRevive();
    }
    public void ResetUponReviveSupreme(Vector3 respawnpoint, float _dura)
    {
        disableGravity = false;
        isInDeadState = false;
        SetSpriteFlip(true, true);
        eyeHeight_Internal = eyeHeight_Default;
        eyeHeight_Current = eyeHeight_Internal;
        morphBody_Progress_Internal = 0;
        morphBody_Progress_Current = morphBody_Progress_Internal;
        morphBody_VerticalS_Internal = 0;
        morphBody_VerticalS_Current = morphBody_VerticalS_Internal;
        risinggravity = 0.0f;
        lastfallingvalue = 0;
        IsGrounded = true;
        IsJumping = false;
        timertogravity = 0;
        timertojump = 0;
        ZoomLvlPointer = ZoomLvlPointerStarting;
        SetZoomScaleStats(ZoomLvlPointer);
        IsDashing = false;
        CanDash = true;
        timertodash = 0.0f;
        transform.position = respawnpoint;
        AttachedWindList = new List<AttachedWind>();
        coyoteTimer = 0.0f;
        InternalLeftRightVelocity = 0;
        CheckIfGrounded();
        SFX_Fall.volume = 0;
        timerForGoingDownFallSFX = 0.0f;

        StartCoroutine(RespawningAnimSupreme(_dura));
    }
    public void ResetUponRevive(Vector3 respawnpoint)
    {
        disableGravity = false;
        isInDeadState = false;
        //FindObjectOfType<EnemySnake_Script>().ResetPosition();
        SetSpriteFlip(true, true);
        eyeHeight_Internal = eyeHeight_Default;
        eyeHeight_Current = eyeHeight_Internal;
        morphBody_Progress_Internal = 0;
        morphBody_Progress_Current = morphBody_Progress_Internal;
        morphBody_VerticalS_Internal = 0;
        morphBody_VerticalS_Current = morphBody_VerticalS_Internal;
        //transform.GetComponentInChildren<SpriteRenderer>().material.SetFloat("_DisintegrationProgress", 0);
        //InteractableInFocus = null;
        //if (GameManager_Script.InteractableManager != null) GameManager_Script.InteractableManager.HandlePopup(InteractableInFocus);
        risinggravity = 0.0f;
        lastfallingvalue = 0;
        //ropedetachbuffer = 0.0f;
        //VineJumpoffTimer = 0.0f;
        IsGrounded = true;
        IsJumping = false;
        //IsRoping = false;
        //IsDead = false;
        timertogravity = 0;
        timertojump = 0;
        //GameManager_Script.CameraEffectsManager.EyesToOpen();
        ZoomLvlPointer = ZoomLvlPointerStarting;
        //ZoomScale = 1.0f;
        //ZoomScaleTarget = 1.0f;
        SetZoomScaleStats(ZoomLvlPointer);
        IsDashing = false;
        CanDash = true;
        timertodash = 0.0f;
        transform.position = respawnpoint; //InitialPosition;
        AttachedWindList = new List<AttachedWind>();
        coyoteTimer = 0.0f;
        InternalLeftRightVelocity = 0;
        CheckIfGrounded();
        SFX_Fall.volume = 0;
        timerForGoingDownFallSFX = 0.0f;


        StartCoroutine(RespawningAnim(levelMangerReference.ScreenTransitionPauseTime));
    }

    void SetZoomScaleStats(int _pointer)
    {
        ZoomScale = ZoomLvlStorage[_pointer];
        ZoomScaleTarget = ZoomLvlStorage[_pointer];
    }
}
