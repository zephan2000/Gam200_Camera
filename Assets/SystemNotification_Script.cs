using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SystemNotification_Script : MonoBehaviour
{
    public string SoftResetText = ">>EXECUTE RESTART";
    public string HardResetText = ">>EXECUTE SYSTEM WIPE";
    private float timerToGo = 0.0f;
    public float timeToGo = 2.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timerToGo > Mathf.Epsilon)
        {
            timerToGo -= Time.deltaTime;
            if (timerToGo <= Mathf.Epsilon)
            {
                timerToGo = 0;
                GetComponent<TMP_Text>().text = "";
            }
        }
    }
    public void OnHardReset()
    {
        timerToGo = timeToGo;
        GetComponent<TMP_Text>().text = HardResetText;
    }
    public void OnSoftReset()
    {
        timerToGo = timeToGo;
        GetComponent<TMP_Text>().text = SoftResetText;
    }
}
