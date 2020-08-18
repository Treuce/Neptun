using Dna;
using GalaSoft.MvvmLight.Command;
using HtmlAgilityPack;
using Neptun.Core;
using RestSharp;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.WebSockets;
using System.Windows;
using System.Windows.Input;
using static Dna.FrameworkDI;
using static Neptun.Core.CoreDI;
using static Neptun.DI;
namespace Neptun
{
	/// <summary>
	/// The View Model for the custom flat window
	/// </summary>
	public class WindowViewModel : BaseViewModel
	{
		#region Private Member

		/// <summary>
		/// The window this view model controls
		/// </summary>
		protected Window mWindow;

		/// <summary>
		/// The window resizer helper that keeps the window size correct in various states
		/// </summary>
		private WindowResizer mWindowResizer;

		/// <summary>
		/// The margin around the window to allow for a drop shadow
		/// </summary>
		private Thickness mOuterMarginSize = new Thickness(5);

		/// <summary>
		/// The radius of the edges of the window
		/// </summary>
		private int mWindowRadius = 10;

		/// <summary>
		/// The last known dock position
		/// </summary>
		private WindowDockPosition mDockPosition = WindowDockPosition.Undocked;

		#endregion

		#region Public Properties

		/// <summary>
		/// The smallest width the window can go to
		/// </summary>
		public double WindowMinimumWidth { get; set; } = 1400;

		/// <summary>
		/// The smallest height the window can go to
		/// </summary>
		public double WindowMinimumHeight { get; set; } = 700;

		/// <summary>
		/// True if the window is currently being moved/dragged
		/// </summary>
		public bool BeingMoved { get; set; }


		/// <summary>
		/// True if the window should be borderless because it is docked or maximized
		/// </summary>
		public bool Borderless => (mWindow.WindowState == WindowState.Maximized || mDockPosition != WindowDockPosition.Undocked);

		/// <summary>
		/// The size of the resize border around the window
		/// </summary>
		public int ResizeBorder => mWindow.WindowState == WindowState.Maximized ? 0 : 4;

		/// <summary>
		/// The size of the resize border around the window, taking into account the outer margin
		/// </summary>
		public Thickness ResizeBorderThickness => new Thickness(OuterMarginSize.Left + ResizeBorder,
																OuterMarginSize.Top + ResizeBorder,
																OuterMarginSize.Right + ResizeBorder,
																OuterMarginSize.Bottom + ResizeBorder);

		/// <summary>
		/// The padding of the inner content of the main window
		/// </summary>
		public Thickness InnerContentPadding { get; set; } = new Thickness(0);

		/// <summary>
		/// The margin around the window to allow for a drop shadow
		/// </summary>
		public Thickness OuterMarginSize
		{
			// If it is maximized or docked, no border
			get => mWindow.WindowState == WindowState.Maximized ? mWindowResizer.CurrentMonitorMargin : (Borderless ? new Thickness(0) : mOuterMarginSize);
			set => mOuterMarginSize = value;
		}

		/// <summary>
		/// The radius of the edges of the window
		/// </summary>
		public int WindowRadius
		{
			// If it is maximized or docked, no border
			get => Borderless ? 0 : mWindowRadius;
			set => mWindowRadius = value;
		}

		/// <summary>
		/// The rectangle border around the window when docked
		/// </summary>
		public int FlatBorderThickness => Borderless && mWindow.WindowState != WindowState.Maximized ? 1 : 0;

		/// <summary>
		/// The radius of the edges of the window
		/// </summary>
		public CornerRadius WindowCornerRadius => new CornerRadius(WindowRadius);

		/// <summary>
		/// The height of the title bar / caption of the window
		/// </summary>
		public int TitleHeight { get; set; } = 42;
		/// <summary>
		/// The height of the title bar / caption of the window
		/// </summary>
		public GridLength TitleHeightGridLength => new GridLength(TitleHeight + ResizeBorder);

		/// <summary>
		/// True if we should have a dimmed overlay on the window
		/// such as when a popup is visible or the window is not focused
		/// </summary>
		public bool DimmableOverlayVisible { get; set; }

		#endregion

		#region Commands

		/// <summary>
		/// The command to minimize the window
		/// </summary>
		public ICommand MinimizeCommand { get; set; }

		/// <summary>
		/// The command to maximize the window
		/// </summary>
		public ICommand MaximizeCommand { get; set; }

		/// <summary>
		/// The command to close the window
		/// </summary>
		public ICommand CloseCommand { get; set; }

		/// <summary>
		/// The command to show the system menu of the window
		/// </summary>
		public ICommand MenuCommand { get; set; }

		public ICommand LogOutCommand { get; set; }

		public ICommand SwitchPageCommand { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		public WindowViewModel(Window window)
		{
			mWindow = window;

			// Listen out for the window resizing
			mWindow.StateChanged += (sender, e) =>
			{
				// Fire off events for all properties that are affected by a resize
				WindowResized();
			};

			// Create commands
			MinimizeCommand = new RelayCommand(() => mWindow.WindowState = WindowState.Minimized);
			MaximizeCommand = new RelayCommand(() => mWindow.WindowState ^= WindowState.Maximized);
			//CloseCommand = new RelayCommand(() => mWindow.Close());
			CloseCommand = new RelayCommand(() =>
			{
				LogOutCommand.Execute(null);
				Application.Current.Shutdown();
			});
			MenuCommand = new RelayCommand(() => SystemCommands.ShowSystemMenu(mWindow, GetMousePosition()));
			LogOutCommand = new RelayCommand(() =>
			{
				try
				{
					var request = new RestRequest("main.aspx/LogOutFromJS", Method.POST);
					request.Body = new RequestBody("ContentType = \"application/json\"", "application/json", "{\"link\": \"Login.aspx?timeout=\"}");
					request.Timeout = 100;
					CoreDI.RestWebClient.Execute(request);
				}
				catch (Exception e)
				{
					Logger.LogInformationSource(e.Message);
				}
				finally
				{
					CoreDI.RestWebClient.CookieContainer = new System.Net.CookieContainer();
					DI.ViewModelApplication.GoToPage(ApplicationPage.Login);
					DI.ViewModelApplication.isLoggedIn = false;
				}
				//TODO: ??? Clear login details saved?
				// TODO: If logged out, load saved stuff on login page
				//DI.ClientDataStore.ClearAllLoginCredentialsAsync();
			});

			SwitchPageCommand = new RelayCommand<MainMenuSubEntryViewModel>(vm =>
			{
				TaskManager.RunAndForget(() =>
				{
					var pagerequest = new RestRequest(Configuration["NeptunServer:HostUrl"] + vm.LinksTo, Method.POST);
					var parameters = new Uri(Configuration["NeptunServer:HostUrl"] + vm.LinksTo).Query.TrimStart('?').Split(new[] { '&', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(parameter => parameter.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries))
							.GroupBy(parts => parts[0],
									 parts => parts.Length > 2 ? string.Join("=", parts, 1, parts.Length - 1) : (parts.Length > 1 ? parts[1] : ""))
							.ToDictionary(grouping => grouping.Key,
										  grouping => string.Join(",", grouping));

					//foreach (var param in parameters)
					//	pagerequest.AddParameter(param.Key, param.Value);
					//Debugger.Break();
					//string HTMLPage;
					//lock (RestWebClient)
					//{
					//	HTMLPage = RestWebClient.Execute(pagerequest).Content;
					//}

					switch (parameters["ctrl"])
					{
						case "0303":
							ViewModelApplication.GoToPage(ApplicationPage.Tárgyfelvétel, new TFViewModel());
							break;
						default:
							ViewModelApplication.GoToPage(ApplicationPage.Default);
							break;

					}
					//Debugger.Break();
				});
			});

			// Fix window resize issue
			mWindowResizer = new WindowResizer(mWindow);

			// Listen out for dock changes
			mWindowResizer.WindowDockChanged += (dock) =>
			{
				// Store last position
				mDockPosition = dock;

				// Fire off resize events
				WindowResized();
			};

			// On window being moved/dragged
			mWindowResizer.WindowStartedMove += () =>
			{
				// Update being moved flag
				BeingMoved = true;
			};

			// Fix dropping an undocked window at top which should be positioned at the
			// very top of screen
			mWindowResizer.WindowFinishedMove += () =>
			{
				// Update being moved flag
				BeingMoved = false;

				// Check for moved to top of window and not at an edge
				if (mDockPosition == WindowDockPosition.Undocked && mWindow.Top == mWindowResizer.CurrentScreenSize.Top)
					// If so, move it to the true top (the border size)
					mWindow.Top = -OuterMarginSize.Top;
			};
		}

		#endregion

		#region Private Helpers

		/// <summary>
		/// Gets the current mouse position on the screen
		/// </summary>
		/// <returns></returns>
		private Point GetMousePosition()
		{
			return mWindowResizer.GetCursorPosition();
		}

		/// <summary>
		/// If the window resizes to a special position (docked or maximized)
		/// this will update all required property change events to set the borders and radius values
		/// </summary>
		private void WindowResized()
		{
			// Fire off events for all properties that are affected by a resize
			OnPropertyChanged(nameof(Borderless));
			OnPropertyChanged(nameof(FlatBorderThickness));
			OnPropertyChanged(nameof(ResizeBorderThickness));
			OnPropertyChanged(nameof(OuterMarginSize));
			OnPropertyChanged(nameof(WindowRadius));
			OnPropertyChanged(nameof(WindowCornerRadius));
		}


		#endregion
	}
}
