﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SE104_OnlineShopManagement.Components.Controls
{
    /// <summary>
    /// Interaction logic for NumericSnipperControl.xaml
    /// </summary>
    public partial class NumericSnipperControl : UserControl
    {
        #region Fields
        public event EventHandler PropertyChanged;
        public event EventHandler ValueChanged;
        public event EventHandler TextChanged;
        #endregion
        public NumericSnipperControl()
        {
            InitializeComponent();
            DependencyPropertyDescriptor.FromProperty(ValueProperty, typeof(NumericSnipperControl)).AddValueChanged(this, PropertyChanged);
            DependencyPropertyDescriptor.FromProperty(ValueProperty, typeof(NumericSnipperControl)).AddValueChanged(this, ValueChanged);
            DependencyPropertyDescriptor.FromProperty(DecimalsProperty, typeof(NumericSnipperControl)).AddValueChanged(this, PropertyChanged);
            DependencyPropertyDescriptor.FromProperty(MinValueProperty, typeof(NumericSnipperControl)).AddValueChanged(this, PropertyChanged);
            DependencyPropertyDescriptor.FromProperty(MaxValueProperty, typeof(NumericSnipperControl)).AddValueChanged(this, PropertyChanged);

            PropertyChanged += (x, y) => validate();
        }

        #region ValueProperty

        public readonly static DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(decimal),
            typeof(NumericSnipperControl),
            new PropertyMetadata(new decimal(1)));

        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set
            {
                if (value < MinValue)
                {
                    value = MinValue;
                    this.brdBrush.BorderBrush = Brushes.Red;
                }
                else
                {
                    this.brdBrush.BorderBrush = Brushes.Gray;

                }
                if (value > MaxValue)
                {
                    value = MaxValue;
                    this.brdBrush.BorderBrush = Brushes.Red;
                }
                else
                {
                    this.brdBrush.BorderBrush = Brushes.Gray;
                }
                SetValue(ValueProperty, value);
                ValueChanged(this, new EventArgs());
                tb_main.Text = Value.ToString();
            }
        }

        public decimal Text
        {
            get { return (decimal)GetValue(ValueProperty); }
            set
            {
                if (value < MinValue)
                    value = MinValue;
                if (value > MaxValue)
                    value = MaxValue;
                SetValue(ValueProperty, value);
                tb_main.Text = value.ToString();
            }
        }
        #endregion

        #region StepProperty

        public readonly static DependencyProperty StepProperty = DependencyProperty.Register(
            "Step",
            typeof(decimal),
            typeof(NumericSnipperControl),
            new PropertyMetadata(new decimal(1)));

        public decimal Step
        {
            get { return (decimal)GetValue(StepProperty); }
            set
            {
                SetValue(StepProperty, value);
            }
        }

        #endregion

        #region DecimalsProperty

        public readonly static DependencyProperty DecimalsProperty = DependencyProperty.Register(
            "Decimals",
            typeof(int),
            typeof(NumericSnipperControl),
            new PropertyMetadata(2));

        public int Decimals
        {
            get { return (int)GetValue(DecimalsProperty); }
            set
            {
                SetValue(DecimalsProperty, value);
            }
        }

        #endregion

        #region MinValueProperty

        public readonly static DependencyProperty MinValueProperty = DependencyProperty.Register(
            "MinValue",
            typeof(decimal),
            typeof(NumericSnipperControl),
            new PropertyMetadata(decimal.MinValue));

        public decimal MinValue
        {
            get { return (decimal)GetValue(MinValueProperty); }
            set
            {
                if (value < MinValue)
                    value = MinValue;

                SetValue(MinValueProperty, value);
            }
        }

        #endregion

        #region MaxValueProperty

        public readonly static DependencyProperty MaxValueProperty = DependencyProperty.Register(
            "MaxValue",
            typeof(decimal),
            typeof(NumericSnipperControl),
            new PropertyMetadata(decimal.MaxValue));

        public decimal MaxValue
        {
            get { return (decimal)GetValue(MaxValueProperty); }
            set
            {
                if (value > MaxValue)
                    value = MaxValue;
                SetValue(MaxValueProperty, value);
            }
        }
        #endregion

        /// <summary>
        /// Revalidate the object, whenever a value is changed...
        /// </summary>
        private void validate()
        {
            // Logically, This is not needed at all... as it's handled within other properties...
            if (MinValue > MaxValue) MinValue = MaxValue;
            if (MaxValue < MinValue) MaxValue = MinValue;
            if (Value < MinValue) Value = MinValue;
            if (Value > MaxValue) Value = MaxValue;

            Value = decimal.Round(Value, Decimals);
        }
        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            Value += Step;
            tb_main.Text = Value.ToString();
        }
        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            Value -= Step;
            tb_main.Text = Value.ToString();
        }

        private void tb_main_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Text = decimal.Parse(this.tb_main.Text);
                tb_main.SelectionStart = tb_main.Text.Length;
                tb_main.SelectionLength = 0;
            }
            catch
            {

            }
        }
        private void tb_main_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void tb_main_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                Text = decimal.Parse(this.tb_main.Text);
            }
            catch
            {
                Text = Value;
            }
        }

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(NumValueChanged), // Event name
        RoutingStrategy.Bubble, // Bubble means the event will bubble up through the tree
        typeof(RoutedEventHandler), // The event type
        typeof(NumericSnipperControl)); // Belongs to ChildControlBase

        // Allows add and remove of event handlers to handle the custom event
        public event RoutedEventHandler NumValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        private void tb_main_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            var newevent = new RoutedEventArgs(ValueChangedEvent);
            RaiseEvent(newevent);
        }

        public string currentvalue
        {
            get { return tb_main.Text; }
            private set { tb_main.Text = value;}
        }
    }

}
