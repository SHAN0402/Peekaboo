using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// A single pending-request row that can never block other UI.
[RequireComponent(typeof(RectTransform))]
public class FriendRequestRowUI : MonoBehaviour
{
    [Header("UI refs")]
    [SerializeField] TMP_Text nameLabel;
    [SerializeField] Button   acceptBtn;
    [SerializeField] Button   declineBtn;

    /* ------------------------------------------------------------------ */
    /*  SAFETY-NET AGAINST “INVISIBLE SHIELD”                             */
    /*  – turns off any ray-cast target on the row root automatically     */
    /*  – clamps size so the row can’t stretch beyond the viewport        */
    /* ------------------------------------------------------------------ */
    void Awake()
    {
        // 1) make absolutely sure the row root does NOT intercept clicks
        if (TryGetComponent(out Image img))      img.raycastTarget = false;
        if (TryGetComponent(out TMP_Text txt))   txt.raycastTarget = false;

        // 2) force a normal row height (layout will respect this)
        var rt = (RectTransform)transform;
        rt.anchorMin = new Vector2(0, 1);   // stretch horizontally only
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot     = new Vector2(.5f, 1);
        rt.sizeDelta = new Vector2(0, 40);  // Width handled by layout, Height 40px

        // 3) if prefab accidentally had a LayoutElement with huge min values,
        //    reset them so it can’t blow up
        if (TryGetComponent(out LayoutElement le))
        {
            le.minHeight      = 0;
            le.preferredHeight= 40;
            le.flexibleHeight = -1;
        }
    }

    /* ------------------------------------------------------------------ */
    /*  INITIALISATION                                                    */
    /* ------------------------------------------------------------------ */
    public void Init(string requesterId, string username,
                     System.Action<string> onAccept,
                     System.Action<string> onDecline)
    {
        nameLabel.text = username;

        acceptBtn.onClick.RemoveAllListeners();
        declineBtn.onClick.RemoveAllListeners();

        acceptBtn.onClick.AddListener(() => onAccept(requesterId));
        declineBtn.onClick.AddListener(() => onDecline(requesterId));
    }
}