using DumplingLib;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace dumplingWeb.App_Start
{
    public class AuthConfig
    {

        public static class CertificateHelper
        {
            public static X509Certificate2 FindCertificateByThumbprint(string findValue)
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                try
                {
                    store.Open(OpenFlags.ReadOnly);

                    X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByThumbprint, findValue, false); // TODO CHANGE THIS FACT: Don't validate certs, since the test root isn't installed.

                    if (col == null || col.Count == 0)
                        return null;
                    return col[0];
                }
                finally
                {
                    store.Close();
                }
            }
        }

        private static AuthenticationContext _context;
        
        public static async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            _context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            

            var result = await _context.AcquireTokenAsync(resource, AssertionCert);

            return result.AccessToken;
        }

        private static ClientAssertionCertificate _cert;
        public static ClientAssertionCertificate AssertionCert
        {
            get
            {
                if (_cert == null)
                {
                    var clientAssertionCertPfx = CertificateHelper.FindCertificateByThumbprint(WebConfigurationManager.AppSettings["thumbprint"]);
                    AssertionCert = new ClientAssertionCertificate(WebConfigurationManager.AppSettings["dumpling_ad_client_id"], clientAssertionCertPfx);
                }

                return _cert;
            }
            private set
            {
                _cert = value;
            }
        }

        public static async Task RegisterAsync()
        {
            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(AuthConfig.GetAccessToken));

            var storage = (await kv.GetSecretAsync("https://dumplingvault.vault.azure.net:443/secrets/dumplingstorage")).Value;
            var servicebus = (await kv.GetSecretAsync("https://dumplingvault.vault.azure.net:443/secrets/dumplingservicebus")).Value;
            var eventhub = (await kv.GetSecretAsync("https://dumplingvault.vault.azure.net:443/secrets/dumplingeventhub")).Value;

            NearbyConfig.Settings.Add("dumpling-service storage account connection string", storage);
            NearbyConfig.Settings.Add("dumpling-service bus connection string", servicebus);
            NearbyConfig.Settings.Add("dumpling-service eventhub connection string", eventhub);
        }
    }
}