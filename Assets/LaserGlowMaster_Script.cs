using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserGlowMaster_Script : MonoBehaviour
{
    private float durationToNEO = 0.15f;
    private float timeToNEO = 0.0f;
    private float durationToSmall = 0.5f;
    private float timeToSmall = 0.0f;
    private float NEOValue = 0; //0 ~ 1
    private SpriteRenderer[] LaserRefs = null;
    public void ExecuteNEO()
    {
        NEOValue = 0;
        timeToNEO = durationToNEO;
    }
    // Start is called before the first frame update
    void Start()
    {
        LaserRefs = transform.GetComponentsInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (timeToNEO > Mathf.Epsilon)
        {
            timeToNEO = Mathf.Clamp(timeToNEO - Time.deltaTime, 0, durationToNEO);
            NEOValue = 1 - timeToNEO / durationToNEO;

            if (timeToNEO <= Mathf.Epsilon)
            {
                NEOValue = 1;
                timeToSmall = durationToSmall;
            }
        }
        else
        {
            timeToSmall = Mathf.Clamp(timeToSmall - Time.deltaTime, 0, durationToSmall);
            NEOValue = timeToSmall / durationToSmall;
        }

        if (LaserRefs != null)
        {
            foreach (SpriteRenderer rend in LaserRefs)
            {
                rend.material.SetFloat("_NEOValue", NEOValue);
            }
        }
    }
}
