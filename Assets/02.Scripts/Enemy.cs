using DG.Tweening;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class Enemy : MonoBehaviour
{
    public GameObject RightHand;
    public GameObject LeftHand;
    public GameObject GaurdEffect;
    public AudioClip CounterSound;
    public AudioClip ParrySound;
    private AudioSource audioSource;
    public float ResetTime;
    public float ShakeSpeed;
    private float _nextTime;
    public GameObject VFX;

    private Vector3 _rightHandLocation;
    private Vector3 _leftHandLocation;

    private const float MAX_HEIGHT = 1f;
    private const float MIN_HEIGHT = -3.0f;
    private Sequence _punchSeq;
    private bool _isPunching = false;
    public static Enemy Instance;
    private bool _successParrying = false;

    public Animator animator;



    void Start()
    {
        Instance = this;
        _rightHandLocation = RightHand.transform.position;
        _leftHandLocation = LeftHand.transform.position;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if(Player.Instance.IsGameOver) return;
        ResetHand();
        MovingHand();
    }

    void MovingHand()
    {
        if (_isPunching) return;
        Vector3 leftDirection = _leftHandLocation - LeftHand.transform.position;
        Vector3 rightDirection = _rightHandLocation - RightHand.transform.position;

        LeftHand.transform.Translate(ShakeSpeed * leftDirection * Time.deltaTime);
        RightHand.transform.Translate(ShakeSpeed * rightDirection * Time.deltaTime);
        
     }

    void ResetHand() 
    {
        if (Time.time < _nextTime) return;

        float height;

        if (Random.value < 0.7f)
        {
            height = Random.Range(0f, MAX_HEIGHT);
        }
        else
        {
            height = Random.Range(MIN_HEIGHT, -1f);
        }
        _leftHandLocation.y = height;

        if (Random.value <0.7f)
        {
            height = Random.Range(0f, MAX_HEIGHT);
        }
        else
        {
            height = Random.Range(MIN_HEIGHT, -1f);
        }
        _rightHandLocation.y = height;


        _nextTime = Time.time + ResetTime;
    }
    public void CounterPunch(PunchType punchType)
    {
        _isPunching = true;
        GameObject punch;
        if (punchType == PunchType.Left)
            punch = LeftHand;
        else
            punch = RightHand;

        Vector3 idle = punch.transform.position;

        _punchSeq = DOTween.Sequence();
        _punchSeq.AppendInterval(0.1f);
        // 앞으로 이동 (0.05초)
        _punchSeq.Append(punch.transform.DOMove(new Vector3(0,0,0), 0.1f).SetEase(Ease.InExpo));
        _punchSeq.Join(punch.transform.DOScale(new Vector3(8f, 8f, 1f), 0.1f).SetEase(Ease.InExpo));
        _punchSeq.AppendInterval(0.1f);
        _punchSeq.JoinCallback(() => _successParrying = Player.Instance.GetIsParrying());


        
        
        _punchSeq.AppendCallback(()=>Player.Instance.SuccesParrying(_successParrying));
        




        _punchSeq.AppendCallback(() => CameraShaking());
        _punchSeq.JoinCallback(() => Player.Instance.SetLight());
        _punchSeq.JoinCallback(() => SetVFX());

        _punchSeq.AppendInterval(0.4f);
        _punchSeq.JoinCallback(() => CameraWhenFailed());


        // 다시 뒤로 (0.1초)
        _punchSeq.Join(punch.transform.DOMove(idle, 0.1f).SetEase(Ease.OutExpo));
        _punchSeq.Join(punch.transform.DOScale(new Vector3(2.5f, 2.5f, 1f), 0.1f).SetEase(Ease.OutExpo));
        _punchSeq.JoinCallback(() => _successParrying = false);
        _punchSeq.OnComplete(() => _isPunching = false);
    }

    private void CameraShaking()
    {
        StartCoroutine(ShakeCamera(0.3f, 20f)); // 0.3초 동안 흔들림
    }

    private IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.position;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude * (duration - elapsed) * (duration - elapsed);
            float y = Random.Range(-1f, 1f) * magnitude * (duration - elapsed) * (duration - elapsed);

            Camera.main.transform.position = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null; // 다음 프레임까지 기다림
        }

        Camera.main.transform.position = originalPos;
    }

    private void SetVFX()
    {
        GameObject vfx;

        if (_successParrying)
            vfx = Instantiate(GaurdEffect, new Vector3(0, -1, 0), Quaternion.identity);
        else
            vfx = Instantiate(VFX, new Vector3(0, 0, 0), Quaternion.identity);
        if(_successParrying)
            audioSource.PlayOneShot(ParrySound);
        else
            audioSource.PlayOneShot(CounterSound);
        vfx.transform.localScale = new Vector3(128, 128, 1);
        Destroy(vfx, 1f);

        if (!_successParrying)
            ScoreManager.Instance.ResetCombo();
    }

    private void CameraWhenFailed()
    {
        if(_successParrying) return;

        Sequence cameraSeq = DOTween.Sequence();
        cameraSeq.Append(Camera.main.transform.DORotate(new Vector3(-25, 0, 0), 0.2f).SetEase(Ease.OutExpo));
        cameraSeq.Join(Camera.main.DOOrthoSize(6.5f, 0.2f).SetEase(Ease.OutExpo));
        cameraSeq.Append(Camera.main.transform.DORotate(new Vector3(0, 0, 0), 0.2f).SetEase(Ease.InExpo));
        cameraSeq.Join(Camera.main.DOOrthoSize(5f, 0.2f).SetEase(Ease.InExpo));
    }
}
