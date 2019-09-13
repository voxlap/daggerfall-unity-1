using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop;

public class VRWeapon : VREquipment
{
    public float minVelocityMagnitudeForDamage = 1f;
    public List<VRWeaponPiece> weaponPieces = new List<VRWeaponPiece>();
    public AudioClip noDamageSound;
    public ParticleSystem noDamageParticles;

    protected virtual void Start()
    {
        for (int i = 0; i < weaponPieces.Count; ++i)
            weaponPieces[i].Init(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryToHitThing(collision, transform);
    }

    public void TryToHitThing(Collision collision, Transform fromTF)
    {
        if (fromTF.GetComponent<Rigidbody>().velocity.magnitude >= minVelocityMagnitudeForDamage)
        {
            Debug.Log("Hit something with weapon! Attempting damage.");
            Vector3 weaponToEnemyV = collision.collider.transform.position - fromTF.position;
            RaycastHit hit;
            if (Physics.Raycast(fromTF.position, weaponToEnemyV, out hit, weaponToEnemyV.magnitude * 2f, 1 << collision.gameObject.layer))
            {
                //get all possible things we might be hitting
                DaggerfallEntityBehaviour entity = collision.gameObject.GetComponent<DaggerfallEntityBehaviour>();
                DaggerfallAction dagAction = collision.gameObject.GetComponent<DaggerfallAction>();
                DaggerfallActionDoor door = collision.gameObject.GetComponent<DaggerfallActionDoor>();
                int entityHealth = entity ? entity.Entity.CurrentHealth : 0;
                int actionActivatedCount = dagAction ? dagAction.activationCount : 0;
                bool willDoorBeBashed = door ? !door.IsOpen && !door.IsMagicallyHeld : false;

                if (GameManager.Instance.WeaponManager.WeaponDamage(hit, collision.impulse.normalized)) // we hit an enemy
                {
                    ActivateHitFeedback(hit);
                    if (entity && entity.Entity.CurrentHealth == entityHealth)
                        ActivateNoDamageFeedback(hit);
                    //Debug.Log("Damaged enemy. Yay!");
                }
                else // we hit something other than an enemy
                {
                    //activate VR feedback for bashing doors and activating things
                    if (willDoorBeBashed || dagAction && dagAction.activationCount > actionActivatedCount)
                        ActivateHitFeedback(hit);
                }
            }
            else
            {
                Debug.Log("Couldn't damage anything with " + gameObject.name + ". Raycast failed :(");
            }
        }
    }

    /// <summary>
    /// Activates haptic pulse, and whatever other VR-specific things that need to happen for the weapon when it hits things
    /// that are effected by weapon hits (regardless of if it does damage)
    /// </summary>
    /// <param name="hit"></param>
    protected virtual void ActivateHitFeedback(RaycastHit hit)
    {
        if (Interactable.attachedToHand != null)
            Interactable.attachedToHand.TriggerHapticPulse(1500);
    }

    /// <summary>
    /// Activates the "no damage" sound and/or particle effect at collision point
    /// </summary>
    protected virtual void ActivateNoDamageFeedback(RaycastHit hit)
    {
        if (noDamageParticles)
        {
            Instantiate(noDamageParticles, hit.point, Quaternion.identity);
        }
        if (noDamageSound)
        {
            AudioSource.PlayClipAtPoint(noDamageSound, hit.point);
        }
    }
}
