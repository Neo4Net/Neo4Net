using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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

	using Neo4Net.Kernel.impl.store;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;

	/// <summary>
	/// Keeps access statistics about a store, i.e. identifying
	/// <seealso cref="RecordStore.getRecord(long, AbstractBaseRecord, RecordLoad)"/> patterns and how random the access is.
	/// </summary>
	public class AccessStatistics
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<org.neo4j.kernel.impl.store.RecordStore<? extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord>,AccessStats> stats = new java.util.HashMap<>();
		 private readonly IDictionary<RecordStore<AbstractBaseRecord>, AccessStats> _stats = new Dictionary<RecordStore<AbstractBaseRecord>, AccessStats>();

		 public virtual void Register<T1>( RecordStore<T1> store, AccessStats accessStats ) where T1 : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  Debug.Assert( !_stats.ContainsKey( store ) );
			  _stats[store] = accessStats;
		 }

		 public virtual string AccessStatSummary
		 {
			 get
			 {
				  string msg = "";
				  foreach ( AccessStats accessStats in _stats.Values )
				  {
						string accessStat = accessStats.ToString();
						if ( accessStat.Length != 0 )
						{
							 msg += format( accessStat + "%n" );
						}
				  }
				  return msg;
			 }
		 }

		 public virtual void Reset()
		 {
			  foreach ( AccessStats accessStats in _stats.Values )
			  {
					accessStats.Reset();
			  }
		 }

		 public class AccessStats
		 {
			  internal long Reads;
			  internal long Writes;
			  internal long InUse;
			  internal long RandomReads;
			  internal long RandomWrites;
			  internal int ProximityValue;
			  internal readonly string StoreType;
			  internal long PrevReadId;
			  internal long PrevWriteId;

			  public AccessStats( string type, int proximity )
			  {
					this.StoreType = type;
					this.ProximityValue = proximity;
			  }

			  public override string ToString()
			  {
					if ( Reads == 0 && Writes == 0 && RandomReads == 0 )
					{
						 return "";
					}
					StringBuilder buf = new StringBuilder( StoreType );
					AppendStat( buf, "InUse", InUse );
					AppendStat( buf, "Reads", Reads );
					AppendStat( buf, "Random Reads", RandomReads );
					AppendStat( buf, "Writes", Writes );
					AppendStat( buf, "Random Writes", RandomWrites );
					int scatterIndex = 0;
					if ( RandomReads > 0 )
					{
						 long scatterReads = Reads == 0 ? RandomReads : Reads;
						 scatterIndex = ( int )( ( RandomReads * 100 ) / scatterReads );
					}
					AppendStat( buf, "ScatterIndex", scatterIndex );

					// TODO enable this comment again when we have an official property reorganization tool,
					// but keep here as a reminder to do so
	//          if ( scatterIndex > 0.5 )
	//          {
	//              buf.append( format( "%n *** Property Store reorganization is recommended for optimal performance ***" ) );
	//          }

					return buf.ToString();
			  }

			  internal virtual void AppendStat( StringBuilder target, string name, long stat )
			  {
					if ( stat > 0 )
					{
						 target.Append( format( "%n  %s: %d", name, stat ) );
					}
			  }

			  public virtual void Reset()
			  {
					this.Reads = 0;
					this.Writes = 0;
					this.RandomReads = 0;
					this.RandomReads = 0;
					this.RandomWrites = 0;
					this.InUse = 0;
			  }

			  public virtual void UpRead( long id )
			  {
					if ( PrevReadId != id )
					{
						 Reads++;
						 IncrementRandomReads( id, PrevReadId );
						 PrevReadId = id;
					}
			  }

			  internal virtual bool NotCloseBy( long id1, long id2 )
			  {
					return id1 >= 0 && id2 >= 0 && Math.Abs( id2 - id1 ) >= this.ProximityValue;
			  }

			  public virtual void UpWrite( long id )
			  {
					if ( PrevWriteId != id )
					{
						 Writes++;
						 if ( id > 0 && NotCloseBy( id, PrevWriteId ) )
						 {
							  RandomWrites++;
						 }
						 PrevWriteId = id;
					}
			  }

			  public virtual void IncrementRandomReads( long id1, long id2 )
			  {
				  lock ( this )
				  {
						if ( NotCloseBy( id1, id2 ) )
						{
							 RandomReads++;
						}
				  }
			  }

			  public virtual void UpInUse()
			  {
				  lock ( this )
				  {
						InUse++;
				  }
			  }
		 }
	}

}