using System;
using System.Linq;
using Advanced_Combat_Tracker;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace NoKill_ACT_Plugin
{
    public class NoKillPlugin : IActPluginV1
    {

        FFXIV_ACT_Plugin.FFXIV_ACT_Plugin ffxivPlugin;
        bool initialized = false;
        Label statusLabel;
        PluginControl control;
        Process FFXIV;

        public IntPtr LobbyErrorHandler { get; private set; }
        public IntPtr StartHandler { get; private set; }
        public IntPtr LoginHandler { get; private set; }

        private delegate Int64 StartHandlerDelegate(Int64 a1, Int64 a2);
        private delegate Int64 LoginHandlerDelegate(Int64 a1, Int64 a2);
        private delegate char LobbyErrorHandlerDelegate(Int64 a1, Int64 a2, Int64 a3);

        private object GetFfxivPlugin()
        {
            ffxivPlugin = null;

            var plugins = ActGlobals.oFormActMain.ActPlugins;
            foreach (var plugin in plugins)
                if (plugin.pluginFile.Name.ToUpper().Contains("FFXIV_ACT_Plugin".ToUpper()) )
                    ffxivPlugin = (FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)plugin.pluginObj;

            if (ffxivPlugin == null)
                throw new Exception("Could not find FFXIV plugin. Make sure that it is loaded before NoKillPlugin.");


            return ffxivPlugin;
        }

        public void DeInitPlugin()
        {
            if (initialized)
            {
                statusLabel.Text = "Exit :|";
            }
            else
            {
                statusLabel.Text = "Error :(";
            }
        }

        public void InitPlugin(TabPage pluginScreenSpace, System.Windows.Forms.Label pluginStatusText)
        {
            statusLabel = pluginStatusText;
            GetFfxivPlugin();
            control = new PluginControl();
            pluginScreenSpace.Text = "No Kill Plugin";
            pluginScreenSpace.Controls.Add(control);

            FFXIV = ffxivPlugin.DataRepository.GetCurrentFFXIVProcess()
                        ?? Process.GetProcessesByName("ffxiv_dx11").FirstOrDefault();
            if (FFXIV == null)
            {
                throw new Exception("Could not find ffxiv_dx11.exe process. Make sure you are running the game in DX11.");
            }
            Log("Info", $"检测到 {FFXIV.ProcessName} PID:{FFXIV.Id}");

            var exePath = ActGlobals.oFormActMain.PluginGetSelfData(this).pluginFile.FullName.Replace("NoKill_ACT_Plugin.dll", "NoKill.exe");
            // Process.Start(exePath, FFXIV.Id.ToString());
            Process cmd = new Process();
            cmd.StartInfo.FileName = exePath;
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.Arguments = FFXIV.Id.ToString();
            cmd.Start();

            string tips = "本插件免费，发布及更新地址 https://file.bluefissure.com/FFXIV/ 或 https://ngabbs.com/read.php?tid=30326362 ，勿从其他渠道（闲鱼卖家或神秘群友）获取以避免虚拟财产受到损失。";
            // MessageBox.Show(tips);
            Log("Info", tips);
            statusLabel.Text = "Happy 6.0 :)";
        }

        void Log(string type, string message)
        {
            var time = (DateTime.Now).ToString("HH:mm:ss");
            var text = $"[{time}] [{type}] {message.Trim()}";
            control.textBoxLog.Text += text + Environment.NewLine;
            control.textBoxLog.SelectionStart = control.textBoxLog.TextLength;
            control.textBoxLog.ScrollToCaret();
            text = $"00|{DateTime.Now.ToString("O")}|0|NoKillPlugin-{message}|";        //解析插件数据格式化
            ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, $"{text}");    //插入ACT日志
        }

    }
}
