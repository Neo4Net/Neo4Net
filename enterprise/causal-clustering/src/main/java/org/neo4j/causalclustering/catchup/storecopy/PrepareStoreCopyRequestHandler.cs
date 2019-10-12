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
namespace Org.Neo4j.causalclustering.catchup.storecopy
{
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using SimpleChannelInboundHandler = io.netty.channel.SimpleChannelInboundHandler;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;


	using Resource = Org.Neo4j.Graphdb.Resource;
	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;
	using CheckPointer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.storecopy.DataSourceChecks.hasSameStoreId;

	public class PrepareStoreCopyRequestHandler : SimpleChannelInboundHandler<PrepareStoreCopyRequest>
	{
		 private readonly CatchupServerProtocol _protocol;
		 private readonly PrepareStoreCopyFilesProvider _prepareStoreCopyFilesProvider;
		 private readonly System.Func<NeoStoreDataSource> _dataSourceSupplier;
		 private readonly StoreFileStreamingProtocol _streamingProtocol = new StoreFileStreamingProtocol();

		 public PrepareStoreCopyRequestHandler( CatchupServerProtocol catchupServerProtocol, System.Func<NeoStoreDataSource> dataSourceSupplier, PrepareStoreCopyFilesProvider prepareStoreCopyFilesProvider )
		 {
			  this._protocol = catchupServerProtocol;
			  this._prepareStoreCopyFilesProvider = prepareStoreCopyFilesProvider;
			  this._dataSourceSupplier = dataSourceSupplier;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void channelRead0(io.netty.channel.ChannelHandlerContext channelHandlerContext, PrepareStoreCopyRequest prepareStoreCopyRequest) throws java.io.IOException
		 protected internal override void ChannelRead0( ChannelHandlerContext channelHandlerContext, PrepareStoreCopyRequest prepareStoreCopyRequest )
		 {
			  CloseablesListener closeablesListener = new CloseablesListener();
			  PrepareStoreCopyResponse response = PrepareStoreCopyResponse.Error( PrepareStoreCopyResponse.Status.EListingStore );
			  try
			  {
					NeoStoreDataSource neoStoreDataSource = _dataSourceSupplier.get();
					if ( !hasSameStoreId( prepareStoreCopyRequest.StoreId, neoStoreDataSource ) )
					{
						 channelHandlerContext.write( ResponseMessageType.PREPARE_STORE_COPY_RESPONSE );
						 response = PrepareStoreCopyResponse.Error( PrepareStoreCopyResponse.Status.EStoreIdMismatch );
					}
					else
					{
						 CheckPointer checkPointer = neoStoreDataSource.DependencyResolver.resolveDependency( typeof( CheckPointer ) );
						 closeablesListener.Add( TryCheckpointAndAcquireMutex( checkPointer ) );
						 PrepareStoreCopyFiles prepareStoreCopyFiles = closeablesListener.Add( _prepareStoreCopyFilesProvider.prepareStoreCopyFiles( neoStoreDataSource ) );

						 StoreResource[] nonReplayable = prepareStoreCopyFiles.AtomicFilesSnapshot;
						 foreach ( StoreResource storeResource in nonReplayable )
						 {
							  _streamingProtocol.stream( channelHandlerContext, storeResource );
						 }
						 channelHandlerContext.write( ResponseMessageType.PREPARE_STORE_COPY_RESPONSE );
						 response = CreateSuccessfulResponse( checkPointer, prepareStoreCopyFiles );
					}
			  }
			  finally
			  {
					channelHandlerContext.writeAndFlush( response ).addListener( closeablesListener );
					_protocol.expect( CatchupServerProtocol.State.MESSAGE_TYPE );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private PrepareStoreCopyResponse createSuccessfulResponse(org.neo4j.kernel.impl.transaction.log.checkpoint.CheckPointer checkPointer, PrepareStoreCopyFiles prepareStoreCopyFiles) throws java.io.IOException
		 private PrepareStoreCopyResponse CreateSuccessfulResponse( CheckPointer checkPointer, PrepareStoreCopyFiles prepareStoreCopyFiles )
		 {
			  LongSet indexIds = prepareStoreCopyFiles.NonAtomicIndexIds;
			  File[] files = prepareStoreCopyFiles.ListReplayableFiles();
			  long lastCommittedTxId = checkPointer.LastCheckPointedTransactionId();
			  return PrepareStoreCopyResponse.Success( files, indexIds, lastCommittedTxId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.graphdb.Resource tryCheckpointAndAcquireMutex(org.neo4j.kernel.impl.transaction.log.checkpoint.CheckPointer checkPointer) throws java.io.IOException
		 private Resource TryCheckpointAndAcquireMutex( CheckPointer checkPointer )
		 {
			  return _dataSourceSupplier.get().StoreCopyCheckPointMutex.storeCopy(() => checkPointer.TryCheckPoint(new SimpleTriggerInfo("Store copy")));
		 }
	}

}