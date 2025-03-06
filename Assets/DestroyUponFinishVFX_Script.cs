using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyUponFinishVFX_Script : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, GetComponentInChildren<Animation>().clip.length);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
