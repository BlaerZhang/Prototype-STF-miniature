using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CirclingResourceUI : MonoBehaviour
{
    public SerializedDictionary<CirclingItemType, GameObject> resourcesUIs;

    void OnEnable()
    {
        CirclingResourceManager.OnItemCountChanged += UpdateItemCount;
    }

    void OnDisable()
    {
        CirclingResourceManager.OnItemCountChanged -= UpdateItemCount;
    }

    void UpdateItemCount(CirclingItemType itemType, int count, int changedCount)
    {
        if (!resourcesUIs.ContainsKey(itemType)) return;
        resourcesUIs[itemType].transform.Find("Resource Icon").GetComponent<Image>().sprite = CirclingResourceManager.Instance.GetItemSprite(itemType);
        resourcesUIs[itemType].transform.Find("Resource Quantity Text").GetComponent<TMP_Text>().text = count.ToString();
    }
}
