using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingBible : MonoBehaviour
{
    public string projectileTag = "KingBible";
    public int numberOfBibles = 1;
    public float radius = 2.5f;
    public float duration = 5f;
    public float cooldown = 8f;

    private List<GameObject> activeBibles = new List<GameObject>();
    private float cooldownTimer = 0f;

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            SpawnBibles();
            cooldownTimer = cooldown;
        }
    }

    void SpawnBibles()
    {
        ClearActiveBibles();

        float angleStep = 360f / numberOfBibles;

        for (int i = 0; i < numberOfBibles; i++)
        {
            GameObject bible = ObjectPooler.Instance.SpawnFromPool(projectileTag, transform.position, Quaternion.identity);
            bible.GetComponent<KingBibleProjectile>().Activate(this.transform, radius, i * angleStep);
            activeBibles.Add(bible);
        }

        StartCoroutine(DisableAfterTime(duration));
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
                FadeEffect fade = bible.GetComponent<FadeEffect>();
                if (fade != null)
                    fade.FadeOutAndDisable();
                else
                    bible.SetActive(false);
            }
        }

        activeBibles.Clear();
    }
}
