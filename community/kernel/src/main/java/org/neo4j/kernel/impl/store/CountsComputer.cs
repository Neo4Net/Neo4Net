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
namespace Org.Neo4j.Kernel.impl.store
{
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using CountsAccessor = Org.Neo4j.Kernel.Impl.Api.CountsAccessor;
	using CountsTracker = Org.Neo4j.Kernel.impl.store.counts.CountsTracker;
	using Org.Neo4j.Kernel.impl.store.kvstore;
	using ProgressReporter = Org.Neo4j.Kernel.impl.util.monitoring.ProgressReporter;
	using SilentProgressReporter = Org.Neo4j.Kernel.impl.util.monitoring.SilentProgressReporter;
	using Configuration = Org.Neo4j.@unsafe.Impl.Batchimport.Configuration;
	using NodeCountsStage = Org.Neo4j.@unsafe.Impl.Batchimport.NodeCountsStage;
	using RelationshipCountsStage = Org.Neo4j.@unsafe.Impl.Batchimport.RelationshipCountsStage;
	using NodeLabelsCache = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NodeLabelsCache;
	using NumberArrayFactory = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.ExecutionSupervisors.superviseDynamicExecution;

	public class CountsComputer : DataInitializer<Org.Neo4j.Kernel.Impl.Api.CountsAccessor_Updater>
	{
		 public static void RecomputeCounts( NeoStores stores, PageCache pageCache, DatabaseLayout databaseLayout )
		 {
			  MetaDataStore metaDataStore = stores.MetaDataStore;
			  CountsTracker counts = stores.Counts;
			  using ( Org.Neo4j.Kernel.Impl.Api.CountsAccessor_Updater updater = Counts.reset( metaDataStore.LastCommittedTransactionId ) )
			  {
					( new CountsComputer( stores, pageCache, databaseLayout ) ).Initialize( updater );
			  }
		 }

		 private readonly NodeStore _nodes;
		 private readonly RelationshipStore _relationships;
		 private readonly int _highLabelId;
		 private readonly int _highRelationshipTypeId;
		 private readonly long _lastCommittedTransactionId;
		 private readonly ProgressReporter _progressMonitor;
		 private readonly NumberArrayFactory _numberArrayFactory;

		 internal CountsComputer( NeoStores stores, PageCache pageCache, DatabaseLayout databaseLayout ) : this( stores.MetaDataStore.LastCommittedTransactionId, stores.NodeStore, stores.RelationshipStore, ( int ) stores.LabelTokenStore.HighId, ( int ) stores.RelationshipTypeTokenStore.HighId, NumberArrayFactory.auto( pageCache, databaseLayout.DatabaseDirectory(), true, org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.NoMonitor ) )
		 {
		 }

		 private CountsComputer( long lastCommittedTransactionId, NodeStore nodes, RelationshipStore relationships, int highLabelId, int highRelationshipTypeId, NumberArrayFactory numberArrayFactory ) : this( lastCommittedTransactionId, nodes, relationships, highLabelId, highRelationshipTypeId, numberArrayFactory, SilentProgressReporter.INSTANCE )
		 {
		 }

		 public CountsComputer( long lastCommittedTransactionId, NodeStore nodes, RelationshipStore relationships, int highLabelId, int highRelationshipTypeId, NumberArrayFactory numberArrayFactory, ProgressReporter progressMonitor )
		 {
			  this._lastCommittedTransactionId = lastCommittedTransactionId;
			  this._nodes = nodes;
			  this._relationships = relationships;
			  this._highLabelId = highLabelId;
			  this._highRelationshipTypeId = highRelationshipTypeId;
			  this._numberArrayFactory = numberArrayFactory;
			  this._progressMonitor = progressMonitor;
		 }

		 public override void Initialize( Org.Neo4j.Kernel.Impl.Api.CountsAccessor_Updater countsUpdater )
		 {
			  if ( HasNotEmptyNodesOrRelationshipsStores() )
			  {
					_progressMonitor.start( _nodes.HighestPossibleIdInUse + _relationships.HighestPossibleIdInUse );
					PopulateCountStore( countsUpdater );
			  }
			  _progressMonitor.completed();
		 }

		 private bool HasNotEmptyNodesOrRelationshipsStores()
		 {
			  return ( _nodes.HighestPossibleIdInUse != -1 ) || ( _relationships.HighestPossibleIdInUse != -1 );
		 }

		 private void PopulateCountStore( Org.Neo4j.Kernel.Impl.Api.CountsAccessor_Updater countsUpdater )
		 {
			  using ( NodeLabelsCache cache = new NodeLabelsCache( _numberArrayFactory, _highLabelId ) )
			  {
					// Count nodes
					superviseDynamicExecution( new NodeCountsStage( Configuration.DEFAULT, cache, _nodes, _highLabelId, countsUpdater, _progressMonitor ) );
					// Count relationships
					superviseDynamicExecution( new RelationshipCountsStage( Configuration.DEFAULT, cache, _relationships, _highLabelId, _highRelationshipTypeId, countsUpdater, _numberArrayFactory, _progressMonitor ) );
			  }
		 }

		 public override long InitialVersion()
		 {
			  return _lastCommittedTransactionId;
		 }
	}

}