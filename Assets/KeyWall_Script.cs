using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyWall_Script : MonoBehaviour
{
    private float keywallvalue = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<Collider2D>().enabled)
        {
            if (GetComponent<Collider2D>().isTrigger)
            {
                keywallvalue = Mathf.Clamp(keywallvalue + Time.deltaTime * 3, 0, 1);
            }
            else
            {
                keywallvalue = Mathf.Clamp(keywallvalue - Time.deltaTime * 3, 0, 1);
            }
            GetComponentInChildren<SpriteRenderer>().material.SetFloat("_IsOpen", keywallvalue);
        }
    }
    public void CloseKeyWall()
    {
        GetComponent<Collider2D>().enabled = true;
        GetComponent<Collider2D>().isTrigger = false;
        transform.GetChild(0).gameObject.SetActive(true);
        keywallvalue = 0;
        GetComponentInChildren<SpriteRenderer>().material.SetFloat("_IsOpen", 0);
    }
    public void TurnIntoTrigger()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }
    public void OpenKeyWall()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Collider2D>().isTrigger = true;
        transform.GetChild(0).gameObject.SetActive(false);
    }
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.gameObject.GetComponent<PlayerBox_Script>() && GetComponent<Collider2D>().isTrigger)
    //    {
    //        OpenKeyWall();
    //    }
    //}
}
