using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop;
using Valve.VR.InteractionSystem;

public class VRWeapon : VREquipment
{
    public float minVelocityMagnitudeForDamage = 1f;
    [Tooltip("Auxiliary pieces of a weapon disconnected from this rigidbody, i.e. the Flail's spike ball")]
    public List<VRWeaponPiece> weaponPieces = new List<VRWeaponPiece>();
    public AudioClip noDamageSound;
    public ParticleSystem noDamageParticles;

    private int lastFrameHitAThing = 0;

    protected Vector3 lastVelocity;
    protected Vector3 lastAngularVelocity;

    protected virtual void Start()
    {
        for (int i = 0; i < weaponPieces.Count; ++i)
            weaponPieces[i].Init(this);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        TryToHitThing(collision, Rigidbody, lastVelocity, lastAngularVelocity);
    }
    protected virtual void FixedUpdate()
    {
        lastVelocity = Rigidbody.velocity;
        lastAngularVelocity = Rigidbody.angularVelocity;
    }
    private void SetScreenWeaponState(Vector3 vel, Vector3 angularVel, Vector3 point)
    {
        Vector3 point2 = point + vel + Quaternion.Euler(angularVel) * point;
        Vector3 dir = point2 - point;
        dir = GameManager.Instance.MainCamera.transform.InverseTransformVector(dir);
        dir.z = 0;
        dir = dir.normalized;

        WeaponStates state;
        if (dir.y < -0.96f)
            state = WeaponStates.StrikeDown;
        else if (dir.y < -0.383f)
        {
            if (dir.x > 0)
                state = WeaponStates.StrikeDownRight;
            else
                state = WeaponStates.StrikeDownLeft;
        }
        else if (dir.y > .383f)
            state = WeaponStates.StrikeUp;
        else if (dir.x > 0)
            state = WeaponStates.StrikeRight;
        else
            state = WeaponStates.StrikeLeft;

        Debug.Log("Striking with vr weapon in direction " + dir.ToString("0.00") + " which translates to: " + state.ToString());
        GameManager.Instance.WeaponManager.ScreenWeapon.ChangeWeaponState(state);
    }

    public void TryToHitThing(Collision collision, Rigidbody rb, Vector3 lastVelocity, Vector3 lastAngularVelocity)
    {
        if (Time.frameCount - lastFrameHitAThing < 3)
            return;
        if (lastVelocity.magnitude >= minVelocityMagnitudeForDamage)
        {
            Debug.Log("Hit " + collision.collider.gameObject.name + " with weapon! Attempting damage.");
            RaycastHit hit;
            Vector3 colPoint = collision.contacts[0].point;
            Vector3 direction = -collision.contacts[0].normal;
            Vector3 origin = colPoint + direction * VREquipmentManager.Instance.OriginOffset;
            float distance = VREquipmentManager.Instance.Distance;

            VREquipmentManager.Instance.DebugRaycastOriginAndDirection(origin, direction, distance);

            if (Physics.Raycast(origin, direction, out hit, distance, 1 << collision.gameObject.layer))
            {
                if(hit.transform != collision.transform)
                {
                    Debug.Log("Hit " + hit.transform.name + " with raycast when was expecting to hit " + collision.transform.name);
                    return;
                }
                //get all possible things we might be hitting
                DaggerfallEntityBehaviour entity = collision.gameObject.GetComponent<DaggerfallEntityBehaviour>();
                DaggerfallAction dagAction = collision.gameObject.GetComponent<DaggerfallAction>();
                DaggerfallActionDoor door = collision.gameObject.GetComponent<DaggerfallActionDoor>();
                int entityHealth = entity ? entity.Entity.CurrentHealth : 0;
                int actionActivatedCount = dagAction ? dagAction.activationCount : 0;
                bool willDoorBeBashed = door ? !door.IsOpen && !door.IsMagicallyHeld : false;

                if (entity || dagAction || door)
                {
                    //set screen animation
                    lastFrameHitAThing = Time.frameCount;
                    SetScreenWeaponState(lastVelocity, lastAngularVelocity, collision.contacts[0].point - rb.position);

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
                        ActivateNoDamageFeedback(hit);
                    }
                }
            }
            else
            {
                Debug.Log("Couldn't damage " + collision.transform.name + " with " + gameObject.name + ". Raycast failed.");
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
