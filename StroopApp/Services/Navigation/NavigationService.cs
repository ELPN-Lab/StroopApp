using System.Windows.Controls;

namespace StroopApp.Services.Navigation.PageFactory
{
    /// <summary>
    /// Service for WPF Frame-based navigation between pages.
    /// Uses IPageFactory for DI-based page resolution.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IPageFactory _pageFactory;

        private Frame? _frame;

        public Type? CurrentPageType { get; private set; } = typeof(Page);
        public event Action<Type?> Navigated;
        /// <summary>
        /// Initializes NavigationService with a page factory for DI resolution.
        /// The Frame must be set via <see cref="SetFrame"/> before navigation.
        /// </summary>
        /// <param name="pageFactory">Factory for creating pages with injected dependencies.</param>
        public NavigationService(IPageFactory pageFactory)
        {
            _pageFactory = pageFactory ?? throw new ArgumentNullException(nameof(pageFactory));
        }

        /// <summary>
        /// Sets the WPF Frame used for navigation. Must be called before any NavigateTo calls.
        /// </summary>
        /// <param name="frame">The WPF Frame control to navigate within.</param>
        public void SetFrame(Frame frame)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        public bool IsCurrentPage<T>() where T : Page
        {
            if (typeof(T) == CurrentPageType)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Navigates to a page of type T, resolved from the DI container via IPageFactory.
        /// If the page implements INavigationAware, injects this NavigationService automatically.
        /// </summary>
        public void NavigateTo<T>(object? parameter = null) where T : Page
        {
            if (_frame == null)
                throw new InvalidOperationException("Frame has not been set. Call SetFrame() before navigating.");

            var page = _pageFactory.CreatePage<T>();

            if (page is INavigationAware aware)
                aware.NavigationService = this;

            _frame.Navigate(page);

            CurrentPageType = typeof(T);
            Navigated?.Invoke(typeof(T));
        }

        /// <summary>
        /// Navigates to a page created by the provided factory function.
        /// Kept for backward compatibility during migration.
        /// </summary>
        public void NavigateTo(Func<Page> pageFactory)
        {
            if (_frame == null)
                throw new InvalidOperationException("Frame has not been set. Call SetFrame() before navigating.");

            var page = pageFactory();
            _frame.Navigate(page);

            CurrentPageType = page.GetType();
            Navigated?.Invoke(page.GetType());
        }
    }
}
