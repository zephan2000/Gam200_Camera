using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTriggerOnce_Script : MonoBehaviour
{
    public DialogueObject[] DialogueObjectList;
    public bool cantrigger = true;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (cantrigger && collision.CompareTag("Player"))
        {
            SendDialogue();
        }
    }
    public void SendDialogue()
    {
        cantrigger = false;
        FindObjectOfType<EyeScript>().InitiateDialogue(DialogueObjectList);
    }
}
