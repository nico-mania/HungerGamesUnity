using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Behavior : MonoBehaviour
{
    [Header("Hunger")]
    public float hunger = 100f;
    public float hungerDecayRate = 5f;
    public float hungerGain = 25f;
    public float sprintHungerMultiplier = 2f;
    public Slider hungerBar;

    [Header("Erkennung")]
    public float detectionRange = 20f;
    public float fov = 60f;

    [Header("Bewegung")]
    public float normalSpeed = 3f;
    public float sprintSpeed = 6f;
    public float directionChangeInterval = 2f;
    public float sprintRange = 5f;
    public Transform planeTransform;

    [Header("Umschauen")]
    public float scanInterval = 6f;
    public float rotationDuration = 0.5f;

    // Private
    private float currentSpeed;
    private Vector3 moveDirection;
    private Rigidbody rb;
    private GameObject targetFood;

    private float timeSinceLastDirectionChange = 0f;
    private float timeSinceLastScan = 0f;
    private bool isScanning = false;

    // Plane-Bounds
    private Vector3 planeCenter;
    private float halfWidth;
    private float halfLength;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ChooseNewDirection();
        UpdatePlaneBounds();
    }

    void FixedUpdate()
    {
        if (isScanning) return;

        HandleMovement();
        HandleDirectionChange();
        UpdatePlaneBounds();
        FindNearestFood();
        DecideSpeed();
        MoveToFood();
        UpdateHunger();
        HandleScanning();
    }

    void HandleMovement()
    {
        Vector3 newPosition = transform.position + moveDirection * currentSpeed * Time.fixedDeltaTime;

        if (IsWithinBounds(newPosition))
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            ChooseNewDirection();
        }
    }

    void HandleDirectionChange()
    {
        timeSinceLastDirectionChange += Time.fixedDeltaTime;

        if (timeSinceLastDirectionChange >= directionChangeInterval)
        {
            ChooseNewDirection();
            timeSinceLastDirectionChange = 0f;
        }
    }

    void HandleScanning()
    {
        if (targetFood == null)
        {
            timeSinceLastScan += Time.fixedDeltaTime;

            if (timeSinceLastScan >= scanInterval)
            {
                StartCoroutine(LookAroundSmooth());
                timeSinceLastScan = 0f;
            }
        }
        else
        {
            timeSinceLastScan = 0f;
        }
    }

    void ChooseNewDirection()
    {
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
    }

    void UpdatePlaneBounds()
    {
        if (!planeTransform)
        {
            Debug.LogError("Plane Transform nicht gesetzt!");
            return;
        }

        planeCenter = planeTransform.position;
        halfWidth = (planeTransform.localScale.x * 10f) / 2f;
        halfLength = (planeTransform.localScale.z * 10f) / 2f;
    }

    bool IsWithinBounds(Vector3 position)
    {
        return position.x >= (planeCenter.x - halfWidth) &&
               position.x <= (planeCenter.x + halfWidth) &&
               position.z >= (planeCenter.z - halfLength) &&
               position.z <= (planeCenter.z + halfLength);
    }

    void FindNearestFood()
    {
        GameObject[] foodItems = GameObject.FindGameObjectsWithTag("Food");
        float shortestDistance = Mathf.Infinity;
        GameObject nearest = null;

        foreach (GameObject food in foodItems)
        {
            Vector3 directionToFood = (food.transform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, food.transform.position);

            if (distance > detectionRange) continue;

            float angleToFood = Vector3.Angle(moveDirection, directionToFood);
            if (angleToFood > fov / 2f) continue;

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearest = food;
            }
        }

        targetFood = nearest;
    }

    void MoveToFood()
    {
        if (targetFood != null)
        {
            Vector3 direction = (targetFood.transform.position - transform.position).normalized;
            moveDirection = new Vector3(direction.x, 0f, direction.z);
        }
    }

    void DecideSpeed()
    {
        if (targetFood == null)
        {
            currentSpeed = normalSpeed;
            return;
        }

        float distance = Vector3.Distance(transform.position, targetFood.transform.position);
        float timeToSprint = distance / sprintSpeed;
        float timeToWalk = distance / normalSpeed;

        float sprintCost = timeToSprint * hungerDecayRate * sprintHungerMultiplier;
        float walkCost = timeToWalk * hungerDecayRate;

        bool canAffordSprint = hunger > sprintCost + 5f;
        bool isWorthIt = walkCost - sprintCost > 2f;

        currentSpeed = (canAffordSprint && isWorthIt) ? sprintSpeed : normalSpeed;
    }

    void UpdateHunger()
    {
        float decay = hungerDecayRate;
        if (currentSpeed == sprintSpeed)
            decay *= sprintHungerMultiplier;

        hunger -= decay * Time.deltaTime;
        hunger = Mathf.Clamp(hunger, 0f, 100f);

        if (hungerBar != null)
            hungerBar.value = hunger;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            hunger = Mathf.Min(hunger + hungerGain, 100f);
            Destroy(other.gameObject);
        }
    }

    IEnumerator LookAroundSmooth()
    {
        isScanning = true;

        float angleChange = Random.Range(60f, 120f) * (Random.value < 0.5f ? -1f : 1f);

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, transform.eulerAngles.y + angleChange, 0);

        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationDuration);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        moveDirection = transform.forward.normalized;
        isScanning = false;
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Vector3 origin = transform.position;
        float radius = detectionRange;
        float halfFOV = fov / 2f;
        int segments = 30;
        Vector3 forward = moveDirection.normalized;

        // Hauptstrahl (Blickrichtung)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + forward * radius);

        // Randstrahlen
        Gizmos.color = Color.yellow;
        Vector3 leftDir = Quaternion.Euler(0, -halfFOV, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, halfFOV, 0) * forward;

        Gizmos.DrawLine(origin, origin + leftDir * radius);
        Gizmos.DrawLine(origin, origin + rightDir * radius);

        // Bogen
        Vector3 prevPoint = origin + leftDir * radius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfFOV + (fov * i / segments);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = origin + dir.normalized * radius;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}
