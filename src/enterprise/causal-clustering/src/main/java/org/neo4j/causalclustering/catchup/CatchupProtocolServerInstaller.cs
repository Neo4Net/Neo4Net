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
namespace Neo4Net.causalclustering.catchup
{
	using Channel = io.netty.channel.Channel;
	using ChannelHandler = io.netty.channel.ChannelHandler;
	using ChannelInboundHandler = io.netty.channel.ChannelInboundHandler;
	using ChunkedWriteHandler = io.netty.handler.stream.ChunkedWriteHandler;


	using FileChunkEncoder = Neo4Net.causalclustering.catchup.storecopy.FileChunkEncoder;
	using FileHeaderEncoder = Neo4Net.causalclustering.catchup.storecopy.FileHeaderEncoder;
	using GetIndexFilesRequest = Neo4Net.causalclustering.catchup.storecopy.GetIndexFilesRequest;
	using GetStoreFileRequest = Neo4Net.causalclustering.catchup.storecopy.GetStoreFileRequest;
	using GetStoreIdRequest = Neo4Net.causalclustering.catchup.storecopy.GetStoreIdRequest;
	using GetStoreIdResponseEncoder = Neo4Net.causalclustering.catchup.storecopy.GetStoreIdResponseEncoder;
	using PrepareStoreCopyRequestDecoder = Neo4Net.causalclustering.catchup.storecopy.PrepareStoreCopyRequestDecoder;
	using PrepareStoreCopyResponse = Neo4Net.causalclustering.catchup.storecopy.PrepareStoreCopyResponse;
	using StoreCopyFinishedResponseEncoder = Neo4Net.causalclustering.catchup.storecopy.StoreCopyFinishedResponseEncoder;
	using TxPullRequestDecoder = Neo4Net.causalclustering.catchup.tx.TxPullRequestDecoder;
	using TxPullResponseEncoder = Neo4Net.causalclustering.catchup.tx.TxPullResponseEncoder;
	using TxStreamFinishedResponseEncoder = Neo4Net.causalclustering.catchup.tx.TxStreamFinishedResponseEncoder;
	using CoreSnapshotEncoder = Neo4Net.causalclustering.core.state.snapshot.CoreSnapshotEncoder;
	using CoreSnapshotRequest = Neo4Net.causalclustering.core.state.snapshot.CoreSnapshotRequest;
	using Neo4Net.causalclustering.protocol;
	using NettyPipelineBuilderFactory = Neo4Net.causalclustering.protocol.NettyPipelineBuilderFactory;
	using Neo4Net.causalclustering.protocol;
	using Neo4Net.causalclustering.protocol;
	using ProtocolInstaller_Orientation = Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class CatchupProtocolServerInstaller : ProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server>
	{
		 private const Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocols APPLICATION_PROTOCOL = Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocols.Catchup_1;

		 public class Factory : Neo4Net.causalclustering.protocol.ProtocolInstaller_Factory<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server, CatchupProtocolServerInstaller>
		 {
			  public Factory( NettyPipelineBuilderFactory pipelineBuilderFactory, LogProvider logProvider, CatchupServerHandler catchupServerHandler ) : base( APPLICATION_PROTOCOL, outerInstance.modifiers -> new CatchupProtocolServerInstaller( pipelineBuilderFactory, outerInstance.modifiers, logProvider, catchupServerHandler ) )
			  {
			  }
		 }

		 private readonly NettyPipelineBuilderFactory _pipelineBuilderFactory;
		 private readonly IList<ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server>> _modifiers;
		 private readonly Log _log;

		 private readonly LogProvider _logProvider;
		 private readonly CatchupServerHandler _catchupServerHandler;

		 private CatchupProtocolServerInstaller( NettyPipelineBuilderFactory pipelineBuilderFactory, IList<ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server>> modifiers, LogProvider logProvider, CatchupServerHandler catchupServerHandler )
		 {
			  this._pipelineBuilderFactory = pipelineBuilderFactory;
			  this._modifiers = modifiers;
			  this._log = logProvider.getLog( this.GetType() );
			  this._logProvider = logProvider;
			  this._catchupServerHandler = catchupServerHandler;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void install(io.netty.channel.Channel channel) throws Exception
		 public override void Install( Channel channel )
		 {
			  CatchupServerProtocol state = new CatchupServerProtocol();

			  _pipelineBuilderFactory.server( channel, _log ).modify( _modifiers ).addFraming().add("enc_req_type", new RequestMessageTypeEncoder()).add("enc_res_type", new ResponseMessageTypeEncoder()).add("enc_res_tx_pull", new TxPullResponseEncoder()).add("enc_res_store_id", new GetStoreIdResponseEncoder()).add("enc_res_copy_fin", new StoreCopyFinishedResponseEncoder()).add("enc_res_tx_fin", new TxStreamFinishedResponseEncoder()).add("enc_res_pre_copy", new PrepareStoreCopyResponse.Encoder()).add("enc_snapshot", new CoreSnapshotEncoder()).add("enc_file_chunk", new FileChunkEncoder()).add("enc_file_header", new FileHeaderEncoder()).add("in_req_type", ServerMessageHandler(state)).add("dec_req_dispatch", RequestDecoders(state)).add("out_chunked_write", new ChunkedWriteHandler()).add("hnd_req_tx", _catchupServerHandler.txPullRequestHandler(state)).add("hnd_req_store_id", _catchupServerHandler.getStoreIdRequestHandler(state)).add("hnd_req_store_listing", _catchupServerHandler.storeListingRequestHandler(state)).add("hnd_req_store_file", _catchupServerHandler.getStoreFileRequestHandler(state)).add("hnd_req_index_snapshot", _catchupServerHandler.getIndexSnapshotRequestHandler(state)).add("hnd_req_snapshot", _catchupServerHandler.snapshotHandler(state).map(Collections.singletonList).orElse(emptyList())).install();
		 }

		 private ChannelHandler ServerMessageHandler( CatchupServerProtocol state )
		 {
			  return new ServerMessageTypeHandler( state, _logProvider );
		 }

		 private ChannelInboundHandler RequestDecoders( CatchupServerProtocol protocol )
		 {
			  RequestDecoderDispatcher<CatchupServerProtocol.State> decoderDispatcher = new RequestDecoderDispatcher<CatchupServerProtocol.State>( protocol, _logProvider );
			  decoderDispatcher.Register( CatchupServerProtocol.State.TxPull, new TxPullRequestDecoder() );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  decoderDispatcher.Register( CatchupServerProtocol.State.GetStoreId, new SimpleRequestDecoder( GetStoreIdRequest::new ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  decoderDispatcher.Register( CatchupServerProtocol.State.GetCoreSnapshot, new SimpleRequestDecoder( CoreSnapshotRequest::new ) );
			  decoderDispatcher.Register( CatchupServerProtocol.State.PrepareStoreCopy, new PrepareStoreCopyRequestDecoder() );
			  decoderDispatcher.Register( CatchupServerProtocol.State.GetStoreFile, new GetStoreFileRequest.Decoder() );
			  decoderDispatcher.Register( CatchupServerProtocol.State.GetIndexSnapshot, new GetIndexFilesRequest.Decoder() );
			  return decoderDispatcher;
		 }

		 public override Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol ApplicationProtocol()
		 {
			  return APPLICATION_PROTOCOL;
		 }

		 public override ICollection<ICollection<Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol>> Modifiers()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return _modifiers.Select( ModifierProtocolInstaller::protocols ).ToList();
		 }
	}

}