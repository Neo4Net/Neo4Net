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
namespace Neo4Net.causalclustering.core.replication
{

	using GlobalSession = Neo4Net.causalclustering.core.replication.session.GlobalSession;
	using LocalOperationId = Neo4Net.causalclustering.core.replication.session.LocalOperationId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using Neo4Net.causalclustering.messaging.marshalling;
	using ReplicatedContentHandler = Neo4Net.causalclustering.messaging.marshalling.ReplicatedContentHandler;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

	/// <summary>
	/// A uniquely identifiable operation.
	/// </summary>
	public class DistributedOperation : ReplicatedContent
	{
		 private readonly ReplicatedContent _content;
		 private readonly GlobalSession _globalSession;
		 private readonly LocalOperationId _operationId;

		 public DistributedOperation( ReplicatedContent content, GlobalSession globalSession, LocalOperationId operationId )
		 {
			  this._content = content;
			  this._globalSession = globalSession;
			  this._operationId = operationId;
		 }

		 public virtual GlobalSession GlobalSession()
		 {
			  return _globalSession;
		 }

		 public virtual LocalOperationId OperationId()
		 {
			  return _operationId;
		 }

		 public virtual ReplicatedContent Content()
		 {
			  return _content;
		 }

		 public override long? Size()
		 {
			  return _content.size();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(Neo4Net.causalclustering.messaging.marshalling.ReplicatedContentHandler contentHandler) throws java.io.IOException
		 public override void Handle( ReplicatedContentHandler contentHandler )
		 {
			  contentHandler.Handle( this );
			  Content().handle(contentHandler);
		 }

		 /// <summary>
		 /// This this consumer ignores the content which is handles by its own serializer.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshalMetaData(Neo4Net.Kernel.Api.StorageEngine.WritableChannel channel) throws java.io.IOException
		 public virtual void MarshalMetaData( WritableChannel channel )
		 {
			  channel.PutLong( GlobalSession().sessionId().MostSignificantBits );
			  channel.PutLong( GlobalSession().sessionId().LeastSignificantBits );
			  ( new MemberId.Marshal() ).marshal(GlobalSession().owner(), channel);

			  channel.PutLong( _operationId.localSessionId() );
			  channel.PutLong( _operationId.sequenceNumber() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static Neo4Net.causalclustering.messaging.marshalling.ContentBuilder<ReplicatedContent> deserialize(Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException, Neo4Net.causalclustering.messaging.EndOfStreamException
		 public static ContentBuilder<ReplicatedContent> Deserialize( ReadableChannel channel )
		 {
			  long mostSigBits = channel.Long;
			  long leastSigBits = channel.Long;
			  MemberId owner = ( new MemberId.Marshal() ).unmarshal(channel);
			  GlobalSession globalSession = new GlobalSession( new System.Guid( mostSigBits, leastSigBits ), owner );

			  long localSessionId = channel.Long;
			  long sequenceNumber = channel.Long;
			  LocalOperationId localOperationId = new LocalOperationId( localSessionId, sequenceNumber );

			  return ContentBuilder.unfinished( subContent => new DistributedOperation( subContent, globalSession, localOperationId ) );
		 }

		 public override string ToString()
		 {
			  return "DistributedOperation{" +
						"content=" + _content +
						", globalSession=" + _globalSession +
						", operationId=" + _operationId +
						'}';
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  DistributedOperation that = ( DistributedOperation ) o;
			  return Objects.Equals( _content, that._content ) && Objects.Equals( _globalSession, that._globalSession ) && Objects.Equals( _operationId, that._operationId );
		 }

		 public override int GetHashCode()
		 {

			  return Objects.hash( _content, _globalSession, _operationId );
		 }
	}

}