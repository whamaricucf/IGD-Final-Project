using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassiveUpgradeIconHandler : MonoBehaviour
{
    [System.Serializable]
    public class PassiveUpgradeSlot
    {
        public GameObject slotObject; // Full PassiveUpgradeIcon prefab instance
        public Image iconImage;        // Image for the passive upgrade
        public List<Image> levelSquares; // Squares showing upgrade levels
    }

    public List<PassiveUpgradeSlot> passiveUpgradeSlots = new List<PassiveUpgradeSlot>();

    private Dictionary<string, int> upgradeNameToSlotIndex = new Dictionary<string, int>();

    private void Start()
    {
        foreach (var slot in passiveUpgradeSlots)
        {
            if (slot.slotObject != null)
                slot.slotObject.SetActive(false);
        }
    }

    public void AddOrUpdatePassiveUpgrade(string upgradeName, Sprite icon, int level)
    {
        if (upgradeNameToSlotIndex.TryGetValue(upgradeName, out int slotIndex))
        {
            UpdateLevelSquares(passiveUpgradeSlots[slotIndex], level);
        }
        else
        {
            for (int i = 0; i < passiveUpgradeSlots.Count; i++)
            {
                if (!passiveUpgradeSlots[i].slotObject.activeSelf)
                {
                    passiveUpgradeSlots[i].slotObject.SetActive(true);
                    passiveUpgradeSlots[i].iconImage.sprite = icon;
                    UpdateLevelSquares(passiveUpgradeSlots[i], level);
                    upgradeNameToSlotIndex[upgradeName] = i;
                    break;
                }
            }
        }
    }

    private void UpdateLevelSquares(PassiveUpgradeSlot slot, int level)
    {
        for (int i = 0; i < slot.levelSquares.Count; i++)
        {
            slot.levelSquares[i].enabled = i < level;
        }
    }
}