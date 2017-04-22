using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BaseEntity : MonoBehaviour
{

    public BoxCollider2D bCollider;
    public LayerMask ImpassableLayer;
    public LayerMask InteractLayer;
    void Awake()
    {
        bCollider = GetComponent<BoxCollider2D>();
    }
    // Use this for initialization
    void Start()
    {
    }

    protected virtual void HandleUpdate()
    {

    }

    protected Vector3 MoveDir;
    // Update is called once per frame
    void Update()
    {
        HandleUpdate();
        TryMove();
    }

    public virtual void CollideWithEntity(BaseEntity e)
    {

    }

    private void TryMove()
    {
        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, MoveDir, Time.deltaTime, ImpassableLayer);
        if (wallHit.collider != null)
        {
            BaseEntity e = wallHit.collider.GetComponent<BaseEntity>();
            if (e != null)
                e.CollideWithEntity(this);

            if (wallHit.normal.x != 0)
                MoveDir.x *= -1;
            if (wallHit.normal.y != 0)
                MoveDir.y *= -1;
            transform.position = wallHit.point + wallHit.normal * .01f;
        }
        else
        {
            RaycastHit2D objectHit = Physics2D.Raycast(transform.position, MoveDir, Time.deltaTime, InteractLayer);
            if (objectHit.collider != null)
            {
                BaseEntity e = objectHit.collider.GetComponent<BaseEntity>();
                if (e != null)
                    e.CollideWithEntity(this);
            }
            transform.position = transform.position + MoveDir * Time.deltaTime;
        }

    }

    protected virtual void HandleFixedUpdate()
    {

    }

    void FixedUpdate()
    {
        HandleFixedUpdate();
    }
}
