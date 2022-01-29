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

    // Start is called before the first frame update
    void Start()
    {
        player.GameOverEvent.AddListener(OnGameOver);
        gameOver.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
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
