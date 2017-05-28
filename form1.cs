using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
namespace WindowsFormsApp1
{

    public partial class Form1 : Form
    {
        const int PROCESS_WM_READ = 0x0010;
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        Thread checkManaTr;
        Thread antiKickTr;

        int mana_addr;
        IntPtr processHandle;

        bool antiKickRunning=false;
        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Process process = Process.GetProcessesByName("dbko v6.1")[0];
            processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);
            IntPtr basead = process.MainModule.BaseAddress;
            int base_addr = basead.ToInt32();
            int bytesRead = 0;
            byte[] manabyte = new byte[4];
            mana_addr = base_addr + 0x235EF0;

            int mana = 0;
            int experience = 0;
            // 0x0046A3B8 is the address where I found the string, replace it with what you found
            ReadProcessMemory((int)processHandle, mana_addr, manabyte, 4, ref bytesRead);
            mana = BitConverter.ToInt32(manabyte, 0);
            Console.WriteLine("MANA: " + mana);

            antiKickTr = new Thread(new ThreadStart(anti_kick));
            checkManaTr = new Thread(new ThreadStart(updateMana));
            checkManaTr.Start();
        }

        private void updateMana()
        {
            int mana = 0;
            byte[] manabyte = new byte[4];
            int bytesRead = 0;

            while (!IsClosed)
            {
                ReadProcessMemory((int)processHandle, mana_addr, manabyte, 4, ref bytesRead);
                mana = BitConverter.ToInt32(manabyte, 0);
                try
                {
                    label1.Invoke((MethodInvoker)delegate
                    {
                        // Running on the UI thread
                        label1.Text = "Mana: " + mana + "";
                    });
                }
                catch(Exception e)
                {
                    break;
                }
            }
            //this.label1.Text = "Mana: " + mana+"";
        }
        public static void sendKeystroke(ushort k)
        {
            const uint WM_KEYDOWN = 0x100;
            const uint WM_SYSCOMMAND = 0x018;
            const uint SC_CLOSE = 0x053;

            IntPtr WindowToFind = FindWindow(null, "DBKO Player");

            IntPtr result3 = SendMessage(WindowToFind, WM_KEYDOWN, ((IntPtr)k), (IntPtr)0);
            //IntPtr result3 = SendMessage(WindowToFind, WM_KEYUP, ((IntPtr)c), (IntPtr)0);
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void anti_kick()
        {
		   
			const uint WM_KEYDOWN = 0x100;
			const uint WM_KEYUP = 0x101;
			const uint WM_SYSCOMMAND = 0x018;
			const uint SC_CLOSE = 0x053;
           
			String name=Process.GetProcessesByName("dbko v6.1")[0].MainWindowTitle;
			IntPtr WindowToFind = FindWindow(null,name);

			Console.WriteLine(WindowToFind + " windowtofind");
            while (antiKickRunning)
			{

                if (antiKickRunning) PostMessage(WindowToFind, WM_KEYDOWN, ((IntPtr)Keys.ControlKey), (IntPtr)0);
                if (antiKickRunning) PostMessage(WindowToFind, WM_KEYDOWN, ((IntPtr)Keys.Left), (IntPtr)0);
                if (antiKickRunning) PostMessage(WindowToFind, WM_KEYUP, ((IntPtr)Keys.Left), (IntPtr)0);
                if (antiKickRunning) PostMessage(WindowToFind, WM_KEYUP, ((IntPtr)Keys.ControlKey), (IntPtr)0);
                if (antiKickRunning) await Task.Delay(2000);
                if (antiKickRunning) PostMessage(WindowToFind, WM_KEYDOWN, ((IntPtr)Keys.ControlKey), (IntPtr)0);
                if (antiKickRunning) PostMessage(WindowToFind, WM_KEYDOWN, ((IntPtr)Keys.Right), (IntPtr)0);
                if (antiKickRunning) PostMessage(WindowToFind, WM_KEYUP, ((IntPtr)Keys.Right), (IntPtr)0);
                if (antiKickRunning) PostMessage(WindowToFind, WM_KEYUP, ((IntPtr)Keys.ControlKey), (IntPtr)0);
			}
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                antiKickRunning = true;
                antiKickTr.Start();
            }
            else
            {
                antiKickRunning = false;
                Console.WriteLine("ending");
            }
        }
    }
}
