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
    public SoundGroup BirthSound;

    public SoundGroup HitTree;
    public SoundGroup Positive;
    public SoundGroup Negative;
    public SoundGroup Place;

    public AnimationCurve DieCurve;
    public float DieTime = .3f;
    public int DieLoop = 5;
    public Sprite DieNotification;
    public SoundGroup DeathSound;


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
    public BoxCollider2D napArea;
    public BoxCollider2D spawnArea;


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

    public void AddCoin(int coinAmount = 1)
    {
        CoinCount += coinAmount;
        RefreshCoinText();
        PlayerPrefs.SetInt("Coins", CoinCount);
    }

    public void SubtractGold(int amount)
    {
        CoinCount -= amount;
        RefreshCoinText();
        PlayerPrefs.SetInt("Coins", CoinCount);
    }

    public void RefreshCoinText()
    {
        foreach (Text t in CoinText)
        {
            t.text = CoinCount.ToString();
            t.transform.localScale = Vector3.one * 4;
        }
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

        SoundManager.Play(positive ? Instance.Positive : Instance.Negative);


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
        else
        {
            if (!Instance.spawnArea.bounds.Contains(newPos))
                return;
        }

        SoundManager.Play(instance.Place);
        Instance.SubtractGold(goldCost);
        GameObject newItem = Instantiate(prefab);
        Tree tComp = newItem.GetComponent<Tree>();
        if (tComp != null)
            tComp.resellValue = goldCost;
        newItem.transform.position = newPos;
        SpawnNeedMet(newItem.transform.position, Instance.CoinSprite, false);


    }

    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        instance = this;

    }
    // Use this for initialization
    void Start()
    {
        Screen.SetResolution(320, 480, false);
        CoinCount = PlayerPrefs.GetInt("Coins", 0);

        RefreshCoinText();

    }

    public LayerMask TreeMask;
    public Tree currentTappingTree;
    private int fingerId = -1;

    // Update is called once per frame
    void Update()
    {
        //Check for being dead broke
        if (Instance.CoinCount < 3 && FindObjectsOfType<Pet>().Length == 0)
            AddCoin(3);

        foreach (Text t in this.CoinText)
        {
            if (t.transform.localScale.x > 1)
            {
                t.transform.localScale = t.transform.localScale * .95f;
            }
            else
            {
                t.transform.localScale = Vector3.one;
            }
        }
        Vector2 touchPos = Input.mousePosition;
        foreach (Touch t in Input.touches)
        {
            switch (t.phase)
            {
                case TouchPhase.Began:
                    if (fingerId == -1)
                    {
                        foreach (Tree tr in Trees)
                        {
                            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(t.position);
                            worldPoint.z = 0;
                            if ((tr.transform.position + Vector3.up * .2f - worldPoint).sqrMagnitude < .01f)
                            {
                                fingerId = t.fingerId;
                                currentTappingTree = tr;
                                break;
                            }
                        }
                    }
                    break;
                case TouchPhase.Ended:
                    if (fingerId == t.fingerId)
                    {
                        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(t.position);
                        worldPoint.z = 0;
                        if ((currentTappingTree.transform.position + Vector3.up * .2f - worldPoint).sqrMagnitude < .01f)
                        {

                            Destroy(currentTappingTree.gameObject);
                            currentTappingTree = null;
                            fingerId = -1;
                        }
                    }
                    break;
                case TouchPhase.Canceled:
                    fingerId = -1;
                    currentTappingTree = null;
                    break;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (fingerId == -1)
            {
                foreach (Tree t in Trees)
                {
                    Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    worldPoint.z = 0;
                    if ((t.transform.position + Vector3.up * .2f - worldPoint).sqrMagnitude < .01f)
                    {
                        fingerId = 0;
                        currentTappingTree = t;
                        break;
                    }
                }

            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (fingerId == 0)
            {
                Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldPoint.z = 0;
                if ((currentTappingTree.transform.position + Vector3.up * .2f - worldPoint).sqrMagnitude < .01f)
                {

                    Destroy(currentTappingTree.gameObject);
                    currentTappingTree = null;
                    fingerId = -1;
                }
            }
        }

    }


}
