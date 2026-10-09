using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ShoeStore_Topoev
{
    public partial class Products
    {
        public bool is_discount => (CurrentDiscount ?? 0) > 15;
        public bool is_on_sale => (CurrentDiscount ?? 0) > 0;
        public bool is_quantity => QuantityInStock <= 0;
        public decimal? price_discount => Price - (Price * (CurrentDiscount ?? 0) / 100);

        public BitmapImage PhotoImage
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Photo))
                    return GetPlaceholder();

                try
                {
                    string fullPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory, "ProductImages", Photo);

                    if (!File.Exists(fullPath))
                        return GetPlaceholder();

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bitmap.UriSource = new Uri(fullPath);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
                catch
                {
                    return GetPlaceholder();
                }
            }
        }

        private BitmapImage GetPlaceholder()
        {
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri("pack://application:,,,/picture.png");
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
        }
    }
}
