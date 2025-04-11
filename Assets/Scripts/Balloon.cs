using System.Collections;
using System.Collections.Generic;
using Autodesk.Fbx;
using UnityEngine;
using UnityEngine.VFX;

public class Balloon : MonoBehaviour
{
    [SerializeField] private GameObject _balloons;
    [SerializeField] private GameObject _balloonVFX;
    [SerializeField] private Vector3 _spawnSize;
    [SerializeField] private Vector3 _defaultSize;
    [SerializeField] private float _destroyWaitTime = 1f;
    [SerializeField] private float _growTime = 1f;

    private VisualEffect _popVFX;
    void Awake()
    {
        _popVFX = _balloonVFX.GetComponent<VisualEffect>();
        if (_popVFX == null)
        {
            Debug.LogWarning("couldn't get pop vfx!");
        }
        _popVFX.Stop();
    }

    void Start()
    {
        StartCoroutine(GrowBalloon(_growTime));
    }

    public void Pop()
    {
        StartCoroutine(DestroyBalloon(_destroyWaitTime));
    }

    private IEnumerator DestroyBalloon(float waitTime)
    {
        _balloons.SetActive(false);
        _popVFX.Play();

        yield return new WaitForSeconds(_destroyWaitTime);
        
        Destroy(gameObject);
    }

    private IEnumerator GrowBalloon(float growTime)
    {
        _balloons.transform.localScale = _spawnSize;

        float timeElapsed = 0f;
        while (timeElapsed < growTime)
        {
            float f = timeElapsed / growTime;
            _balloons.transform.localScale = Vector3.Lerp(_spawnSize, _defaultSize, f);

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        _balloons.transform.localScale = _defaultSize;
    }
}
