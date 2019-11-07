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
	using LdapGroupHasUsersAuthPlugin = Neo4Net.Server.security.enterprise.auth.plugin.LdapGroupHasUsersAuthPlugin;
	using PredefinedRoles = Neo4Net.Server.security.enterprise.auth.plugin.api.PredefinedRoles;
	using SecuritySettings = Neo4Net.Server.security.enterprise.configuration.SecuritySettings;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(FrameworkRunner.class) @CreateDS(name = "Test", partitions = { @CreatePartition(name = "example", suffix = "dc=example,dc=com", contextEntry = @ContextEntry(entryLdif = "dn: dc=example,dc=com\n" + "dc: example\n" + "o: example\n" + "objectClass: top\n" + "objectClass: dcObject\n" + "objectClass: organization\n\n"))}, loadedSchemas = { @LoadSchema(name = "nis", enabled = true)}) @CreateLdapServer(transports = { @CreateTransport(protocol = "LDAP", port = 10389, address = "0.0.0.0"), @CreateTransport(protocol = "LDAPS", port = 10636, address = "0.0.0.0", ssl = true) }, saslMechanisms = { @SaslMechanism(name = "DIGEST-MD5", implClass = org.apache.directory.server.ldap.handlers.sasl.digestMD5.DigestMd5MechanismHandler.class), @SaslMechanism(name = "CRAM-MD5", implClass = org.apache.directory.server.ldap.handlers.sasl.cramMD5.CramMd5MechanismHandler.class) }, saslHost = "0.0.0.0", extendedOpHandlers = { StartTlsHandler.class }, keyStore = "target/test-classes/Neo4Net_ldap_test_keystore.jks", certificatePassword = "secret") @ApplyLdifFiles("ldap_group_has_users_test_data.ldif") public class LdapExamplePluginAuthenticationIT extends EnterpriseAuthenticationTestBase
	public class LdapExamplePluginAuthenticationIT : EnterpriseAuthenticationTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before @Override public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public override void Setup()
		 {
			  base.Setup();
			  LdapServer.ConfidentialityRequired = false;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected java.util.Map<Neo4Net.graphdb.config.Setting<?>, String> getSettings()
		 protected internal override IDictionary<Setting<object>, string> Settings
		 {
			 get
			 {
				  return Collections.singletonMap( SecuritySettings.auth_provider, SecuritySettings.PLUGIN_REALM_NAME_PREFIX + ( new LdapGroupHasUsersAuthPlugin() ).name() );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLoginAndAuthorizeWithLdapGroupHasUsersAuthPlugin()
		 public virtual void ShouldBeAbleToLoginAndAuthorizeWithLdapGroupHasUsersAuthPlugin()
		 {
			  using ( Driver driver = ConnectDriver( "neo", "abc123" ) )
			  {
					AssertRoles( driver, PredefinedRoles.READER );
			  }

			  using ( Driver driver = ConnectDriver( "tank", "abc123" ) )
			  {
					AssertRoles( driver, PredefinedRoles.PUBLISHER );
			  }

			  using ( Driver driver = ConnectDriver( "smith", "abc123" ) )
			  {
					AssertRoles( driver );
			  }
		 }
	}

}