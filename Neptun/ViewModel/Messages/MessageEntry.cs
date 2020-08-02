using RestSharp;
using System;
using System.Threading.Tasks;
using static Dna.FrameworkDI;
using static Neptun.Core.CoreDI;
using Dna;
namespace Neptun
{
	public class MessageEntry : IEquatable<MessageEntry>
	{

		public string Sender { get; set; }

		public string Subject { get; set; }

		public string Time { get; set; }

		public string ID { get; set; }

		public bool isRead { get; set; }

		public string eventstr { get; set; }
		public string viewstatestr { get; set; }
		public bool Equals(MessageEntry other)
		{
			return Sender == other.Sender && Subject == other.Subject && Time == other.Time && ID == other.ID;
		}

		public MessageEntry()
		{

		}

		public MessageEntry(string eventvalidator, string viewstate, string onclick, string sender, string subject, string date)
		{
			ID = onclick;
			Sender = sender;
			Subject = subject;
			Time = date;
			var id = ID.Split('(')[1].Split(',')[0].Replace("\'", "");
			eventstr = eventvalidator;
			viewstatestr = viewstate;
		}
	}
}