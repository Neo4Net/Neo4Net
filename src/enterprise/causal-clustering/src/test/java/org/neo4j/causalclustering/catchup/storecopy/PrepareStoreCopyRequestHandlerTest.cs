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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelPromise = io.netty.channel.ChannelPromise;
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using StoreCopyCheckPointMutex = Neo4Net.Kernel.impl.transaction.log.checkpoint.StoreCopyCheckPointMutex;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class PrepareStoreCopyRequestHandlerTest
	{
		 private static readonly StoreId _storeIdMatching = new StoreId( 1, 2, 3, 4 );
		 private static readonly StoreId _storeIdMismatching = new StoreId( 5000, 6000, 7000, 8000 );
		 private readonly ChannelHandlerContext _channelHandlerContext = mock( typeof( ChannelHandlerContext ) );
		 private EmbeddedChannel _embeddedChannel;

		 private static readonly CheckPointer _checkPointer = mock( typeof( CheckPointer ) );
		 private static readonly NeoStoreDataSource _neoStoreDataSource = mock( typeof( NeoStoreDataSource ) );
		 private CatchupServerProtocol _catchupServerProtocol;
		 private readonly PrepareStoreCopyFiles _prepareStoreCopyFiles = mock( typeof( PrepareStoreCopyFiles ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  Dependencies dependencies = new Dependencies();
			  dependencies.SatisfyDependency( _checkPointer );
			  StoreCopyCheckPointMutex storeCopyCheckPointMutex = new StoreCopyCheckPointMutex();
			  when( _neoStoreDataSource.StoreCopyCheckPointMutex ).thenReturn( storeCopyCheckPointMutex );
			  when( _neoStoreDataSource.DependencyResolver ).thenReturn( dependencies );
			  PrepareStoreCopyRequestHandler subject = CreateHandler();
			  _embeddedChannel = new EmbeddedChannel( subject );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveErrorResponseIfStoreMismatch()
		 public virtual void ShouldGiveErrorResponseIfStoreMismatch()
		 {
			  // given store id doesn't match

			  // when PrepareStoreCopyRequest is written to channel
			  _embeddedChannel.writeInbound( new PrepareStoreCopyRequest( _storeIdMismatching ) );

			  // then there is a store id mismatch message
			  assertEquals( ResponseMessageType.PREPARE_STORE_COPY_RESPONSE, _embeddedChannel.readOutbound() );
			  PrepareStoreCopyResponse response = PrepareStoreCopyResponse.Error( PrepareStoreCopyResponse.Status.EStoreIdMismatch );
			  assertEquals( response, _embeddedChannel.readOutbound() );

			  // and the expected message type is reset back to message type
			  assertTrue( _catchupServerProtocol.isExpecting( CatchupServerProtocol.State.MESSAGE_TYPE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetSuccessfulResponseFromPrepareStoreCopyRequest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetSuccessfulResponseFromPrepareStoreCopyRequest()
		 {
			  // given storeId matches
			  LongSet indexIds = LongSets.immutable.of( 1 );
			  File[] files = new File[]{ new File( "file" ) };
			  long lastCheckpoint = 1;

			  ConfigureProvidedStoreCopyFiles( new StoreResource[0], files, indexIds, lastCheckpoint );

			  // when store listing is requested
			  _embeddedChannel.writeInbound( _channelHandlerContext, new PrepareStoreCopyRequest( _storeIdMatching ) );

			  // and the contents of the store listing response is sent
			  assertEquals( ResponseMessageType.PREPARE_STORE_COPY_RESPONSE, _embeddedChannel.readOutbound() );
			  PrepareStoreCopyResponse response = PrepareStoreCopyResponse.Success( files, indexIds, lastCheckpoint );
			  assertEquals( response, _embeddedChannel.readOutbound() );

			  // and the protocol is reset to expect any message type after listing has been transmitted
			  assertTrue( _catchupServerProtocol.isExpecting( CatchupServerProtocol.State.MESSAGE_TYPE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRetainLockWhileStreaming() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRetainLockWhileStreaming()
		 {
			  // given
			  ChannelPromise channelPromise = _embeddedChannel.newPromise();
			  ChannelHandlerContext channelHandlerContext = mock( typeof( ChannelHandlerContext ) );
			  when( channelHandlerContext.writeAndFlush( any( typeof( PrepareStoreCopyResponse ) ) ) ).thenReturn( channelPromise );

			  ReentrantReadWriteLock @lock = new ReentrantReadWriteLock();
			  when( _neoStoreDataSource.StoreCopyCheckPointMutex ).thenReturn( new StoreCopyCheckPointMutex( @lock ) );
			  PrepareStoreCopyRequestHandler subjectHandler = CreateHandler();

			  // and
			  LongSet indexIds = LongSets.immutable.of( 42 );
			  File[] files = new File[]{ new File( "file" ) };
			  long lastCheckpoint = 1;
			  ConfigureProvidedStoreCopyFiles( new StoreResource[0], files, indexIds, lastCheckpoint );

			  // when
			  subjectHandler.ChannelRead0( channelHandlerContext, new PrepareStoreCopyRequest( _storeIdMatching ) );

			  // then
			  assertEquals( 1, @lock.ReadLockCount );

			  // when
			  channelPromise.setSuccess();

			  //then
			  assertEquals( 0, @lock.ReadLockCount );
		 }

		 private PrepareStoreCopyRequestHandler CreateHandler()
		 {
			  _catchupServerProtocol = new CatchupServerProtocol();
			  _catchupServerProtocol.expect( CatchupServerProtocol.State.PREPARE_STORE_COPY );
			  System.Func<NeoStoreDataSource> dataSourceSupplier = () => _neoStoreDataSource;
			  when( _neoStoreDataSource.StoreId ).thenReturn( new Neo4Net.Kernel.Api.StorageEngine.StoreId( 1, 2, 5, 3, 4 ) );

			  PrepareStoreCopyFilesProvider prepareStoreCopyFilesProvider = mock( typeof( PrepareStoreCopyFilesProvider ) );
			  when( prepareStoreCopyFilesProvider.PrepareStoreCopyFiles( any() ) ).thenReturn(_prepareStoreCopyFiles);

			  return new PrepareStoreCopyRequestHandler( _catchupServerProtocol, dataSourceSupplier, prepareStoreCopyFilesProvider );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void configureProvidedStoreCopyFiles(StoreResource[] atomicFiles, java.io.File[] files, org.eclipse.collections.api.set.primitive.LongSet indexIds, long lastCommitedTx) throws java.io.IOException
		 private void ConfigureProvidedStoreCopyFiles( StoreResource[] atomicFiles, File[] files, LongSet indexIds, long lastCommitedTx )
		 {
			  when( _prepareStoreCopyFiles.AtomicFilesSnapshot ).thenReturn( atomicFiles );
			  when( _prepareStoreCopyFiles.NonAtomicIndexIds ).thenReturn( indexIds );
			  when( _prepareStoreCopyFiles.listReplayableFiles() ).thenReturn(files);
			  when( _checkPointer.lastCheckPointedTransactionId() ).thenReturn(lastCommitedTx);
		 }
	}

}