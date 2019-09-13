using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game.Items;
using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Throwable))]
public class VREquipment : MonoBehaviour
{
    // daggerfall references
    protected DaggerfallUnityItem daggerfallItem;
    // unity references
    [Tooltip("The mesh renderer whose color gets shifted depending on the daggerfall item's material")]
    [SerializeField]
    protected MeshRenderer metalMeshRenderer;
    protected Throwable throwable;
    protected Interactable interactable;
    protected VelocityEstimator velocityEstimator;
    protected Rigidbody rb;
    // properties
    public bool IsEquipped { get; private set; }
    public ulong UID { get { return daggerfallItem == null ? ulong.MaxValue : daggerfallItem.UID; } }
    public virtual Throwable Throwable { get { return throwable; } }
    public virtual Interactable Interactable { get { return interactable; } }
    public virtual VelocityEstimator VelocityEstimator { get { return velocityEstimator; } }
    public virtual Rigidbody Rigidbody { get { return rb; } }

    protected virtual void Awake()
    {
        throwable = GetComponent<Throwable>();
        interactable = GetComponent<Interactable>();
        velocityEstimator = GetComponent<VelocityEstimator>();
        rb = GetComponent<Rigidbody>();
        throwable.onDetachFromHand.AddListener(delegate { rb.isKinematic = false; });
    }
    
    public virtual void Equip(DaggerfallUnityItem daggerfallItem)
    {
        gameObject.SetActive(true);
        this.daggerfallItem = daggerfallItem;
        metalMeshRenderer.material.color = VREquipmentManager.Instance.GetDaggerfallItemMaterialColor(daggerfallItem);
        SteamVR_Input_Sources handType = daggerfallItem.EquipSlot == EquipSlots.RightHand ? SteamVR_Input_Sources.RightHand : SteamVR_Input_Sources.LeftHand;
        ForceAttachToHand(handType); //TODO: Float in front of player instead of forcefully attaching
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
        //transform.position = handToAttach.transform.position;
        if(handToAttach.grabGripAction.GetState(handToAttach.handType))
            handToAttach.AttachObject(gameObject, GrabTypes.Grip, throwable.attachmentFlags, throwable.attachmentOffset);
    }
    public virtual void ForceDetachFromHand()
    {
        if (interactable.attachedToHand)
            interactable.attachedToHand.DetachObject(gameObject);
    }
}
