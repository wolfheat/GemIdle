using System.Collections;
using UnityEngine;

public class CardAnimator : MonoBehaviour
{
    private Coroutine animateRoutine;

    private const float ScaleMax = 1.04f;
    private const float ScaleMin = 1f;
    private const float animateGrowTime = 0.45f;
    private const float animateShrinkTime = 0.45f;


    internal void ResetScale()
    {
        transform.localScale = Vector3.one;
    }
    
    internal void Animate()
    {
        if(!gameObject.activeSelf || !Stats.AnimateCards)
            return;
        StopAllCoroutines();
        animateRoutine = StartCoroutine(AnimateCO());
    }

    private IEnumerator AnimateCO()
    {
        // Reset The Object
        transform.localScale = Vector3.one;
        float animationTimer = 0f;
        float percent = 0f;
        float currentScale = 1f;

        while (animationTimer < animateGrowTime) {
            animationTimer += Time.deltaTime;
            percent = animationTimer / animateGrowTime;
            currentScale = Mathf.Lerp(ScaleMin, ScaleMax, percent);
            transform.localScale = Vector3.one * currentScale;
            yield return null;
        }

        transform.localScale = Vector3.one * ScaleMax;

        while (animationTimer < animateShrinkTime + animateGrowTime) {
            animationTimer += Time.deltaTime;
            percent = (animationTimer - animateGrowTime) / animateShrinkTime;
            currentScale = Mathf.Lerp(ScaleMax, ScaleMin, percent);
            transform.localScale = Vector3.one * currentScale;
            yield return null;
        }

        // Reset Size
        transform.localScale = Vector3.one;

        // Unset the Coroutine
        animateRoutine = null;
    }
}
