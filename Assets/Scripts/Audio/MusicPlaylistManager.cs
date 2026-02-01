using UnityEngine;
using System.Collections.Generic;
using EscapeTrainRun.Core;
using EscapeTrainRun.Environment;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Audio
{
    /// <summary>
    /// Manages music playlists and track transitions.
    /// Supports theme-based playlists and dynamic music.
    /// </summary>
    public class MusicPlaylistManager : MonoBehaviour
    {
        [Header("Playlists")]
        [SerializeField] private MusicPlaylist menuPlaylist;
        [SerializeField] private MusicPlaylist trainPlaylist;
        [SerializeField] private MusicPlaylist busPlaylist;
        [SerializeField] private MusicPlaylist groundPlaylist;

        [Header("Settings")]
        [SerializeField] private bool shufflePlaylist = true;
        [SerializeField] private float trackTransitionTime = 1f;
        [SerializeField] private float intensityTransitionTime = 2f;

        // State
        private MusicPlaylist currentPlaylist;
        private int currentTrackIndex;
        private List<int> shuffledIndices = new List<int>();
        private int shuffleIndex;
        private float trackTimer;
        private bool isPlaying;
        private MusicIntensity currentIntensity = MusicIntensity.Normal;

        private void OnEnable()
        {
            Events.GameEvents.OnThemeChanged += OnThemeChanged;
            Events.GameEvents.OnGameStarted += OnGameStarted;
            Events.GameEvents.OnGameOver += OnGameOver;
        }

        private void OnDisable()
        {
            Events.GameEvents.OnThemeChanged -= OnThemeChanged;
            Events.GameEvents.OnGameStarted -= OnGameStarted;
            Events.GameEvents.OnGameOver -= OnGameOver;
        }

        private void Update()
        {
            if (isPlaying && currentPlaylist != null && currentPlaylist.Tracks.Length > 1)
            {
                UpdateTrackTimer();
            }
        }

        private void OnThemeChanged(ThemeType theme)
        {
            var playlist = GetPlaylistForTheme(theme);
            if (playlist != null && playlist != currentPlaylist)
            {
                SwitchPlaylist(playlist);
            }
        }

        private void OnGameStarted()
        {
            isPlaying = true;
        }

        private void OnGameOver(GameOverData data)
        {
            isPlaying = false;
        }

        /// <summary>
        /// Plays the menu music playlist.
        /// </summary>
        public void PlayMenuMusic()
        {
            SwitchPlaylist(menuPlaylist);
        }

        /// <summary>
        /// Switches to a new playlist.
        /// </summary>
        public void SwitchPlaylist(MusicPlaylist playlist)
        {
            if (playlist == null || playlist.Tracks == null || playlist.Tracks.Length == 0)
            {
                return;
            }

            currentPlaylist = playlist;
            currentTrackIndex = 0;

            if (shufflePlaylist)
            {
                ShufflePlaylist();
            }

            PlayCurrentTrack();
        }

        private MusicPlaylist GetPlaylistForTheme(ThemeType theme)
        {
            return theme switch
            {
                ThemeType.Train => trainPlaylist,
                ThemeType.Bus => busPlaylist,
                ThemeType.Ground => groundPlaylist,
                _ => trainPlaylist
            };
        }

        private void ShufflePlaylist()
        {
            if (currentPlaylist == null) return;

            shuffledIndices.Clear();
            for (int i = 0; i < currentPlaylist.Tracks.Length; i++)
            {
                shuffledIndices.Add(i);
            }

            // Fisher-Yates shuffle
            for (int i = shuffledIndices.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffledIndices[i], shuffledIndices[j]) = (shuffledIndices[j], shuffledIndices[i]);
            }

            shuffleIndex = 0;
        }

        private void PlayCurrentTrack()
        {
            if (currentPlaylist == null || currentPlaylist.Tracks.Length == 0) return;

            int index = shufflePlaylist ? shuffledIndices[shuffleIndex] : currentTrackIndex;
            AudioClip track = currentPlaylist.Tracks[index];

            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.CrossfadeMusic(track, trackTransitionTime);
            }

            trackTimer = track.length;
        }

        private void UpdateTrackTimer()
        {
            trackTimer -= Time.deltaTime;

            if (trackTimer <= 0f)
            {
                PlayNextTrack();
            }
        }

        /// <summary>
        /// Plays the next track in the playlist.
        /// </summary>
        public void PlayNextTrack()
        {
            if (currentPlaylist == null) return;

            if (shufflePlaylist)
            {
                shuffleIndex = (shuffleIndex + 1) % shuffledIndices.Count;
                if (shuffleIndex == 0)
                {
                    ShufflePlaylist(); // Reshuffle when we've played all tracks
                }
            }
            else
            {
                currentTrackIndex = (currentTrackIndex + 1) % currentPlaylist.Tracks.Length;
            }

            PlayCurrentTrack();
        }

        /// <summary>
        /// Sets the music intensity (for dynamic music).
        /// </summary>
        public void SetIntensity(MusicIntensity intensity)
        {
            if (intensity == currentIntensity) return;
            currentIntensity = intensity;

            // Could switch to different track layers or adjust tempo
            AdjustMusicForIntensity(intensity);
        }

        private void AdjustMusicForIntensity(MusicIntensity intensity)
        {
            // This could be expanded to support layered music
            float pitch = intensity switch
            {
                MusicIntensity.Low => 0.95f,
                MusicIntensity.Normal => 1f,
                MusicIntensity.High => 1.05f,
                MusicIntensity.Intense => 1.1f,
                _ => 1f
            };

            // Apply pitch change (requires access to music source)
            Debug.Log($"[MusicPlaylistManager] Intensity changed to {intensity}");
        }

        /// <summary>
        /// Skips to a specific track.
        /// </summary>
        public void SkipToTrack(int index)
        {
            if (currentPlaylist == null || index < 0 || index >= currentPlaylist.Tracks.Length)
                return;

            currentTrackIndex = index;
            shuffleIndex = 0;
            PlayCurrentTrack();
        }
    }

    /// <summary>
    /// A collection of music tracks for a theme or mood.
    /// </summary>
    [System.Serializable]
    public class MusicPlaylist
    {
        public string Name;
        public AudioClip[] Tracks;
        public bool AllowShuffle = true;
    }

    /// <summary>
    /// Music intensity levels for dynamic music.
    /// </summary>
    public enum MusicIntensity
    {
        Low,
        Normal,
        High,
        Intense
    }
}
