#pragma warning disable 4014
using UnityEngine;
using PrimeTween;

public class CameraShaker : Singleton<CameraShaker>
{
	private Camera main_camera;
	private PauseMenuBackgroundEffect bg;

	[Header("Shake magnitude")]
	[SerializeField][Range(0f, 1f)] protected float shake_magnitude = 1f;
	protected float base_shake_magnitude = 0.75f;
	private Setting screenshake_global_setting = null;
	[SerializeField] protected PrimeTween.ShakeSettings settings;

	[Header("Chroma effect")]
	protected float chroma_magnitude_threshold = 0.8f;
	private Setting chroma_thresh_setting = null;
	protected float chroma_duration_factor = 2f;
	private Setting chroma_duration_setting = null;

	[Header("Logs")]
	[SerializeField] protected bool log = false;

	// START
	private void Start()
	{
		main_camera = GetComponent<Camera>();
		bg = GetComponent<PauseMenuBackgroundEffect>();

		// we get the settings & set callbacks
		set_settings();
    }

	// SETTINGS MANAGEMENT
	private void set_settings()
    {
        if (SettingsManager.Instance == null) { return; }

		// screenshake_global_setting
		screenshake_global_setting = SettingsManager.Instance.GetSetting("screenshake_global_setting");
		if (screenshake_global_setting != null)
		{
			screenshake_global_setting.OnValueChanged += set_global_shake;
			set_global_shake(screenshake_global_setting.value);
		}

		// chroma threshold
		chroma_thresh_setting = SettingsManager.Instance.GetSetting("screenshake_chroma_threshold");
		if (chroma_thresh_setting != null)
        {
			chroma_thresh_setting.OnValueChanged += set_chroma_thresh;
			set_chroma_thresh(chroma_thresh_setting.value);
        }

		// chroma duration
		chroma_duration_setting = SettingsManager.Instance.GetSetting("screenshake_chroma_duration_factor");
		if (chroma_duration_setting != null)
		{
			chroma_duration_setting.OnValueChanged += set_chroma_duration;
			set_chroma_duration(chroma_duration_setting.value);
		}
	}
	private void set_global_shake(float shake) { base_shake_magnitude = shake; }
	private void set_chroma_thresh(float thresh) { chroma_magnitude_threshold = thresh; }
	private void set_chroma_duration(float duration) { chroma_duration_factor = duration; }
	private void OnDestroy()
	{
		if (screenshake_global_setting != null) { screenshake_global_setting.OnValueChanged -= set_global_shake; }
		if (chroma_thresh_setting != null) { chroma_thresh_setting.OnValueChanged -= set_chroma_thresh; }
		if (chroma_duration_setting != null) { chroma_duration_setting.OnValueChanged -= set_chroma_duration; }
	}

	// SHAKE
	public async void Shake(float magnitude = 1f)
	{
		// clamp and apply global setting to magnitude
		magnitude *= shake_magnitude * base_shake_magnitude * 2f;
		if (magnitude <= 0f)
		{
			if (log) { Debug.Log("(CameraShaker) tried to shake the screen with " + magnitude + " magnitude, but it was too low"); }
			PostProcessManager.Instance.UpdateChroma();
			return;
		}
		magnitude = Mathf.Clamp(magnitude, 0f, 4f);

		// tween the camera position
		settings.strength.x = magnitude;
		settings.strength.y = magnitude;
		Tween.ShakeLocalPosition(main_camera.transform, settings);

		// "shake" the chroma effect if the magnitude is > chroma_magnitude_threshold
		float chroma_threshold = chroma_magnitude_threshold * base_shake_magnitude;
		if (log) { Debug.Log("(CameraShaker) shaked the screen with " + magnitude + $" magnitude (chroma threshold is {chroma_threshold})");}
		if (magnitude < chroma_threshold) { PostProcessManager.Instance.UpdateChroma(); return; } // update chroma based on perso's life if the threshold is not meet
		
		// we "shake" the chromatic aberration
		PostProcessManager.Instance.TransitionChroma(1f, duration:0f);
		await System.Threading.Tasks.Task.Yield(); // on attend une frame pour que l'effet soit visible
		await PostProcessManager.Instance.TransitionChroma(0f, duration:settings.duration*chroma_duration_factor);
	}
}