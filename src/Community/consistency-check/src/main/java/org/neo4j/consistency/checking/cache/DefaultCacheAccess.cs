using System.Collections.Generic;

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
namespace Neo4Net.Consistency.checking.cache
{

	using Neo4Net.Consistency.checking.full;
	using Counts = Neo4Net.Consistency.statistics.Counts;
	using Counts_Type = Neo4Net.Consistency.statistics.Counts_Type;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using ByteArray = Neo4Net.@unsafe.Impl.Batchimport.cache.ByteArray;

	/// <summary>
	/// <seealso cref="CacheAccess"/> that uses <seealso cref="PackedMultiFieldCache"/> for cache.
	/// </summary>
	public class DefaultCacheAccess : CacheAccess
	{
		 public const int DEFAULT_QUEUE_SIZE = 1_000;

		 private readonly IdAssigningThreadLocal<CacheAccess_Client> clients = new IdAssigningThreadLocalAnonymousInnerClass();

		 private class IdAssigningThreadLocalAnonymousInnerClass : IdAssigningThreadLocal<CacheAccess_Client>
		 {
			 protected internal override CacheAccess_Client initialValue( int id )
			 {
				  return new DefaultClient( id );
			 }
		 }

		 private readonly ICollection<PropertyRecord>[] _propertiesProcessed;
		 private bool _forwardScan = true;
		 private readonly PackedMultiFieldCache _cache;
		 private long _recordsPerCPU;
		 private readonly Counts _counts;

		 public DefaultCacheAccess( Counts counts, int threads ) : this( PackedMultiFieldCache.DefaultArray(), counts, threads )
		 {
		 }

		 public DefaultCacheAccess( ByteArray array, Counts counts, int threads )
		 {
			  this._counts = counts;
			  this._propertiesProcessed = new System.Collections.ICollection[threads];
			  this._cache = new PackedMultiFieldCache( array, ByteArrayBitsManipulator.MAX_SLOT_BITS, 1 );
		 }

		 public override CacheAccess_Client Client()
		 {
			  return clients.get();
		 }

		 public override void ClearCache()
		 {
			  _cache.clear();
		 }

		 public virtual params int[] CacheSlotSizes
		 {
			 set
			 {
				  _cache.SlotSizes = value;
			 }
		 }

		 public virtual bool Forward
		 {
			 set
			 {
				  _forwardScan = value;
			 }
			 get
			 {
				  return _forwardScan;
			 }
		 }


		 public override void PrepareForProcessingOfSingleStore( long recordsPerCpu )
		 {
			  clients.resetId();
			  this._recordsPerCPU = recordsPerCpu;
		 }

		 private class DefaultClient : CacheAccess_Client
		 {
			 private readonly DefaultCacheAccess _outerInstance;

			  internal readonly int ThreadIndex;

			  internal DefaultClient( DefaultCacheAccess outerInstance, int threadIndex )
			  {
				  this._outerInstance = outerInstance;
					this.ThreadIndex = threadIndex;
			  }

			  public override long GetFromCache( long id, int slot )
			  {
					return outerInstance.cache.Get( id, slot );
			  }

			  public override bool GetBooleanFromCache( long id, int slot )
			  {
					return outerInstance.cache.Get( id, slot ) != 0;
			  }

			  public override void PutToCache( long id, params long[] values )
			  {
					outerInstance.cache.Put( id, values );
			  }

			  public override void PutToCacheSingle( long id, int slot, long value )
			  {
					outerInstance.cache.Put( id, slot, value );
			  }

			  public override void ClearCache( long index )
			  {
					outerInstance.cache.Clear( index );
					outerInstance.counts.IncAndGet( Counts_Type.clearCache, ThreadIndex );
					outerInstance.counts.IncAndGet( Counts_Type.activeCache, ThreadIndex );
			  }

			  public override bool WithinBounds( long id )
			  {
					return outerInstance.recordsPerCPU == 0 || id >= ThreadIndex * outerInstance.recordsPerCPU && id < ( ThreadIndex + 1 ) * outerInstance.recordsPerCPU;
			  }

			  public override void PutPropertiesToCache( ICollection<PropertyRecord> properties )
			  {
					outerInstance.propertiesProcessed[ThreadIndex] = properties;
			  }

			  public virtual IEnumerable<PropertyRecord> PropertiesFromCache
			  {
				  get
				  {
						return CachedProperties( true );
				  }
			  }

			  public override PropertyRecord GetPropertyFromCache( long id )
			  {
					ICollection<PropertyRecord> properties = CachedProperties( false );
					if ( properties != null )
					{
						 foreach ( PropertyRecord property in properties )
						 {
							  if ( property.Id == id )
							  {
									return property;
							  }
						 }
					}
					return null;
			  }

			  internal virtual ICollection<PropertyRecord> CachedProperties( bool clear )
			  {
					try
					{
						 return outerInstance.propertiesProcessed[ThreadIndex];
					}
					finally
					{
						 if ( clear )
						 {
							  outerInstance.propertiesProcessed[ThreadIndex] = null;
						 }
					}
			  }

			  public override void IncAndGetCount( Counts_Type type )
			  {
					outerInstance.counts.IncAndGet( type, ThreadIndex );
			  }

			  public override string ToString()
			  {
					return "Client[" + ThreadIndex + ", records/CPU:" + outerInstance.recordsPerCPU + "]";
			  }
		 }
	}

}