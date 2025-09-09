using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [SerializeField] Button profileBtn;
    [SerializeField] GameObject profilePanel;
    [SerializeField] GameObject previewPanel;
    [SerializeField] Image outfitImage;
    [SerializeField] Image frameImage;
    [SerializeField] Button friendBtn;
    
    // Start is called before the first frame update
    void Start()
    {
        profilePanel.SetActive(false);
        profileBtn.onClick.AddListener(OpenProfile);
        friendBtn.onClick.AddListener(() => UI_ButtonInteraction.LoadScene("Friends"));
    }

    void OpenProfile()
    {
        profilePanel.SetActive(true);
        previewPanel.SetActive(false);
        outfitImage.gameObject.SetActive(false);
        frameImage.gameObject.SetActive(false);

        OutfitManager.Instance.LoadOutfit();
        FrameManager.Instance.LoadFrame();
    }
}
