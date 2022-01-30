using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//EnemyBaseを継承した敵Turtle
public class Enemy_Turtle : EnemyBase
{
    [Header("Sub Param")]
    //遠距離攻撃コライダーコール
    [SerializeField] ColliderCallReceiver farAttackCall = null;
    //遠距離攻撃リジッドボディ
    [SerializeField] Rigidbody farAttackRigid = null;

    //近距離フラグ
    bool isNear = true;
    //遠距離攻撃コルーチン
    Coroutine farAttackCor = null;

    protected override void Start()
    {
        base.Start();

        farAttackCall.TriggerEnterEvent.AddListener(OnFarAttackEnter);
    }

    protected override void Update()
    {
        base.Update();
    }

    //周辺レーダーコライダーステイイベントコール
    protected override void OnAroundTriggerStay(Collider other)
    {
        base.OnAroundTriggerStay(other);

        if (other.gameObject.tag == "Player")
        {
            var sqrMag = (this.transform.position - other.gameObject.transform.position).sqrMagnitude;
            if (sqrMag > 3f) isNear = false;　//値は適宜調整
            else isNear = true;

        }
    }

    //攻撃Hitアニメーションコール
    protected override void Anim_AttackHit()
    {
        if (isNear == true) attackHitColliderCall.gameObject.SetActive(true);
        else
        {
            farAttackCall.gameObject.SetActive(true);

            var dir = (currentAttackTarget.position - this.transform.position);
            dir.y = 0;
            farAttackRigid.AddForce(dir * 100f); //100は適宜調整

            if (farAttackCor != null)
            {
                StopCoroutine(farAttackCor);
                farAttackCor = null;
                ResetFarAttack();
            }
            farAttackCor = StartCoroutine(AutoErase());
        }
    }

    //攻撃アニメーション終了時コール
    protected override void Anim_AttackEnd()
    {
        base.Anim_AttackEnd();
    }

    //遠距離攻撃コライダーエンターコール
    void OnFarAttackEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (farAttackCor != null)
            {
                StopCoroutine(farAttackCor);
                farAttackCor = null;
                ResetFarAttack();
            }

            var player = col.GetComponent<PlayerController>();
            player?.OnEnemyAttackHit(CurrentStatus.Power, this.transform.position);
            attackHitColliderCall.gameObject.SetActive(false);
        }
    }

    //時間経過で自動削除
    IEnumerator AutoErase()
    {
        yield return new WaitForSeconds(1f);
        ResetFarAttack();
    }

    //遠距離攻撃をリセット
    void ResetFarAttack()
    {
        farAttackCall.gameObject.SetActive(false);
        farAttackRigid.velocity = Vector3.zero;
        farAttackRigid.angularVelocity = Vector3.zero;
        var reset = Vector3.zero;
        reset.y = 0.4f;
        reset.z = 1f;
        farAttackCall.gameObject.transform.localPosition = reset;
    }
}
