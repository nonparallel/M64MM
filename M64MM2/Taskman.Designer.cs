﻿namespace M64MM2
{
    partial class Taskman
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Taskman));
            this.listView1 = new System.Windows.Forms.ListView();
            this.btn_OK = new System.Windows.Forms.Button();
            this.lb_Desc = new System.Windows.Forms.Label();
            this.cb_pluginEnabled = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Location = new System.Drawing.Point(12, 12);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(316, 137);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // btn_OK
            // 
            this.btn_OK.Location = new System.Drawing.Point(253, 254);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(75, 23);
            this.btn_OK.TabIndex = 1;
            this.btn_OK.Text = "OK";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // lb_Desc
            // 
            this.lb_Desc.Location = new System.Drawing.Point(12, 152);
            this.lb_Desc.Name = "lb_Desc";
            this.lb_Desc.Size = new System.Drawing.Size(316, 99);
            this.lb_Desc.TabIndex = 2;
            this.lb_Desc.Text = "<Please select an item.>";
            this.lb_Desc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cb_pluginEnabled
            // 
            this.cb_pluginEnabled.AutoSize = true;
            this.cb_pluginEnabled.Location = new System.Drawing.Point(15, 258);
            this.cb_pluginEnabled.Name = "cb_pluginEnabled";
            this.cb_pluginEnabled.Size = new System.Drawing.Size(175, 17);
            this.cb_pluginEnabled.TabIndex = 3;
            this.cb_pluginEnabled.Text = "Plugin can execute every frame";
            this.cb_pluginEnabled.UseVisualStyleBackColor = true;
            this.cb_pluginEnabled.CheckedChanged += new System.EventHandler(this.cb_pluginEnabled_CheckedChanged);
            // 
            // Taskman
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(340, 289);
            this.Controls.Add(this.cb_pluginEnabled);
            this.Controls.Add(this.lb_Desc);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.listView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Taskman";
            this.Text = "Installed Addons";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Label lb_Desc;
        private System.Windows.Forms.CheckBox cb_pluginEnabled;
    }
}