using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1f;

    private float rotation = 0f;
    private float rotationTimer = 0f;


    private void Update()
    {
        rotation = (Time.deltaTime * rotationSpeed);        
        Vector3 rotationVector = new Vector3(0, 0, rotation);

        gameObject.transform.Rotate(rotationVector);
    }
}
