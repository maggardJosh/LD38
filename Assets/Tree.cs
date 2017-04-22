﻿
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

    public Pet.PetType PetType = Pet.PetType.BLUE;

    void Start()
    { 
        GameManager.AddObject(this, GameManager.Instance.Trees);
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
            fallingFruit.Fall(FruitDispUpMax);
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
            fruitObject.transform.position = transform.position + new Vector3(Random.Range(FruitDispHorizMin, FruitDispHorizMax), Random.Range(FruitDispUpMin, FruitDispUpMax), 0);
            positioned = true;
            foreach (Fruit fr in GrownFruit)
            {
                if ((fr.transform.position - fruitObject.transform.position).sqrMagnitude < .001f)
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
