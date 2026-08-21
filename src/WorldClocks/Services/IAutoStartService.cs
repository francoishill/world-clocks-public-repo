namespace WorldClocks.Services;

public interface IAutoStartService
{
    bool IsEnabled();

    void SetEnabled(bool enabled);
}
