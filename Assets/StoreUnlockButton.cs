using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class StoreUnlockButton : MonoBehaviour, IPointerClickHandler
{

    public int GoldCost = 1;
    public ScrollRect parentScroll;
    public GameObject[] itemsToEnable;
    public Text[] priceTextBoxes;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying)
            return;
        foreach (Text t in priceTextBoxes)
            t.text = GoldCost.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Application.isPlaying)
            return;
        if (GameManager.Instance.CoinCount >= GoldCost)
        {
            foreach (GameObject o in itemsToEnable)
                o.SetActive(true);
            gameObject.SetActive(false);
            GameManager.Instance.SubtractGold(GoldCost);
        }
    }
}
