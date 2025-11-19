using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuzzy.Models.ViewModel
{
    public class OrderVM
    {
        public OrderHeader orderheader { get; set; }
        public IEnumerable<OrderDetail> orderdetail { get; set; }
     
    }








}
