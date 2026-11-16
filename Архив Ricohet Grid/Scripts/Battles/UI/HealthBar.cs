using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _fillContainer;
    [SerializeField] private GameObject _segmentPrefab;

    [Header("Settings")]
    // ВАЖНО: Это значение должно ТОЧНО совпадать с шириной твоего черного фона (Background)
    [SerializeField] private float _totalWidth = 1f;

    private GameObject[] _segments;
    private int _maxHealth;

    public void Initialize(int maxHealth)
    {
        _maxHealth = maxHealth;
        CreateSegments();
        UpdateHealthBar(maxHealth);
    }

    private void CreateSegments()
    {
        // Очищаем старые сегменты
        foreach (Transform child in _fillContainer)
        {
            Destroy(child.gameObject);
        }

        _segments = new GameObject[_maxHealth];

        // ЧИСТАЯ МАТЕМАТИКА: Делим общую ширину на количество здоровья
        float segmentWidth = _totalWidth / _maxHealth;

        // Вычисляем стартовую позицию X (левый край полоски)
        float startX = -_totalWidth / 2f + segmentWidth / 2f;

        for (int i = 0; i < _maxHealth; i++)
        {
            GameObject segment = Instantiate(_segmentPrefab, _fillContainer);

            // Позиционируем сегменты ровно встык друг к другу
            float xPos = startX + i * segmentWidth;
            segment.transform.localPosition = new Vector3(xPos, 0f, 0f);

            // Настраиваем масштаб. 
            // Мы просто задаем локальный Scale.X равным вычисленной ширине.
            Vector3 localScale = segment.transform.localScale;
            segment.transform.localScale = new Vector3(segmentWidth, localScale.y, localScale.z);

            _segments[i] = segment;
        }
    }

    public void UpdateHealthBar(int currentHealth)
    {
        for (int i = 0; i < _maxHealth; i++)
        {
            if (_segments[i] != null)
            {
                _segments[i].SetActive(i < currentHealth);
            }
        }
    }
}