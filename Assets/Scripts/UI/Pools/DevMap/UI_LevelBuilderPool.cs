using System.Collections;

public class UI_LevelBuilderPool : UI_SlottablePool
{
    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        yield return base.enable_coroutine();
        LevelBuilder.StaticInstance?.gameObject.SetActive(true);
    }
    protected override IEnumerator disable_coroutine()
    {
        LevelBuilder.StaticInstance?.gameObject.SetActive(false);
        yield return base.disable_coroutine();
    }
}