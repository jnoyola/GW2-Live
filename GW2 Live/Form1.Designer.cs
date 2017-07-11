namespace GW2_Live
{
    partial class Form1
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.livePage = new System.Windows.Forms.TabPage();
            this.startLabel = new System.Windows.Forms.Label();
            this.liveStartButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.toggleChatText = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.interactText = new System.Windows.Forms.TextBox();
            this.inventoryText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.planningPage = new System.Windows.Forms.TabPage();
            this.hotkeyLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.resetButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.editButton = new System.Windows.Forms.Button();
            this.planningLabel = new System.Windows.Forms.Label();
            this.mapView = new GW2_Live.MapView();
            this.pathButton = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.livePage.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.planningPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mapView)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.livePage);
            this.tabControl.Controls.Add(this.planningPage);
            this.tabControl.Location = new System.Drawing.Point(-3, 0);
            this.tabControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(590, 563);
            this.tabControl.TabIndex = 0;
            // 
            // livePage
            // 
            this.livePage.Controls.Add(this.startLabel);
            this.livePage.Controls.Add(this.liveStartButton);
            this.livePage.Controls.Add(this.groupBox1);
            this.livePage.Location = new System.Drawing.Point(4, 30);
            this.livePage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.livePage.Name = "livePage";
            this.livePage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.livePage.Size = new System.Drawing.Size(582, 529);
            this.livePage.TabIndex = 0;
            this.livePage.Text = "GW2 Live";
            this.livePage.UseVisualStyleBackColor = true;
            // 
            // startLabel
            // 
            this.startLabel.Location = new System.Drawing.Point(160, 153);
            this.startLabel.Name = "startLabel";
            this.startLabel.Size = new System.Drawing.Size(415, 50);
            this.startLabel.TabIndex = 2;
            this.startLabel.Text = "Waiting for Mumble Link . . .";
            this.startLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // liveStartButton
            // 
            this.liveStartButton.Location = new System.Drawing.Point(7, 153);
            this.liveStartButton.Name = "liveStartButton";
            this.liveStartButton.Size = new System.Drawing.Size(147, 50);
            this.liveStartButton.TabIndex = 1;
            this.liveStartButton.Text = "Start";
            this.liveStartButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.toggleChatText);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.interactText);
            this.groupBox1.Controls.Add(this.inventoryText);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(7, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(568, 138);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Configuration";
            // 
            // toggleChatText
            // 
            this.toggleChatText.Font = new System.Drawing.Font("Segoe UI Symbol", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toggleChatText.Location = new System.Drawing.Point(92, 94);
            this.toggleChatText.Name = "toggleChatText";
            this.toggleChatText.Size = new System.Drawing.Size(55, 25);
            this.toggleChatText.TabIndex = 5;
            this.toggleChatText.Text = "\\";
            this.toggleChatText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Symbol", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(6, 97);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Toggle Chat";
            // 
            // interactText
            // 
            this.interactText.Font = new System.Drawing.Font("Segoe UI Symbol", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.interactText.Location = new System.Drawing.Point(92, 63);
            this.interactText.Name = "interactText";
            this.interactText.Size = new System.Drawing.Size(55, 25);
            this.interactText.TabIndex = 3;
            this.interactText.Text = "F";
            this.interactText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // inventoryText
            // 
            this.inventoryText.Font = new System.Drawing.Font("Segoe UI Symbol", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.inventoryText.Location = new System.Drawing.Point(92, 32);
            this.inventoryText.Name = "inventoryText";
            this.inventoryText.Size = new System.Drawing.Size(55, 25);
            this.inventoryText.TabIndex = 2;
            this.inventoryText.Text = "I";
            this.inventoryText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Symbol", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "Interact";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Symbol", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Inventory";
            // 
            // planningPage
            // 
            this.planningPage.Controls.Add(this.pathButton);
            this.planningPage.Controls.Add(this.hotkeyLabel);
            this.planningPage.Controls.Add(this.label4);
            this.planningPage.Controls.Add(this.resetButton);
            this.planningPage.Controls.Add(this.saveButton);
            this.planningPage.Controls.Add(this.editButton);
            this.planningPage.Controls.Add(this.planningLabel);
            this.planningPage.Controls.Add(this.mapView);
            this.planningPage.Location = new System.Drawing.Point(4, 30);
            this.planningPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.planningPage.Name = "planningPage";
            this.planningPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.planningPage.Size = new System.Drawing.Size(582, 529);
            this.planningPage.TabIndex = 1;
            this.planningPage.Text = "GW2 Planning";
            this.planningPage.UseVisualStyleBackColor = true;
            // 
            // hotkeyLabel
            // 
            this.hotkeyLabel.BackColor = System.Drawing.Color.White;
            this.hotkeyLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.hotkeyLabel.Font = new System.Drawing.Font("Segoe UI Symbol", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hotkeyLabel.Location = new System.Drawing.Point(7, 490);
            this.hotkeyLabel.Name = "hotkeyLabel";
            this.hotkeyLabel.Size = new System.Drawing.Size(58, 25);
            this.hotkeyLabel.TabIndex = 8;
            this.hotkeyLabel.Text = "[keys]";
            this.hotkeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.hotkeyLabel.Click += new System.EventHandler(this.hotkeyLabel_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Symbol", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(11, 470);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Hotkey";
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(7, 87);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(58, 30);
            this.resetButton.TabIndex = 4;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(7, 51);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(58, 30);
            this.saveButton.TabIndex = 3;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // editButton
            // 
            this.editButton.Location = new System.Drawing.Point(7, 15);
            this.editButton.Name = "editButton";
            this.editButton.Size = new System.Drawing.Size(58, 30);
            this.editButton.TabIndex = 2;
            this.editButton.Text = "Edit";
            this.editButton.UseVisualStyleBackColor = true;
            this.editButton.Click += new System.EventHandler(this.editButton_Click);
            // 
            // planningLabel
            // 
            this.planningLabel.Location = new System.Drawing.Point(71, 244);
            this.planningLabel.Name = "planningLabel";
            this.planningLabel.Size = new System.Drawing.Size(500, 42);
            this.planningLabel.TabIndex = 1;
            this.planningLabel.Text = "Waiting for Mumble Link . . .";
            this.planningLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mapView
            // 
            this.mapView.BackColor = System.Drawing.Color.LightGray;
            this.mapView.IsEditing = false;
            this.mapView.Location = new System.Drawing.Point(71, 15);
            this.mapView.Name = "mapView";
            this.mapView.Plan = null;
            this.mapView.Size = new System.Drawing.Size(500, 500);
            this.mapView.TabIndex = 5;
            this.mapView.TabStop = false;
            this.mapView.Click += new System.EventHandler(this.mapView_Click);
            // 
            // pathButton
            // 
            this.pathButton.Location = new System.Drawing.Point(7, 250);
            this.pathButton.Name = "pathButton";
            this.pathButton.Size = new System.Drawing.Size(58, 30);
            this.pathButton.TabIndex = 9;
            this.pathButton.Text = "Path";
            this.pathButton.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 561);
            this.Controls.Add(this.tabControl);
            this.Font = new System.Drawing.Font("Segoe UI Symbol", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "GW2 Live";
            this.tabControl.ResumeLayout(false);
            this.livePage.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.planningPage.ResumeLayout(false);
            this.planningPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mapView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage livePage;
        private System.Windows.Forms.TabPage planningPage;
        private System.Windows.Forms.Label startLabel;
        private System.Windows.Forms.Button liveStartButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox toggleChatText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox interactText;
        private System.Windows.Forms.TextBox inventoryText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label planningLabel;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button editButton;
        private MapView mapView;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button pathButton;
        private System.Windows.Forms.Label hotkeyLabel;
    }
}

