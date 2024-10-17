using UnityEngine;

public class shaderRotChange : MonoBehaviour
{
    private Material material;
    [SerializeField] private float rotationSpeed = 30f;
    private Vector4 rotationVector;

    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        if (material.HasProperty("_Rotation")) rotationVector = material.GetVector("_Rotation");
    }

    void Update()
    {
        rotationVector.x += rotationSpeed * Time.deltaTime;
        if (rotationVector.x >= 360f) rotationVector.x = 0f;
        material.SetVector("_Rotation", rotationVector);
    }
}
