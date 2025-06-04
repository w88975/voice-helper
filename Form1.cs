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
        public Form1()
        {
            // 启动SocketServer（只需访问一次Instance即可自动启动）
            var server = VoiceHelper.SocketServer.Instance;
            server.OnMessageReceived += (clientId, message) =>
            {
                // 这里可以处理收到的消息，比如输出到日志
                Console.WriteLine($"收到客户端[{clientId}]消息: {message}");
            };
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            var devices = VoiceHelper.VoiceUtils.GetInputDevices();

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
