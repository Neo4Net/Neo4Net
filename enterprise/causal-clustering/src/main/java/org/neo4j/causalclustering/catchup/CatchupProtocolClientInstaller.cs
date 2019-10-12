using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.catchup
{
	using Channel = io.netty.channel.Channel;


	using FileChunkDecoder = Org.Neo4j.causalclustering.catchup.storecopy.FileChunkDecoder;
	using FileChunkHandler = Org.Neo4j.causalclustering.catchup.storecopy.FileChunkHandler;
	using FileHeaderDecoder = Org.Neo4j.causalclustering.catchup.storecopy.FileHeaderDecoder;
	using FileHeaderHandler = Org.Neo4j.causalclustering.catchup.storecopy.FileHeaderHandler;
	using GetIndexFilesRequest = Org.Neo4j.causalclustering.catchup.storecopy.GetIndexFilesRequest;
	using GetStoreFileRequest = Org.Neo4j.causalclustering.catchup.storecopy.GetStoreFileRequest;
	using GetStoreIdRequestEncoder = Org.Neo4j.causalclustering.catchup.storecopy.GetStoreIdRequestEncoder;
	using GetStoreIdResponseDecoder = Org.Neo4j.causalclustering.catchup.storecopy.GetStoreIdResponseDecoder;
	using GetStoreIdResponseHandler = Org.Neo4j.causalclustering.catchup.storecopy.GetStoreIdResponseHandler;
	using PrepareStoreCopyRequestEncoder = Org.Neo4j.causalclustering.catchup.storecopy.PrepareStoreCopyRequestEncoder;
	using PrepareStoreCopyResponse = Org.Neo4j.causalclustering.catchup.storecopy.PrepareStoreCopyResponse;
	using StoreCopyFinishedResponseDecoder = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyFinishedResponseDecoder;
	using StoreCopyFinishedResponseHandler = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyFinishedResponseHandler;
	using TxPullRequestEncoder = Org.Neo4j.causalclustering.catchup.tx.TxPullRequestEncoder;
	using TxPullResponseDecoder = Org.Neo4j.causalclustering.catchup.tx.TxPullResponseDecoder;
	using TxPullResponseHandler = Org.Neo4j.causalclustering.catchup.tx.TxPullResponseHandler;
	using TxStreamFinishedResponseDecoder = Org.Neo4j.causalclustering.catchup.tx.TxStreamFinishedResponseDecoder;
	using TxStreamFinishedResponseHandler = Org.Neo4j.causalclustering.catchup.tx.TxStreamFinishedResponseHandler;
	using CoreSnapshotDecoder = Org.Neo4j.causalclustering.core.state.snapshot.CoreSnapshotDecoder;
	using CoreSnapshotRequestEncoder = Org.Neo4j.causalclustering.core.state.snapshot.CoreSnapshotRequestEncoder;
	using CoreSnapshotResponseHandler = Org.Neo4j.causalclustering.core.state.snapshot.CoreSnapshotResponseHandler;
	using Org.Neo4j.causalclustering.protocol;
	using NettyPipelineBuilderFactory = Org.Neo4j.causalclustering.protocol.NettyPipelineBuilderFactory;
	using Org.Neo4j.causalclustering.protocol;
	using Org.Neo4j.causalclustering.protocol;
	using ProtocolInstaller_Orientation = Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	public class CatchupProtocolClientInstaller : ProtocolInstaller<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client>
	{
		 private const Org.Neo4j.causalclustering.protocol.Protocol_ApplicationProtocols APPLICATION_PROTOCOL = Org.Neo4j.causalclustering.protocol.Protocol_ApplicationProtocols.Catchup_1;
		 public class Factory : Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Factory<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client, CatchupProtocolClientInstaller>
		 {
			  public Factory( NettyPipelineBuilderFactory pipelineBuilder, LogProvider logProvider, CatchUpResponseHandler handler ) : base( APPLICATION_PROTOCOL, outerInstance.modifiers -> new CatchupProtocolClientInstaller( pipelineBuilder, outerInstance.modifiers, logProvider, handler ) )
			  {
			  }
		 }

		 private readonly IList<ModifierProtocolInstaller<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client>> _modifiers;
		 private readonly LogProvider _logProvider;
		 private readonly Log _log;
		 private readonly NettyPipelineBuilderFactory _pipelineBuilder;
		 private readonly CatchUpResponseHandler _handler;

		 public CatchupProtocolClientInstaller( NettyPipelineBuilderFactory pipelineBuilder, IList<ModifierProtocolInstaller<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client>> modifiers, LogProvider logProvider, CatchUpResponseHandler handler )
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

		 public override Org.Neo4j.causalclustering.protocol.Protocol_ApplicationProtocol ApplicationProtocol()
		 {
			  return APPLICATION_PROTOCOL;
		 }

		 public override ICollection<ICollection<Org.Neo4j.causalclustering.protocol.Protocol_ModifierProtocol>> Modifiers()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return _modifiers.Select( ModifierProtocolInstaller::protocols ).ToList();
		 }
	}

}