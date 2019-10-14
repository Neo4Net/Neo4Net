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
namespace Neo4Net.Server.security.enterprise.auth.plugin
{
	using Test = org.junit.Test;

	using Config = Neo4Net.Kernel.configuration.Config;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using AuthProviderOperations = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthProviderOperations;
	using SecurityLog = Neo4Net.Server.security.enterprise.log.SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class PluginRealmTest
	{
		private bool InstanceFieldsInitialized = false;

		public PluginRealmTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_securityLog = new SecurityLog( _log.getLog( this.GetType() ) );
		}

		 private Config _config = mock( typeof( Config ) );
		 private AssertableLogProvider _log = new AssertableLogProvider();
		 private SecurityLog _securityLog;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogToSecurityLogFromAuthPlugin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogToSecurityLogFromAuthPlugin()
		 {
			  PluginRealm pluginRealm = new PluginRealm( new LoggingAuthPlugin( this ), _config, _securityLog, Clock.systemUTC(), mock(typeof(SecureHasher)) );
			  pluginRealm.Initialize();
			  AssertLogged( "LoggingAuthPlugin" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogToSecurityLogFromAuthenticationPlugin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogToSecurityLogFromAuthenticationPlugin()
		 {
			  PluginRealm pluginRealm = new PluginRealm( new LoggingAuthenticationPlugin( this ), null, _config, _securityLog, Clock.systemUTC(), mock(typeof(SecureHasher)) );
			  pluginRealm.Initialize();
			  AssertLogged( "LoggingAuthenticationPlugin" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogToSecurityLogFromAuthorizationPlugin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogToSecurityLogFromAuthorizationPlugin()
		 {
			  PluginRealm pluginRealm = new PluginRealm( null, new LoggingAuthorizationPlugin( this ), _config, _securityLog, Clock.systemUTC(), mock(typeof(SecureHasher)) );
			  pluginRealm.Initialize();
			  AssertLogged( "LoggingAuthorizationPlugin" );
		 }

		 private void AssertLogged( string name )
		 {
			  _log.assertExactly( inLog( this.GetType() ).info(format("{plugin-%s} info line", name)), inLog(this.GetType()).warn(format("{plugin-%s} warn line", name)), inLog(this.GetType()).error(format("{plugin-%s} error line", name)) );
		 }

		 private class LoggingAuthPlugin : TestAuthPlugin
		 {
			 private readonly PluginRealmTest _outerInstance;

			 public LoggingAuthPlugin( PluginRealmTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Initialize( AuthProviderOperations api )
			  {
					LogLines( api );
			  }
		 }

		 private class LoggingAuthenticationPlugin : TestAuthenticationPlugin
		 {
			 private readonly PluginRealmTest _outerInstance;

			 public LoggingAuthenticationPlugin( PluginRealmTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Initialize( AuthProviderOperations api )
			  {
					LogLines( api );
			  }
		 }

		 private class LoggingAuthorizationPlugin : TestAuthorizationPlugin
		 {
			 private readonly PluginRealmTest _outerInstance;

			 public LoggingAuthorizationPlugin( PluginRealmTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Initialize( AuthProviderOperations api )
			  {
					LogLines( api );
			  }
		 }

		 private static void LogLines( AuthProviderOperations api )
		 {
			  Neo4Net.Server.security.enterprise.auth.plugin.api.AuthProviderOperations_Log log = api.Log();
			  if ( log.DebugEnabled )
			  {
					log.Debug( "debug line" );
			  }
			  log.Info( "info line" );
			  log.Warn( "warn line" );
			  log.Error( "error line" );
		 }
	}

}