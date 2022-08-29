﻿using System.Collections;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader.DataManager {
    internal class LeaderboardInfoManager : MonoBehaviour {
        #region Start

        private void Start() {
            StartCoroutine(FullCacheUpdateTask());
            LeaderboardState.AddSelectedBeatmapListener(OnSelectedBeatmapWasChanged);
        }

        private void OnDestroy() {
            LeaderboardState.RemoveSelectedBeatmapListener(OnSelectedBeatmapWasChanged);
        }

        #endregion

        #region OnSelectedBeatmapWasChanged

        private string _lastSelectedHash;

        private void OnSelectedBeatmapWasChanged(bool selectedAny, LeaderboardKey leaderboardKey, IDifficultyBeatmap beatmap) {
            if (!selectedAny || leaderboardKey.Hash.Equals(_lastSelectedHash)) return;
            _lastSelectedHash = leaderboardKey.Hash;
            UpdateLeaderboardsByHash(leaderboardKey.Hash);
        }

        private Coroutine _selectedLeaderboardUpdateCoroutine;

        private void UpdateLeaderboardsByHash(string hash) {
            if (_selectedLeaderboardUpdateCoroutine != null) StopCoroutine(_selectedLeaderboardUpdateCoroutine);

            void OnSuccess(HashLeaderboardsInfoResponse result) {
                foreach (var leaderboardInfo in result.leaderboards) {
                    LeaderboardsCache.PutLeaderboardInfo(result.song, leaderboardInfo.id, leaderboardInfo.difficulty, leaderboardInfo.qualification);
                }

                LeaderboardsCache.NotifyCacheWasChanged();
            }

            void OnFail(string reason) {
                Plugin.Log.Debug($"UpdateLeaderboardsByHash failed! {reason}");
            }

            _selectedLeaderboardUpdateCoroutine = StartCoroutine(LeaderboardsRequest.SendSingleRequest(hash, OnSuccess, OnFail));
        }

        #endregion

        #region FullCacheUpdate

        private static IEnumerator FullCacheUpdateTask() {
            yield return UpdateLeaderboards("nominated");
            yield return UpdateLeaderboards("qualified");
            yield return UpdateLeaderboards("ranked");
            LeaderboardsCache.NotifyCacheWasChanged();
        }

        private static IEnumerator UpdateLeaderboards(string type) {
            const int itemsPerPage = 500;
            var totalPages = 1;
            var page = 1;
            var failed = false;

            void OnSuccess(Paged<MassLeaderboardsInfoResponse> result) {
                foreach (var response in result.data) {
                    LeaderboardsCache.PutLeaderboardInfo(response.song, response.id, response.difficulty, response.qualification);
                }

                totalPages = Mathf.CeilToInt((float) result.metadata.total / result.metadata.itemsPerPage);
                page = result.metadata.page + 1;
            }

            void OnFail(string reason) {
                failed = true;
                Plugin.Log.Debug($"{type} {page}/{totalPages} cache update failed! {reason}");
            }

            do {
                yield return LeaderboardsRequest.SendBulkRequest(type, page, itemsPerPage, OnSuccess, OnFail);
            } while (!failed && page < totalPages);
        }

        #endregion
    }
}