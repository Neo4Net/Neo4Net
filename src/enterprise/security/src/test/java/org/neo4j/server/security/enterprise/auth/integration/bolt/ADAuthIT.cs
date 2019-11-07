using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
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
	using FrameworkRunner = org.apache.directory.server.core.integ.FrameworkRunner;
	using StartTlsHandler = org.apache.directory.server.ldap.handlers.extended.StartTlsHandler;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;


	using Driver = Neo4Net.driver.v1.Driver;
	using Neo4Net.GraphDb.config;
	using SecuritySettings = Neo4Net.Server.security.enterprise.configuration.SecuritySettings;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(FrameworkRunner.class) @CreateDS(name = "Test", partitions = { @CreatePartition(name = "example", suffix = "dc=example,dc=com", contextEntry = @ContextEntry(entryLdif = "dn: dc=example,dc=com\n" + "dc: example\n" + "o: example\n" + "objectClass: top\n" + "objectClass: dcObject\n" + "objectClass: organization\n\n"))}, loadedSchemas = { @LoadSchema(name = "nis")}) @CreateLdapServer(transports = { @CreateTransport(protocol = "LDAP", port = 10389, address = "0.0.0.0"), @CreateTransport(protocol = "LDAPS", port = 10636, address = "0.0.0.0", ssl = true) }, saslMechanisms = { @SaslMechanism(name = "DIGEST-MD5", implClass = org.apache.directory.server.ldap.handlers.sasl.digestMD5.DigestMd5MechanismHandler.class), @SaslMechanism(name = "CRAM-MD5", implClass = org.apache.directory.server.ldap.handlers.sasl.cramMD5.CramMd5MechanismHandler.class) }, saslHost = "0.0.0.0", extendedOpHandlers = { StartTlsHandler.class }, keyStore = "target/test-classes/Neo4Net_ldap_test_keystore.jks", certificatePassword = "secret") @ApplyLdifFiles({"ad_schema.ldif", "ad_test_data.ldif"}) public class ADAuthIT extends EnterpriseAuthenticationTestBase
	public class ADAuthIT : EnterpriseAuthenticationTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before @Override public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public override void Setup()
		 {
			  base.Setup();
			  LdapServer.ConfidentialityRequired = false;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") @Override protected java.util.Map<Neo4Net.graphdb.config.Setting<?>, String> getSettings()
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 protected internal override IDictionary<Setting<object>, string> Settings
		 {
			 get
			 {
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: java.util.Map<Neo4Net.graphdb.config.Setting<?>,String> settings = new java.util.HashMap<>();
				  IDictionary<Setting<object>, string> settings = new Dictionary<Setting<object>, string>();
				  settings[SecuritySettings.auth_provider] = SecuritySettings.LDAP_REALM_NAME;
				  settings[SecuritySettings.native_authentication_enabled] = "false";
				  settings[SecuritySettings.native_authorization_enabled] = "false";
				  settings[SecuritySettings.ldap_authentication_enabled] = "true";
				  settings[SecuritySettings.ldap_authorization_enabled] = "true";
				  settings[SecuritySettings.ldap_server] = "0.0.0.0:10389";
				  settings[SecuritySettings.ldap_authentication_user_dn_template] = "cn={0},ou=local,ou=users,dc=example,dc=com";
				  settings[SecuritySettings.ldap_authentication_cache_enabled] = "true";
				  settings[SecuritySettings.ldap_authorization_system_username] = "uid=admin,ou=system";
				  settings[SecuritySettings.ldap_authorization_system_password] = "secret";
				  settings[SecuritySettings.ldap_authorization_use_system_account] = "true";
				  settings[SecuritySettings.ldap_authorization_user_search_base] = "dc=example,dc=com";
				  settings[SecuritySettings.ldap_authorization_user_search_filter] = "(&(objectClass=*)(samaccountname={0}))";
				  settings[SecuritySettings.ldap_authorization_group_membership_attribute_names] = "memberOf";
				  settings[SecuritySettings.ldap_authorization_group_to_role_mapping] = "cn=reader,ou=groups,dc=example,dc=com=reader;" + "cn=publisher,ou=groups,dc=example,dc=com=publisher;" + "cn=architect,ou=groups,dc=example,dc=com=architect;" + "cn=admin,ou=groups,dc=example,dc=com=admin";
				  settings[SecuritySettings.procedure_roles] = "test.allowedReadProcedure:role1";
				  settings[SecuritySettings.ldap_read_timeout] = "1s";
				  settings[SecuritySettings.ldap_authentication_use_samaccountname] = "true";
				  return settings;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoginWithSamAccountName()
		 public virtual void ShouldLoginWithSamAccountName()
		 {
			  // dn: cn=n.Neo4Net,ou=local,ou=users,dc=example,dc=com
			  AssertAuth( "Neo4Net", "abc123" );
			  AssertAuth( "Neo4Net", "abc123" );
			  // dn: cn=n.neo,ou=remote,ou=users,dc=example,dc=com
			  AssertAuth( "neo", "abc123" );
			  AssertAuth( "neo", "abc123" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailLoginSamAccountNameWrongPassword()
		 public virtual void ShouldFailLoginSamAccountNameWrongPassword()
		 {
			  AssertAuthFail( "Neo4Net", "wrong" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailLoginSamAccountNameWithDN()
		 public virtual void ShouldFailLoginSamAccountNameWithDN()
		 {
			  AssertAuthFail( "n.Neo4Net", "abc123" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadWithSamAccountName()
		 public virtual void ShouldReadWithSamAccountName()
		 {
			  using ( Driver driver = ConnectDriver( "Neo4Net", "abc123" ) )
			  {
					AssertReadSucceeds( driver );
			  }
		 }
	}

}