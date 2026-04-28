using UnityEngine;

[DisallowMultipleComponent]
public class PlacementRangePreview : MonoBehaviour
{
    [SerializeField] private float previewAlpha = 0.18f;
    [SerializeField] private int sortingOrderOffset = -1;

    private const string PreviewObjectName = "PlacementRangePreview";

    private static Sprite _circleSprite;

    private GameObject _previewObject;
    private SpriteRenderer _previewRenderer;

    private void Awake()
    {
        EnsurePreview();
    }

    public void Configure(float radius)
    {
        EnsurePreview();

        if (_previewObject == null)
        {
            return;
        }

        _previewObject.transform.localScale = Vector3.one * Mathf.Max(radius * 2f, 0f);
        _previewObject.SetActive(radius > 0f);
        MatchSorting();
        ApplyFilter(Color.white);
    }

    public void ApplyFilter(Color color)
    {
        EnsurePreview();
        if (_previewRenderer == null)
        {
            return;
        }

        Color previewColor = color;
        previewColor.a = Mathf.Clamp01(color.a) * previewAlpha;
        _previewRenderer.color = previewColor;
    }

    public void Hide()
    {
        if (_previewObject != null)
        {
            _previewObject.SetActive(false);
        }
    }

    private void EnsurePreview()
    {
        if (_previewObject != null && _previewRenderer != null)
        {
            return;
        }

        Transform existingPreview = transform.Find(PreviewObjectName);
        if (existingPreview == null)
        {
            _previewObject = new GameObject(PreviewObjectName);
            _previewObject.transform.SetParent(transform, false);
            _previewObject.transform.localPosition = Vector3.zero;
            _previewObject.transform.localRotation = Quaternion.identity;
            _previewObject.transform.localScale = Vector3.one;
            _previewRenderer = _previewObject.AddComponent<SpriteRenderer>();
        }
        else
        {
            _previewObject = existingPreview.gameObject;
            _previewRenderer = _previewObject.GetComponent<SpriteRenderer>();
            if (_previewRenderer == null)
            {
                _previewRenderer = _previewObject.AddComponent<SpriteRenderer>();
            }
        }

        _previewRenderer.sprite = GetCircleSprite();
        _previewRenderer.drawMode = SpriteDrawMode.Simple;
        MatchSorting();
    }

    private void MatchSorting()
    {
        if (_previewRenderer == null)
        {
            return;
        }

        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer == _previewRenderer)
            {
                continue;
            }

            _previewRenderer.sortingLayerID = renderer.sortingLayerID;
            _previewRenderer.sortingOrder = renderer.sortingOrder + sortingOrderOffset;
            return;
        }
    }

    private static Sprite GetCircleSprite()
    {
        if (_circleSprite != null)
        {
            return _circleSprite;
        }

        const int textureSize = 128;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        texture.name = "PlacementRangePreviewCircle";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
        float radius = textureSize / 2f - 1f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = distance <= radius ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        _circleSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            texture.width
        );

        return _circleSprite;
    }
}
