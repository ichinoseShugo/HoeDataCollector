using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib;

namespace HoeDataCollector
{
    class WiiManager
    {
        Wiimote Wii = new Wiimote();

        WiiManager()
        {
            Wii.WiimoteChanged += wm_WiimoteChanged;  //イベント関数の登録
            Wii.Connect();
            Wii.SetReportType(InputReport.ButtonsAccel, true);
        }

        void wm_WiimoteChanged(object sender, WiimoteChangedEventArgs args)
        {
            WiimoteState wiiState = args.WiimoteState;
            /*
            MainWindow.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    XAxis.Content = wiiState.AccelState.RawValues.X.ToString();
                    YAxis.Content = wiiState.AccelState.RawValues.Y.ToString();
                    ZAxis.Content = wiiState.AccelState.RawValues.Z.ToString();
                    XGyro.Content = wiiState.
                })
            );
            */
        }

    }
}
