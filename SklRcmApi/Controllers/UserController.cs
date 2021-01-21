﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace ContractAPI.Controllers
{
	public class UserController : ApiController
	{
		string consString = System.Configuration.ConfigurationManager.AppSettings.Get("RCMDbConnStr");
		//static string ldapserver = System.Configuration.ConfigurationManager.AppSettings.Get("LdapServer"); //= "localhost";
		//static string port = System.Configuration.ConfigurationManager.AppSettings.Get("LdapServerPort"); // = "10389";
		static string path = System.Configuration.ConfigurationManager.AppSettings.Get("LdapSearchPath");
		static string username = System.Configuration.ConfigurationManager.AppSettings.Get("LdapServerUserName");
		static string password = System.Configuration.ConfigurationManager.AppSettings.Get("LdapServerUserPassword"); 
		static string[] listLdapField = { "displayname", "title", "department", "name", "mail", "manager", "samaccountname", "physicaldeliveryofficename" };
		//init a directory entry
		DirectoryEntry dEntry = new DirectoryEntry(path, username, password);

		public Dictionary<string, dynamic> Login(UserAuthData userdata)
		{
			string username = userdata.Username;
			string password = userdata.Password;
			Dictionary<string, dynamic> userData = new Dictionary<string, dynamic>();

			if (ValidateLDAPUser(username, password))
			{

				userData = GetLdapUserData(username);
				Dictionary<string, dynamic> userRole = GetUserDb(username);

				if (userRole != null && userRole.ContainsKey("user_status") && userRole["user_status"] == true)
				{
					foreach (var role in userRole)
					{
						userData.Add(role.Key, role.Value);
					}

				}
				else
				{
					userData.Clear();
				}


            }
            else
            {
				userData.Add("error", "ldap error");
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
					var ldapUserId = username + "@systex.demo";
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
		public Dictionary<string, dynamic> GetLdapUserData(string searchUser)
		{


			DirectorySearcher dSearcher = new DirectorySearcher(dEntry);

			Dictionary<string, dynamic> ldapUserData = new Dictionary<string, dynamic>();

			dSearcher.Filter = "(|(cn=" + searchUser + ")(samaccountname=" + searchUser + ")(sn=" + searchUser + "))";
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
						if (listLdapField.Contains(ldapField))
						{
							if (ldapField == "manager")
							{

								string strManager = myCollection.ToString();
								string pattern = "^CN=(.*),OU";
								Match m = Regex.Match(strManager, pattern, RegexOptions.IgnoreCase);
								if (m.Success)
								{
									string manager = m.Groups[1].Value;
									ldapUserData.Add(ldapField, manager);

									Debug.WriteLine(manager);
									Debug.WriteLine("Found '{0}' at position {1}.", m.Value, m.Index);
								}
								else
								{
									ldapUserData.Add(ldapField, "");

								}

                                //var pattern = Regex.Match(strManager, );
                                //Debug.WriteLine(pattern);
                            }
                            else
							ldapUserData.Add(ldapField, myCollection.ToString());

						}
					//
					//;
					//Console.WriteLine(String.Format("{0,-20} : {1}",ldapField, myCollection.ToString()));
				}
				ldapUserData.Add("login", true);
				ldapUserData.Add("access_token", "access_token");
				//ldapUserData.Add("role", role);
			}

			else
			{
				// user does not exist  
				Console.WriteLine("User not found!");
			}

			return ldapUserData;



		}
		private Dictionary<string, dynamic> GetUserDb(string name)
		{


			SqlConnection conn = new SqlConnection(this.consString);

			//SqlConnection conn = new SqlConnection("data source=.\\SQLExpress; initial catalog = FUBON_DLP; user id = fubon_dlp; password = 1234");
			conn.Open();
			Dictionary<string, dynamic> UserRole = new Dictionary<string, dynamic>();
			if ((conn.State & ConnectionState.Open) > 0)
			{
				string sSqlCmdUser = $"select * from users where user_id='{name}'";
				Debug.WriteLine(sSqlCmdUser);
				//string sSqlCmdUser = "select * from user";  
				//Console.WriteLine(sSqlCmdUser);
				SqlCommand cmd = new SqlCommand(sSqlCmdUser, conn);
				SqlDataReader dr = cmd.ExecuteReader();

				if (dr.HasRows)
				{
					while (dr.Read())
					{
						UserRole.Add("user_role", dr[1]);
						UserRole.Add("user_status", dr[2]);
					}

				}
			}

			conn.Close();

			return UserRole;
		}

		public List<Dictionary<string, dynamic>> SearchLdapUserData(string searchUser)
		{


			//dEntry.Path= path;
			DirectorySearcher dSearcher = new DirectorySearcher(dEntry);
			List<Dictionary<string, dynamic>> ldapUserDataCollection = new List<Dictionary<string, dynamic>>();
			//Dictionary<int, Dictionary<string, string>> ldapUserData = new Dictionary<int, Dictionary<string, string>>();

			dSearcher.Filter = "(|(cn=*" + searchUser + "*)(samaccountname=*" + searchUser + "*)(displayname=*" + searchUser + "*)(sn=*" + searchUser + "*))";
			SearchResultCollection results = dSearcher.FindAll();
			if (results != null)
			{
				// user exists, cycle through LDAP fields (cn, telephonenumber etc.)  
				foreach (SearchResult result in results)
				{
					ResultPropertyCollection fields = result.Properties;
					Dictionary<string, dynamic> ldapUserData = new Dictionary<string, dynamic>();
					string searchName = "";
					foreach (String ldapField in fields.PropertyNames)
					{
						// cycle through objects in each field e.g. group membership  
						// (for many fields there will only be one object such as name)  
						
						foreach (Object myCollection in fields[ldapField])
                        {
							Debug.WriteLine(ldapField+","+myCollection.ToString());
							if (listLdapField.Contains(ldapField))
							{

								if (ldapField == "manager")
								{

									string strManager = myCollection.ToString();
									string pattern = "^CN=(.*),OU";
									Match m = Regex.Match(strManager, pattern, RegexOptions.IgnoreCase);
									if (m.Success)
									{
										string manager = m.Groups[1].Value;
										ldapUserData.Add(ldapField, manager);

										Debug.WriteLine(manager);
										Debug.WriteLine("Found '{0}' at position {1}.", m.Value, m.Index);
									}
									else
									{
										ldapUserData.Add(ldapField, "");

									}

									//var pattern = Regex.Match(strManager, );
									//Debug.WriteLine(pattern);
								}
								else
								{
									ldapUserData.Add(ldapField, myCollection.ToString());
								}

								if (ldapField == "samaccountname")
								{
									searchName = myCollection.ToString();
								}

							}
						}
							



					}
					Dictionary<string, dynamic> userRole = GetUserDb(searchName);

					if (userRole != null)
					{
						foreach (var roleData in userRole)
						{
							ldapUserData.Add(roleData.Key, roleData.Value);
						}


					}

					ldapUserDataCollection.Add(ldapUserData);


				}


			}

			else
			{
				// user does not exist  
				Console.WriteLine("User not found!");
			}

			return ldapUserDataCollection;



		}
		[System.Web.Http.HttpPost]
		public string TestApi()
		{
			string test = "test";
			return test;
		}
		[System.Web.Http.HttpPost]
		public IHttpActionResult GetUserData(UserSearchData searchUser)
		{
			//string searchUser = "1600218s";
			Dictionary<string, dynamic> userData = GetLdapUserData(searchUser.searchName);
			//return userData;

			return Ok(userData);


		}
		[System.Web.Http.HttpPost]
		public IHttpActionResult SearchUserData(UserSearchData searchUser)
		{
			//string searchUser = "1600218s";
			List<Dictionary<string, dynamic>> userData = SearchLdapUserData(searchUser.searchName);
			//return userData;

			return Ok(userData);


		}

		[System.Web.Http.HttpPost]
		public IHttpActionResult AddUser(UserData user)
		{
			SqlConnection conn = new SqlConnection(this.consString);
			conn.Open();
			if ((conn.State & ConnectionState.Open) > 0)
			{
				string sSqlInsert = $"INSERT INTO USERS (user_id,user_role,user_status) values('{user.user_id}','{user.user_role}','{1}')";
				Debug.WriteLine(sSqlInsert);
				//string sSqlCmdUser = "select * from user";  
				//Console.WriteLine(sSqlCmdUser);
				SqlCommand sqlInsert = new SqlCommand(sSqlInsert, conn);

				int numberOfRecords = sqlInsert.ExecuteNonQuery();
				if (numberOfRecords > 0)
				{
					return Ok(true);
				}
			}

			conn.Close();

			return Ok(false);


		}
		[System.Web.Http.HttpPost]
		public IHttpActionResult UpdateUser(UserData user)
		{
			SqlConnection conn = new SqlConnection(this.consString);
			conn.Open();
			if ((conn.State & ConnectionState.Open) > 0)
			{
				string sSqlUpdate = $"UPDATE USERS set user_role='{user.user_role}' , user_status='{user.user_status}' where user_id='{user.user_id}' ";
				Debug.WriteLine(sSqlUpdate);
				//string sSqlCmdUser = "select * from user";  
				//Console.WriteLine(sSqlCmdUser);
				SqlCommand sqlInsert = new SqlCommand(sSqlUpdate, conn);

				int numberOfRecords = sqlInsert.ExecuteNonQuery();
				if (numberOfRecords > 0)
				{
					return Ok(true);
				}
			}

			conn.Close();

			return Ok(false);


		}
		[System.Web.Http.HttpGet]
		public IHttpActionResult ListUser()
		{
			SqlConnection conn = new SqlConnection(this.consString);



			//SqlConnection conn = new SqlConnection("data source=.\\SQLExpress; initial catalog = FUBON_DLP; user id = fubon_dlp; password = 1234");
			conn.Open();
			if ((conn.State & ConnectionState.Open) > 0)
			{
				string sSQLCmdList = $"SELECT * FROM USERS; ";
				Debug.WriteLine(sSQLCmdList);
				//string sSqlCmdUser = "select * from user";  
				//Console.WriteLine(sSqlCmdUser);
				SqlCommand sqlInsert = new SqlCommand(sSQLCmdList, conn);
				SqlCommand cmd = new SqlCommand(sSQLCmdList, conn);
				SqlDataReader dr = cmd.ExecuteReader();
				List<Dictionary<string, dynamic>> listUsers = new List<Dictionary<string, dynamic>>();
				//Dictionary<string, string> ldapUserData = new Dictionary<string, string>();
				if (dr.HasRows)
				{

					while (dr.Read())
					{
						Dictionary<string, dynamic> userData = new Dictionary<string, dynamic>();
						userData.Add("user_id", dr[0].ToString());
						Dictionary<string, dynamic> ldapUserData = GetLdapUserData(dr[0].ToString());
						if (ldapUserData != null)
						{
							foreach (var item in ldapUserData)
							{
								userData.Add(item.Key, item.Value);
							}
						}
						listUsers.Add(userData);
						userData.Add("user_role", dr[1]);
						userData.Add("user_status", dr[2]);
					}


				}

				return Ok(listUsers);

			}

			conn.Close();

			return Ok(false);


		}
		public class UserAuthData
		{
			public string Username { get; set; }
			public string Password { get; set; }
		}
		public class UserData
		{
			public string user_id { get; set; }
			public string user_role { get; set; }
			public string user_status { get; set; }
		}
		public class UserSearchData
		{
			public string searchName { get; set; }
		}
	}
}
