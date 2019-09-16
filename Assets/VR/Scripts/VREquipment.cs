using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game.Items;
using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Throwable), typeof(InteractableHoverEvents))]
public class VREquipment : MonoBehaviour
{
    // daggerfall references
    protected DaggerfallUnityItem daggerfallItem;
    // unity references
    [Tooltip("The mesh renderer whose color gets shifted depending on the daggerfall item's material")]
    [SerializeField]
    protected MeshRenderer metalMeshRenderer;
    // properties
    public bool IsEquipped { get; private set; }
    public ulong UID { get { return daggerfallItem == null ? ulong.MaxValue : daggerfallItem.UID; } }

    public virtual Throwable Throwable { get; protected set; }
    public virtual Interactable Interactable { get; protected set; }
    public virtual VelocityEstimator VelocityEstimator { get; protected set; }
    public virtual Rigidbody Rigidbody { get; protected set; }
    public virtual InteractableHoverEvents InteractableHoverEvents { get; protected set; }
    private cakeslice.Outline[] outlines = new cakeslice.Outline[0];

    protected virtual void Awake()
    {
        //get components
        Throwable = GetComponent<Throwable>();
        Interactable = GetComponent<Interactable>();
        VelocityEstimator = GetComponent<VelocityEstimator>();
        Rigidbody = GetComponent<Rigidbody>();
        InteractableHoverEvents = GetComponent<InteractableHoverEvents>();
        outlines = gameObject.GetComponentsInChildren<cakeslice.Outline>();
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

        Debug.Log("Equipped " + daggerfallItem.LongName + " to " + handType.ToString());
    }
    public virtual void Unequip()
    {
        IsEquipped = false;
        ForceDetachFromHand();
        gameObject.SetActive(false);

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
