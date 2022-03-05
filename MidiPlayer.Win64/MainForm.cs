﻿
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MidiPlayer.Win64 {
    /// <summary>
    /// main form for the application.
    /// </summary>
    public partial class MainForm : Form {
#nullable enable

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Fields [nouns, noun phrases]

        string _soundFontPath = "undefined";

        string _midiFilePath = "undefined";

        PlayList _playList;

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Constructor

        public MainForm() {
            InitializeComponent();
            DoubleBuffered = true;
            _playList = new();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // EventHandler

        /// <summary>
        /// MainForm load.
        /// </summary>
        void MainForm_Load(object sender, EventArgs e) {
            Conf.Load();
            initializeControl();

            // load previous setting.
            if (Env.ExistsSoundFont && Env.ExistsMidiFile) {
                Synth.SoundFontPath = Env.SoundFontPath;
                Synth.MidiFilePath = Env.MidiFilePath;
                _soundFontPath = Env.SoundFontPath;
                _midiFilePath = Env.MidiFilePath;
            }

            /// <summary>
            /// add a callback function to be called when the synth is playback.
            /// </summary>
            Synth.Playbacking += (IntPtr data, IntPtr evt) => {
                return Synth.HandleEvent(data, evt);
            };

            /// <summary>
            /// add a callback function to be called when the synth started.
            /// </summary>
            Synth.Started += () => {
                Log.Info("Started called.");
                Invoke((MethodInvoker) (() => {
                    Text = $"MidiPlayer: {Synth.MidiFilePath.ToFileName()} {Synth.SoundFontPath.ToFileName()}";
                    _listView.Items.Clear();
                    Enumerable.Range(0, Synth.TrackCount).ToList().ForEach(x => {
                        _listView.Items.Add(new ListViewItem(new string[] { "  ●", "--", "--", "--", "--", "--" }));
                    });
                }));
            };

            /// <summary>
            /// add a callback function to be called when the synth ended.
            /// </summary>
            Synth.Ended += () => {
                Log.Info("Ended called.");
                if (!_playList.Ready) {
                    Synth.Stop();
                    Synth.Start();
                } else {
                    Synth.Stop();
                    Synth.MidiFilePath = _playList.Next;
                    Synth.Start();
                }
            };

            /// <summary>
            /// add a callback function to be called when the synth updated.
            /// </summary>
            Synth.Updated += (object sender, PropertyChangedEventArgs e) => {
                var track = (Synth.Track) sender;
                Invoke(updateList(track));
            };
        }

        void buttonLoadSoundFont_Click(object sender, EventArgs e) {
            Log.Info("buttonLoadSoundFont clicked.");
            try {
                if (Synth.Playing) {
                    stopSong();
                }
                _openFileDialog.InitialDirectory = Env.SoundFontDir;
                var dialog = _openFileDialog.ShowDialog();
                if (dialog == DialogResult.OK) {
                    _soundFontPath = Path.GetFullPath(_openFileDialog.FileName);
                    Synth.SoundFontPath = _soundFontPath;
                    Env.SoundFontPath = _soundFontPath;
                }
            } catch (Exception ex) {
                Log.Error(ex.Message);
            }
        }

        void buttonLoadMidiFile_Click(object sender, EventArgs e) {
            Log.Info("buttonLoadMidiFile clicked.");
            try {
                if (Synth.Playing) {
                    stopSong();
                }
                _openFileDialog.InitialDirectory = Env.MidiFileDir;
                var dialog = _openFileDialog.ShowDialog();
                if (dialog == DialogResult.OK) {
                    _midiFilePath = Path.GetFullPath(_openFileDialog.FileName);
                    Synth.MidiFilePath = _midiFilePath;
                    Env.MidiFilePath = _midiFilePath;
                }
            } catch (Exception ex) {
                Log.Error(ex.Message);
            }
        }

        void buttonStart_Click(object sender, EventArgs e) {
            Log.Info("buttonStart clicked.");
            try {
                if (!_midiFilePath.HasValue()) {
                    return;
                }
                playSong();
            } catch (Exception ex) {
                Log.Error(ex.Message);
            }
        }

        void buttonStop_Click(object sender, EventArgs e) {
            Log.Info("buttonStop clicked.");
            try {
                stopSong();
                Conf.Save();
            } catch (Exception ex) {
                Log.Error(ex.Message);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // private Methods [verb, verb phrases]

        /// <summary>
        /// play the song.
        /// </summary>
        async void playSong() {
            try {
                await Task.Run(() => {
                    if (!_playList.Ready) {
                        Synth.MidiFilePath = _midiFilePath;
                        Synth.Start();
                    } else {
                        Synth.MidiFilePath = _playList.Next;
                        Synth.Start();
                    }
                });
            } catch (Exception ex) {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// stop the song.
        /// </summary>
        async void stopSong() {
            try {
                await Task.Run(() => Synth.Stop());
                if (_listView.Items.Count != 0) {
                    Invoke((MethodInvoker) (() => {
                        Enumerable.Range(0, Synth.TrackCount).ToList().ForEach(x => {
                            _listView.Items[x].SubItems[0].ForeColor = Color.Black;
                        });
                    }));
                }
                Conf.Save(); // TODO: save
            } catch (Exception ex) {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// a callback function to be called when the synth updated.
        /// </summary>
        MethodInvoker updateList(Synth.Track track) {
            const int _column1Idx = 0;
            var trackIdx = track.Index - 1; // exclude conductor track;
            return () => {
                var listViewItem = new ListViewItem(new string[] {
                    "  ●",
                    track.Name,
                    Synth.GetVoice(track.Index),
                    track.Channel.ToString(),
                    track.Bank.ToString(),
                    track.Program.ToString()
                });
                _listView.BeginUpdate();
                _listView.Items[trackIdx] = listViewItem;
                _listView.Items[trackIdx].UseItemStyleForSubItems = false;
                if (track.Sounds) {
                    _listView.Items[trackIdx].SubItems[_column1Idx].ForeColor = Color.Lime;
                } else {
                    _listView.Items[trackIdx].SubItems[_column1Idx].ForeColor = Color.Black;
                }
                _listView.EndUpdate();
            };
        }

        /// <summary>
        /// initialize UI control.
        /// </summary>
        void initializeControl() {
            // initialize ListView
            _listView.FullRowSelect = true;
            _listView.GridLines = true;
            _listView.Sorting = SortOrder.None; // do not sort automatically.
            _listView.View = View.Details;
            ColumnHeader column1 = new();
            column1.Text = "On";
            column1.Width = 35;
            ColumnHeader column2 = new();
            column2.Text = "Name";
            column2.Width = 115;
            ColumnHeader column3 = new();
            column3.Text = "Voice";
            column3.Width = 115;
            ColumnHeader column4 = new();
            column4.Text = "Chan";
            column4.Width = 45;
            ColumnHeader column5 = new();
            column5.Text = "Bank";
            column5.Width = 45;
            ColumnHeader column6 = new();
            column6.Text = "Prog";
            column6.Width = 45;
            ColumnHeader[] columnHeaderArray = { column1, column2, column3, column4, column5, column6 };
            _listView.Columns.AddRange(columnHeaderArray);
        }
    }
}
