using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                throw new System.Exception("Need GameManager Instance in Scene");
            return instance;
        }
    }

	public AnimationCurve PetIdleCurve;
    public float PetIdleTime = 1.5f;

    public AnimationCurve PetMoveCurve;
    public float PetMoveTime = 1.0f;

    void Awake()
    {
		instance = this;
    }
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
