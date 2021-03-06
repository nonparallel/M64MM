﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using M64MM2.Properties;
using static M64MM.Utils.Core;
using M64MM.Utils;
using M64MM.Additions;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;

namespace M64MM2
{
    public partial class MainForm : Form
    {
        AppearanceForm appearanceForm;
        ExtraControlsForm extraControlsForm;
        bool cameraFrozen = false;
        bool cameraSoftFrozen = false;
        public Animation selectedAnimOld => cbAnimOld.SelectedIndex >= 0 ? animList[cbAnimOld.SelectedIndex] : new Animation();
        public Animation selectedAnimNew => cbAnimNew.SelectedIndex >= 0 ? animList[cbAnimNew.SelectedIndex] : new Animation();

        //This handles the "Each ingame frame"
        //ASYNCHRONOUS FOR THE WIN
        //FUNNILY ENOUGH! This takes little to no CPU, actually
        //It's goddamn amazing
        Task updateFunction = Task.Run(() => performUpdate());

        public MainForm()
        {
            /* Code for plugin sandboxing */

            PermissionSet trustedLoadFromRemoteSourcesGrantSet = new PermissionSet(PermissionState.Unrestricted);
            AppDomainSetup trustedLoadFromRemoteSourcesSetup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase
            };

            AppDomain trustedRemoteLoadDomain = AppDomain.CreateDomain("Trusted LoadFromRemoteSources Domain",
                           null,
                           trustedLoadFromRemoteSourcesSetup,
                           trustedLoadFromRemoteSourcesGrantSet);
            AddonErrorsBuilder = new System.Text.StringBuilder();
            InitializeComponent();
            ToolStripMenuItem addons = new ToolStripMenuItem("Addons");
            try
            { // Loading DLLs from ./Plugins
                DirectoryInfo d = new DirectoryInfo(Application.StartupPath + "\\Addons");
                foreach (FileInfo file in d.GetFiles("*.dll"))
                { // For each DLL
                    try // If getting all types fails for some reason (Ex: cannot load required assembly)...
                    {
                        Assembly assmb = Assembly.LoadFile(file.FullName);
                        Type[] classes = assmb.GetTypes();
                        foreach (Type typ in classes)
                        {
                            if (typ.GetInterface("IModule") != null)
                            { // If type implements interface IModule
                                IModule mod = (IModule)assmb.CreateInstance(typ.FullName); // Instance the IModule
                                Addon neoAddon = new Addon(mod, mod.SafeName, FileVersionInfo.GetVersionInfo(file.FullName).FileVersion.ToString(), mod.Description); // Instance Addon
                                List<ToolCommand> tc_list = mod.GetCommands(); // Get list of custom commands
                                if (tc_list != null)
                                { // Add them to the Plugins toolstrip
                                    foreach (ToolCommand tc in tc_list)
                                    {
                                        ToolStripMenuItem mod_ = new ToolStripMenuItem(tc.name);
                                        mod_.Click += (a, b) => tc.Summon(a, b);
                                        addons.DropDownItems.Add(mod_);
                                    }
                                }
                                moduleList.Add(neoAddon); // Add addon to the plugins list
                            }
                        }
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        AddonErrorsBuilder.AppendFormat("{0} [LOAD ERROR] - Error while loading addon from DLL {1}. Exception: {2}\nAre all the dependencies met?\n--------\n", DateTime.Now.ToLongTimeString(), ex.Types[0].Module.ToString(), ex.Message);
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(Application.StartupPath + "\\Addons");
                MessageBox.Show("No addons folder was present, addons folder created.\nMake sure you're running M64MM from an extracted folder.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (AddonErrorsBuilder.Length > 0)
            {
                // If there were any errors (String, may make a collection of objects?)
                addons.DropDownItems.Add(new ToolStripSeparator());
                addons.DropDownItems.Add(new ToolStripMenuItem("Addon warnings", null, (a, b) => { new AddonErrors().ShowDialog(); }));
            }

            InitializeModules();
            programTimer.Tick += (a, b) => Update(this, null);
            menuStrip.Items.Add(addons);

            Text = Resources.programName + " " + Application.ProductVersion;
            programTimer.Interval = 1000;
            programTimer.Start();
            animList = new List<Animation>();
            camStyles = new List<CameraStyle>();
            defaultAnimation.Value = "0";
            lblCameraStatus.Text = Resources.cameraStateDefault;
            toolsMenuItem.Enabled = false;

            //Load animation data
            try
            {
                using (StreamReader sr = new StreamReader("animation_data.txt"))
                {
                    while (!sr.EndOfStream && sr.Peek() != 0)
                    {
                        string rawLine = sr.ReadLine();
                        string[] splitLine = rawLine.Trim().Split('|');

                        Animation anim = new Animation
                        {
                            Value = splitLine[0],
                            Description = splitLine[1],
                            RealIndex = int.Parse(splitLine[2])
                        };

                        animList.Add(anim);
                        try
                        {
                            if (splitLine[3] != null)
                            {
                                defaultAnimation = anim;
                            }
                        }
                        catch (Exception)
                        {

                        }

                        cbAnimOld.Items.Add(splitLine[1]);
                        cbAnimNew.Items.Add(splitLine[1]);

                        cbAnimOld.SelectedIndex = 0;
                        cbAnimNew.SelectedIndex = 0;
                    }

                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                cbAnimOld.Text = cbAnimNew.Text = Resources.animDataNotLoaded;
                cbAnimOld.Enabled = false;
                cbAnimNew.Enabled = false;
                btnAnimSwap.Enabled = false;
                btnAnimReset.Enabled = false;
                btnAnimResetAll.Enabled = false;
                btnAnimReset.Enabled = false;
                chbAutoApply.Enabled = false;
            }

            //Load camera style data
            try
            {
                using (StreamReader sr = new StreamReader("camera_data.txt"))
                {
                    while (sr.Peek() >= 0)
                    {
                        string rawLine = sr.ReadLine().Trim();
                        string[] splitLine = rawLine.Split('|');
                        CameraStyle style = new CameraStyle
                        {
                            Value = byte.Parse(splitLine[0], NumberStyles.HexNumber),
                            Name = splitLine[1]
                        };
                        camStyles.Add(style);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                cbCamStyles.Text = Resources.cameraDataNotLoaded;
                cbCamStyles.Enabled = false;
                btnChangeCamStyle.Enabled = false;
            }

            if (camStyles.Count > 0)
            {
                foreach (CameraStyle style in camStyles)
                {
                    cbCamStyles.Items.Add(style.Name);
                }
                cbCamStyles.SelectedIndex = 0;
                cbCamStyles.Refresh();
            }
        }

        void InitializeModules()
        {
            foreach (Addon mod in moduleList)
            {
                mod.Module.Initialize();
            }
        }

        void Update(object sender, EventArgs e)
        {
            //Early validity checks
            if (!IsEmuOpen)
            {
                Text = Resources.programName + " " + Application.ProductVersion;
                lblProgramStatus.Text = Resources.programStatus1;
                FindEmuProcess();
                if (BaseAddress > 0)
                {
                    Task.Run(() => performBaseAddrUpd());
                    BaseAddress = 0;
                }
                modelStatus = ModelStatus.NONE;
                return;
            }

            FindBaseAddress();
            //Finding base address
            if (BaseAddress <= 0)
            {
                Text = Resources.programName + " " + Application.ProductVersion;
                lblProgramStatus.Text = Resources.programStatus2;
                modelStatus = ModelStatus.NONE;
                return;
            }

            //Reading level address (It's meant to be 0x32DDF8 but ENDIANESS:TM:)
            if (CurrentLevelID < 3)
            {
                toolsMenuItem.Enabled = false;
                lblProgramStatus.Text = Resources.programStatusAwaitingLevel + "0x" + BaseAddress.ToString("X8");
                modelStatus = ModelStatus.NONE;
                return;
            }

            //Are we running a moddded model ROM? (Working with Vanilla-styled vs. COMET / [redacted])
            modelStatus = ValidateModel();
            toolsMenuItem.Enabled = true;
            Text = Resources.programName + " " + Application.ProductVersion + " - " + modelStatus.ToString() + " ROM.";

            lblProgramStatus.Text = Resources.programStatus3 + "0x" + BaseAddress.ToString("X8");



            //==============================
            //Main program logic starts here
            //------------------------------

            //Don't overwrite the camera state if we're in non-bugged first-person
            byte[] cameraState = SwapEndian(ReadBytes(BaseAddress + 0x33C848, 4), 4);
            lblCameraCode.Text = "0x" + BitConverter.ToString(cameraState).Replace("-", "");

            if (cameraFrozen && (cameraState[0] == 0xA2 || cameraState[0] < 0x80))
            {
                byte[] data = { 0x80 };
                WriteBytes(BaseAddress + 0x33C84B, data);
            }


            //Handle hotkey input
            if (GetKey(Keys.LControlKey) || GetKey(Keys.RControlKey))
            {
                if (GetKey(Keys.D1))
                    FreezeCam(null, null);

                if (GetKey(Keys.D2))
                    UnfreezeCam(null, null);

                if (GetKey(Keys.D4))
                    SoftFreezeCam(null, null);

                if (GetKey(Keys.D5))
                    SoftUnfreezeCam(null, null);
            }
        }


        void FreezeCam(object sender, EventArgs e)
        {
            if (!IsEmuOpen || BaseAddress == 0) return;

            cameraFrozen = true;
            byte[] data = { 0x80 };
            WriteBytes(BaseAddress + 0x33C84B, data);
            lblCameraStatus.Text = Resources.cameraStateFrozen;
        }

        void UnfreezeCam(object sender, EventArgs e)
        {
            if (!IsEmuOpen || BaseAddress == 0) return;

            cameraFrozen = false;
            byte[] data = { 0x00 };
            WriteBytes(BaseAddress + 0x33C84B, data);

            lblCameraStatus.Text = cameraSoftFrozen ? Resources.cameraStateSoftFrozen : Resources.cameraStateDefault;
        }

        void SoftFreezeCam(object sender, EventArgs e)
        {
            if (!IsEmuOpen || BaseAddress == 0) return;

            cameraSoftFrozen = true;
            WriteBytes(BaseAddress + 0x33B204, BitConverter.GetBytes(0x8001C520));

            lblCameraStatus.Text = cameraFrozen ? Resources.cameraStateFrozen : Resources.cameraStateSoftFrozen;
        }

        void SoftUnfreezeCam(object sender, EventArgs e)
        {
            if (!IsEmuOpen || BaseAddress == 0) return;

            cameraSoftFrozen = false;
            WriteBytes(BaseAddress + 0x33B204, BitConverter.GetBytes(0x8033C520));

            lblCameraStatus.Text = cameraFrozen ? Resources.cameraStateFrozen : Resources.cameraStateDefault;
        }

        void changeCameraStyle(object sender, EventArgs e)
        {
            if (!IsEmuOpen || BaseAddress == 0) return;

            byte[] data = { camStyles[cbCamStyles.SelectedIndex].Value };

            WriteBytes(BaseAddress + 0x33C6D6, data);
            WriteBytes(BaseAddress + 0x33C6D7, data);
        }


        void WriteAnimSwap(object sender, EventArgs e)
        {
            if (!IsEmuOpen || BaseAddress == 0) return;

            if (selectedAnimOld.Value == "" || selectedAnimNew.Value == "")
            {
                MessageBox.Show(this, String.Format(Resources.invalidAnimSelected, ((Control)sender).Name));
                return;
            }

            byte[] stuffToWrite = SwapEndian(StringToByteArray(selectedAnimNew.Value), 4);
            long address = BaseAddress + 0x64040 + (selectedAnimOld.RealIndex + 1) * 8;

            WriteBytes(address, stuffToWrite);
        }

        void WriteAnimReset(object sender, EventArgs e)
        {
            if (!IsEmuOpen || BaseAddress == 0) return;

            if (selectedAnimOld.Value == "" || selectedAnimNew.Value == "")
            {
                MessageBox.Show(this, String.Format(Resources.invalidAnimSelected, ((Control)sender).Name));
                return;
            }

            byte[] stuffToWrite = SwapEndian(StringToByteArray(selectedAnimOld.Value), 4);
            long address = BaseAddress + 0x64040 + (selectedAnimOld.RealIndex + 1) * 8;

            WriteBytes(address, stuffToWrite);
            cbAnimNew.SelectedIndex = cbAnimOld.SelectedIndex;
        }

        void WriteAnimResetAll(object sender, EventArgs e)
        {
            if (!IsEmuOpen || BaseAddress == 0) return;

            foreach (Animation anim in animList)
            {
                byte[] stuffToWrite = SwapEndian(StringToByteArray(anim.Value), 4);
                long address = BaseAddress + 0x64040 + (anim.RealIndex + 1) * 8;

                WriteBytes(address, stuffToWrite);
            }

            cbAnimNew.SelectedIndex = cbAnimOld.SelectedIndex;
        }

        void cbAnimOld_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!IsEmuOpen || BaseAddress == 0) return;

            long address = BaseAddress + 0x64040 + (selectedAnimOld.RealIndex + 1) * 8;
            byte[] currentAnim = SwapEndian(ReadBytes(address, 8), 4);
            string currentAnimValue = BitConverter.ToString(currentAnim).Replace("-", "");

            for (int i = 0; i < animList.Count; i++)
            {
                if (animList[i].Value == currentAnimValue)
                    cbAnimNew.SelectedIndex = i;
            }
        }

        void openAppearanceSettings(object sender, EventArgs e)
        {
            switch (modelStatus)
            {
                case ModelStatus.EMPTY:
                    MessageBox.Show(Resources.colorCodeEmptyRom, "...", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    break;
                case ModelStatus.MODDED:
                    MessageBox.Show(Resources.colorCodeModdedRom, "...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case ModelStatus.COMET:
                    MessageBox.Show(Resources.colorCodeCometRom, "...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case ModelStatus.VANILLA:
                    if (appearanceForm == null || appearanceForm.IsDisposed) appearanceForm = new AppearanceForm();

                    if (!appearanceForm.Visible)
                        appearanceForm.Show();

                    if (appearanceForm.WindowState == FormWindowState.Minimized)
                        appearanceForm.WindowState = FormWindowState.Normal;
                    break;
            }
        }

        void openAboutForm(object sender, EventArgs e)
        {
            AboutForm about = new AboutForm();
            about.ShowDialog(this);
        }

        void openExtraControls(object sender, EventArgs e)
        {
            if (extraControlsForm == null || extraControlsForm.IsDisposed) extraControlsForm = new ExtraControlsForm();

            if (!extraControlsForm.Visible)
                extraControlsForm.Show();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            foreach (Addon mod in moduleList)
            {
                mod.Module.Close(e);
            }
            base.OnClosed(e);
        }

        private void showRunningPluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Taskman tman = new Taskman(ref moduleList);
            tman.Show();
        }

        private void cbAnimNew_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (chbAutoApply.Checked)
            {
                WriteAnimSwap(sender, e);
            }
        }

        private void cbAnimOld_TextChanged(object sender, EventArgs e)
        {
            // Hold up.
        }
    }
}
