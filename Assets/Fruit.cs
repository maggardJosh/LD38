using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Fruit : BaseEntity
{
    public bool InTree = true;
    public Tree treeParent;
    public bool IsGrown = false;
    public float GrowTime = 2.0f;
    public AnimationCurve GrowCurve;

    public AnimationCurve FallCurve;
    public float FallTime = .3f;
    public bool isFalling = false;
    public float yDisp = 0;
    private Vector3 fallStartPos;
    public Pet.PetType PetType = Pet.PetType.BLUE;



    public SpriteRenderer sRenderer;

    public float HungerValue = .2f;

    public bool IsAvailable = false;
    public float DecayTime = 5.0f;

    void Start()
    {
        sRenderer = GetComponent<SpriteRenderer>();
        GameManager.AddObject(this, GameManager.Instance.Fruits);
    }

    void OnDestroy()
    {
        GameManager.RemoveObject(this, GameManager.Instance.Fruits);
    }

    public override void CollideWithEntity(BaseEntity e)
    {
        if (!IsAvailable)
            return;
        Pet p = e.GetComponent<Pet>();
        if(p != null)
        {
            p.TryEat(this);
        }

        base.CollideWithEntity(e);
    }
    float count;
    protected override void HandleUpdate()
    {
        if (!IsGrown)
        {
            count += Time.deltaTime;
            float t = count / GrowTime;
            float scaleValue = GrowCurve.Evaluate(t);
            transform.localScale = new Vector3(scaleValue, scaleValue, 1);
            if (t >= 1)
            {
                IsGrown = true;
                count = 0;
            }
        }
        if (isFalling)
        {
            count += Time.deltaTime;
            float t = count / FallTime;
            float fallValue = FallCurve.Evaluate(t) * yDisp;
            transform.position = fallStartPos + Vector3.down * fallValue;
            if (t >= 1)
            {
                isFalling = false;
                IsAvailable = true;
                InTree = false;
                count = 0;
            }
        }
        if(IsAvailable)
        {
            count += Time.deltaTime;
            if(count > DecayTime)
            {
                Destroy(gameObject);
            }
        }
        base.HandleUpdate();
    }

    public void Fall(float yDisp)
    {
        fallStartPos = transform.position;
        this.yDisp = yDisp;
        isFalling = true;
        treeParent.GrownFruit.Remove(this);
    }
}

