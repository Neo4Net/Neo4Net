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

	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using NodeRelationshipCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;
	using NodeType = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeType;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using StageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl;
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;

	/// <summary>
	/// Links relationship chains together, the "prev" pointers of them. "next" pointers are set when
	/// initially creating the relationship records. Setting prev pointers at that time would incur
	/// random access and so that is done here separately with help from <seealso cref="NodeRelationshipCache"/>.
	/// </summary>
	public abstract class RelationshipLinkStep : ForkedProcessorStep<RelationshipRecord[]>
	{
		 protected internal readonly NodeRelationshipCache Cache;
		 private readonly int _nodeTypes;
		 private readonly System.Predicate<RelationshipRecord> _filter;
		 private readonly bool _forwards;
		 private readonly RelationshipLinkingProgress _progress;

		 public RelationshipLinkStep( StageControl control, Configuration config, NodeRelationshipCache cache, System.Predicate<RelationshipRecord> filter, int nodeTypes, bool forwards, params StatsProvider[] additionalStatsProvider ) : base( control, "LINK", config, additionalStatsProvider )
		 {
			  this.Cache = cache;
			  this._filter = filter;
			  this._nodeTypes = nodeTypes;
			  this._forwards = forwards;
			  this._progress = FindLinkingProgressStatsProvider();
		 }

		 /// <summary>
		 /// There should be a <seealso cref="RelationshipLinkingProgress"/> injected from the outside to better keep track of global
		 /// progress of relationship linking even when linking in multiple passes.
		 /// </summary>
		 private RelationshipLinkingProgress FindLinkingProgressStatsProvider()
		 {
			  foreach ( StatsProvider provider in additionalStatsProvider )
			  {
					if ( provider is RelationshipLinkingProgress )
					{
						 return ( RelationshipLinkingProgress ) provider;
					}
			  }
			  return new RelationshipLinkingProgress();
		 }

		 protected internal override void ForkedProcess( int id, int processors, RelationshipRecord[] batch )
		 {
			  int stride = _forwards ? 1 : -1;
			  int start = _forwards ? 0 : batch.Length - 1;
			  int end = _forwards ? batch.Length : -1;
			  int localChangeCount = 0;
			  for ( int i = start; i != end; i += stride )
			  {
					RelationshipRecord item = batch[i];
					if ( item != null && item.InUse() )
					{
						 int changeCount = Process( item, id, processors );
						 if ( changeCount == -1 )
						 {
							  // No change for this record, it's OK, all the processors will reach the same conclusion
							  batch[i].InUse = false;
						 }
						 else
						 {
							  localChangeCount += changeCount;
						 }
					}
			  }
			  _progress.add( localChangeCount );
		 }

		 public virtual int Process( RelationshipRecord record, int id, int processors )
		 {
			  long startNode = record.FirstNode;
			  long endNode = record.SecondNode;
			  bool processFirst = startNode % processors == id;
			  bool processSecond = endNode % processors == id;
			  int changeCount = 0;
			  if ( !processFirst && !processSecond )
			  {
					// We won't process this relationship, but we cannot return false because that means
					// that it won't even be updated. Arriving here merely means that this thread won't process
					// this record at all and so we won't even have to ask cache about dense or not (which is costly)
					return changeCount;
			  }

			  bool firstIsDense = Cache.isDense( startNode );
			  bool changed = false;
			  bool isLoop = startNode == endNode;
			  if ( isLoop )
			  {
					// Both start/end node
					if ( ShouldChange( firstIsDense, record ) )
					{
						 if ( processFirst )
						 {
							  LinkLoop( record );
							  changeCount += 2;
						 }
						 changed = true;
					}
			  }
			  else
			  {
					// Start node
					if ( ShouldChange( firstIsDense, record ) )
					{
						 if ( processFirst )
						 {
							  LinkStart( record );
							  changeCount++;
						 }
						 changed = true;
					}

					// End node
					bool secondIsDense = Cache.isDense( endNode );
					if ( ShouldChange( secondIsDense, record ) )
					{
						 if ( processSecond )
						 {
							  LinkEnd( record );
							  changeCount++;
						 }
						 changed = true;
					}
			  }

			  return changed ? changeCount : -1;
		 }

		 protected internal abstract void LinkStart( RelationshipRecord record );

		 protected internal abstract void LinkEnd( RelationshipRecord record );

		 protected internal abstract void LinkLoop( RelationshipRecord record );

		 private bool ShouldChange( bool isDense, RelationshipRecord record )
		 {
			  if ( !NodeType.matchesDense( _nodeTypes, isDense ) )
			  {
					return false;
			  }
			  // Here we have a special case where we want to filter on type, but only for dense nodes
			  return !( isDense && _filter != null && !_filter.test( record ) );
		 }
	}

}