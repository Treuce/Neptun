using HtmlAgilityPack;
using RestSharp;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using static Dna.FrameworkDI;
using static Neptun.DI;
using static Neptun.Core.CoreDI;
using System.Windows.Documents;
using System.Windows.Markup;

namespace Neptun
{
	/// <summary>
	/// Interaction logic for MainPage.xaml
	/// </summary>
	public partial class MessagePage : BasePage<MessagesListViewModel>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public MessagePage() : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with specific view model
        /// </summary>
        /// <param name="specificViewModel">The specific view model to use for this page</param>
        public MessagePage(MessagesListViewModel specificViewModel) : base(specificViewModel)
        {
            InitializeComponent();
        }

		#endregion

		#region Override Methods

		/// <summary>
		/// Fired when the view model changes
		/// </summary>
		protected override void OnViewModelChanged()
		{
			// Make sure UI exists first
			//if (ChatMessageList == null)
			return;
		}

		#endregion

		private void TextBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
            e.Handled = true;
            (sender as TextBox).SelectAll();
		}

		private void ContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
            var message = (sender as ContentControl).DataContext as MessageEntry;
			// TODO start loading message here on seperate thread, create window etc
			Task.Run(async () =>
			{
				RestRequest request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx", Method.POST);
				request.AddParameter("__EVENTVALIDATION", message.eventstr);
				request.AddParameter("__VIEWSTATE", message.viewstatestr);
				var id = message.ID;
				request.AddParameter("__EVENTARGUMENT", $"commandname=Subject;commandsource=select;id={id};level=1");
				request.AddHeader("Cache-Control", "no-cache");
				request.AddHeader("X-Requested-With", "XMLHttpRequest");
				request.AddHeader("X-MicrosoftAjax", "Delta=true");
				request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
				request.AddHeader("Accept", "*/*");
				request.AddHeader("Sec-Fetch-Site", "same-origin");
				request.AddHeader("Sec-Fetch-Mode", "cors");
				request.AddHeader("Sec-Fetch-Dest", "empty");
				request.AddParameter("upFilter$rblMessageTypes", "Összes üzenet");
				request.AddParameter("__EVENTTARGET", "upFunction$c_messages$upMain$upGrid$gridMessages");
				IRestResponse response;
				lock (RestWebClient)
				{
					response = RestWebClient.Execute(request);
				}
				HtmlDocument html = new HtmlDocument();
				html.LoadHtml(response.Content);
				var XAMLStr = HtmlToXamlConverter.ConvertHtmlToXaml(html.GetElementbyId("upFunction_c_messages_upModal_upmodalextenderReadMessage_ctl02_Readmessage1_UpdatePanel1").ChildNodes[5].InnerHtml, false);
				await UI.ShowMessage(new MessageViewModel()
				{
					Title = "Üzenet",
					Message = XAMLStr,
					OkText = "Bezárás"
				});
			});
		}
	}
}
