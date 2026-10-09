using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore_Topoev
{
    public partial class PickupPoints
    {
        public string FullAddress
        {
            get
            {
                var parts = new[] { PostalCode, City, Street, House }
                    .Where(p => !string.IsNullOrWhiteSpace(p));
                return string.Join(", ", parts);
            }
        }
    }
}
