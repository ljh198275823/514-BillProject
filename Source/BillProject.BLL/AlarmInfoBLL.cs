using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJH.BillProject.Model;
using LJH.BillProject.DAL;
using LJH.GeneralLibrary.Core.DAL;

namespace LJH.BillProject.BLL
{
    public class AlarmInfoBLL : BLLBase<Guid, AlarmInfo>
    {
        #region 构造函数
        public AlarmInfoBLL(string repoUri)
            : base(repoUri)
        {

        }
        #endregion
    }
}
