using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Utility para crear el prefab del escudo de dash automáticamente
/// Ejecuta desde el menu Tools > ARPG > Create Dash Shield Prefab
/// </summary>
public class DashShieldPrefabCreator
{
#if UNITY_EDITOR
    [MenuItem("Tools/ARPG/Create Dash Shield Prefab")]
    public static void CreateDashShieldPrefab()
    {
        string prefabPath = "Assets/Prefab/DashShield.prefab";

        // Crear GameObject raíz
        GameObject shieldGO = new GameObject("DashShield");

        // Crear Esfera hijo
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Shield";
        sphere.transform.SetParent(shieldGO.transform);
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        // Remover collider de la esfera (no lo necesitamos)
        Collider col = sphere.GetComponent<Collider>();
        if (col != null)
            UnityEngine.Object.DestroyImmediate(col);

        // Crear material transparente azul
        Material shieldMat = new Material(Shader.Find("Standard"));
        shieldMat.SetFloat("_Mode", 3); // Transparent mode
        shieldMat.renderQueue = 3000;
        shieldMat.color = new Color(0, 0.5f, 1, 0.4f); // Azul transparente

        // Aplicar material
        MeshRenderer meshRenderer = sphere.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material = shieldMat;
        }

        // Agregar script DashShieldVFX
        DashShieldVFX vfxScript = shieldGO.AddComponent<DashShieldVFX>();
        vfxScript.shieldMaterial = shieldMat;
        vfxScript.rotationSpeed = 360f;
        vfxScript.scalePulse = 0.15f;
        vfxScript.pulseSpeed = 6f;

        // Crear el prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(shieldGO, prefabPath);
        
        // Destruir el GameObject temporal
        UnityEngine.Object.DestroyImmediate(shieldGO);

        Debug.Log($"✅ Prefab DashShield creado en: {prefabPath}");
        
        // Recargar la base de datos de assets
        AssetDatabase.Refresh();
        
        // Seleccionar el prefab en el project
        EditorGUIUtility.PingObject(prefab);
    }
#endif
}
