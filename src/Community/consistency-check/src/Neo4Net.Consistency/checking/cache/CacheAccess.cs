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

	using CheckStage = Neo4Net.Consistency.checking.full.CheckStage;
	using Counts = Neo4Net.Consistency.statistics.Counts;
	using Counts_Type = Neo4Net.Consistency.statistics.Counts_Type;
	using RecordAccess = Neo4Net.Consistency.Store.RecordAccess;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;

	/// <summary>
	/// Just as <seealso cref="RecordAccess"/> is the main access point for <seealso cref="AbstractBaseRecord"/> and friends,
	/// so is <seealso cref="CacheAccess"/> the main access point for cached values related to records, most often caching
	/// parts of records, specific to certain <seealso cref="CheckStage stages"/> of the consistency check.
	/// 
	/// The access patterns to <seealso cref="CacheAccess"/> is designed to have multiple threads concurrently, and so
	/// <seealso cref="client()"/> provides a <seealso cref="Client"/> that accesses the cache on the current thread's behalf.
	/// 
	/// The cache is a compact representation of records, tied to an id, for example nodeId. There can be multiple
	/// cached values per id, selected by {@code slot}.
	/// </summary>
	public interface ICacheAccess
	{
		 /// <summary>
		 /// Client per thread for accessing cache and counts for statistics
		 /// </summary>

		 /// <returns> <seealso cref="Client"/> for the current <seealso cref="System.Threading.Thread"/>. </returns>
		 CacheAccess_Client Client();

		 /// <summary>
		 /// A flag for record checkers using this cache, where cached values are treated differently if
		 /// we're scanning through a store forwards or backwards.
		 /// </summary>
		 /// <returns> {@code true} if the scanning is currently set to go forward. </returns>
		 bool Forward { get;set; }


		 /// <summary>
		 /// Clears all cached values.
		 /// </summary>
		 void ClearCache();

		 /// <summary>
		 /// Sets the slot sizes of the cached values.
		 /// </summary>
		 /// <param name="slotSize"> defines how many and how big the slots are for cached values that are put after this call. </param>
		 params int[] CacheSlotSizes { set; }

		 void PrepareForProcessingOfSingleStore( long recordsPerCPU );

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 Client EMPTY_CLIENT = new Client()
	//	 {
	//		  @@Override public void putPropertiesToCache(Collection<PropertyRecord> properties)
	//		  {
	//		  }
	//
	//		  @@Override public void putToCache(long id, long... cacheFields)
	//		  {
	//		  }
	//
	//		  @@Override public void putToCacheSingle(long id, int slot, long value)
	//		  {
	//		  }
	//
	//		  @@Override public void clearCache(long id)
	//		  {
	//		  }
	//
	//		  @@Override public void incAndGetCount(Type type)
	//		  {
	//		  }
	//
	//		  @@Override public PropertyRecord getPropertyFromCache(long id)
	//		  {
	//				return null;
	//		  }
	//
	//		  @@Override public Iterable<PropertyRecord> getPropertiesFromCache()
	//		  {
	//				return null;
	//		  }
	//
	//		  @@Override public long getFromCache(long id, int slot)
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public boolean getBooleanFromCache(long id, int slot)
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public boolean withinBounds(long id)
	//		  {
	//				return false;
	//		  }
	//	 };

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 CacheAccess EMPTY = new CacheAccess()
	//	 {
	//		  @@Override public Client client()
	//		  {
	//				return EMPTY_CLIENT;
	//		  }
	//
	//		  @@Override public void setForward(boolean forward)
	//		  {
	//		  }
	//
	//		  @@Override public void setCacheSlotSizes(int... slotSizes)
	//		  {
	//		  }
	//
	//		  @@Override public boolean isForward()
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public void clearCache()
	//		  {
	//		  }
	//
	//		  @@Override public void prepareForProcessingOfSingleStore(long recordsPerCPU)
	//		  {
	//		  }
	//	 };
	}

	 public interface ICacheAccess_Client
	 {
		  /// <summary>
		  /// Gets a cached value, put there with <seealso cref="putToCache(long, long...)"/> or
		  /// <seealso cref="putToCacheSingle(long, int, long)"/>.
		  /// </summary>
		  /// <param name="id"> the IEntity id this cached value is tied to. </param>
		  /// <param name="slot"> which cache slot for this id. </param>
		  /// <returns> the cached value. </returns>
		  long GetFromCache( long id, int slot );

		  /// <summary>
		  /// Gets a cached value, put there with <seealso cref="putToCache(long, long...)"/> or
		  /// <seealso cref="putToCacheSingle(long, int, long)"/> and interpret field value as a boolean.
		  /// 0 will be treated as false all the rest as true.
		  /// </summary>
		  /// <param name="id"> the IEntity id this cached value is tied to. </param>
		  /// <param name="slot"> which cache slot for this id. </param>
		  /// <returns> false if slot value is 0, true otherwise. </returns>
		  bool GetBooleanFromCache( long id, int slot );

		  /// <summary>
		  /// Caches all values for an id, i.e. fills all slots.
		  /// </summary>
		  /// <param name="id"> the IEntity id these cached values will be tied to. </param>
		  /// <param name="cacheFields"> the values to cache, one per slot. </param>
		  void PutToCache( long id, params long[] cacheFields );

		  /// <summary>
		  /// Caches a single value for an id and slot.
		  /// </summary>
		  /// <param name="id"> the IEntity id this cached values will be tied to. </param>
		  /// <param name="slot"> the slot for the given {@code id}. </param>
		  /// <param name="value"> the value to cache for this id and slot. </param>
		  void PutToCacheSingle( long id, int slot, long value );

		  /// <summary>
		  /// Clears the cached values for the specified {@code id}.
		  /// </summary>
		  /// <param name="id"> the IEntity id to clear the cached values for. </param>
		  void ClearCache( long id );

		  /// <summary>
		  /// Caches a <seealso cref="System.Collections.ICollection"/> of <seealso cref="PropertyRecord"/> for later checking.
		  /// </summary>
		  /// <param name="properties"> property records to cache for this thread. </param>
		  void PutPropertiesToCache( ICollection<PropertyRecord> properties );

		  /// <summary>
		  /// Gets a cached <seealso cref="PropertyRecord"/> of a specific {@code id}, see <seealso cref="putPropertiesToCache(System.Collections.ICollection)"/>.
		  /// </summary>
		  /// <param name="id"> the property record id to look for. </param>
		  /// <returns> cached <seealso cref="PropertyRecord"/> <seealso cref="PropertyRecord.getId() id"/>, or {@code null} if not found. </returns>
		  PropertyRecord GetPropertyFromCache( long id );

		  /// <returns> cached properties. </returns>
		  IEnumerable<PropertyRecord> PropertiesFromCache { get; }

		  /// <summary>
		  /// Increases the count of the specified {@code type}, for gathering statistics during a run.
		  /// </summary>
		  /// <param name="type"> counts type. </param>
		  void IncAndGetCount( Counts_Type type );

		  /// <summary>
		  /// Some consistency check stages splits the id range into segments, one per thread.
		  /// That split is initiated by <seealso cref="CacheAccess.prepareForProcessingOfSingleStore(long)"/> and checker,
		  /// per thread, using this method.
		  /// </summary>
		  /// <param name="id"> the record id to check whether or not to process for this thread. </param>
		  /// <returns> {@code true} if the thread represented by this client should process the record
		  /// of the given {@code id}, otherwise {@code false}. </returns>
		  bool WithinBounds( long id );
	 }

}