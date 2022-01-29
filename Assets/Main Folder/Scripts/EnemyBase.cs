using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBase : MonoBehaviour
{
    //ステータス
    [System.Serializable]
    public class Status
    {
        //HP
        public int Hp = 10;
        //攻撃力
        public int Power = 1;
    }

    //基本ステータス
    [SerializeField] Status DefaultStatus = new Status();
    //現在のステータス
    public Status CurrentStatus = new Status();
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
    [SerializeField] ColliderCallReceiver attackHitColliderCall = null;
    //開始時位置
    Vector3 startPosition = new Vector3();
    //開始時角度
    Quaternion startRotation = new Quaternion();
    //HPバーのスライダー
    [SerializeField] Slider hpBar = null;

    void Start()
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

    // Update is called once per frame
    void Update()
    {
        //攻撃できる状態の時
        if(IsBattle == true)
        {
            attackTimer += Time.deltaTime;
 
            if(attackTimer >= 3f)
            {
                animator.SetTrigger("isAttack");
                attackTimer = 0;
            }
        }
        else
        {
            attackTimer = 0;
        }
    }

    //攻撃時ヒットコール
    public void OnAttackHit(int damage, Vector3 attackPosition)
    {
        CurrentStatus.Hp -= damage;
        hpBar.value = CurrentStatus.Hp;
        Debug.Log("Hit Damage " + damage + "/CurrentHp = " + CurrentStatus.Hp);

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
        Debug.Log("死亡");
        animator.SetBool("isDie", true);
    }

    //死亡アニメーション終了コール
    void Anim_DieEnd()
    {
        this.gameObject.SetActive(false);
    }

    //周辺レーダーコライダーエンターイベントコール
    void OnAroundTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            IsBattle = true;
        }
    }

    //周辺レーダーコライダーステイイベントコール
    void OnAroundTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            var _dir = (other.gameObject.transform.position - this.transform.position).normalized;
            _dir.y = this.transform.position.y;
            this.transform.forward = _dir;
        }
    }

    //周辺レーダーコライダー終了イベントコール
    void OnAroundTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            IsBattle = false;
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
    void Anim_AttackHit()
    {
        attackHitColliderCall.gameObject.SetActive(true);
    }

    //攻撃アニメーション終了時コール
    void Anim_AttackEnd()
    {
        attackHitColliderCall.gameObject.SetActive( false );
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