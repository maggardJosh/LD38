using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Pet : BaseEntity
{
    public Sprite[] sprites;
    public GameObject nextEvolutionPrefab;
    public GameObject FruitPrefab;

    public float HungerSatisfied = 1f;
    public float HungerDecay = .1f;
    public float MinHungerBeforeEat = .7f;

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
        EVOLVING
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
                    HungerSatisfied += .5f;
                    GameManager.SpawnNeedMet(transform.position + Vector3.up * .1f, eatingFruit.sRenderer.sprite);
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
                    GameManager.SpawnNeedMet(transform.position + Vector3.up * .1f, GameManager.Instance.EvolveNotification);
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
    protected override void HandleFixedUpdate()
    {
        HungerSatisfied -= HungerDecay * Time.fixedUnscaledDeltaTime;
        count += Time.fixedUnscaledDeltaTime;
        lifeCount += Time.fixedUnscaledDeltaTime;
        if (count >= DecisionTime)
        {
            switch (CurrentState)
            {
                case State.IDLE:
                    if (!CheckEvolve() && !CheckHunger())
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

    private bool CheckHunger()
    {
        if (HungerSatisfied < MinHungerBeforeEat)
        {
            Fruit closestFruit = null;
            float closestDist = float.MaxValue;
            foreach (Fruit f in GameManager.Instance.Fruits)
            {
                if (f.PetType == this.MyPetType && f.IsAvailable)
                {
                    float distSqr = (transform.position - f.transform.position).sqrMagnitude;
                    if (distSqr < closestDist)
                    {
                        closestFruit = f;
                        closestDist = distSqr;
                    }

                }
            }
            if (closestFruit != null)
            {
                Vector3 targetPos = new Vector3(closestFruit.bCollider.offset.x, closestFruit.bCollider.offset.y) + closestFruit.transform.position;
                MoveDir = targetPos - transform.position;
                MoveDir = MoveDir.normalized * speed;
                if (GameManager.Instance.DrawDebug)
                    Debug.DrawLine(transform.position, targetPos, Color.red, 4f);
                CurrentState = State.MOVING;
                return true;

            }
            Tree closestTree = null;
            foreach (Tree t in GameManager.Instance.Trees)
            {
                if (t.PetType == this.MyPetType)
                {
                    float distSqr = (transform.position - t.transform.position).sqrMagnitude;
                    if (distSqr < closestDist)
                    {
                        closestTree = t;
                        closestDist = distSqr;
                    }
                }
            }
            if (closestTree != null)
            {
                Vector3 targetPos = new Vector3(closestTree.bCollider.offset.x, closestTree.bCollider.offset.y) + closestTree.transform.position;

                MoveDir = targetPos - transform.position;
                MoveDir = MoveDir.normalized * speed;
                if (GameManager.Instance.DrawDebug)
                    Debug.DrawLine(transform.position, targetPos, Color.blue, 4f);
                CurrentState = State.MOVING;
                return true;
            }
        }

        return false;
    }

    public Fruit eatingFruit = null;
    public void TryEat(Fruit f)
    {
        if (f.PetType == MyPetType && HungerSatisfied <= MinHungerBeforeEat)
        {
            eatingFruit = f;
            f.IsAvailable = false;
            CurrentState = State.EATING;
            MoveDir = Vector3.zero;
            animCount = 0;
        }
    }
}

