using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Transform Player_Reference;

    private Vector3 Position_Previous;

    [SerializeField]
    private Transform BgParallaxGroup;

    private LevelManager_Script levelManagerReference;

    private float ScreenshakeTimeLeft = 0.0f;

    public float CameraFollow_Speed = 10.0f;
    public float CameraFollow_MinDistanceThreshold = 1.5f;
    public float CameraFollow_MaxDistanceThreshold = 5.0f;
    public void HardReset()
    {
        transform.position = RestrictVectorWithinBoundary(new Vector3(Player_Reference.position.x, Player_Reference.position.y, transform.position.z));
    }
    private void Awake()
    {
        levelManagerReference = GameObject.FindObjectOfType<LevelManager_Script>();
    }
    public void DoScreenshake()
    {
        ScreenshakeTimeLeft = 1.0f;
    }
    // Start is called before the first frame update
    void Start()
    {
        Player_Reference = GameObject.FindGameObjectWithTag("Player").transform;
        HardReset();
        SetParallax();
    }
    void SetParallax()
    {

        foreach (Transform trans in BgParallaxGroup)
        {
            trans.GetComponent<SpriteRenderer>().material.SetFloat("_XCoord", transform.position.x);
            trans.GetComponent<SpriteRenderer>().material.SetFloat("_YCoord", transform.position.y);
        }
    }
    // Update is called once per frame
    void Update()
    {

        Vector3 TargetPosition = new Vector3(Player_Reference.position.x, Player_Reference.position.y, transform.position.z);

        TargetPosition = RestrictVectorWithinBoundary(TargetPosition);

        Vector3 Position_Difference = TargetPosition - transform.position;
        float Position_Difference_Length = Mathf.Sqrt(
            Mathf.Pow(Position_Difference.x, 2)
            +
            Mathf.Pow(Position_Difference.y, 2)
            );
        transform.position += levelManagerReference.PauseMenuSpeedMultiplier * Time.unscaledDeltaTime * CameraFollow_Speed * Position_Difference * Mathf.SmoothStep(CameraFollow_MinDistanceThreshold, CameraFollow_MaxDistanceThreshold, Mathf.SmoothStep(0.0f, 1.0f, Position_Difference_Length / (CameraFollow_MaxDistanceThreshold - CameraFollow_MinDistanceThreshold)));

        SetParallax();

        transform.position += ScreenshakeTimeLeft * new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
        ScreenshakeTimeLeft = Mathf.Clamp(ScreenshakeTimeLeft - 5 * Time.deltaTime, 0, 1);

        Position_Previous = transform.position;
    }

    Bounds FindBoundary()
    {

        //if (!GameObject.Find("Bounds")) return new Bounds();

        return levelManagerReference.GetCurrentLevelBoundary; //GameObject.Find("Bounds").GetComponent<BoxCollider2D>().bounds;
    }
    Vector3 RestrictVectorWithinBoundary(Vector3 _vector)
    {
        Bounds Boundary = FindBoundary();

        Vector3 restrictedTarget = _vector;

        float halfWidth = GetComponent<Camera>().orthographicSize * ((float)Screen.width / (float)Screen.height);
        float halfHeight = GetComponent<Camera>().orthographicSize;

        bool leftCondition = (restrictedTarget.x - halfWidth) < Boundary.min.x;
        bool rightCondition = (restrictedTarget.x + halfWidth) > Boundary.max.x;
        bool upCondition = (restrictedTarget.y - halfHeight) < Boundary.min.y;
        bool downCondition = (restrictedTarget.y + halfHeight) > Boundary.max.y;

        if (Boundary.extents.x < halfWidth)
        {
            restrictedTarget = new Vector3(Boundary.center.x, restrictedTarget.y, restrictedTarget.z);
        }
        else if (leftCondition)
        {
            restrictedTarget = new Vector3(Boundary.min.x + halfWidth, restrictedTarget.y, restrictedTarget.z);
        }
        else if (rightCondition)
        {
            restrictedTarget = new Vector3(Boundary.max.x - halfWidth, restrictedTarget.y, restrictedTarget.z);
        }

        if (Boundary.extents.y < halfHeight)
        {
            restrictedTarget = new Vector3(restrictedTarget.x, Boundary.center.y, restrictedTarget.z);
        }
        else if (upCondition)
        {
            restrictedTarget = new Vector3(restrictedTarget.x, Boundary.min.y + halfHeight, restrictedTarget.z);
        }
        else if (downCondition)
        {
            restrictedTarget = new Vector3(restrictedTarget.x, Boundary.max.y - halfHeight, restrictedTarget.z);
        }

        return restrictedTarget;
    }
    //void RestrictWithinBoundary()
    //{
    //    Bounds Boundary = FindBoundary();

    //    float halfWidth = GetComponent<Camera>().orthographicSize * ((float)Screen.width / (float)Screen.height);
    //    float halfHeight = GetComponent<Camera>().orthographicSize;
    //    //Left
    //    if (transform.position.x - halfWidth < Boundary.min.x)
    //    {
    //        transform.position = new Vector3(Boundary.min.x + halfWidth, transform.position.y, transform.position.z);
    //    } //Right
    //    else if (transform.position.x + halfWidth > Boundary.max.x)
    //    {
    //        transform.position = new Vector3(Boundary.max.x - halfWidth, transform.position.y, transform.position.z);
    //    }

    //    //Up
    //    if (transform.position.y - halfHeight < Boundary.min.y)
    //    {
    //        transform.position = new Vector3(transform.position.x, Boundary.min.y + halfHeight, transform.position.z);
    //    } //Down
    //    else if (transform.position.y + halfHeight > Boundary.max.y)
    //    {
    //        transform.position = new Vector3(transform.position.x, Boundary.max.y - halfHeight, transform.position.z);
    //    }
    //}
}
