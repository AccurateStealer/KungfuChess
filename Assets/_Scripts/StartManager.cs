using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float _startSize = 1f;
    [SerializeField] private float _gameSize = 5f;
    [SerializeField] private float _zoomDuration = 1.0f;
    [SerializeField] private Ease _zoomEase = Ease.InOutSine;

    [Header("Menu Bounce Out")]
    [SerializeField] private RectTransform _menuRoot;
    [SerializeField] private float _menuBounceDuration = 0.45f;
    [SerializeField] private float _menuMoveUp = 180f;  
    [SerializeField] private float _menuScaleUp = 1.05f;
    [SerializeField] private float _menuFadeDuration = 0.25f;

    [Tooltip("Optional: CanvasGroup for fading. If null, fade is skipped.")]
    [SerializeField] private CanvasGroup _menuCanvasGroup;

    [Header("Spawning")]
    [SerializeField] private GameObject _king1;
    [SerializeField] private GameObject _king2;
    [SerializeField] private GameObject _smokeVFX;


    private bool _started;
    private Vector2 _menuStartPos;
    private Vector3 _menuStartScale;

    private void Awake()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }

        if (_camera != null)
        {
            _camera.orthographicSize = _startSize;
        }

        if (_menuRoot != null)
        {
            _menuStartPos = _menuRoot.anchoredPosition;
            _menuStartScale = _menuRoot.localScale;
        }
    }

    public void Play()
    {
        if (_started) return;

        _started = true;

        DOTween.Kill(_menuRoot);
        DOTween.Kill(_camera);

        Instantiate(_smokeVFX, _king1.transform.position, Quaternion.identity);
        Instantiate(_smokeVFX, _king2.transform.position, Quaternion.identity);
        Destroy(_king1);
        Destroy(_king2);


        Sequence sequence = DOTween.Sequence();
        sequence.SetUpdate(true);

        if (_menuRoot != null)
        {
            sequence.Append(_menuRoot.DOScale(_menuStartScale * _menuScaleUp, _menuBounceDuration * 0.35f)
                .SetEase(Ease.OutBack));

            sequence.Append(_menuRoot.DOAnchorPos(_menuStartPos + Vector2.up * _menuMoveUp, _menuBounceDuration)
                .SetEase(Ease.InBack));

            sequence.Join(_menuRoot.DOScale(_menuStartScale * 0.9f, _menuBounceDuration)
                .SetEase(Ease.InBack));

            if (_menuCanvasGroup != null)
            {
                sequence.Join(_menuCanvasGroup.DOFade(0f, _menuFadeDuration).SetEase(Ease.OutQuad));
            }

            sequence.AppendCallback(() =>
            {
                _menuRoot.gameObject.SetActive(false);
            });
        }

        if (_camera != null)
        {
            sequence.Append(_camera.DOOrthoSize(_gameSize, _zoomDuration).SetEase(_zoomEase));
        }

        sequence.AppendCallback(StartGame);

        sequence.Play();
    }

    public void StartGame()
    {
        GameManager.Instance.StartMatch();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
