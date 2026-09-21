using System;
using System.Windows.Controls;

using StroopApp.Services.Navigation;
using StroopApp.Views.Home;

namespace StroopApp.XUnitTests.TestDummies
{
	public class DummyNavigationService : INavigationService
	{
		public bool Navigated;

        public Type? CurrentPageType => typeof(HomePage);

        event Action<Type?> INavigationService.Navigated
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public bool IsCurrentPage<T>() where T : Page
        {
            return typeof(T) == CurrentPageType;
        }

        void INavigationService.NavigateTo<T>(object parameter)
		{
			Navigated = true;
		}
		void INavigationService.NavigateTo(Func<Page> pageFactory)
		{
			Navigated = true;
		}
		void INavigationService.SetFrame(Frame frame)
		{
		}
	}
}
