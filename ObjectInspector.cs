using System;
using UnityEngine;

public class ObjectInspector : MonoBehaviour
{
    public GameObject ispectObject;
    [SerializeField] private Vector3 targetPos;
    private Vector3 targetRot;
    [SerializeField] private Vector3 initialSpawnOffset = Vector3.down*5f;
    [SerializeField] private Transform objectRorator;
    [SerializeField] private Vector2 zoom = new Vector2(0.5f, 2f);

    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float zooming;

    public float dragSpeed = 2;
    private Vector3 dragOrigin;
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;

    private void Update()
    {
        if (ispectObject == null) return;
        try
        {
            ZoomInOut();
            RotateObject();
            Move();
        }
        catch (Exception ex) { print(ex.ToString()); }
    }

    private void Move()
    {
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = Input.mousePosition;
            return;
        }
        if (Input.GetMouseButton(2))
        {
            Vector3 difference = Input.mousePosition - dragOrigin;
            Vector3 move = new Vector3(difference.x * dragSpeed * Time.deltaTime, difference.y * dragSpeed * Time.deltaTime, 0);
            Vector3 newPosition = transform.position - move;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
            transform.position = newPosition;

            dragOrigin = Input.mousePosition;
        }
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        transform.position = clampedPosition;
}

    private void ZoomInOut()
    {
        targetPos = new Vector3(targetPos.x, targetPos.y, Mathf.Clamp(targetPos.z - Input.GetAxis("Mouse ScrollWheel")* zooming, zoom.x, zoom.y));
        objectRorator.localPosition = Vector3.Lerp(objectRorator.localPosition, targetPos, Time.deltaTime * zoomSpeed);
    }

    private void RotateObject()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            objectRorator.Rotate(new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * Time.deltaTime * rotationSpeed, Space.World);
        }
    }
}
