﻿using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ACTTimeline
{
    public class TimelineController
    {
        private string timelineTxtFilePath;
        public string TimelineTxtFilePath
        {
            get { return timelineTxtFilePath; }
            set
            {
                if (value == null || value == "")
                    return;

                try
                {
                    if (!System.IO.File.Exists(value))
                        throw new ResourceNotFoundException(value);

                    timelineTxtFilePath = value;
                    Timeline = TimelineLoader.LoadFromFile(timelineTxtFilePath);
                }
                catch (Exception e)
                {
                    MessageBox.Show(String.Format("Failed to load timeline. Error: {0}", e.Message), "ACT Timeline Plugin");
                }
            }
        }

        private Timeline timeline;
        public Timeline Timeline
        {
            get { return timeline; }
            set
            {
                timeline = value;
                OnTimelineUpdate();
            }
        }

        public event EventHandler TimelineUpdate;
        public void OnTimelineUpdate()
        {
            CurrentTime = 0;

            if (TimelineUpdate != null)
                TimelineUpdate(this, EventArgs.Empty);
        }

        public event EventHandler CurrentTimeUpdate;
        public void OnCurrentTimeUpdate()
        {
            if (CurrentTimeUpdate != null)
                CurrentTimeUpdate(this, EventArgs.Empty);
        }

        bool forceSynchronizeOnce;

        private RelativeClock relativeClock;
        public double CurrentTime
        {
            get
            {
                return relativeClock.CurrentTime;
            }
            set
            {
                relativeClock.CurrentTime = value;
                forceSynchronizeOnce = true;
            }
        }
        
        private Timer timer;

        private bool paused;
        public bool Paused {
            get { return paused; }
            set {
                if (paused == value)
                    return;

                paused = value;
                OnPausedUpdate();
            }
        }

        public event EventHandler PausedUpdate;
        public void OnPausedUpdate()
        {
            if (!Paused)
            {
                // adjust relativeClock when unpaused
                relativeClock.CurrentTime = timeline.CurrentTime;
            }

            if (PausedUpdate != null)
                PausedUpdate(this, EventArgs.Empty);
        }

        public TimelineController()
        {
            timer = new Timer();
            timer.Tick += (object sender, EventArgs e) => { Synchronize(); };
            timer.Interval = 50;
            timer.Start();

            relativeClock = new RelativeClock();
            forceSynchronizeOnce = true;
            Paused = false;

            ActGlobals.oFormActMain.OnLogLineRead += act_OnLogLineRead;
        }

        private void act_OnLogLineRead(bool isImport, LogLineEventArgs logInfo)
        {
            if (isImport || timeline == null)
                return;

            string line = logInfo.logLine;
            foreach (TimelineAnchor anchor in timeline.ActiveAnchors())
            {
                if (anchor.Regex.IsMatch(line)) {
                    CurrentTime = anchor.TimeFromStart;
                    break;
                }
            }
        }

        public void Stop()
        {
            timer.Stop();
            timeline = null;

            ActGlobals.oFormActMain.OnLogLineRead -= act_OnLogLineRead;
        }

        private void Synchronize()
        {
            if (timeline == null)
                return;

            if (Paused && !forceSynchronizeOnce)
                return;

            forceSynchronizeOnce = false;
            timeline.CurrentTime = relativeClock.CurrentTime;

            OnCurrentTimeUpdate();
        }
    }
}
