using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StoreUnlockButton : MonoBehaviour, IPointerClickHandler
{
    
    public int GoldCost = 1;
    public ScrollRect parentScroll;
    public GameObject[] itemsToEnable;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if(GameManager.Instance.CoinCount > GoldCost)
        {
            GameManager.Instance.SubtractGold(GoldCost);
            foreach (GameObject o in itemsToEnable)
                o.SetActive(true);
            transform.parent.gameObject.SetActive(false);
        }
    }
}
