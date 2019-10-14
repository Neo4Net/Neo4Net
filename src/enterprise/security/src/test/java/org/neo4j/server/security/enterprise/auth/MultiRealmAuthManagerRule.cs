using System;
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
namespace Neo4Net.Server.security.enterprise.auth
{
	using MemoryConstrainedCacheManager = org.apache.shiro.cache.MemoryConstrainedCacheManager;
	using After = org.junit.After;
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;


	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using FormattedLog = Neo4Net.Logging.FormattedLog;
	using Log = Neo4Net.Logging.Log;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using AuthenticationStrategy = Neo4Net.Server.Security.Auth.AuthenticationStrategy;
	using BasicPasswordPolicy = Neo4Net.Server.Security.Auth.BasicPasswordPolicy;
	using InMemoryUserRepository = Neo4Net.Server.Security.Auth.InMemoryUserRepository;
	using UserRepository = Neo4Net.Server.Security.Auth.UserRepository;
	using SecurityLog = Neo4Net.Server.security.enterprise.log.SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class MultiRealmAuthManagerRule : TestRule
	{
		 private UserRepository _users;
		 private AuthenticationStrategy _authStrategy;
		 private MultiRealmAuthManager _manager;
		 private SecurityLog _securityLog;
		 private StringWriter _securityLogWriter;

		 public MultiRealmAuthManagerRule( UserRepository users, AuthenticationStrategy authStrategy )
		 {
			  this._users = users;
			  this._authStrategy = authStrategy;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setupAuthManager(org.neo4j.server.security.auth.AuthenticationStrategy authStrategy) throws Throwable
		 private void SetupAuthManager( AuthenticationStrategy authStrategy )
		 {
			  FormattedLog.Builder builder = FormattedLog.withUTCTimeZone();
			  _securityLogWriter = new StringWriter();
			  Log log = builder.ToWriter( _securityLogWriter );

			  _securityLog = new SecurityLog( log );
			  InternalFlatFileRealm internalFlatFileRealm = new InternalFlatFileRealm(_users, new InMemoryRoleRepository(), new BasicPasswordPolicy(), authStrategy, mock(typeof(JobScheduler)), new InMemoryUserRepository(), new InMemoryUserRepository()
							 );

			  _manager = new MultiRealmAuthManager( internalFlatFileRealm, Collections.singleton( internalFlatFileRealm ), new MemoryConstrainedCacheManager(), _securityLog, true, false, Collections.emptyMap() );
			  _manager.init();
		 }

		 public virtual EnterpriseAuthAndUserManager Manager
		 {
			 get
			 {
				  return _manager;
			 }
		 }

		 public virtual LoginContext MakeLoginContext( ShiroSubject shiroSubject )
		 {
			  return new StandardEnterpriseLoginContext( _manager, shiroSubject );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, final org.junit.runner.Description description)
		 public override Statement Apply( Statement @base, Description description )
		 {
			  return new StatementAnonymousInnerClass( this, @base );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly MultiRealmAuthManagerRule _outerInstance;

			 private Statement @base;

			 public StatementAnonymousInnerClass( MultiRealmAuthManagerRule outerInstance, Statement @base )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
			 }

			 public override void evaluate()
			 {
				  try
				  {
						_outerInstance.setupAuthManager( _outerInstance.authStrategy );
						@base.evaluate();
				  }
				  catch ( Exception t )
				  {
						fail( "Got unexpected exception " + t );
				  }
				  finally
				  {
						try
						{
							 outerInstance.TearDown();
						}
						catch ( Exception t )
						{
							 throw new Exception( "Failed to shut down MultiRealmAuthManager", t );
						}
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _manager.stop();
			  _manager.shutdown();
		 }

		 public virtual FullSecurityLog GetFullSecurityLog()
		 {
			  return new FullSecurityLog( _securityLogWriter.Buffer.ToString().Split("\n", true) );
		 }

		 public class FullSecurityLog
		 {
			  internal IList<string> Lines;

			  internal FullSecurityLog( string[] logLines )
			  {
					Lines = Arrays.asList( logLines );
			  }

			  public virtual void AssertHasLine( string subject, string msg )
			  {
					assertThat( Lines, hasItem( containsString( "[" + subject + "]: " + msg ) ) );
			  }
		 }
	}

}