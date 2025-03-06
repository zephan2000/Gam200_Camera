using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class respawnballscript : MonoBehaviour
{
    private float timer = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.unscaledDeltaTime;
        GetComponent<SpriteRenderer>().material.SetFloat("_Progress", timer / 0.15f);
        if (timer >= 0.15f) Destroy(gameObject);
    }
}
