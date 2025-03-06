using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkScript : MonoBehaviour
{
    [SerializeField]
    private GameObject glowballVFX;
    public float lifespanTotal = 3;
    private float lifespan;
    public float speed = 3;
    private Vector3 dir;
    public float gravitymag = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void Init(Vector3 _dir)
    {
        dir = _dir;
        transform.right = dir;
        lifespan = lifespanTotal;
    }
    // Update is called once per frame
    void Update()
    {
        lifespan = Mathf.Clamp(lifespan - Time.unscaledDeltaTime, 0, lifespanTotal);
        if (lifespan <= Mathf.Epsilon) Destroy(gameObject);
        else
        {
            transform.position += dir * speed * Time.unscaledDeltaTime * Mathf.Pow(lifespan / lifespanTotal, 1.1f);
            dir = Vector3.RotateTowards(dir, new Vector3(0, -1, 0), Time.unscaledDeltaTime * Mathf.PI * gravitymag, 0.0f);
            transform.right = dir;
            transform.localScale = transform.localScale + new Vector3(lifespan / lifespanTotal - transform.localScale.x, 0.35f * lifespan / lifespanTotal - transform.localScale.y, 0);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            ContactPoint2D[] point = new ContactPoint2D[2];
            collision.GetContacts(point);
            dir = dir - 2 * Vector3.Dot(dir, point[0].normal) * (Vector3)point[0].normal;
            lifespan *= 0.75f;
            GameObject glowball = Instantiate(glowballVFX);
            glowball.transform.position = transform.position + new Vector3(0, 0, glowballVFX.transform.position.z - transform.position.z);
            glowball.transform.localScale *= lifespan / lifespanTotal;
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            lifespan *= 0.2f;
        }
    }
}
