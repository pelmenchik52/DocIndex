using DocIndex.Model;

namespace DocIndex.Services
{
    // IThemeService: Контракт для служби, що керує динамічним застосуванням тем у додатку.
    public interface IThemeService
    {
        // Застосовує вказану тему (Light/Dark), змінюючи ResourceDictionary.
        void ApplyTheme(Theme theme);
    }
}
