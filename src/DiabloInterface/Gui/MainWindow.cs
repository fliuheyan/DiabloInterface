﻿namespace Zutatensuppe.DiabloInterface.Gui
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;

    using Zutatensuppe.DiabloInterface.Business.Services;
    using Zutatensuppe.DiabloInterface.Business.Settings;
    using Zutatensuppe.DiabloInterface.Core.Logging;
    using Zutatensuppe.DiabloInterface.Gui.Controls;
    using Zutatensuppe.DiabloInterface.Gui.Forms;

    public partial class MainWindow : WsExCompositedForm
    {
        static readonly ILogger Logger = LogServiceLocator.Get(MethodBase.GetCurrentMethod().DeclaringType);

        readonly ISettingsService settingsService;
        readonly IGameService gameService;
        readonly IAutoSplitService autoSplitService;

        Form debugWindow;
        AbstractLayout currentLayout;

        public MainWindow(ISettingsService settingsService, IGameService gameService, IAutoSplitService autoSplitService)
        {
            Logger.Info("Creating main window.");

            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (gameService == null) throw new ArgumentNullException(nameof(gameService));
            if (autoSplitService == null) throw new ArgumentNullException(nameof(autoSplitService));

            this.settingsService = settingsService;
            this.gameService = gameService;
            this.autoSplitService = autoSplitService;

            RegisterServiceEventHandlers();
            InitializeComponent();
            UpdateLayoutView(settingsService.CurrentSettings);
            PopulateSetingsFileListContextMenu(settingsService.SettingsFileCollection);
        }

        void RegisterServiceEventHandlers()
        {
            settingsService.SettingsChanged += SettingsServiceOnSettingsChanged;
            settingsService.SettingsCollectionChanged += SettingsServiceOnSettingsCollectionChanged;
        }

        void SettingsServiceOnSettingsChanged(object sender, ApplicationSettingsEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke((Action)(() => SettingsServiceOnSettingsChanged(sender, e)));
                return;
            }

            ApplySettings(e.Settings);
        }

        void SettingsServiceOnSettingsCollectionChanged(object sender, SettingsCollectionEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke((Action)(() => SettingsServiceOnSettingsCollectionChanged(sender, e)));
                return;
            }

            PopulateSetingsFileListContextMenu(e.Collection);
        }

        void ApplyLayoutChanges(ApplicationSettings settings)
        {
            var isHorizontal = currentLayout is HorizontalLayout;
            if (isHorizontal != settings.DisplayLayoutHorizontal)
            {
                UpdateLayoutView(settings);
            }
        }

        void UpdateLayoutView(ApplicationSettings settings)
        {
            var nextLayout = CreateLayout(settings.DisplayLayoutHorizontal);
            if (currentLayout != null)
            {
                Controls.Remove(currentLayout);
                currentLayout.Dispose();
                currentLayout = null;
            }

            Controls.Add(nextLayout);
            currentLayout = nextLayout;
        }

        AbstractLayout CreateLayout(bool horizontal)
        {
            return horizontal
                ? new HorizontalLayout(settingsService, gameService) as AbstractLayout
                : new VerticalLayout(settingsService, gameService);
        }

        void MainWindowLoad(object sender, EventArgs e)
        {
            SetTitleWithApplicationVersion();
            CheckForUpdates();
            ApplySettings(settingsService.CurrentSettings);
        }

        void SetTitleWithApplicationVersion()
        {
            Text = $@"Diablo Interface v{Application.ProductVersion}";
            Update();
        }

        void CheckForUpdates()
        {
            if (settingsService.CurrentSettings.CheckUpdates)
            {
                VersionChecker.CheckForUpdate(false);
            }
        }

        void ApplySettings(ApplicationSettings settings)
        {
            ApplyLayoutChanges(settings);
        }

        void PopulateSetingsFileListContextMenu(IEnumerable<FileInfo> settingsFileCollection)
        {
            loadConfigMenuItem.DropDownItems.Clear();
            IEnumerable<ToolStripItem> items = settingsFileCollection.Select(CreateSettingsToolStripMenuItem);
            loadConfigMenuItem.DropDownItems.AddRange(items.ToArray());
        }

        ToolStripMenuItem CreateSettingsToolStripMenuItem(FileInfo fileInfo)
        {
            var item = new ToolStripMenuItem
            {
                Text = Path.GetFileNameWithoutExtension(fileInfo.Name),
                Tag = fileInfo.FullName
            };

            item.Click += LoadConfigFile;

            return item;
        }

        void LoadConfigFile(object sender, EventArgs e)
        {
            var fileName = ((ToolStripMenuItem)sender).Tag.ToString();

            // TODO: LoadSettings should throw a custom Exception with information about why this happened.
            if (!settingsService.LoadSettings(fileName))
            {
                Logger.Error($"Failed to load settings from file: {fileName}.");
                MessageBox.Show(
                    $@"Failed to load settings.{Environment.NewLine}See the error log for more details.",
                    @"Settings Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        void ExitMenuItemOnClick(object sender, EventArgs e)
        {
            Close();
        }

        void ResetMenuItemOnClick(object sender, EventArgs e)
        {
            autoSplitService.ResetAutoSplits();
            currentLayout?.Reset();
        }

        void SettingsMenuItemOnClick(object sender, EventArgs e)
        {
            using (var settingsWindow = new SettingsWindow(settingsService))
            {
                settingsWindow.ShowDialog();
            }
        }

        void DebugMenuItemOnClick(object sender, EventArgs e)
        {
            if (debugWindow == null || debugWindow.IsDisposed)
            {
                debugWindow = new DebugWindow(settingsService, gameService);
            }

            debugWindow.Show();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnregisterServiceEventHandlers();
            base.OnFormClosing(e);
        }

        void UnregisterServiceEventHandlers()
        {
            settingsService.SettingsChanged -= SettingsServiceOnSettingsChanged;
            settingsService.SettingsCollectionChanged -= SettingsServiceOnSettingsCollectionChanged;
        }
    }
}
