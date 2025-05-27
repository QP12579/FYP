using UnityEngine;
using UnityEngine.InputSystem;

public class PointerController : MonoBehaviour
{
    public LayerMask groundMask;

    [Header("KeyCode")]
    [SerializeField] private InputActionAsset inputActions;

    private Vector3 screenPos = Vector3.zero;

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
        // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, groundMask))
        // {
        //     return hitInfo.point;
        // }

        // Find the "Aim" action in the inputActions asset
        var aimAction = inputActions != null ? inputActions.FindAction(Constraints.InputKey.Aim) : null;
        if (aimAction != null)
        {
            Vector2 aimValue = aimAction.ReadValue<Vector2>();
            // Only show the dot if the stick is being moved
            if (aimValue.sqrMagnitude > 0.01f)
            {
                // Use right stick (aimValue) as a screen position offset from screen center
                Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
                float maxOffsetY = Screen.height / 2f; // Top/bottom edge from center
                float maxOffsetX = Screen.width / 2f;  // Left/right edge from center
                Vector3 aimScreenOffset = new Vector3(aimValue.x * maxOffsetX, aimValue.y * maxOffsetY, 0f);
                screenPos = screenCenter + aimScreenOffset;

                Ray ray = Camera.main.ScreenPointToRay(screenPos);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, groundMask))
                {
                    return hitInfo.point;
                }
            }
        }
        
        return Vector3.zero;
    }

    private void OnGUI()
    {
        float dotSize = 16f;
        Rect dotRect = new Rect(screenPos.x - dotSize / 2, Screen.height - screenPos.y - dotSize / 2, dotSize, dotSize);
        
        // Save previous GUI color
        Color prevColor = GUI.color;

        // Draw white outline
        float outlineSize = 2f;
        Rect outlineRect = new Rect(dotRect.x - outlineSize, dotRect.y - outlineSize, dotRect.width + 2 * outlineSize, dotRect.height + 2 * outlineSize);
        GUI.color = Color.white;
        GUI.DrawTexture(outlineRect, Texture2D.whiteTexture);

        // Draw red dot
        GUI.color = Color.red;
        GUI.DrawTexture(dotRect, Texture2D.whiteTexture);

        GUI.color = prevColor;
    }
}
