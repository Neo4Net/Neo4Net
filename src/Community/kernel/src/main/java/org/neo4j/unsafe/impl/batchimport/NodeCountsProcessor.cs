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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using StatementConstants = Neo4Net.Kernel.api.StatementConstants;
	using CountsAccessor = Neo4Net.Kernel.Impl.Api.CountsAccessor;
	using NodeLabelsField = Neo4Net.Kernel.impl.store.NodeLabelsField;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using NodeLabelsCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeLabelsCache;

	/// <summary>
	/// Calculates counts per label and puts data into <seealso cref="NodeLabelsCache"/> for use by {@link
	/// RelationshipCountsProcessor}.
	/// </summary>
	public class NodeCountsProcessor : RecordProcessor<NodeRecord>
	{
		 private readonly NodeStore _nodeStore;
		 private readonly long[] _labelCounts;
		 private ProgressReporter _progressReporter;
		 private readonly NodeLabelsCache _cache;
		 private readonly Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater _counts;
		 private readonly int _anyLabel;

		 internal NodeCountsProcessor( NodeStore nodeStore, NodeLabelsCache cache, int highLabelId, Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater counts, ProgressReporter progressReporter )
		 {
			  this._nodeStore = nodeStore;
			  this._cache = cache;
			  this._anyLabel = highLabelId;
			  this._counts = counts;
			  // Instantiate with high id + 1 since we need that extra slot for the ANY count
			  this._labelCounts = new long[highLabelId + 1];
			  this._progressReporter = progressReporter;
		 }

		 public override bool Process( NodeRecord node )
		 {
			  long[] labels = NodeLabelsField.get( node, _nodeStore );
			  if ( labels.Length > 0 )
			  {
					foreach ( long labelId in labels )
					{
						 _labelCounts[( int ) labelId]++;
					}
					_cache.put( node.Id, labels );
			  }
			  _labelCounts[_anyLabel]++;
			  _progressReporter.progress( 1 );

			  // No need to update the store, we're just reading things here
			  return false;
		 }

		 public override void Done()
		 {
			  for ( int i = 0; i < _labelCounts.Length; i++ )
			  {
					_counts.incrementNodeCount( i == _anyLabel ? StatementConstants.ANY_LABEL : i, _labelCounts[i] );
			  }
		 }

		 public override void Close()
		 {
		 }
	}

}