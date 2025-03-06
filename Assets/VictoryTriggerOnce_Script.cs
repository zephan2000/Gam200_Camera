using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryTriggerOnce_Script : MonoBehaviour
{
    public bool cantrigger = true;
    public bool AcknowledgeDeathless = true;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (cantrigger && collision.CompareTag("Player"))
        {
            NotifyVictory();
        }
    }
    public void NotifyVictory()
    {
        cantrigger = false;
        FindObjectOfType<EyeScript>().PlayerVictory(AcknowledgeDeathless);
    }
}
