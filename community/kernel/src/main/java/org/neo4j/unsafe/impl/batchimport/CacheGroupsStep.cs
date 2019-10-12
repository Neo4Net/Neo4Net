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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using BatchSender = Org.Neo4j.@unsafe.Impl.Batchimport.staging.BatchSender;
	using Org.Neo4j.@unsafe.Impl.Batchimport.staging;
	using StageControl = Org.Neo4j.@unsafe.Impl.Batchimport.staging.StageControl;
	using StatsProvider = Org.Neo4j.@unsafe.Impl.Batchimport.stats.StatsProvider;

	/// <summary>
	/// Caches <seealso cref="RelationshipGroupRecord"/> into <seealso cref="RelationshipGroupCache"/>.
	/// </summary>
	public class CacheGroupsStep : ProcessorStep<RelationshipGroupRecord[]>
	{
		 private readonly RelationshipGroupCache _cache;

		 public CacheGroupsStep( StageControl control, Configuration config, RelationshipGroupCache cache, params StatsProvider[] additionalStatsProviders ) : base( control, "CACHE", config, 1, additionalStatsProviders )
		 {
			  this._cache = cache;
		 }

		 protected internal override void Process( RelationshipGroupRecord[] batch, BatchSender sender )
		 {
			  // These records are read page-wise forwards, but should be cached in reverse
			  // since the records exists in the store in reverse order.
			  for ( int i = batch.Length - 1; i >= 0; i-- )
			  {
					RelationshipGroupRecord record = batch[i];
					if ( record.InUse() )
					{
						 _cache.put( record );
					}
			  }
		 }
	}

}