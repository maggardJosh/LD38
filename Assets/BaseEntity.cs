using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BaseEntity : MonoBehaviour
{

    BoxCollider2D bCollider;
    public LayerMask ImpassableLayer;
    // Use this for initialization
    void Start()
    {
        bCollider = GetComponent<BoxCollider2D>();
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

    private void TryMove()
    {
        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, MoveDir, Time.deltaTime, ImpassableLayer);
        if (wallHit.collider != null)
        {
            if (wallHit.normal.x != 0)
                MoveDir.x *= -1;
            if (wallHit.normal.y != 0)
                MoveDir.y *= -1;
            transform.position = wallHit.point + wallHit.normal * .01f;
        }
        else
        {
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
