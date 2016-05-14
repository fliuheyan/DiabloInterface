﻿using System;
using System.Drawing;
using System.Windows.Forms;
using DiabloInterface.Properties;
using System.Collections.Generic;

namespace DiabloInterface
{
    public partial class SettingsWindow : Form
    {

        private MainWindow main;
        
        private bool awaitingInput = false;

        public SettingsWindow( MainWindow main )
        {
            this.main = main;
            InitializeComponent();
        }

        private void resetSettings()
        {
            //todo : implement
        }

        private void saveSettings()
        {
            List<AutoSplit.AutoSplit> asList = new List<AutoSplit.AutoSplit>();
            foreach (AutoSplit.AutoSplit a in main.settings.autosplits) {
                if (!a.deleted)
                {
                    asList.Add(a);
                }
            }
            main.settings.autosplits = asList;
            main.settings.doAutosplit = chkAutosplit.Checked;
            main.settings.triggerKeys = txtAutoSplitHotkey.Text;
            main.settings.fontSize = Int32.Parse(txtFontSize.Text);
            main.settings.titleFontSize = Int32.Parse(txtTitleFontSize.Text);
            main.settings.fontName = lblFontExample.Text;

            main.settings.save();
            main.applySettings();
            main.updateAutosplits();
        }

        private void btnFont_Click(object sender, EventArgs e)
        {
            FontDialog fontDialog1 = new FontDialog();
            fontDialog1.Font = new Font(Settings.Default["Font"].ToString(), 8);
            DialogResult result = fontDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                lblFontExample.Text = fontDialog1.Font.Name;
            }
        }

        private void SettingsWindow_Load(object sender, EventArgs e)
        {
            this.lblFontExample.Text = main.settings.fontName;
            this.txtFontSize.Text = main.settings.fontSize.ToString();
            this.txtTitleFontSize.Text = main.settings.titleFontSize.ToString();
            this.chkCreateFiles.Checked = main.settings.createFiles;
            this.chkAutosplit.Checked = main.settings.doAutosplit;
            this.txtAutoSplitHotkey.Text = main.settings.triggerKeys;

            int x = 0;
            foreach (AutoSplit.AutoSplit a in main.settings.autosplits)
            {
                addAutosplit(a, x, false);
                x++;
            }
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            this.saveSettings();
            this.Close();
        }
        private void btnApply_Click(object sender, EventArgs e)
        {
            this.saveSettings();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.resetSettings();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AutoSplit.AutoSplit autosplit = new AutoSplit.AutoSplit();
            this.addAutosplit(autosplit, main.settings.autosplits.Count);
        }

        private void addAutosplit(AutoSplit.AutoSplit autosplit, int idx, bool addToMain = true)
        {
            TextBox txtName = new TextBox();
            ComboBox cmbType = new ComboBox();
            ComboBox cmbValueCharLevel = new ComboBox();
            ComboBox cmbValueArea = new ComboBox();
            ComboBox cmbValueItem = new ComboBox();
            ComboBox cmbValueQuest = new ComboBox();
            ComboBox cmbValueSpecial = new ComboBox();

            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbValueCharLevel.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbValueArea.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbValueItem.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbValueQuest.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbValueSpecial.DropDownStyle = ComboBoxStyle.DropDownList;
            Button btnRemove = new Button();
            txtName.SetBounds(0, 24 + idx * 24, 80, 16);
            cmbType.SetBounds(90, 24 + idx * 24, 50, 16);
            cmbValueCharLevel.SetBounds(150, 24 + idx * 24, 150, 16);
            cmbValueArea.SetBounds(150, 24 + idx * 24, 150, 16);
            cmbValueItem.SetBounds(150, 24 + idx * 24, 150, 16);
            cmbValueQuest.SetBounds(150, 24 + idx * 24, 150, 16);
            cmbValueSpecial.SetBounds(150, 24 + idx * 24, 150, 16);
            btnRemove.SetBounds(300, 24 + idx * 24, 20, 20);
            btnRemove.Text = "X";

            cmbValueCharLevel.Hide();
            cmbValueArea.Hide();
            cmbValueItem.Hide();
            cmbValueQuest.Hide();

            autosplit.setSettingControls(
                txtName, 
                cmbType, 
                cmbValueCharLevel, 
                cmbValueArea, 
                cmbValueItem, 
                cmbValueQuest,
                cmbValueSpecial
            );

            this.panel1.Controls.Add(txtName);
            this.panel1.Controls.Add(cmbType);
            this.panel1.Controls.Add(cmbValueCharLevel);
            this.panel1.Controls.Add(cmbValueArea);
            this.panel1.Controls.Add(cmbValueItem);
            this.panel1.Controls.Add(cmbValueQuest);
            this.panel1.Controls.Add(cmbValueSpecial);
            this.panel1.Controls.Add(btnRemove);
            btnRemove.Click += BtnRemove_Click;
            btnRemove.Tag = autosplit;
            
            if (addToMain)
            {
                main.settings.autosplits.Add(autosplit);
            }
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            AutoSplit.AutoSplit a = (AutoSplit.AutoSplit)b.Tag;
            a.deleted = true;
        }
        
        private List<Keys> downKeys = new List<Keys>();
        private List<Keys> upKeys = new List<Keys>();
        private void txtAutoSplitHotkey_KeyDown(object sender, KeyEventArgs e)
        {
            if (!downKeys.Contains(e.KeyCode))
            {
                downKeys.Add(e.KeyCode);
            }
            e.Handled = true;
        }

        private void txtAutoSplitHotkey_KeyUp(object sender, KeyEventArgs e)
        {
            if (!upKeys.Contains(e.KeyCode))
            {
                upKeys.Add(e.KeyCode);
            }

            if (downKeys.Contains(e.KeyCode))
            {
                downKeys.Remove(e.KeyCode);
                if (downKeys.Count == 0)
                {
                    String str = "";
                    foreach (Keys k in upKeys)
                    {
                        str += (str == "" ? "" : "+") + k.ToString();
                    }
                    upKeys.Clear();
                    txtAutoSplitHotkey.Text = str;
                    e.Handled = true;
                }
            }
        }

        private void txtAutoSplitHotkey_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }
    }

}