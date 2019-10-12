using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Kernel.Impl.Index.Schema.fusion
{

	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using IndexCapability = Neo4Net.@internal.Kernel.Api.IndexCapability;
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using IndexProviderDescriptor = Neo4Net.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using UnionIndexCapability = Neo4Net.Kernel.Impl.Newapi.UnionIndexCapability;
	using StoreMigrationParticipant = Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using ValueCategory = Neo4Net.Values.Storable.ValueCategory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.InternalIndexState.FAILED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.InternalIndexState.POPULATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.IndexSlot.LUCENE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.IndexSlot.NUMBER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.IndexSlot.SPATIAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.IndexSlot.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.IndexSlot.TEMPORAL;

	/// <summary>
	/// This <seealso cref="IndexProvider index provider"/> act as one logical index but is backed by four physical
	/// indexes, the number, spatial, temporal native indexes, and the general purpose lucene index.
	/// </summary>
	public class FusionIndexProvider : IndexProvider
	{
		 private readonly bool _archiveFailedIndex;
		 private readonly InstanceSelector<IndexProvider> _providers;
		 private readonly SlotSelector _slotSelector;
		 private readonly IndexDropAction _dropAction;

		 public FusionIndexProvider( IndexProvider stringProvider, IndexProvider numberProvider, IndexProvider spatialProvider, IndexProvider temporalProvider, IndexProvider luceneProvider, SlotSelector slotSelector, IndexProviderDescriptor descriptor, IndexDirectoryStructure.Factory directoryStructure, FileSystemAbstraction fs, bool archiveFailedIndex ) : base( descriptor, directoryStructure )
		 {
			  this._archiveFailedIndex = archiveFailedIndex;
			  this._slotSelector = slotSelector;
			  this._providers = new InstanceSelector<IndexProvider>();
			  this._dropAction = new FileSystemIndexDropAction( fs, directoryStructure() );
			  FillProvidersSelector( stringProvider, numberProvider, spatialProvider, temporalProvider, luceneProvider );
			  slotSelector.ValidateSatisfied( _providers );
		 }

		 private void FillProvidersSelector( IndexProvider stringProvider, IndexProvider numberProvider, IndexProvider spatialProvider, IndexProvider temporalProvider, IndexProvider luceneProvider )
		 {
			  _providers.put( STRING, stringProvider );
			  _providers.put( NUMBER, numberProvider );
			  _providers.put( SPATIAL, spatialProvider );
			  _providers.put( TEMPORAL, temporalProvider );
			  _providers.put( LUCENE, luceneProvider );
		 }

		 public override IndexPopulator GetPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory )
		 {
			  Dictionary<IndexSlot, IndexPopulator> populators = _providers.map( provider => provider.getPopulator( descriptor, samplingConfig, bufferFactory ) );
			  return new FusionIndexPopulator( _slotSelector, new InstanceSelector<IndexPopulator>( populators ), descriptor.Id, _dropAction, _archiveFailedIndex );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexAccessor getOnlineAccessor(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor, org.neo4j.kernel.impl.api.index.sampling.IndexSamplingConfig samplingConfig) throws java.io.IOException
		 public override IndexAccessor GetOnlineAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig )
		 {
			  Dictionary<IndexSlot, IndexAccessor> accessors = _providers.map( provider => provider.getOnlineAccessor( descriptor, samplingConfig ) );
			  return new FusionIndexAccessor( _slotSelector, new InstanceSelector<IndexAccessor>( accessors ), descriptor, _dropAction );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getPopulationFailure(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws IllegalStateException
		 public override string GetPopulationFailure( StoreIndexDescriptor descriptor )
		 {
			  StringBuilder builder = new StringBuilder();
			  _providers.forAll( p => writeFailure( p.GetType().Name, builder, p, descriptor ) );
			  string failure = builder.ToString();
			  if ( failure.Length > 0 )
			  {
					return failure;
			  }
			  throw new System.InvalidOperationException( "None of the indexes were in a failed state" );
		 }

		 private void WriteFailure( string indexName, StringBuilder builder, IndexProvider provider, StoreIndexDescriptor descriptor )
		 {
			  try
			  {
					string failure = provider.GetPopulationFailure( descriptor );
					builder.Append( indexName );
					builder.Append( ": " );
					builder.Append( failure );
					builder.Append( ' ' );
			  }
			  catch ( System.InvalidOperationException )
			  { // Just catch
			  }
		 }

		 public override InternalIndexState GetInitialState( StoreIndexDescriptor descriptor )
		 {
			  IEnumerable<InternalIndexState> statesIterable = _providers.transform( p => p.getInitialState( descriptor ) );
			  IList<InternalIndexState> states = Iterables.asList( statesIterable );
			  if ( states.Contains( FAILED ) )
			  {
					// One of the state is FAILED, the whole state must be considered FAILED
					return FAILED;
			  }
			  if ( states.Contains( POPULATING ) )
			  {
					// No state is FAILED and one of the state is POPULATING, the whole state must be considered POPULATING
					return POPULATING;
			  }
			  // This means that all parts are ONLINE
			  return InternalIndexState.ONLINE;
		 }

		 public override IndexCapability GetCapability( StoreIndexDescriptor descriptor )
		 {
			  IEnumerable<IndexCapability> capabilities = _providers.transform( indexProvider => indexProvider.getCapability( descriptor ) );
			  return new UnionIndexCapabilityAnonymousInnerClass( this, capabilities );
		 }

		 private class UnionIndexCapabilityAnonymousInnerClass : UnionIndexCapability
		 {
			 private readonly FusionIndexProvider _outerInstance;

			 public UnionIndexCapabilityAnonymousInnerClass( FusionIndexProvider outerInstance, IEnumerable<IndexCapability> capabilities ) : base( capabilities )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override IndexOrder[] orderCapability( params ValueCategory[] valueCategories )
			 {
				  // No order capability when combining results from different indexes
				  if ( valueCategories.Length == 1 && valueCategories[0] == ValueCategory.UNKNOWN )
				  {
						return Neo4Net.@internal.Kernel.Api.IndexCapability_Fields.OrderNone;
				  }
				  // Otherwise union of capabilities
				  return base.orderCapability( valueCategories );
			 }
		 }

		 public override StoreMigrationParticipant StoreMigrationParticipant( FileSystemAbstraction fs, PageCache pageCache )
		 {
			  return Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant_Fields.NotParticipating;
		 }

	}

}