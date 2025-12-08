using System.Collections.Generic;
using Shopping_Tutorial.Models;

namespace Shopping_Tutorial.Models.ViewModels
{
	public class CompareManyViewModel
	{
		public IList<ProductModel> Products { get; set; } = new List<ProductModel>();
	}
}

