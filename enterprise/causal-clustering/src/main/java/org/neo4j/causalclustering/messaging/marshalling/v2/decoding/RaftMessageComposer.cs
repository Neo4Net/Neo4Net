using System.Collections.Generic;
using System.Text;

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
namespace Org.Neo4j.causalclustering.messaging.marshalling.v2.decoding
{
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using MessageToMessageDecoder = io.netty.handler.codec.MessageToMessageDecoder;


	using RaftMessages = Org.Neo4j.causalclustering.core.consensus.RaftMessages;
	using ReplicatedContent = Org.Neo4j.causalclustering.core.replication.ReplicatedContent;

	public class RaftMessageComposer : MessageToMessageDecoder<object>
	{
		 private readonly LinkedList<ReplicatedContent> _replicatedContents = new LinkedList<ReplicatedContent>();
		 private readonly LinkedList<long> _raftLogEntryTerms = new LinkedList<long>();
		 private RaftMessageDecoder.ClusterIdAwareMessageComposer _messageComposer;
		 private readonly Clock _clock;

		 public RaftMessageComposer( Clock clock )
		 {
			  this._clock = clock;
		 }

		 protected internal override void Decode( ChannelHandlerContext ctx, object msg, IList<object> @out )
		 {
			  if ( msg is ReplicatedContent )
			  {
					_replicatedContents.AddLast( ( ReplicatedContent ) msg );
			  }
			  else if ( msg is RaftLogEntryTermsDecoder.RaftLogEntryTerms )
			  {
					foreach ( long term in ( ( RaftLogEntryTermsDecoder.RaftLogEntryTerms ) msg ).Terms() )
					{
						 _raftLogEntryTerms.AddLast( term );
					}
			  }
			  else if ( msg is RaftMessageDecoder.ClusterIdAwareMessageComposer )
			  {
					if ( _messageComposer != null )
					{
						 throw new System.InvalidOperationException( "Received raft message header. Pipeline already contains message header waiting to build." );
					}
					_messageComposer = ( RaftMessageDecoder.ClusterIdAwareMessageComposer ) msg;
			  }
			  else
			  {
					throw new System.InvalidOperationException( "Unexpected object in the pipeline: " + msg );
			  }
			  if ( _messageComposer != null )
			  {
					Optional<Org.Neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage> clusterIdAwareMessage = _messageComposer.maybeCompose( _clock, _raftLogEntryTerms, _replicatedContents );
					clusterIdAwareMessage.ifPresent(message =>
					{
					 Clear( message );
					 @out.Add( message );
					});
			  }
		 }

		 private void Clear( Org.Neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage message )
		 {
			  _messageComposer = null;
			  if ( _replicatedContents.Count > 0 || _raftLogEntryTerms.Count > 0 )
			  {
					throw new System.InvalidOperationException( string.Format( "Message [{0}] was composed without using all resources in the pipeline. " + "Pipeline still contains Replicated contents[{1}] and RaftLogEntryTerms [{2}]", message, Stringify( _replicatedContents ), Stringify( _raftLogEntryTerms ) ) );
			  }
		 }

		 private string Stringify<T1>( IEnumerable<T1> objects )
		 {
			  StringBuilder stringBuilder = new StringBuilder();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<?> iterator = objects.iterator();
			  IEnumerator<object> iterator = objects.GetEnumerator();
			  while ( iterator.MoveNext() )
			  {
					stringBuilder.Append( iterator.Current );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( iterator.hasNext() )
					{
						 stringBuilder.Append( ", " );
					}
			  }
			  return stringBuilder.ToString();
		 }
	}

}