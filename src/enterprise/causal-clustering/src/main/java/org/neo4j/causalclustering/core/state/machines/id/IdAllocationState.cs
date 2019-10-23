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
namespace Neo4Net.causalclustering.core.state.machines.id
{

	using Neo4Net.causalclustering.core.state.storage;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

	/// <summary>
	/// An in-memory representation of the IDs allocated to this core instance.
	/// Instances of this class are serialized to disk by
	/// <p/>
	/// <seealso cref="Marshal"/>. The serialized form:
	/// <p/>
	/// +----------------------------------+
	/// |  8-byte length marker            |
	/// +----------------------------------+
	/// |  first unallocated               |
	/// |  15x 8-byte                      |
	/// +----------------------------------+
	/// </summary>
	public class IdAllocationState : UnallocatedIds
	{
		 private readonly long[] _firstUnallocated;
		 private long _logIndex;

		 internal IdAllocationState() : this(new long[Enum.GetValues(typeof(IdType)).length], -1L)
		 {
		 }

		 public IdAllocationState( long[] firstUnallocated, long logIndex )
		 {
			  this._firstUnallocated = firstUnallocated;
			  this._logIndex = logIndex;
		 }

		 /// <returns> The last set log index, which is the value last passed to <seealso cref="logIndex(long)"/> </returns>
		 public virtual long LogIndex()
		 {
			  return _logIndex;
		 }

		 /// <summary>
		 /// Sets the last seen log index, which is the last log index at which a replicated value that updated this state
		 /// was encountered.
		 /// </summary>
		 /// <param name="logIndex"> The value to set as the last log index at which this state was updated </param>
		 public virtual void LogIndex( long logIndex )
		 {
			  this._logIndex = logIndex;
		 }

		 /// <param name="idType"> the type of graph object whose ID is under allocation </param>
		 /// <returns> the first unallocated entry for idType </returns>
		 public override long FirstUnallocated( IdType idType )
		 {
			  return _firstUnallocated[( int )idType];
		 }

		 /// <param name="idType">     the type of graph object whose ID is under allocation </param>
		 /// <param name="idRangeEnd"> the first unallocated entry for idType </param>
		 internal virtual void FirstUnallocated( IdType idType, long idRangeEnd )
		 {
			  _firstUnallocated[( int )idType] = idRangeEnd;
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

			  IdAllocationState that = ( IdAllocationState ) o;

			  return _logIndex == that._logIndex && Arrays.Equals( _firstUnallocated, that._firstUnallocated );
		 }

		 public override int GetHashCode()
		 {
			  int result = Arrays.GetHashCode( _firstUnallocated );
			  result = 31 * result + ( int )( _logIndex ^ ( ( long )( ( ulong )_logIndex >> 32 ) ) );
			  return result;
		 }

		 public virtual IdAllocationState NewInstance()
		 {
			  return new IdAllocationState( _firstUnallocated.Clone(), _logIndex );
		 }

		 public class Marshal : SafeStateMarshal<IdAllocationState>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(IdAllocationState state, org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( IdAllocationState state, WritableChannel channel )
			  {
					channel.PutLong( ( long ) state._firstUnallocated.Length );
					foreach ( long l in state._firstUnallocated )
					{
						 channel.PutLong( l );
					}

					channel.PutLong( state._logIndex );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public IdAllocationState unmarshal0(org.Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException
			  public override IdAllocationState Unmarshal0( ReadableChannel channel )
			  {
					long[] firstNotAllocated = new long[( int ) channel.Long];

					for ( int i = 0; i < firstNotAllocated.Length; i++ )
					{
						 firstNotAllocated[i] = channel.Long;
					}

					long logIndex = channel.Long;

					return new IdAllocationState( firstNotAllocated, logIndex );
			  }

			  public override IdAllocationState StartState()
			  {
					return new IdAllocationState();
			  }

			  public override long Ordinal( IdAllocationState state )
			  {
					return state.LogIndex();
			  }
		 }

		 public override string ToString()
		 {
			  return string.Format( "IdAllocationState{{firstUnallocated={0}, logIndex={1:D}}}", Arrays.ToString( _firstUnallocated ), _logIndex );
		 }
	}

}