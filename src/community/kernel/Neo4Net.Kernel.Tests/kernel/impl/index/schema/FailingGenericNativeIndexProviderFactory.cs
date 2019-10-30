using System;
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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using IndexCapability = Neo4Net.Kernel.Api.Internal.IndexCapability;
	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using ExtensionType = Neo4Net.Kernel.extension.ExtensionType;
	using Neo4Net.Kernel.extension;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using StoreMigrationParticipant = Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using IndexSample = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.copyOfRange;

	/// <summary>
	/// Testing utility which takes a fully functional <seealso cref="GenericNativeIndexProviderFactory"/> and turns it into a provider which
	/// is guaranteed to fail for various reasons, e.g. failing index population with the goal of creating an index which is in a
	/// <seealso cref="InternalIndexState.FAILED"/> state. To get to this state in high-level testing is surprisingly hard,
	/// so this test utility helps a lot to accomplish this.
	/// 
	/// To be sure to use this provider in your test please do something like:
	/// <pre>
	/// db = new TestGraphDatabaseFactory()
	///     .removeKernelExtensions( TestGraphDatabaseFactory.INDEX_PROVIDERS_FILTER )
	///     .addKernelExtension( new FailingGenericNativeIndexProviderFactory( FailureType.INITIAL_STATE ) )
	///     .newEmbeddedDatabase( dir );
	/// </pre>
	/// </summary>
	public class FailingGenericNativeIndexProviderFactory : KernelExtensionFactory<GenericNativeIndexProviderFactory.Dependencies>
	{
		 public const string INITIAL_STATE_FAILURE_MESSAGE = "Override initial state as failed";
		 public const string POPULATION_FAILURE_MESSAGE = "Fail on update during population";

		 public enum FailureType
		 {
			  Population,
			  InitialState
		 }

		 private readonly GenericNativeIndexProviderFactory _actual;
		 private readonly EnumSet<FailureType> _failureTypes;

		 public FailingGenericNativeIndexProviderFactory( params FailureType[] failureTypes ) : this( new GenericNativeIndexProviderFactory(), 10_000, failureTypes )
		 {
		 }

		 private FailingGenericNativeIndexProviderFactory( GenericNativeIndexProviderFactory actual, int priority, params FailureType[] failureTypes ) : base( ExtensionType.DATABASE, actual.Keys.GetEnumerator().next() )
		 {
			  if ( failureTypes.Length == 0 )
			  {
					throw new System.ArgumentException( "At least one failure type, otherwise there's no point in this provider" );
			  }
			  this._actual = actual;
			  this._failureTypes = EnumSet.of( failureTypes[0], copyOfRange( failureTypes, 1, failureTypes.Length ) );
		 }

		 public override Lifecycle NewInstance( KernelContext context, GenericNativeIndexProviderFactory.Dependencies dependencies )
		 {
			  IndexProvider actualProvider = _actual.newInstance( context, dependencies );
			  return new IndexProviderAnonymousInnerClass( this, actualProvider.ProviderDescriptor, IndexDirectoryStructure.given( actualProvider.DirectoryStructure() ), actualProvider );
		 }

		 private class IndexProviderAnonymousInnerClass : IndexProvider
		 {
			 private readonly FailingGenericNativeIndexProviderFactory _outerInstance;

			 private IndexProvider _actualProvider;

			 public IndexProviderAnonymousInnerClass( FailingGenericNativeIndexProviderFactory outerInstance, Neo4Net.Kernel.Api.Internal.Schema.IndexProviderDescriptor getProviderDescriptor, IndexDirectoryStructure.Factory given, IndexProvider actualProvider ) : base( getProviderDescriptor, given )
			 {
				 this.outerInstance = outerInstance;
				 this._actualProvider = actualProvider;
			 }

			 public override IndexPopulator getPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory )
			 {
				  IndexPopulator actualPopulator = _actualProvider.getPopulator( descriptor, samplingConfig, bufferFactory );
				  if ( _outerInstance.failureTypes.contains( FailureType.Population ) )
				  {
						return new IndexPopulatorAnonymousInnerClass( this, actualPopulator );
				  }
				  return actualPopulator;
			 }

			 private class IndexPopulatorAnonymousInnerClass : IndexPopulator
			 {
				 private readonly IndexProviderAnonymousInnerClass _outerInstance;

				 private IndexPopulator _actualPopulator;

				 public IndexPopulatorAnonymousInnerClass( IndexProviderAnonymousInnerClass outerInstance, IndexPopulator actualPopulator )
				 {
					 this.outerInstance = outerInstance;
					 this._actualPopulator = actualPopulator;
				 }

				 public void create()
				 {
					  _actualPopulator.create();
				 }

				 public void drop()
				 {
					  _actualPopulator.drop();
				 }

				 public void add<T1>( ICollection<T1> updates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
				 {
					  throw new Exception( POPULATION_FAILURE_MESSAGE );
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor nodePropertyAccessor) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
				 public void verifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
				 {
					  _actualPopulator.verifyDeferredConstraints( nodePropertyAccessor );
				 }

				 public IndexUpdater newPopulatingUpdater( NodePropertyAccessor accessor )
				 {
					  return _actualPopulator.newPopulatingUpdater( accessor );
				 }

				 public void close( bool populationCompletedSuccessfully )
				 {
					  _actualPopulator.close( populationCompletedSuccessfully );
				 }

				 public void markAsFailed( string failure )
				 {
					  _actualPopulator.markAsFailed( failure );
				 }

				 public void includeSample<T1>( IndexEntryUpdate<T1> update )
				 {
					  _actualPopulator.includeSample( update );
				 }

				 public IndexSample sampleResult()
				 {
					  return _actualPopulator.sampleResult();
				 }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.kernel.api.index.IndexAccessor getOnlineAccessor(org.Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor descriptor, org.Neo4Net.kernel.impl.api.index.sampling.IndexSamplingConfig samplingConfig) throws java.io.IOException
			 public override IndexAccessor getOnlineAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig )
			 {
				  return _actualProvider.getOnlineAccessor( descriptor, samplingConfig );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getPopulationFailure(org.Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor descriptor) throws IllegalStateException
			 public override string getPopulationFailure( StoreIndexDescriptor descriptor )
			 {
				  return outerInstance.failureTypes.contains( FailureType.InitialState ) ? INITIAL_STATE_FAILURE_MESSAGE : _actualProvider.getPopulationFailure( descriptor );
			 }

			 public override InternalIndexState getInitialState( StoreIndexDescriptor descriptor )
			 {
				  return outerInstance.failureTypes.contains( FailureType.InitialState ) ? InternalIndexState.FAILED : _actualProvider.getInitialState( descriptor );
			 }

			 public override IndexCapability getCapability( StoreIndexDescriptor descriptor )
			 {
				  return _actualProvider.getCapability( descriptor );
			 }

			 public override StoreMigrationParticipant storeMigrationParticipant( FileSystemAbstraction fs, PageCache pageCache )
			 {
				  return _actualProvider.storeMigrationParticipant( fs, pageCache );
			 }
		 }
	}

}