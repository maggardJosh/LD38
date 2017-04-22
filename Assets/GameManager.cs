using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

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

    public bool DrawDebug = false;

    public AnimationCurve PetIdleCurve;
    public float PetIdleTime = 1.5f;

    public AnimationCurve PetMoveCurve;
    public float PetMoveTime = 1.0f;

    public AnimationCurve PetEatCurve;
    public float PetEatTime = 1.5f;
    public int PetEatLoop = 5;

    public List<Tree> Trees = new List<Tree>();
    public List<Fruit> Fruits = new List<Fruit>();

    public static void AddObject<T>(T obj, List<T> list)
    {
        if (!list.Contains(obj))
            list.Add(obj);
    }

    public static void RemoveObject<T>(T obj, List<T> list)
    {
        if (list.Contains(obj))
            list.Remove(obj);
    }

    void Awake()
    {
        instance = this;
    }
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
