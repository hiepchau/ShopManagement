﻿using MongoDB.Driver;
using SE104_OnlineShopManagement.Commands;
using SE104_OnlineShopManagement.Models.ModelEntity;
using SE104_OnlineShopManagement.Network.Get_database;
using SE104_OnlineShopManagement.ViewModels.FunctionViewModel;
using SE104_OnlineShopManagement.ViewModels.FunctionViewModel.Detail_Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SE104_OnlineShopManagement.ViewModels.ComponentViewModel
{
    public class ProductsControlViewModel : ViewModelBase
    {
        #region Properties
        public ProductsInformation product { get; set; }
        public string ID { get; private set; }
        public string name { get; set; }
        public int quantity { get; set; }
        public string price { get; set; }
        public string StockCost { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public string displayID { get; set; }
        public bool isActivated { get; set; }
        public string Producer { get; set; }
        private IUpdateProductList _parent;
        #endregion

        #region ICommand
        public ICommand EditProductsCommand { get; set; }
        public ICommand DeleteProductsCommand { get; set; }
        #endregion

        public ProductsControlViewModel(ProductsInformation products, IUpdateProductList parent)
        {
            this.product = products;
            ID = product.ID;
            name = product.name;
            quantity = product.quantity;
            price = SeparateThousands(product.price.ToString());
            StockCost = SeparateThousands(product.StockCost.ToString());
            Unit = product.Unit;
            displayID = product.displayID;
            isActivated = product.isActivated;
            Producer = product.ProducerInformation;
            _parent = parent;
            GetTypeName();
            EditProductsCommand = new RelayCommand<Object>(null, EditProduct);
            DeleteProductsCommand = new RelayCommand<Object>(null, DeleteProduct);
        }

        #region Function
        public void DeleteProduct(Object o)
        {
            _parent.UpdateProductList(product);
        }
        public void EditProduct(Object o)
        {
            _parent.EditProduct(product);
        }

        public async void GetTypeName()
        {
            var filter = Builders<ProductTypeInfomation>.Filter.Eq(x => x.ID, product.Category);
            GetProductType getter = new GetProductType((_parent as BaseFunction).Connect.client, (_parent as BaseFunction).Session, filter);
            var ls = await getter.Get();
            if(ls != null && ls.Count > 0)
            {
                Category = ls.First().name;
                OnPropertyChanged(nameof(Category));
            }
            else
            {
                return;
            }
        }
        public string SeparateThousands(String text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US");
                ulong valueBefore = ulong.Parse(text, System.Globalization.NumberStyles.AllowThousands);
                string res = String.Format(culture, "{0:N0}", valueBefore);
                return res;
            }
            return "";
        }
        #endregion
    }
}
