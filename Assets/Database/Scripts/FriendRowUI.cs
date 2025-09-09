using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Image = UnityEngine.UI.Image;

/// A single pending-request row that can never block other UI.
[RequireComponent(typeof(RectTransform))]
public class FriendRowUI : MonoBehaviour
{
    [Header("UI refs")]
    [SerializeField] TMP_Text nameLabel;
    [SerializeField] Image avatarImage;
    [SerializeField] Image outfitImage;
    [SerializeField] Image frameImage;
    [SerializeField] Button avatarButton;
    [SerializeField] Button deleteBtn;

    public string PlayFabId { get; private set; }

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

    public void Init(string id, string displayName, string url, int outfitId, int frameId, FriendListManager owner)
    {
        outfitImage.gameObject.SetActive(false);
        frameImage.gameObject.SetActive(false);
        PlayFabId = id;
        nameLabel.text = displayName;
        if (!string.IsNullOrEmpty(url))
            StartCoroutine(PlayFabData.LoadAvatarImage(url, avatarImage));

        if (outfitId != -1)
        {
            outfitImage.gameObject.SetActive(true);
            PlayFabData.LoadOutfitFromResources(outfitId, outfitImage);
        }

        if (frameId != -1)
        {
            frameImage.gameObject.SetActive(true);
            PlayFabData.LoadFrameFromResources(frameId, frameImage);
        }

        deleteBtn.onClick.RemoveAllListeners();
        deleteBtn.onClick.AddListener(() => owner.RemoveFriend(PlayFabId));

        avatarButton.onClick.RemoveAllListeners();
        avatarButton.onClick.AddListener(() => FriendProfileManager.Instance.ShowProfile(PlayFabId));
    }
}
