using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeHole : MonoBehaviour {

    public Tree currentTree;
	// Use this for initialization
	void Start () { 
        GameManager.AddObject(this, GameManager.Instance.TreeHoles);
        foreach (Tree t in FindObjectsOfType<Tree>())
            if ((t.transform.position - transform.position).sqrMagnitude < .01f)
                currentTree = t;
    }
	
    public void RefreshTree()
    {
        currentTree = null;
        foreach (Tree t in GameManager.Instance.Trees)
            if ((t.transform.position - transform.position).sqrMagnitude < .01f)
                currentTree = t;
    }
	// Update is called once per frame
	void Update () {
		
	}
}
