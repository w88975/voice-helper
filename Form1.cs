using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VoiceHelper
{
    public partial class Form1 : Form
    {
        VoiceUtils voiceUtils = VoiceUtils.Instance;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;

        public Form1()
        {
            // 启动SocketServer（只需访问一次Instance即可自动启动）
            var server = VoiceHelper.SocketServer.Instance;
            server.OnMessageReceived += (clientId, message) =>
            {
                this.Invoke(new Action(() =>
                {
                    int deviceIndex = this.comboBoxDevice.SelectedIndex;

                    if (message == "start")
                    {
                        voiceUtils.StartRecord(deviceIndex, 16000, 2);
                    }
                    else if (message == "stop")
                    {
                        voiceUtils.StopRecord();
                    }

                    Console.WriteLine($"收到客户端[{clientId}]消息: {message}");
                }));
            };

            voiceUtils.OnRecording += (channels) =>
            {
                this.Invoke(new Action(() =>
                {
                    // 处理录音数据
                    foreach (var channel in channels)
                    {
                        Console.WriteLine($"Channel {channel.Channel}: Buffer Length = {channel.Buffer.Length}");
                    }
                }));
            };

            InitializeComponent();
            InitNotifyIcon();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var devices = voiceUtils.GetInputDevices();

            foreach (var device in devices)
            {
                this.comboBoxDevice.Items.Add(device);
            }

            if (this.comboBoxDevice.Items.Count > 0)
            {
                this.comboBoxDevice.SelectedIndex = 0;
            }
        }

        private void InitNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = SystemIcons.Application;
            notifyIcon.Text = "VoiceHelper";
            notifyIcon.Visible = true;

            // 右键菜单
            contextMenu = new ContextMenuStrip();
            var showItem = new ToolStripMenuItem("显示主界面", null, (s, e) => ShowMainWindow());
            var exitItem = new ToolStripMenuItem("退出", null, (s, e) => Application.Exit());
            contextMenu.Items.Add(showItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(exitItem);

            notifyIcon.ContextMenuStrip = contextMenu;

            notifyIcon.MouseClick += NotifyIcon_MouseClick;
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowMainWindow();
            }
            // 右键由 ContextMenuStrip 自动处理，无需手动弹出
        }

        private void ShowMainWindow()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            this.Show();
            this.Activate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 用户点击关闭按钮时，隐藏到托盘，不退出进程
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                notifyIcon.ShowBalloonTip(1000, "VoiceHelper", "程序已最小化到托盘", ToolTipIcon.Info);
            }
            else
            {
                base.OnFormClosing(e);
            }
        }
    }
}