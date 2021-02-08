﻿
using Android.App;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Widget;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;

namespace MidiPlayer {

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity {

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Fields

        string filePath;

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Constructor

        public MainActivity() {
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // public Methods [verb]

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults) {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // protected Methods [verb]

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            initializeComponent();
        }

        protected override void OnStop() {
            base.OnStop();
            Player.Stop();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // private Methods [verb]

        async Task<bool> loadTarget() {
            try {
                FileData _fileData = await CrossFilePicker.Current.PickFile();
                if (_fileData is null) {
                    return false; // user canceled file picking
                }
                filePath = _fileData.FilePath;
                var _fileName = _fileData.FileName;
                if (!(_fileName.Contains(".MID") || _fileName.Contains(".mid"))) {
                    return false;
                }
                Log.Info($"File name chosen: {_fileName}");
                return true;
            } catch (Exception ex) {
                Log.Error($"Exception choosing file: {ex.ToString()}");
                return false;
            }
        }

        async void onOpenButton_Click(object sender, EventArgs e) {
            Log.Info("openButton clicked.");
            var _result = await loadTarget();
            this.Title = $"MidiPlayer: {filePath.Split("/").ToList().Last()}";
        }

        void onStartButton_Click(object sender, EventArgs e) {
            Log.Info("startButton clicked.");
            if (!filePath.HasValue()) {
                return;
            }
            Player.Target = filePath;
            Player.Start();
        }

        void onStopButton_Click(object sender, EventArgs e) {
            Log.Info("stopButton clicked.");
            Player.Stop();
        }

        /// <summary>
        /// コンポーネントを初期化します
        /// </summary>
        void initializeComponent() {
            Button _openButton = FindViewById<Button>(Resource.Id.openButton);
            _openButton.Click += onOpenButton_Click;

            Button _startButton = FindViewById<Button>(Resource.Id.startButton);
            _startButton.Click += onStartButton_Click;

            Button _stopButton = FindViewById<Button>(Resource.Id.stopButton);
            _stopButton.Click += onStopButton_Click;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // inner Classes

        class Player {

            ///////////////////////////////////////////////////////////////////////////////////////////
            // Fields

            static string target;

            static MediaPlayer mediaPlayer = null;

            ///////////////////////////////////////////////////////////////////////////////////////////
            // Constructor

            static Player() {
            }

            ///////////////////////////////////////////////////////////////////////////////////////////
            // Properties [noun, adjective] 

            public static string Target {
                set => target = value;
            }

            ///////////////////////////////////////////////////////////////////////////////////////////
            // public Methods [verb]

            /// <summary>
            /// MIDIファイル再生
            /// </summary>
            public static void Start() {
                mediaPlayer = new MediaPlayer();
                mediaPlayer.SetDataSource(target);
                mediaPlayer.Prepare();
                mediaPlayer.Looping = true;
                mediaPlayer.Start();
                Log.Info("start.");
            }

            public static void Stop() {
                if (mediaPlayer is null) {
                    return;
                }
                mediaPlayer.Stop();
                mediaPlayer.Release();
                mediaPlayer = null;
                Log.Info("stop.");
            }
        }
    }

    /// <summary>
    /// 共通拡張メソッド
    /// </summary>
    public static class Extensions {
        /// <summary>
        /// 文字列が null または 空文字("")ではない場合 TRUE を返します
        /// </summary>
        public static bool HasValue(this string source) {
            return !(source is null || source.Equals(""));
        }
    }
}