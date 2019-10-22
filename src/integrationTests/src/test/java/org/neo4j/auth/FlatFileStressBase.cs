﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.auth
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using User = Neo4Net.Kernel.impl.security.User;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using IJobSchedulerAdapter = Neo4Net.Scheduler.JobSchedulerAdapter;
	using BasicPasswordPolicy = Neo4Net.Server.Security.Auth.BasicPasswordPolicy;
	using CommunitySecurityModule = Neo4Net.Server.Security.Auth.CommunitySecurityModule;
	using Neo4Net.Server.Security.Auth;
	using RateLimitedAuthenticationStrategy = Neo4Net.Server.Security.Auth.RateLimitedAuthenticationStrategy;
	using UserRepository = Neo4Net.Server.Security.Auth.UserRepository;
	using EnterpriseSecurityModule = Neo4Net.Server.security.enterprise.auth.EnterpriseSecurityModule;
	using InternalFlatFileRealm = Neo4Net.Server.security.enterprise.auth.InternalFlatFileRealm;
	using RoleRecord = Neo4Net.Server.security.enterprise.auth.RoleRecord;
	using RoleRepository = Neo4Net.Server.security.enterprise.auth.RoleRepository;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	internal abstract class FlatFileStressBase
	{
		 private readonly long _oneSecond = 1000;
		 protected internal long TimeoutInSeconds = 10;
		 protected internal int N = 10;
		 protected internal int ErrorLimit = 100;

		 internal InternalFlatFileRealm FlatFileRealm;
		 private UserRepository _userRepository;
		 private RoleRepository _roleRepository;

		 private volatile bool _keepRunning = true;
		 internal readonly ISet<Exception> Errors = ConcurrentDictionary.newKeySet();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  Config config = Config.defaults();
			  LogProvider logProvider = NullLogProvider.Instance;
			  IJobScheduler jobScheduler = new IJobSchedulerAdapter();

			  _userRepository = CommunitySecurityModule.getUserRepository( config, logProvider, FileSystem );
			  _roleRepository = EnterpriseSecurityModule.getRoleRepository( config, logProvider, FileSystem );

			  FlatFileRealm = new InternalFlatFileRealm( _userRepository, _roleRepository, new BasicPasswordPolicy(), new RateLimitedAuthenticationStrategy(Clock.systemUTC(), Config.defaults()), jobScheduler, CommunitySecurityModule.getInitialUserRepository(config, logProvider, FileSystem), EnterpriseSecurityModule.getDefaultAdminRepository(config, logProvider, FileSystem) );

			  FlatFileRealm.init();
			  FlatFileRealm.start();
		 }

		 internal abstract FileSystemAbstraction FileSystem { get; }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void teardown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Teardown()
		 {
			  FlatFileRealm.stop();
			  FlatFileRealm.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMaintainConsistency() throws InterruptedException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMaintainConsistency()
		 {
			  ExecutorService service = SetupWorkload( N );

			  for ( int t = 0; t < TimeoutInSeconds; t++ )
			  {
					Thread.Sleep( _oneSecond );
					if ( Errors.Count > ErrorLimit )
					{
						 break;
					}
			  }

			  _keepRunning = false;
			  service.shutdown();
			  service.awaitTermination( 5, SECONDS );

			  // Assert that no errors occured
			  string msg = string.join( Environment.NewLine, Errors.Select( Exception.getMessage ).ToList() );
			  assertThat( msg, Errors, empty() );

			  // Assert that user and role repos are consistent
			  ListSnapshot<User> users = _userRepository.PersistedSnapshot;
			  ListSnapshot<RoleRecord> roles = _roleRepository.PersistedSnapshot;
			  assertTrue( "User and role repositories are no longer consistent", RoleRepository.validate( users.Values(), roles.Values() ) );
		 }

		 internal abstract ExecutorService SetupWorkload( int n );

		 internal abstract class IrrationalAdmin : ThreadStart
		 {
			 private readonly FlatFileStressBase _outerInstance;

			 public IrrationalAdmin( FlatFileStressBase outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal readonly Random Random = new Random();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal ThreadStart[] ActionsConflict;

			  public override void Run()
			  {
					while ( outerInstance.keepRunning )
					{
						 RandomAction().run();
					}
			  }

			  internal virtual ThreadStart RandomAction()
			  {
					return ActionsConflict[Random.Next( ActionsConflict.Length )];
			  }

			  internal virtual params ThreadStart[] Actions
			  {
				  set
				  {
						this.ActionsConflict = value;
				  }
			  }
		 }
	}

}