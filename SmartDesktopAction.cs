using Aspose.Imaging;
using Aspose.Imaging.ImageOptions;
using System;
using System.IO;
using System.Net;
using Aspose.Imaging.Brushes;
using Aspose.Imaging.Shapes;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Text;
using System.Collections.Generic;

namespace SeewoKiller
{
    internal class SmartDesktopAction : Action
    {
        internal bool IsNewsEnabled { get; private set; }
        internal bool IsCustomWallpaperEnabled { get; private set; }

        public override string ToString()
        {
            return "智能桌面，要闻" + (IsNewsEnabled ? "开，" : "关，") + (IsCustomWallpaperEnabled ? "" : "不") + "使用wallpapers图片";
        }

        internal SmartDesktopAction(bool isNewsEnabled, bool isCustomWallpaperEnabled)
        {
            IsNewsEnabled = isNewsEnabled;
            IsCustomWallpaperEnabled = isCustomWallpaperEnabled;
        }

        internal override bool Execute()
        {
            Image i;
            if (!IsCustomWallpaperEnabled)
            {
                //下载图片
                HttpWebRequest req = WebRequest.Create("https://api.kdcc.cn/") as HttpWebRequest;
                System.Drawing.Image _im;
                using (Stream s = req.GetResponse().GetResponseStream())
                    _im = System.Drawing.Image.FromStream(s);
                _im.Save("wallpaper.jpg");
                //读取
                i = Image.Load("wallpaper.jpg");
            }
            else
            {
                //自定义壁纸
                if (Directory.Exists("wallpapers\\"))
                {
                    string[] wallpapers = Directory.GetFiles("wallpapers\\", "*.jpg");
                    if (wallpapers.Length > 0)
                    {
                        string selectedpath = wallpapers[new Random().Next(wallpapers.Length)];
                        i = Image.Load(selectedpath);
                        i.Resize(1920, 1080, ResizeType.HighQualityResample);
                    }
                    else
                    {
                        throw new FileNotFoundException("于" + Path.GetFullPath("wallpapers\\") + "中未找到有效jpg图片。");
                    }
                }
                else
                {
                    Directory.CreateDirectory("wallpapers\\");
                    throw new FileNotFoundException("于" + Path.GetFullPath("wallpapers\\") + "中未找到有效jpg图片。");
                }
            }

            //画图
            DeployWallpaper(i);
            if (IsNewsEnabled)
                DeployNews(i);

            //保存并设置壁纸
            i.Save("wallpaper_edited.bmp",
                new BmpOptions() { BitsPerPixel = 24 });
            SetWallPaper(Path.GetFullPath("wallpaper_edited.bmp"), Style.Stretch);

            return true;
        }

        /// <summary>
        /// 画高考倒计时
        /// 接口：https://api.kdcc.cn/
        /// </summary>
        private void DeployWallpaper(Image wallpaper)
        {
            Graphics g = new Graphics(wallpaper);
            g.PageUnit = GraphicsUnit.Pixel;
            Font yahei = new Font("Microsoft YaHei UI", 40, GraphicsUnit.Pixel);
            Font arial = new Font("Arial", 150, GraphicsUnit.Pixel);

            //高考倒计时背景矩形
            Figure f = new Figure();
            f.AddShape(new RectangleShape(new RectangleF(1620, 40, 260, 260)));
            GraphicsPath path = new GraphicsPath(new Figure[] { f });
            g.DrawPath(new Pen(Color.FromArgb(223, 230, 233)), path);//https://flatuicolors.com/palette/us
            g.FillPath(new SolidBrush(Color.FromArgb(223, 230, 233)), path);

            //高考倒计时文字
            SizeF text = g.MeasureString("高考倒计时", yahei, SizeF.Empty, StringFormat.GenericDefault);
            g.DrawString("高考倒数日",
                yahei,
                new SolidBrush(Color.FromArgb(99, 110, 114)),
                1620 + (260 - text.Width) / 2,
                60);
            SizeF countdown = g.MeasureString(new TimeSpan(new DateTime(2024, 6, 7).Ticks - DateTime.Now.Ticks).Days.ToString(),
                arial, SizeF.Empty, StringFormat.GenericDefault);
            g.DrawString((new TimeSpan(new DateTime(2024, 6, 7).Ticks - DateTime.Now.Ticks).Days + 1).ToString(),
                arial,
                new SolidBrush(Color.FromArgb(9, 132, 227)),
                1620 + (260 - countdown.Width) / 2,
                (260 - text.Height - countdown.Height) / 2 + 55 + text.Height); 
        }

        /// <summary>
        /// 画新闻
        /// 接口：https://top.baidu.com/api/board
        /// </summary>
        private void DeployNews(Image wallpaper)
        {
            HttpWebRequest baidureq = WebRequest.CreateHttp("https://top.baidu.com/api/board");
            string baiduresp;
            using (StreamReader s = new StreamReader(baidureq.GetResponse().GetResponseStream(), Encoding.UTF8))
                baiduresp = s.ReadToEnd();
            XDocument xd = JsonConvert.DeserializeXNode(baiduresp, "root");
            List<string> hotlist = new List<string>();
            //遍历各热点条目，取出放入List
            foreach (var v in xd.Root.Element("data").Element("cards").Descendants("content"))
            {
                hotlist.Add(v.Element("word").Value);
            }
            //获取头条条目
            string top = xd.Root.Element("data").Element("cards").Element("topContent").Element("word").Value;

            //绘图开始
            Font yahei18 = new Font("Microsoft YaHei UI", 18, FontStyle.Bold, GraphicsUnit.Pixel);
            Font yahei20 = new Font("Microsoft YaHei UI", 20, FontStyle.Regular, GraphicsUnit.Pixel);
            Graphics g = new Graphics(wallpaper);
            g.PageUnit = GraphicsUnit.Pixel;
            SolidBrush emphasizebrush = new SolidBrush(Color.FromArgb(9, 132, 227));//强调色 https://flatuicolors.com/palette/us
            SolidBrush textbrush = new SolidBrush(Color.FromArgb(45, 52, 54));//正文色

            ////测量阶段
            //要闻标题
            float titleheight = g.MeasureString("要闻", yahei18, SizeF.Empty, StringFormat.GenericDefault).Height;

            //正文
            string fulltext = "";
            fulltext += "★ " + top;
            foreach (var v in hotlist.GetRange(0,5))
            {
                fulltext += "\n" + "● " + v;
            }
            RectangleF textrect = new RectangleF(1630, 310 + titleheight + 3, 240, 1000);
            float textheight = g.MeasureString(fulltext, yahei20, textrect.Size, StringFormat.GenericDefault).Height;

            ////绘制阶段
            //背景矩形 1620,300,260,(上下间距各10)
            Figure f = new Figure();
            f.AddShape(new RectangleShape(new RectangleF(1620, 300, 260, titleheight + textheight + 10 + 10 + 3)));
            GraphicsPath path = new GraphicsPath(new Figure[] { f });
            g.DrawPath(new Pen(Color.FromArgb(223, 230, 233)), path);
            g.FillPath(new SolidBrush(Color.FromArgb(223, 230, 233)), path);
            //标题
            g.DrawString("要闻", yahei18, emphasizebrush, 1630, 310);
            //正文
            g.DrawString(fulltext,
                yahei20,
                textbrush,
                textrect,
                StringFormat.GenericDefault);


        }
        #region 壁纸设置 from 博客园
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Fill,
            Fit,
            Span,
            Stretch,
            Tile,
            Center
        }

        private static void SetWallPaper(string wpaper, Style style)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
                if (style == Style.Fill)
                {
                    key.SetValue(@"WallpaperStyle", 10.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }
                if (style == Style.Fit)
                {
                    key.SetValue(@"WallpaperStyle", 6.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }
                if (style == Style.Span) // Windows 8 or newer only!
                {
                    key.SetValue(@"WallpaperStyle", 22.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }
                if (style == Style.Stretch)
                {
                    key.SetValue(@"WallpaperStyle", 2.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }
                if (style == Style.Tile)
                {
                    key.SetValue(@"WallpaperStyle", 0.ToString());
                    key.SetValue(@"TileWallpaper", 1.ToString());
                }
                if (style == Style.Center)
                {
                    key.SetValue(@"WallpaperStyle", 0.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                    0,
                    wpaper,
                    SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
        #endregion
    }
}
