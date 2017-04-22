using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class NeedMetNotification : MonoBehaviour {

    public SpriteRenderer sRend;
    public float count = 0;
    Vector3 startPos;
	// Use this for initialization
	void Start () {
        sRend = GetComponent<SpriteRenderer>();
	}
	
    public void SetSprite(Sprite s)
    {
        if (sRend == null)
            sRend = GetComponent<SpriteRenderer>();
        sRend.sprite = s;
    }
	// Update is called once per frame
	void Update () {
        if (count == 0)
            startPos = transform.position;
        count += Time.deltaTime;
        float t = count / GameManager.Instance.NeedMetTime;
        float dispValue = GameManager.Instance.NeedMetCurve.Evaluate(t);
        transform.position = startPos + Vector3.up * (GameManager.Instance.NeedMetYDisp * ( dispValue));
        sRend.material.color = new Color(1, 1, 1, 1 - dispValue);
        if (count >= GameManager.Instance.NeedMetTime)
            Destroy(gameObject);

	}
}
