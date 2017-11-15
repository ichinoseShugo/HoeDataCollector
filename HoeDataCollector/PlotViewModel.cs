using System;
using System.Collections;
using System.Collections.Generic;

using OxyPlot;
using OxyPlot.Series;

namespace HoeDataCollector
{
    class PlotViewModel
    {
        DataList MyDataList;
        public PlotModel MyPlotModel;
        /// <summary>
        /// 読み込んだファイル内の各列についている名前の配列
        /// </summary>
        public string[] DataNames;
        /// <summary>
        /// 名前をkeyとしてvalueをSeriesとしたHash
        /// </summary>
        private Hashtable NameToSeries = new Hashtable();
        /// <summary>
        /// Seriesの最大値
        /// </summary>
        public double Max = 0;
        /// <summary>
        /// Seriesの最小値
        /// </summary>
        public double Min = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataList"></param>
        public PlotViewModel(DataList dataList)
        {
            this.MyDataList = dataList;
            this.MyPlotModel = new PlotModel();
            this.DataNames = MyDataList.DataNames;

            foreach (var dataname in this.DataNames)
            {
                CreateLineSeries(dataname, 1, MyDataList.GetDataList(dataname));
            }
            CreateLineSeries("ImagePosition", 1, new List<double[]>() { new double[] { 0, 0 }, new double[] { 0, 0 }, });
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataList"></param>
        public PlotViewModel()
        {
            this.MyPlotModel = new PlotModel();
            this.DataNames = MyDataList.DataNames;

            foreach (var dataname in this.DataNames)
            {
                CreateLineSeries(dataname, 1, MyDataList.GetDataList(dataname));
            }
            CreateLineSeries("ImagePosition", 1, new List<double[]>() { new double[] { 0, 0 }, new double[] { 0, 0 }, });
        }

        /// <summary>
        /// DataNameとDataListからSeriesのを作成しDataNameと紐付ける
        /// </summary>
        /// <param name="dataName"></param>
        /// <param name="tickness"></param>
        /// <param name="dataList"></param>
        public void CreateLineSeries(String dataName, int tickness, List<double[]> dataList)
        {
            IList<DataPoint> points = new List<DataPoint>();
            foreach (var list in dataList) points.Add(new DataPoint(list[0], list[1]));
            LineSeries lineSeries = new LineSeries
            {
                Title = dataName,
                StrokeThickness = tickness,
                ItemsSource = points,
            };
            NameToSeries.Add(dataName, lineSeries);
        }

        /// <summary>
        /// 描画するSeriesを追加
        /// </summary>
        /// <param name="dataName"></param>
        /// <param name="dataList"></param>
        public void AddLineSeries(String dataName)
        {
            this.MyPlotModel.Series.Add((LineSeries)NameToSeries[dataName]);
        }

        /// <summary>
        /// Seriesを削除
        /// </summary>
        public void RemoveSeries(string dataName)
        {
            int index = this.MyPlotModel.Series.IndexOf((LineSeries)NameToSeries[dataName]);
            this.MyPlotModel.Series.RemoveAt(index);
        }

        /// <summary>
        /// すべてのSeriesをクリア
        /// </summary>
        public void ClearSeries()
        {
            if (this.MyPlotModel.Series.Count > 0) this.MyPlotModel.Series.Clear();
        }

        /// <summary>
        /// 選択されているSeries中の最大値と最小値を求める
        /// </summary>
        public void SetSlectedSeries(List<string> selectedNames)
        {
            if (selectedNames.Count < 1)
            {
                Max = 0;
                Min = 0;
                return;
            }
            double max = MyDataList.GetMax(selectedNames[0]);
            double min = MyDataList.GetMin(selectedNames[0]);
            foreach (var name in selectedNames)
            {
                if (max < MyDataList.GetMax(name)) max = MyDataList.GetMax(name);
                if (min > MyDataList.GetMin(name)) min = MyDataList.GetMin(name);
            }
            Max = max;
            Min = min;
        }

        /// <summary>
        /// DataNameのSeriesが含まれているかを返す
        /// </summary>
        /// <param name="dataName"></param>
        /// <returns></returns>
        public bool Contains(string dataName)
        {
            return this.MyPlotModel.Series.Contains((LineSeries)NameToSeries[dataName]);
        }

        /// <summary>
        /// モデルの取得
        /// </summary>
        /// <returns></returns>
        public PlotModel GetModel()
        {
            return MyPlotModel;
        }
    }
}