using UnityEngine;
using System.Collections.Generic;
using PrimeTween;

public class CameraShaker : Singleton<CameraShaker>
{
	private Camera main_camera;

    [Header("Shake magnitude")]
	[SerializeField][Range(0f, 1f)] protected float shake_magnitude = 1f;

	[Header("Shake low settings")]
	[SerializeField] protected ShakeSettings settings;

	[Header("Logs")]
	[SerializeField] protected bool log = false;

	private void Start()
	{
		main_camera = GetComponent<Camera>();
    }

	public void Shake(float magnitude = 1f)
	{
		// clamp and apply global setting to magnitude
		magnitude *= shake_magnitude;
		magnitude = Mathf.Clamp(magnitude, 0f, 2f);

		// tween the camera position
		settings.strength.x = magnitude;
		settings.strength.y = magnitude;
		Tween.ShakeLocalPosition(main_camera.transform, settings);
		if (log) { Debug.Log("(CameraShaker) shaked the screen with " + magnitude + " magnitude");}
	}
}