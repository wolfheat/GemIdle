using UnityEngine;
using UnityEngine.EventSystems;

public class GemClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicking Gem");
        if(transform.TryGetComponent(out GemItem item)) {
            Stats.AddGems(item.itemType, 1);
        }
    }
}
