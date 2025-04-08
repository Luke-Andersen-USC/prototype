using System;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class Skeleton : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Walking,
        Attacking,
        Flying,
        Dead,
        Falling
    }

    private BehaviorTree _bt;
    public EnemyState _currentState;
    private Animator _animator;
    private AudioSource _audioSource;

    [SerializeField] GameObject _spareParts;
    
    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Image icon;
    [SerializeField] private Transform uiWorldPos;
    
    [Header("SFX")] 
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private float _swingVol = 1f;
    [SerializeField] private float _deathVol = 1f;
    [SerializeField] private float _landVol = 1f;

    public bool IsGrounded = false;
    
    void Awake()
    {
        _animator = GetComponent<Animator>();
        _bt = GetComponent<BehaviorTree>();
        _audioSource = GetComponent<AudioSource>();

        if (_bt == null)
        {
            Debug.LogWarning("No reference to bt for " + gameObject.name);
        }
    }

    private void Start()
    {
        SwitchState(EnemyState.Idle);
    }

    private void Update()
    {
        UpdateUI();
    }

    public void Die()
    {
        if (_currentState != EnemyState.Dead)
        {
            _audioSource.PlayOneShot(deathSound, _deathVol);
            Instantiate(_spareParts, gameObject.transform.position, Quaternion.identity);
            SwitchState(EnemyState.Dead);
            StartCoroutine(DestroySkeleton());
        }
    }

    private IEnumerator DestroySkeleton()
    {
        _bt.enabled = false;
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        float timer = stateInfo.length;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        
        EnemyManager.Instance.DestroyEnemy(gameObject);
    }

    public AnimatorStateInfo GetCurrentAnimStateInfo()
    {
        return _animator.GetCurrentAnimatorStateInfo(0);
    }
    
    public void SwitchState(EnemyState state)
    {
        if (_currentState != state)
        {
            if (state == EnemyState.Attacking)
            {
                _audioSource.PlayOneShot(swingSound, _swingVol);
            }
            else if (state == EnemyState.Idle && _currentState == EnemyState.Flying)
            {
                _audioSource.PlayOneShot(landSound, _landVol);
            }
            else if (state == EnemyState.Falling)
            {
                _bt.enabled = false;
            }
            
            _currentState = state;
            _animator.SetInteger("EnemyState", (int)_currentState);
        }
    }
    
    private void UpdateUI()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(uiWorldPos.position);
        icon.rectTransform.position = screenPos;
    }
}
