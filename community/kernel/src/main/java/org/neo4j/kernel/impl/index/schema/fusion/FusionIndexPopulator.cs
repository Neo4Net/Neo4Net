using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema.fusion
{

	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexConfigProvider = Org.Neo4j.Kernel.Api.Index.IndexConfigProvider;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexPopulator = Org.Neo4j.Kernel.Api.Index.IndexPopulator;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexSample = Org.Neo4j.Storageengine.Api.schema.IndexSample;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexSampler.combineSamples;

	internal class FusionIndexPopulator : FusionIndexBase<IndexPopulator>, IndexPopulator
	{
		 private readonly long _indexId;
		 private readonly IndexDropAction _dropAction;
		 private readonly bool _archiveFailedIndex;

		 internal FusionIndexPopulator( SlotSelector slotSelector, InstanceSelector<IndexPopulator> instanceSelector, long indexId, IndexDropAction dropAction, bool archiveFailedIndex ) : base( slotSelector, instanceSelector )
		 {
			  this._indexId = indexId;
			  this._dropAction = dropAction;
			  this._archiveFailedIndex = archiveFailedIndex;
		 }

		 public override void Create()
		 {
			  _dropAction.drop( _indexId, _archiveFailedIndex );
			  InstanceSelector.forAll( IndexPopulator.create );
		 }

		 public override void Drop()
		 {
			  InstanceSelector.forAll( IndexPopulator.drop );
			  _dropAction.drop( _indexId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void add(java.util.Collection<? extends org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Add<T1>( ICollection<T1> updates ) where T1 : Org.Neo4j.Kernel.Api.Index.IndexEntryUpdate<T1>
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: LazyInstanceSelector<java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>>> batchSelector = new LazyInstanceSelector<>(slot -> new java.util.ArrayList<>());
			  LazyInstanceSelector<ICollection<IndexEntryUpdate<object>>> batchSelector = new LazyInstanceSelector<ICollection<IndexEntryUpdate<object>>>( slot => new List<>() );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.api.index.IndexEntryUpdate<?> update : updates)
			  foreach ( IndexEntryUpdate<object> update in updates )
			  {
					batchSelector.Select( SlotSelector.selectSlot( update.Values(), GroupOf ) ).Add(update);
			  }

			  // Manual loop due do multiple exception types
			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> batch = batchSelector.getIfInstantiated(slot);
					ICollection<IndexEntryUpdate<object>> batch = batchSelector.GetIfInstantiated( slot );
					if ( batch != null )
					{
						 this.InstanceSelector.select( slot ).add( batch );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		 {
			  // Manual loop due do multiple exception types
			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					InstanceSelector.select( slot ).verifyDeferredConstraints( nodePropertyAccessor );
			  }
		 }

		 public override IndexUpdater NewPopulatingUpdater( NodePropertyAccessor accessor )
		 {
			  LazyInstanceSelector<IndexUpdater> updaterSelector = new LazyInstanceSelector<IndexUpdater>( slot => InstanceSelector.select( slot ).newPopulatingUpdater( accessor ) );
			  return new FusionIndexUpdater( SlotSelector, updaterSelector );
		 }

		 public override void Close( bool populationCompletedSuccessfully )
		 {
			  InstanceSelector.close( populator => populator.close( populationCompletedSuccessfully ) );
		 }

		 public override void MarkAsFailed( string failure )
		 {
			  InstanceSelector.forAll( populator => populator.markAsFailed( failure ) );
		 }

		 public override void IncludeSample<T1>( IndexEntryUpdate<T1> update )
		 {
			  InstanceSelector.select( SlotSelector.selectSlot( update.Values(), GroupOf ) ).includeSample(update);
		 }

		 public override IndexSample SampleResult()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return combineSamples( InstanceSelector.transform( IndexPopulator::sampleResult ) );
		 }

		 public override IDictionary<string, Value> IndexConfig()
		 {
			  IDictionary<string, Value> indexConfig = new Dictionary<string, Value>();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  InstanceSelector.transform( IndexPopulator::indexConfig ).forEach( source => IndexConfigProvider.putAllNoOverwrite( indexConfig, source ) );
			  return indexConfig;
		 }
	}

}