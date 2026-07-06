#nullable enable
#if UNITY_EDITOR

using System.Globalization;
using System.IO;

using UnityEditor;

using UnityEngine;
using UnityEngine.InputSystem;

namespace My.Common.Scripts.Editor
{
  /// <summary>
  /// A window in the scene view that shows the current world coordinates of the mouse cursor in 2D mode.
  /// </summary>
  /// <remarks>
  /// Creates some assets in the project for the GUI skin and the window background texture.
  /// </remarks>
  [InitializeOnLoad]
  public class MouseWorldSceneWindow
  {
    private static Vector3 _currentMouseWorldPosition;
    public static Vector3 CurrentMouseWorldPositionInSceneView => _currentMouseWorldPosition;
    private static GUISkin? _customSkin;
    private static Vector2Int _formerMouseScreenPosition;
    private static bool _isWindowEnabled;

    private static double _nextUpdateTime;
    private static Rect _windowRect = new(0, 0, 150, 20);

    private const string BaseAssetPath = @"Assets\My\Common\Resources\UI\Styles\" + MouseWorldSceneWindow.MouseWorldSceneWindowName;
    private static readonly Vector2 DefaultPosition = new(50f, 10f);
    private const string GuiSkinAssetPath = MouseWorldSceneWindow.BaseAssetPath + @"\GUISkin.asset";
    private const string MouseWorldSceneWindowName = nameof(MouseWorldSceneWindow);
    private const string ResetSceneWindowMenuName = "Tools/SceneView/Reset " + MouseWorldSceneWindow.MouseWorldSceneWindowName;
    private const string ToggleSceneWindowMenuName = "Tools/SceneView/Toggle " + MouseWorldSceneWindow.MouseWorldSceneWindowName;

    /// <summary>
    /// The update frequency of the mouse world position.
    /// The divisor defines the update times per second.
    /// </summary>
    private const double UpdateFrequency = 1 / 30d;

    private const string WindowBackgroundTextureAssetPath = MouseWorldSceneWindow.BaseAssetPath + @"\T_WindowBackground.asset";
    private const string WindowEnabledEditorPrefsName = MouseWorldSceneWindow.MouseWorldSceneWindowName + "/WindowEnabled";
    private const string WindowPositionXEditorPrefsName = MouseWorldSceneWindow.MouseWorldSceneWindowName + "/SceneWindowPositionX";
    private const string WindowPositionYEditorPrefsName = MouseWorldSceneWindow.MouseWorldSceneWindowName + "/SceneWindowPositionY";

    /// <summary>
    /// Static constructor that initializes the window.
    /// </summary>
    static MouseWorldSceneWindow()
    {
      // Load preferences
      var desiredWorkingState = EditorPrefs.GetBool(MouseWorldSceneWindow.WindowEnabledEditorPrefsName, true);
      MouseWorldSceneWindow._windowRect.x = EditorPrefs.GetFloat(MouseWorldSceneWindow.WindowPositionXEditorPrefsName, MouseWorldSceneWindow.DefaultPosition.x);
      MouseWorldSceneWindow._windowRect.y = EditorPrefs.GetFloat(MouseWorldSceneWindow.WindowPositionYEditorPrefsName, MouseWorldSceneWindow.DefaultPosition.y);

      MouseWorldSceneWindow.CreateGUISkin();

      // Toggle the working state
      MouseWorldSceneWindow.ToggleEnabledState(desiredWorkingState);
    }

    /// <summary>
    /// Create the GUI skin for the scene window.
    /// </summary>
    private static void CreateGUISkin()
    {
      MouseWorldSceneWindow._customSkin = AssetDatabase.LoadAssetAtPath<GUISkin>(MouseWorldSceneWindow.GuiSkinAssetPath);

      // Because of skinning does not seem to work when called at first, do it a few times
      if (!MouseWorldSceneWindow._customSkin || MouseWorldSceneWindow._customSkin == null)
      {
        Color semiTransparentBackground = new Color32(0, 0, 0, 160);
        Color textColor = new Color32(224, 224, 224, 255);

        var windowTexture = MouseWorldSceneWindow.CreateWindowBackgroundTexture((int)MouseWorldSceneWindow._windowRect.width, (int)MouseWorldSceneWindow._windowRect.height,
          semiTransparentBackground, 5);

        var customSkin = ScriptableObject.CreateInstance<GUISkin>();
        customSkin.window.normal.background = windowTexture;
        customSkin.window.onNormal.background = windowTexture;
        customSkin.window.focused.background = windowTexture;
        customSkin.window.onFocused.background = windowTexture;

        var labelStyle = new GUIStyle();
        labelStyle.fontSize = 16;
        labelStyle.normal.textColor = textColor;
        labelStyle.onFocused.textColor = textColor;
        labelStyle.hover.textColor = textColor;
        customSkin.label = labelStyle;

        MouseWorldSceneWindow._customSkin = customSkin;

        MouseWorldSceneWindow.EnsureFolder(MouseWorldSceneWindow.BaseAssetPath);
        AssetDatabase.CreateAsset(windowTexture, MouseWorldSceneWindow.WindowBackgroundTextureAssetPath);
        AssetDatabase.CreateAsset(customSkin, MouseWorldSceneWindow.GuiSkinAssetPath);
      }
    }

    /// <summary>
    /// Creates the background texture for the window.
    /// </summary>
    private static Texture2D CreateWindowBackgroundTexture(int width, int height, Color col, int borderRadius)
    {
      var tex = new Texture2D(width, height);
      var colors = new Color[width * height];
      var cornerReferencePoints = new[]
      {
        new Vector2(borderRadius, borderRadius),
        new Vector2(width - borderRadius, borderRadius),
        new Vector2(borderRadius, height - borderRadius),
        new Vector2(width - borderRadius, height - borderRadius),
      };

      for (var y = 0; y < height; y++)
      {
        for (var x = 0; x < width; x++)
        {
          bool isBorder;

          var coord = new Vector2(x, y);

          if (coord.x < cornerReferencePoints[0].x && coord.y < cornerReferencePoints[0].y)
          {
            isBorder = Vector2.Distance(coord, cornerReferencePoints[0]) > borderRadius;
          }
          else if (coord.x > cornerReferencePoints[1].x && coord.y < cornerReferencePoints[1].y)
          {
            isBorder = Vector2.Distance(coord, cornerReferencePoints[1]) > borderRadius;
          }
          else if (coord.x < cornerReferencePoints[2].x && coord.y > cornerReferencePoints[2].y)
          {
            isBorder = Vector2.Distance(coord, cornerReferencePoints[2]) > borderRadius;
          }
          else if (coord.x > cornerReferencePoints[3].x && coord.y > cornerReferencePoints[3].y)
          {
            isBorder = Vector2.Distance(coord, cornerReferencePoints[3]) > borderRadius;
          }
          else
          {
            isBorder = false;
          }

          colors[y * width + x] = isBorder ? Color.clear : col;
        }
      }

      tex.SetPixels(colors);
      tex.Apply();

      return tex;
    }

    private static void DrawWindow(int windowID)
    {
      // Show the mouse world position
      const string FormatString = "F2";
      GUILayout.Space(3);
      GUILayout.BeginHorizontal();
      GUILayout.Space(5);
      GUILayout.Label(MouseWorldSceneWindow._currentMouseWorldPosition.x.ToString(FormatString, CultureInfo.CurrentCulture), GUILayout.Width(60));
      GUILayout.Space(5);
      GUILayout.Label("/", GUILayout.Width(5));
      GUILayout.Space(5);
      GUILayout.Label(MouseWorldSceneWindow._currentMouseWorldPosition.y.ToString(FormatString, CultureInfo.CurrentCulture));
      GUILayout.EndHorizontal();
      GUILayout.Space(3);

      // Make the window draggable
      GUI.DragWindow();
    }

    /// <summary>
    /// Ensure the specified folder exists.
    /// </summary>
    /// <param name="folderPath">The folder to be checked for existence. When not existing, it will be created.</param>
    /// <returns>True when the folder has just been created.</returns>
    private static bool EnsureFolder(string folderPath)
    {
      if (!AssetDatabase.IsValidFolder(folderPath))
      {
        string[] directories = folderPath.Split(Path.DirectorySeparatorChar);
        var currentDirectory = "";

        foreach (var directory in directories)
        {
          currentDirectory = Path.Combine(currentDirectory, directory);

          if (!AssetDatabase.IsValidFolder(currentDirectory))
          {
            AssetDatabase.CreateFolder(Path.GetDirectoryName(currentDirectory), Path.GetFileName(currentDirectory));
          }
        }

        return true;
      }

      return false;
    }

    /// <summary>
    /// Draws the scene view window.
    /// </summary>
    private static void OnSceneGUI(SceneView sceneView)
    {
      if (MouseWorldSceneWindow._isWindowEnabled)
      {
        // Draw the scene window
        Handles.BeginGUI();
        GUI.skin = MouseWorldSceneWindow._customSkin;
        MouseWorldSceneWindow._windowRect =
          GUILayout.Window(0, MouseWorldSceneWindow._windowRect, MouseWorldSceneWindow.DrawWindow, string.Empty, MouseWorldSceneWindow._customSkin?.window);
        GUI.skin = null;
        Handles.EndGUI();

        // Save the window positions
        EditorPrefs.SetFloat(MouseWorldSceneWindow.WindowPositionXEditorPrefsName, MouseWorldSceneWindow._windowRect.x);
        EditorPrefs.SetFloat(MouseWorldSceneWindow.WindowPositionYEditorPrefsName, MouseWorldSceneWindow._windowRect.y);
      }
    }

    /// <summary>
    /// Resets the scene view window to its default position.
    /// </summary>
    [MenuItem(MouseWorldSceneWindow.ResetSceneWindowMenuName)]
    public static void ResetWindowPosition()
    {
      MouseWorldSceneWindow._windowRect.x = MouseWorldSceneWindow.DefaultPosition.x;
      MouseWorldSceneWindow._windowRect.y = MouseWorldSceneWindow.DefaultPosition.y;

      EditorPrefs.SetFloat(MouseWorldSceneWindow.WindowPositionXEditorPrefsName, MouseWorldSceneWindow._windowRect.x);
      EditorPrefs.SetFloat(MouseWorldSceneWindow.WindowPositionYEditorPrefsName, MouseWorldSceneWindow._windowRect.y);
    }

    private static void ToggleEnabledState(bool isEnabled)
    {
      // When being enabled from disabled
      if (isEnabled && !MouseWorldSceneWindow._isWindowEnabled)
      {
        // Register events
        SceneView.duringSceneGui += MouseWorldSceneWindow.OnSceneGUI;
        EditorApplication.update += MouseWorldSceneWindow.Update;
      }
      // When being disabled from enabled
      else if (!isEnabled && MouseWorldSceneWindow._isWindowEnabled)
      {
        // Unregister events
        SceneView.duringSceneGui -= MouseWorldSceneWindow.OnSceneGUI;
        EditorApplication.update -= MouseWorldSceneWindow.Update;
      }

      EditorPrefs.SetBool(MouseWorldSceneWindow.WindowEnabledEditorPrefsName, isEnabled);
      Menu.SetChecked(MouseWorldSceneWindow.ToggleSceneWindowMenuName, isEnabled);
      MouseWorldSceneWindow._isWindowEnabled = isEnabled;
    }

    /// <summary>
    /// Shows the scene view window.
    /// </summary>
    [MenuItem(MouseWorldSceneWindow.ToggleSceneWindowMenuName)]
    public static void ToggleWindowVisibility()
    {
      var isNowEnabled = !MouseWorldSceneWindow._isWindowEnabled;

      // Toggle the working state
      MouseWorldSceneWindow.ToggleEnabledState(isNowEnabled);
    }

    /// <summary>
    /// Updates the scene view window data.
    /// </summary>
    private static void Update()
    {
      // Update mouse position during design-time and play mode
      // (But only throttled considering the update frequency)
      if (EditorApplication.timeSinceStartup >= MouseWorldSceneWindow._nextUpdateTime)
      {
        MouseWorldSceneWindow._nextUpdateTime = EditorApplication.timeSinceStartup + MouseWorldSceneWindow.UpdateFrequency;
        MouseWorldSceneWindow.UpdateCurrentMouseWorldPosition();
      }
    }

    /// <summary>
    /// Update the mouse world position.
    /// </summary>
    private static void UpdateCurrentMouseWorldPosition()
    {
      // Gets the current scene view and scene view camera while design-time and play mode
      var sceneView = SceneView.sceneViews.Count > 0 ? (SceneView)SceneView.sceneViews[0] : null;
      var sceneViewCamera = sceneView?.camera;

      // Determine current mouse position with new input system
      var mouseScreenPosition = Mouse.current.position.ReadValue();
      var integerMouseScreenPosition = new Vector2Int((int)mouseScreenPosition.x, (int)mouseScreenPosition.y);

      if (sceneView != null && sceneViewCamera != null
          // Do no update in case of unchanged mouse screen position
          && integerMouseScreenPosition != MouseWorldSceneWindow._formerMouseScreenPosition)
      {
        MouseWorldSceneWindow._formerMouseScreenPosition = integerMouseScreenPosition;

        // Determine offsets and bars of the scene view
        var sceneViewTopBarsHeight = sceneView.rootVisualElement.worldBound.y;
        var sceneViewAbsoluteScreenOffset = sceneView.position.min;

        // Check whether mouse screen position is within the scene view
        var adjustedMouseScreenPosition = integerMouseScreenPosition - new Vector2Int((int)sceneViewAbsoluteScreenOffset.x, (int)sceneViewAbsoluteScreenOffset.y) -
          new Vector2Int(0, (int)sceneViewTopBarsHeight);
        var isMouseWithinSceneView = adjustedMouseScreenPosition is { x: >= 0, y: >= 0 }
          && adjustedMouseScreenPosition.x < sceneViewCamera.pixelWidth && adjustedMouseScreenPosition.y < sceneViewCamera.pixelHeight;

        if (isMouseWithinSceneView)
        {
          // Adjust y so it takes the top bars of the scene view into account and flip y-axis (cause of different coordinate systems)
          // and also adjust x/y regarding the scene view offset
          // (This is the correct code when calling from Update)
          mouseScreenPosition.y = sceneViewCamera.pixelHeight - (mouseScreenPosition.y - sceneViewTopBarsHeight - sceneViewAbsoluteScreenOffset.y);
          mouseScreenPosition.x -= sceneViewAbsoluteScreenOffset.x;

          // Adjust y so it takes the top bars of the scene view into account and flip y-axis (cause of different coordinate systems)
          // (This would be the correct code when calling from OnSceneGUI)
          //mouseScreenPosition.y = sceneViewCamera.pixelHeight - (mouseScreenPosition.y - sceneViewTopBarsHeight);

          MouseWorldSceneWindow._currentMouseWorldPosition = sceneViewCamera.ScreenToWorldPoint(mouseScreenPosition);

          // Force repaint the scene view, so the scene window is also repainted with the new data
          // This introduce some CPU load
          SceneView.RepaintAll();
        }
      }
    }
  }
}
#endif