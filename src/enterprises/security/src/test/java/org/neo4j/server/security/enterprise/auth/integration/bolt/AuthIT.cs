using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Server.security.enterprise.auth.integration.bolt
{
	using CreateLdapServer = org.apache.directory.server.annotations.CreateLdapServer;
	using CreateTransport = org.apache.directory.server.annotations.CreateTransport;
	using SaslMechanism = org.apache.directory.server.annotations.SaslMechanism;
	using ApplyLdifFiles = org.apache.directory.server.core.annotations.ApplyLdifFiles;
	using ContextEntry = org.apache.directory.server.core.annotations.ContextEntry;
	using CreateDS = org.apache.directory.server.core.annotations.CreateDS;
	using CreatePartition = org.apache.directory.server.core.annotations.CreatePartition;
	using LoadSchema = org.apache.directory.server.core.annotations.LoadSchema;
	using CreateLdapServerRule = org.apache.directory.server.core.integ.CreateLdapServerRule;
	using StartTlsHandler = org.apache.directory.server.ldap.handlers.extended.StartTlsHandler;
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Neo4Net.Graphdb.config;
	using PredefinedRoles = Neo4Net.Server.security.enterprise.auth.plugin.api.PredefinedRoles;
	using SecuritySettings = Neo4Net.Server.security.enterprise.configuration.SecuritySettings;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") @RunWith(Parameterized.class) @CreateDS(name = "Test", partitions = {@CreatePartition(name = "example", suffix = "dc=example,dc=com", contextEntry = @ContextEntry(entryLdif = "dn: dc=example,dc=com\n" + "dc: example\n" + "o: example\n" + "objectClass: top\n" + "objectClass: dcObject\n" + "objectClass: organization\n\n"))}, loadedSchemas = { @LoadSchema(name = "nis")}) @CreateLdapServer(transports = {@CreateTransport(protocol = "LDAP", port = 10389, address = "0.0.0.0"), @CreateTransport(protocol = "LDAPS", port = 10636, address = "0.0.0.0", ssl = true) }, saslMechanisms = { @SaslMechanism(name = "DIGEST-MD5", implClass = org.apache.directory.server.ldap.handlers.sasl.digestMD5.DigestMd5MechanismHandler.class), @SaslMechanism(name = "CRAM-MD5", implClass = org.apache.directory.server.ldap.handlers.sasl.cramMD5.CramMd5MechanismHandler.class) }, saslHost = "0.0.0.0", extendedOpHandlers = {StartTlsHandler.class}, keyStore = "target/test-classes/neo4j_ldap_test_keystore.jks", certificatePassword = "secret") @ApplyLdifFiles("ldap_test_data.ldif") public class AuthIT extends AuthTestBase
	public class AuthIT : AuthTestBase
	{
		 private static EmbeddedTestCertificates _embeddedTestCertificates;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.apache.directory.server.core.integ.CreateLdapServerRule ldapServerRule = new org.apache.directory.server.core.integ.CreateLdapServerRule();
		 public static CreateLdapServerRule LdapServerRule = new CreateLdapServerRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<Object[]> configurations()
		 public static ICollection<object[]> Configurations()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { "Ldap", "abc123", false, Arrays.asList( SecuritySettings.auth_provider, SecuritySettings.LDAP_REALM_NAME, SecuritySettings.ldap_server, "ldap://0.0.0.0:10389", SecuritySettings.ldap_use_starttls, "false", SecuritySettings.native_authentication_enabled, "false", SecuritySettings.native_authorization_enabled, "false", SecuritySettings.ldap_authentication_enabled, "true", SecuritySettings.ldap_authorization_enabled, "true", SecuritySettings.ldap_authorization_use_system_account, "false" ) },
				  new object[] { "Ldaps", "abc123", true, Arrays.asList( SecuritySettings.auth_provider, SecuritySettings.LDAP_REALM_NAME, SecuritySettings.ldap_server, "ldaps://localhost:10636", SecuritySettings.ldap_use_starttls, "false", SecuritySettings.native_authentication_enabled, "false", SecuritySettings.native_authorization_enabled, "false", SecuritySettings.ldap_authentication_enabled, "true", SecuritySettings.ldap_authorization_enabled, "true", SecuritySettings.ldap_authorization_use_system_account, "false" ) },
				  new object[] { "StartTLS", "abc123", true, Arrays.asList( SecuritySettings.auth_provider, SecuritySettings.LDAP_REALM_NAME, SecuritySettings.ldap_server, "ldap://localhost:10389", SecuritySettings.ldap_use_starttls, "true", SecuritySettings.native_authentication_enabled, "false", SecuritySettings.native_authorization_enabled, "false", SecuritySettings.ldap_authentication_enabled, "true", SecuritySettings.ldap_authorization_enabled, "true", SecuritySettings.ldap_authorization_use_system_account, "false" ) },
				  new object[] { "LdapSystemAccount", "abc123", false, Arrays.asList( SecuritySettings.auth_provider, SecuritySettings.LDAP_REALM_NAME, SecuritySettings.ldap_server, "ldap://0.0.0.0:10389", SecuritySettings.ldap_use_starttls, "false", SecuritySettings.native_authentication_enabled, "false", SecuritySettings.native_authorization_enabled, "false", SecuritySettings.ldap_authentication_enabled, "true", SecuritySettings.ldap_authorization_enabled, "true", SecuritySettings.ldap_authorization_use_system_account, "true", SecuritySettings.ldap_authorization_system_password, "secret", SecuritySettings.ldap_authorization_system_username, "uid=admin,ou=system" ) },
				  new object[] { "Ldaps SystemAccount", "abc123", true, Arrays.asList( SecuritySettings.auth_provider, SecuritySettings.LDAP_REALM_NAME, SecuritySettings.ldap_server, "ldaps://localhost:10636", SecuritySettings.ldap_use_starttls, "false", SecuritySettings.native_authentication_enabled, "false", SecuritySettings.native_authorization_enabled, "false", SecuritySettings.ldap_authentication_enabled, "true", SecuritySettings.ldap_authorization_enabled, "true", SecuritySettings.ldap_authorization_use_system_account, "true", SecuritySettings.ldap_authorization_system_password, "secret", SecuritySettings.ldap_authorization_system_username, "uid=admin,ou=system" ) },
				  new object[] { "StartTLS SystemAccount", "abc123", true, Arrays.asList( SecuritySettings.auth_provider, SecuritySettings.LDAP_REALM_NAME, SecuritySettings.ldap_server, "ldap://localhost:10389", SecuritySettings.ldap_use_starttls, "true", SecuritySettings.native_authentication_enabled, "false", SecuritySettings.native_authorization_enabled, "false", SecuritySettings.ldap_authentication_enabled, "true", SecuritySettings.ldap_authorization_enabled, "true", SecuritySettings.ldap_authorization_use_system_account, "true", SecuritySettings.ldap_authorization_system_password, "secret", SecuritySettings.ldap_authorization_system_username, "uid=admin,ou=system" ) },
				  new object[] { "Ldap authn cache disabled", "abc123", false, Arrays.asList( SecuritySettings.auth_provider, SecuritySettings.LDAP_REALM_NAME, SecuritySettings.ldap_server, "ldap://0.0.0.0:10389", SecuritySettings.ldap_use_starttls, "false", SecuritySettings.native_authentication_enabled, "false", SecuritySettings.native_authorization_enabled, "false", SecuritySettings.ldap_authentication_enabled, "true", SecuritySettings.ldap_authorization_enabled, "true", SecuritySettings.ldap_authorization_use_system_account, "false", SecuritySettings.ldap_authentication_cache_enabled, "false" ) },
				  new object[] { "Ldap Digest MD5", "{MD5}6ZoYxCjLONXyYIU2eJIuAw==", false, Arrays.asList( SecuritySettings.auth_provider, SecuritySettings.LDAP_REALM_NAME, SecuritySettings.ldap_server, "ldap://0.0.0.0:10389", SecuritySettings.ldap_use_starttls, "false", SecuritySettings.native_authentication_enabled, "false", SecuritySettings.native_authorization_enabled, "false", SecuritySettings.ldap_authentication_enabled, "true", SecuritySettings.ldap_authorization_enabled, "true", SecuritySettings.ldap_authorization_use_system_account, "false", SecuritySettings.ldap_authentication_mechanism, "DIGEST-MD5", SecuritySettings.ldap_authentication_user_dn_template, "{0}" ) },
				  new object[] { "Ldap Cram MD5", "{MD5}6ZoYxCjLONXyYIU2eJIuAw==", false, Arrays.asList( SecuritySettings.auth_provider, SecuritySettings.LDAP_REALM_NAME, SecuritySettings.ldap_server, "ldap://0.0.0.0:10389", SecuritySettings.ldap_use_starttls, "false", SecuritySettings.native_authentication_enabled, "false", SecuritySettings.native_authorization_enabled, "false", SecuritySettings.ldap_authentication_enabled, "true", SecuritySettings.ldap_authorization_enabled, "true", SecuritySettings.ldap_authorization_use_system_account, "false", SecuritySettings.ldap_authentication_mechanism, "CRAM-MD5", SecuritySettings.ldap_authentication_user_dn_template, "{0}" ) },
				  new object[] { "Ldap authn Native authz", "abc123", false, Arrays.asList( SecuritySettings.auth_providers, SecuritySettings.LDAP_REALM_NAME + ", " + SecuritySettings.NATIVE_REALM_NAME, SecuritySettings.ldap_server, "ldap://0.0.0.0:10389", SecuritySettings.ldap_use_starttls, "false", SecuritySettings.native_authentication_enabled, "false", SecuritySettings.native_authorization_enabled, "true", SecuritySettings.ldap_authentication_enabled, "true", SecuritySettings.ldap_authorization_enabled, "false", SecuritySettings.ldap_authorization_use_system_account, "false" ) },
				  new object[] { "Ldap authz Native authn", "abc123", false, Arrays.asList( SecuritySettings.auth_providers, SecuritySettings.LDAP_REALM_NAME + ", " + SecuritySettings.NATIVE_REALM_NAME, SecuritySettings.ldap_server, "ldap://0.0.0.0:10389", SecuritySettings.ldap_use_starttls, "false", SecuritySettings.native_authentication_enabled, "true", SecuritySettings.native_authorization_enabled, "false", SecuritySettings.ldap_authentication_enabled, "false", SecuritySettings.ldap_authorization_enabled, "true", SecuritySettings.ldap_authorization_use_system_account, "true", SecuritySettings.ldap_authorization_system_password, "secret", SecuritySettings.ldap_authorization_system_username, "uid=admin,ou=system" ) },
				  new object[] { "Ldap with Native authn", "abc123", false, Arrays.asList( SecuritySettings.auth_providers, SecuritySettings.LDAP_REALM_NAME + ", " + SecuritySettings.NATIVE_REALM_NAME, SecuritySettings.ldap_server, "ldap://0.0.0.0:10389", SecuritySettings.ldap_use_starttls, "false", SecuritySettings.native_authentication_enabled, "true", SecuritySettings.native_authorization_enabled, "false", SecuritySettings.ldap_authentication_enabled, "true", SecuritySettings.ldap_authorization_enabled, "true", SecuritySettings.ldap_authorization_use_system_account, "false" ) },
				  new object[] { "Ldap with Native authz", "abc123", false, Arrays.asList( SecuritySettings.auth_providers, SecuritySettings.LDAP_REALM_NAME + ", " + SecuritySettings.NATIVE_REALM_NAME, SecuritySettings.ldap_server, "ldap://0.0.0.0:10389", SecuritySettings.ldap_use_starttls, "false", SecuritySettings.native_authentication_enabled, "false", SecuritySettings.native_authorization_enabled, "true", SecuritySettings.ldap_authentication_enabled, "true", SecuritySettings.ldap_authorization_enabled, "true", SecuritySettings.ldap_authorization_use_system_account, "false" ) },
				  new object[] { "Ldap and Native", "abc123", false, Arrays.asList( SecuritySettings.auth_providers, SecuritySettings.LDAP_REALM_NAME + ", " + SecuritySettings.NATIVE_REALM_NAME, SecuritySettings.ldap_server, "ldap://0.0.0.0:10389", SecuritySettings.ldap_use_starttls, "false", SecuritySettings.native_authentication_enabled, "true", SecuritySettings.native_authorization_enabled, "true", SecuritySettings.ldap_authentication_enabled, "true", SecuritySettings.ldap_authorization_enabled, "true", SecuritySettings.ldap_authorization_use_system_account, "false" ) },
				  new object[] { "Native with unresponsive ldap", "abc123", false, Arrays.asList( SecuritySettings.auth_providers, SecuritySettings.LDAP_REALM_NAME + ", " + SecuritySettings.NATIVE_REALM_NAME, SecuritySettings.ldap_server, "ldap://127.0.0.1:10389", SecuritySettings.ldap_use_starttls, "false", SecuritySettings.native_authentication_enabled, "true", SecuritySettings.native_authorization_enabled, "true", SecuritySettings.ldap_authentication_enabled, "true", SecuritySettings.ldap_authorization_enabled, "true", SecuritySettings.ldap_authorization_use_system_account, "false" ) },
				  new object[] { "Native", "abc123", false, Arrays.asList( SecuritySettings.auth_provider, SecuritySettings.NATIVE_REALM_NAME, SecuritySettings.native_authentication_enabled, "true", SecuritySettings.native_authorization_enabled, "true" ) }
			  });
		 }

		 private readonly string _password;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<org.neo4j.graphdb.config.Setting<?>,String> configMap;
		 private readonly IDictionary<Setting<object>, string> _configMap;
		 private readonly bool _confidentialityRequired;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public AuthIT(String suiteName, String password, boolean confidentialityRequired, java.util.List<Object> settings)
		 public AuthIT( string suiteName, string password, bool confidentialityRequired, IList<object> settings )
		 {
			  this._password = password;
			  this._confidentialityRequired = confidentialityRequired;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: this.configMap = new java.util.HashMap<>();
			  this._configMap = new Dictionary<Setting<object>, string>();
			  for ( int i = 0; i < settings.Count - 1; i += 2 )
			  {
					Setting setting = ( Setting ) settings[i];
					string value = ( string ) settings[i + 1];
					_configMap[setting] = value;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void classSetup()
		 public static void ClassSetup()
		 {
			  _embeddedTestCertificates = new EmbeddedTestCertificates();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before @Override public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public override void Setup()
		 {
			  base.Setup();
			  LdapServerRule.LdapServer.ConfidentialityRequired = _confidentialityRequired;

			  EnterpriseAuthAndUserManager authManager = DbRule.resolveDependency( typeof( EnterpriseAuthAndUserManager ) );
			  EnterpriseUserManager userManager = authManager.UserManager;
			  if ( userManager != null )
			  {
					userManager.NewUser( NONE_USER, _password.GetBytes(), false );
					userManager.NewUser( PROC_USER, _password.GetBytes(), false );
					userManager.NewUser( READ_USER, _password.GetBytes(), false );
					userManager.NewUser( WRITE_USER, _password.GetBytes(), false );
					userManager.AddRoleToUser( PredefinedRoles.READER, READ_USER );
					userManager.AddRoleToUser( PredefinedRoles.PUBLISHER, WRITE_USER );
					userManager.NewRole( "role1", PROC_USER );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected java.util.Map<org.neo4j.graphdb.config.Setting<?>, String> getSettings()
		 protected internal override IDictionary<Setting<object>, string> Settings
		 {
			 get
			 {
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: java.util.Map<org.neo4j.graphdb.config.Setting<?>, String> settings = new java.util.HashMap<>();
				  IDictionary<Setting<object>, string> settings = new Dictionary<Setting<object>, string>();
				  settings[SecuritySettings.ldap_authentication_user_dn_template] = "cn={0},ou=users,dc=example,dc=com";
				  settings[SecuritySettings.ldap_authentication_cache_enabled] = "true";
				  settings[SecuritySettings.ldap_authorization_user_search_base] = "dc=example,dc=com";
				  settings[SecuritySettings.ldap_authorization_user_search_filter] = "(&(objectClass=*)(uid={0}))";
				  settings[SecuritySettings.ldap_authorization_group_membership_attribute_names] = "gidnumber";
				  settings[SecuritySettings.ldap_authorization_group_to_role_mapping] = "500=reader;501=publisher;502=architect;503=admin;505=role1";
				  settings[SecuritySettings.procedure_roles] = "test.staticReadProcedure:role1";
				  settings[SecuritySettings.ldap_read_timeout] = "1s";
	//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
				  settings.putAll( _configMap );
				  return settings;
			 }
		 }

		 protected internal override string Password
		 {
			 get
			 {
				  return _password;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void classTeardown()
		 public static void ClassTeardown()
		 {
			  if ( _embeddedTestCertificates != null )
			  {
					_embeddedTestCertificates.close();
			  }
		 }
	}

}