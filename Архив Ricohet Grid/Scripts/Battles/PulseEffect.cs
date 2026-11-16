using UnityEngine;

public class PulseEffect : MonoBehaviour
{
    private Renderer _renderer;
    private float _minAlpha = 0.3f;
    private float _maxAlpha = 0.6f;
    private float _speed = 2f;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        float alpha = Mathf.Lerp(_minAlpha, _maxAlpha, (Mathf.Sin(Time.time * _speed) + 1f) / 2f);
        Color color = _renderer.material.color;
        color.a = alpha;
        _renderer.material.color = color;
    }
}