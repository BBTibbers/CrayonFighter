using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UIElements;
using UnityEngine.Rendering.Universal;


public class Player : MonoBehaviour
{
    public GameObject LightObject;
    public GameObject PlayerLeftHand;
    public GameObject PlayerRightHand;
    public GameObject PivotHeadLeft;
    public GameObject PivotHeadRight;
    public GameObject PivotBodyLeft;
    public GameObject PivotBodyRight;
    public GameObject[] HeadHitEffect;
    public GameObject[] BodyHitEffect;
    public GameObject[] GaurdEffect;
    public AudioClip[] HeadSound;
    public AudioClip[] BodySound;
    public AudioClip GaurdSound;
    private AudioSource audioSource;
    public int MaxHealth = 4;
    private int _health = 4;
    private bool _effectHead;
    private Vector3 _targetPoint;
    private Vector3 _leftHead = new Vector3(-2f,2f,-1);
    private Vector3 _rightHead = new Vector3(2f, 2f, -1);
    private Vector3 _leftBody = new Vector3 (-2f,-3f, -1);
    private Vector3 _rightBody = new Vector3 (2f,-3f,-1);
    private Vector3 _leftIdle;
    private Vector3 _rightIdle;
    private bool _isPunching;
    private bool _isGaurding = false;
    private Sequence _punchSeq;
    private Sequence _breathSeq;
    private Sequence _parryingSeq;
    private PunchType _punchingType;
    public bool _isParrying = false;
    private bool _isParryingDelay = false;
    public bool IsGameOver = true;
    private float _lastParryTime = 0;
    private const float ParryCoolDown = 5f;

    public static Player Instance;


    void Start()
    {
        Instance = this;
        _leftIdle = PlayerLeftHand.transform.position;
        _rightIdle = PlayerRightHand.transform.position;
        audioSource = GetComponent<AudioSource>();
        //Breathing();

    }

    void Update()
    {
        if (IsGameOver) return;
        GetKey();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            IsParrying();
        }
        UI_Game.Instance.SetSlider(Time.time-_lastParryTime);
    }

    public void Init()
    {
        Camera.main.transform.eulerAngles = new Vector3(0, 0, 0); 
        _health = MaxHealth;
        SetLight();
        UI_Game.Instance.UpdateHealth(_health);
        IsGameOver = false;

        DOVirtual.DelayedCall(0.5f, () => Breathing());
        Debug.Log("게임 시작");
    }

    public void Regame()
    {
        _breathSeq.Kill();
        Init();
        ScoreManager.Instance.Regame();
    }

    void GetKey()
    {
        if (_isPunching) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            _targetPoint = _leftHead;
            Punching(_targetPoint,PlayerLeftHand,_leftIdle, PivotHeadLeft.transform.position);
            _effectHead = true;
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            _targetPoint = _rightHead;
            Punching(_targetPoint, PlayerRightHand, _rightIdle,PivotHeadRight.transform.position);
            _effectHead = true;
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            _targetPoint = _leftBody;
            Punching(_targetPoint, PlayerLeftHand, _leftIdle,PivotBodyLeft.transform.position);
            _effectHead = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            _targetPoint = _rightBody;
            Punching(_targetPoint, PlayerRightHand, _rightIdle,PivotBodyRight.transform.position);
            _effectHead = false;
            return;
        }
    }
    
    private void Punching(Vector3 target, GameObject punch, Vector3 idle, Vector3 pivot)
    {
        if (_isPunching) return; // 중복 방지
        if(_isParrying) return;
        if (_isParryingDelay) return;
        _isPunching = true;
        _punchingType = punch.GetComponent<PlayerPunch>().PunchType;
        _breathSeq.Kill();

        punch.transform.position = pivot;

        _punchSeq = DOTween.Sequence();

        // 앞으로 이동 (0.05초)
        _punchSeq.Append(punch.transform.DOMove(target, 0.1f).SetEase(Ease.InExpo));
        _punchSeq.Join(punch.transform.DOScale(new Vector3(2f, 2f, 1f), 0.1f).SetEase(Ease.InExpo));
        _punchSeq.Join(Camera.main.DOOrthoSize(4.5f, 0.1f).SetEase(Ease.InExpo));
        if(punch.GetComponent<PlayerPunch>().PunchType == PunchType.Left)
        {
            _punchSeq.Join(Camera.main.transform.DORotate(new Vector3(0, 10, 0), 0.1f).SetEase(Ease.InExpo));
        }
        else
        {
            _punchSeq.Join(Camera.main.transform.DORotate(new Vector3(0, -10, 0), 0.1f).SetEase(Ease.InExpo));
        }
        _punchSeq.AppendInterval(0.05f);

        _punchSeq.AppendCallback(()=>CounterPunch(punch.GetComponent<PlayerPunch>().PunchType));

        // 짧은 대기
        _punchSeq.AppendInterval(0.05f);

        // 다시 뒤로 (0.1초)
        _punchSeq.Append(punch.transform.DOMove(idle, 0.2f).SetEase(Ease.OutExpo));
        _punchSeq.Join(punch.transform.DOScale(new Vector3(4f, 4f, 1f), 0.2f).SetEase(Ease.OutExpo));
        _punchSeq.Join(Camera.main.DOOrthoSize(5f, 0.2f).SetEase(Ease.OutExpo));
        _punchSeq.Join(Camera.main.transform.DORotate(new Vector3(0, 0, 0), 0.2f).SetEase(Ease.OutExpo));
        // 끝난 뒤에 flag 해제
        _punchSeq.AppendCallback(() => Breathing());
        _punchSeq.OnComplete(() => _isPunching = false);
    }

    public void CounterPunch(PunchType punchType)
    {
        GameObject vfx;
        if (!_isGaurding)
        {
            if (_effectHead)
            {
                vfx = Instantiate(HeadHitEffect[Random.Range(0, HeadHitEffect.Length)], _targetPoint, Quaternion.identity);
                ScoreManager.Instance.AddScore(250);
                audioSource.PlayOneShot(HeadSound[Random.Range(0, HeadSound.Length)]);
            }
            else
            {

                vfx = Instantiate(BodyHitEffect[Random.Range(0, BodyHitEffect.Length)], _targetPoint, Quaternion.identity);
                audioSource.PlayOneShot(BodySound[Random.Range(0, BodySound.Length)]);
                ScoreManager.Instance.AddScore(50);
            }

            ScoreManager.Instance.AddCombo();
            vfx.transform.localScale = new Vector3(4, 4, 1);
            Destroy(vfx, 1f);
            
            Enemy.Instance.animator.SetTrigger("Hit");

            Camera.main.DOShakePosition(0.1f, 1f, 60, 180, false);
            return;
            
        }

        _punchSeq.Pause();
        _health--;

        vfx = Instantiate(GaurdEffect[Random.Range(0, GaurdEffect.Length)], _targetPoint, Quaternion.identity);
        vfx.transform.localScale = new Vector3(6, 6, 1);
        audioSource.PlayOneShot(GaurdSound);
        Destroy(vfx, 1f);

        // 패널티 처리 (예: 넉백, 색 변화 등)
        Debug.Log("카운터 맞음! 패널티 적용");

        Enemy.Instance.CounterPunch(punchType);

        DOVirtual.DelayedCall(0.5f, () => UI_Game.Instance.UpdateHealth(_health));
        DOVirtual.DelayedCall(0.5f, () => UI_Game.Instance.GameOver(_health));
        DOVirtual.DelayedCall(0.5f, () => _punchSeq.Play());
    }

    public void SetGaurd(bool isGaurding)
    {
        _isGaurding = isGaurding;
    }

    private void Breathing()
    {
        float angle = -5 + _health * 4 / MaxHealth;
        float duration = 0.3f + _health * 0.6f / MaxHealth;
     _breathSeq = DOTween.Sequence()
    .Append(Camera.main.transform.DORotate(new Vector3(angle, 0, 0), duration).SetEase(Ease.OutQuad))
    .JoinCallback(()=> BreathSound.Instance.PlayBreathInhale(3-_health))
    .Append(Camera.main.transform.DORotate(new Vector3(0, 0, 0), duration).SetEase(Ease.OutQuad))
    .JoinCallback(() => BreathSound.Instance.PlayBreathExhale(3 - _health))
    .SetLoops(-1)
    .SetAutoKill(false)
    .Pause(); // 시작은 멈춘 상태

        _breathSeq.Play();
    }

    public void SetLight()
    {
        LightObject.GetComponent<Light2D>().color = new Color(1f, (float)_health / (float)MaxHealth, (float)_health / (float)MaxHealth);
    }

    private void IsParrying()
    {
        if (!_isPunching) return;
        if (_isParrying) return;
        if (_isParryingDelay) return;
        if(Time.time < _lastParryTime + ParryCoolDown) return;

        _lastParryTime = Time.time;
        _isParrying = true;
        _parryingSeq = DOTween.Sequence();
        GameObject gaurd;
        if (_punchingType == PunchType.Left)
            gaurd = PlayerRightHand;
        else
            gaurd = PlayerLeftHand;

        _parryingSeq.Append(gaurd.transform.DOMove(new Vector3(0, -1, 0), 0.15f).SetEase(Ease.OutQuad));
        _parryingSeq.Join(gaurd.transform.DOScale(new Vector3(6, 6, 1), 0.15f).SetEase(Ease.OutQuad));
        _parryingSeq.AppendInterval(0.35f);
        _parryingSeq.AppendCallback(() => _isParrying = false);
        _parryingSeq.AppendCallback(() => _isParryingDelay = true);

        Vector3 idle;
        if(_punchingType == PunchType.Left)
            idle = _rightIdle;
        else
            idle = _leftIdle;

        _parryingSeq.Append(gaurd.transform.DOMove(idle, 0.5f).SetEase(Ease.OutQuad));
        _parryingSeq.Join(gaurd.transform.DOScale(new Vector3(4, 4, 1), 0.5f).SetEase(Ease.OutQuad));
        _parryingSeq.OnComplete(() => _isParryingDelay = false);

    }

    public bool GetIsParrying()
    {
        return _isParrying;
    }

    public void SuccesParrying(bool success)
    {
        if(success)
            _health++;
        Debug.Log("패링 성공");
    }

    public void KillBreath()
    {
        Debug.Log("죽음");
        _breathSeq.Kill();
    }
}
