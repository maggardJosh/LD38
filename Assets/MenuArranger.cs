using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MenuArranger : MonoBehaviour
{

    public float iconWidth = 70;
    public float margins = 20f;

    // Use this for initialization
    void Start()
    {

    }


    // Update is called once per frame
    void LateUpdate()
    {
        int ind = 0;
        foreach (RectTransform t in transform)
        {
            if (!t.gameObject.activeSelf)
                continue;
            t.sizeDelta = new Vector2(iconWidth, 100);
            t.anchoredPosition = new Vector2(margins + iconWidth * ind, 0);
            ind++;
        }
        ((RectTransform)transform).sizeDelta = new Vector2(ind * iconWidth + margins*2f, 100);
    }
}
