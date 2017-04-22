﻿using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Pet : BaseEntity
{
    public Sprite[] sprites;
    public GameObject nextEvolutionPrefab;
    public Sprite FruitSprite;

    public float HungerSatisfied = 1f;
    public float HungerDecay = .1f;
    public float MinHungerBeforeEat = .7f;

    public float SecondsPerCoin = 5f;

    public enum PetType
    {
        BLUE,
        RED,
        GREEN,
        PURPLE,
        ORANGE
    }


    public PetType MyPetType = PetType.BLUE;

    public enum State
    {
        IDLE,
        MOVING,
        ATTACKING,
        EATING,
        SLEEPING,
        EVOLVING,
        DYING
    }

    private SpriteRenderer sRenderer;
    private void Start()
    {
        animCount = Random.Range(0, GameManager.Instance.PetIdleTime);
        sRenderer = GetComponent<SpriteRenderer>();
    }

    public State CurrentState = State.IDLE;

    private float lifeCount = 0;
    private float count = 0;

    private float animCount = 0;
    protected override void HandleUpdate()
    {
        animCount += Time.deltaTime;
        switch (CurrentState)
        {
            case State.IDLE:
                float scaleValue = GameManager.Instance.PetIdleCurve.Evaluate(animCount / GameManager.Instance.PetIdleTime);
                transform.localScale = new Vector3(1 + scaleValue, 1 - scaleValue, 1);
                while (animCount > GameManager.Instance.PetIdleTime * 2f)
                    animCount -= GameManager.Instance.PetIdleTime * 2f;
                break;
            case State.MOVING:
                scaleValue = GameManager.Instance.PetMoveCurve.Evaluate(animCount / GameManager.Instance.PetMoveTime);
                transform.localScale = new Vector3(1 + scaleValue, 1 - scaleValue, 1);
                while (animCount > GameManager.Instance.PetMoveTime * 2f)
                    animCount -= GameManager.Instance.PetMoveTime * 2f;
                break;
            case State.EATING:
                scaleValue = GameManager.Instance.PetEatCurve.Evaluate(animCount / GameManager.Instance.PetEatTime);
                transform.localScale = new Vector3(1 + scaleValue, 1 - scaleValue, 1);
                eatingFruit.transform.localScale = transform.localScale;
                if (animCount >= GameManager.Instance.PetEatTime * GameManager.Instance.PetEatLoop)
                {
                    animCount = 0;
                    CurrentState = State.IDLE;
                    HungerSatisfied += eatingFruit.HungerValue;
                    HungerSatisfied = Mathf.Min(1, HungerSatisfied);
                    GameManager.SpawnNeedMet(transform.position, eatingFruit.sRenderer.sprite);
                    Destroy(eatingFruit.gameObject);
                }
                break;
            case State.EVOLVING:
                scaleValue = GameManager.Instance.EvolveCurve.Evaluate(animCount / GameManager.Instance.EvolveTime);
                transform.localScale = new Vector3(1 + scaleValue, 1 - scaleValue, 1);
                if (animCount >= GameManager.Instance.EvolveTime * GameManager.Instance.EvolveLoop)
                {                    
                    GameObject newPet = Instantiate(nextEvolutionPrefab);
                    newPet.transform.position = transform.position;
                    GameManager.SpawnNeedMet(transform.position, GameManager.Instance.EvolveNotification);
                    Destroy(gameObject);
                }
                break;
            case State.DYING:
                scaleValue = GameManager.Instance.DieCurve.Evaluate(animCount / GameManager.Instance.DieTime);
                transform.localScale = new Vector3(1 - scaleValue, 1 + scaleValue, 1);
                if (animCount >= GameManager.Instance.DieTime * GameManager.Instance.DieLoop)
                {
                    GameManager.SpawnNeedMet(transform.position, GameManager.Instance.DieNotification, false );
                    Destroy(gameObject);
                }
                break;
        }
        base.HandleUpdate();
    }
    public float DecisionTime = .3f;
    public float MoveChance = .7f;
    public float StopMoveChance = .5f;
    public float speed = 1.0f;
    public float coinCount = 0;
    protected override void HandleFixedUpdate()
    {
        if ((int)(HungerSatisfied * 10f) != (int)((HungerSatisfied - HungerDecay * Time.fixedUnscaledDeltaTime) * 10f))
            GameManager.SpawnNeedMet(transform.position, FruitSprite, false);
        HungerSatisfied -= HungerDecay * Time.fixedUnscaledDeltaTime;
       
        count += Time.fixedUnscaledDeltaTime;
        lifeCount += Time.fixedUnscaledDeltaTime;
        coinCount += Time.fixedUnscaledDeltaTime;
        while(coinCount > SecondsPerCoin)
        {
            coinCount -= SecondsPerCoin;
            GameManager.Instance.AddCoin();
            GameManager.SpawnNeedMet(transform.position, GameManager.Instance.CoinSprite);
        }
        if (count >= DecisionTime)
        {
            switch (CurrentState)
            {
                case State.IDLE:
                    if (!CheckDeath() && !CheckEvolve() && !CheckHunger())
                        if (Random.Range(0, 1f) <= MoveChance)
                        {
                            CurrentState = State.MOVING;
                            float angle = Random.Range(0, Mathf.PI * 2f);
                            MoveDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                        }
                    break;
                case State.MOVING:
                    if (Random.Range(0, 1f) <= StopMoveChance)
                    {
                        MoveDir = Vector2.zero;
                        CurrentState = State.IDLE;
                    }
                    break;
            }
            count = 0;
        }

        base.HandleFixedUpdate();
    }

    public float EvolveTime = 10f;
    private bool CheckEvolve()
    {
        if(nextEvolutionPrefab != null & lifeCount > EvolveTime)
        {
            CurrentState = State.EVOLVING;
            animCount = 0;
            return true;    
        }
        return false;
    }

    private bool CheckDeath()
    {
        if (HungerSatisfied <= 0)
        {
            Die();
            return true;
        }
        return false;
    }
    private void Die()
    {
        if (CurrentState == State.DYING)
            return;
        CurrentState = State.DYING;
        animCount = 0;
    }

    private bool CheckHunger()
    {
        if (HungerSatisfied < MinHungerBeforeEat)
        {
            Fruit closestFruit = null;
            float closestDist = float.MaxValue;
            Vector3 targetPos = Vector3.zero;
            foreach (Fruit f in GameManager.Instance.Fruits)
            {
                if (f.PetType == this.MyPetType && (f.IsAvailable || (f.InTree && f.IsGrown)))
                {
                    //Go for the fruit on the ground or the grown fruit on a tree (whichever is closer)
                    Vector3 pos = new Vector3(f.bCollider.offset.x, f.bCollider.offset.y) + f.transform.position;
                    if(f.InTree)
                        pos = new Vector3(f.treeParent.bCollider.offset.x, f.treeParent.bCollider.offset.y) + f.treeParent.transform.position;
                    float distSqr = (transform.position - f.transform.position).sqrMagnitude;
                    if (distSqr < closestDist)
                    {
                        closestFruit = f;
                        closestDist = distSqr;
                        targetPos = pos;
                    }

                }
            }
            if (closestFruit != null)
            {
                MoveDir = targetPos - transform.position;
                MoveDir = MoveDir.normalized * speed;
                if (GameManager.Instance.DrawDebug)
                    Debug.DrawLine(transform.position, targetPos, Color.red, GameManager.Instance.DebugDrawTime);
                CurrentState = State.MOVING;
                return true;
            }
        }

        return false;
    }

    public Fruit eatingFruit = null;
    public void TryEat(Fruit f)
    {
        if (f.PetType == MyPetType && HungerSatisfied <= MinHungerBeforeEat && CurrentState != State.EVOLVING)
        {
            eatingFruit = f;
            f.IsAvailable = false;
            CurrentState = State.EATING;
            MoveDir = Vector3.zero;
            animCount = 0;
        }
    }
}

