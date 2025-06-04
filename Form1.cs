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
        public Form1()
        {
            // 启动SocketServer（只需访问一次Instance即可自动启动）
            var server = VoiceHelper.SocketServer.Instance;
            server.OnMessageReceived += (clientId, message) =>
            {
                this.Invoke(new Action(() =>
                {
                    int deviceIndex = this.comboBox1.SelectedIndex;

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

            voiceUtils.OnRecording+= (channels) =>
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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            var devices = voiceUtils.GetInputDevices();

            foreach (var device in devices)
            {
                this.comboBox1.Items.Add(device);
            }

            if (this.comboBox1.Items.Count > 0)
            {
                this.comboBox1.SelectedIndex = 0;
            }
        }
    }
}
