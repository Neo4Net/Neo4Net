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
namespace Neo4Net.causalclustering.core.state.snapshot
{

	using Neo4Net.causalclustering.core.state.storage;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

	public class CoreSnapshot
	{
		 private readonly long _prevIndex;
		 private readonly long _prevTerm;

		 private readonly IDictionary<CoreStateType, object> _snapshotCollection = new Dictionary<CoreStateType, object>();

		 public CoreSnapshot( long prevIndex, long prevTerm )
		 {
			  this._prevIndex = prevIndex;
			  this._prevTerm = prevTerm;
		 }

		 public virtual long PrevIndex()
		 {
			  return _prevIndex;
		 }

		 public virtual long PrevTerm()
		 {
			  return _prevTerm;
		 }

		 public virtual void Add( CoreStateType type, object state )
		 {
			  _snapshotCollection[type] = state;
		 }

		 public virtual T Get<T>( CoreStateType type )
		 {
			  return ( T ) _snapshotCollection[type];
		 }

		 public virtual IEnumerable<CoreStateType> Types()
		 {
			  return _snapshotCollection.Keys;
		 }

		 public virtual int Size()
		 {
			  return _snapshotCollection.Count;
		 }

		 public class Marshal : SafeChannelMarshal<CoreSnapshot>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(CoreSnapshot coreSnapshot, Neo4Net.Kernel.Api.StorageEngine.WritableChannel buffer) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( CoreSnapshot coreSnapshot, WritableChannel buffer )
			  {
					buffer.PutLong( coreSnapshot._prevIndex );
					buffer.PutLong( coreSnapshot._prevTerm );

					buffer.PutInt( coreSnapshot.Size() );
					foreach ( CoreStateType type in coreSnapshot.Types() )
					{
						 buffer.PutInt( type.ordinal() );
						 type.marshal.marshal( coreSnapshot.Get( type ), buffer );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public CoreSnapshot unmarshal0(Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException, Neo4Net.causalclustering.messaging.EndOfStreamException
			  public override CoreSnapshot Unmarshal0( ReadableChannel channel )
			  {
					long prevIndex = channel.Long;
					long prevTerm = channel.Long;

					CoreSnapshot coreSnapshot = new CoreSnapshot( prevIndex, prevTerm );
					int snapshotCount = channel.Int;
					for ( int i = 0; i < snapshotCount; i++ )
					{
						 int typeOrdinal = channel.Int;
						 CoreStateType type = CoreStateType.values()[typeOrdinal];
						 object state = type.marshal.unmarshal( channel );
						 coreSnapshot.Add( type, state );
					}

					return coreSnapshot;
			  }
		 }

		 public override string ToString()
		 {
			  return format( "CoreSnapshot{prevIndex=%d, prevTerm=%d, snapshotCollection=%s}", _prevIndex, _prevTerm, _snapshotCollection );
		 }
	}

}