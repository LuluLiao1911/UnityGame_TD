using System.Collections;
using UnityEngine;

public class LiverProducer : MonoBehaviour
{
    public GameObject liverPrefab; // 把做好的「肝臟」Prefab 拖進來
    public float produceInterval = 10.0f; // 10秒產出一個

    void Start()
    {
        StartCoroutine(ProduceRoutine());
    }

    IEnumerator ProduceRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(produceInterval);
            SpawnLiver();
        }
    }

    void SpawnLiver()
    {
        // 在學生附近隨機一點點位置生成，避免重疊
        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, 0);
        Instantiate(liverPrefab, spawnPos, Quaternion.identity);
    }
}