using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Speech.Synthesis;
using System.IO;
using System.Diagnostics;

namespace TextToSpeech
{
    /// <summary>
    /// Program to easily pass strings to Microsoft installed speech bots and then pass that speech onto voice programs such as skype/discord/etc.
    /// Plays the created soundclip back to you and through selected device.
    /// Can be easily minimized and maximized by pressing LCntrl + Space for quick responses when doing other activities
    /// Warning: Will need a virtual audio cable to enable play to voice programs. I use the free one located at https://www.vb-audio.com/Cable/
    /// After installation, you need to route the output to the cable, and then in your voice program choose the cable in the input device.
    /// Eventually will write own virtual audio cable and splice it into the program for one stop quick and easy use, but for now this works.
    /// </summary>
    public partial class MainWindow : Window
    {
        string wFile = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".wav"; //one file for playing to virtual cable/selected device
        string wFile2 = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".wav"; //one file for playback to default device for user
        NAudio.Wave.WaveFileReader waveReader = null;
        NAudio.Wave.WaveFileReader waveReaderToMe = null;
        NAudio.Wave.WaveOut waveOut = null;
        NAudio.Wave.WaveOut waveToMe = null;
        bool cntrlFlag = false;
        bool spaceFlag = false;

        private LowLevelKeyboardListener _listener;

        public MainWindow()
        {
            InitializeComponent();

            //loads the Keyboard Hook class
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += _listener_OnKeyPressed;
            _listener.HookKeyboard();
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Window_Closing);

            txtB.Focus();
        }
        private void cBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> voices = new List<string>();
            SpeechSynthesizer synth = new SpeechSynthesizer();
            foreach (var voice in synth.GetInstalledVoices())//Fix to only get proper voices is to build for x64. Project -> Properties ->Build
            {
                voices.Add(voice.VoiceInfo.Name);
            }
            synth.SetOutputToNull();
            cBox.ItemsSource = voices;
            cBox.SelectedIndex = 0;
        }

        private void cBox_Changed(object sender, SelectionChangedEventArgs e)//when comboBox is changed, auto focuses on the textBox
        {
            txtB.Focus();
        }

        private void deviceComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> devices = new List<string>();
            int virtIndex = 0;

            for (int i = 0; i < NAudio.Wave.WaveOut.DeviceCount; i++)
            {
                string getcap = NAudio.Wave.WaveOut.GetCapabilities(i).ProductName;
                devices.Add(getcap);
                if( getcap.Contains("Virtual") || getcap.Contains("virtual") )
                {
                    virtIndex = i;
                }
            }

            deviceComboBox.ItemsSource = devices;
            deviceComboBox.SelectedIndex = virtIndex;
        }

        private void deviceComboBox_Changed(object sender, SelectionChangedEventArgs e)//when comboBox is changed, auto focuses on the textBox
        {
            txtB.Focus();
        }

        void _listener_OnKeyPressed(object sender, KeyPressedArgs e)//checks if cntrl and flag are pressed concurrently. if so mins or maxs program
        {
            if (e.KeyPressed == Key.LeftCtrl)
            {
                cntrlFlag = true;
            }
            else if (e.KeyPressed == Key.Space)
            {
                spaceFlag = true;
            }
            else
            {
                cntrlFlag = false; spaceFlag = false;
            }

            if (cntrlFlag && spaceFlag)
            {
                if (this.WindowState == WindowState.Minimized)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                    txtB.Focus();
                }
                else
                {
                    this.WindowState = WindowState.Minimized;
                }
                cntrlFlag = false; spaceFlag = false;
            }
        }
        
        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            clearWave();
            File.Delete(wFile); File.Delete(wFile2);
            _listener.UnHookKeyboard();
        }


        private void clearWave()
        {
            if (waveOut != null && waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }
            if (waveToMe != null && waveToMe.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                waveToMe.Stop();
                waveToMe.Dispose();
                waveToMe = null;
            }
            if (waveReader != null)
            {
                waveReader.Dispose();
                waveReader = null;
            }
            if (waveReaderToMe != null)
            {
                waveReaderToMe.Dispose();
                waveReaderToMe = null;
            }
        }
        private void txtB_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                clearWave();
                SpeechSynthesizer synth = new SpeechSynthesizer();

                synth.SelectVoice(cBox.SelectedItem.ToString());
                synth.SetOutputToWaveFile(wFile);
                synth.Speak(txtB.Text);
                synth.SetOutputToNull();

                File.Copy(wFile, wFile2, true);

                waveReader = new NAudio.Wave.WaveFileReader(wFile); 
                waveReaderToMe = new NAudio.Wave.WaveFileReader(wFile2);

                waveOut = new NAudio.Wave.WaveOut(); //for playing to selected device
                waveToMe = new NAudio.Wave.WaveOut(); //for playback to user through default device

                waveOut.DeviceNumber = deviceComboBox.SelectedIndex;

                waveOut.Init(waveReader);
                waveOut.Play();

                waveToMe.Init(waveReaderToMe);
                waveToMe.Play();

                txtB.Text = "";
            }
        }
    }
}
