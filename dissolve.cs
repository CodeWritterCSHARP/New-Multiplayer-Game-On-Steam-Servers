using UnityEngine;

public class dissolve : MonoBehaviour
{
    public float time = .75f;
    public bool apper = true;
    [SerializeField] private float speed = 1000f;
    public Material materials; 

    private int dissolveAmount = Shader.PropertyToID("_dissolveAmount");
    private int verticalDissolve = Shader.PropertyToID("_verticaldissolve");

    private void Update()
    {
        if (apper) {
            time -= dissolveAmount * Time.deltaTime / speed;
            materials.SetFloat(verticalDissolve, time);
        }
        else
        {
            time += dissolveAmount * Time.deltaTime / speed;
            materials.SetFloat(dissolveAmount, time);
            materials.SetFloat(verticalDissolve, 0);
        }
        if (time >= 1.5f) { time = 0; materials.SetFloat(dissolveAmount, 0.15f); }
    }
}
