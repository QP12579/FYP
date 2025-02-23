using UnityEngine;

public class PointerController : MonoBehaviour
{
    public LayerMask groundMask;
    void Update()
    {
        Vector3 mousePosition = GetMouseWorldPosition();

        Vector3 direction = mousePosition - transform.position;
        direction.y = 0;

        if (direction.magnitude > 0)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        }
            /*if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("direction: "+ direction);
        }*/
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, groundMask))
        {
            return hitInfo.point;
        }
        return Vector3.zero;
    }
}
