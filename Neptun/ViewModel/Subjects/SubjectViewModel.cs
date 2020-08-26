using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Neptun
{
	public class SubjectVMComparer : IEqualityComparer<SubjectViewModel>
	{
		public bool Equals(SubjectViewModel x, SubjectViewModel y)
		{
			if (Object.ReferenceEquals(x, y)) return true;

			if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
				return false;

			return x.Name == y.Name && x.Code == y.Code && x.onClick == y.onClick;
		}

		public int GetHashCode(SubjectViewModel obj)
		{
			if (Object.ReferenceEquals(obj, null)) return 0;

			//Get hash code for the Name field if it is not null.
			int hashName = obj.Name == null ? 0 : obj.Name.GetHashCode();

			//Get hash code for the Code field.
			int hashCode = obj.Code.GetHashCode();

			//Calculate the hash code for the product.
			return hashName ^ hashCode;
		}
	}
	public class SubjectViewModel : BaseViewModel
	{
		public SubjectViewModel()
		{

		}
		public SubjectViewModel(SubjectViewModel vm)
		{
			Name = vm.Name;
			Code = vm.Code;
			id = vm.id;
			semester = vm.semester;
			TakeViewModel = vm.TakeViewModel;
			SubjectType = vm.SubjectType;
			type = vm.type;
		}
		public string Name { get; set; }

		public string Code { get; set; }

		public string Credit { get; set; }

		public string onClick { get; set; }

		public string Category { get; set; }
		public bool completed { get; set; }
		public bool taken { get; set; }
		public string id { get; set; }
		public string type { get; set; }

		public bool isSelected { get; set; }

		public TFViewModel.SubjectType SubjectType { get; set; }
		public bool InfoExpanded { get; set; } = false;
		public SolidColorBrush background { get => completed ? Brushes.DarkSeaGreen : (taken ? Brushes.RosyBrown : Brushes.Transparent); }

		public TFViewModel.SemesterViewModel semester { get; set; }
		public TakeSubjectViewModel TakeViewModel { get; set; }
		public string ToolTip { get; set; }

		public bool isPopUpOpen { get; set; }

		public Action Expanded { get; set; } = () => { };
		public Action HasCourses { get; set; } = () => { };
		public Action Collapsed { get; set; } = () => { };

		public bool TFView { get; set; } = true;

	}
	
}
