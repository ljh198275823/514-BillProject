using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using LJH.BillProject.Model;
using LJH.BillProject.BLL;
using LJH.BillProject.Control;

namespace LJH.BillProject.BillProject
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        #region 私有变量
        private string _ConStr = string.Format("SQLITE:Data Source={0}", Path.Combine(Application.StartupPath, "BillProject.db"));
        private List<PaymentPanel> _panels = new List<PaymentPanel>();
        private DateTime _LogFrom = DateTime.Now;
        private DateTime? _LogTo = null;
        private List<PaymentLog> _ShowingLogs = null;
        private string _Title = string.Empty;
        #endregion

        #region 私有方法
        private void UpGradeDataBase()
        {
            bool ret = false;
            string path = System.IO.Path.Combine(Application.StartupPath, "DbUpdate_Sqlite.sql");
            if (File.Exists(path))
            {
                List<string> commands = (new LJH.GeneralLibrary.SQLHelper.SQLStringExtractor()).ExtractFromFile(path);
                if (commands != null && commands.Count > 0)
                {
                    string connStr = string.Format("Data Source={0}", Path.Combine(Application.StartupPath, "BillProject.db"));
                    using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection(connStr))
                    {
                        using (System.Data.SQLite.SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand(con))
                        {
                            con.Open();
                            foreach (string command in commands)
                            {
                                try
                                {
                                    cmd.CommandText = command;
                                    cmd.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    //LJH.GeneralLibrary.ExceptionHandling.ExceptionPolicy.HandleException(ex);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InitPanel(DateTime logFrom, DateTime? logTo, int mode)
        {
            _ShowingLogs = null;
            PaymentLogSearchCondition con = new PaymentLogSearchCondition() { LogFrom = logFrom, LogEnd = logTo };
            List<PaymentLog> items = new PaymentLogBLL(AppSettings.Current.ConnStr).GetItems(con).QueryObjects;
            InitPanel(items, cmbShowmode.SelectedIndex >= 0 ? cmbShowmode.SelectedIndex : 0);
        }

        private void InitPanel(List<PaymentLog> items, int mode)
        {
            foreach (PaymentPanel p in _panels)
            {
                this.flowLayoutPanel1.Controls.Remove(p);
            }
            _panels.Clear();
            if (items != null && items.Count > 0)
            {
                IEnumerable<IGrouping<string, PaymentLog>> groups = null;
                if (mode == 0) //按天
                {
                    groups = from item in items
                             orderby item.PaymentDate descending
                             group item by item.PaymentDate.ToString("yyyy年MM月dd日");
                }
                else if (mode == 1) //按月
                {
                    groups = from item in items
                             orderby item.PaymentDate descending
                             group item by string.Format("{0:D4}年{1:D2}月", AppSettings.Current.YearOf(item.PaymentDate), AppSettings.Current.MonthOf(item.PaymentDate));
                }
                else if (mode == 2)  //按年
                {
                    groups = from item in items
                             orderby item.PaymentDate descending
                             group item by string.Format("{0:D4}年", AppSettings.Current.YearOf(item.PaymentDate));
                }
                else if (mode == 3) //按支出项目
                {
                    groups = from item in items
                             orderby item.Category ascending
                             group item by item.Category;
                }
                else if (mode == 4) //按用户
                {
                    groups = from item in items
                             orderby item.User ascending
                             group item by item.User;
                }
                else if (mode == 5) //按支付方式
                {
                    groups = from item in items
                             orderby item.PaymentMode ascending
                             group item by item.PaymentMode;
                }
                foreach (var group in groups)
                {
                    PaymentPanel p = new PaymentPanel();
                    p.Title = group.Key;
                    p.Amount = group.Sum(it => it.Deleted != null && it.Deleted.Value ? 0 : it.Amount);
                    p.Tag = group.ToList();
                    p.ContentDoubleClick += new EventHandler(p_DoubleClick);
                    _panels.Add(p);
                    this.flowLayoutPanel1.Controls.Add(p);
                }
            }
            lblAmount.Text = string.Format("{0}  共 {1} 项，消费 {2} 元", _Title, _panels.Count, items != null && items.Count > 0 ? items.Sum(it => it.Deleted != null && it.Deleted.Value ? 0 : it.Amount) : 0);
        }
        #endregion

        #region 事件处理程序
        private void FrmMain_Load(object sender, EventArgs e)
        {
            this.Text += string.Format(" [{0}]", Application.ProductVersion);
            AppSettings.Current.ConnStr = _ConStr;
            UpGradeDataBase();
            cmbShowmode.SelectedIndex = 0;
            btnThisMonth.PerformClick();
        }

        private void btnAddLog_Click(object sender, EventArgs e)
        {
            FrmPaymentLogDetail frm = new FrmPaymentLogDetail();
            frm.IsAdding = true;
            frm.ShowDialog();
            if (_ShowingLogs == null)
            {
                InitPanel(_LogFrom, _LogTo, cmbShowmode.SelectedIndex >= 0 ? cmbShowmode.SelectedIndex : 0);
            }
        }

        private void btnThisMonth_Click(object sender, EventArgs e)
        {
            _LogFrom = AppSettings.Current.ThisMonthBegin;
            _LogTo = null;
            _Title = btnThisMonth.Text;
            cmbShowmode.SelectedIndex = 0;
            InitPanel(_LogFrom, _LogTo, cmbShowmode.SelectedIndex >= 0 ? cmbShowmode.SelectedIndex : 0);
        }

        private void btnShowLastMonth_Click(object sender, EventArgs e)
        {
            DateTime dt = AppSettings.Current.ThisMonthBegin;
            _LogFrom = dt.AddMonths(-1);
            _LogTo = dt.AddSeconds(-1);
            _Title = btnShowLastMonth.Text;
            cmbShowmode.SelectedIndex = 0;
            InitPanel(_LogFrom, _LogTo, cmbShowmode.SelectedIndex >= 0 ? cmbShowmode.SelectedIndex : 0);
        }

        private void btnShowThisYear_Click(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            _LogFrom = new DateTime(AppSettings.Current.YearOf(dt), 1, AppSettings.Current.MonthStart);
            _LogTo = null;
            _Title = btnShowThisYear.Text;
            cmbShowmode.SelectedIndex = 1;
            InitPanel(_LogFrom, _LogTo, cmbShowmode.SelectedIndex >= 0 ? cmbShowmode.SelectedIndex : 0);
        }

        private void btnShowLastYear_Click(object sender, EventArgs e)
        {
            DateTime dt = AppSettings.Current.ThisMonthBegin;
            _LogFrom = dt.AddMonths(-11); //最近一年是从本月开始日期往前推11个月
            _LogTo = null;
            _Title = btnShowLastYear.Text;
            cmbShowmode.SelectedIndex = 1;
            InitPanel(_LogFrom, _LogTo, cmbShowmode.SelectedIndex >= 0 ? cmbShowmode.SelectedIndex : 0);
        }

        private void p_DoubleClick(object sender, EventArgs e)
        {
            int mode = cmbShowmode.SelectedIndex;
            if (mode == 1 || mode == 2)
            {
                _Title += "-->" + (sender as PaymentPanel).Title;
                _ShowingLogs = (sender as PaymentPanel).Tag as List<PaymentLog>;
                cmbShowmode.SelectedIndex = mode - 1;
            }
            else
            {
                PaymentPanel p = sender as PaymentPanel;
                FrmPaymentLogMaster frm = new FrmPaymentLogMaster();
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.PaymentLogs = (sender as PaymentPanel).Tag as List<PaymentLog>;
                frm.Text = string.Format("{0}", p.Title);
                frm.ShowDialog();
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        
        private void btn_Report_Click(object sender, EventArgs e)
        {
            FrmPaymentLogReport frm = new FrmPaymentLogReport();
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.ShowDialog();
        }

        private void cmbShowmode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ShowingLogs == null)
            {
                InitPanel(_LogFrom, _LogTo, cmbShowmode.SelectedIndex >= 0 ? cmbShowmode.SelectedIndex : 0);
            }
            else
            {
                InitPanel(_ShowingLogs, cmbShowmode.SelectedIndex >= 0 ? cmbShowmode.SelectedIndex : 0);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            FrmLogView frm = new FrmLogView();
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.ShowDialog();
        }
        #endregion
    }
}
