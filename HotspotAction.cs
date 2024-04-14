using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows;
using System.Runtime;
using System.Windows;

namespace SeewoKiller
{
    /// <summary>
    /// 移动热点操作
    /// </summary>
    internal class HotspotAction : Action
    {
        internal enum HotspotStatus
        {
            On,
            Off
        }

        internal HotspotStatus HotspotActionType { get; private set; }
        internal string HotspotId { get; private set; }
        internal string HotspotPassword { get; private set; }
        internal override bool Execute()
        {
            /*
            var connectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            var tetheringManager = Windows.Networking.NetworkOperators.NetworkOperatorTetheringManager.CreateFromConnectionProfile(connectionProfile);
            var access = tetheringManager.GetCurrentAccessPointConfiguration();
            access.Ssid = "PWJ";
            access.Passphrase = "12345678";
            var result = await tetheringManager.StartTetheringAsync();
            if (result.Status == TetheringOperationStatus.Success)
            {
                //wifi热点开启成功
            }
            */
            MessageBox.Show("热点已执行：\nID:" + HotspotId + "\nPassword:" + HotspotPassword);
            return true;
        }

        public override string ToString()
        {
            return "移动热点" + HotspotActionType.ToString() + "，ID：" + HotspotId + "，密码：" + HotspotPassword;
        }

        internal HotspotAction(HotspotStatus hotspotActionType, string id = "16", string passwd = "class16senior2")
        {
            HotspotActionType = hotspotActionType;
            HotspotId = id;
            HotspotPassword = passwd;
        }
    }
}
