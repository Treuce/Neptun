using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Neptun
{
	/// <summary>
	/// The View Model for the starting page, only the messages
	/// </summary>
	public class MessagesListDesignModel : MessagesListViewModel
	{
		public static MessagesListDesignModel Instance { get; set; } = new MessagesListDesignModel();
		public MessagesListDesignModel() : base()
		{

			Messages.Add(
				new MessageEntry()
				{
					Subject = "testing",
					Sender = "test",
					Time = "1653.5.1"
				});
			for (int i = 0; i < 100; ++i)
				AllMessages.Add(new MessageEntry()
				{
					Subject = "testing",
					Sender = "test",
					Time = "1653.5.1"
				});
		}
	}
}
