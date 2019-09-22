using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;

[RequireComponent(typeof(Camera))] // script requires a camera to do OnPreRender, which is needed for up-to-date VR controller transform data
public class Flail : MonoBehaviour
{
    public Transform chain;
    public ConfigurableJoint spikeBall;

    private SteamVR_TrackedObject trackedObj;
    private Rigidbody spikeRB;
    private MeshRenderer spikeMeshRenderer;
    private MeshRenderer[] chainlinks = new MeshRenderer[0];
    private VRWeapon daggerfallVRWeapon;
    private Interactable interactable;

    private Vector3 startSpikeballPos;
    private Vector3 startChainPos;
    private Vector3 spikeballAnchor;
    private int lastNumEnabledChainlinks = 0;

    private bool _init;

    private void Awake()
    {
        startSpikeballPos = spikeBall.transform.localPosition;
        startChainPos = chain.transform.localPosition;
        spikeballAnchor = spikeBall.connectedAnchor;
        spikeRB = spikeBall.GetComponent<Rigidbody>();
        spikeMeshRenderer = spikeBall.GetComponent<MeshRenderer>();
        interactable = GetComponent<Interactable>();
        daggerfallVRWeapon = GetComponent<VRWeapon>();
        
        interactable.onAttachedToHand += Interactable_onAttachedToHand;
        interactable.onDetachedFromHand += Interactable_onDetachedFromHand;
        if (interactable.attachedToHand != null)
            Interactable_onAttachedToHand(interactable.attachedToHand);
    }

    private void Start()
    {
        //Get chainlinks. Set their colors.
        Color metalColor = daggerfallVRWeapon.MetalMeshRenderer.material.color;
        chainlinks = new MeshRenderer[chain.childCount];
        for (int i = 0; i < chainlinks.Length; ++i)
        {
            chainlinks[i] = chain.GetChild(i).GetComponent<MeshRenderer>();
            chainlinks[i].material.color = metalColor;
        }
        spikeMeshRenderer.material.color = metalColor;

        //subscribe to equipped event
        daggerfallVRWeapon.Equipped += DaggerfallVRWeapon_Equipped;

        //initialize finished
        _init = true;
    }
    private void OnDestroy()
    {
        if (daggerfallVRWeapon)
            daggerfallVRWeapon.Equipped -= DaggerfallVRWeapon_Equipped;
    }

    private void OnEnable()
    {
        spikeBall.gameObject.SetActive(true);
        ResetBallPosition();

    }
    private void OnDisable()
    {
        if(spikeBall)
            spikeBall.gameObject.SetActive(false);
    }
    // OnPreRender is needed for up-to-date VR controller transform data
    private void OnPreRender()
    {
        RotateChain();
        LengthenChain();
    }

    private Vector3 lastControllerPosition = Vector3.zero;
    /// <summary>
    /// Rotates the flail's chain toward the spike ball
    /// </summary>
    private void RotateChain()
    {
        chain.rotation = Quaternion.LookRotation(spikeBall.transform.position - chain.position, spikeBall.transform.up);
    }
    /// <summary>
    /// Lengthens or contracts the chain so that it reaches the spike ball
    /// </summary>
    private void LengthenChain()
    {
        int numEnabledChainlinks = Mathf.FloorToInt(Vector3.Distance(spikeBall.transform.position, chain.position) / 0.015f);
        if(numEnabledChainlinks != lastNumEnabledChainlinks)
        {
            for (int i = 0; i < chainlinks.Length; ++i)
                chainlinks[i].gameObject.SetActive(i < numEnabledChainlinks);
        }
        lastNumEnabledChainlinks = numEnabledChainlinks;
    }

    private void ResetBallPosition()
    {
        spikeBall.connectedAnchor = spikeballAnchor;
        spikeBall.transform.position = transform.TransformPoint(startSpikeballPos);
    }

    private void Interactable_onAttachedToHand(Hand hand)
    {
        //set spike ball's parent to the parent of the hand. We want it rotated/translated with player, not with the hand itself
        spikeRB.transform.SetParent(hand.transform.parent);
        ResetBallPosition();
    }

    private void Interactable_onDetachedFromHand(Hand hand)
    {
        if (spikeRB)
            spikeRB.transform.SetParent(null);
    }

    private void DaggerfallVRWeapon_Equipped()
    {
        //Set chainlink colors
        Color metalColor = daggerfallVRWeapon.MetalMeshRenderer.material.color;
        for (int i = 0; i < chainlinks.Length; ++i)
            chainlinks[i].material.color = metalColor;
        spikeMeshRenderer.material.color = metalColor;
    }

}
