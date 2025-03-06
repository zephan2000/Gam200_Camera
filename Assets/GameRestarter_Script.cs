using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRestarter_Script : MonoBehaviour
{
    public int FinishCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (FindObjectOfType<LevelManager_Script>())
            {
                FindObjectOfType<LevelManager_Script>().RestartHardPlain();

                FinishCount++;

                //FindObjectOfType<EyeScript>().CongratsDialogue(FinishCount);
            }
        }
    }
}
