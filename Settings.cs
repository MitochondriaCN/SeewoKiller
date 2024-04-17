using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SeewoKiller
{
    internal static class Settings
    {
        internal static bool IsStartupQueueExecutable { get; private set; }
        internal static bool IsTimerEnabled { get; private set; }
        internal static List<Action> StartupQueue { get; private set; }

        internal static void LoadSettings()
        {
            try
            {
                if (File.Exists("settings.xml"))
                {
                    XDocument xd = XDocument.Load("settings.xml");

                    IsStartupQueueExecutable = bool.Parse(xd.Root.Element("IsStartupQueueExecutable").Value.ToString());
                    IsTimerEnabled = bool.Parse(xd.Root.Element("IsTimerEnabled").Value.ToString());

                    StartupQueue = new List<Action>();
                    foreach (var v in xd.Root.Element("StartupQueue").Descendants())
                    {
                        switch (v.Name.ToString())
                        {
                            case "HotspotAction":
                                HotspotAction ha = new HotspotAction
                                    (v.Attribute("HotspotActionType").Value == "On" ? HotspotAction.HotspotStatus.On : HotspotAction.HotspotStatus.Off,
                                    v.Attribute("HotspotId").Value,
                                    v.Attribute("HotspotPassword").Value);
                                StartupQueue.Add(ha);
                                break;
                            case "ResolutionAction":
                                ResolutionAction ra = new ResolutionAction(
                                    uint.Parse(v.Attribute("DPI").Value),
                                    uint.Parse(v.Attribute("XRes").Value),
                                    uint.Parse(v.Attribute("YRes").Value));
                                StartupQueue.Add(ra);
                                break;
                            case "SmartDesktopAction":
                                SmartDesktopAction sda = new SmartDesktopAction(
                                    bool.Parse(v.Attribute("IsNewsEnabled").Value),
                                    bool.Parse(v.Attribute("IsCustomWallpaperEnabled").Value));
                                StartupQueue.Add(sda);
                                break;
                        }
                    }
                }
                else
                {
                    CreateDefaultSettings();
                }
            }
            catch
            {
                new IOException("无法读取设置文件。");
            }
        }
        private static void CreateDefaultSettings()
        {
            XDocument xd = new XDocument();
            XElement root =
                new XElement("SeewoKillerSettings",
                    new XElement("IsStartupQueueExecutable", "true"),
                    new XElement("IsTimerEnabled", "true"),
                    new XElement("StartupQueue"));
            xd.Add(root);
            xd.Save("settings.xml");
            LoadSettings();
        }

        internal static void SaveStartupQueue()
        {
            XDocument xd = XDocument.Load("settings.xml");
            XElement dqueue = xd.Root.Element("StartupQueue");
            dqueue.RemoveNodes();
            foreach (var v in StartupQueue)
            {
                XElement e = new XElement(v.GetType().Name);
                foreach (var t in v.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    e.Add(new XAttribute(t.Name, t.GetValue(v)));
                }
                dqueue.Add(e);
            }
            xd.Save("settings.xml");
        }
    }
}
