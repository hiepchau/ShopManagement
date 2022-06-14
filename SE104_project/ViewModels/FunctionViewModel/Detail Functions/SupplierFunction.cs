﻿using MaterialDesignThemes.Wpf;
using SE104_OnlineShopManagement.Commands;
using SE104_OnlineShopManagement.Components;
using SE104_OnlineShopManagement.Models.ModelEntity;
using SE104_OnlineShopManagement.Network;
using SE104_OnlineShopManagement.Network.Insert_database;
using SE104_OnlineShopManagement.Network.Get_database;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Collections.ObjectModel;
using SE104_OnlineShopManagement.Services;
using SE104_OnlineShopManagement.ViewModels.ComponentViewModel;
using SE104_OnlineShopManagement.Network.Update_database;
using System.Windows;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SE104_OnlineShopManagement.ViewModels.FunctionViewModel.Detail_Functions
{
    public interface IUpdateSuplierList
    {
        void UpdateSuplierList(ProducerInformation producer);
        void EditSupplier(ProducerInformation producer);
    }
    class SupplierFunction : BaseFunction, IUpdateSuplierList
    {
        #region Properties
        public string supplierName { get; set; }
        public string supplierAddress { get; set; }
        public string supplierPhone { get; set; }
        public string supplierMail { get; set; }
        public int IsSelectedIndex { get; set; }
        public string searchString { get; set; }
        public int supplierCount { get; set; }
        public bool isLoaded { get; set; }
        public string totalSupplierSpent { get; set; }
        public SupplierControlViewModel selectedProducer { get; set; }
        private MongoConnect _connection;
        private AppSession _session;
        public ObservableCollection<SupplierControlViewModel> listActiveItemsProducer { get; set; }
        public ObservableCollection<SupplierControlViewModel> listAllProducer { get; set; }
        #endregion

        #region ICommand
        //Supplier
        public ICommand OpenAddSupplierControlCommand { get; set; }
        //AddSupplier
        public ICommand SaveCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand TextChangedCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand ReloadCommand { get; set; }


        #endregion
        public SupplierFunction(AppSession session, MongoConnect connect) : base(session, connect)
        {
            this._connection = connect;
            this._session = session;
            IsSelectedIndex = -1;
            isLoaded= true;
            searchString = "";
            listAllProducer = new ObservableCollection<SupplierControlViewModel>();
            listActiveItemsProducer = new ObservableCollection<SupplierControlViewModel>();
            TextChangedCommand = new RelayCommand<Object>(null, TextChangedHandle);
            OpenAddSupplierControlCommand = new RelayCommand<Object>(null, OpenAddSupplierControl);
            SearchCommand = new RelayCommand<Object>(null, search);
            ReloadCommand = new RelayCommand<object>(null, reload);

        }
        #region Function
        public void OpenAddSupplierControl(Object o = null)
        {
            AddSupplierControl addSupplierControl = new AddSupplierControl();
            addSupplierControl.DataContext = this;
            DialogHost.Show(addSupplierControl, delegate (object sender, DialogClosingEventArgs args)
            {
                SetNull();
            });
            SaveCommand = new RelayCommand<Object>(CheckValidSave, SaveSupplier);
            ExitCommand = new RelayCommand<Object>(null, exit =>
            {
                SetNull();
                DialogHost.CloseDialogCommand.Execute(null, null);
                SetNull();
            });
        }
        private async void reload(Object o = null)
        {
            await GetData();
            await GetAllData();
            Console.WriteLine("Executed active producer for reload " + listActiveItemsProducer.Count.ToString());
            Console.WriteLine("Executed all producer for reload " + listAllProducer.Count.ToString());

            supplierCount = (listActiveItemsProducer.Count > 0) ? listActiveItemsProducer.Count : 0;
            isLoaded = false;
            OnPropertyChanged(nameof(isLoaded));
            OnPropertyChanged(nameof(supplierCount));

            //totalSupplierSpent
            long displayTotalSupplierSpent = 0;
            if(listActiveItemsProducer.Count > 0)
            {
                foreach (var producer in listActiveItemsProducer)
                {
                    await producer.GetBillAmount();
                    displayTotalSupplierSpent += ConvertToNumber(producer.sumPrice);
                }
                Console.WriteLine("Total Supplier sent: " + displayTotalSupplierSpent);
                totalSupplierSpent = SeparateThousands(displayTotalSupplierSpent.ToString());
                OnPropertyChanged(nameof(totalSupplierSpent));
            }
        }
        public long ConvertToNumber(string str)
        {
            string[] s = str.Split(',');
            string tmp = "";
            foreach (string a in s)
            {
                tmp += a;
            }

            return long.Parse(tmp);
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
        public void TextChangedHandle(Object o = null)
        {
            (SaveCommand as RelayCommand<Object>).OnCanExecuteChanged();
        }
        public bool CheckValidSave(Object o = null)
        {
            if (String.IsNullOrEmpty(supplierName) || String.IsNullOrEmpty(supplierAddress)
                || String.IsNullOrEmpty(supplierMail) || supplierPhone.Length != 10)
            {
                return false;
            }
            return true;
        }

        public void SetNull(Object o = null)
        {
            supplierName = "";
            supplierAddress = "";
            supplierMail = "";
            supplierPhone = "";
            OnPropertyChanged(nameof(supplierPhone));
            OnPropertyChanged(nameof(supplierAddress));
            OnPropertyChanged(nameof(supplierName));
            OnPropertyChanged(nameof(supplierMail));
        }
        public async void SaveSupplier(object o = null)
        {
            if (selectedProducer != null)
            {
                var filter = Builders<ProducerInformation>.Filter.Eq("ID", selectedProducer.ID);
                var update = Builders<ProducerInformation>.Update.Set("Name", supplierName).Set("Email", supplierAddress).Set("Phone", supplierPhone).Set("Address",supplierAddress);
                UpdateProducerInformation updater = new UpdateProducerInformation(_connection.client, _session, filter, update);
                var s = await updater.update();
                listActiveItemsProducer.Clear();
                _ = GetData();
                OnPropertyChanged(nameof(listActiveItemsProducer));
            }
            else
            {
                int flag = CheckExist();
                switch (flag)
                {
                    case 0:
                        CustomMessageBox.Show("Thuộc tính đã tồn tại", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    case 1:
                        SetActive(selectedProducer);
                        break;
                    case 2:
                        ProducerInformation info = new ProducerInformation("", supplierName, supplierMail, supplierPhone, supplierAddress, true, await new AutoProducerIDGenerator(_session, _connection.client).Generate());
                        RegisterProducer regist = new RegisterProducer(info, _connection.client, _session);
                        string s = await regist.register();
                        listActiveItemsProducer.Clear();
                        listAllProducer.Clear();
                        _ = GetAllData();
                        _ = GetData();
                        OnPropertyChanged(nameof(listActiveItemsProducer));
                        Console.WriteLine(s);
                        break;
                }
            }
            DialogHost.CloseDialogCommand.Execute(null, null);
            CustomMessageBox.Show("Thêm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            //Set Null
            SetNull();
        }
        public void UpdateSuplierList(ProducerInformation producer)
        {
            var result = CustomMessageBox.Show("Bạn có chắc chắn muốn xóa?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                int i = 0;
                if (listActiveItemsProducer.Count > 0)
                {
                    foreach (SupplierControlViewModel ls in listActiveItemsProducer)
                    {
                        if (ls.producer.ID.Equals(producer.ID))
                        {
                            SetUnactive(ls);
                            listAllProducer.Clear();
                            _ = GetAllData();
                            break;
                        }
                        i++;
                    }
                    listActiveItemsProducer.RemoveAt(i);
                    OnPropertyChanged(nameof(listActiveItemsProducer));
                }

            }
            else
            {
                CustomMessageBox.Show("Xóa không thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        public void EditSupplier(ProducerInformation producer)
        {
            if (listActiveItemsProducer.Count > 0)
            {
                foreach (SupplierControlViewModel ls in listActiveItemsProducer)
                {
                    if (ls.producer.Equals(producer))
                    {
                        selectedProducer = ls;
                        break;
                    }
                }
            }
            else
            {
                return;
            }
            supplierName = selectedProducer.Name;
            supplierMail = selectedProducer.Email;
            supplierAddress = selectedProducer.Address;
            supplierPhone = selectedProducer.PhoneNumber;
            OnPropertyChanged(nameof(supplierPhone));
            OnPropertyChanged(nameof(supplierAddress));
            OnPropertyChanged(nameof(supplierName));
            OnPropertyChanged(nameof(supplierMail));
            OpenAddSupplierControl();
        }
        public async void SetActive(SupplierControlViewModel producerinfo)
        {
            var filter = Builders<ProducerInformation>.Filter.Eq("ID", producerinfo.ID);
            var update = Builders<ProducerInformation>.Update.Set("isActivated", true);
            UpdateProducerInformation updater = new UpdateProducerInformation(_connection.client, _session, filter, update);
            var s = await updater.update();
            listActiveItemsProducer.Clear();
            _ = GetData();
            OnPropertyChanged(nameof(listActiveItemsProducer));
            Console.WriteLine(s);
            selectedProducer = null;
        }
        public async void SetUnactive(SupplierControlViewModel producerinfo)
        {
            if (producerinfo.isActivated == true)
            {
                var filter = Builders<ProducerInformation>.Filter.Eq("ID", producerinfo.ID);
                var update = Builders<ProducerInformation>.Update.Set("isActivated", false);
                UpdateProducerInformation updater = new UpdateProducerInformation(_connection.client, _session, filter, update);
                var s = await updater.update();
                Console.WriteLine(s);
                selectedProducer = null;
            }
            else Console.WriteLine("Cant execute");
        }
        public int CheckExist()
        {
            foreach (SupplierControlViewModel ls in listActiveItemsProducer)
            {
                if (supplierMail == ls.Email || supplierPhone == ls.PhoneNumber)
                {
                    return 0;
                }
            }

            foreach (SupplierControlViewModel ls1 in listAllProducer)
            {
                if (supplierMail == ls1.Email || supplierPhone == ls1.PhoneNumber)
                {
                    selectedProducer = ls1;
                    //Set Active
                    return 1;
                }
            }
            return 2;
        }
        public void SetNull()
        {
            supplierName = "";
            supplierAddress = "";
            supplierMail = "";
            supplierPhone = "";
            selectedProducer = null;
            OnPropertyChanged(nameof(supplierName));
            OnPropertyChanged(nameof(supplierAddress));
            OnPropertyChanged(nameof(supplierMail));
            OnPropertyChanged(nameof(supplierPhone));
        }
        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {

            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        public void NumberValidationTextBox(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }
        private async void search(object o)
        {
            searchString = (o.ToString());
            if (string.IsNullOrEmpty(searchString))
            {
                listActiveItemsProducer.Clear();
                await GetData();
            }
            else
            {
                await getsearchdata();
            }
        }
        #endregion

        #region DB
        public async Task GetData()
        {
            var filter = Builders<ProducerInformation>.Filter.Eq("isActivated",true);
            GetProducer getter = new GetProducer(_connection.client, _session, filter);
            var ls = await getter.Get();
            foreach (ProducerInformation pro in ls)
            {
                listActiveItemsProducer.Add(new SupplierControlViewModel(pro,this));
            }
            OnPropertyChanged(nameof(listActiveItemsProducer));
        }
        public async Task GetAllData()
        {
            var filter = Builders<ProducerInformation>.Filter.Empty;
            GetProducer getter = new GetProducer(_connection.client, _session, filter);
            var ls = await getter.Get();
            foreach (ProducerInformation pro in ls)
            {
                listAllProducer.Add(new SupplierControlViewModel(pro, this));
            }
        }
        private async Task getsearchdata()
        {
            listActiveItemsProducer.Clear();
            OnPropertyChanged(nameof(listActiveItemsProducer));
            FilterDefinition<ProducerInformation> filter = Builders<ProducerInformation>.Filter.Eq(x => x.Name, searchString);
            var tmp = new GetProducer(_connection.client, _session, filter);
            var ls = await tmp.Get();
            foreach (ProducerInformation pr in ls)
            {
                listActiveItemsProducer.Add(new SupplierControlViewModel(pr, this));
            }
            OnPropertyChanged(nameof(listActiveItemsProducer));
        }
        #endregion
    }
}
