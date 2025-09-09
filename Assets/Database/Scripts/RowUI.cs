using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class RowUI : MonoBehaviour
{
    [SerializeField] TMP_Text keyText;
    [SerializeField] TMP_Text valueText;
    [SerializeField] Button deleteButton;

    public string Key { get; private set; }

    public void SetData(string key, string value)
    {
        Key = key;
        keyText.text = key;
        valueText.text = value;
    }

    public void UpdateValue(string newVal) => valueText.text = newVal;
}
