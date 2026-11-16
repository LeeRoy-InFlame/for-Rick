using UnityEngine;

public class DestructibleWall : MonoBehaviour
{
    [SerializeField] private int _health = 1;
    [SerializeField] private GameObject _destroyedEffectPrefab;

    public void TakeDamage(int damageAmount)
    {
        _health -= damageAmount;
        if (_health <= 0)
        {
            DestroyWall();
        }
    }

    private void DestroyWall()
    {
        // Здесь логика удаления тайла со сетки и спавна эффекта
        Debug.Log("Стена разрушена!");
        Destroy(gameObject);
    }
}