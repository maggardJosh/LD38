using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Pet : BaseEntity
{
    public Sprite normalSprite;
    public Sprite sleepSprite;

    public GameObject nextEvolutionPrefab;
    public Sprite FruitSprite;

    public float HungerSatisfied = 1f;
    public float SleepSatisfied = 1f;

    public float EatChance = .2f;
    public float SleepChance = .2f;

    public float MinHungerBeforeEat = .7f;
    public float MinTiredBeforeSleep = .7f;
    public float WakeUpChance = .3f;

    public float EmergencyEatValue = .3f;
    public float EmergencySleepValue = .3f;

    public float SleepDecay = .1f;
    public float HungerDecay = .1f;

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
        sRenderer.sprite = normalSprite;
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
                    GameManager.SpawnNeedMet(transform.position, GameManager.Instance.DieNotification, false);
                    Destroy(gameObject);
                }
                break;
            case State.SLEEPING:
                sRenderer.sprite = sleepSprite;
                scaleValue = GameManager.Instance.SleepCurve.Evaluate(animCount / GameManager.Instance.SleepTime);
                transform.localScale = new Vector3(1 + scaleValue, 1 - scaleValue, 1);
                if (animCount >= GameManager.Instance.SleepTime * GameManager.Instance.SleepLoop)
                {
                    animCount -= GameManager.Instance.SleepTime * GameManager.Instance.SleepLoop;
                    SleepSatisfied += .1f;
                    if (SleepSatisfied > 1 || SleepSatisfied > MinTiredBeforeSleep && Random.Range(0f, 1f) < WakeUpChance)
                    {
                        SleepSatisfied = Mathf.Min(1, SleepSatisfied);
                        CurrentState = State.IDLE;
                    }
                    GameManager.SpawnNeedMet(transform.position, GameManager.Instance.SleepNotification, true);
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
        if ((int)(HungerSatisfied * 10f) != (int)((HungerSatisfied - HungerDecay * Time.fixedDeltaTime) * 10f))
            GameManager.SpawnNeedMet(transform.position, FruitSprite, false);
        HungerSatisfied -= HungerDecay * Time.fixedDeltaTime;

        if (CurrentState != State.SLEEPING)
        {
            if ((int)(SleepSatisfied * 10f) != (int)((SleepSatisfied - SleepDecay * Time.fixedDeltaTime) * 10f))
                GameManager.SpawnNeedMet(transform.position, GameManager.Instance.SleepNotification, false);
            SleepSatisfied -= SleepDecay * Time.fixedDeltaTime;
        }

        count += Time.fixedDeltaTime;
        lifeCount += Time.fixedDeltaTime;
        coinCount += Time.fixedDeltaTime;
        while (coinCount > SecondsPerCoin)
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
                    if (!CheckDeath() && !CheckEvolve() && !CheckHunger() && !CheckSleep())
                        if (Random.Range(0, 1f) <= MoveChance)
                        {
                            CurrentState = State.MOVING;
                            Bounds playArea = GameManager.Instance.playArea.bounds;
                            Vector2 randomTarget = new Vector2(Random.Range(playArea.center.x - playArea.extents.x, playArea.center.x + playArea.extents.y), Random.Range(playArea.center.y - playArea.extents.y, playArea.center.y + playArea.extents.y));

                            MoveDir = (randomTarget - new Vector2(transform.position.x, transform.position.y)).normalized * speed;
                            if (GameManager.Instance.DrawDebug)
                                Debug.DrawLine(transform.position, randomTarget, Color.blue, GameManager.Instance.DebugDrawTime);
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
        if (nextEvolutionPrefab != null & lifeCount > EvolveTime)
        {
            CurrentState = State.EVOLVING;
            animCount = 0;
            return true;
        }
        return false;
    }

    private bool CheckDeath()
    {
        if (HungerSatisfied <= 0 || SleepSatisfied <= 0)
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

    private bool CheckSleep()
    {
        if ((SleepSatisfied < MinTiredBeforeSleep && Random.Range(0, 1f) < SleepChance) || SleepSatisfied < EmergencySleepValue)
        {
            if (!GameManager.Instance.playArea.bounds.Contains(transform.position))
            {
                CurrentState = State.MOVING;
                MoveDir = Vector2.down * speed;
                return true;
            }
            else
            {
                CurrentState = State.SLEEPING;
                MoveDir = Vector2.zero;
                return true;
            }
        }
        return false;
    }

    private bool CheckHunger()
    {
        if ((HungerSatisfied < MinHungerBeforeEat && Random.Range(0, 1f) < EatChance) || HungerSatisfied < EmergencyEatValue)
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
                    if (f.InTree)
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

