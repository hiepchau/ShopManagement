﻿using SE104_OnlineShopManagement.Models.ModelEntity;
using SE104_OnlineShopManagement.Network;
using SE104_OnlineShopManagement.Commands;
using SE104_OnlineShopManagement.Network.Insert_database;
using SE104_OnlineShopManagement.Network.Get_database;
using SE104_OnlineShopManagement.Network.Update_database;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Collections.ObjectModel;
using MongoDB.Driver;
using SE104_OnlineShopManagement.ViewModels.ComponentViewModel;
using System.Windows;
using SE104_OnlineShopManagement.Services;
using System.Threading.Tasks;

namespace SE104_OnlineShopManagement.ViewModels.FunctionViewModel.Detail_Functions
{
    public interface IUpdateProductTypeList
    {
        void UpdateProductTypeList(ProductTypeInfomation type);
        void EditProductType(ProductTypeInfomation type);
    }
    class ProductsTypeFunction:BaseFunction, IUpdateProductTypeList
    {
        #region Properties
        public string productTypeName { get; set; }
        public string note { get; set; }
        public bool isLoaded { get; set; }
        private MongoConnect _connection;
        private AppSession _session;
        public ProductsTypeControlViewModel selectedProductType { get; set; }
        public ObservableCollection<ProductsTypeControlViewModel> listItemsProductType { get; set; }
        public ObservableCollection<ProductsTypeControlViewModel> listItemsUnactiveProductType { get; set; }
        #endregion

        #region ICommand
        public ICommand SaveCommand { get; set; }
        public ICommand SetUnactiveCommand { get; set; }
        public ICommand SetActiveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand TextChangedCommand { get; set; }
        #endregion

        public ProductsTypeFunction(AppSession session, MongoConnect connect) : base(session, connect)
        {
            this._connection = connect;
            this._session = session;
            listItemsProductType = new ObservableCollection<ProductsTypeControlViewModel>();
            listItemsUnactiveProductType = new ObservableCollection<ProductsTypeControlViewModel>();
            isLoaded = true;
            //Get Data
            GetData();
            GetUnactiveProductType();
            //
            TextChangedCommand = new RelayCommand<Object>(null, TextChanged);
            SaveCommand = new RelayCommand<Object>(CheckValidSave, SaveProductType);
            SetUnactiveCommand = new RelayCommand<Object>(null, SetUnactive);
            SetActiveCommand = new RelayCommand<Object>(null, SetActive);
            CancelCommand = new RelayCommand<Object>(null, SetNull);
        }

        #region Function
        public void TextChanged(Object o = null)
        {
            (SaveCommand as RelayCommand<Object>).OnCanExecuteChanged();
        }
        public bool CheckValidSave(Object o = null)
        {
            if (string.IsNullOrEmpty(productTypeName) 
                || char.IsNumber(productTypeName[0]) 
                || char.IsSymbol(productTypeName[0]) 
                || char.IsPunctuation(productTypeName[0])) return false;
            return true;
        }
        public void SetNull(object o = null)
        {
            selectedProductType = null;
            productTypeName = "";
            note = "";
            OnPropertyChanged(nameof(productTypeName));
            OnPropertyChanged(nameof(note));
        }
        public async void SaveProductType(object o = null)
        {
            if (selectedProductType!=null)
            {
                var filter = Builders<ProductTypeInfomation>.Filter.Eq(x=>x.ID, selectedProductType.ID);
                var update = Builders<ProductTypeInfomation>.Update.Set("ProductTypeName", productTypeName).Set("Note", note);
                UpdateProductTypeInformation updater = new UpdateProductTypeInformation(_connection.client, _session, filter, update);
                var s = await updater.update();
                listItemsProductType.Clear();
                await GetData();
                OnPropertyChanged(nameof(listItemsProductType));
            }
            else 
            {
                int flag = CheckExist();
                switch (flag)
                {
                    case 0:
                        CustomMessageBox.Show("Loại sản phẩm đã tồn tại", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    case 1:
                        SetActive();
                        return;
                    case 2:
                        ProductTypeInfomation info = new ProductTypeInfomation("", productTypeName, note="");
                        RegisterProductType regist = new RegisterProductType(info, _connection.client, _session);
                        string s = await regist.register();
                        info.ID = s;
                        listItemsProductType.Clear();
                        GetData();
                        OnPropertyChanged(nameof(listItemsProductType));
                        Console.WriteLine(s);
                        return;
                }           
            }
            SetNull();
        }
        public void UpdateProductTypeList(ProductTypeInfomation type)
        {
            var result = CustomMessageBox.Show("Bạn có chắc chắn muốn xóa?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                int i = 0;
                if (listItemsProductType.Count > 0)
                {
                    foreach (ProductsTypeControlViewModel ls in listItemsProductType)
                    {
                        if (ls.type.ID.Equals(type.ID))
                        {
                            break;
                        }
                        i++;
                    }
                    listItemsProductType.RemoveAt(i);
                    OnPropertyChanged(nameof(listItemsProductType));
                }
                else
                {
                    return;
                }
            }
            else
            {
                CustomMessageBox.Show("Xóa không thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        public async void SetUnactive(object o = null)
        {
            var result = CustomMessageBox.Show("Loại sản phẩm này sẽ ngừng hoạt động ?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                if (selectedProductType != null && selectedProductType.isActivated == true)
                {
                    var filter = Builders<ProductTypeInfomation>.Filter.Eq(x=>x.ID, selectedProductType.ID);
                    var update = Builders<ProductTypeInfomation>.Update.Set("isActivated", false);
                    UpdateProductTypeInformation updater = new UpdateProductTypeInformation(_connection.client, _session, filter, update);
                    var s = await updater.update();
                    listItemsUnactiveProductType.Clear();
                    listItemsProductType.Clear();
                    await GetData();
                    await GetUnactiveProductType();
                    OnPropertyChanged(nameof(listItemsUnactiveProductType));
                    OnPropertyChanged(nameof(listItemsProductType));
                    Console.WriteLine(s);
                    selectedProductType = null;
                }
                else
                {
                    CustomMessageBox.Show("Loại sản phẩm này đang ngừng hoạt động!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    Console.WriteLine("Cant execute");
                }
            }
            else return;
        }
        public async void SetActive(object o = null)
        {
            var result = CustomMessageBox.Show("Loại sản phẩm này sẽ hoạt động trở lại ?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                if (selectedProductType != null && selectedProductType.isActivated == false)
                {
                    var filter = Builders<ProductTypeInfomation>.Filter.Eq("ID", selectedProductType.ID);
                    var update = Builders<ProductTypeInfomation>.Update.Set("isActivated", true);
                    UpdateProductTypeInformation updater = new UpdateProductTypeInformation(_connection.client, _session, filter, update);
                    var s = await updater.update();
                    listItemsUnactiveProductType.Clear();
                    listItemsProductType.Clear();
                    await GetData();
                    await GetUnactiveProductType();
                    OnPropertyChanged(nameof(listItemsUnactiveProductType));
                    OnPropertyChanged(nameof(listItemsProductType));
                    Console.WriteLine(s);
                    selectedProductType = null;
                }
                else
                {
                    CustomMessageBox.Show("Loại sản phẩm này đang hoạt động!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    Console.WriteLine("Cant execute");
                }
            }
            else return;
        }
        public void EditProductType(ProductTypeInfomation type)
        {
            if (listItemsProductType.Count > 0)
            {
                foreach (ProductsTypeControlViewModel ls in listItemsProductType)
                {
                    if (ls.type.Equals(type))
                    {
                        selectedProductType = ls;
                        break;
                    }
                }              
            }
            else
            {
                return;
            }
            productTypeName = selectedProductType.name;
            note = selectedProductType.note;
            OnPropertyChanged(nameof(productTypeName)); 
            OnPropertyChanged(nameof(note));           
        }
        #endregion

        #region DB
        public async Task GetData()
        {
            var filter = Builders<ProductTypeInfomation>.Filter.Eq("isActivated",true);
            GetProductType getter = new GetProductType(_connection.client, _session, filter);
            var ls = await getter.Get();
            int No = 1;
            foreach (ProductTypeInfomation type in ls)
            {
                listItemsProductType.Add(new ProductsTypeControlViewModel(type, this, No.ToString()));
                No++;
            }
            isLoaded = false;
            OnPropertyChanged(nameof(isLoaded));
            OnPropertyChanged(nameof(listItemsProductType));
        }
        public async Task GetUnactiveProductType()
        {
            var filter = Builders<ProductTypeInfomation>.Filter.Eq("isActivated", false);
            GetProductType getter = new GetProductType(_connection.client, _session, filter);
            var ls = await getter.Get();
            int No = 1;
            foreach (ProductTypeInfomation type in ls)
            {
                listItemsUnactiveProductType.Add(new ProductsTypeControlViewModel(type, this, No.ToString()));
                No++;
            }
            Console.Write("Executed");
            OnPropertyChanged(nameof(listItemsUnactiveProductType));
        }
        public int CheckExist()
        {
            foreach (ProductsTypeControlViewModel ls in listItemsProductType)
            {
                if (productTypeName == ls.name)
                {
                    return 0;
                }
            }

            foreach (ProductsTypeControlViewModel ls1 in listItemsUnactiveProductType)
            {
                if (productTypeName == ls1.name)
                {
                    selectedProductType = ls1;
                    //Set Active
                    return 1;
                }
            }
            return 2;
        }
        #endregion
    }
}
