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
        private void InitializeComponent() {
            serverRules = new TextBox();
            ageGroupComboBox = new ComboBox();
            serverContentComboBox = new ComboBox();
            serverContentTypeComboBox = new ComboBox();
            ageGroupLabel = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            textBox1 = new TextBox();
            runServerButton = new Button();
            saveSettingsButton = new Button();
            testServerButton = new Button();
            SuspendLayout();
            // 
            // serverRules
            // 
            serverRules.Location = new Point(12, 86);
            serverRules.Multiline = true;
            serverRules.Name = "serverRules";
            serverRules.Size = new Size(375, 129);
            serverRules.TabIndex = 0;
            serverRules.TextChanged += serverRules_TextChanged;
            // 
            // ageGroupComboBox
            // 
            ageGroupComboBox.FormattingEnabled = true;
            ageGroupComboBox.Location = new Point(12, 25);
            ageGroupComboBox.Name = "ageGroupComboBox";
            ageGroupComboBox.Size = new Size(121, 23);
            ageGroupComboBox.TabIndex = 2;
            // 
            // serverContentComboBox
            // 
            serverContentComboBox.FormattingEnabled = true;
            serverContentComboBox.Location = new Point(139, 25);
            serverContentComboBox.Name = "serverContentComboBox";
            serverContentComboBox.Size = new Size(121, 23);
            serverContentComboBox.TabIndex = 3;
            // 
            // serverContentTypeComboBox
            // 
            serverContentTypeComboBox.FormattingEnabled = true;
            serverContentTypeComboBox.Location = new Point(266, 25);
            serverContentTypeComboBox.Name = "serverContentTypeComboBox";
            serverContentTypeComboBox.Size = new Size(121, 23);
            serverContentTypeComboBox.TabIndex = 4;
            // 
            // ageGroupLabel
            // 
            ageGroupLabel.AutoSize = true;
            ageGroupLabel.Location = new Point(12, 7);
            ageGroupLabel.Name = "ageGroupLabel";
            ageGroupLabel.Size = new Size(64, 15);
            ageGroupLabel.TabIndex = 5;
            ageGroupLabel.Text = "Age Group";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(139, 7);
            label1.Name = "label1";
            label1.Size = new Size(87, 15);
            label1.TabIndex = 6;
            label1.Text = "Content Rating";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(267, 7);
            label2.Name = "label2";
            label2.Size = new Size(85, 15);
            label2.TabIndex = 7;
            label2.Text = "Server Content";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 68);
            label3.Name = "label3";
            label3.Size = new Size(70, 15);
            label3.TabIndex = 8;
            label3.Text = "Server Rules";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 235);
            label4.Name = "label4";
            label4.Size = new Size(102, 15);
            label4.TabIndex = 10;
            label4.Text = "Server Description";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 253);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(375, 129);
            textBox1.TabIndex = 9;
            // 
            // runServerButton
            // 
            runServerButton.Location = new Point(128, 388);
            runServerButton.Name = "runServerButton";
            runServerButton.Size = new Size(75, 23);
            runServerButton.TabIndex = 11;
            runServerButton.Text = "Run Server";
            runServerButton.UseVisualStyleBackColor = true;
            // 
            // saveSettingsButton
            // 
            saveSettingsButton.Location = new Point(12, 388);
            saveSettingsButton.Name = "saveSettingsButton";
            saveSettingsButton.Size = new Size(110, 23);
            saveSettingsButton.TabIndex = 12;
            saveSettingsButton.Text = "Save Settings";
            saveSettingsButton.UseVisualStyleBackColor = true;
            // 
            // testServerButton
            // 
            testServerButton.Location = new Point(312, 388);
            testServerButton.Name = "testServerButton";
            testServerButton.Size = new Size(75, 23);
            testServerButton.TabIndex = 13;
            testServerButton.Text = "Test Server";
            testServerButton.UseVisualStyleBackColor = true;
            testServerButton.Click += testServerButton_Click;
            // 
            // ServerConfigurator
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(403, 420);
            Controls.Add(testServerButton);
            Controls.Add(saveSettingsButton);
            Controls.Add(runServerButton);
            Controls.Add(label4);
            Controls.Add(textBox1);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(ageGroupLabel);
            Controls.Add(serverContentTypeComboBox);
            Controls.Add(serverContentComboBox);
            Controls.Add(ageGroupComboBox);
            Controls.Add(serverRules);
            Name = "ServerConfigurator";
            Text = "Server Configurator";
            TopMost = true;
            Load += ServerConfigurator_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox serverRules;
        private ComboBox ageGroupComboBox;
        private ComboBox serverContentComboBox;
        private ComboBox serverContentTypeComboBox;
        private Label ageGroupLabel;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TextBox textBox1;
        private Button runServerButton;
        private Button saveSettingsButton;
        private Button testServerButton;
    }
}
