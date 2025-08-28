namespace ServerConfigurator {
    partial class ServerConfigurator {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            serverRulesTextBox = new TextBox();
            ageGroupComboBox = new ComboBox();
            serverContentRatingComboBox = new ComboBox();
            serverContentTypeComboBox = new ComboBox();
            ageGroupLabel = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            serverDescriptionTextBox = new TextBox();
            runServerButton = new Button();
            saveSettingsButton = new Button();
            testServerButton = new Button();
            serverNameTextBox = new TextBox();
            label5 = new Label();
            SuspendLayout();
            // 
            // serverRulesTextBox
            // 
            serverRulesTextBox.Location = new Point(12, 126);
            serverRulesTextBox.Multiline = true;
            serverRulesTextBox.Name = "serverRulesTextBox";
            serverRulesTextBox.Size = new Size(375, 129);
            serverRulesTextBox.TabIndex = 0;
            serverRulesTextBox.TextChanged += serverRules_TextChanged;
            // 
            // ageGroupComboBox
            // 
            ageGroupComboBox.FormattingEnabled = true;
            ageGroupComboBox.Location = new Point(12, 65);
            ageGroupComboBox.Name = "ageGroupComboBox";
            ageGroupComboBox.Size = new Size(121, 23);
            ageGroupComboBox.TabIndex = 2;
            // 
            // serverContentComboBox
            // 
            serverContentRatingComboBox.FormattingEnabled = true;
            serverContentRatingComboBox.Location = new Point(139, 65);
            serverContentRatingComboBox.Name = "serverContentComboBox";
            serverContentRatingComboBox.Size = new Size(121, 23);
            serverContentRatingComboBox.TabIndex = 3;
            // 
            // serverContentTypeComboBox
            // 
            serverContentTypeComboBox.FormattingEnabled = true;
            serverContentTypeComboBox.Location = new Point(266, 65);
            serverContentTypeComboBox.Name = "serverContentTypeComboBox";
            serverContentTypeComboBox.Size = new Size(121, 23);
            serverContentTypeComboBox.TabIndex = 4;
            // 
            // ageGroupLabel
            // 
            ageGroupLabel.AutoSize = true;
            ageGroupLabel.Location = new Point(12, 47);
            ageGroupLabel.Name = "ageGroupLabel";
            ageGroupLabel.Size = new Size(64, 15);
            ageGroupLabel.TabIndex = 5;
            ageGroupLabel.Text = "Age Group";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(139, 47);
            label1.Name = "label1";
            label1.Size = new Size(87, 15);
            label1.TabIndex = 6;
            label1.Text = "Content Rating";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(267, 47);
            label2.Name = "label2";
            label2.Size = new Size(85, 15);
            label2.TabIndex = 7;
            label2.Text = "Server Content";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 108);
            label3.Name = "label3";
            label3.Size = new Size(70, 15);
            label3.TabIndex = 8;
            label3.Text = "Server Rules";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 275);
            label4.Name = "label4";
            label4.Size = new Size(102, 15);
            label4.TabIndex = 10;
            label4.Text = "Server Description";
            // 
            // serverDescriptionTextBox
            // 
            serverDescriptionTextBox.Location = new Point(12, 293);
            serverDescriptionTextBox.Multiline = true;
            serverDescriptionTextBox.Name = "serverDescriptionTextBox";
            serverDescriptionTextBox.Size = new Size(375, 129);
            serverDescriptionTextBox.TabIndex = 9;
            // 
            // runServerButton
            // 
            runServerButton.Location = new Point(128, 428);
            runServerButton.Name = "runServerButton";
            runServerButton.Size = new Size(75, 23);
            runServerButton.TabIndex = 11;
            runServerButton.Text = "Run Server";
            runServerButton.UseVisualStyleBackColor = true;
            // 
            // saveSettingsButton
            // 
            saveSettingsButton.Location = new Point(12, 428);
            saveSettingsButton.Name = "saveSettingsButton";
            saveSettingsButton.Size = new Size(110, 23);
            saveSettingsButton.TabIndex = 12;
            saveSettingsButton.Text = "Save Settings";
            saveSettingsButton.UseVisualStyleBackColor = true;
            saveSettingsButton.Click += saveSettingsButton_Click;
            // 
            // testServerButton
            // 
            testServerButton.Location = new Point(312, 428);
            testServerButton.Name = "testServerButton";
            testServerButton.Size = new Size(75, 23);
            testServerButton.TabIndex = 13;
            testServerButton.Text = "Test Server";
            testServerButton.UseVisualStyleBackColor = true;
            testServerButton.Click += testServerButton_Click;
            // 
            // serverNameTextBox
            // 
            serverNameTextBox.Location = new Point(139, 12);
            serverNameTextBox.Name = "serverNameTextBox";
            serverNameTextBox.Size = new Size(248, 23);
            serverNameTextBox.TabIndex = 14;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 15);
            label5.Name = "label5";
            label5.Size = new Size(74, 15);
            label5.TabIndex = 15;
            label5.Text = "Server Name";
            // 
            // ServerConfigurator
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(403, 463);
            Controls.Add(label5);
            Controls.Add(serverNameTextBox);
            Controls.Add(testServerButton);
            Controls.Add(saveSettingsButton);
            Controls.Add(runServerButton);
            Controls.Add(label4);
            Controls.Add(serverDescriptionTextBox);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(ageGroupLabel);
            Controls.Add(serverContentTypeComboBox);
            Controls.Add(serverContentRatingComboBox);
            Controls.Add(ageGroupComboBox);
            Controls.Add(serverRulesTextBox);
            Name = "ServerConfigurator";
            Text = "Server Configurator";
            TopMost = true;
            Load += ServerConfigurator_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox serverRulesTextBox;
        private ComboBox ageGroupComboBox;
        private ComboBox serverContentRatingComboBox;
        private ComboBox serverContentTypeComboBox;
        private Label ageGroupLabel;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TextBox serverDescriptionTextBox;
        private Button runServerButton;
        private Button saveSettingsButton;
        private Button testServerButton;
        private TextBox serverNameTextBox;
        private Label label5;
    }
}
