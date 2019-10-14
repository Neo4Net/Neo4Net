using System;
using System.Threading;

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
namespace Neo4Net.Kernel.ha.@lock
{
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;
	using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
	using Answer = org.mockito.stubbing.Answer;


	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using ForsetiLockManager = Neo4Net.Kernel.impl.enterprise.@lock.forseti.ForsetiLockManager;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using Log = Neo4Net.Logging.Log;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class SlaveLocksClientConcurrentTest
	{
		 private static ExecutorService _executor;

		 private Master _master;
		 private ForsetiLockManager _lockManager;
		 private RequestContextFactory _requestContextFactory;
		 private DatabaseAvailabilityGuard _databaseAvailabilityGuard;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void initExecutor()
		 public static void InitExecutor()
		 {
			  _executor = Executors.newCachedThreadPool();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void closeExecutor()
		 public static void CloseExecutor()
		 {
			  _executor.shutdownNow();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _master = mock( typeof( Master ), new LockedOnMasterAnswer() );
			  _lockManager = new ForsetiLockManager( Config.defaults(), Clocks.systemClock(), ResourceTypes.values() );
			  _requestContextFactory = mock( typeof( RequestContextFactory ) );
			  _databaseAvailabilityGuard = new DatabaseAvailabilityGuard( GraphDatabaseSettings.DEFAULT_DATABASE_NAME, Clocks.systemClock(), mock(typeof(Log)) );

			  when( _requestContextFactory.newRequestContext( Mockito.anyInt() ) ).thenReturn(RequestContext.anonymous(1));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 1000) public void readersCanAcquireLockAsSoonAsItReleasedOnMaster() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadersCanAcquireLockAsSoonAsItReleasedOnMaster()
		 {
			  SlaveLocksClient reader = CreateClient();
			  SlaveLocksClient writer = CreateClient();

			  System.Threading.CountdownEvent readerCompletedLatch = new System.Threading.CountdownEvent( 1 );
			  System.Threading.CountdownEvent resourceLatch = new System.Threading.CountdownEvent( 1 );

			  when( _master.endLockSession( any( typeof( RequestContext ) ), anyBoolean() ) ).then(new WaitLatchAnswer(resourceLatch, readerCompletedLatch));

			  long nodeId = 10L;
			  ResourceReader resourceReader = new ResourceReader( this, reader, ResourceTypes.NODE, nodeId, resourceLatch, readerCompletedLatch );
			  ResourceWriter resourceWriter = new ResourceWriter( this, writer, ResourceTypes.NODE, nodeId );

			  _executor.submit( resourceReader );
			  _executor.submit( resourceWriter );

			  assertTrue( "Reader should wait for writer to release locks before acquire", readerCompletedLatch.await( 1000, TimeUnit.MILLISECONDS ) );
		 }

		 private SlaveLocksClient CreateClient()
		 {
			  return new SlaveLocksClient( _master, _lockManager.newClient(), _lockManager, _requestContextFactory, _databaseAvailabilityGuard, NullLogProvider.Instance );
		 }

		 private class LockedOnMasterAnswer : Answer
		 {
			  internal readonly Response LockResult;

			  internal LockedOnMasterAnswer()
			  {
					LockResult = Mockito.mock( typeof( Response ) );
					when( LockResult.response() ).thenReturn(new LockResult(LockStatus.OkLocked));
			  }

			  public override object Answer( InvocationOnMock invocation )
			  {
					return LockResult;
			  }
		 }

		 private class WaitLatchAnswer : Answer<Void>
		 {
			  internal readonly System.Threading.CountdownEvent ResourceLatch;
			  internal readonly System.Threading.CountdownEvent ResourceReleaseLatch;

			  internal WaitLatchAnswer( System.Threading.CountdownEvent resourceLatch, System.Threading.CountdownEvent resourceReleaseLatch )
			  {
					this.ResourceLatch = resourceLatch;
					this.ResourceReleaseLatch = resourceReleaseLatch;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void answer(org.mockito.invocation.InvocationOnMock invocation) throws Throwable
			  public override Void Answer( InvocationOnMock invocation )
			  {
					// releasing reader after local lock released
					ResourceLatch.Signal();
					// waiting here for reader to finish read lock acquisition.
					// by this we check that local exclusive lock where released before releasing it on
					// master otherwise reader will be blocked forever
					ResourceReleaseLatch.await();
					return null;
			  }
		 }

		 private class ResourceWriter : ResourceWorker
		 {
			 private readonly SlaveLocksClientConcurrentTest _outerInstance;

			  internal ResourceWriter( SlaveLocksClientConcurrentTest outerInstance, SlaveLocksClient locksClient, ResourceType resourceType, long id ) : base( outerInstance, locksClient, resourceType, id )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override void Run()
			  {
					LocksClient.acquireExclusive( LockTracer.NONE, ResourceType, Id );
					LocksClient.close();
			  }
		 }

		 private class ResourceReader : ResourceWorker
		 {
			 private readonly SlaveLocksClientConcurrentTest _outerInstance;

			  internal readonly System.Threading.CountdownEvent ResourceLatch;
			  internal readonly System.Threading.CountdownEvent ResourceReleaseLatch;

			  internal ResourceReader( SlaveLocksClientConcurrentTest outerInstance, SlaveLocksClient locksClient, ResourceType resourceType, long id, System.Threading.CountdownEvent resourceLatch, System.Threading.CountdownEvent resourceReleaseLatch ) : base( outerInstance, locksClient, resourceType, id )
			  {
				  this._outerInstance = outerInstance;
					this.ResourceLatch = resourceLatch;
					this.ResourceReleaseLatch = resourceReleaseLatch;
			  }

			  public override void Run()
			  {
					try
					{
						 ResourceLatch.await();
						 LocksClient.acquireShared( LockTracer.NONE, ResourceType, Id );
						 ResourceReleaseLatch.Signal();
						 LocksClient.close();
					}
					catch ( Exception e )
					{
						 throw new Exception( e );
					}
			  }
		 }

		 private abstract class ResourceWorker : ThreadStart
		 {
			 private readonly SlaveLocksClientConcurrentTest _outerInstance;

			  protected internal readonly SlaveLocksClient LocksClient;
			  protected internal readonly ResourceType ResourceType;
			  protected internal readonly long Id;

			  internal ResourceWorker( SlaveLocksClientConcurrentTest outerInstance, SlaveLocksClient locksClient, ResourceType resourceType, long id )
			  {
				  this._outerInstance = outerInstance;
					this.LocksClient = locksClient;
					this.ResourceType = resourceType;
					this.Id = id;
			  }
		 }
	}

}