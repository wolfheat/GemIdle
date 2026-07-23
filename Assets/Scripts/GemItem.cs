using System;
using TMPro;
using UnityEngine;



public class GemItem : MonoBehaviour
{

    public GemType itemType;

    [SerializeField] private TextMeshProUGUI gemAmountText;

    [SerializeField] private ItemGrowAnimator[] itemGrowAnimators;


    private void OnEnable() => Stats.GemUpdate += OnGemsUpdated;

    private void OnDisable() => Stats.GemUpdate -= OnGemsUpdated;


    private void OnGemsUpdated(GemType type)
    {
        if(type != itemType)
            return;

        gemAmountText.text = Stats.GetGemAmount(itemType).ToString();
        foreach (var itemGrowAnimator in itemGrowAnimators) 
            itemGrowAnimator?.Animate();
    }

}
