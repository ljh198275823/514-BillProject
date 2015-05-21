using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJH.BillProject.Model;
using LJH.BillProject.DAL;
using LJH.GeneralLibrary.Core.DAL;

namespace LJH.BillProject.BLL
{
    public class PaymentLogBLL : BLLBase<Guid, LJH.BillProject.Model.PaymentLog>
    {
        #region 构造函数
        public PaymentLogBLL(string reporUri)
            : base(reporUri)
        {
        }
        #endregion

        #region 重写基类方法
        public override CommandResult Delete(PaymentLog info)
        {
            PaymentLog original = GetByID(info.ID).QueryObject;
            if (original == null)
            {
                return new CommandResult(ResultCode.Fail, "更新失败，记录被删除");
            }
            original = info.Clone();
            info.Deleted = true;
            IUnitWork unitWork = ProviderFactory.Create<IUnitWork>(_RepoUri);
            ProviderFactory.Create<IProvider<PaymentLog, Guid>>(_RepoUri).Update(info, original,unitWork );

            AlarmInfo alarm = new AlarmInfo()
            {
                ID = Guid.NewGuid(),
                AlarmDateTime = DateTime.Now,
                AlarmType = AlarmType.删除,
                AlarmSource = info.ID.ToString(),
                AlarmDescr = string.Format("删除消费记录, 记录日期:{0} 金额:{1} 消费类别:{2} 支出类别:{3} 消费者:{4}", info.PaymentDate.ToString("yyyy-MM-dd"), info.Amount, info.Category, info.PaymentMode, info.User),
            };
            ProviderFactory.Create<IProvider<AlarmInfo, Guid>>(_RepoUri).Insert(alarm, unitWork);
            return unitWork.Commit();
        }
        #endregion
    }
}
