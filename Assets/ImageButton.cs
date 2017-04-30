using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageButton : MonoBehaviour, IPointerClickHandler
{
    [Serializable]
    public class DisableEvent : UnityEvent { }
    public DisableEvent dEvent;



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
        dEvent.Invoke();
    }
}
