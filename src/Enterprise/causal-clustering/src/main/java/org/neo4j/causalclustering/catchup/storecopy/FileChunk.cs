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

	using Neo4Net.causalclustering.core.state.storage;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using Neo4Net.causalclustering.messaging.marshalling;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

	public class FileChunk
	{
		 internal const int MAX_SIZE = 8192;
		 private const int USE_MAX_SIZE_AND_EXPECT_MORE_CHUNKS = -1;
		 private readonly int _encodedLength;
		 private readonly sbyte[] _bytes;

		 internal static FileChunk Create( sbyte[] bytes, bool last )
		 {
			  if ( !last && bytes.Length != MAX_SIZE )
			  {
					throw new System.ArgumentException( "All chunks except for the last must be of max size." );
			  }
			  return new FileChunk( last ? bytes.Length : USE_MAX_SIZE_AND_EXPECT_MORE_CHUNKS, bytes );
		 }

		 internal FileChunk( int encodedLength, sbyte[] bytes )
		 {
			  this._encodedLength = encodedLength;
			  this._bytes = bytes;
		 }

		 public virtual bool Last
		 {
			 get
			 {
				  return _encodedLength != USE_MAX_SIZE_AND_EXPECT_MORE_CHUNKS;
			 }
		 }

		 public virtual sbyte[] Bytes()
		 {
			  return _bytes;
		 }

		 public virtual int Length()
		 {
			  return _encodedLength;
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
			  FileChunk fileChunk = ( FileChunk ) o;
			  return _encodedLength == fileChunk._encodedLength && Arrays.Equals( _bytes, fileChunk._bytes );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _encodedLength, Arrays.GetHashCode( _bytes ) );
		 }

		 public override string ToString()
		 {
			  return "FileChunk{" + Arrays.ToString( _bytes ) + '}';
		 }

		 public static ChannelMarshal<FileChunk> Marshal()
		 {
			  return Marshal.Instance;
		 }

		 private class Marshal : SafeChannelMarshal<FileChunk>
		 {
			  internal static readonly Marshal Instance = new Marshal();

			  internal Marshal()
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(FileChunk fileChunk, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( FileChunk fileChunk, WritableChannel channel )
			  {
					channel.PutInt( fileChunk._encodedLength );
					sbyte[] bytes = fileChunk.Bytes();
					channel.Put( bytes, bytes.Length );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected FileChunk unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
			  protected internal override FileChunk Unmarshal0( ReadableChannel channel )
			  {
					int encodedLength = channel.Int;
					int length = encodedLength == USE_MAX_SIZE_AND_EXPECT_MORE_CHUNKS ? MAX_SIZE : encodedLength;
					sbyte[] bytes = new sbyte[length];
					channel.Get( bytes, length );
					return new FileChunk( encodedLength, bytes );
			  }
		 }
	}

}