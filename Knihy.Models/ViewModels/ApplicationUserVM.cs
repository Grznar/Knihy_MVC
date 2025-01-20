using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knihy.Models.ViewModels
{
    public class ApplicationUserVM
    {
        public ApplicationUser ApplicationUser { get; set; }
        public IEnumerable<SelectListItem> RoleList   { get; set; }
        public IEnumerable<SelectListItem> CompanyList { get; set; }
        public string ActiveRole { get; set; }
    }
}
