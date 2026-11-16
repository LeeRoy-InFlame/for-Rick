using UnityEngine;

public class UnitAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _animator;

    [Header("Timing")]
    [SerializeField] private float _shootAnimationDuration = 0.5f;

    // Состояния анимации
    public enum AnimationState { Idle, Moving, Shooting, Dead }
    private AnimationState _currentState = AnimationState.Idle;

    // Хэши параметров Animator (для оптимизации, Морти! Строки — это зло!)
    private static readonly int _isMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int _isShootingHash = Animator.StringToHash("IsShooting");
    private static readonly int _isDeadHash = Animator.StringToHash("IsDead");
    private static readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");

    // Свойство для чтения текущего состояния (вдруг понадобится)
    public AnimationState CurrentState { get { return _currentState; } }

    private void Awake()
    {
        // Если ты забыл назначить Animator в инспекторе, я найду его сам
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        if (_animator == null)
        {
            Debug.LogError($"[UnitAnimationController] На объекте {gameObject.name} нет компонента Animator! Анимации не будет, как и моей веры в тебя.");
            enabled = false;
            return;
        }

        // По умолчанию стоим на месте
        PlayIdle();
    }

    // --- ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ УПРАВЛЕНИЯ АНИМАЦИЕЙ ---

    public void PlayIdle()
    {
        if (_currentState == AnimationState.Dead) return; // Мертвые не отдыхают, Морти

        _currentState = AnimationState.Idle;
        _animator.SetBool(_isMovingHash, false);
        _animator.SetBool(_isShootingHash, false);
    }

    public void PlayMove()
    {
        if (_currentState == AnimationState.Dead) return;

        _currentState = AnimationState.Moving;
        _animator.SetBool(_isMovingHash, true);
        _animator.SetBool(_isShootingHash, false);
    }

    public void PlayShoot()
    {
        if (_currentState == AnimationState.Dead) return;

        _currentState = AnimationState.Shooting;

        // Временно останавливаем движение, чтобы выстрел выглядел нормально
        _animator.SetBool(_isMovingHash, false);
        _animator.SetBool(_isShootingHash, true);

        // Запускаем корутину, чтобы после выстрела вернуться в Idle
        StopAllCoroutines();
        StartCoroutine(ResetAfterShoot());
    }

    public void PlayDie()
    {
        if (_currentState == AnimationState.Dead) return;

        _currentState = AnimationState.Dead;

        // Сбрасываем все триггеры и включаем смерть
        _animator.SetBool(_isMovingHash, false);
        _animator.SetBool(_isShootingHash, false);
        _animator.SetBool(_isDeadHash, true);

        StopAllCoroutines();
    }

    // Дополнительно: если захочешь менять скорость анимации бега в зависимости от скорости юнита
    public void SetMoveSpeed(float speed)
    {
        if (_animator != null && _currentState != AnimationState.Dead)
        {
            _animator.SetFloat(_moveSpeedHash, speed);
        }
    }

    // --- ПРИВАТНАЯ ЛОГИКА ---

    private System.Collections.IEnumerator ResetAfterShoot()
    {
        // Ждем, пока анимация выстрела проигрится
        yield return new WaitForSeconds(_shootAnimationDuration);

        // Если мы все еще живы и не начали двигаться, возвращаемся в Idle
        if (_currentState == AnimationState.Shooting)
        {
            PlayIdle();
        }
    }
}