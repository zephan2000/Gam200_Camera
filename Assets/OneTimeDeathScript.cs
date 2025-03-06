using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneTimeDeathScript : MonoBehaviour
{
    public bool cankill = true;
    private bool EligibleDeath
    {
        get
        {
            int selflevel = transform.parent.parent.GetSiblingIndex();
            int curlevel = FindObjectOfType<LevelManager_Script>().GetCurrentLevelPointer;

            return curlevel == selflevel;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (cankill && collision.gameObject.GetComponent<PlayerBox_Script>() && EligibleDeath)
        {
            cankill = false;
            //collision.gameObject.GetComponent<PlayerBox_Script>().Kill("LMAO"); //special kill command
            
            //do there
            //collision.gameObject.GetComponent<PlayerBox_Script>().SupremeDeath();
            FindObjectOfType<LevelManager_Script>().InitializeFirstCutscene();
        }
    }
}
