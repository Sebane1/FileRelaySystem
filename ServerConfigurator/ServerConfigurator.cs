using Microsoft.VisualBasic.ApplicationServices;
using RelayUploadProtocol;
using static RelayUploadProtocol.Structs;

namespace ServerConfigurator
{
    public partial class ServerConfigurator : Form
    {
        public ServerConfigurator()
        {
            InitializeComponent();
        }

        private void serverRules_TextChanged(object sender, EventArgs e)
        {

        }

        private async void ServerConfigurator_Load(object sender, EventArgs e)
        {
            var ageGroups = Enum.GetNames<AgeGroup>();
            var contentRatings = Enum.GetNames<ServerContentRating>();
            var serverContent = Enum.GetNames<ServerContentType>();
            ageGroupComboBox.Items.AddRange(ageGroups);
            serverContentRatingComboBox.Items.AddRange(contentRatings);
            serverContentTypeComboBox.Items.AddRange(serverContent);


            serverNameTextBox.Text = await ClientManager.GetServerAlias("root");
            serverRulesTextBox.Text = await ClientManager.GetServerRules("root");
            serverDescriptionTextBox.Text = await ClientManager.GetServerDescription("root");
            ageGroupComboBox.SelectedIndex = await ClientManager.GetServerAgeGroup("root");
            serverContentRatingComboBox.SelectedIndex = await ClientManager.GetServerContentRating("root");
            serverContentTypeComboBox.SelectedIndex = await ClientManager.GetServerContentType("root");
        }

        private void testServerButton_Click(object sender, EventArgs e)
        {
            ClientManager.PutPersistedFile("root", "artemis", "cake", @"C:\Users\stel9\Pictures\AMF.png");
        }

        private async void saveSettingsButton_Click(object sender, EventArgs e)
        {
            await ClientManager.SetServerAlias("root", "artemis", serverNameTextBox.Text);
            await ClientManager.SetServerRules("root", "artemis", serverRulesTextBox.Text);
            await ClientManager.SetServerDescription("root", "artemis", serverDescriptionTextBox.Text);
            await ClientManager.SetServerAgeGroup("root", "artemis", ageGroupComboBox.SelectedIndex);
            await ClientManager.SetServerContentRating("root", "artemis", serverContentRatingComboBox.SelectedIndex);
            await ClientManager.SetServerContentType("root", "artemis", serverContentTypeComboBox.SelectedIndex);
        }
    }
}
