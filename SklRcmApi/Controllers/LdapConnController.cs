using System;
using System.Collections.Generic;
using System.DirectoryServices;
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
		public bool Login(UserData userdata)
		{
			string username = userdata.username;
			string password = userdata.password;
            if (ValidateLDAPUser(username, password))
            {
				GetLdapUserData(username, password);
				return true;
            }
            else
            {
				return false;
            }
			
		}
		static bool ValidateLDAPUser(string userId, string password)
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
					var ldapUserId = userId+"@systex.tw";
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
		private Dictionary<string,string> GetLdapUserData(string userId, string password)
        {
			//SearchResultCollection sResults = null;
			
			SearchResult objSearchResult;
			ResultPropertyCollection objPropertyCollection;
			string path = "localhost:10389";
			
			//init a directory entry
			DirectoryEntry dEntry = new DirectoryEntry(path, userId, password);
			DirectorySearcher dSearcher = new DirectorySearcher(dEntry);

			Dictionary<string, string> properties = new Dictionary<string, string>();
				
			dSearcher.Filter = "(cn="+ userId + ")";
            try
            {
				dSearcher.FindOne();
			}
			catch (Exception e)
            {
				System.Diagnostics.Debug.WriteLine(e);
            }
			objSearchResult = dSearcher.FindOne();
			if (objSearchResult != null)
				{
					objPropertyCollection = objSearchResult.Properties;
					foreach (string strPropertyName in objPropertyCollection.PropertyNames)
					{
						foreach (Object objMyCollection in objPropertyCollection[strPropertyName])
						{
						Console.WriteLine("Property Value: " + (string)objMyCollection.ToString());
					}
					}
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
