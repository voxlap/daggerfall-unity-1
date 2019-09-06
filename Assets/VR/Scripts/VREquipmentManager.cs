using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Entity;

public class VREquipmentManager : MonoBehaviour
{
    public string resourcesPath = "VR/Equipment/";
    public string backupResourcesPath = "VR/EquipmentBackup/"; //used if there are missing assets in VR/Equipment.
    public Transform equipmentParent;

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

    /// <summary> Dictionary of instantiated weapons. It's a list of equipment because some weapons can be dual weilded, so two will be spawned.
    /// The key is the item's template index. </summary>
    private Dictionary<int, List<VREquipment>> instantiatedEquipment = new Dictionary<int, List<VREquipment>>();
    
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
        ItemEquipTable.OnItemEquipped += ItemEquipTable_OnItemEquipped;
        ItemEquipTable.OnItemUnequipped += ItemEquipTable_OnItemUnequipped;
    }
    private void OnDestroy()
    {
        ItemEquipTable.OnItemEquipped -= ItemEquipTable_OnItemEquipped;
        ItemEquipTable.OnItemUnequipped -= ItemEquipTable_OnItemUnequipped;
    }

    public void SpawnAllEquipment()
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
        if (item.EquipSlot != EquipSlots.LeftHand && item.EquipSlot != EquipSlots.RightHand) //no support, yet, for showing anything but what you're holding
            return false;
        return IsEquipable(item.ItemGroup, item.TemplateIndex);
    }
    private bool IsEquipable(ItemGroups group, int templateIndex)
    {
        switch (group)
        {
            case ItemGroups.Armor:
                return (Armor)templateIndex >= k_armorStart && (Armor)templateIndex <= k_armorEnd;
            case ItemGroups.Weapons:
                return (Weapons)templateIndex >= k_weaponStart && (Weapons)templateIndex <= k_weaponStart;
            case ItemGroups.Artifacts:
                return GetWeaponEquivalentForArtifact((ArtifactsSubTypes)templateIndex) != Weapons.None &&
                    GetArmorEquivalentForArtifact((ArtifactsSubTypes)templateIndex) != Armor.None;
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
        int equivalentItemIndexForArtifact = (int)GetWeaponEquivalentForArtifact((ArtifactsSubTypes)forItem.TemplateIndex);
        if (equivalentItemIndexForArtifact < 0)
            //not a weapon. Check if it's a shield
            equivalentItemIndexForArtifact = (int)GetArmorEquivalentForArtifact((ArtifactsSubTypes)forItem.TemplateIndex);

        //return. If it's not a weapon or shield, the list will be null.
        return instantiatedEquipment[equivalentItemIndexForArtifact];
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
            Debug.LogError("Unable to equip item " + item.TemplateIndex + " because an unequipped vr equipment of that type was not found.");
        else
            vrItems[itemIndex].Equip(item);
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
        if (itemIndex < 0)
            Debug.LogError("Unable to unequip item " + item.TemplateIndex + " because a spawned VR item of UID " + item.UID + " was not found.");
        else
            vrItems[itemIndex].Unequip();
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

    public static Weapons GetWeaponEquivalentForArtifact(ArtifactsSubTypes artifactType)
    {
        switch (artifactType)
        {
            case ArtifactsSubTypes.Mehrunes_Razor:
                return Weapons.Dagger;
            case ArtifactsSubTypes.Mace_of_Molag_Bal:
                return Weapons.Mace;
            case ArtifactsSubTypes.Wabbajack:
            case ArtifactsSubTypes.Skull_of_Corruption:
            case ArtifactsSubTypes.Staff_of_Magnus:
                return Weapons.Staff;
            case ArtifactsSubTypes.Volendrung:
                return Weapons.Warhammer;
            case ArtifactsSubTypes.Auriels_Bow:
                return Weapons.Long_Bow;
            case ArtifactsSubTypes.Chrysamere:
                return Weapons.Claymore;
            case ArtifactsSubTypes.Ebony_Blade:
                return Weapons.Katana;
            default:
                return Weapons.None;
        }
    }
    public static Armor GetArmorEquivalentForArtifact(ArtifactsSubTypes artifactType)
    {
        switch (artifactType)
        {
            case ArtifactsSubTypes.Lords_Mail:
            case ArtifactsSubTypes.Ebony_Mail:
                return Armor.Cuirass;
            case ArtifactsSubTypes.Auriels_Shield:
            case ArtifactsSubTypes.Spell_Breaker:
                return Armor.Tower_Shield;
            default:
                return Armor.None;
        }
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
