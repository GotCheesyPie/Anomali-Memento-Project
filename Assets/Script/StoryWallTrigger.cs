using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StoryWallTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Dialogue.GetInstance().dialogueIsPlaying)
        {
            gameObject.GetComponent<Collider2D>().enabled = true;
        }
        else
        {
            gameObject.GetComponent<Collider2D>().enabled = false;
        }
    }
}
