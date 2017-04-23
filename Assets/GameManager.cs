using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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


    public AnimationCurve SleepCurve;
    public float SleepTime = .5f;
    public int SleepLoop = 3;
    public Sprite SleepNotification;

    public Sprite CoinSprite;
    public int CoinCount = 10;
    public Text[] CoinText;


    public Sprite PositiveNeed;
    public Sprite NegativeNeed;

    public List<Tree> Trees = new List<Tree>();
    public List<Fruit> Fruits = new List<Fruit>();
    public List<TreeHole> TreeHoles = new List<TreeHole>();

    public BoxCollider2D playArea;


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

    public void SubtractGold(int amount)
    {
        CoinCount -= amount;
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
        needMet.transform.position = pos + Vector3.up * .1f + Vector3.left * .07f;
        needMetClass = needMet.GetComponent<NeedMetNotification>();
        if (needMetClass != null)
            needMetClass.SetSprite(positive ? Instance.PositiveNeed : Instance.NegativeNeed);


    }

    public static void TrySpawnItem(GameObject prefab, int goldCost, Vector2 pos)
    {
        if (instance.CoinCount < goldCost)
            return;
        Vector3 newPos = Camera.main.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 0));
        newPos.z = 0;
        Tree t = prefab.GetComponent<Tree>();
        if (t != null)
        {
            float closestDist = .02f;
            TreeHole closestHole = null;
            foreach (TreeHole h in Instance.TreeHoles)
            {
                float dist = (h.transform.position - newPos).sqrMagnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestHole = h;
                }
            }

            if (closestHole != null && closestHole.currentTree == null)
            {
                newPos = closestHole.transform.position - Vector3.up * (.772f - .72f);
                closestHole.currentTree = t;

            }
            else
                return;
        }
        Instance.SubtractGold(goldCost);
        GameObject newItem = Instantiate(prefab);
        newItem.transform.position = newPos;
        SpawnNeedMet(newItem.transform.position, Instance.CoinSprite, false);


    }

    void Awake()
    {
        instance = this;

    }
    // Use this for initialization
    void Start()
    {
    }

    public LayerMask StoreMask;
    // Update is called once per frame
    void Update()
    {
    }
}
