using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AlphaHitButton : MonoBehaviour, ICanvasRaycastFilter
{
    [Range(0f, 1f)]
    public float alphaThreshold = 0.2f;

    private Image targetImage;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (targetImage == null || targetImage.sprite == null)
            return true;

        RectTransform rectTransform = targetImage.rectTransform;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                screenPoint,
                eventCamera,
                out Vector2 localPoint))
        {
            return false;
        }

        Rect rect = rectTransform.rect;

        float normalizedX = (localPoint.x - rect.x) / rect.width;
        float normalizedY = (localPoint.y - rect.y) / rect.height;

        if (normalizedX < 0 || normalizedX > 1 || normalizedY < 0 || normalizedY > 1)
            return false;

        Sprite sprite = targetImage.sprite;
        Texture2D texture = sprite.texture;
        Rect textureRect = sprite.textureRect;

        int pixelX = Mathf.FloorToInt(textureRect.x + normalizedX * textureRect.width);
        int pixelY = Mathf.FloorToInt(textureRect.y + normalizedY * textureRect.height);

        Color pixelColor = texture.GetPixel(pixelX, pixelY);

        return pixelColor.a >= alphaThreshold;
    }
}
