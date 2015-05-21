using System;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using LJH.GeneralLibrary.SoftDog;

namespace LJH.BillProject.Model
{
    public class AppSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public static AppSettings Current
        {
            get
            {
                if (_instance == null)
                    _instance = new AppSettings(Path.Combine(Application.StartupPath, "config.xml"));
                return _instance;
            }
        }

        #region 私有变量
        private static AppSettings _instance = null;
        private XmlDocument _doc = null;
        private XmlNode _parent = null;
        private string _path;

        private string _ConnectString;

        private string _Categorys;
        private string _Users;
        private string _PaymentModes;
        private int _MonthStart;
        #endregion

        #region 构造函数
        public AppSettings(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    _path = path;
                    this._doc = new XmlDocument();
                    this._doc.Load(_path);
                    _parent = this._doc.SelectSingleNode("configuration/appSettings");

                    _ConnectString = GetConfigContent("ConnectString");

                    string temp;
                    _Categorys = GetConfigContent("Categorys");
                    _Users = GetConfigContent("Users");
                    _PaymentModes = GetConfigContent("PaymentModes");
                    int.TryParse(GetConfigContent("MonthStart"), out _MonthStart);
                    if (_MonthStart <= 0 || _MonthStart > 31) _MonthStart = 1;
                }
                catch
                {
                }
            }
        }
        #endregion

        #region 公共属性
        /// <summary>
        /// 停车场连接字串
        /// </summary>
        public string ConnStr
        {
            //连接字串分两段加密，首先前8个字符为加密的日期，做为实际连接字符串信息的加密密码。
            //解密连接字串：先用默认加密密码的加密类型解密出前8个字符的明文，再用一个密码为此明文的加密类解密出后续字符，得到连接字符的明文。
            get
            {
                string con = string.Empty;
                return (new DTEncrypt()).DSEncrypt(_ConnectString);
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _ConnectString = (new DTEncrypt()).Encrypt(value);
                    SaveConfig("ConnectString", _ConnectString);
                }
                else
                {
                    SaveConfig("ConnectString", string.Empty);
                }
            }
        }

        public string Categorys
        {
            get { return _Categorys; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _Categorys = value;
                    SaveConfig("Categorys", value);
                }
            }
        }

        public string Users
        {
            get { return _Users; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _Users = value;
                    SaveConfig("Users", value);
                }
            }
        }

        public string PaymentModes
        {
            get { return _PaymentModes; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _PaymentModes = value;
                    SaveConfig("PaymentModes", value);
                }
            }
        }
        /// <summary>
        /// 获取或设置每个月从哪天开始统计
        /// </summary>
        public int MonthStart
        {
            get { return _MonthStart; }
            set
            {
                if (_MonthStart != value)
                {
                    _MonthStart = value;
                    SaveConfig("MonthStart", value.ToString());
                }
            }
        }

        public bool SaveConfig(string configName, string configContent)
        {
            if (_parent != null)
            {
                try
                {
                    XmlElement add = null;
                    XmlAttribute key = null;
                    XmlAttribute value = null;
                    XmlNodeList nodeList = _parent.ChildNodes;
                    foreach (XmlNode xn in nodeList)
                    {
                        if (xn is XmlElement)
                        {
                            XmlElement xe = (XmlElement)xn;
                            if (xe.GetAttribute("key") == configName)
                            {
                                xe.SetAttribute("value", configContent);
                                add = xe;
                                break;
                            }
                        } // end if
                    }
                    if (add == null)
                    {
                        add = _doc.CreateElement("add");
                        key = _doc.CreateAttribute("key");
                        key.Value = configName;
                        value = _doc.CreateAttribute("value");
                        value.Value = configContent;

                        add.Attributes.Append(key);
                        add.Attributes.Append(value);
                        _parent.AppendChild(add);
                    }
                    this._doc.Save(_path.ToString());
                    return true;
                }
                catch (Exception ex)
                {
                    LJH.GeneralLibrary.ExceptionHandling.ExceptionPolicy.HandleException(ex);
                }
            }
            return false;
        }

        public string GetConfigContent(string configName)
        {
            if (_parent != null)
            {
                try
                {
                    XmlNodeList nodeList = _parent.ChildNodes;
                    foreach (XmlNode xn in nodeList)
                    {
                        if (xn is XmlElement)
                        {
                            XmlElement xe = (XmlElement)xn;
                            if (xe.GetAttribute("key") == configName)
                            {
                                return xe.GetAttribute("value");
                            }
                        } // end if
                    }
                }
                catch (Exception ex)
                {
                    LJH.GeneralLibrary.ExceptionHandling.ExceptionPolicy.HandleException(ex);
                }
            }
            return "";
        }
        #endregion

        #region 时间日期相关的只读属性
        /// <summary>
        /// 获取本统计月开始的那天的日期
        /// </summary>
        public DateTime ThisMonthBegin
        {
            get
            {
                DateTime now = DateTime.Now;
                return new DateTime(YearOf(now), MonthOf(now), AppSettings.Current.MonthStart);
            }
        }

        /// <summary>
        /// 获取当前时间的月份,由于统计月份有可能不是从1号开始,所以统计月份与实际月份有可能有区别
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public int MonthOf(DateTime dt)
        {
            DateTime monthStart = new DateTime(dt.Year, dt.Month, AppSettings.Current.MonthStart); //统计月分在本月的开始日期
            if (dt >= monthStart) return dt.Month;
            int ret = dt.Month - 1;
            return ret > 0 ? ret : 12;
        }
        /// <summary>
        /// 获取当前时间的年份,由于统计月份有可能不是从1号开始,所以统计年份与实际年份有可能有区别
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public int YearOf(DateTime dt)
        {
            DateTime yearStart = new DateTime(dt.Year, 1, AppSettings.Current.MonthStart); //统计月分在本月的开始日期
            return dt >= yearStart ? dt.Year : dt.Year - 1;
        }
        #endregion
    }
}
