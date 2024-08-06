using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponEquipping : MonoBehaviour
{
    public Player playerMovement;
    public void Equipped()
    {
        playerMovement.Equipped();
    }
    public void ActivateWeapon()
    {
        playerMovement.ActivateWeapon();
    }
}
