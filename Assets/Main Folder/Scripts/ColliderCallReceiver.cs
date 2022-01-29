using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


//コライダーコールバックの受信ユーティリティクラス
public class ColliderCallReceiver : MonoBehaviour
{
    //トリガーイベント定義クラス
    public class TriggerEvent : UnityEvent<Collider>{}
    //トリガーエンターイベント
    public TriggerEvent TriggerEnterEvent = new TriggerEvent();
    //トリガーステイイベント
    public TriggerEvent TriggerStayEvent = new TriggerEvent();
    //トリガーイグジットイベント
    public TriggerEvent TriggerExitEvent = new TriggerEvent();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    //トリガーエンターコールバック
    void OnTriggerEnter(Collider other)
    {
        TriggerEnterEvent?.Invoke(other);
    }
 
    //トリガーステイコールバック
    void OnTriggerStay(Collider other)
    {
        TriggerStayEvent?.Invoke(other);
    }
 
    //トリガーイグジットコールバック
    void OnTriggerExit(Collider other)
    {
        TriggerExitEvent?.Invoke(other);
    }
}
