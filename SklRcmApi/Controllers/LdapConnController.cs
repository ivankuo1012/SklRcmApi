using System;
using System.Collections.Generic;
using System.DirectoryServices;
//using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SklRcmApi.Controllers
{
    public class LdapConnController : ApiController
    {
        public IEnumerable<string> Get()
        {
			string ladpserver = "10.1.255.200";
			string port = "10389";
			string userId = "1600218S";
			
            return new string[] { "value1", "value2" };
        }
		[HttpPost]
		public Dictionary<string,string> Login(UserData userdata)
		{
			string username = userdata.username;
			string password = userdata.password;
			Dictionary<string, string> userData = new Dictionary<string, string>();

			if (ValidateLDAPUser(username, password))
            {
				userData = GetLdapUserData(username, password);
				//return userData;
            }
			return userData;
            
			
		}
		static bool ValidateLDAPUser(string username, string password)
		{
			string ldapserver = "localhost";
			string port = "10389";
			try
			{
				using (var ldapConnection = new LdapConnection(
						new LdapDirectoryIdentifier($"{ldapserver}:{port}")))
				{
					ldapConnection.AuthType = AuthType.Basic;
					ldapConnection.AutoBind = false;
					ldapConnection.Timeout = new TimeSpan(0, 0, 0, 15);
					var ldapUserId = username + "@systex.tw";
					var credential = new NetworkCredential(ldapUserId, password);
					ldapConnection.Bind(credential);
					

					return true;
				}

			}
			catch (LdapException e)
			{
				Console.WriteLine(("Error with ldap server " + ldapserver + e.ToString()));
				return false;
			}
		}
		
		private Dictionary<string,string> GetLdapUserData(string username, string password)
        {
			//SearchResultCollection sResults = null;
			
			
			//string ldapServer = "localhost:10389";
			string path = "LDAP://localhost:10389/DC=systex,DC=tw";
			
			//init a directory entry
			DirectoryEntry dEntry = new DirectoryEntry(path, username, password);
			dEntry.Path= path;
			DirectorySearcher dSearcher = new DirectorySearcher(dEntry);

			Dictionary<string, string> properties = new Dictionary<string, string>();
				
			dSearcher.Filter = "(cn="+ username + ")";
			SearchResult result = dSearcher.FindOne();
			if (result != null)
			{
				// user exists, cycle through LDAP fields (cn, telephonenumber etc.)  

				ResultPropertyCollection fields = result.Properties;

				foreach (String ldapField in fields.PropertyNames)
				{
					// cycle through objects in each field e.g. group membership  
					// (for many fields there will only be one object such as name)  

					foreach (Object myCollection in fields[ldapField])
                        if (!properties.ContainsKey(ldapField))
                        {
							properties.Add(ldapField, myCollection.ToString());
						}
						
						//Console.WriteLine(String.Format("{0,-20} : {1}",ldapField, myCollection.ToString()));
				}
			}

			else
			{
				// user does not exist  
				Console.WriteLine("User not found!");
			}
			/*
			foreach (SearchResult searchResult in sResults)
				{
					
						ResultPropertyValueCollection valueCollection =
						searchResult.Properties["position"];
					properties.Add("position", valueCollection.ToString());

						foreach (Object propertyValue in valueCollection)
						{

							
							Console.WriteLine("Property Value: " + (string)propertyValue.ToString());

							//["sAMAccountName"][0].ToString();
						}
						//}
						
					
				}*/
			return properties;
			


		}
		public class UserData
		{
			public string username { get; set; }
			public string password { get; set; }
		}
	}

}
