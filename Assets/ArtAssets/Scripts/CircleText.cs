using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CircleText : MonoBehaviour
{
    public TMP_Text textComponent;
    public float radius = 100f;
    public float startAngle = 0f; // 起始角度（比如 -90 是正上方）

    void Start()
    {
        ArrangeTextInCircle();
    }

    void ArrangeTextInCircle()
    {
        string text = textComponent.text;
        textComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = textComponent.textInfo;

        float angleStep = 360f / text.Length;

        for (int i = 0; i < text.Length; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;


            Transform charTransform = textComponent.transform.GetChild(i);
            if (charTransform == null) continue;

            float angle = startAngle + i * angleStep;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 pos = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
            charTransform.localPosition = pos;
            charTransform.rotation = Quaternion.Euler(0, 0, angle + 90f);
        }
    }
}
