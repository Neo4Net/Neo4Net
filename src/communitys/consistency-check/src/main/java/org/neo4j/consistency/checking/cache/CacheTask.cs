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
	using ConsistencyCheckerTask = Neo4Net.Consistency.checking.full.ConsistencyCheckerTask;
	using Stage = Neo4Net.Consistency.checking.full.Stage;
	using StoreProcessor = Neo4Net.Consistency.checking.full.StoreProcessor;
	using Statistics = Neo4Net.Consistency.statistics.Statistics;
	using Neo4Net.Graphdb;
	using Neo4Net.Graphdb;
	using Neo4Net.Kernel.impl.store;
	using StoreAccess = Neo4Net.Kernel.impl.store.StoreAccess;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;

	/// <summary>
	/// Action to be manipulate the <seealso cref="CacheAccess"/> in some way.
	/// </summary>
	public abstract class CacheTask : ConsistencyCheckerTask
	{
		 protected internal readonly Stage Stage;
		 protected internal readonly CacheAccess CacheAccess;

		 public CacheTask( Stage stage, CacheAccess cacheAccess ) : base( "CacheTask-" + stage, Statistics.NONE, 1 )
		 {
			  this.Stage = stage;
			  this.CacheAccess = cacheAccess;
		 }

		 public override void Run()
		 {
			  if ( Stage.CacheSlotSizes.Length > 0 )
			  {
					CacheAccess.CacheSlotSizes = Stage.CacheSlotSizes;
			  }
			  ProcessCache();
		 }

		 protected internal abstract void ProcessCache();

		 public class CacheNextRel : CacheTask
		 {
			  internal readonly ResourceIterable<NodeRecord> Nodes;

			  public CacheNextRel( Stage stage, CacheAccess cacheAccess, ResourceIterable<NodeRecord> nodes ) : base( stage, cacheAccess )
			  {
					this.Nodes = nodes;
			  }

			  protected internal override void ProcessCache()
			  {
					CacheAccess.clearCache();
					long[] fields = new long[] { -1, 1, 0 };
					CacheAccess_Client client = CacheAccess.client();
					using ( ResourceIterator<NodeRecord> nodeRecords = Nodes.GetEnumerator() )
					{
						 while ( nodeRecords.MoveNext() )
						 {
							  NodeRecord node = nodeRecords.Current;
							  if ( node.InUse() )
							  {
									fields[CacheSlots_NextRelationship_Fields.SLOT_RELATIONSHIP_ID] = node.NextRel;
									client.PutToCache( node.Id, fields );
							  }
						 }
					}
			  }
		 }

		 public class CheckNextRel : CacheTask
		 {
			  internal readonly StoreAccess StoreAccess;
			  internal readonly StoreProcessor StoreProcessor;

			  public CheckNextRel( Stage stage, CacheAccess cacheAccess, StoreAccess storeAccess, StoreProcessor storeProcessor ) : base( stage, cacheAccess )
			  {
					this.StoreAccess = storeAccess;
					this.StoreProcessor = storeProcessor;
			  }

			  protected internal override void ProcessCache()
			  {
					RecordStore<NodeRecord> nodeStore = StoreAccess.NodeStore;
					CacheAccess_Client client = CacheAccess.client();
					long highId = nodeStore.HighId;
					for ( long nodeId = 0; nodeId < highId; nodeId++ )
					{
						 if ( client.GetFromCache( nodeId, CacheSlots_NextRelationship_Fields.SLOT_FIRST_IN_TARGET ) == 0 )
						 {
							  NodeRecord node = nodeStore.GetRecord( nodeId, nodeStore.NewRecord(), FORCE );
							  if ( node.InUse() && !node.Dense )
							  {
									StoreProcessor.processNode( nodeStore, node );
							  }
						 }
					}
			  }
		 }
	}

}