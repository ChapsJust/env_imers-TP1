using UnityEngine;

public class SymbolAnimation : MonoBehaviour
{
    [SerializeField] private float animationDuration = 0.3f;

    private float timer = 0;
    private Vector3 targetScale;

    void Start()
    {
        targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    // Anime le symbole en le faisant grandir avec un effet de rebond
    void Update()
    {
        if (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;
            // Ease out bounce (effet)
            t = 1 - Mathf.Pow(1 - t, 3);
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
        }
    }
}