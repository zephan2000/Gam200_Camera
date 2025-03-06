using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashGem_Script : MonoBehaviour
{
    public float timeToRecharge = 2.0f;
    private float timerToRecharge = 0.0f;
    private bool activated = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!activated)
        {
            timerToRecharge = Mathf.Clamp(timerToRecharge - Time.deltaTime, 0, timeToRecharge);
            if (timerToRecharge < Mathf.Epsilon)
            {
                activated = true;
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = transform.GetChild(0).GetComponent<SpriteRenderer>().color + new Color(0, 0, 0, 0.8f);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!activated) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerBox_Script>().ResetDash();

            activated = false;
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = transform.GetChild(0).GetComponent<SpriteRenderer>().color - new Color(0, 0, 0, 0.8f);
            timerToRecharge = timeToRecharge;
        }
    }
}
