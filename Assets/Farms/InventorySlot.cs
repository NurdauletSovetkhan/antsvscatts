using UnityEngine;
using UnityEngine.UI;
using TMPro; // Раскомментируй, если используешь TMP

public class InventorySlot : MonoBehaviour
{
    [Header("UI Элементы")]
    public Image iconImage;
    public GameObject selectionBorder;
    public TMP_Text amountText; 

    private CropData currentData;

    public void Setup(CropData data, int count)
    {
        currentData = data;

        if (currentData != null)
        {
            iconImage.sprite = currentData.icon;
            iconImage.enabled = true;
            // Показываем количество, если > 0, иначе можно скрывать или писать 0
            if (amountText != null) amountText.text = count.ToString();
        }
        else
        {
            iconImage.enabled = false;
            if (amountText != null) amountText.text = "";
        }
    }

    public void Select() => selectionBorder.SetActive(true);
    public void Deselect() => selectionBorder.SetActive(false);
}