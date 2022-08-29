﻿using System;
using System.Linq;
using System.Reflection;
using BeatLeader.Utils;
using BeatLeader.Replayer;
using BeatLeader.Replayer.Camera;
using BeatLeader.Replayer.Emulation;
using BeatLeader.Replayer.Movement;
using BeatLeader.Replayer.Scoring;
using BeatLeader.ViewControllers;
using BeatLeader.Models;
using SiraUtil.Tools.FPFC;
using UnityEngine;
using Zenject;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;

namespace BeatLeader.Replayer
{
    public class ReplayerManualInstaller
    {
        public static void Install(ReplayLaunchData data, DiContainer Container)
        {
            new ReplayerManualInstaller().InstallBindings(data, Container);
        }

        public void InstallBindings(ReplayLaunchData data, DiContainer Container)
        {
            Container.Bind<ReplayLaunchData>().FromInstance(data).AsSingle();
            Container.Bind<LocksController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<IScoringInterlayer>().To<ReplayToBaseScoringInterlayer>().AsSingle().NonLazy();
            Container.BindMemoryPool<SimpleNoteCutComparator, SimpleNoteCutComparator.Pool>().WithInitialSize(30)
               .FromComponentInNewPrefab(new GameObject("ComparatorPrefab").AddComponent<SimpleNoteCutComparator>());
            Container.Bind<SimpleNoteComparatorsSpawner>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<BeatmapTimeController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            //scorecontroller patch
            Container.Bind<IReplayerScoreController>().To<ReplayerScoreController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Rebind<IScoreController>().To<IReplayerScoreController>().FromResolve().AsSingle().NonLazy();
            Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault(x => x.name == "GameplayData").TryDestroy();

            Container.Bind<InputManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<BeatmapVisualsController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<VRControllersManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<VRControllersMovementEmulator>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<PlaybackController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.Bind<SceneTweaksManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<ReplayerCameraController.InitData>().FromInstance(new ReplayerCameraController.InitData(

                new StaticCameraPose(0, "LeftView", new Vector3(-3.70f, 2.30f, -1.10f), Quaternion.Euler(new Vector3(0, 60, 0))),
                new StaticCameraPose(1, "RightView", new Vector3(3.70f, 2.30f, -1.10f), Quaternion.Euler(new Vector3(0, -60, 0))),
                new StaticCameraPose(2, "BehindView", new Vector3(0f, 1.9f, -2f), Quaternion.Euler(Vector3.zero), InputManager.InputType.FPFC),
                new StaticCameraPose(2, "BehindView", new Vector3(0, 0, -2), Quaternion.Euler(Vector3.zero), InputManager.InputType.VR),
                new StaticCameraPose(3, "CenterView", new Vector3(0f, 1.7f, 0f), Quaternion.Euler(Vector3.zero), InputManager.InputType.FPFC),
                new StaticCameraPose(3, "CenterView", Vector3.zero, Quaternion.Euler(Vector3.zero), InputManager.InputType.VR),

                new FlyingCameraPose(new Vector2(0.5f, 0.5f), new Vector2(0, 1.7f), 4, true, "FreeView"),
                new PlayerViewCameraPose(3)

                )).AsSingle();
            Container.Bind<ReplayerCameraController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesTo<SettingsLoader>().AsSingle().Lazy();

            InstallUI(Container, !InputManager.IsInFPFC);
            Plugin.Log.Notice("[Installer] Replay system successfully installed");
        }
        private static void InstallUI(DiContainer container, bool vr)
        {
            if (vr)
                container.Bind<ReplayerVRViewController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            else
            {
                container.BindInterfacesAndSelfTo<UI2DManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                container.Bind<ReplayerPCViewController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                PatchSiraFreeView(container);
            }
        }
        private static void PatchSiraFreeView(DiContainer container)
        {
            try
            {
                container.Resolve<IFPFCSettings>().Enabled = false;
                Assembly assembly = typeof(IFPFCSettings).Assembly;

                Type smoothCameraListenerType = assembly.GetType("SiraUtil.Tools.FPFC.SmoothCameraListener");
                Type FPFCToggleType = assembly.GetType("SiraUtil.Tools.FPFC.FPFCToggle");
                Type simpleCameraControllerType = assembly.GetType("SiraUtil.Tools.FPFC.SimpleCameraController");

                container.Unbind<IFPFCSettings>();
                container.UnbindInterfacesTo(smoothCameraListenerType);
                container.UnbindInterfacesTo(FPFCToggleType);
                //GameObject.Destroy((UnityEngine.Object)Container.TryResolve(simpleCameraControllerType));
            }
            catch (Exception ex)
            {
                Plugin.Log.Critical($"An unhandled exception occurred during attemping to remove Sira's FPFC system! {ex}");
            }
        }
    }
}
