﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LJH.BillProject .Model ;
using LJH.BillProject .BLL ;

namespace BillProject_WPF
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class WinPaymentLogDetail : Window
    {
        public WinPaymentLogDetail()
        {
            InitializeComponent();
        }

        #region 公共属性
        public List<PaymentLog> PaymentLogs { get; set; }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (PaymentLogs != null)
            {
                
            }
        }
    }
}
