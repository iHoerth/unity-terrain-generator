using UnityEngine;

public class PlayerLook : MonoBehaviour
{   
    public Camera cam;
    public RaycastHit hit;  

    private float xRotation = 0f;

    public float xSensitivity = 30f;
    public float ySensitivity = 30f;

    public void ProcessLook(Vector2 input) // we get the input from the InputManager
    {

        float mouseX = input.x;
        float mouseY = input.y;

        // calculate camera rotation for looking up and down
        xRotation -= (mouseY * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80, 80);

        //apply to our cam transform
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        //rotate our player
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * xSensitivity);
    }

    public void Aim()
    {
        Vector3 origin = cam.transform.position;
        Vector3 direction = cam.transform.forward;
        float maxDistance = 4f;

        if(Physics.Raycast(origin, direction, out hit, maxDistance))
        {
            Debug.Log("Hitting something");
            Debug.DrawRay(origin, direction * 10, Color.red);
            target = hit.collider.gameObject;
            // target.Destroy() esto destruiria el chunk entero!
        }
        else {
            Debug.Log("Not hitting anything");
            Debug.DrawRay(origin, direction * 10, Color.green);

        }
    }
}
