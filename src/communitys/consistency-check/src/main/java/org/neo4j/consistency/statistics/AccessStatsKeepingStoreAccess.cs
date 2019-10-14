/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Consistency.statistics
{
	using AccessStats = Neo4Net.Consistency.statistics.AccessStatistics.AccessStats;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using Neo4Net.Kernel.impl.store;
	using StoreAccess = Neo4Net.Kernel.impl.store.StoreAccess;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;

	/// <summary>
	/// <seealso cref="StoreAccess"/> that decorates each store, feeding stats about access into <seealso cref="AccessStatistics"/>.
	/// </summary>
	public class AccessStatsKeepingStoreAccess : StoreAccess
	{
		 private readonly AccessStatistics _accessStatistics;

		 public AccessStatsKeepingStoreAccess( NeoStores neoStore, AccessStatistics accessStatistics ) : base( neoStore )
		 {
			  this._accessStatistics = accessStatistics;
		 }

		 protected internal override RecordStore<R> WrapStore<R>( RecordStore<R> store ) where R : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  AccessStats accessStats = new AccessStats( store.GetType().Name, store.RecordsPerPage );
			  _accessStatistics.register( store, accessStats );
			  return new AccessStatsKeepingRecordStore( store, accessStats );
		 }

		 private class AccessStatsKeepingRecordStore<RECORD> : Neo4Net.Kernel.impl.store.RecordStore_Delegator<RECORD> where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly AccessStats AccessStatsConflict;

			  internal AccessStatsKeepingRecordStore( RecordStore<RECORD> actual, AccessStats accessStats ) : base( actual )
			  {
					this.AccessStatsConflict = accessStats;
			  }

			  protected internal virtual AccessStats AccessStats
			  {
				  get
				  {
						return AccessStatsConflict;
				  }
			  }

			  public override RECORD GetRecord( long id, RECORD record, RecordLoad load )
			  {
					AccessStatsConflict.upRead( id );
					return base.GetRecord( id, record, load );
			  }
		 }
	}

}