using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public float DebugDrawTime = .3f;

    public AnimationCurve PetIdleCurve;
    public float PetIdleTime = 1.5f;

    public AnimationCurve PetMoveCurve;
    public float PetMoveTime = 1.0f;

    public AnimationCurve PetEatCurve;
    public float PetEatTime = 1.5f;
    public int PetEatLoop = 5;

    public AnimationCurve NeedMetCurve;
    public float NeedMetTime = 1.0f;
    public float NeedMetYDisp = .2f;

    public AnimationCurve EvolveCurve;
    public float EvolveTime = .3f;
    public int EvolveLoop = 5;
    public Sprite EvolveNotification;

    public AnimationCurve DieCurve;
    public float DieTime = .3f;
    public int DieLoop = 5;
    public Sprite DieNotification;

    public Sprite CoinSprite;
    public int CoinCount = 10;
    public Text[] CoinText;


    public Sprite PositiveNeed;
    public Sprite NegativeNeed;

    public List<Tree> Trees = new List<Tree>();
    public List<Fruit> Fruits = new List<Fruit>();


    public GameObject NeedMetNotificationPrefab;

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

    public void AddCoin()
    {
        CoinCount++;
        foreach (Text t in CoinText)
            t.text = CoinCount.ToString();
    }
    
    public static void SpawnNeedMet(Vector3 pos, Sprite s, bool positive = true)
    {
        GameObject needMet = Instantiate(Instance.NeedMetNotificationPrefab);
        needMet.transform.position = pos + Vector3.up * .1f;
        NeedMetNotification needMetClass = needMet.GetComponent<NeedMetNotification>();
        if (needMetClass != null)
            needMetClass.SetSprite(s);

        needMet = Instantiate(Instance.NeedMetNotificationPrefab);
        needMet.transform.position = pos + Vector3.up * .1f  + Vector3.left * .07f;
        needMetClass = needMet.GetComponent<NeedMetNotification>();
        if (needMetClass != null)
            needMetClass.SetSprite(positive ? Instance.PositiveNeed : Instance.NegativeNeed);


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
