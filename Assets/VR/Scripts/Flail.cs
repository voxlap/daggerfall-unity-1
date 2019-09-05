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
    public Rigidbody handle;

    private SteamVR_TrackedObject trackedObj;
    private Rigidbody spikeRB;
    private Vector3 startSpikeballPos;
    private Vector3 startChainPos;
    private Vector3 spikeballAnchor;
    private GameObject[] chainlinks = new GameObject[0];
    private int lastNumEnabledChainlinks = 0;

    private void Awake()
    {
        startSpikeballPos = spikeBall.transform.localPosition;
        startChainPos = chain.transform.localPosition;
        spikeballAnchor = spikeBall.connectedAnchor;
        spikeRB = spikeBall.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        chainlinks = new GameObject[chain.childCount];
        for (int i = 0; i < chainlinks.Length; ++i)
            chainlinks[i] = chain.GetChild(i).gameObject;
        //Valve.VR.InteractionSystem.Player.instance.rightHand.AttachObject(gameObject, Valve.VR.InteractionSystem.GrabTypes.None);
        spikeBall.transform.parent = null;
    }

    private void OnEnable()
    {
        spikeBall.gameObject.SetActive(true);
        spikeBall.connectedAnchor = spikeballAnchor;
        spikeBall.transform.position = transform.TransformPoint(startSpikeballPos);
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
                chainlinks[i].SetActive(i < numEnabledChainlinks);
        }
        lastNumEnabledChainlinks = numEnabledChainlinks;
    }
}
