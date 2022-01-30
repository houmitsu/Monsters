using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    //ステータス
    [System.Serializable]
    public class Status
    {
        //HP
        public int Hp = 5;
        //攻撃力
        public int Power = 1;
    }

    //基本ステータス
    [SerializeField] Status DefaultStatus = new Status();
    //現在のステータス
    public Status CurrentStatus = new Status();

    [SerializeField] public bool IsBoss = false;

    //アニメーター
    Animator animator = null;

    //周辺レーダーコライダーコール
    [SerializeField] ColliderCallReceiver aroundColliderCall = null;
    //自身のコライダー
    [SerializeField] Collider myCollider = null;
    //攻撃ヒット時エフェクトプレハブ
    [SerializeField] GameObject hitParticlePrefab = null;
    //攻撃間隔
    [SerializeField] float attackInterval = 3f;
    //攻撃状態フラグ
    public bool IsBattle = false;
    //攻撃時間計測用
    float attackTimer = 0f;
    //攻撃判定用コライダーコール
    [SerializeField] protected ColliderCallReceiver attackHitColliderCall = null;
    //現在の攻撃ターゲット
    protected Transform currentAttackTarget = null;
    //開始時位置
    Vector3 startPosition = new Vector3();
    //開始時角度
    Quaternion startRotation = new Quaternion();
    //HPバーのスライダー
    [SerializeField] Slider hpBar = null;

    //敵の移動イベント定義クラス
    public class EnemyMoveEvent : UnityEvent<EnemyBase> { }
    //目的地設定イベント
    public EnemyMoveEvent ArrivalEvent = new EnemyMoveEvent();

    //ナビメッシュ
    NavMeshAgent navMeshAgent = null;

    //現在設定されている目的地
    Transform currentTarget = null;

    //死亡時イベント
    public EnemyMoveEvent DestroyEvent = new EnemyMoveEvent();

    protected virtual void Start()
    {
        //Animatorを取得し保管
        animator = GetComponent<Animator>();
        //最初に現在のステータスを基本ステータスとして設定
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        //周辺コライダーイベント登録
        aroundColliderCall.TriggerEnterEvent.AddListener(OnAroundTriggerEnter);
        aroundColliderCall.TriggerStayEvent.AddListener(OnAroundTriggerStay);
        aroundColliderCall.TriggerExitEvent.AddListener(OnAroundTriggerExit);
        //攻撃コライダーイベント登録
        attackHitColliderCall.TriggerEnterEvent.AddListener(OnAttackTriggerEnter);
        attackHitColliderCall.gameObject.SetActive(false);
        //開始時の位置回転を保管
        startPosition = this.transform.position;
        startRotation = this.transform.rotation;
        //スライダーを初期化
        hpBar.maxValue = DefaultStatus.Hp;
        hpBar.value = CurrentStatus.Hp;
    }

    protected virtual void Update()
    {
        //攻撃できる状態の時
        if(IsBattle == true)
        {
            attackTimer += Time.deltaTime;

            animator.SetBool("isRun", false);

            if (attackTimer >= 3f)
            {
                animator.SetTrigger("isAttack");
                attackTimer = 0;
            }
        }
        else
        {
            attackTimer = 0;

            //ターゲットまでの距離を測定し、イベントを実行
            if (currentTarget == null)
            {
                animator.SetBool("isRun", false);

                ArrivalEvent?.Invoke(this);
            }
            else
            {
                animator.SetBool("isRun", true);

                var sqrDistance = (currentTarget.position - this.transform.position).sqrMagnitude;
                if (sqrDistance < 3f)
                {
                    ArrivalEvent?.Invoke(this);
                }
            }
        }
    }

    //ナビメッシュの次の目的地を設定
    public void SetNextTarget(Transform target)
    {
        if (target == null) return;
        if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.SetDestination(target.position);
        currentTarget = target;
    }

    //攻撃時ヒットコール
    public void OnAttackHit(int damage, Vector3 attackPosition)
    {
        CurrentStatus.Hp -= damage;
        hpBar.value = CurrentStatus.Hp;

        var pos = myCollider.ClosestPoint( attackPosition );
        var obj = Instantiate( hitParticlePrefab, pos, Quaternion.identity );
        var par = obj.GetComponent<ParticleSystem>();
        StartCoroutine( WaitDestroy( par ) );
 
        if(CurrentStatus.Hp <= 0)
        {
            OnDie();
        }
        else
        {
            animator.SetTrigger("isHit");
        }
    }

    //パーティクルが終了したら破棄する
    IEnumerator WaitDestroy(ParticleSystem particle)
    {
        yield return new WaitUntil(() => particle.isPlaying == false);
        Destroy(particle.gameObject);
    }

    //死亡時コール
    void OnDie()
    {
        animator.SetBool("isDie", true);
    }

    //死亡アニメーション終了コール
    void Anim_DieEnd()
    {
        //Destroy(gameObject);
        DestroyEvent?.Invoke(this);
    }

    //周辺レーダーコライダーエンターイベントコール
    void OnAroundTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            IsBattle = true;

            navMeshAgent.SetDestination(this.transform.position);
            currentTarget = null;
        }
    }

    //周辺レーダーコライダーステイイベントコール
    protected virtual void OnAroundTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            var _dir = (other.gameObject.transform.position - this.transform.position).normalized;
            _dir.y = this.transform.position.y;
            this.transform.forward = _dir;

            currentAttackTarget = other.gameObject.transform;
        }
    }

    //周辺レーダーコライダー終了イベントコール
    protected virtual void OnAroundTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            IsBattle = false;
            currentAttackTarget = null;
        }
    }

    //攻撃コライダーエンターイベントコール
    void OnAttackTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            var player = other.GetComponent<PlayerController>();
            player?.OnEnemyAttackHit(CurrentStatus.Power, this.transform.position);
            attackHitColliderCall.gameObject.SetActive(false);
        }
    }

    //攻撃Hitアニメーションコール
    protected virtual void Anim_AttackHit()
    {
        attackHitColliderCall.gameObject.SetActive(true);
    }

    //攻撃アニメーション終了時コール
    protected virtual void Anim_AttackEnd()
    {
        attackHitColliderCall.gameObject.SetActive(false);
    }

    //プレイヤーリトライ時の処理
    public void OnRetry()
    {
        // 現在のステータスを基本ステータスとして設定.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;

        hpBar.value = CurrentStatus.Hp;

        // 開始時の位置回転を保管.
        this.transform.position = startPosition;
        this.transform.rotation = startRotation;
 
        //敵を再度表示
        this.gameObject.SetActive(true);
    }
}