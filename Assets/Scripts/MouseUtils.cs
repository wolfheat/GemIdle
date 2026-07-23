using UnityEngine;
using UnityEngine.InputSystem;

public static class MouseUtils
{   
    public static Vector2 MousePosition => Mouse.current.position.ReadValue();
}
