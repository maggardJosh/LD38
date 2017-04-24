using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Pet : BaseEntity
{
    [Header("Stats")]
    public State CurrentState = State.IDLE;

    public float lifeCount = 0;

    public float HungerSatisfied = 1f;
    public float SleepSatisfied = 1f;

    [Header("Stats Settings")]
    public float SecondsPerCoin = 5f;
    public float EvolveTime = 10f;
    [Space(10)]
    public float SleepDecay = .1f;
    public float HungerDecay = .1f;
    [Space(10)]
    public float EatChance = .2f;
    public float SleepChance = .2f;

    public float MoveChance = .7f;
    public float WakeUpChance = .3f;
    public float speed = 1.0f;
    [Space(10)]
    public float MinHungerBeforeEat = .7f;
    public float MinTiredBeforeSleep = .7f;

    public float EmergencyEatValue = .3f;
    public float EmergencySleepValue = .3f;

    [Header("Settings")]
    public PetType MyPetType = PetType.BLUE;
    public Sprite normalSprite;
    public Sprite sleepSprite;

    public GameObject nextEvolutionPrefab;
    public Sprite FruitSprite;
    public AudioSource evolveAudio;


    public enum PetType
    {
        BLUE,
        RED,
        GREEN,
        PURPLE,
        ORANGE
    }

    public enum State
    {
        IDLE,
        MOVING,
        ATTACKING,
        EATING,
        SLEEPING,
        EVOLVING,
        DYING,
        MOVING_TO_SLEEP,
        MOVING_TO_FOOD
    }

    private SpriteRenderer sRenderer;
    private void Start()
    {
        animCount = Random.Range(0, GameManager.Instance.PetIdleTime);
        sRenderer = GetComponent<SpriteRenderer>();
    }
    [Header("Misc")]

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
            case State.MOVING_TO_FOOD:
            case State.MOVING_TO_SLEEP:
                scaleValue = GameManager.Instance.PetMoveCurve.Evaluate(animCount / GameManager.Instance.PetMoveTime);
                transform.localScale = new Vector3(1 + scaleValue, 1 - scaleValue, 1);
                while (animCount > GameManager.Instance.PetMoveTime * 2f)
                    animCount -= GameManager.Instance.PetMoveTime * 2f;
                break;
            case State.EATING:
                scaleValue = GameManager.Instance.PetEatCurve.Evaluate(animCount / GameManager.Instance.PetEatTime);
                transform.localScale = new Vector3(1 + scaleValue, 1 - scaleValue, 1);
                if (eatingFruit == null)
                {
                    CurrentState = State.IDLE;
                    break;
                }


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
                transform.localScale = new Vector3(1 + scaleValue, 1 + scaleValue * .5f, 1);
                if (animCount >= GameManager.Instance.EvolveTime * GameManager.Instance.EvolveLoop)
                {
                    GameObject newPet = Instantiate(nextEvolutionPrefab);
                    newPet.transform.position = transform.position;
                    GameManager.SpawnNeedMet(transform.position, GameManager.Instance.EvolveNotification);
                    Pet p = newPet.GetComponent<Pet>();
                    if (p != null)
                    {
                        p.coinCount = p.SecondsPerCoin - 2f;
                    }
                    SoundManager.Play(GameManager.Instance.BirthSound);
                    Destroy(gameObject);
                }
                break;
            case State.DYING:
                scaleValue = GameManager.Instance.DieCurve.Evaluate(animCount / GameManager.Instance.DieTime);
                transform.localScale = new Vector3(1 - scaleValue, 1 + scaleValue, 1);
                if (animCount >= GameManager.Instance.DieTime * GameManager.Instance.DieLoop)
                {
                    SoundManager.Play(GameManager.Instance.DeathSound);
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
                    SleepSatisfied += .2f;
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

    public bool RethinkFruit()
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
            CurrentState = State.MOVING_TO_FOOD;
            eatingFruit = closestFruit;
            return true;
        }
        return false;
    }

    public float DecisionTime = .3f;

    public float coinCount = 0;
    public Vector3 moveTarget;
    private float moveToSleepCount = 0;
    private const float MAX_MOVE_TO_SLEEP_TIME = 3.0f;
    private const float ShowIndValue = 1.5f;
    protected override void HandleFixedUpdate()
    {
        //  if ((int)(HungerSatisfied * ShowIndValue) != (int)((HungerSatisfied - HungerDecay * Time.fixedDeltaTime) * ShowIndValue))

        HungerSatisfied -= HungerDecay * Time.fixedDeltaTime;

        if (CurrentState != State.SLEEPING)
        {
            //  if ((int)(SleepSatisfied * ShowIndValue) != (int)((SleepSatisfied - SleepDecay * Time.fixedDeltaTime) * ShowIndValue))

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
            if (CheckDeath())
                return;
            switch (CurrentState)
            {
                case State.IDLE:
                    if (!CheckEvolve() && !CheckHunger() && !CheckSleep())
                        if (Random.Range(0, 1f) <= MoveChance)
                        {
                            CurrentState = State.MOVING;
                            Bounds playArea = GameManager.Instance.playArea.bounds;
                            Vector2 randomTarget = new Vector2(Random.Range(playArea.center.x - playArea.extents.x, playArea.center.x + playArea.extents.y), Random.Range(playArea.center.y - playArea.extents.y, playArea.center.y + playArea.extents.y));
                            moveTarget = randomTarget;
                            MoveDir = (randomTarget - new Vector2(transform.position.x, transform.position.y)).normalized * speed;
                            if (GameManager.Instance.DrawDebug)
                                Debug.DrawLine(transform.position, randomTarget, Color.blue, GameManager.Instance.DebugDrawTime);
                        }
                    break;
                case State.MOVING:
                    if (((transform.position + MoveDir) - moveTarget).sqrMagnitude > ((transform.position - MoveDir) - moveTarget).sqrMagnitude)
                    {
                        MoveDir = Vector2.zero;
                        CurrentState = State.IDLE;
                    }
                    break;
                case State.MOVING_TO_SLEEP:
                    moveToSleepCount += Time.fixedDeltaTime;
                    if (GameManager.Instance.napArea.bounds.Contains(transform.position) &&
                        (transform.position - moveTarget).sqrMagnitude < .004f)
                    {
                        CurrentState = State.SLEEPING;
                        MoveDir = Vector2.zero;
                    }
                    else
                    {
                        if (moveToSleepCount > MAX_MOVE_TO_SLEEP_TIME)
                        {
                            RethinkSleep();
                        }
                        MoveDir = Vector3.RotateTowards(MoveDir, (moveTarget - transform.position).normalized * speed, .5f, 1f);
                    }
                    break;
                case State.MOVING_TO_FOOD:
                    if (eatingFruit == null)
                        CurrentState = State.IDLE;
                    else
                    if (eatingFruit.IsAvailable)
                    {
                        MoveDir = Vector3.RotateTowards(MoveDir, (new Vector3(eatingFruit.bCollider.offset.x, eatingFruit.bCollider.offset.y) + eatingFruit.transform.position - transform.position).normalized * speed, .5f, 1f);
                    }
                    else if (eatingFruit.InTree)
                    {
                        MoveDir = Vector3.RotateTowards(MoveDir, (new Vector3(eatingFruit.treeParent.bCollider.offset.x, eatingFruit.treeParent.bCollider.offset.y) + eatingFruit.treeParent.transform.position - transform.position).normalized * speed, .5f, 1f);
                    }
                    else
                        RethinkFruit();
                    break;
            }
            count = 0;
        }

        base.HandleFixedUpdate();
    }


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
        if (CurrentState == State.EATING)
            return false;
        if (HungerSatisfied <= 0 || SleepSatisfied <= 0)
        {
            Die();
            return true;
        }
        return false;
    }
    int lastDeathCount = 0;
    private void Die()
    {
        if (lastDeathCount != Mathf.FloorToInt(count))
        {
            lastDeathCount = Mathf.FloorToInt(count);

            if (HungerSatisfied <= 0)
                GameManager.SpawnNeedMet(transform.position, FruitSprite, false);
            else if (SleepSatisfied <= 0)
                GameManager.SpawnNeedMet(transform.position, GameManager.Instance.SleepNotification, false);
        }

        if (CurrentState == State.DYING)
            return;
        CurrentState = State.DYING;
        MoveDir = Vector2.zero;
        animCount = 0;
    }
    private void RethinkSleep()
    {
        if (!GameManager.Instance.napArea.bounds.Contains(transform.position))
        {
            CurrentState = State.MOVING_TO_SLEEP;
            moveToSleepCount = 0;

            Bounds napArea = GameManager.Instance.napArea.bounds;
            Vector2 randomTarget = new Vector2(Random.Range(napArea.center.x - napArea.extents.x, napArea.center.x + napArea.extents.y), Random.Range(napArea.center.y - napArea.extents.y, napArea.center.y + napArea.extents.y));
            moveTarget = randomTarget;
            if (GameManager.Instance.DrawDebug)
                Debug.DrawLine(transform.position, moveTarget, Color.cyan, GameManager.Instance.DebugDrawTime);
            MoveDir = (randomTarget - new Vector2(transform.position.x, transform.position.y)).normalized * speed;
        }
        else
        {
            CurrentState = State.SLEEPING;
            MoveDir = Vector2.zero;
        }
    }
    private bool CheckSleep()
    {
        if ((SleepSatisfied < MinTiredBeforeSleep && Random.Range(0, 1f) < SleepChance) || SleepSatisfied < EmergencySleepValue)
        {
            GameManager.SpawnNeedMet(transform.position, GameManager.Instance.SleepNotification, false);
            RethinkSleep();
            return true;
        }
        return false;
    }

    private bool CheckHunger()
    {
        if ((HungerSatisfied < MinHungerBeforeEat && Random.Range(0, 1f) < EatChance) || HungerSatisfied < EmergencyEatValue)
        {
            GameManager.SpawnNeedMet(transform.position, FruitSprite, false);
            return RethinkFruit();
        }

        return false;
    }

    public Fruit eatingFruit = null;
    public void TryEat(Fruit f)
    {
        if (f.PetType == MyPetType && HungerSatisfied <= MinHungerBeforeEat && CurrentState == State.MOVING_TO_FOOD)
        {
            eatingFruit = f;
            f.IsAvailable = false;
            CurrentState = State.EATING;
            MoveDir = Vector3.zero;
            animCount = 0;
        }
    }
}

