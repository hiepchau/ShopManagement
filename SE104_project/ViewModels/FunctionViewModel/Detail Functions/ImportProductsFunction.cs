﻿using MaterialDesignThemes.Wpf;
using MongoDB.Driver;
using SE104_OnlineShopManagement.Commands;
using SE104_OnlineShopManagement.Components;
using SE104_OnlineShopManagement.Models.ModelEntity;
using SE104_OnlineShopManagement.Network;
using SE104_OnlineShopManagement.Network.Get_database;
using SE104_OnlineShopManagement.ViewModels.ComponentViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace SE104_OnlineShopManagement.ViewModels.FunctionViewModel.Detail_Functions
{
    public interface IUpdateImportProductList
    {
        void UpdateProductList(CustomerInformation cus);
    }
    class ImportProductsFunction : BaseFunction
    {
        #region Properties
        private MongoConnect _connection;
        private AppSession _session;
        public ObservableCollection<ProducerInformation> ItemSourceSupplier { get; set; }
        public ObservableCollection<ImportProductsControlViewModel> listItemsImportProduct { get; set; }
        #endregion

        #region ICommand
        public ICommand OpenAddReceiptControlCommand { get; set; }
        //AddReceiptControl
        public ICommand ExitCommand { get; set; }
        public ICommand OpenAddSupplierControlCommand { get; set; }
        #endregion
        public ImportProductsFunction(AppSession session, MongoConnect connect) : base(session, connect)
        {
            _connection = connect;
            _session = session;
            ItemSourceSupplier = new ObservableCollection<ProducerInformation>();
            listItemsImportProduct = new ObservableCollection<ImportProductsControlViewModel>();
            //Test
            GetProducerInfo();
            listItemsImportProduct.Add(new ImportProductsControlViewModel(new ProductsInformation("1", "hip", 12, 1000, 900, "ohye", "ohye", "nguoi")));
            //
            OpenAddReceiptControlCommand = new RelayCommand<Object>(null, OpenAddReceiptControl);

        }

        #region Function
        public void OpenAddReceiptControl(Object o = null)
        {
            AddReceiptControl addReceiptControl = new AddReceiptControl();
            addReceiptControl.DataContext = this;
            DialogHost.Show(addReceiptControl);
            ExitCommand = new RelayCommand<Object>(null, exit =>
            {
                DialogHost.CloseDialogCommand.Execute(null, null);
            });
            OpenAddSupplierControlCommand = new RelayCommand<Object>(null, OpenAddSupplierControl);


        }
        public void OpenAddSupplierControl(Object o = null)
        {
            AddSupplierControl addSupplierControl = new AddSupplierControl();
            addSupplierControl.DataContext = this;
            DialogHost.Show(addSupplierControl);
            ExitCommand = new RelayCommand<Object>(null, exit =>
            {
                DialogHost.CloseDialogCommand.Execute(null, null);
            });

        }
        #endregion

        #region DB
        public async void GetProducerInfo()
        {
            var filter = Builders<ProducerInformation>.Filter.Empty;
            GetProducer getter = new GetProducer(_connection.client, _session, filter);
            var ls = await getter.Get();
            foreach (ProducerInformation pro in ls)
            {
                ItemSourceSupplier.Add(pro);
            }
            Console.Write("Executed");
            OnPropertyChanged(nameof(ItemSourceSupplier));
          
        }
        #endregion
    }
}
