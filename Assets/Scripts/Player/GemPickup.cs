using UnityEngine;
using DG.Tweening;

public class GemPickup : MonoBehaviour
{
    public int expValue = 1;
    public float tweenDuration = 0.4f;
    public Ease easeType = Ease.InQuad;
    public AudioClip pickupSFX;

    private bool isBeingCollected = false;
    private Transform playerTarget;
    private SpriteRenderer sr;
    private AudioSource audioSource;
    private GemRotator rotator;

    private float moveSpeed = 10f; // Adjust this value to change movement speed
    private float collectionDistance = 1f; // Larger threshold to collect the gem

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rotator = GetComponent<GemRotator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
    }

    private void OnEnable()
    {
        isBeingCollected = false;

        if (sr != null)
            sr.color = Color.white;

        if (rotator != null)
            rotator.rotate = true;

        GetComponent<Collider>().enabled = true;

        transform.localScale = Vector3.one * Random.Range(0.95f, 1.05f);
        transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 3, 0.5f);

        transform.DOLocalMoveY(transform.localPosition.y + 0.1f, 1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetId("GemFloat");
    }

    private void OnDisable()
    {
        DOTween.Kill("GemFloat");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Magnet") && !isBeingCollected)
        {
            isBeingCollected = true;
            playerTarget = other.transform.parent;

            GetComponent<Collider>().enabled = false;

            if (rotator != null)
                rotator.rotate = false;

            transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 4, 0.5f);

            // Initiate the position update to move towards player
            if (sr != null)
            {
                sr.DOFade(0f, tweenDuration).SetEase(Ease.InSine);
                sr.material.DOColor(Color.yellow, "_Color", 0.1f)
                    .OnComplete(() => sr.material.DOColor(Color.white, "_Color", 0.2f));
            }
        }
    }

    private void Update()
    {
        // Only update position if the gem is being collected
        if (isBeingCollected && playerTarget != null)
        {
            // Move the gem towards the player's position
            transform.position = Vector3.MoveTowards(transform.position, playerTarget.position, Time.deltaTime * moveSpeed);

            // If the gem is close enough to the player, stop moving and collect it
            if (Vector3.Distance(transform.position, playerTarget.position) < collectionDistance)
            {
                CollectGem();
            }
        }
    }

    private void CollectGem()
    {
        // Gain experience for the player
        PlayerExperience.Instance.GainExperience(expValue);
        // Update the UI after gaining experience
        PlayerExperience.Instance.UpdateUI();

        if (pickupSFX != null)
        {
            audioSource.PlayOneShot(pickupSFX, 0.75f);
        }

        // Return the gem to the pool
        ObjectPooler.Instance.ReturnToPool("Gem", gameObject);

        // Stop further movement by setting isBeingCollected to false
        isBeingCollected = false;
        gameObject.SetActive(false); // Ensure the gem is deactivated after collection
    }
}
