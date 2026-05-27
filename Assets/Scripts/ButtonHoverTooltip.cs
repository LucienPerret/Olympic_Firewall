using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class ButtonHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private const float TooltipMaxWidth = 230f;

    private readonly Vector3[] _worldCorners = new Vector3[4];

    private Button _button;
    private Canvas _canvas;
    private RectTransform _buttonRect;
    private RectTransform _tooltipRect;
    private TextMeshProUGUI _tooltipText;
    private Func<string> _contentProvider;
    private bool _isPointerOver;
    private Vector2 _offset = new Vector2(-12f, 12f);

    public void Configure(Func<string> contentProvider, Vector2? offset = null)
    {
        _contentProvider = contentProvider;
        if (offset.HasValue)
        {
            _offset = offset.Value;
        }

        EnsureReferences();
        EnsureTooltip();
        HideTooltip();
    }

    private void Awake()
    {
        EnsureReferences();
    }

    private void Update()
    {
        if (_tooltipRect == null || _contentProvider == null)
        {
            return;
        }

        bool shouldShow = _isPointerOver &&
                          _button != null &&
                          _button.IsInteractable();

        if (!shouldShow)
        {
            HideTooltip();
            return;
        }

        ShowTooltip();
    }

    private void OnDisable()
    {
        _isPointerOver = false;
        HideTooltip();
    }

    private void OnDestroy()
    {
        if (_tooltipRect != null)
        {
            Destroy(_tooltipRect.gameObject);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerOver = true;
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;
        HideTooltip();
    }

    private void EnsureReferences()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        if (_buttonRect == null)
        {
            _buttonRect = transform as RectTransform;
        }

        if (_canvas == null)
        {
            _canvas = GetComponentInParent<Canvas>();
        }
    }

    private void EnsureTooltip()
    {
        if (_tooltipRect != null || _canvas == null)
        {
            return;
        }

        GameObject tooltipObject = new GameObject("HoverTooltip", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        tooltipObject.transform.SetParent(_canvas.transform, false);

        _tooltipRect = tooltipObject.GetComponent<RectTransform>();
        _tooltipRect.pivot = new Vector2(1f, 0f);
        _tooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
        _tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);

        Image background = tooltipObject.GetComponent<Image>();
        background.color = new Color(0.06f, 0.09f, 0.15f, 0.95f);
        background.raycastTarget = false;

        VerticalLayoutGroup layoutGroup = tooltipObject.GetComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(12, 12, 10, 10);
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        ContentSizeFitter sizeFitter = tooltipObject.GetComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textObject.transform.SetParent(tooltipObject.transform, false);

        LayoutElement layoutElement = textObject.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = TooltipMaxWidth;

        _tooltipText = textObject.GetComponent<TextMeshProUGUI>();
        _tooltipText.raycastTarget = false;
        _tooltipText.fontSize = 20f;
        _tooltipText.enableWordWrapping = true;
        _tooltipText.overflowMode = TextOverflowModes.Overflow;
        _tooltipText.alignment = TextAlignmentOptions.TopLeft;
        _tooltipText.color = new Color(0.94f, 0.97f, 1f, 1f);

        TMP_Text sourceText = GetComponentInChildren<TMP_Text>();
        if (sourceText != null)
        {
            _tooltipText.font = sourceText.font;
            _tooltipText.fontSharedMaterial = sourceText.fontSharedMaterial;
        }

        tooltipObject.SetActive(false);
    }

    private void ShowTooltip()
    {
        if (_tooltipRect == null || _tooltipText == null || _contentProvider == null)
        {
            return;
        }

        string content = _contentProvider.Invoke();
        if (string.IsNullOrWhiteSpace(content))
        {
            HideTooltip();
            return;
        }

        _tooltipText.text = content;
        _tooltipRect.gameObject.SetActive(true);
        _tooltipRect.SetAsLastSibling();

        LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRect);
        UpdateTooltipPosition();
    }

    private void HideTooltip()
    {
        if (_tooltipRect != null)
        {
            _tooltipRect.gameObject.SetActive(false);
        }
    }

    private void UpdateTooltipPosition()
    {
        if (_buttonRect == null || _canvas == null || _tooltipRect == null)
        {
            return;
        }

        _buttonRect.GetWorldCorners(_worldCorners);
        Vector3 targetCorner = _worldCorners[2];

        RectTransform canvasRect = _canvas.transform as RectTransform;
        if (canvasRect == null)
        {
            return;
        }

        Camera eventCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(eventCamera, targetCorner);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, eventCamera, out Vector2 localPoint))
        {
            return;
        }

        _tooltipRect.anchoredPosition = localPoint + _offset;
        ClampToCanvas(canvasRect);
    }

    private void ClampToCanvas(RectTransform canvasRect)
    {
        Vector2 size = _tooltipRect.rect.size;
        Vector2 position = _tooltipRect.anchoredPosition;
        Rect canvasBounds = canvasRect.rect;

        float minX = canvasBounds.xMin + size.x;
        float maxX = canvasBounds.xMax;
        float minY = canvasBounds.yMin;
        float maxY = canvasBounds.yMax - size.y;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        _tooltipRect.anchoredPosition = position;
    }
}
