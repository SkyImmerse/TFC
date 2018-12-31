using Assets.Tibia.ClassicNetwork;
using Assets.Tibia.DAO;
using Assets.Tibia.UI.Login_Interface;
using Game.DAO;
using Game.Graphics;
using GameClient;
using GameClient.Network;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using SkyImmerseEngine;
using SkyImmerseEngine.Graphics;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public class Main
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Run()
        {
            Config.Load();

            MapRenderer.InitCameras();
            FeatureManager.Init();

            ThingTypeRender.Init();

            ThingTypeManager.LoadingComplete += ThingTypeManager_LoadingComplete;

            ThingTypeManager.Init();

            LoadingCircle.Global.Visible = true;
            ThingTypeManager.OpenThingsFile(File.OpenRead(Path.Combine(Application.streamingAssetsPath, "Things", Config.ClientVersion.ToString(), "Tibia.dat")));

            AtlasSpriteManager.LoadingComplete += AtlasSpriteManager_LoadingComplete;


            using (System.IO.FileStream fs = new FileStream(Path.Combine(Application.streamingAssetsPath, "Things", Config.ClientVersion.ToString(), "Tibia.aspr"), FileMode.Open, FileAccess.Read))
            using (GZipStream decompressionStream = new GZipStream(fs, CompressionMode.Decompress, true))
            {
                AtlasSpriteManager.OpenFile(decompressionStream, "Custom/Creature", "Custom/ItemUnlit");

            }
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

        }

        private static void AtlasSpriteManager_LoadingComplete()
        {
            GC.Collect();
            LoadingCircle.Global.Visible = false;
            GameObject.FindObjectOfType<UILoginController>().Visible = true;
            GameObject.Find("StartBackground")?.SetActive(false);
            UIInterfaceVisibility.ShowAllLoginInterface();
        }

        private static void ThingTypeManager_LoadingComplete()
        {
            GC.Collect();
            Debug.Log("Things loaded");
        }
    }
}
