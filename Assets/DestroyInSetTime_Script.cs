using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyInSetTime_Script : MonoBehaviour
{
    public float thetime;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, thetime);
    }
}
