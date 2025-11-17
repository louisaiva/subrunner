#pragma warning disable 4014
using UnityEngine;
using PrimeTween;

public class CameraShaker : Singleton<CameraShaker>
{
	private Camera main_camera;
	private PauseMenuBackgroundEffect bg;

	[Header("Shake magnitude")]
	[SerializeField][Range(0f, 1f)] protected float shake_magnitude = 1f;
	protected const float base_shake_magnitude = 0.75f;
	[SerializeField] protected ShakeSettings settings;

	[Header("Chroma effect")]
	[SerializeField][Range(0f, 2f)] protected float chroma_magnitude_threshold = 0.8f;
	[SerializeField][Range(0f, 8f)] protected float chroma_duration_factor = 2f;

	[Header("Logs")]
	[SerializeField] protected bool log = false;

	private void Start()
	{
		main_camera = GetComponent<Camera>();
		bg = GetComponent<PauseMenuBackgroundEffect>();
    }

	public async void Shake(float magnitude = 1f)
	{
		// clamp and apply global setting to magnitude
		magnitude *= shake_magnitude * base_shake_magnitude;
		if (magnitude <= 0f)
		{
			if (log) { Debug.Log("(CameraShaker) tried to shake the screen with " + magnitude + " magnitude, but it was too low"); }
			return;
		}
		magnitude = Mathf.Clamp(magnitude, 0f, 2f);

		// tween the camera position
		settings.strength.x = magnitude;
		settings.strength.y = magnitude;
		Tween.ShakeLocalPosition(main_camera.transform, settings);
		if (log) { Debug.Log("(CameraShaker) shaked the screen with " + magnitude + " magnitude");}

		

		// "shake" the chroma effect if the magnitude is > chroma_magnitude_threshold
		if (magnitude < chroma_magnitude_threshold) { return; }
		bg.TransitionEffect(show: true, duration:0f,bloom_effect:false);
		await System.Threading.Tasks.Task.Yield(); // on attend une frame pour que l'effet soit visible
		await bg.TransitionEffect(show: false, duration:settings.duration*chroma_duration_factor,bloom_effect:false);
	}
}