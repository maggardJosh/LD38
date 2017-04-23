using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class StoreButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public Sprite DragSprite;
    public GameObject SpawnObj;
    public int GoldCost = 1;
    public ScrollRect parentScroll;
    public GameObject dragImage;
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentScroll.SendMessage("OnBeginDrag", eventData);
        dragImage.GetComponent<Image>().sprite = DragSprite;
    }

    public void OnDrag(PointerEventData eventData)
    {

        if (eventData.position.y > 100)
        {
            dragImage.transform.position = eventData.position;
            dragImage.SetActive(true);

        }
        else
        {
            dragImage.SetActive(false);

            parentScroll.SendMessage("OnDrag", eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        parentScroll.SendMessage("OnEndDrag", eventData);
        if (eventData.position.y > 100)
            GameManager.TrySpawnItem(SpawnObj, GoldCost, eventData.position);
        dragImage.SetActive(false);
    }
}
