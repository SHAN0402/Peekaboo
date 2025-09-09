using UnityEngine;



public class SidePanelController : MonoBehaviour
{
    public RectTransform sidePanel;
    public float hiddenY = -1413f;
    public float shownY = 492f;
    public float moveSpeed = 500f;

    private bool isVisible = false;

    void Update()
    {
        float targetY = isVisible ? shownY : hiddenY;
        Vector2 pos = sidePanel.anchoredPosition;
        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * 5);
        sidePanel.anchoredPosition = pos;
    }

    public void TogglePanel()
    {
        isVisible = !isVisible;
    }


}
