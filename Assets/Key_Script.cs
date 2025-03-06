using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeyState
{
    CANCOLLECT,
    COLLECTEDHOVER,
    USED,
}
public class Key_Script : MonoBehaviour
{
    [SerializeField]
    private AudioSource CollectSFX;
    public KeyState state = KeyState.CANCOLLECT;
    private Transform playerReference = null;
    public Vector3 OffsetFromPlayer = new Vector3(0, 0.4f, 0);
    // Start is called before the first frame update
    void Start()
    {
        state = KeyState.CANCOLLECT;
    }
    public void Reset()
    {
        playerReference = null;
        transform.GetChild(0).localPosition = new Vector3();
        transform.GetChild(0).gameObject.SetActive(true);
        state = KeyState.CANCOLLECT;
    }
    public void Used()
    {
        playerReference = null;
        transform.GetChild(0).localPosition = new Vector3();
        transform.GetChild(0).gameObject.SetActive(false);
        state = KeyState.USED;
    }
    // Update is called once per frame
    void Update()
    {
        switch(state)
        {
            case KeyState.CANCOLLECT:
                transform.GetChild(0).localPosition = new Vector3();
                transform.GetChild(0).gameObject.SetActive(true);
                break;
            case KeyState.COLLECTEDHOVER:
                if (playerReference != null)
                {
                    transform.GetChild(0).position = playerReference.position + OffsetFromPlayer;
                }
                transform.GetChild(0).gameObject.SetActive(true);
                break;
            case KeyState.USED:
                transform.GetChild(0).localPosition = new Vector3();
                transform.GetChild(0).gameObject.SetActive(false);
                break;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (state != KeyState.CANCOLLECT) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            CollectSFX.Play();
            playerReference = collision.transform;
            state = KeyState.COLLECTEDHOVER;

            KeyWall_Script keywall = transform.parent.GetComponentInChildren<KeyWall_Script>();
            if(keywall != null)
            {
                keywall.TurnIntoTrigger();
            }
            else
            {
                print("ERROR");
            }
        }
    }
}
