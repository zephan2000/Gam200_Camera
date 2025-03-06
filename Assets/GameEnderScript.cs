using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnderScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (FindObjectOfType<LevelManager_Script>())
            {
                FindObjectOfType<LevelManager_Script>().Force_ExitToMenu();
            }
        }
    }
}
