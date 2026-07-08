using System;
using System.Globalization;
using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using RuntimePaths = NinjutsuGames.FusionNetwork.Runtime.RuntimePaths;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(FusionSubModules))]
    public class FusionSubModulesDrawer : TBoxDrawer
    {
        private static readonly TextInfo TXT = CultureInfo.InvariantCulture.TextInfo;
        private const string USS_PATH = EditorPaths.COMMON + "Settings/StyleSheets/Updates";

        private const string STORE_LINK = "https://www.ninjutsugames.com/link/{0}";

        private const string EXPAND_MORE = "+";
        private const string EXPAND_LESS = "-";
        
        private static readonly IIcon ICON_INSTALLED_YES = new IconCircleSolid(ColorTheme.Type.Green);
        private static readonly IIcon ICON_INSTALLED_UPD = new IconCircleSolid(ColorTheme.Type.Yellow);
        private static readonly IIcon ICON_INSTALLED_NO = new IconCircleOutline(ColorTheme.Type.TextLight);

        private const string NAME_LOADING = "GC-Updates-Loading";

        private const string NAME_CONTAINER_ROOT = "GC-Updates-Container-Root";
        private const string NAME_CONTAINER_BODY = "GC-Updates-Container-Body";
        private const string NAME_CONTAINER_FOOT = "GC-Updates-Container-Foot";
        
        private const string NAME_ASSET_ROOT = "GC-Updates-Asset-Root";
        private const string NAME_ASSET_HEAD = "GC-Updates-Asset-Head";
        private const string NAME_ASSET_BODY = "GC-Updates-Asset-Body";
        
        // MEMBERS: -------------------------------------------------------------------------------

        private VisualElement m_Root;
        private VisualElement m_Body;
        private VisualElement m_Foot;

        // PAINT METHOD: --------------------------------------------------------------------------
        
        protected override void CreatePropertyContent(VisualElement container, SerializedProperty property)
        {
            VersionsManager.Initialize();

            m_Root = new VisualElement { name = NAME_CONTAINER_ROOT };
            m_Body = new VisualElement { name = NAME_CONTAINER_BODY };
            // m_Foot = new VisualElement { name = NAME_CONTAINER_FOOT };
            
            var styleSheets = StyleSheetUtils.Load(USS_PATH);
            foreach (var sheet in styleSheets) m_Root.styleSheets.Add(sheet);

            // RefreshFoot();
            RefreshBody();
            VersionsManager.EventChange += RefreshBody;
            
            m_Root.Add(m_Body);
            // m_Root.Add(m_Foot);
            
            container.Add(m_Root);
        }

        private void RefreshFoot()
        {
            var remindUpdates = new Toggle
            {
                value = VersionsNotifications.RemindUpdates
            };

            var remindLabel = new Label("Remind me when there is a new update");

            remindUpdates.RegisterValueChangedCallback(changeEvent =>
            {
                VersionsNotifications.RemindUpdates = changeEvent.newValue;
            });
            
            m_Foot.Add(remindUpdates);
            m_Foot.Add(remindLabel);
        }

        private void RefreshBody()
        {
            m_Body.Clear();

            switch (VersionsManager.Latest.State)
            {
                case State.Loading: RefreshLoading(); break;
                case State.Ready: RefreshReady(); break;
                case State.Error: RefreshError(); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void RefreshLoading()
        {
            var loading = new Label("Fetching information...")
            {
                name = NAME_LOADING
            };
        
            m_Body.Add(loading);
        }
        
        private void RefreshError()
        {
            var error = new ErrorMessage("Error while fetching. Please check later");
            m_Body.Add(error);
        }
        
        private void RefreshReady()
        {
            foreach (var entry in VersionsManager.LatestEntries)
            {
                RefreshAsset(entry.Key, entry.Value);
            }
        }

        private void RefreshAsset(string id, AssetEntry asset)
        {
            if(!id.Contains("fusion-")) return;

            var root = new VisualElement { name = NAME_ASSET_ROOT };
            var head = new VisualElement { name = NAME_ASSET_HEAD };
            var body = new VisualElement { name = NAME_ASSET_BODY };

            root.Add(head);
            root.Add(body);
            
            m_Body.Add(root);
            
            CreateHead(id, asset, head, body);
            CreateBody(id, asset, body);
        }

        private void CreateHead(string id, AssetEntry asset, VisualElement head, VisualElement body)
        {
            var path = RuntimePaths.PACKAGES + TXT.ToTitleCase(TXT.ToTitleCase(id));
            if(id.Contains("fusion-"))
            {
                var title = TXT.ToTitleCase(id).Replace("-", "");
                path = $"{RuntimePaths.SUB_MODULES}{title}";
            }
            var isInstalled = AssetDatabase.IsValidFolder(path);
            var installedVersion = VersionsManager.GetInstalledVersion(id);
            var isInstalledOlder = installedVersion?.IsOlderThan(asset?.Version) ?? false; 

            Texture icon = isInstalled
                ? isInstalledOlder ? ICON_INSTALLED_UPD.Texture : ICON_INSTALLED_YES.Texture
                : ICON_INSTALLED_NO.Texture;

            var btnExpand = new Button
            {
                text = EXPAND_MORE,
                style =
                {
                    width = new Length(20, LengthUnit.Pixel),
                    borderRightWidth = new StyleFloat(1)
                }
            };
            
            btnExpand.clicked += () =>
            {
                body.style.display = body.style.display == DisplayStyle.None
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

                btnExpand.text = body.style.display == DisplayStyle.None
                    ? EXPAND_MORE
                    : EXPAND_LESS;
            };

            var available = asset?.Unavailable == false;
            var btnInstall = new Button
            {
                text = isInstalled 
                    ? isInstalledOlder ? "Update" : "Installed" 
                    : available ? "Download" : "Unavailable",
                style =
                {
                    width = new Length(100, LengthUnit.Pixel),
                    borderLeftWidth = new StyleFloat(1)
                }
            };
            
            btnInstall.clicked += () =>
            {
                Application.OpenURL(string.Format(STORE_LINK, id));
            };
            
            btnInstall.SetEnabled(!isInstalled || isInstalledOlder);
            if(!available) btnInstall.SetEnabled(false);
            
            var label = asset?.Version.ToString();
            if (isInstalled && isInstalledOlder)
            {
                label = $"{installedVersion} → {label}";
            }
            if(isInstalled && asset == null || asset?.Version == null)
            {
                label = $"{installedVersion}";
            }

            head.Add(btnExpand);
            head.Add(new LabelTitle(TextUtils.Humanize(id)));
            head.Add(new Label(label));
            head.Add(new Image { image = icon });
            head.Add(btnInstall);
        }

        private void CreateBody(string id, AssetEntry asset, VisualElement body)
        {
            if (asset.Unavailable)
            {
                body.Add(new Label("Coming soon"));
                body.style.display = DisplayStyle.None;
                return;
            }
            
            body.Add(new LabelTitle($"Released on {asset.Release?.Date}"));
            
            if (asset?.Changes != null)
            {
                if (asset.Changes.New.Length > 0)
                {
                    body.Add(new SpaceSmaller());
                    body.Add(new LabelTitle("New"));
                    foreach (var log in asset.Changes.New)
                    {
                        var lbl = new Label($"- {log}");
                        lbl.style.whiteSpace = WhiteSpace.Normal;
                        body.Add(lbl);
                    }
                }
                
                if (asset.Changes.Enhanced.Length > 0)
                {
                    body.Add(new SpaceSmaller());
                    body.Add(new LabelTitle("Enhanced"));
                    foreach (var log in asset.Changes.Enhanced)
                    {
                        var lbl = new Label($"- {log}");
                        lbl.style.whiteSpace = WhiteSpace.Normal;
                        body.Add(lbl);
                    }
                }
                
                if (asset.Changes.Changed.Length > 0)
                {
                    body.Add(new SpaceSmaller());
                    body.Add(new LabelTitle("Changed"));
                    foreach (var log in asset.Changes.Changed)
                    {
                        var lbl = new Label($"- {log}");
                        lbl.style.whiteSpace = WhiteSpace.Normal;
                        body.Add(lbl);
                    }
                }
                
                if (asset.Changes.Removed.Length > 0)
                {
                    body.Add(new SpaceSmaller());
                    body.Add(new LabelTitle("Removed"));
                    foreach (var log in asset.Changes.Removed)
                    {
                        var lbl = new Label($"- {log}");
                        lbl.style.whiteSpace = WhiteSpace.Normal;
                        body.Add(lbl);
                    }
                }
                
                if (asset.Changes.Fixed.Length > 0)
                {
                    body.Add(new SpaceSmaller());
                    body.Add(new LabelTitle("Fixed"));
                    foreach (var log in asset.Changes.Fixed)
                    {
                        var lbl = new Label($"- {log}");
                        lbl.style.whiteSpace = WhiteSpace.Normal;
                        body.Add(lbl);
                    }
                }
            }

            body.style.display = DisplayStyle.None;
        }
    }
}