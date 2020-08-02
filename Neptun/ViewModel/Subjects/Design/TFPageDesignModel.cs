using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptun
{
	public class TFPageDesignModel : TFViewModel
	{
		public static TFPageDesignModel Instance { get; set; } = new TFPageDesignModel();

		public TFPageDesignModel()
		{
			Subjects = new System.Collections.ObjectModel.ObservableCollection<SubjectViewModel>()
			{
				new SubjectViewModel()
				{
					Name = "Test",
					Code = "Impprog"
				},
				new SubjectViewModel()
				{

					Name = "Test2",
					Code = "Java"
				}
			};
		}
	}
}
