using System.Collections;
using UnityEngine;

public class DestroySelfTimer : MonoBehaviour
{
    [SerializeField] float _timer = 10f;

    private void Awake()
    {
        StartCoroutine(DestroyTimer());
    }

    private IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(_timer);

        Destroy(this);
    }
}
