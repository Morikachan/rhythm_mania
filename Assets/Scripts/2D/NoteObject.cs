using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public bool canBePressed;
    public KeyCode keyToPress;

    public GameObject hitEffect, goodEffect, perfectEffect, missEffect;
    public Transform targetObjectTransform;

    void Start()
    {
        
    }

    
    void Update()
    {
        if (Input.GetKeyDown(keyToPress)) {
            if (canBePressed) { 
                gameObject.SetActive(false);

                if(Mathf.Abs(transform.position.y) > 0.25)
                {
                    GameManager2D.instance.NormalHit();
                    Instantiate(hitEffect, targetObjectTransform.position, targetObjectTransform.rotation);
                } else if (Mathf.Abs(transform.position.y) > 0.05)
                {
                    GameManager2D.instance.GoodHit();
                    Instantiate(goodEffect, targetObjectTransform.position, targetObjectTransform.rotation);
                } else
                {
                    GameManager2D.instance.PerfectHit();
                    Instantiate(perfectEffect, targetObjectTransform.position, targetObjectTransform.rotation);
                }
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Activator")
        {
            canBePressed = true;
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Activator")
        {
            canBePressed = false;

            GameManager2D.instance.NoteMissed();
            Instantiate(missEffect, targetObjectTransform.position, targetObjectTransform.rotation);
        }
    }
}
