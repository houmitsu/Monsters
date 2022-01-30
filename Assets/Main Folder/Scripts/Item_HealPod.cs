using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//回復アイテムクラス
public class Item_HealPod : ItemBase
{
    [Header("Item Param")]
    //回復量
    [SerializeField] int healPoint = 10;

    protected override void Start()
    {
        base.Start();
    }

    //アイテム取得時処理
    protected override void ItemAction(Collider col)
    {
        base.ItemAction(col);
        var player = col.gameObject.GetComponent<PlayerController>();
        player.OnHeal(healPoint);
    }
}
