using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MenuArranger : MonoBehaviour {

    public float iconWidth = 70;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Application.isPlaying)
            return;
        int ind = 0;
        foreach(RectTransform t in transform)
        {
            t.sizeDelta = new Vector2(iconWidth, 100);
            t.anchoredPosition = new Vector2(iconWidth * ind, 0);
            ind++;
        }
	}
}
