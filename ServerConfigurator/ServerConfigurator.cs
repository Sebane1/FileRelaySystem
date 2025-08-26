using FileRelaySystem;
using static RelayUploadProtocol.Structs;

namespace ServerConfigurator {
    public partial class ServerConfigurator : Form {
        public ServerConfigurator() {
            InitializeComponent();
        }

        private void serverRules_TextChanged(object sender, EventArgs e) {

        }

        private void ServerConfigurator_Load(object sender, EventArgs e) {
            var ageGroups = Enum.GetNames<AgeGroup>();
            var contentRatings = Enum.GetNames<ServerContentRating>();
            var serverContent = Enum.GetNames<ServerContentType>();
            ageGroupComboBox.Items.AddRange(ageGroups);
            serverContentComboBox.Items.AddRange(contentRatings);
            serverContentTypeComboBox.Items.AddRange(serverContent);
        }

        private void testServerButton_Click(object sender, EventArgs e) {

        }
    }
}
