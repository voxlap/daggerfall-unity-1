using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game;
using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Throwable), typeof(InteractableHoverEvents))]
public class VREquipment : MonoBehaviour
{
    // daggerfall references
    protected DaggerfallUnityItem daggerfallItem;
    // unity references
    [SerializeField]
    [Tooltip("The mesh renderer whose color gets shifted depending on the daggerfall item's material")]
    protected MeshRenderer metalMeshRenderer;
    [SerializeField]
    [Tooltip("Colliders that make metallic sounds on hit")]
    protected List<Collider> metalColliders = new List<Collider>();
    [SerializeField]
    [Tooltip("Colliders that make wood sounds on hit")]
    protected List<Collider> woodColliders = new List<Collider>();

    // properties
    public bool IsEquipped { get; private set; }
    public ulong UID { get { return daggerfallItem == null ? ulong.MaxValue : daggerfallItem.UID; } }
    public EquipSlots EquipSlot { get { return daggerfallItem == null ? EquipSlots.None : daggerfallItem.EquipSlot; } }

    public virtual Throwable Throwable { get; protected set; }
    public virtual Interactable Interactable { get; protected set; }
    public virtual VelocityEstimator VelocityEstimator { get; protected set; }
    public virtual Rigidbody Rigidbody { get; protected set; }
    public virtual InteractableHoverEvents InteractableHoverEvents { get; protected set; }
    public virtual SteamVR_Skeleton_Poser SkeletonPoser { get; protected set; }
    public virtual DaggerfallUnityItem DaggerfallItem { get { return daggerfallItem; } }
    public virtual MeshRenderer MetalMeshRenderer { get { return metalMeshRenderer; } }
    private cakeslice.Outline[] outlines = new cakeslice.Outline[0];

    //consts
    public const float MIN_VELOCITY_FOR_HIT_SOUND = 1f; // m/s
    public const float MAX_VELOCITY_FOR_HIT_SOUND = 200f; // m/s
    public const float METAL_VOLUME_MODIFIER = 0.2f;

    //events
    public event Action Equipped;
    public event Action Unequipped;

    protected virtual void Awake()
    {
        //get components
        Throwable = GetComponent<Throwable>();
        Interactable = GetComponent<Interactable>();
        VelocityEstimator = GetComponent<VelocityEstimator>();
        Rigidbody = GetComponent<Rigidbody>();
        InteractableHoverEvents = GetComponent<InteractableHoverEvents>();
        SkeletonPoser = GetComponent<SteamVR_Skeleton_Poser>();
        outlines = gameObject.GetComponentsInChildren<cakeslice.Outline>(true);
        UnHighlight();
        //add event listeners
        Throwable.onPickUp.AddListener(OnPickup);
        Throwable.onDetachFromHand.AddListener(OnThrow);
        InteractableHoverEvents.onHandHoverBegin.AddListener(OnHoverHandBegins);
        InteractableHoverEvents.onHandHoverEnd.AddListener(OnHoverHandEnds);
    }

    protected virtual void Reset()
    {
        //setup local fields
        metalMeshRenderer = GetComponentInChildren<MeshRenderer>();
    }
    protected virtual void OnCollisionEnter(Collision collision)
    {
        PlayHitSoundAtCollision(collision);
    }
    private void PlayHitSoundAtCollision(Collision collision)
    {
        float volume = GetVolumeForHit(collision.impulse.magnitude / Time.fixedDeltaTime); //modulate volume of hit by the collision impulse magnitude
        if (volume <= 0)
            return;

        //We need to find out if a wooden part or metal part hit, then play the appropriate sound

        Vector3 contactPos = Rigidbody.ClosestPointOnBounds(collision.contacts[0].point);
        float closestWoodDistance = 100000f, closestMetalDistance = 100000f;
        
        if (metalColliders.Count == 0) //wood must be closest if it's only made of wood
            closestWoodDistance = 0;
        else if (woodColliders.Count == 0) //metal must be closest if it's only made of metal
            closestMetalDistance = 0;
        else
        {       // we need to find out if it's wood or metal based on which is closest to the contact position
            for (int i = 0; i < woodColliders.Count; ++i)
            {
                float dist = Vector3.Distance(contactPos, woodColliders[i].ClosestPointOnBounds(contactPos));
                if (dist < closestWoodDistance)
                    closestWoodDistance = dist;
            }
            for (int i = 0; i < metalColliders.Count; ++i)
            {
                float dist = Vector3.Distance(contactPos, metalColliders[i].ClosestPointOnBounds(contactPos));
                if (dist < closestMetalDistance)
                    closestMetalDistance = dist;
            }
        }
        //play random clip of the appropriate material
        if (closestWoodDistance < closestMetalDistance)
            VREquipmentManager.Instance.PlayRandomWoodHitSound(contactPos, volume);
        else
            VREquipmentManager.Instance.PlayRandomMetalHitSound(contactPos, volume);
    }
    public static float GetVolumeForHit(float velocityMagnitudeAtHit)
    {
        return Mathf.InverseLerp(MIN_VELOCITY_FOR_HIT_SOUND, MAX_VELOCITY_FOR_HIT_SOUND, velocityMagnitudeAtHit);
    }

#if UNITY_EDITOR
    [ContextMenu("Create Attachment Offset")]
    public void CreateAttachmentOffset()
    {
        //setup throwable component
        Throwable = GetComponent<Throwable>();
        Throwable.attachmentOffset = transform.Find("AttachmentOffset");
        if (!Throwable.attachmentOffset)
            Throwable.attachmentOffset = (new GameObject("AttachmentOffset")).transform;
        Throwable.attachmentOffset.parent = transform;
        Throwable.attachmentOffset.localPosition = Vector3.zero;
        Throwable.attachmentOffset.localRotation = Quaternion.identity;
    }
#endif

    public virtual void Highlight()
    {
        for (int i = 0; i < outlines.Length; ++i)
            outlines[i].enabled = true;
    }
    public virtual void UnHighlight()
    {
        for (int i = 0; i < outlines.Length; ++i)
            outlines[i].enabled = false;
    }
    
    public virtual void Equip(DaggerfallUnityItem daggerfallItem)
    {
        gameObject.SetActive(true);
        this.daggerfallItem = daggerfallItem;
        metalMeshRenderer.material.color = VREquipmentManager.Instance.GetDaggerfallItemMaterialColor(daggerfallItem);
        SteamVR_Input_Sources handType = daggerfallItem.EquipSlot == EquipSlots.RightHand ? SteamVR_Input_Sources.RightHand : SteamVR_Input_Sources.LeftHand;
        ForceAttachToHand(handType);
        IsEquipped = true;

        if (Equipped != null)
            Equipped();

        Debug.Log("Equipped " + daggerfallItem.LongName + " to " + handType.ToString());
    }
    public virtual void Unequip()
    {
        IsEquipped = false;
        ForceDetachFromHand();
        transform.parent = VREquipmentManager.Instance.equipmentParent;
        gameObject.SetActive(false);

        if (Unequipped != null)
            Unequipped();

        Debug.Log("Unequipped " + daggerfallItem.LongName);
    }
    public virtual void ForceAttachToHand(SteamVR_Input_Sources handType)
    {
        Hand handToAttach = handType == SteamVR_Input_Sources.LeftHand ? Player.instance.leftHand : Player.instance.rightHand;
        // attach to hand immediately if hand is gripping and game is not paused
        if(handToAttach.grabGripAction.GetState(handToAttach.handType) && !DaggerfallWorkshop.Game.GameManager.IsGamePaused)
            handToAttach.AttachObject(gameObject, GrabTypes.Grip, Throwable.attachmentFlags, Throwable.attachmentOffset);
        else //else, float in front of player at hand position
            VREquipmentManager.Instance.FloatItemInFrontOfPlayer(this, handType);
    }
    public virtual void ForceDetachFromHand()
    {
        if (Interactable.attachedToHand)
            Interactable.attachedToHand.DetachObject(gameObject);
    }

    protected virtual void OnPickup()
    {
        //if throwable isn't parenting to hand, set parent to vr player origin
        if ((Throwable.attachmentFlags & Hand.AttachmentFlags.ParentToHand) == 0)
            transform.SetParent(Player.instance.trackingOriginTransform);
        //if throwable isn't turning on kinematic, turn off kinematic
        if ((Throwable.attachmentFlags & Hand.AttachmentFlags.TurnOnKinematic) == 0)
            Rigidbody.isKinematic = false;
    }
    protected virtual void OnThrow()
    {
        transform.SetParent(VREquipmentManager.Instance.equipmentParent);
        Rigidbody.isKinematic = false;
        Rigidbody.AddForce(GameManager.Instance.PlayerController.velocity, ForceMode.VelocityChange);
    }
    protected virtual void OnHoverHandBegins()
    {
        Interactable.hoveringHand.TriggerHapticPulse(500);
        Highlight();
    }
    protected virtual void OnHoverHandEnds()
    {
        UnHighlight();
    }
}
