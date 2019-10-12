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
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ImmediateEventExecutor = io.netty.util.concurrent.ImmediateEventExecutor;
	using Promise = io.netty.util.concurrent.Promise;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;
	using Mockito = org.mockito.Mockito;


	using Neo4Net.Cursors;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using StoreCopyCheckPointMutex = Neo4Net.Kernel.impl.transaction.log.checkpoint.StoreCopyCheckPointMutex;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.storecopy.StoreCopyFinishedResponse.Status.E_STORE_ID_MISMATCH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.storecopy.StoreCopyFinishedResponse.Status.SUCCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Cursors.rawCursorOf;

	public class StoreStreamingProcessTest
	{
		private bool InstanceFieldsInitialized = false;

		public StoreStreamingProcessTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_checkPointerSupplier = () => _checkPointer;
			_mutex = new StoreCopyCheckPointMutex( @lock );
		}

		 // mocks
		 private readonly StoreFileStreamingProtocol _protocol = mock( typeof( StoreFileStreamingProtocol ) );
		 private readonly CheckPointer _checkPointer = mock( typeof( CheckPointer ) );
		 private readonly StoreResourceStreamFactory _resourceStream = mock( typeof( StoreResourceStreamFactory ) );
		 private readonly ChannelHandlerContext _ctx = mock( typeof( ChannelHandlerContext ) );
		 private System.Func<CheckPointer> _checkPointerSupplier;

		 private ReentrantReadWriteLock @lock = new ReentrantReadWriteLock();
		 private StoreCopyCheckPointMutex _mutex;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformSuccessfulStoreCopyProcess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformSuccessfulStoreCopyProcess()
		 {
			  // given
			  StoreStreamingProcess process = new StoreStreamingProcess( _protocol, _checkPointerSupplier, _mutex, _resourceStream );

			  // mocked behaviour
			  ImmediateEventExecutor eventExecutor = ImmediateEventExecutor.INSTANCE;
			  Promise<Void> completionPromise = eventExecutor.newPromise();
			  long lastCheckpointedTxId = 1000L;
			  RawCursor<StoreResource, IOException> resources = rawCursorOf();

			  when( _checkPointer.tryCheckPoint( any() ) ).thenReturn(lastCheckpointedTxId);
			  when( _checkPointer.lastCheckPointedTransactionId() ).thenReturn(lastCheckpointedTxId);
			  when( _protocol.end( _ctx, SUCCESS ) ).thenReturn( completionPromise );
			  when( _resourceStream.create() ).thenReturn(resources);

			  // when
			  process.Perform( _ctx );

			  // then
			  InOrder inOrder = Mockito.inOrder( _protocol, _checkPointer );
			  inOrder.verify( _checkPointer ).tryCheckPoint( any() );
			  inOrder.verify( _protocol ).end( _ctx, SUCCESS );
			  inOrder.verifyNoMoreInteractions();

			  assertEquals( 1, @lock.ReadLockCount );

			  // when
			  completionPromise.Success = null;

			  // then
			  assertEquals( 0, @lock.ReadLockCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSignalFailure()
		 public virtual void ShouldSignalFailure()
		 {
			  // given
			  StoreStreamingProcess process = new StoreStreamingProcess( _protocol, _checkPointerSupplier, _mutex, _resourceStream );

			  // when
			  process.Fail( _ctx, E_STORE_ID_MISMATCH );

			  // then
			  verify( _protocol ).end( _ctx, E_STORE_ID_MISMATCH );
		 }
	}

}