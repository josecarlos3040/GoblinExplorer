using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class VerticalLoop : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Camera mainCam;

    [Header("Prefab")]
    [Tooltip("Prefab do segmento (deve ter Collider2D ou Renderer)")]
    [SerializeField] private GameObject segmentPrefab;

    [Header("Posicionamento")]
    [Tooltip("X fixo para todos os spawns")]
    [SerializeField] private float spawnX = 0.3f;
    [Tooltip("Y da primeira borda (instanciada no Start)")]
    [SerializeField] private float firstSpawnY = 11f;

    [Header("Spawner")]
    [Tooltip("Distância acima do topo da câmera para manter blocos carregados")]
    [SerializeField] private float spawnDistanceAboveCamera = 15f;

    private float spawnIntervalY; // Agora recebe AUTOMATICAMENTE o Y real do prefab
    private float nextSpawnY;

    void Start()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null || segmentPrefab == null)
        {
            Debug.LogError("[VerticalLoop] Referências ausentes no Inspector. Desabilitando script.");
            enabled = false;
            return;
        }

        // 1. Pega a altura REAL do Y do seu prefab de forma infalível
        spawnIntervalY = MeasurePrefabHeight();

        // 2. Instancia a primeira borda onde você mandou
        SpawnAt(firstSpawnY);

        // CORREÇÃO AQUI: O próximo spawn DEVE ser a posição da primeira + a altura dela.
        // Isso evita buracos e faz o intervalo Y funcionar perfeitamente em fila.
        nextSpawnY = firstSpawnY + spawnIntervalY;
    }

    void LateUpdate()
    {
        float camTop = GetCameraTopWorldY();

        // Enquanto o próximo ponto de spawn estiver dentro do alcance visível de cima, gera blocos
        while (nextSpawnY < camTop + spawnDistanceAboveCamera)
        {
            SpawnAt(nextSpawnY);
            nextSpawnY += spawnIntervalY; // Avança o Y usando o tamanho exato do bloco
        }
    }

    void SpawnAt(float y)
    {
        Vector3 pos = new Vector3(spawnX, y, 0f);
        GameObject go = Instantiate(segmentPrefab, pos, Quaternion.identity, transform);
        go.name = segmentPrefab.name + "_spawned";
    }

    float GetCameraTopWorldY()
    {
        if (mainCam.orthographic)
            return mainCam.transform.position.y + mainCam.orthographicSize;

        float zDist = Mathf.Abs(mainCam.transform.position.z - 0f);
        return mainCam.ViewportToWorldPoint(new Vector3(0.5f, 1f, zDist)).y;
    }

    float MeasurePrefabHeight()
    {
        // Instancia um objeto temporário bem longe para medir o tamanho real sem bugar a física/visão do jogador
        GameObject temp = Instantiate(segmentPrefab, new Vector3(9999f, 9999f, 9999f), Quaternion.identity);
        Bounds combinedBounds = new Bounds(temp.transform.position, Vector3.zero);

        // Mede priorizando Sprites/Renderers (que é o seu caso)
        var rends = temp.GetComponentsInChildren<Renderer>(true);
        if (rends.Length > 0)
        {
            combinedBounds = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) combinedBounds.Encapsulate(rends[i].bounds);
        }
        else
        {
            // Se não achar sprite, tenta medir por Colliders
            var cols = temp.GetComponentsInChildren<Collider2D>(true);
            if (cols.Length > 0)
            {
                combinedBounds = cols[0].bounds;
                for (int i = 1; i < cols.Length; i++) combinedBounds.Encapsulate(cols[i].bounds);
            }
        }

        Destroy(temp); // Destrói o clone de teste imediatamente

        // Se encontrou um tamanho válido, retorna ele. Se falhar, usa o seu valor padrão de 7.111111f
        return combinedBounds.size.y > 0.001f ? combinedBounds.size.y : 7.111111f;
    }
}