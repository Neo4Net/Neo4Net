using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Test.impl
{

	using PrimitiveLongCollections = Neo4Net.Collection.PrimitiveLongCollections;
	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdRange = Neo4Net.Kernel.impl.store.id.IdRange;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using CommunityIdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.CommunityIdTypeConfigurationProvider;
	using IdTypeConfiguration = Neo4Net.Kernel.impl.store.id.configuration.IdTypeConfiguration;
	using IdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.IdTypeConfigurationProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.min;

	public class EphemeralIdGenerator : IdGenerator
	{
		 public class Factory : IdGeneratorFactory
		 {
			  protected internal readonly IDictionary<IdType, IdGenerator> Generators = new Dictionary<IdType, IdGenerator>( typeof( IdType ) );
			  internal readonly IdTypeConfigurationProvider IdTypeConfigurationProvider = new CommunityIdTypeConfigurationProvider();

			  public override IdGenerator Open( File filename, IdType idType, System.Func<long> highId, long maxId )
			  {
					return Open( filename, 0, idType, highId, maxId );
			  }

			  public override IdGenerator Open( File fileName, int grabSize, IdType idType, System.Func<long> highId, long maxId )
			  {
					IdGenerator generator = Generators[idType];
					if ( generator == null )
					{
						 IdTypeConfiguration idTypeConfiguration = IdTypeConfigurationProvider.getIdTypeConfiguration( idType );
						 generator = new EphemeralIdGenerator( idType, idTypeConfiguration );
						 Generators[idType] = generator;
					}
					return generator;
			  }

			  public override void Create( File fileName, long highId, bool throwIfFileExists )
			  {
			  }

			  public override IdGenerator Get( IdType idType )
			  {
					return Generators[idType];
			  }
		 }

		 private readonly AtomicLong _nextId = new AtomicLong();
		 private readonly IdType _idType;
		 private readonly LinkedList<long> _freeList;
		 private readonly AtomicInteger _freedButNotReturnableIdCount = new AtomicInteger();

		 public EphemeralIdGenerator( IdType idType, IdTypeConfiguration idTypeConfiguration )
		 {
			  this._idType = idType;
			  this._freeList = idType != null && idTypeConfiguration.AllowAggressiveReuse() ? new ConcurrentLinkedQueue<long>() : null;
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[" + _idType + "]";
		 }

		 public override long NextId()
		 {
			 lock ( this )
			 {
				  if ( _freeList != null )
				  {
						long? id = _freeList.RemoveFirst();
						if ( id != null )
						{
							 return id.Value;
						}
				  }
				  return _nextId.AndIncrement;
			 }
		 }

		 public override IdRange NextIdBatch( int size )
		 {
			 lock ( this )
			 {
				  long[] defragIds = PrimitiveLongCollections.EMPTY_LONG_ARRAY;
				  if ( _freeList != null && _freeList.Count > 0 )
				  {
						defragIds = new long[min( size, _freeList.Count )];
						for ( int i = 0; i < defragIds.Length; i++ )
						{
							 defragIds[i] = _freeList.RemoveFirst();
						}
						size -= defragIds.Length;
				  }
				  return new IdRange( defragIds, _nextId.getAndAdd( size ), size );
			 }
		 }

		 public virtual long HighId
		 {
			 set
			 {
				  _nextId.set( value );
			 }
			 get
			 {
				  return _nextId.get();
			 }
		 }


		 public override void FreeId( long id )
		 {
			  if ( _freeList != null )
			  {
					_freeList.AddLast( id );
			  }
			  else
			  {
					_freedButNotReturnableIdCount.AndIncrement;
			  }
		 }

		 public override void Close()
		 {
		 }

		 public virtual long NumberOfIdsInUse
		 {
			 get
			 {
				  long result = _freeList == null ? _nextId.get() : _nextId.get() - _freeList.Count;
				  return result - _freedButNotReturnableIdCount.get();
			 }
		 }

		 public virtual long DefragCount
		 {
			 get
			 {
				  return 0;
			 }
		 }

		 public override void Delete()
		 {
		 }

		 public virtual long HighestPossibleIdInUse
		 {
			 get
			 {
				  return _nextId.get();
			 }
		 }
	}

}