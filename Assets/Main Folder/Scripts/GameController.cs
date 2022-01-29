using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    //ゲームオーバーオブジェクト
    [SerializeField] GameObject gameOver = null;
    //プレイヤー
    [SerializeField] PlayerController player = null;
    //敵リスト
    [SerializeField] List<EnemyBase> enemys = new List<EnemyBase>();
    //敵の移動ターゲットリスト
    [SerializeField] List<Transform> enemyTargets = new List<Transform>();

    void Start()
    {
        player.GameOverEvent.AddListener(OnGameOver);
        gameOver.SetActive(false);

        foreach (var enemy in enemys)
        {
            enemy.ArrivalEvent.AddListener(EnemyMove);
        }
    }

    //リストからランダムにターゲットを取得
    Transform GetEnemyMoveTarget()
    {
        if (enemyTargets == null || enemyTargets.Count == 0) return null;
        else if (enemyTargets.Count == 1) return enemyTargets[0];

        int num = Random.Range(0, enemyTargets.Count);
        return enemyTargets[num];
    }

    //敵に次の目的地を設定
    void EnemyMove( EnemyBase enemy )
    {
        var target = GetEnemyMoveTarget();
        if( target != null ) enemy.SetNextTarget( target );
    }

    //ゲームオーバー時にプレイヤーから呼ばれる
    void OnGameOver()
    {
        //ゲームオーバーを表示
        gameOver.SetActive(true);
        //プレイヤーを非表示
        player.gameObject.SetActive(false);
        //敵の攻撃フラグを解除
        foreach(EnemyBase enemy in enemys) enemy.IsBattle = false;
    }

    //リトライボタンクリックコールバック
    public void OnRetryButtonClicked()
    {
        //プレイヤーリトライ処理
        player.Retry();
        //敵のリトライ処理
        foreach( EnemyBase enemy in enemys ) enemy.OnRetry();
        //プレイヤーを表示
        player.gameObject.SetActive( true );
        //ゲームオーバーを非表示
        gameOver.SetActive( false );
    }
}
