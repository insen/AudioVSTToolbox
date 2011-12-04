﻿namespace ProcessVSTPlugin
{
    partial class EditorFrame
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
        	this.LoadBtn = new System.Windows.Forms.Button();
        	this.SaveBtn = new System.Windows.Forms.Button();
        	this.presetComboBox = new System.Windows.Forms.ComboBox();
        	this.presetLabel = new System.Windows.Forms.Label();
        	this.pluginPanel = new System.Windows.Forms.Panel();
        	this.statusStrip1 = new System.Windows.Forms.StatusStrip();
        	this.InvestigatePluginPresetFileCheckbox = new System.Windows.Forms.CheckBox();
        	this.PresetContentBtn = new System.Windows.Forms.Button();
        	this.pluginPanel.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// LoadBtn
        	// 
        	this.LoadBtn.Location = new System.Drawing.Point(364, 4);
        	this.LoadBtn.Name = "LoadBtn";
        	this.LoadBtn.Size = new System.Drawing.Size(46, 23);
        	this.LoadBtn.TabIndex = 0;
        	this.LoadBtn.Text = "Load";
        	this.LoadBtn.UseVisualStyleBackColor = true;
        	this.LoadBtn.Click += new System.EventHandler(this.LoadBtnClick);
        	// 
        	// SaveBtn
        	// 
        	this.SaveBtn.Location = new System.Drawing.Point(413, 4);
        	this.SaveBtn.Name = "SaveBtn";
        	this.SaveBtn.Size = new System.Drawing.Size(46, 23);
        	this.SaveBtn.TabIndex = 1;
        	this.SaveBtn.Text = "Save";
        	this.SaveBtn.UseVisualStyleBackColor = true;
        	this.SaveBtn.Click += new System.EventHandler(this.SaveBtnClick);
        	// 
        	// presetComboBox
        	// 
        	this.presetComboBox.FormattingEnabled = true;
        	this.presetComboBox.Location = new System.Drawing.Point(42, 6);
        	this.presetComboBox.Name = "presetComboBox";
        	this.presetComboBox.Size = new System.Drawing.Size(316, 21);
        	this.presetComboBox.TabIndex = 2;
        	this.presetComboBox.SelectedValueChanged += new System.EventHandler(this.PresetComboBoxSelectedValueChanged);
        	// 
        	// presetLabel
        	// 
        	this.presetLabel.Location = new System.Drawing.Point(1, 9);
        	this.presetLabel.Name = "presetLabel";
        	this.presetLabel.Size = new System.Drawing.Size(43, 24);
        	this.presetLabel.TabIndex = 3;
        	this.presetLabel.Text = "Preset:";
        	// 
        	// pluginPanel
        	// 
        	this.pluginPanel.Controls.Add(this.statusStrip1);
        	this.pluginPanel.Location = new System.Drawing.Point(1, 36);
        	this.pluginPanel.Name = "pluginPanel";
        	this.pluginPanel.Size = new System.Drawing.Size(740, 231);
        	this.pluginPanel.TabIndex = 4;
        	// 
        	// statusStrip1
        	// 
        	this.statusStrip1.Location = new System.Drawing.Point(0, 209);
        	this.statusStrip1.Name = "statusStrip1";
        	this.statusStrip1.Size = new System.Drawing.Size(740, 22);
        	this.statusStrip1.TabIndex = 0;
        	this.statusStrip1.Text = "statusStrip1";
        	// 
        	// InvestigatePluginPresetFileCheckbox
        	// 
        	this.InvestigatePluginPresetFileCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.InvestigatePluginPresetFileCheckbox.Location = new System.Drawing.Point(579, 4);
        	this.InvestigatePluginPresetFileCheckbox.Name = "InvestigatePluginPresetFileCheckbox";
        	this.InvestigatePluginPresetFileCheckbox.Size = new System.Drawing.Size(162, 24);
        	this.InvestigatePluginPresetFileCheckbox.TabIndex = 5;
        	this.InvestigatePluginPresetFileCheckbox.Text = "Track Preset File Changes";
        	this.InvestigatePluginPresetFileCheckbox.UseVisualStyleBackColor = true;
        	this.InvestigatePluginPresetFileCheckbox.CheckedChanged += new System.EventHandler(this.InvestigatePluginPresetFileCheckboxCheckedChanged);
        	// 
        	// PresetContentBtn
        	// 
        	this.PresetContentBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.PresetContentBtn.Location = new System.Drawing.Point(464, 4);
        	this.PresetContentBtn.Name = "PresetContentBtn";
        	this.PresetContentBtn.Size = new System.Drawing.Size(111, 23);
        	this.PresetContentBtn.TabIndex = 6;
        	this.PresetContentBtn.Text = "PresetFile Changes";
        	this.PresetContentBtn.UseVisualStyleBackColor = true;
        	this.PresetContentBtn.Click += new System.EventHandler(this.PresetContentBtnClick);
        	// 
        	// EditorFrame
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.AutoSize = true;
        	this.ClientSize = new System.Drawing.Size(740, 266);
        	this.Controls.Add(this.PresetContentBtn);
        	this.Controls.Add(this.InvestigatePluginPresetFileCheckbox);
        	this.Controls.Add(this.pluginPanel);
        	this.Controls.Add(this.presetLabel);
        	this.Controls.Add(this.presetComboBox);
        	this.Controls.Add(this.SaveBtn);
        	this.Controls.Add(this.LoadBtn);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        	this.MaximizeBox = false;
        	this.Name = "EditorFrame";
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        	this.Text = "EditorFrame";
        	this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditorFrameFormClosing);
        	this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EditorFrameKeyDown);
        	this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.EditorFrameKeyUp);
        	this.pluginPanel.ResumeLayout(false);
        	this.pluginPanel.PerformLayout();
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.CheckBox InvestigatePluginPresetFileCheckbox;
        private System.Windows.Forms.Button PresetContentBtn;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel pluginPanel;
        private System.Windows.Forms.Label presetLabel;
        private System.Windows.Forms.ComboBox presetComboBox;
        private System.Windows.Forms.Button LoadBtn;
        private System.Windows.Forms.Button SaveBtn;

        #endregion
        
    }
}