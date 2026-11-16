using UnityEngine;

/// <summary>
/// Универсальный мост для передачи событий из анимации (Animation Events) 
/// в основной скрипт логики (Unit), который находится на родительском объекте.
/// </summary>
public class UnitAnimationBridge : MonoBehaviour
{
    // Ссылка на основной скрипт логики
    private Unit _parentUnit;

    private void Awake()
    {
        // Ищем компонент Unit на этом объекте или на любом из родителей
        _parentUnit = GetComponentInParent<Unit>();

        if (_parentUnit == null)
        {
            Debug.LogError($"[UnitAnimationBridge] КРИТИЧЕСКАЯ ОШИБКА: Не удалось найти компонент Unit на родительском объекте для {gameObject.name}!", this);
        }
    }

    // ==========================================
    // МЕТОДЫ ДЛЯ ANIMATION EVENTS
    // ==========================================

    /// <summary>
    /// Вызывается из Animation Event в момент выстрела/удара
    /// </summary>
    public void TriggerAttackExecution()
    {
        if (_parentUnit != null)
        {
            _parentUnit.TriggerAttackExecution();
        }
        else
        {
            Debug.LogWarning("[UnitAnimationBridge] _parentUnit равен null, не могу вызвать атаку!");
        }
    }

    /// <summary>
    /// Пример для будущего: вызов звука шага из анимации
    /// </summary>
    public void PlayFootstepSound()
    {
        // Здесь можно вызвать метод из Unit или напрямую проиграть звук
        // if (_parentUnit != null) _parentUnit.OnFootstep();
    }

    /// <summary>
    /// Пример для будущего: событие получения удара (дергание анимации)
    /// </summary>
    public void OnHitReaction()
    {
        // if (_parentUnit != null) _parentUnit.PlayHitEffect();
    }
}