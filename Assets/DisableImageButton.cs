using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DisableImageButton : MonoBehaviour, IPointerClickHandler
{
    [Serializable]
    public class DisableEvent : UnityEvent<bool> { }
    public DisableEvent dEvent;
    public bool disabled = false;
    private Image buttonImage;

    // Use this for initialization
    void Start()
    {
        buttonImage = GetComponent<Image>();
        disabled = PlayerPrefs.GetInt("MutePref", 0) == 1;
        UpdateColor();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void UpdateColor()
    {
        buttonImage.material.color = new Color(1, 1, 1, disabled ? .3f : 1f);

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        disabled = !disabled;
        dEvent.Invoke(disabled);
        PlayerPrefs.SetInt("MutePref", disabled ? 1 : 0);
        UpdateColor();
    }
}
