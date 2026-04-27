using UnityEngine;

[DisallowMultipleComponent]
public class SpriteColorFilter : MonoBehaviour
{
    private SpriteRenderer[] _spriteRenderers = System.Array.Empty<SpriteRenderer>();
    private Color[] _defaultColors = System.Array.Empty<Color>();

    private void Awake()
    {
        CacheRenderers();
    }

    public void SetColor(Color color)
    {
        if (_spriteRenderers.Length == 0)
        {
            CacheRenderers();
        }

        foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
        {
            if (spriteRenderer == null)
            {
                continue;
            }

            spriteRenderer.color = color;
        }
    }

    public void ResetColor()
    {
        if (_spriteRenderers.Length == 0)
        {
            CacheRenderers();
        }

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            if (_spriteRenderers[i] == null)
            {
                continue;
            }

            _spriteRenderers[i].color = _defaultColors[i];
        }
    }

    private void CacheRenderers()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        _defaultColors = new Color[_spriteRenderers.Length];

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            _defaultColors[i] = _spriteRenderers[i].color;
        }
    }
}
