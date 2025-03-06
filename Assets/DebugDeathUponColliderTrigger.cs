using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDeathUponColliderTrigger : MonoBehaviour
{
    private bool EligibleDeath
    {
        get
        {
            if (!FindObjectOfType<LevelManager_Script>().HasAbilityToRestart) return false;

            int selflevel = transform.parent.parent.GetSiblingIndex();
            int curlevel = FindObjectOfType<LevelManager_Script>().GetCurrentLevelPointer;

            return curlevel > selflevel;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerBox_Script>() && EligibleDeath)
        {
            collision.gameObject.GetComponent<PlayerBox_Script>().Kill("InvisibleLaser");
        }
    }
}
