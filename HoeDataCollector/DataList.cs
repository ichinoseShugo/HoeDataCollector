using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoeDataCollector
{
    public class DataList
    {
        /// <summary>
        /// 読み込んだファイル内の各列についている名前の配列
        /// </summary>
        public string[] DataNames = new string[0];
        /// <summary>
        /// 列の名前をkeyとしてvalueをdouble配列としたHash
        /// </summary>
        private Hashtable NameToData = new Hashtable();
        /// <summary>
        /// 列の名前をkeyとしてvalueを列の最大値と最小値([0]:max [1]:minのdouble配列)としたHash
        /// </summary>
        private Hashtable NameToMaxMin = new Hashtable();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="directoryPath"></param>
        public DataList()
        {
        }

        /// <summary>
        /// データの名前からdouble配列リスト([0]:X軸 [1]:Y軸)を取得
        /// </summary>
        /// <param name="DataName"></param>
        /// <returns></returns>
        public List<double[]> GetDataList(string DataName)
        {
            return (List<double[]>)NameToData[DataName];
        }

        /// <summary>
        /// データの名前から、そのデータ列の最大値を返す
        /// </summary>
        /// <param name="DataName"></param>
        /// <returns></returns>
        public double GetMax(string DataName)
        {
            return ((double[])NameToMaxMin[DataName])[0];
        }

        /// <summary>
        /// データの名前から、そのデータ列の最大値を返す
        /// </summary>
        /// <param name="DataName"></param>
        /// <returns></returns>
        public double GetMin(string DataName)
        {
            return ((double[])NameToMaxMin[DataName])[1];
        }
    }
}