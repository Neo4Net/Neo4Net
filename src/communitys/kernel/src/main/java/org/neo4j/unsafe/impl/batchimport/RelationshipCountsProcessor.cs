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
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using LongArray = Neo4Net.@unsafe.Impl.Batchimport.cache.LongArray;
	using NodeLabelsCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeLabelsCache;
	using NumberArrayFactory = Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;

	/// <summary>
	/// Calculates counts as labelId --[type]--> labelId for relationships with the labels coming from its start/end nodes.
	/// </summary>
	public class RelationshipCountsProcessor : RecordProcessor<RelationshipRecord>
	{
		 private readonly NodeLabelsCache _nodeLabelCache;
		 private readonly LongArray _labelsCounts;
		 private readonly LongArray _wildcardCounts;

		 // and grows on demand
		 private int[] _startScratch = new int[20];
		 private int[] _endScratch = new int[20];
		 private readonly Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater _countsUpdater;
		 private readonly long _anyLabel;
		 private readonly long _anyRelationshipType;
		 private readonly NodeLabelsCache.Client _client;
		 private readonly long _itemsPerLabel;
		 private readonly long _itemsPerType;

		 private const int START = 0;
		 private const int END = 1;
		 private const int SIDES = 2;

		 public RelationshipCountsProcessor( NodeLabelsCache nodeLabelCache, int highLabelId, int highRelationshipTypeId, Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater countsUpdater, NumberArrayFactory cacheFactory )
		 {
			  this._nodeLabelCache = nodeLabelCache;
			  this._client = nodeLabelCache.NewClient();
			  this._countsUpdater = countsUpdater;

			  // Make room for high id + 1 since we need that extra slot for the ANY counts
			  this._anyLabel = highLabelId;
			  this._anyRelationshipType = highRelationshipTypeId;
			  this._itemsPerType = _anyLabel + 1;
			  this._itemsPerLabel = _anyRelationshipType + 1;
			  this._labelsCounts = cacheFactory.NewLongArray( SideSize() * SIDES, 0 );
			  this._wildcardCounts = cacheFactory.NewLongArray( _anyRelationshipType + 1, 0 );
		 }

		 internal static long CalculateMemoryUsage( int highLabelId, int highRelationshipTypeId )
		 {
			  int labels = highLabelId + 1;
			  int types = highRelationshipTypeId + 1;
			  long labelsCountsUsage = labels * types * SIDES * Long.BYTES;
			  long wildcardCountsUsage = types * Long.BYTES;
			  return labelsCountsUsage + wildcardCountsUsage;
		 }

		 public override bool Process( RelationshipRecord record )
		 {
			  // Below is logic duplication of CountsState#addRelationship
			  int type = record.Type;
			  Increment( _wildcardCounts, _anyRelationshipType );
			  Increment( _wildcardCounts, type );
			  _startScratch = _nodeLabelCache.get( _client, record.FirstNode, _startScratch );
			  foreach ( int startNodeLabelId in _startScratch )
			  {
					if ( startNodeLabelId == -1 )
					{ // We reached the end of it
						 break;
					}

					Increment( _labelsCounts, startNodeLabelId, _anyRelationshipType, START );
					Increment( _labelsCounts, startNodeLabelId, type, START );
			  }
			  _endScratch = _nodeLabelCache.get( _client, record.SecondNode, _endScratch );
			  foreach ( int endNodeLabelId in _endScratch )
			  {
					if ( endNodeLabelId == -1 )
					{ // We reached the end of it
						 break;
					}

					Increment( _labelsCounts, endNodeLabelId, _anyRelationshipType, END );
					Increment( _labelsCounts, endNodeLabelId, type, END );
			  }
			  return false;
		 }

		 public override void Done()
		 {
			  for ( int wildcardType = 0; wildcardType <= _anyRelationshipType; wildcardType++ )
			  {
					int type = wildcardType == _anyRelationshipType ? StatementConstants.ANY_RELATIONSHIP_TYPE : wildcardType;
					long count = _wildcardCounts.get( wildcardType );
					_countsUpdater.incrementRelationshipCount( StatementConstants.ANY_LABEL, type, StatementConstants.ANY_LABEL, count );
			  }

			  for ( int labelId = 0; labelId < _anyLabel; labelId++ )
			  {
					for ( int typeId = 0; typeId <= _anyRelationshipType; typeId++ )
					{

						 long startCount = _labelsCounts.get( ArrayIndex( labelId, typeId, START ) );
						 long endCount = _labelsCounts.get( ArrayIndex( labelId, typeId, END ) );
						 int type = typeId == _anyRelationshipType ? StatementConstants.ANY_RELATIONSHIP_TYPE : typeId;

						 _countsUpdater.incrementRelationshipCount( labelId, type, StatementConstants.ANY_LABEL, startCount );
						 _countsUpdater.incrementRelationshipCount( StatementConstants.ANY_LABEL, type, labelId, endCount );
					}
			  }
		 }

		 public override void Close()
		 {
			  _labelsCounts.close();
			  _wildcardCounts.close();
		 }

		 public virtual void AddCountsFrom( RelationshipCountsProcessor from )
		 {
			  MergeCounts( _labelsCounts, from._labelsCounts );
			  MergeCounts( _wildcardCounts, from._wildcardCounts );
		 }

		 private void MergeCounts( LongArray destination, LongArray part )
		 {
			  long length = destination.length();
			  for ( long i = 0; i < length; i++ )
			  {
					destination.Set( i, destination.Get( i ) + part.Get( i ) );
			  }
		 }

		 private long ArrayIndex( long labelId, long relationshipTypeId, long side )
		 {
			  return ( side * SideSize() ) + (labelId * _itemsPerLabel + relationshipTypeId);
		 }

		 private long SideSize()
		 {
			  return _itemsPerType * _itemsPerLabel;
		 }

		 private void Increment( LongArray counts, long labelId, long relationshipTypeId, long side )
		 {
			  long index = ArrayIndex( labelId, relationshipTypeId, side );
			  Increment( counts, index );
		 }

		 private void Increment( LongArray counts, long index )
		 {
			  Counts.set( index, Counts.get( index ) + 1 );
		 }
	}

}