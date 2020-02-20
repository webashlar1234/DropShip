using DropshipPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.Models
{
    public class CategoryData : Category
    {
       public string categoryFullPath { get; set; }
    }
    
}
