using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RandomSprite : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    public Sprite[] possibleSprites;
    public bool NeedsReset = false;
	// Update is called once per frame
	void Update () {
        if (Application.isPlaying)
            return;
        if(NeedsReset && possibleSprites.Length > 0)
        {
            NeedsReset = false;
            GetComponent<SpriteRenderer>().sprite = possibleSprites[Random.Range(0, possibleSprites.Length)];
        }
	}
}
