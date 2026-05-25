using UnityEngine;

public class GunData : MonoBehaviour
{
    //銃の名前
    [SerializeField] private string gunName;
    //弾数
    [SerializeField] private int ammoRounds;
    //反動
    [SerializeField] private float backlash;
    //集弾率
    [SerializeField] private float accuracy;
    //ダメージ
    [SerializeField] private int damage;
    //射撃レート
    [SerializeField] private int rpm;
    //射撃Pos
    [SerializeField] private Transform muzzlePoint;
    //射撃Pos
    [SerializeField] private Transform shellEjectPoint;

    public string GunName { get { return gunName; } }
    public int AmmoRounds { get { return ammoRounds; } }
    public float Backlash { get { return backlash; } }
    public float Accuracy { get { return accuracy; } }
    public int Damage { get { return damage; } }
    public int Rpm { get { return rpm; } }
    public Transform MuzzlePoint { get { return muzzlePoint; } }
    public Transform ShellEjectPoint { get { return shellEjectPoint; } }
    
}
