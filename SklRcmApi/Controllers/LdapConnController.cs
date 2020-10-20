using System;
using System.Collections.Generic;
using System.DirectoryServices;
//using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace SklRcmApi.Controllers
{
    public class LdapConnController : ApiController
    {
		public Dictionary<string,string> Login(UserAuthData userdata)
		{
			string username = userdata.Username;
			string password = userdata.Password;
			Dictionary<string, string> userData = new Dictionary<string, string>();

			if (ValidateLDAPUser(username, password))
            {
				userData = GetLdapUserData(username);
				//return userData;
            }
			return userData;
            
			
		}
		
		static bool ValidateLDAPUser(string username, string password)
		{
			
			string ldapserver = System.Configuration.ConfigurationManager.AppSettings.Get("LdapServer"); //= "localhost";
			string port = System.Configuration.ConfigurationManager.AppSettings.Get("LdapServerPort"); // = "10389";
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
		[System.Web.Mvc.HttpPost]
		public Dictionary<string,string> GetLdapUserData(string searchUser)
        {
			string ldapserver = System.Configuration.ConfigurationManager.AppSettings.Get("LdapServer"); //= "localhost";
			string port = System.Configuration.ConfigurationManager.AppSettings.Get("LdapServerPort"); // = "10389";
			string path = "LDAP://"+ ldapserver + ":"+ port + "/DC=systex,DC=tw";
			string username = System.Configuration.ConfigurationManager.AppSettings.Get("LdapServerUserName");//= "1600218s";
			string password = System.Configuration.ConfigurationManager.AppSettings.Get("LdapServerUserPassword"); //= "P@ssw0rdIvankuo";
			string role = System.Configuration.ConfigurationManager.AppSettings.Get("role"); //= "P@ssw0rdIvankuo";
																												   //init a directory entry
			DirectoryEntry dEntry = new DirectoryEntry(path, username, password);
			//dEntry.Path= path;
			DirectorySearcher dSearcher = new DirectorySearcher(dEntry);

			Dictionary<string, string> ldapUserData = new Dictionary<string, string>();
				
			dSearcher.Filter = "(|(cn=*"+ searchUser + "*)(displayname=*" + searchUser + "*)(sn=*" + searchUser + "*))";
			SearchResult result = dSearcher.FindOne();
			string[] listLdapField = { "displayname", "title", "department", "name", "mail" };
			if (result != null)
			{
				// user exists, cycle through LDAP fields (cn, telephonenumber etc.)  

				ResultPropertyCollection fields = result.Properties;

				foreach (String ldapField in fields.PropertyNames)
				{
					// cycle through objects in each field e.g. group membership  
					// (for many fields there will only be one object such as name)  

					foreach (Object myCollection in fields[ldapField])
						if (listLdapField.Contains(ldapField)){
							ldapUserData.Add(ldapField, myCollection.ToString());
							
						}
					//
					//;
					//Console.WriteLine(String.Format("{0,-20} : {1}",ldapField, myCollection.ToString()));
				}
				ldapUserData.Add("login", true.ToString());
				ldapUserData.Add("access_token", role);
			}

			else
			{
				// user does not exist  
				Console.WriteLine("User not found!");
			}
			
			return ldapUserData;
			


		}
		[System.Web.Http.HttpPost]
		public string TestApi()
        {
			string test = "test";
			return  test ;
        }
		[System.Web.Http.HttpPost]
		public IHttpActionResult GetUserData(UserSearchData searchUser)
		{
			//string searchUser = "1600218s";
			Dictionary<string,string> userData = GetLdapUserData(searchUser.searchName);
			//return userData;

			return Ok(userData);


		}
		public class UserAuthData
		{
			public string Username { get; set; }
			public string Password { get; set; }
		}
		public class UserData
        {
			public string Name { get; set; }
			public string Displayname { get; set; }
			public string Department { get; set; }
			public string Title { get; set; }
			public string Mail { get; set; }
        }
		public class UserSearchData
        {
			public string searchName { get; set; }
        }
	}

}
