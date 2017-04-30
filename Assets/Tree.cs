
using System;
using System.Collections.Generic;

using UnityEngine;

public class Tree : BaseEntity
{
    public GameObject FruitPrefab;
    public List<Fruit> GrownFruit = new List<Fruit>();

    public int maxFruit = 3;
    public float FruitGrowTime = 2f;
    public float FruitDispHorizMin = -.19f;
    public float FruitDispHorizMax = .03f;
    public float FruitDispUpMin = .02f;
    public float FruitDispUpMax = .03f;
    public int resellValue = 1;

    public Pet.PetType PetType = Pet.PetType.BLUE;

    void Start()
    {
        GameManager.AddObject(this, GameManager.Instance.Trees);
    }

    void OnDestroy()
    {
        try
        {

            if (GameManager.Instance.Trees.Contains(this))
                GameManager.Instance.Trees.Remove(this);
            foreach (Fruit f in GrownFruit)
                if (f.InTree)
                    Destroy(f.gameObject);
            foreach (TreeHole h in GameManager.Instance.TreeHoles)
                h.RefreshTree();
        }catch(Exception)
        {

        }
    }
    public override void CollideWithEntity(BaseEntity e)
    {
        Fruit fallingFruit = null;
        foreach (Fruit f in GrownFruit)
        {
            if (f.IsGrown)
            {
                fallingFruit = f;
                break;
            }
        }
        if (fallingFruit != null)
        {
            fallingFruit.Fall(FruitDispUpMax);
            if (e is Pet)
            {
                if (((Pet)e).eatingFruit != null)
                    if (((Pet)e).eatingFruit.treeParent == this && ((Pet)e).CurrentState == Pet.State.MOVING_TO_FOOD)
                        ((Pet)e).eatingFruit = fallingFruit;    //They knocked down a different fruit than they expected... Help them out
            }
        }
        base.CollideWithEntity(e);
    }

    protected override void HandleUpdate()
    {
        base.HandleUpdate();
    }

    protected override void HandleFixedUpdate()
    {
        if (GrownFruit.Count < maxFruit)
        {
            bool allGrown = true;
            foreach (Fruit f in GrownFruit)
                if (!f.IsGrown)
                    allGrown = false;
            if (allGrown)
                SpawnFruit();
        }
        base.HandleFixedUpdate();
    }

    private void SpawnFruit()
    {
        GameObject fruitObject = Instantiate(FruitPrefab);
        bool positioned = false;
        int positionTry = 0;
        while (!positioned)
        {
            fruitObject.transform.position = transform.position + new Vector3(UnityEngine.Random.Range(FruitDispHorizMin, FruitDispHorizMax), UnityEngine.Random.Range(FruitDispUpMin, FruitDispUpMax), 0);
            positioned = true;
            foreach (Fruit fr in GrownFruit)
            {
                if ((fr.transform.position - fruitObject.transform.position).sqrMagnitude < .01f)
                    positioned = false;
            }
            if (positionTry++ > 5)
                break;

        }
        Fruit f = fruitObject.GetComponent<Fruit>();
        f.treeParent = this;
        GrownFruit.Add(f);

    }

}

