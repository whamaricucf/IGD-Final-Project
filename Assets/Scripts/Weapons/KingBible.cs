using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingBible : MonoBehaviour
{
    public string projectileTag = "KingBible";
    public WeaponData bibleStats;

    private List<GameObject> activeBibles = new List<GameObject>();
    private float cooldownTimer = 0f;
    private bool poolReady = false;

    void Start()
    {
        cooldownTimer = 0f;

        if (bibleStats == null)
            Debug.LogWarning("KingBible: bibleStats is not assigned!");

        StartCoroutine(WaitForPoolReady());
    }

    IEnumerator WaitForPoolReady()
    {
        // Wait until ObjectPooler is initialized
        while (ObjectPooler.Instance == null || !ObjectPooler.Instance.IsInitialized)
            yield return null;

        poolReady = true;
    }

    void Update()
    {
        if (!poolReady || bibleStats == null) return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            SpawnBibles();
            cooldownTimer = bibleStats.cd;
        }
    }

    void SpawnBibles()
    {
        ClearActiveBibles();

        float angleStep = 360f / Mathf.Max(1, bibleStats.amount);

        for (int i = 0; i < bibleStats.amount; i++)
        {
            GameObject bible = ObjectPooler.Instance.SpawnFromPool(projectileTag, transform.position, Quaternion.identity);
            if (bible == null) continue;

            var bibleScript = bible.GetComponent<KingBibleProjectile>();
            if (bibleScript != null)
            {
                bibleScript.Activate(
                    this.transform,
                    bibleStats.area,
                    i * angleStep,
                    bibleStats.baseDMG,
                    bibleStats.knockback,
                    bibleStats.critChance,
                    bibleStats.critMulti,
                    bibleStats.spd
                );
            }

            activeBibles.Add(bible);
        }

        StartCoroutine(DisableAfterTime(bibleStats.duration));
    }

    IEnumerator DisableAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        ClearActiveBibles();
    }

    void ClearActiveBibles()
    {
        foreach (var bible in activeBibles)
        {
            if (bible != null)
            {
                var fade = bible.GetComponent<FadeEffect>();
                if (fade != null)
                    fade.FadeOutAndDisable();
                else
                    bible.SetActive(false);
            }
        }

        activeBibles.Clear();
    }
}
