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
namespace Neo4Net.Kernel.impl.store.id
{
	using Resource = Neo4Net.Graphdb.Resource;
	using Neo4Net.Kernel.impl.store;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;

	public class RenewableBatchIdSequences : Resource
	{
		 private readonly IdSequence[] _types = new IdSequence[StoreType.values().length];

		 public RenewableBatchIdSequences( NeoStores stores, int batchSize )
		 {
			  foreach ( StoreType type in StoreType.values() )
			  {
					if ( type.RecordStore )
					{
						 RecordStore<AbstractBaseRecord> store = stores.GetRecordStore( type );
						 if ( type.LimitedIdStore || batchSize == 1 )
						 {
							  // This is a token store or otherwise meta-data store, so let's not add batching for it
							  _types[type.ordinal()] = store;
						 }
						 else
						 {
							  // This is a normal record store where id batching is beneficial
							  _types[type.ordinal()] = new RenewableBatchIdSequence(store, batchSize, store.freeId);
						 }
					}
			  }
		 }

		 public virtual long NextId( StoreType type )
		 {
			  return IdGenerator( type ).nextId();
		 }

		 public virtual IdSequence IdGenerator( StoreType type )
		 {
			  return _types[type.ordinal()];
		 }

		 public override void Close()
		 {
			  foreach ( StoreType type in StoreType.values() )
			  {
					IdSequence generator = IdGenerator( type );
					if ( generator is RenewableBatchIdSequence )
					{
						 ( ( RenewableBatchIdSequence )generator ).Close();
					}
			  }
		 }
	}

}