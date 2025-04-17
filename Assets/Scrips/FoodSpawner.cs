using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;
    public Transform planeTransform;

    public float spawnInterval = 5f;
    public int maxFoodOnField = 30;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        // Prüfen: alle X Sekunden neues Futter
        if (timer >= spawnInterval && GameManager.Instance.CanSpawnFood())
        {
            TrySpawnFood();
            timer = 0f;
        }
    }

    void TrySpawnFood()
    {
        // Spawn nur, wenn noch nicht zu viel Essen rumliegt
        GameObject[] currentFood = GameObject.FindGameObjectsWithTag("Food");
        if (currentFood.Length >= maxFoodOnField) return;

        float width = planeTransform.localScale.x * 10f;
        float length = planeTransform.localScale.z * 10f;
        Vector3 center = planeTransform.position;

        float x = Random.Range(center.x - width / 2, center.x + width / 2);
        float z = Random.Range(center.z - length / 2, center.z + length / 2);
        Vector3 spawnPos = new Vector3(x, 0.5f, z);

        Instantiate(foodPrefab, spawnPos, Quaternion.identity);
    }
}
