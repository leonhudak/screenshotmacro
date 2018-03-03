﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using System.Threading;

namespace screenshotmacro
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            btnStopClick.Enabled = false;
            timer1.Enabled = false;
        }

        #region Dll imports
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        #endregion

        #region Mouse Click
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;

        public static void move(int xDelta, int yDelta)
        {
            mouse_event(MOUSEEVENTF_MOVE, xDelta, yDelta, 0, 0);
        }

        public static void LeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y, 0, 0);
        }
        #endregion

        Bitmap ss;
        Tuple<int, int> pos;
        bool onlyApply = true;

        private void Form1_Load(object sender, EventArgs e)
        {
            ss = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            timer1.Interval = 1000;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Form1.RegisterHotKey(this.Handle, 0, 0x0002, 0x7B); // Ctrl + F12  

            if (cbBoth.Checked)
                onlyApply = false;

            if (loggerIsRunning())
            {
                //DialogResult dialogResult = MessageBox.Show("Do you want to open the Logger?", "Logger not running", MessageBoxButtons.YesNo);
                //if (dialogResult == DialogResult.Yes)
                //{
                //    //   Process.Start(loggerpath);
                //}
                //else if (dialogResult == DialogResult.No)
                //{
                //    MessageBox.Show("k den bb");
                //    Application.Exit();
                //}

                //   Form1.RegisterHotKey(this.Handle, 1, 0x0000, 0x20);
                //                            handle, id, modifier key, key
                // id - your own numeber to differentiate between hotkeys
                // modifier - ctrl, shift, alt etc.
                //

                if (rbSpace.Checked)
                {
                    Form1.RegisterHotKey(this.Handle, 1, 0x0000, 0x20); // Space
                    this.Visible = false;
                    MessageBox.Show("To pause/disable the hotkey, press Ctrl + F12." +
                             "\nHotkeys won't work unless the Logger is open and in fullscreen mode.");
                }
                else if (rbCtrlE.Checked)
                {
                    Form1.RegisterHotKey(this.Handle, 1, 0x0002, 0x45); // Ctrl + E
                    this.Visible = false;
                    MessageBox.Show("To pause/disable the hotkey, press Ctrl + F12." +
                       "\nHotkeys won't work unless the Logger is open and in fullscreen mode.");
                }
                else if (rbBackSlash.Checked)
                {
                    Form1.RegisterHotKey(this.Handle, 1, 0x0000, 0xDC); //  \ 
                    this.Visible = false;
                    MessageBox.Show("To pause/disable the hotkey, press Ctrl + F12." +
                                        "\nHotkeys won't work unless the Logger is open and in fullscreen mode.");
                }
                else if (rbCtrlF.Checked)
                {
                    Form1.RegisterHotKey(this.Handle, 1, 0x0002, 0x46); // Ctrl + F
                    this.Visible = false;
                    MessageBox.Show("To pause/disable the hotkey, press Ctrl + F12." +
                        "\nHotkeys won't work unless the Logger is open and in fullscreen mode.");
                }
                else
                    MessageBox.Show("Please select a hotkey.", "Error");
            }
            else
                MessageBox.Show("Please open the Logger first.", "Error");
        }

        public static bool isProcessFocused(int processId)
        {
            //copied code i dont understand lol
            IntPtr fgWin = GetForegroundWindow();
            if (fgWin == IntPtr.Zero)
            {
                return false;
            }
            int fgProc;
            GetWindowThreadProcessId(fgWin, out fgProc);
            return processId == fgProc;
        }

        private bool loggerIsRunning()
        {
            Process[] logger = Process.GetProcessesByName("Synergy.BaseballLogger.WinApp");
            if (logger.Length == 0)
                return false;
            else
                return true;
        }

        private Tuple<int, int> GetPixels()
        {
            // makes a pair of x,y for where the color was found in the screenshot

            // green from the apply button
            Color c = Color.FromArgb(30, 165, 46);

            for (int y = 5 * Screen.PrimaryScreen.Bounds.Height / 6; y < ss.Height; y++)
                for (int x = 2 * Screen.PrimaryScreen.Bounds.Width / 5; x < ss.Width; x++)
                    if (ss.GetPixel(x, y) == c)
                    {
                        pos = Tuple.Create(x, y);
                        return pos;
                    }
            return null;
        }

        protected override void WndProc(ref Message m)
        {
            // processes windows messages

            Process[] logger = Process.GetProcessesByName("Synergy.BaseballLogger.WinApp");
            if (m.Msg == 0x0312) // detect a hotkey
            {
                IntPtr i = m.WParam; // which hotkey was pressed
                if (logger.Length != 0) // if logger is running because isProcessFocused(logger[0].Id) breaks if array is empty
                {
                    if (isProcessFocused(logger[0].Id)) //if logger is focused
                    {
                        if ((int)i == 1)
                        {
                            Graphics g = Graphics.FromImage(ss);
                            Size s = new Size(ss.Width, ss.Height);
                            g.CopyFromScreen(0, 0, 0, 0, s);
                            //ss.Save(@"C:\Users\Leon\Desktop\ss.png");
                            GetPixels();
                            this.Cursor = new Cursor(Cursor.Current.Handle);
                            Point firstPos = Cursor.Position;
                            if (pos != null)
                            {
                                if (onlyApply)
                                {
                                    Cursor.Position = new Point(pos.Item1 + 22, pos.Item2 + 13);
                                    LeftClick();
                                    Cursor.Position = firstPos;
                                }
                                else
                                {
                                    Cursor.Position = new Point(pos.Item1 + 22, pos.Item2 + 13);
                                    LeftClick();
                                    Cursor.Position = new Point(pos.Item1 + 253, pos.Item2 + 13);
                                    LeftClick();
                                    Cursor.Position = firstPos;
                                }
                            }
                        }
                        else
                        {
                            this.Visible = true;
                            Form1.UnregisterHotKey(this.Handle, 1);
                            Form1.UnregisterHotKey(this.Handle, 0);
                        }
                    }
                    else
                    {
                        if ((int)i == 0)
                        {
                            this.Visible = true;
                            Form1.UnregisterHotKey(this.Handle, 1);
                            Form1.UnregisterHotKey(this.Handle, 0);
                        }
                    }
                }
            }
            base.WndProc(ref m);
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            fHelp h = new fHelp();
            h.Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Process[] dashboard = Process.GetProcessesByName("Synergy Logger Dashboard");
            if (isProcessFocused(dashboard[0].Id))
                LeftClick();       
        }

        private void btnStartClick_Click(object sender, EventArgs e)
        {
            timer1.Start();
            btnStopClick.Enabled = true;
            btnStartClick.Enabled = false;
        }

        private void btnStopClick_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            btnStopClick.Enabled = false;
            btnStartClick.Enabled = true;
        }
    }
}
