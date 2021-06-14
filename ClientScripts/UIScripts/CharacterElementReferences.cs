using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class CharacterElementReferences : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TMP_Text CharacterName;
    public TMP_Text CharacterTotalLevel;
    public TMP_Text CharacterLocation;

    private EnterWorldButton EnterWorld;

    [SerializeField]
    private GameObject RightSideGroup;

    public GameObject HighlightImage;

    public void Awake()
    {
        EnterWorld = GameObject.Find("EnterWorldButton").GetComponent<EnterWorldButton>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (HighlightImage.activeSelf)
        {
            RightSideGroup.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (HighlightImage.activeSelf)
        {
            RightSideGroup.SetActive(false);
        }
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            EnterWorld.EnterSelectedWorld();
        }
    }
}
