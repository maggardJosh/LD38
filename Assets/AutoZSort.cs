using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class AutoZSort : MonoBehaviour {

    SpriteRenderer r;
    public float sortOffset = 0;
	// Use this for initialization
	void Start () {
        r = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        r.sortingOrder = -(int)((transform.position.y+sortOffset) * 100);
	}
}
