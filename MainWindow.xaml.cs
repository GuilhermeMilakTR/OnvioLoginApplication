using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using BlueMoon.Core.Models;

namespace LoginApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string BMSERVICES_KEY_CI = "46jgS6UF";
        private const string BMSERVICES_KEY_DEMO = "P5aOQy80";
        private const string BMSERVICES_KEY_QED = "VP69Gk2n";
        private const string ServiceName = "CoreServices";

        private Dictionary<string, string> EnvironmentKeys = new Dictionary<string, string>()
        {
            { "CI", BMSERVICES_KEY_CI },
            { "DEMO", BMSERVICES_KEY_DEMO },
            { "QED", BMSERVICES_KEY_QED }
        };

        private const string ProxyHost = "trta-prof-devops-squid.int.thomsonreuters.com";
        private const int ProxyPort = 3128;

        private readonly AccountsService accountsService;

        private AccountDTO selectedAccount;
        private AccountDTO SelectedAccount
        {
            get
            {
                return selectedAccount;
            }
            set
            {
                if (value != selectedAccount)
                {
                    selectedAccount = value;
                    FormControl.Instance.AccountSelected = value != null;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = FormControl.Instance;

            accountsService = AccountsService.Instance;

            foreach (AccountDTO account in accountsService.Accounts.Accounts)
            {
                ComboBoxItem comboBoxItem = CreateAccountComboBoxItem(account);

                cbAccounts.Items.Add(comboBoxItem);
            }

            if (cbAccounts.Items.Count > 0)
            {
                cbAccounts.SelectedIndex = 0;
            }
        }

        private void BtnAddSavedAccount_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            cbAccounts.SelectedIndex = -1;

            SelectedAccount = null;

            FormControl.Instance.IsAddingOrEditing = true;
        }

        private void BtnRemoveSavedAccount_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedAccount != null)
            {
                accountsService.RemoveByCode(SelectedAccount.Code);

                ComboBoxItem toRemove = null;

                foreach (ComboBoxItem item in cbAccounts.Items)
                {
                    if (int.Parse(item.Tag.ToString()) == SelectedAccount.Code)
                    {
                        toRemove = item;

                        break;
                    }
                }

                cbAccounts.Items.Remove(toRemove);
                cbAccounts.SelectedIndex = -1;

                SelectedAccount = null;
            }
        }

        private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (cbEnv.SelectedIndex == 3 && cbServiceType.SelectedIndex == 1)
            {
                MessageBox.Show("Não é possível gerar token em PROD com o \"InternalSecurityServices /v3/sessions\" pois não existe a chave \"bm_services\"");

                return;
            }

            string environment = (cbEnv.SelectedItem as ComboBoxItem)!.Content.ToString()!;

            try
            {
                if (cbServiceType.SelectedIndex == 0) // Internal company mapping v1/sessions
                {
                    if (!string.IsNullOrEmpty(txtCompanyId.Text))
                    {
                        btnGenerate.IsEnabled = !btnGenerate.IsEnabled;

                        string token = await CompanyMappingLogin(environment, txtCompanyId.Text.Replace("-", string.Empty).ToUpperInvariant(),
                            txtContactId.Text.Replace("-", string.Empty).ToUpperInvariant());

                        txtToken.Text = token = $"UDSLongToken {token.Replace("\"", string.Empty)}";

                        Clipboard.SetText(token);

                        btnGenerate.IsEnabled = !btnGenerate.IsEnabled;
                    }
                }
                else if (cbServiceType.SelectedIndex == 1) // InternalSecurityServices /v3/sessions
                {
                    if (!string.IsNullOrEmpty(txtCompanyId.Text) && !string.IsNullOrEmpty(txtContactId.Text) && !string.IsNullOrEmpty(txtUserId.Text))
                    {
                        btnGenerate.IsEnabled = !btnGenerate.IsEnabled;

                        string token = await InternalSecurityLogin(environment, txtCompanyId.Text.Replace("-", string.Empty).ToUpperInvariant(),
                            txtContactId.Text.Replace("-", string.Empty).ToUpperInvariant(), txtUserId.Text.Replace("-", string.Empty).ToUpperInvariant());

                        txtToken.Text = token = $"UDSLongToken {token.Replace("\"", string.Empty).Replace("{LongToken:", string.Empty).Replace("}", string.Empty)}";

                        Clipboard.SetText(token);

                        btnGenerate.IsEnabled = !btnGenerate.IsEnabled;
                    }
                }
                else // SecurityServices /v2/sessions
                {
                    if (!string.IsNullOrEmpty(txtUsername.Text) && !string.IsNullOrEmpty(txtPassword.Text))
                    {
                        btnGenerate.IsEnabled = !btnGenerate.IsEnabled;

                        string token = await SecurityServicesLogin(environment, txtUsername.Text.Replace("-", string.Empty).ToUpperInvariant(),
                            txtPassword.Text.Replace("-", string.Empty).ToUpperInvariant());

                        txtToken.Text = token = $"UDSLongToken {token.Replace("\"", string.Empty).Replace("{LongToken:", string.Empty).Replace("}", string.Empty)}";

                        Clipboard.SetText(token);

                        btnGenerate.IsEnabled = !btnGenerate.IsEnabled;
                    }
                }
            }
            catch (Exception ex)
            {
                txtToken.Text = "Falha ao gerar token.";
                btnGenerate.IsEnabled = !btnGenerate.IsEnabled;

                MessageBox.Show("Falha ao gerar token, " + ex.Message + (ex.InnerException != null ? "\n" + ex.InnerException.Message : string.Empty));
            }
        }

        private void BtnSaveAccount_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtCompanyId.Text) && (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text)))
            {
                return;
            }

            bool isNew = SelectedAccount == null;

            if (isNew)
            {
                SelectedAccount = new AccountDTO() { Code = -1 };
            }

            SelectedAccount.CompanyId = txtCompanyId.Text;
            SelectedAccount.ContactId = txtContactId.Text;
            SelectedAccount.UserId = txtUserId.Text;
            SelectedAccount.Username = txtUsername.Text;
            SelectedAccount.Password = txtPassword.Text;
            SelectedAccount.Environment = ((ComboBoxItem)cbEnv.SelectedItem).Content.ToString();

            accountsService.Save(SelectedAccount);

            if (isNew)
            {
                cbAccounts.Items.Add(CreateAccountComboBoxItem(SelectedAccount));
                cbAccounts.SelectedIndex = cbAccounts.Items.Count - 1;
            }
            else
            {
                foreach (ComboBoxItem item in cbAccounts.Items)
                {
                    if (int.Parse(item.Tag.ToString()) == SelectedAccount.Code)
                    {
                        item.Content = GenerateAccountItemName(SelectedAccount);

                        break;
                    }
                }
            }
        }

        private void CbAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbAccounts.SelectedIndex == -1)
            {
                return;
            }

            FormControl.Instance.IsAddingOrEditing = false;

            ComboBoxItem selectedItem = cbAccounts.SelectedItem as ComboBoxItem;

            if (int.TryParse(selectedItem.Tag.ToString(), out int code))
            {
                AccountDTO account = accountsService.FindByCode(code);

                if (account != null)
                {
                    SelectedAccount = account;

                    txtCompanyId.Text = account.CompanyId;
                    txtContactId.Text = account.ContactId;
                    txtUserId.Text = account.UserId;
                    txtUsername.Text = account.Username;
                    txtPassword.Text = account.Password;

                    cbEnv.SelectedIndex = GetIndexByDomainName(account.Environment);
                }
            }
        }

        private void TxtToken_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtToken.Text.Trim()))
            {
                Clipboard.SetText(txtToken.Text.Trim());
            }
        }

        private async Task<string> CompanyMappingLogin(string environment, string companyId, string contactId)
        {
            using (HttpClientHandler clientHandler = GetProxiedHttpClientHandler())
            {
                using (HttpClient httpClient = new HttpClient(clientHandler))
                {
                    Uri uri;
                    string domain = GetDomainUri(environment, true);

                    if (!string.IsNullOrEmpty(contactId))
                    {
                        uri = new Uri(domain + "api/internalcompanymapping/v1/sessions/session/" + companyId + "/" + contactId);
                    }
                    else
                    {
                        uri = new Uri(domain + "api/internalcompanymapping/v1/sessions/session/" + companyId);
                    }

                    HttpResponseMessage response = await httpClient.PostAsync(uri, null);
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        private async Task<string> InternalSecurityLogin(string environment, string companyId, string contactId, string userId)
        {
            using (HttpClientHandler clientHandler = GetProxiedHttpClientHandler())
            {
                using (HttpClient httpClient = new HttpClient(clientHandler))
                {
                    string domain = GetDomainUri(environment, true);
                    Uri uri = new Uri(domain + "api/internalsecurity/v3/sessions");

                    using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, uri))
                    {
                        requestMessage.Headers.Add("Authorization", BlueMoonServicesToken.GenerateAuthHeader(
                            () => EnvironmentKeys[environment],
                            BlueMoonServicesToken.BMServicesV2HeaderPrefix,
                            ServiceName,
                            DateTime.UtcNow,
                            []
                            ).ToStringHeader());

                        requestMessage.Content = new StringContent(JsonSerializer.Serialize(new { id = contactId, companyId, userId }), Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

                        response.EnsureSuccessStatusCode();

                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
        }

        private async Task<string> SecurityServicesLogin(string environment, string username, string password)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string domain = GetDomainUri(environment, false);
                Uri uri = new Uri(domain + "api/security/v2/sessions");

                HttpResponseMessage response = await httpClient.PostAsync(uri, new StringContent(JsonSerializer.Serialize(new { username, password }), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        private HttpClientHandler GetProxiedHttpClientHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();

            handler.Proxy = new WebProxy(ProxyHost, ProxyPort);

            return handler;
        }

        private string GetDomainUri(string selectedDomain, bool isInternal)
        {
            if (!string.IsNullOrEmpty(selectedDomain))
            {
                string internalUri = isInternal ? "int." : string.Empty;

                switch (selectedDomain)
                {
                    case "CI":
                        return $"https://ci.{internalUri}onvio.com.br/";
                    case "DEMO":
                        return $"https://demo.{internalUri}onvio.com.br/";
                    case "QED":
                        return $"https://qed.{internalUri}onvio.com.br/";
                    case "PROD":
                        return $"https://{internalUri}onvio.com.br/";
                }
            }

            return string.Empty;
        }

        private static int GetIndexByDomainName(string environment)
        {
            if (!string.IsNullOrEmpty(environment))
            {
                return environment.ToUpperInvariant() switch
                {
                    "CI" => 0,
                    "DEMO" => 1,
                    "QED" => 2,
                    "PROD" => 3,
                    _ => 0,
                };
            }

            return 0;
        }

        private ComboBoxItem CreateAccountComboBoxItem(AccountDTO account)
        {
            return new ComboBoxItem()
            {
                Content = GenerateAccountItemName(account),
                Tag = account.Code
            };
        }

        private string GenerateAccountItemName(AccountDTO account)
        {
            if (!string.IsNullOrEmpty(account.Username))
            {
                return (string.IsNullOrEmpty(account.Environment) ? string.Empty : $"[{account.Environment}]\t") + account.Username;
            }
            else if (!string.IsNullOrEmpty(account.CompanyId))
            {
                return (string.IsNullOrEmpty(account.Environment) ? string.Empty : $"[{account.Environment}]\t") + account.CompanyId;
            }

            return string.Empty;
        }

        private void ClearFields()
        {
            txtCompanyId.Clear();
            txtContactId.Clear();
            txtUserId.Clear();
            txtUsername.Clear();
            txtPassword.Clear();

            cbEnv.SelectedIndex = 0;
        }
    }

}