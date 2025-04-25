using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GemMergeManager : MonoBehaviour
{
    public GameObject superGemPrefab;
    public float mergeCheckInterval = 5f;
    public float mergeDuration = 0.4f;

    void Start()
    {
        InvokeRepeating(nameof(MergeNearbyGems), 5f, mergeCheckInterval);
    }

    void MergeNearbyGems()
    {
        GameObject[] gems = GameObject.FindGameObjectsWithTag("Gem");
        if (gems.Length < 20) return;

        Vector3 center = Vector3.zero;
        int totalXP = 0;

        foreach (GameObject gem in gems)
        {
            center += gem.transform.position;
            totalXP += gem.GetComponent<GemPickup>().expValue;
        }

        center /= gems.Length;

        foreach (GameObject gem in gems)
        {
            Transform gemT = gem.transform;
            gemT.DOMove(center, mergeDuration).SetEase(Ease.InSine).OnComplete(() =>
            {
                ObjectPooler.Instance.ReturnToPool("Gem", gem);
            });
        }

        StartCoroutine(SpawnSuperGem(center, totalXP, mergeDuration));
    }

    private IEnumerator SpawnSuperGem(Vector3 position, int xp, float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject superGem = ObjectPooler.Instance.SpawnFromPool("SuperGem", position, Quaternion.identity);
        superGem.GetComponent<GemPickup>().expValue = xp;
        superGem.transform.localScale = Vector3.zero;
        superGem.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
    }
}
