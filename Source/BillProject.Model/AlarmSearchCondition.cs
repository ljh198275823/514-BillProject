using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJH.BillProject.Model
{
    public class AlarmSearchCondition : LJH.GeneralLibrary.Core.DAL.SearchCondition
    {
        public DateTime? From { get; set; }
        public DateTime? End { get; set; }
        public AlarmType? AlarmType { get; set; } //报警类型
        public string OperatorID { get; set; } //报警来源
    }
}
