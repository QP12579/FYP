using UnityEngine;
using UnityEngine.InputSystem;

public class PointerController : MonoBehaviour
{
    public LayerMask groundMask;

    private InputActionAsset inputActions;

    private Vector3 screenPos = Vector3.zero;

    private Vector2 padAimValue = Vector2.zero;

    private Player player;

    void Start()
    {
        player = this.GetComponentInParent<Player>();

        if (!player.isLocalPlayer)
        {
            return;
        }

        // Set the initial screen position to the center of the screen
        screenPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        inputActions = this.GetComponentInParent<Player>().GetInputActions();
        
        if (inputActions.bindingMask == InputBinding.MaskByGroup(Constraints.ControlScheme.GamePad))
        {
            Debug.Log($"control Scheme [{InputSystemData.instance.controlScheme}] is active");
        }
        else if (inputActions.bindingMask == InputBinding.MaskByGroup(Constraints.ControlScheme.Keyboard))
        {
            Debug.Log($"control Scheme [{InputSystemData.instance.controlScheme}] is active");
        }
        else
        {
            Debug.LogError($"control Scheme [{InputSystemData.instance.controlScheme}] not found!");
        }
    }

    void Update()
    {
        if (!player.isLocalPlayer)
        {
            return;
        }

        Vector3 mousePosition = GetMouseWorldPosition();
        Vector3 direction = mousePosition - transform.position;
        direction.y = 0;

        if (direction.magnitude > 0)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        }
    }

    private Vector3 GetMouseWorldPosition()
    {

        if (inputActions.bindingMask == InputBinding.MaskByGroup(Constraints.ControlScheme.GamePad))
        {
            // Gamepad control scheme is active
            return GetGamepadAimPosition();
        }
        else if (inputActions.bindingMask == InputBinding.MaskByGroup(Constraints.ControlScheme.Keyboard))
        {
            // Keyboard control scheme is active
            return GetMouseAimPosition();
        }
        else
        {
            return Vector3.zero;
        }
    }

    public void SetGamePadAimPosition(InputAction.CallbackContext context)
    {
        padAimValue = context.ReadValue<Vector2>();

        // Debug.Log($"Gamepad aim value: {padAimValue}");
    }

    public Vector3 GetGamepadAimPosition()
    {
        if (padAimValue != null)
        {
            // Only show the dot if the stick is being moved
            if (padAimValue.sqrMagnitude > 0.01f)
            {
                // Use right stick (aimValue) as a screen position offset from screen center
                Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
                float maxOffsetY = Screen.height / 2f; // Top/bottom edge from center
                float maxOffsetX = Screen.width / 2f;  // Left/right edge from center
                Vector3 aimScreenOffset = new Vector3(padAimValue.x * maxOffsetX, padAimValue.y * maxOffsetY, 0f);
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

    public Vector3 GetMouseAimPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, groundMask))
        {
            return hitInfo.point;
        }
        
        return Vector3.zero;
    }

    private void OnGUI()
    {
        if (player.isLocalPlayer && inputActions.bindingMask == InputBinding.MaskByGroup(Constraints.ControlScheme.GamePad))
        {
            float dotSize = 16f;
            Rect dotRect = new Rect(screenPos.x - dotSize / 2, Screen.height - screenPos.y - dotSize / 2, dotSize, dotSize);

            // Draw white outline
            float outlineSize = 2f;
            Rect outlineRect = new Rect(dotRect.x - outlineSize, dotRect.y - outlineSize, dotRect.width + 2 * outlineSize, dotRect.height + 2 * outlineSize);
            GUI.color = Color.white;
            GUI.DrawTexture(outlineRect, Texture2D.whiteTexture);

            // Draw red dot
            GUI.color = Color.red;
            GUI.DrawTexture(dotRect, Texture2D.whiteTexture);
        }
    }
}
