/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.impl.locking
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Ignore = org.junit.Ignore;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using VerboseTimeout = Neo4Net.Test.rule.VerboseTimeout;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.rule.concurrent.OtherThreadRule.isWaiting;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite, run from LockingCompatibilityTestSuite.") public class AcquisitionTimeoutCompatibility extends LockingCompatibilityTestSuite.Compatibility
	public class AcquisitionTimeoutCompatibility : LockingCompatibilityTestSuite.Compatibility
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			Timeout = VerboseTimeout.builder().withTimeout(_testTimeoutSeconds, TimeUnit.SECONDS).build();
		}


		 private readonly long _testTimeoutSeconds = 30;
		 private FakeClock _clock;
		 private Config _customConfig;
		 private Locks _lockManager;
		 private Locks_Client _client;
		 private Locks_Client _client2;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.VerboseTimeout timeout = org.Neo4Net.test.rule.VerboseTimeout.builder().withTimeout(TEST_TIMEOUT_SECONDS, java.util.concurrent.TimeUnit.SECONDS).build();
		 public VerboseTimeout Timeout;

		 public AcquisitionTimeoutCompatibility( LockingCompatibilityTestSuite suite ) : base( suite )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _customConfig = Config.defaults( GraphDatabaseSettings.lock_acquisition_timeout, "100ms" );
			  _clock = Clocks.fakeClock( 100000, TimeUnit.MINUTES );
			  _lockManager = Suite.createLockManager( _customConfig, _clock );
			  _client = _lockManager.newClient();
			  _client2 = _lockManager.newClient();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _client2.close();
			  _client.close();
			  _lockManager.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateSharedLockAcquisition() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminateSharedLockAcquisition()
		 {
			  _client.acquireExclusive( LockTracer.NONE, ResourceTypes.Node, 1 );
			  Future<bool> sharedLockAcquisition = ThreadB.execute(state =>
			  {
				_client2.acquireShared( LockTracer.NONE, ResourceTypes.Node, 1 );
				return true;
			  });

			  assertThat( ThreadB, Waiting );

			  _clock.forward( 101, TimeUnit.MILLISECONDS );

			  VerifyAcquisitionFailure( sharedLockAcquisition );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateExclusiveLockAcquisitionForExclusivelyLockedResource() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminateExclusiveLockAcquisitionForExclusivelyLockedResource()
		 {
			  _client.acquireExclusive( LockTracer.NONE, ResourceTypes.Node, 1 );
			  Future<bool> exclusiveLockAcquisition = ThreadB.execute(state =>
			  {
				_client2.acquireExclusive( LockTracer.NONE, ResourceTypes.Node, 1 );
				return true;
			  });

			  assertThat( ThreadB, Waiting );

			  _clock.forward( 101, TimeUnit.MILLISECONDS );

			  VerifyAcquisitionFailure( exclusiveLockAcquisition );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateExclusiveLockAcquisitionForSharedLockedResource() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminateExclusiveLockAcquisitionForSharedLockedResource()
		 {
			  _client.acquireShared( LockTracer.NONE, ResourceTypes.Node, 1 );
			  Future<bool> exclusiveLockAcquisition = ThreadB.execute(state =>
			  {
				_client2.acquireExclusive( LockTracer.NONE, ResourceTypes.Node, 1 );
				return true;
			  });

			  assertThat( ThreadB, Waiting );

			  _clock.forward( 101, TimeUnit.MILLISECONDS );

			  VerifyAcquisitionFailure( exclusiveLockAcquisition );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateExclusiveLockAcquisitionForSharedLockedResourceWithSharedLockHeld() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminateExclusiveLockAcquisitionForSharedLockedResourceWithSharedLockHeld()
		 {
			  _client.acquireShared( LockTracer.NONE, ResourceTypes.Node, 1 );
			  _client2.acquireShared( LockTracer.NONE, ResourceTypes.Node, 1 );
			  Future<bool> exclusiveLockAcquisition = ThreadB.execute(state =>
			  {
				_client2.acquireExclusive( LockTracer.NONE, ResourceTypes.Node, 1 );
				return true;
			  });

			  assertThat( ThreadB, Waiting );

			  _clock.forward( 101, TimeUnit.MILLISECONDS );

			  VerifyAcquisitionFailure( exclusiveLockAcquisition );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyAcquisitionFailure(java.util.concurrent.Future<bool> lockAcquisition) throws InterruptedException
		 private void VerifyAcquisitionFailure( Future<bool> lockAcquisition )
		 {
			  try
			  {
					lockAcquisition.get();
					fail( "Lock acquisition should fail." );
			  }
			  catch ( ExecutionException e )
			  {
					assertThat( Exceptions.rootCause( e ), instanceOf( typeof( LockAcquisitionTimeoutException ) ) );
			  }
		 }
	}

}