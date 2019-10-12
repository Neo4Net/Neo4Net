using System;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using StoreCopyRequest = Neo4Net.causalclustering.messaging.StoreCopyRequest;
	using Neo4Net.Graphdb;
	using Iterators = Neo4Net.Helpers.Collection.Iterators;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using TriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.TriggerInfo;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using JobSchedulerAdapter = Neo4Net.Scheduler.JobSchedulerAdapter;
	using StoreFileMetadata = Neo4Net.Storageengine.Api.StoreFileMetadata;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class StoreCopyRequestHandlerTest
	{
		private bool InstanceFieldsInitialized = false;

		public StoreCopyRequestHandlerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_checkPointer = new FakeCheckPointer( this );
			_checkPointerService = new CheckPointerService( () => _checkPointer, _jobScheduler, Group.CHECKPOINT );
		}

		 private static readonly StoreId _storeIdMismatching = new StoreId( 1, 1, 1, 1 );
		 private static readonly StoreId _storeIdMatching = new StoreId( 1, 2, 3, 4 );
		 private readonly DefaultFileSystemAbstraction _fileSystemAbstraction = new DefaultFileSystemAbstraction();

		 private readonly NeoStoreDataSource _neoStoreDataSource = mock( typeof( NeoStoreDataSource ) );
		 private FakeCheckPointer _checkPointer;
		 private EmbeddedChannel _embeddedChannel;
		 private CatchupServerProtocol _catchupServerProtocol;
		 private JobScheduler _jobScheduler = new FakeSingleThreadedJobScheduler();
		 private CheckPointerService _checkPointerService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _catchupServerProtocol = new CatchupServerProtocol();
			  _catchupServerProtocol.expect( CatchupServerProtocol.State.GET_STORE_FILE );
			  StoreCopyRequestHandler storeCopyRequestHandler = new NiceStoreCopyRequestHandler( this, _catchupServerProtocol, () => _neoStoreDataSource, new StoreFileStreamingProtocol(), _fileSystemAbstraction, NullLogProvider.Instance );
			  Dependencies dependencies = new Dependencies();
			  when( _neoStoreDataSource.StoreId ).thenReturn( new Neo4Net.Storageengine.Api.StoreId( 1, 2, 5, 3, 4 ) );
			  when( _neoStoreDataSource.DependencyResolver ).thenReturn( dependencies );
			  when( _neoStoreDataSource.DatabaseLayout ).thenReturn( DatabaseLayout.of( new File( "." ) ) );
			  _embeddedChannel = new EmbeddedChannel( storeCopyRequestHandler );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveProperErrorOnStoreIdMismatch()
		 public virtual void ShouldGiveProperErrorOnStoreIdMismatch()
		 {
			  _embeddedChannel.writeInbound( new GetStoreFileRequest( _storeIdMismatching, new File( "some-file" ), 1 ) );

			  assertEquals( ResponseMessageType.STORE_COPY_FINISHED, _embeddedChannel.readOutbound() );
			  StoreCopyFinishedResponse expectedResponse = new StoreCopyFinishedResponse( StoreCopyFinishedResponse.Status.EStoreIdMismatch );
			  assertEquals( expectedResponse, _embeddedChannel.readOutbound() );

			  assertTrue( _catchupServerProtocol.isExpecting( CatchupServerProtocol.State.MESSAGE_TYPE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveProperErrorOnTxBehind()
		 public virtual void ShouldGiveProperErrorOnTxBehind()
		 {
			  _embeddedChannel.writeInbound( new GetStoreFileRequest( _storeIdMatching, new File( "some-file" ), 2 ) );

			  assertEquals( ResponseMessageType.STORE_COPY_FINISHED, _embeddedChannel.readOutbound() );
			  StoreCopyFinishedResponse expectedResponse = new StoreCopyFinishedResponse( StoreCopyFinishedResponse.Status.ETooFarBehind );
			  assertEquals( expectedResponse, _embeddedChannel.readOutbound() );

			  assertTrue( _catchupServerProtocol.isExpecting( CatchupServerProtocol.State.MESSAGE_TYPE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResetProtocolAndGiveErrorOnUncheckedException()
		 public virtual void ShouldResetProtocolAndGiveErrorOnUncheckedException()
		 {
			  when( _neoStoreDataSource.StoreId ).thenThrow( new System.InvalidOperationException() );

			  try
			  {
					_embeddedChannel.writeInbound( new GetStoreFileRequest( _storeIdMatching, new File( "some-file" ), 1 ) );
					fail();
			  }
			  catch ( System.InvalidOperationException )
			  {

			  }
			  assertEquals( ResponseMessageType.STORE_COPY_FINISHED, _embeddedChannel.readOutbound() );
			  StoreCopyFinishedResponse expectedResponse = new StoreCopyFinishedResponse( StoreCopyFinishedResponse.Status.EUnknown );
			  assertEquals( expectedResponse, _embeddedChannel.readOutbound() );

			  assertTrue( _catchupServerProtocol.isExpecting( CatchupServerProtocol.State.MESSAGE_TYPE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shoulResetProtocolAndGiveErrorIfFilesThrowException()
		 public virtual void ShoulResetProtocolAndGiveErrorIfFilesThrowException()
		 {
			  EmbeddedChannel alternativeChannel = new EmbeddedChannel( new EvilStoreCopyRequestHandler( this, _catchupServerProtocol, () => _neoStoreDataSource, new StoreFileStreamingProtocol(), _fileSystemAbstraction, NullLogProvider.Instance ) );
			  try
			  {
					alternativeChannel.writeInbound( new GetStoreFileRequest( _storeIdMatching, new File( "some-file" ), 1 ) );
					fail();
			  }
			  catch ( System.InvalidOperationException )
			  {
					// do nothing
			  }
			  assertEquals( ResponseMessageType.STORE_COPY_FINISHED, alternativeChannel.readOutbound() );
			  StoreCopyFinishedResponse expectedResponse = new StoreCopyFinishedResponse( StoreCopyFinishedResponse.Status.EUnknown );
			  assertEquals( expectedResponse, alternativeChannel.readOutbound() );

			  assertTrue( _catchupServerProtocol.isExpecting( CatchupServerProtocol.State.MESSAGE_TYPE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionsTooFarBehindStartCheckpointAsynchronously()
		 public virtual void TransactionsTooFarBehindStartCheckpointAsynchronously()
		 {
			  // given checkpoint will fail if performed
			  _checkPointer._tryCheckPoint = null;

			  // when
			  try
			  {
					_embeddedChannel.writeInbound( new GetStoreFileRequest( _storeIdMatching, new File( "some-file" ), 123 ) );
					fail();
			  }
			  catch ( Exception e )
			  {
					assertEquals( "FakeCheckPointer", e.Message );
			  }

			  // then should have received error message
			  assertEquals( ResponseMessageType.STORE_COPY_FINISHED, _embeddedChannel.readOutbound() );

			  // and should have failed on async
			  assertEquals( 1, _checkPointer.invocationCounter.get() );
			  assertEquals( 1, _checkPointer.failCounter.get() );
		 }

		 private class NiceStoreCopyRequestHandler : StoreCopyRequestHandler<StoreCopyRequest>
		 {
			 private readonly StoreCopyRequestHandlerTest _outerInstance;

			  internal NiceStoreCopyRequestHandler( StoreCopyRequestHandlerTest outerInstance, CatchupServerProtocol protocol, System.Func<NeoStoreDataSource> dataSource, StoreFileStreamingProtocol storeFileStreamingProtocol, FileSystemAbstraction fs, LogProvider logProvider ) : base( protocol, dataSource, outerInstance.checkPointerService, storeFileStreamingProtocol, fs, logProvider )
			  {
				  this._outerInstance = outerInstance;
			  }

			  internal override ResourceIterator<StoreFileMetadata> Files( StoreCopyRequest request, NeoStoreDataSource neoStoreDataSource )
			  {
					return Iterators.emptyResourceIterator();
			  }
		 }

		 private class EvilStoreCopyRequestHandler : StoreCopyRequestHandler<StoreCopyRequest>
		 {
			 private readonly StoreCopyRequestHandlerTest _outerInstance;

			  internal EvilStoreCopyRequestHandler( StoreCopyRequestHandlerTest outerInstance, CatchupServerProtocol protocol, System.Func<NeoStoreDataSource> dataSource, StoreFileStreamingProtocol storeFileStreamingProtocol, FileSystemAbstraction fs, LogProvider logProvider ) : base( protocol, dataSource, outerInstance.checkPointerService, storeFileStreamingProtocol, fs, logProvider )
			  {
				  this._outerInstance = outerInstance;
			  }

			  internal override ResourceIterator<StoreFileMetadata> Files( StoreCopyRequest request, NeoStoreDataSource neoStoreDataSource )
			  {
					throw new System.InvalidOperationException( "I am evil" );
			  }
		 }

		 private class FakeCheckPointer : CheckPointer
		 {
			 private readonly StoreCopyRequestHandlerTest _outerInstance;

			 public FakeCheckPointer( StoreCopyRequestHandlerTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal long? _checkPointIfNeeded = 1L;
			  internal long? _tryCheckPoint = 1L;
			  internal long? _forceCheckPoint = 1L;
			  internal long? _lastCheckPointedTransactionId = 1L;
			  internal System.Func<Exception> ExceptionIfEmpty = () => new Exception("FakeCheckPointer");
			  internal AtomicInteger InvocationCounter = new AtomicInteger();
			  internal AtomicInteger FailCounter = new AtomicInteger();

			  public override long CheckPointIfNeeded( TriggerInfo triggerInfo )
			  {
					IncrementInvocationCounter( _checkPointIfNeeded );
					return _checkPointIfNeeded.orElseThrow( ExceptionIfEmpty );
			  }

			  public override long TryCheckPoint( TriggerInfo triggerInfo )
			  {
					IncrementInvocationCounter( _tryCheckPoint );
					return _tryCheckPoint.orElseThrow( ExceptionIfEmpty );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long tryCheckPoint(org.neo4j.kernel.impl.transaction.log.checkpoint.TriggerInfo triggerInfo, System.Func<boolean> timeout) throws java.io.IOException
			  public override long TryCheckPoint( TriggerInfo triggerInfo, System.Func<bool> timeout )
			  {
					return TryCheckPoint( triggerInfo, () => false );
			  }

			  public override long ForceCheckPoint( TriggerInfo triggerInfo )
			  {
					IncrementInvocationCounter( _forceCheckPoint );
					return _forceCheckPoint.orElseThrow( ExceptionIfEmpty );
			  }

			  public override long LastCheckPointedTransactionId()
			  {
					IncrementInvocationCounter( _lastCheckPointedTransactionId );
					return _lastCheckPointedTransactionId.orElseThrow( ExceptionIfEmpty );
			  }

			  internal virtual void IncrementInvocationCounter( long? variable )
			  {
					if ( variable.HasValue )
					{
						 InvocationCounter.AndIncrement;
						 return;
					}
					FailCounter.AndIncrement;
			  }
		 }

		 internal class FakeSingleThreadedJobScheduler : JobSchedulerAdapter
		 {
			  public override JobHandle Schedule( Group group, ThreadStart job )
			  {
					job.run();
					return mock( typeof( JobHandle ) );
			  }
		 }
	}

}