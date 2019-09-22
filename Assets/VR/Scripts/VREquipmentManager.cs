using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Serialization;
using Valve.VR.InteractionSystem;

public class VREquipmentManager : MonoBehaviour
{
    public string resourcesPath = "VR/Equipment/";
    public string backupResourcesPath = "VR/EquipmentBackup/"; //used if there are missing assets in VR/Equipment.
    public Transform equipmentParent;
    public Transform leftHandEquippedItemParent;
    public Transform rightHandEquippedItemParent;

    [Header("Debug")]
    public bool shouldDebugRaycasts = false;
    public Transform debugOrigin;
    public Transform debugDirection;
    private Color originalOriginColor, originalDirectionColor;

    [Header("Raycast params")]
    public float Distance = 1f;
    public float OriginOffset = -.2f;

    [Header("Item material colors")]
    public Color leatherColor = (Color.green + Color.yellow + Color.red) / 3f;
    public Color chainColor = Color.grey;
    public Color ironColor = Color.gray;
    public Color steelColor = Color.grey;
    public Color silverColor = Color.white;
    public Color elvenColor = Color.white;
    public Color dwarvenColor = (Color.red + Color.yellow) / 2f;
    public Color mithrilColor = Color.cyan;
    public Color adamantiumColor = Color.green;
    public Color ebonyColor = (Color.black + Color.grey * 2f) / 3f;
    public Color orcishColor = (Color.red + Color.yellow + Color.grey) / 3f;
    public Color daedricColor = (Color.red + Color.black) / 2f;

    [Header("Sounds")]
    public List<AudioClip> woodHitSounds = new List<AudioClip>();
    public List<AudioClip> metalHitSounds = new List<AudioClip>();

    //properties
    private ItemEquipTable itemEquipTable { get { return GameManager.Instance.PlayerEntity.ItemEquipTable; } }
    
    /// <summary> Dictionary of instantiated weapons. It's a list of equipment because some weapons can be dual weilded, so two will be spawned.
    /// The key is the item's template index. </summary>
    private Dictionary<int, List<VREquipment>> instantiatedEquipment = new Dictionary<int, List<VREquipment>>();

    private List<VREquipment> currentlyEquippedItems = new List<VREquipment>();
    private Coroutine equipItemsCoroutine;

    const Weapons k_weaponStart = Weapons.Dagger;
    const Weapons k_weaponEnd = Weapons.Long_Bow;
    const Armor k_armorStart = Armor.Buckler;
    const Armor k_armorEnd = Armor.Tower_Shield;

    #region Singleton

    public static VREquipmentManager Instance { get; private set; }
    private void SetupSingleton()
    {
        if (!Instance)
            Instance = this;
        else
        {
            Debug.LogError("Second DaggerfallVRPlayer singleton has been spawned in the scene. This obviously shouldn't happen.");
        }
    }

    #endregion

    private void Awake()
    {
        SetupSingleton();
    }
    private void Start()
    {
        originalOriginColor = debugOrigin.GetComponentInChildren<MeshRenderer>().material.color;
        originalDirectionColor = debugDirection.GetComponentInChildren<MeshRenderer>().material.color;
        debugOrigin.gameObject.SetActive(false);
        debugDirection.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        ItemEquipTable.OnItemEquipped -= ItemEquipTable_OnItemEquipped;
        ItemEquipTable.OnItemUnequipped -= ItemEquipTable_OnItemUnequipped;
        SaveLoadManager.OnLoad -= SaveLoadManager_OnLoad;
    }
    public void Init()
    {
        //attach floating item parents to VR player
        leftHandEquippedItemParent.parent.SetParent(Player.instance.trackingOriginTransform);
        leftHandEquippedItemParent.parent.localPosition = Vector3.zero;
        leftHandEquippedItemParent.parent.localRotation = Quaternion.identity;
        //spawn equipment
        SpawnAllEquipment();
        //set up event listeners
        ItemEquipTable.OnItemEquipped += ItemEquipTable_OnItemEquipped;
        ItemEquipTable.OnItemUnequipped += ItemEquipTable_OnItemUnequipped;
        SaveLoadManager.OnLoad += SaveLoadManager_OnLoad;
    }
    public void DebugRaycastOriginAndDirection(Vector3 origin, Vector3 direction, float distance, bool succeeded = true)
    {
        if (!shouldDebugRaycasts)
            return;

        debugOrigin.gameObject.SetActive(true);
        debugDirection.gameObject.SetActive(true);

        debugOrigin.position = origin;
        debugDirection.position = origin;
        debugDirection.forward = direction;
        debugDirection.localScale = new Vector3(1, 1, distance);

        debugOrigin.GetComponentInChildren<MeshRenderer>().material.color = succeeded ? originalOriginColor : Color.red;
        debugDirection.GetComponentInChildren<MeshRenderer>().material.color = succeeded ? originalDirectionColor : Color.red;
    }

    public void PlayRandomWoodHitSound(Vector3 worldPosition, float volume = 1f)
    {
        if (woodHitSounds.Count == 0)
            return;
        AudioSource.PlayClipAtPoint(woodHitSounds[Random.Range(0, woodHitSounds.Count)], worldPosition, volume);
    }

    public void PlayRandomMetalHitSound(Vector3 worldPosition, float volume = 1f)
    {
        if (metalHitSounds.Count == 0)
            return;
        AudioSource.PlayClipAtPoint(metalHitSounds[Random.Range(0, metalHitSounds.Count)], worldPosition, volume * VREquipment.METAL_VOLUME_MODIFIER);
    }

    private void UnequipAllItems()
    {
        for (int i = 0; i < currentlyEquippedItems.Count; ++i)
            UnequipItem(currentlyEquippedItems[i].DaggerfallItem);
        Debug.Assert(currentlyEquippedItems.Count == 0, "Tried to unequip all items, but we still have " + currentlyEquippedItems.Count + " items equipped.");
    }

    private bool IsEquipTableLoaded()
    {
        var table = itemEquipTable.EquipTable;
        for (int i = 0; i < table.Length; ++i)
            if (table[i] != null)
                return true;
        return false;
    }
    private void EquipItemsOnLoad()
    {
        //stop current coroutine if it's running
        if(equipItemsCoroutine != null)
            StopCoroutine(equipItemsCoroutine);
        //start a new iteration of the coroutine
        equipItemsCoroutine = StartCoroutine(EquipItemsOnLoadCoroutine());
    }
    private IEnumerator EquipItemsOnLoadCoroutine()
    {
        while (!GameManager.Instance.SaveLoadManager.IsReady() || GameManager.Instance.SaveLoadManager.LoadInProgress || GameManager.IsGamePaused || !IsEquipTableLoaded())
            yield return null;

        UnequipAllItems();

        Hand rightHand = DaggerfallVRPlayer.Instance.RightHand;
        Hand leftHand = DaggerfallVRPlayer.Instance.LeftHand;
        bool didSpawnLeft = false, didSpawnRight = false;
       
        while(!didSpawnLeft || !didSpawnRight)
        {
            yield return new WaitForSeconds(.1f);
            if (!didSpawnRight && rightHand.GetTrackedObjectVelocity().sqrMagnitude > .001f)
            {
                yield return new WaitForSeconds(.5f);
                didSpawnRight = true;
                DaggerfallUnityItem rightItem = GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.RightHand);
                if (rightItem != null)
                    EquipItem(rightItem);
            }
            if (!didSpawnLeft && leftHand.GetTrackedObjectVelocity().sqrMagnitude > .001f)
            {
                yield return new WaitForSeconds(.5f);
                didSpawnLeft = true;
                DaggerfallUnityItem leftItem = GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.LeftHand);
                if (leftItem != null)
                    EquipItem(leftItem);
            }
        }
    }

    private void SpawnAllEquipment()
    {
        for(Weapons w = k_weaponStart; w <= k_weaponEnd; w++)
        {
            if (!IsWeaponTwoHanded(w))
                InstantiateEquipment((int)w, w.ToString()); //spawn a 2nd weapon for one-handed weapons
            InstantiateEquipment((int)w, w.ToString());
        }
        for(Armor a = k_armorStart; a <= k_armorEnd; a++)
        {
            InstantiateEquipment((int)a, a.ToString());
        }
    }

    private bool IsEquipable(DaggerfallUnityItem item)
    {
        if(item == null)
        {
            return false;
        }
        return IsEquipable(item.ItemGroup, item.TemplateIndex);
    }
    private bool IsEquipable(ItemGroups group, int templateIndex)
    {
        Armor artifactArmor;
        Weapons artifactWeapon;
        switch (group)
        {
            case ItemGroups.Armor:
                return (Armor)templateIndex >= k_armorStart && (Armor)templateIndex <= k_armorEnd;
            case ItemGroups.Weapons:
                return (Weapons)templateIndex >= k_weaponStart && (Weapons)templateIndex <= k_weaponEnd;
            case ItemGroups.Artifacts:
                return GetWeaponEquivalentForArtifact((ArtifactsSubTypes)templateIndex, out artifactWeapon) ||
                    GetArmorEquivalentForArtifact((ArtifactsSubTypes)templateIndex, out artifactArmor);
            default:
                return false;
        }
    }

    private List<VREquipment> GetListOfSpawnedEquipment(DaggerfallUnityItem forItem)
    {
        //not an artifact? just return at the template index
        if (!forItem.IsArtifact)
            return instantiatedEquipment[forItem.TemplateIndex];

        //it's an artifact. Check if it's a weapon
        Weapons equivalentWeaponForArtifact;
        Armor equivalentArmorForArtifact;
        if (GetWeaponEquivalentForArtifact((ArtifactsSubTypes)forItem.TemplateIndex, out equivalentWeaponForArtifact))
            return instantiatedEquipment[(int)equivalentWeaponForArtifact];
        else if(GetArmorEquivalentForArtifact((ArtifactsSubTypes)forItem.TemplateIndex, out equivalentArmorForArtifact))
            //not a weapon. Check if it's a shield
            return instantiatedEquipment[(int)equivalentArmorForArtifact];
        else
            //return. If it's not a weapon or shield, the list will be null.
            return null;
    }

    public void FloatItemInFrontOfPlayer(VREquipment item, Valve.VR.SteamVR_Input_Sources handType = Valve.VR.SteamVR_Input_Sources.RightHand)
    {
        //parent item to floaty parent
        item.transform.SetParent(handType == Valve.VR.SteamVR_Input_Sources.LeftHand ? leftHandEquippedItemParent : rightHandEquippedItemParent);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.Rigidbody.isKinematic = true;
        //set floaty parent to be at hand position.
        if(handType == Valve.VR.SteamVR_Input_Sources.LeftHand)
        {
            leftHandEquippedItemParent.position = DaggerfallVRPlayer.Instance.LeftHand.transform.position;
            leftHandEquippedItemParent.rotation = DaggerfallVRPlayer.Instance.LeftHand.transform.rotation;
        }
        else
        {
            rightHandEquippedItemParent.position = DaggerfallVRPlayer.Instance.RightHand.transform.position;
            rightHandEquippedItemParent.rotation = DaggerfallVRPlayer.Instance.RightHand.transform.rotation;
        }
    }

    public void EquipItem(DaggerfallUnityItem item)
    {
        if (!IsEquipable(item))
            return;

        List<VREquipment> vrItems = GetListOfSpawnedEquipment(item);
        if(vrItems == null || vrItems.Count == 0)
        {
            Debug.LogError("Something went wrong. Could not find spawned items for item " + item.LongName);
            return;
        }
        int itemIndex = vrItems.FindIndex(p => !p.IsEquipped);
        if (itemIndex < 0)
        {
            Debug.LogError("Unable to equip item " + item.TemplateIndex + " because an unequipped vr equipment of that type was not found.");
            return;
        }
        else
        {
            vrItems[itemIndex].Equip(item);
            currentlyEquippedItems.Add(vrItems[itemIndex]);
        }

    }
    public void UnequipItem(DaggerfallUnityItem item)
    {
        if (!IsEquipable(item))
            return;

        List<VREquipment> vrItems = GetListOfSpawnedEquipment(item);
        if (vrItems == null || vrItems.Count == 0)
        {
            Debug.LogError("Something went wrong. Could not find spawned items for item " + item.LongName);
            return;
        }
        int itemIndex = vrItems.FindIndex(p => p.UID == item.UID);
        if (itemIndex < 0) //this is currently always the case the first time you unequip something, because I can't seem to get a reference to the filled equip table on Init()
            Debug.LogError("Unable to unequip item " + item.TemplateIndex + " because a spawned VR item of UID " + item.UID + " was not found.");
        else
        {
            currentlyEquippedItems.Remove(vrItems[itemIndex]);
            vrItems[itemIndex].Unequip();
        }
    }
    public static bool IsWeaponTwoHanded(Weapons weaponType)
    {
        switch (weaponType)
        {
            case Weapons.Dagger:
            case Weapons.Tanto:
            case Weapons.Shortsword:
            case Weapons.Wakazashi:
            case Weapons.Broadsword:
            case Weapons.Battle_Axe:
            case Weapons.Saber:
            case Weapons.Longsword:
            case Weapons.Katana:
            case Weapons.Mace:
            case Weapons.Arrow: //uhh... not really.
                return false;
            case Weapons.Claymore:
            case Weapons.Dai_Katana:
            case Weapons.Staff:
            case Weapons.Flail:
            case Weapons.Warhammer:
            case Weapons.War_Axe:
            case Weapons.Short_Bow:
            case Weapons.Long_Bow:
                return true;
            default:
                return false;
        }
    }

    public static bool GetWeaponEquivalentForArtifact(ArtifactsSubTypes artifactType, out Weapons weaponEquivalent)
    {
        switch (artifactType)
        {
            case ArtifactsSubTypes.Mehrunes_Razor:
                weaponEquivalent = Weapons.Dagger;
                break;
            case ArtifactsSubTypes.Mace_of_Molag_Bal:
                weaponEquivalent = Weapons.Mace;
                break;
            case ArtifactsSubTypes.Wabbajack:
            case ArtifactsSubTypes.Skull_of_Corruption:
            case ArtifactsSubTypes.Staff_of_Magnus:
                weaponEquivalent = Weapons.Staff;
                break;
            case ArtifactsSubTypes.Volendrung:
                weaponEquivalent = Weapons.Warhammer;
                break;
            case ArtifactsSubTypes.Auriels_Bow:
                weaponEquivalent = Weapons.Long_Bow;
                break;
            case ArtifactsSubTypes.Chrysamere:
                weaponEquivalent = Weapons.Claymore;
                break;
            case ArtifactsSubTypes.Ebony_Blade:
                weaponEquivalent = Weapons.Katana;
                break;
            default:
                weaponEquivalent = Weapons.Arrow;
                return false;
        }
        return true;
    }
    public static bool GetArmorEquivalentForArtifact(ArtifactsSubTypes artifactType, out Armor armorEquivalent)
    {
        switch (artifactType)
        {
            case ArtifactsSubTypes.Lords_Mail:
            case ArtifactsSubTypes.Ebony_Mail:
                armorEquivalent = Armor.Cuirass;
                break;
            case ArtifactsSubTypes.Auriels_Shield:
            case ArtifactsSubTypes.Spell_Breaker:
                armorEquivalent = Armor.Tower_Shield;
                break;
            default:
                armorEquivalent = Armor.Helm;
                return false;
        }
        return true;
    }
    private VREquipment InstantiateEquipment(int templateIndex, string itemName)
    {
        //load resource
        GameObject equipmentPrefab = LoadResource(itemName);
        if (!equipmentPrefab)
        {
            Debug.LogError("Couldn't instantiate item " + itemName);
            return null;
        }
        //spawn it
        VREquipment spawnedEquipment = Instantiate(equipmentPrefab, equipmentParent).GetComponent<VREquipment>();
        spawnedEquipment.gameObject.SetActive(false);
        //add to dict
        if (instantiatedEquipment.ContainsKey(templateIndex))
            instantiatedEquipment[templateIndex].Add(spawnedEquipment);
        else
            instantiatedEquipment[templateIndex] = new List<VREquipment>(new VREquipment[] { spawnedEquipment });

        return spawnedEquipment;
    }
    private GameObject LoadResource(string resourceName)
    {
        GameObject go = Resources.Load<GameObject>(resourcesPath + resourceName);
        if (!go)
            go = Resources.Load<GameObject>(backupResourcesPath + resourceName);
        return go;
    }

    private bool IsPlayerEntity(DaggerfallEntity entity)
    {
        return IsTypeof<PlayerEntity>(entity);
    }

    private void ItemEquipTable_OnItemUnequipped(DaggerfallUnityItem item, DaggerfallEntity entity)
    {
        if (IsPlayerEntity(entity))
            UnequipItem(item);
    }

    private void ItemEquipTable_OnItemEquipped(DaggerfallUnityItem item, DaggerfallEntity entity)
    {
        if (IsPlayerEntity(entity))
            EquipItem(item);
    }

    private void SaveLoadManager_OnLoad(SaveData_v1 saveData)
    {
        EquipItemsOnLoad();
    }

    public Color GetDaggerfallItemMaterialColor(DaggerfallUnityItem item)
    {
        if (item == null)
            return Color.white;

        //weapon colors
        if (item.ItemGroup == ItemGroups.Weapons)
        {
            switch ((WeaponMaterialTypes)item.NativeMaterialValue)
            {
                case WeaponMaterialTypes.None:
                    return Color.white;
                case WeaponMaterialTypes.Iron:
                    return ironColor;
                case WeaponMaterialTypes.Steel:
                    return steelColor;
                case WeaponMaterialTypes.Silver:
                    return silverColor;
                case WeaponMaterialTypes.Elven:
                    return elvenColor;
                case WeaponMaterialTypes.Dwarven:
                    return dwarvenColor;
                case WeaponMaterialTypes.Mithril:
                    return mithrilColor;
                case WeaponMaterialTypes.Adamantium:
                    return adamantiumColor;
                case WeaponMaterialTypes.Ebony:
                    return ebonyColor;
                case WeaponMaterialTypes.Orcish:
                    return orcishColor;
                case WeaponMaterialTypes.Daedric:
                    return daedricColor;
                default:
                    return Color.white;
            }

        }

        //armor colors
        if(item.ItemGroup == ItemGroups.Armor)
        {
            switch ((ArmorMaterialTypes)item.NativeMaterialValue)
            {
                case ArmorMaterialTypes.None:
                    break;
                case ArmorMaterialTypes.Leather:
                    return leatherColor;
                case ArmorMaterialTypes.Chain:
                case ArmorMaterialTypes.Chain2:
                    return chainColor;
                case ArmorMaterialTypes.Iron:
                    return ironColor;
                case ArmorMaterialTypes.Steel:
                    return steelColor;
                case ArmorMaterialTypes.Silver:
                    return silverColor;
                case ArmorMaterialTypes.Elven:
                    return elvenColor;
                case ArmorMaterialTypes.Dwarven:
                    return dwarvenColor;
                case ArmorMaterialTypes.Mithril:
                    return mithrilColor;
                case ArmorMaterialTypes.Adamantium:
                    return adamantiumColor;
                case ArmorMaterialTypes.Ebony:
                    return ebonyColor;
                case ArmorMaterialTypes.Orcish:
                    return orcishColor;
                case ArmorMaterialTypes.Daedric:
                    return daedricColor;
                default:
                    return Color.white;
            }
        }

        //artifact colors
        if(item.ItemGroup == ItemGroups.Artifacts)
        {
            switch ((ArtifactsSubTypes)item.TemplateIndex)
            {
                case ArtifactsSubTypes.Auriels_Bow:
                    return elvenColor;
                case ArtifactsSubTypes.Auriels_Shield:
                    return elvenColor;
                case ArtifactsSubTypes.Chrysamere:
                    return adamantiumColor;
                case ArtifactsSubTypes.Lords_Mail:
                    return mithrilColor;
                case ArtifactsSubTypes.Ebony_Blade:
                case ArtifactsSubTypes.Ebony_Mail:
                    return ebonyColor;
                case ArtifactsSubTypes.Wabbajack:
                case ArtifactsSubTypes.Staff_of_Magnus:
                case ArtifactsSubTypes.Mace_of_Molag_Bal:
                    return daedricColor;
                case ArtifactsSubTypes.Mehrunes_Razor:
                    return orcishColor;
                case ArtifactsSubTypes.Volendrung:
                case ArtifactsSubTypes.Spell_Breaker:
                    return dwarvenColor;

                default:
                    return daedricColor;
            }
        }

        return Color.white;
    }

    //vTODO: move this to generic utilities class
    public static bool IsTypeof<T>(object obj)
    {
        return (obj is T);
    }
}
