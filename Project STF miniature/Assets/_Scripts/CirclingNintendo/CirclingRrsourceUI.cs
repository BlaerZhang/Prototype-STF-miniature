using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class CirclingRrsourceUI : MonoBehaviour
{
    public List<GameObject> resourcesUIs;

    void OnEnable()
    {
        CirclingResourceManager.OnItemCountChanged += UpdateItemCount;
    }

    void OnDisable()
    {
        CirclingResourceManager.OnItemCountChanged -= UpdateItemCount;
    }

    void UpdateItemCount(CirclingItemType itemType, int count)
    {
        if (resourcesUIs.Count > (int)itemType)
        {
            resourcesUIs[(int)itemType].transform.Find("Resource Quantity Text").GetComponent<TMP_Text>().text = count.ToString();
        }
    }
}
