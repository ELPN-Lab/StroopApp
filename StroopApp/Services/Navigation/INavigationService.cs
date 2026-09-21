using System.Windows.Controls;

namespace StroopApp.Services.Navigation
{
    /// <summary>
    /// Defines contract for page navigation within the application.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates to a page of the specified type with optional parameter.
        /// </summary>
        void NavigateTo<T>(object? parameter = null) where T : Page;

        /// <summary>
        /// Navigates to a page created by the provided factory.
        /// </summary>
        void NavigateTo(Func<Page> pageFactory);
        /// <summary>
        ///  Sets the specified frame as the current navigation context.
        /// </summary>
        /// <remarks>Ensure that the frame is properly initialized before calling this method to avoid
        /// unexpected behavior.</remarks>
        /// <param name="frame">The frame to set as the current context. This parameter cannot be null.</param>
        void SetFrame(Frame frame);

        Type? CurrentPageType { get; }
        event Action<Type?> Navigated;

        bool IsCurrentPage<T>() where T : Page;
    }

}
