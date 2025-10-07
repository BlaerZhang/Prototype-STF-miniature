using UnityEngine;
using TMPro;

public class ResourceCounter : MonoBehaviour
{
    public TMP_Text resourceCountText;

    void Start()
    {
        resourceCountText.text = "0";
    }

    public void ChangeResource(int amount)
    {
        resourceCountText.text = (int.Parse(resourceCountText.text) + amount).ToString();
    }
}
