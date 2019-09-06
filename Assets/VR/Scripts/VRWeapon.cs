using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game;

public class VRWeapon : VREquipment
{
    public float minVelocityMagnitudeForDamage = 1f;
    private void OnCollisionEnter(Collision collision)
    {
        if(Rigidbody.velocity.magnitude >= minVelocityMagnitudeForDamage && collision.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            Debug.Log("Hit enemy! Attempting damage.");
            Vector3 weaponToEnemyV = collision.collider.transform.position - transform.position;
            RaycastHit hit;
            if(Physics.Raycast(transform.position, weaponToEnemyV, out hit, weaponToEnemyV.magnitude * 2f, LayerMask.GetMask("Enemies")))
            {
                if (GameManager.Instance.WeaponManager.WeaponDamage(hit, collision.impulse.normalized))
                {
                    Debug.Log("Damaged enemy. Yay!");
                }
                else
                {
                    Debug.Log("Weapon manager failed to damage enemy :(");
                }
            }
            else
            {
                Debug.Log("Couldn't damage enemy. Raycast failed :(");
            }
        }
    }
}
