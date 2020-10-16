using Format;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainProgram
{
    /// <summary>
    /// List of DataBaseUtils.
    /// </summary>
    class DataBaseList :List<DataBaseUtils>
    {
        public DataBaseList(DbInfoList dbInfos)
        {
            foreach(DbInfo info in dbInfos)
            {
                Add(new DataBaseUtils(info));
            }
        }
        public void PostData(DataPackage dataPackage)
        {
            foreach(DataBaseUtils dataBaseUtils in this)
            {
                dataBaseUtils.PostData(dataPackage);
            }
        }
    }
}
