using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader {
    internal static class ConfigDefaults {
        #region Enabled

        public const bool Enabled = true;

        #endregion

        #region ScoresContext

        public static readonly ScoresContext ScoresContext = ScoresContext.Modifiers;

        #endregion

        #region LeaderboardTableMask

        public const ScoreRowCellType LeaderboardTableMask = ScoreRowCellType.Rank |
                                              ScoreRowCellType.Username |
                                              ScoreRowCellType.Modifiers |
                                              ScoreRowCellType.Accuracy |
                                              ScoreRowCellType.PerformancePoints |
                                              ScoreRowCellType.Score;

        #endregion

        #region ReplayerSettings

        public static readonly InternalReplayerCameraSettings InternalReplayerCameraSettings = new() {
            MaxCameraFOV = 110,
            MinCameraFOV = 70,
            CameraFOV = 90,
            FpfcCameraView = "PlayerView",
            VRCameraView = "BehindView"
        };
        
        public static readonly ReplayerSettings ReplayerSettings = new() {
            AutoHideUI = false,
            LoadPlayerEnvironment = false,
            ExitReplayAutomatically = true,

            ShowHead = false,
            ShowLeftSaber = true,
            ShowRightSaber = true,
            ShowWatermark = true,

            ShowTimelineMisses = true,
            ShowTimelineBombs = true,
            ShowTimelinePauses = true,

            CameraSettings = InternalReplayerCameraSettings,
                
            Shortcuts = new() {
                LayoutEditorPartialModeHotkey = KeyCode.H,
                HideCursorHotkey = KeyCode.C,
                PauseHotkey = KeyCode.Space,
                RewindForwardHotkey = KeyCode.RightArrow,
                RewindBackwardHotkey = KeyCode.LeftArrow,
                LayoutEditorAntiSnapHotkey = KeyCode.LeftShift,
            }
        };

        #endregion

        #region EnableReplayCaching

        public static bool EnableReplayCaching = false;

        #endregion
    }
}