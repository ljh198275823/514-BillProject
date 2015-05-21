using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LJH.BillProject.Model;
using LJH.BillProject.BLL;
using LJH.BillProject.Control;

namespace LJH.BillProject.BillProject
{
    public partial class FrmLogView : Form
    {
        public FrmLogView()
        {
            InitializeComponent();
        }

        private void ShowItemsOnGrid(List<AlarmInfo> logs)
        {
            this.dataGridView1.Rows.Clear();
            foreach (AlarmInfo log in logs)
            {
                int row = dataGridView1.Rows.Add();
                dataGridView1.Rows[row].Tag = log;
                dataGridView1.Rows[row].Cells["colAlarmDateTime"].Value = log.AlarmDateTime;
                dataGridView1.Rows[row].Cells["colAlarmType"].Value = log.AlarmType;
                dataGridView1.Rows[row].Cells["colAlarmDescr"].Value = log.AlarmDescr;
                dataGridView1.Rows[row].Cells["colOperator"].Value = log.OperatorID;
            }
            this.toolStripStatusLabel1.Text = string.Format("总共 {0} 项", dataGridView1.Rows.Count);
        }

        private void FrmLogView_Load(object sender, EventArgs e)
        {
            this.ucDateTimeInterval1.SelectToday();
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            AlarmSearchCondition con = new AlarmSearchCondition();
            con.From = this.ucDateTimeInterval1.StartDateTime;
            con.End = this.ucDateTimeInterval1.EndDateTime;
            List<AlarmInfo> logs = new AlarmInfoBLL(AppSettings.Current.ConnStr).GetItems(con).QueryObjects;
            ShowItemsOnGrid(logs);
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "Excel文档|*.xls;*.xlsx|所有文件(*.*)|*.*";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string path = saveFileDialog1.FileName;
                    LJH.GeneralLibrary.WinformControl.DataGridViewExporter.Export(this.dataGridView1, path);
                    MessageBox.Show("导出成功");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存到电子表格时出现错误!");
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                AlarmInfo alarm = dataGridView1.Rows[e.RowIndex].Tag as AlarmInfo;
                if (alarm != null && !string.IsNullOrEmpty(alarm.AlarmSource))
                {
                    PaymentLog payment = new PaymentLogBLL(AppSettings.Current.ConnStr).GetByID(Guid.Parse(alarm.AlarmSource)).QueryObject;
                    if (payment != null)
                    {
                        FrmPaymentLogDetail frm = new FrmPaymentLogDetail();
                        frm.UpdatingItem = payment;
                        frm.IsAdding = false;
                        frm.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}