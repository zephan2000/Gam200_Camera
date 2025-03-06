using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeInitTrigger_Script : MonoBehaviour
{
    public bool cantrigger = true;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (cantrigger && collision.CompareTag("Player"))
        {
            FindObjectOfType<EyeScript>().InitiateSpawn();
            cantrigger = false;
        }
    }
}
