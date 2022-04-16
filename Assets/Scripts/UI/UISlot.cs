using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISlot : MonoBehaviour, IPointerClickHandler
{
    UIManager parentUI;
    //which array slot this element corresponds to
    public int fieldIdx;
    //image element to show cross/zero
    [SerializeField] private Image keptIcon;

    public void Init(UIManager ui, int idx)
    {
        parentUI = ui;
        fieldIdx = idx;
    }

    //called on clicking the button
    public void OnPointerClick(PointerEventData eventData)
    {
       parentUI.gm.PlayerTurn(fieldIdx);
    }

    public void UpdateIcon(byte fig)
    {
        switch(fig)
        {
            case 1:
                keptIcon.gameObject.SetActive(true);
                keptIcon.sprite = parentUI.gm.ZERO_ICON;
                break;
            case 2:
                keptIcon.gameObject.SetActive(true);
                keptIcon.sprite = parentUI.gm.CROSS_ICON;
                break;
            default:
                keptIcon.gameObject.SetActive(false);
                break;
        }
    }
}
