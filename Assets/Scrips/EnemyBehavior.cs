using System.Collections;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Patrouille")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float directionChangeInterval = 3f;

    [Header("Sichtfeld")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float fieldOfView = 40f;

    [Header("Verfolgung")]
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float normalSpeed = 3f;
    [SerializeField] private float sprintDuration = 2f;
    [SerializeField] private float sprintCooldown = 5f;

    [SerializeField] private Transform planeTransform;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private float timeSinceLastDirectionChange = 0f;

    private GameObject playerCube;
    private bool hasTarget = false;

    private Vector3 planeCenter;
    private float halfWidth;
    private float halfLength;

    private float currentSpeed;
    private bool isSprinting = false;
    private float sprintTimer = 0f;
    private float cooldownTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCube = GameObject.FindWithTag("Player");

        currentSpeed = normalSpeed;

        ChooseNewDirection();
        UpdatePlaneBounds();
    }

    private void FixedUpdate()
    {
        if (!hasTarget)
        {
            Patrol();
            LookForPlayer();
        }
        else
        {
            UpdateSprintState();
            ChasePlayer();
        }
    }

    private void Patrol()
    {
        Vector3 newPosition = transform.position + moveDirection * speed * Time.fixedDeltaTime;

        if (IsWithinBounds(newPosition))
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            ChooseNewDirection();
        }

        timeSinceLastDirectionChange += Time.fixedDeltaTime;
        if (timeSinceLastDirectionChange >= directionChangeInterval)
        {
            ChooseNewDirection();
            timeSinceLastDirectionChange = 0f;
        }
    }

    private void LookForPlayer()
    {
        if (playerCube == null) return;

        Vector3 directionToPlayer = (playerCube.transform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, playerCube.transform.position);

        if (distance > detectionRange) return;

        float angle = Vector3.Angle(moveDirection, directionToPlayer);
        if (angle <= fieldOfView / 2f)
        {
            hasTarget = true;
        }
    }

    private void ChasePlayer()
    {
        if (playerCube == null) return;

        Vector3 direction = (playerCube.transform.position - transform.position).normalized;
        moveDirection = new Vector3(direction.x, 0f, direction.z);

        rb.MovePosition(transform.position + moveDirection * currentSpeed * Time.fixedDeltaTime);
    }

    private void ChooseNewDirection()
    {
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
    }

    private void UpdatePlaneBounds()
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

    private bool IsWithinBounds(Vector3 position)
    {
        return position.x >= (planeCenter.x - halfWidth) &&
               position.x <= (planeCenter.x + halfWidth) &&
               position.z >= (planeCenter.z - halfLength) &&
               position.z <= (planeCenter.z + halfLength);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(other.gameObject); // Cube wird gefressen
            GameManager.Instance.TriggerGameOver(); // Game Over einleiten
        }
    }

    private void UpdateSprintState()
    {
        if (isSprinting)
        {
            sprintTimer -= Time.deltaTime;
            if (sprintTimer <= 0f)
            {
                isSprinting = false;
                currentSpeed = normalSpeed;
                cooldownTimer = sprintCooldown;
            }
        }
        else
        {
            cooldownTimer -= Time.deltaTime;

            // Falls genug Zeit vergangen ist, kann Enemy wieder sprinten
            if (cooldownTimer <= 0f)
            {
                isSprinting = true;
                currentSpeed = sprintSpeed;
                sprintTimer = sprintDuration;
            }
        }
    }
}
