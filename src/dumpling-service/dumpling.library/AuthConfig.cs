using DumplingLib;
using Microsoft.Azure;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using triage.database;

namespace DumplingLib
{
    /// <summary>
    /// This class houses the code required to handle the certificate auth that lets us pull code from the KeyVault
    /// </summary>
    public class DumplingKeyVaultAuthConfig
    {
        private static X509Certificate2 FindCertificateByThumbprint(string thumbprint)
        {           
            // https://azure.microsoft.com/en-us/blog/using-certificates-in-azure-websites-applications/
            // we look to where Azure places certificates.
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        
            try
            {
                store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

                if (col == null || col.Count == 0)
                    return null;

                return col[0];
            }
            finally
            {
                store.Close();
            }
        }
        
        private static AuthenticationContext _context;
        
        private static async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            _context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            
            var result = await _context.AcquireTokenAsync(resource, _cert);

            return result.AccessToken;
        }

        private static ClientAssertionCertificate _cert;


        public static async Task RegisterAsync(string clientid, string thumbprint)
        {
            var clientAssertionCertPfx = FindCertificateByThumbprint(thumbprint);
            _cert = new ClientAssertionCertificate(clientid, clientAssertionCertPfx);

            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(DumplingKeyVaultAuthConfig.GetAccessToken));

            var storage = (await kv.GetSecretAsync("https://dumplingvault.vault.azure.net:443/secrets/dumplingstorage")).Value;
            var servicebus = (await kv.GetSecretAsync("https://dumplingvault.vault.azure.net:443/secrets/dumplingservicebus")).Value;
            var eventhub = (await kv.GetSecretAsync("https://dumplingvault.vault.azure.net:443/secrets/dumplingeventhub")).Value;
            var dbconnectionstring = (await kv.GetSecretAsync(@"https://dumplingvault.vault.azure.net:443/secrets/dumplingdbconnectionstring")).Value;

            NearbyConfig.Settings.Add("dumpling-service storage account connection string", storage);
            NearbyConfig.Settings.Add("dumpling-service bus connection string", servicebus);
            NearbyConfig.Settings.Add("dumpling-service eventhub connection string", eventhub);

            TriageDb.Init(dbconnectionstring);
        }
    }
}