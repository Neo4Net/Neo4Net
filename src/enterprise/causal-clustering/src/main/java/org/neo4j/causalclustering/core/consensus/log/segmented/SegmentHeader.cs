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
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{

	using Neo4Net.causalclustering.core.state.storage;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

	/// <summary>
	/// The header written at the beginning of each segment.
	/// </summary>
	internal class SegmentHeader
	{
		 internal static readonly int Size = 4 * Long.BYTES;

		 private readonly long _prevFileLastIndex;
		 private readonly long _version;
		 private readonly long _prevIndex;
		 private readonly long _prevTerm;

		 internal SegmentHeader( long prevFileLastIndex, long version, long prevIndex, long prevTerm )
		 {
			  this._prevFileLastIndex = prevFileLastIndex;
			  this._version = version;
			  this._prevIndex = prevIndex;
			  this._prevTerm = prevTerm;
		 }

		 internal virtual long PrevFileLastIndex()
		 {
			  return _prevFileLastIndex;
		 }

		 public virtual long Version()
		 {
			  return _version;
		 }

		 public virtual long PrevIndex()
		 {
			  return _prevIndex;
		 }

		 public virtual long PrevTerm()
		 {
			  return _prevTerm;
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
			  SegmentHeader that = ( SegmentHeader ) o;
			  return _prevFileLastIndex == that._prevFileLastIndex && _version == that._version && _prevIndex == that._prevIndex && _prevTerm == that._prevTerm;
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _prevFileLastIndex, _version, _prevIndex, _prevTerm );
		 }

		 public override string ToString()
		 {
			  return "SegmentHeader{" +
						"prevFileLastIndex=" + _prevFileLastIndex +
						", version=" + _version +
						", prevIndex=" + _prevIndex +
						", prevTerm=" + _prevTerm +
						'}';
		 }

		 internal class Marshal : SafeChannelMarshal<SegmentHeader>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(SegmentHeader header, Neo4Net.Kernel.Api.StorageEngine.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( SegmentHeader header, WritableChannel channel )
			  {
					channel.PutLong( header._prevFileLastIndex );
					channel.PutLong( header._version );
					channel.PutLong( header._prevIndex );
					channel.PutLong( header._prevTerm );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SegmentHeader unmarshal0(Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException
			  public override SegmentHeader Unmarshal0( ReadableChannel channel )
			  {
					long prevFileLastIndex = channel.Long;
					long version = channel.Long;
					long prevIndex = channel.Long;
					long prevTerm = channel.Long;
					return new SegmentHeader( prevFileLastIndex, version, prevIndex, prevTerm );
			  }
		 }
	}

}