using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathUponCollide_Script : MonoBehaviour
{
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
        if (collision.gameObject.GetComponent<PlayerBox_Script>())
        {
            collision.gameObject.GetComponent<PlayerBox_Script>().Kill("Laser");
            if (transform.GetComponentInParent<LaserGlowMaster_Script>())
            {
                transform.GetComponentInParent<LaserGlowMaster_Script>().ExecuteNEO();
            }
        }
    }
}
