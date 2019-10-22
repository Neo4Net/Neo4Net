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
namespace Neo4Net.causalclustering.core.state.machines.dummy
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChunkedInput = io.netty.handler.stream.ChunkedInput;


	using CoreReplicatedContent = Neo4Net.causalclustering.core.state.machines.tx.CoreReplicatedContent;
	using Neo4Net.causalclustering.core.state.storage;
	using ByteArrayChunkedEncoder = Neo4Net.causalclustering.messaging.marshalling.ByteArrayChunkedEncoder;
	using ReplicatedContentHandler = Neo4Net.causalclustering.messaging.marshalling.ReplicatedContentHandler;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

	public class DummyRequest : CoreReplicatedContent
	{
		 private readonly sbyte[] _data;

		 public DummyRequest( sbyte[] data )
		 {
			  this._data = data;
		 }

		 public override long? Size()
		 {
			  return long?.of( ( long ) _data.Length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(org.Neo4Net.causalclustering.messaging.marshalling.ReplicatedContentHandler contentHandler) throws java.io.IOException
		 public override void Handle( ReplicatedContentHandler contentHandler )
		 {
			  contentHandler.Handle( this );
		 }

		 public virtual long ByteCount()
		 {
			  return _data != null ? _data.Length : 0;
		 }

		 public override void Dispatch( CommandDispatcher commandDispatcher, long commandIndex, System.Action<Result> callback )
		 {
			  commandDispatcher.Dispatch( this, commandIndex, callback );
		 }

		 public virtual ChunkedInput<ByteBuf> Encoder()
		 {
			  sbyte[] array = _data;
			  if ( array == null )
			  {
					array = new sbyte[0];
			  }
			  return new ByteArrayChunkedEncoder( array );
		 }

		 public static DummyRequest Decode( ByteBuf byteBuf )
		 {
			  int length = byteBuf.readableBytes();
			  sbyte[] array = new sbyte[length];
			  byteBuf.readBytes( array );
			  return new DummyRequest( array );
		 }

		 public class Marshal : SafeChannelMarshal<DummyRequest>
		 {
			  public static readonly Marshal Instance = new Marshal();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(DummyRequest dummy, org.Neo4Net.storageengine.api.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( DummyRequest dummy, WritableChannel channel )
			  {
					if ( dummy._data != null )
					{
						 channel.PutInt( dummy._data.Length );
						 channel.Put( dummy._data, dummy._data.Length );
					}
					else
					{
						 channel.PutInt( 0 );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected DummyRequest unmarshal0(org.Neo4Net.storageengine.api.ReadableChannel channel) throws java.io.IOException
			  protected internal override DummyRequest Unmarshal0( ReadableChannel channel )
			  {
					int length = channel.Int;
					sbyte[] data;
					if ( length > 0 )
					{
						 data = new sbyte[length];
						 channel.Get( data, length );
					}
					else
					{
						 data = null;
					}
					return new DummyRequest( data );
			  }
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
			  DummyRequest that = ( DummyRequest ) o;
			  return Arrays.Equals( _data, that._data );
		 }

		 public override int GetHashCode()
		 {
			  return Arrays.GetHashCode( _data );
		 }
	}

}