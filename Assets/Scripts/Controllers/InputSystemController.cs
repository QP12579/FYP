using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InputSystemController : MonoBehaviour
{
    [SerializeField] private Button GamePadButton;
    [SerializeField] private Button KeyboardButton;

    private void Start() 
    {
        Time.timeScale = 1f; // Ensure time scale is normal
        OnGamePadSelected();
    }
    
    public void OnGamePadSelected()
    {
        // Switch to gamepad control scheme
        InputSystemData.instance.controlScheme = Constraints.ControlScheme.GamePad;        
        // Update button states
        GamePadButton.interactable = false;
        KeyboardButton.interactable = true;
    }

    public void OnKeyboardSelected()
    {
        // Switch to keyboard control scheme
        InputSystemData.instance.controlScheme = Constraints.ControlScheme.Keyboard;
        
        // Update button states
        GamePadButton.interactable = true;
        KeyboardButton.interactable = false;
    }
    
}
