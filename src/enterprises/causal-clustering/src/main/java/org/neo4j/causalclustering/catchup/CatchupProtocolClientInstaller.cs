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
namespace Neo4Net.causalclustering.catchup
{
	using Channel = io.netty.channel.Channel;


	using FileChunkDecoder = Neo4Net.causalclustering.catchup.storecopy.FileChunkDecoder;
	using FileChunkHandler = Neo4Net.causalclustering.catchup.storecopy.FileChunkHandler;
	using FileHeaderDecoder = Neo4Net.causalclustering.catchup.storecopy.FileHeaderDecoder;
	using FileHeaderHandler = Neo4Net.causalclustering.catchup.storecopy.FileHeaderHandler;
	using GetIndexFilesRequest = Neo4Net.causalclustering.catchup.storecopy.GetIndexFilesRequest;
	using GetStoreFileRequest = Neo4Net.causalclustering.catchup.storecopy.GetStoreFileRequest;
	using GetStoreIdRequestEncoder = Neo4Net.causalclustering.catchup.storecopy.GetStoreIdRequestEncoder;
	using GetStoreIdResponseDecoder = Neo4Net.causalclustering.catchup.storecopy.GetStoreIdResponseDecoder;
	using GetStoreIdResponseHandler = Neo4Net.causalclustering.catchup.storecopy.GetStoreIdResponseHandler;
	using PrepareStoreCopyRequestEncoder = Neo4Net.causalclustering.catchup.storecopy.PrepareStoreCopyRequestEncoder;
	using PrepareStoreCopyResponse = Neo4Net.causalclustering.catchup.storecopy.PrepareStoreCopyResponse;
	using StoreCopyFinishedResponseDecoder = Neo4Net.causalclustering.catchup.storecopy.StoreCopyFinishedResponseDecoder;
	using StoreCopyFinishedResponseHandler = Neo4Net.causalclustering.catchup.storecopy.StoreCopyFinishedResponseHandler;
	using TxPullRequestEncoder = Neo4Net.causalclustering.catchup.tx.TxPullRequestEncoder;
	using TxPullResponseDecoder = Neo4Net.causalclustering.catchup.tx.TxPullResponseDecoder;
	using TxPullResponseHandler = Neo4Net.causalclustering.catchup.tx.TxPullResponseHandler;
	using TxStreamFinishedResponseDecoder = Neo4Net.causalclustering.catchup.tx.TxStreamFinishedResponseDecoder;
	using TxStreamFinishedResponseHandler = Neo4Net.causalclustering.catchup.tx.TxStreamFinishedResponseHandler;
	using CoreSnapshotDecoder = Neo4Net.causalclustering.core.state.snapshot.CoreSnapshotDecoder;
	using CoreSnapshotRequestEncoder = Neo4Net.causalclustering.core.state.snapshot.CoreSnapshotRequestEncoder;
	using CoreSnapshotResponseHandler = Neo4Net.causalclustering.core.state.snapshot.CoreSnapshotResponseHandler;
	using Neo4Net.causalclustering.protocol;
	using NettyPipelineBuilderFactory = Neo4Net.causalclustering.protocol.NettyPipelineBuilderFactory;
	using Neo4Net.causalclustering.protocol;
	using Neo4Net.causalclustering.protocol;
	using ProtocolInstaller_Orientation = Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class CatchupProtocolClientInstaller : ProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client>
	{
		 private const Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocols APPLICATION_PROTOCOL = Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocols.Catchup_1;
		 public class Factory : Neo4Net.causalclustering.protocol.ProtocolInstaller_Factory<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client, CatchupProtocolClientInstaller>
		 {
			  public Factory( NettyPipelineBuilderFactory pipelineBuilder, LogProvider logProvider, CatchUpResponseHandler handler ) : base( APPLICATION_PROTOCOL, outerInstance.modifiers -> new CatchupProtocolClientInstaller( pipelineBuilder, outerInstance.modifiers, logProvider, handler ) )
			  {
			  }
		 }

		 private readonly IList<ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client>> _modifiers;
		 private readonly LogProvider _logProvider;
		 private readonly Log _log;
		 private readonly NettyPipelineBuilderFactory _pipelineBuilder;
		 private readonly CatchUpResponseHandler _handler;

		 public CatchupProtocolClientInstaller( NettyPipelineBuilderFactory pipelineBuilder, IList<ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client>> modifiers, LogProvider logProvider, CatchUpResponseHandler handler )
		 {
			  this._modifiers = modifiers;
			  this._logProvider = logProvider;
			  this._log = logProvider.getLog( this.GetType() );
			  this._pipelineBuilder = pipelineBuilder;
			  this._handler = handler;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void install(io.netty.channel.Channel channel) throws Exception
		 public override void Install( Channel channel )
		 {
			  CatchupClientProtocol protocol = new CatchupClientProtocol();

			  RequestDecoderDispatcher<CatchupClientProtocol.State> decoderDispatcher = new RequestDecoderDispatcher<CatchupClientProtocol.State>( protocol, _logProvider );
			  decoderDispatcher.Register( CatchupClientProtocol.State.StoreId, new GetStoreIdResponseDecoder() );
			  decoderDispatcher.Register( CatchupClientProtocol.State.TxPullResponse, new TxPullResponseDecoder() );
			  decoderDispatcher.Register( CatchupClientProtocol.State.CoreSnapshot, new CoreSnapshotDecoder() );
			  decoderDispatcher.Register( CatchupClientProtocol.State.StoreCopyFinished, new StoreCopyFinishedResponseDecoder() );
			  decoderDispatcher.Register( CatchupClientProtocol.State.TxStreamFinished, new TxStreamFinishedResponseDecoder() );
			  decoderDispatcher.Register( CatchupClientProtocol.State.FileHeader, new FileHeaderDecoder() );
			  decoderDispatcher.Register( CatchupClientProtocol.State.PrepareStoreCopyResponse, new PrepareStoreCopyResponse.Decoder() );
			  decoderDispatcher.Register( CatchupClientProtocol.State.FileContents, new FileChunkDecoder() );

			  _pipelineBuilder.client( channel, _log ).modify( _modifiers ).addFraming().add("enc_req_tx", new TxPullRequestEncoder()).add("enc_req_index", new GetIndexFilesRequest.Encoder()).add("enc_req_store", new GetStoreFileRequest.Encoder()).add("enc_req_snapshot", new CoreSnapshotRequestEncoder()).add("enc_req_store_id", new GetStoreIdRequestEncoder()).add("enc_req_type", new ResponseMessageTypeEncoder()).add("enc_res_type", new RequestMessageTypeEncoder()).add("enc_req_precopy", new PrepareStoreCopyRequestEncoder()).add("in_res_type", new ClientMessageTypeHandler(protocol, _logProvider)).add("dec_dispatch", decoderDispatcher).add("hnd_res_tx", new TxPullResponseHandler(protocol, _handler)).add("hnd_res_snapshot", new CoreSnapshotResponseHandler(protocol, _handler)).add("hnd_res_copy_fin", new StoreCopyFinishedResponseHandler(protocol, _handler)).add("hnd_res_tx_fin", new TxStreamFinishedResponseHandler(protocol, _handler)).add("hnd_res_file_header", new FileHeaderHandler(protocol, _handler, _logProvider)).add("hnd_res_file_chunk", new FileChunkHandler(protocol, _handler)).add("hnd_res_store_id", new GetStoreIdResponseHandler(protocol, _handler)).add("hnd_res_store_listing", new StoreListingResponseHandler(protocol, _handler)).onClose(_handler.onClose).install();
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