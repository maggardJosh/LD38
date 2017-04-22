using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Pet : BaseEntity
{
    public Sprite[] sprites;
    public GameObject nextEvolutionPrefab;

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
        }
        base.HandleUpdate();
    }
    public float DecisionTime = .3f;
    public float MoveChance = .7f;
    public float StopMoveChance = .5f;
    public float speed = 1.0f;
    protected override void HandleFixedUpdate()
    {
        count += Time.fixedUnscaledDeltaTime;
        if (count >= DecisionTime)
        {
            switch (CurrentState)
            {
                case State.IDLE:
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
}

