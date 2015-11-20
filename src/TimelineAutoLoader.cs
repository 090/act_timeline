using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ACTTimeline
{
    public class TimelineAutoLoader
    {
        private string m_currentzone = string.Empty;
        private bool m_autoload = false;
        public bool Autoload { get; set; }
        public bool AutoShow { get; set; }
        public bool AutoHide { get; set; }
        public string FindFilename { get; set; }
        public event EventHandler ZoneChange;
        private Timer timer;
        private ACTPlugin plugin;
        public TimelineAutoLoader(ACTPlugin _plugin)
        {
            plugin = _plugin;
            timer = new Timer();
            timer.Interval = 5000;
            timer.Tick += timer_CheckCurrentZone;
        }

        void timer_CheckCurrentZone(object sender, EventArgs e)
        {
            if (!Autoload)
                return;
            var zonename = ActGlobals.oFormActMain.CurrentZone;
            if (zonename.Length == 0)
                return;
            if (m_currentzone != zonename)
            {
                var file = zonename;
                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    file = file.Replace(c, '_');
                }
                FindFilename = file;

                if (ZoneChange != null)
                    ZoneChange(this, EventArgs.Empty);

                file = String.Format("{0}/{1}.txt", Globals.TimelineTxtsRoot, FindFilename);
                if (System.IO.File.Exists(file))
                {
                    plugin.Controller.Paused = true;
                    plugin.Controller.TimelineTxtFilePath = file;
                    m_autoload = true;
                    if (AutoShow)
                        plugin.TimelineView.Show();
                }
                else
                {
                    if (m_autoload && AutoHide)
                        plugin.TimelineView.Hide();
                    m_autoload = false;
                }
                m_currentzone = zonename;
            }
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }
    }
}
