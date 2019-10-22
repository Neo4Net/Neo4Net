using System.Diagnostics;

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

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IdContainer = Neo4Net.Kernel.impl.store.id.IdContainer;
	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdRange = Neo4Net.Kernel.impl.store.id.IdRange;
	using IdRangeIterator = Neo4Net.Kernel.impl.store.id.IdRangeIterator;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdRangeIterator.EMPTY_ID_RANGE_ITERATOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdRangeIterator.VALUE_REPRESENTING_NULL;

	internal class ReplicatedIdGenerator : IdGenerator
	{
		 private readonly IdType _idType;
		 private readonly Log _log;
		 private readonly ReplicatedIdRangeAcquirer _acquirer;
		 private volatile long _highId;
		 private volatile IdRangeIterator _idQueue = EMPTY_ID_RANGE_ITERATOR;
		 private readonly IdContainer _idContainer;
		 private readonly ReentrantLock _idContainerLock = new ReentrantLock();

		 internal ReplicatedIdGenerator( FileSystemAbstraction fs, File file, IdType idType, System.Func<long> highId, ReplicatedIdRangeAcquirer acquirer, LogProvider logProvider, int grabSize, bool aggressiveReuse )
		 {
			  this._idType = idType;
			  this._highId = highId();
			  this._acquirer = acquirer;
			  this._log = logProvider.getLog( this.GetType() );
			  _idContainer = new IdContainer( fs, file, grabSize, aggressiveReuse );
			  _idContainer.init();
		 }

		 public override void Close()
		 {
			  _idContainerLock.@lock();
			  try
			  {
					_idContainer.close( _highId );
			  }
			  finally
			  {
					_idContainerLock.unlock();
			  }
		 }

		 public override void FreeId( long id )
		 {
			  _idContainerLock.@lock();
			  try
			  {
					_idContainer.freeId( id );
			  }
			  finally
			  {
					_idContainerLock.unlock();
			  }
		 }

		 public virtual long HighId
		 {
			 get
			 {
				  return _highId;
			 }
			 set
			 {
				  this._highId = max( this._highId, value );
			 }
		 }


		 public virtual long HighestPossibleIdInUse
		 {
			 get
			 {
				  return _highId - 1;
			 }
		 }

		 public virtual long NumberOfIdsInUse
		 {
			 get
			 {
				  return _highId - DefragCount;
			 }
		 }

		 public override long NextId()
		 {
			 lock ( this )
			 {
				  long id = ReusableId;
				  if ( id != IdContainer.NO_RESULT )
				  {
						return id;
				  }
      
				  long nextId = _idQueue.nextId();
				  if ( nextId == VALUE_REPRESENTING_NULL )
				  {
						AcquireNextIdBatch();
						nextId = _idQueue.nextId();
				  }
				  _highId = max( _highId, nextId + 1 );
				  return nextId;
			 }
		 }

		 private void AcquireNextIdBatch()
		 {
			  IdAllocation allocation = _acquirer.acquireIds( _idType );

			  Debug.Assert( allocation.IdRange.RangeLength > 0 );
			  _log.debug( "Received id allocation " + allocation + " for " + _idType );
			  StoreLocally( allocation );
		 }

		 public override IdRange NextIdBatch( int size )
		 {
			 lock ( this )
			 {
				  IdRange idBatch = GetReusableIdBatch( size );
				  if ( idBatch.TotalSize() > 0 )
				  {
						return idBatch;
				  }
				  IdRange range = _idQueue.nextIdBatch( size );
				  if ( range.TotalSize() == 0 )
				  {
						AcquireNextIdBatch();
						range = _idQueue.nextIdBatch( size );
						HighId = range.HighId;
				  }
				  return range;
			 }
		 }

		 public virtual long DefragCount
		 {
			 get
			 {
				  _idContainerLock.@lock();
				  try
				  {
						return _idContainer.FreeIdCount;
				  }
				  finally
				  {
						_idContainerLock.unlock();
				  }
			 }
		 }

		 public override void Delete()
		 {
			  _idContainerLock.@lock();
			  try
			  {
					_idContainer.delete();
			  }
			  finally
			  {
					_idContainerLock.unlock();
			  }
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[" + this._idQueue + "]";
		 }

		 internal static void CreateGenerator( FileSystemAbstraction fs, File fileName, long highId, bool throwIfFileExists )
		 {
			  IdContainer.createEmptyIdFile( fs, fileName, highId, throwIfFileExists );
		 }

		 private long ReusableId
		 {
			 get
			 {
				  _idContainerLock.@lock();
				  try
				  {
						return _idContainer.ReusableId;
				  }
				  finally
				  {
						_idContainerLock.unlock();
				  }
			 }
		 }

		 private IdRange GetReusableIdBatch( int maxSize )
		 {
			  _idContainerLock.@lock();
			  try
			  {
					return _idContainer.getReusableIdBatch( maxSize );
			  }
			  finally
			  {
					_idContainerLock.unlock();
			  }
		 }

		 private void StoreLocally( IdAllocation allocation )
		 {
			  HighId = allocation.HighestIdInUse + 1; // high id is certainly bigger than the highest id in use
			  this._idQueue = RespectingHighId( allocation.IdRange ).GetEnumerator();
		 }

		 private IdRange RespectingHighId( IdRange idRange )
		 {
			  int adjustment = 0;
			  long originalRangeStart = idRange.RangeStart;
			  if ( _highId > originalRangeStart )
			  {
					adjustment = ( int )( _highId - originalRangeStart );
			  }
			  long rangeStart = max( this._highId, originalRangeStart );
			  int rangeLength = idRange.RangeLength - adjustment;
			  if ( rangeLength <= 0 )
			  {
					throw new System.InvalidOperationException( "IdAllocation state is probably corrupted or out of sync with the cluster. " + "Local highId is " + _highId + " and allocation range is " + idRange );
			  }
			  return new IdRange( idRange.DefragIds, rangeStart, rangeLength );
		 }
	}

}