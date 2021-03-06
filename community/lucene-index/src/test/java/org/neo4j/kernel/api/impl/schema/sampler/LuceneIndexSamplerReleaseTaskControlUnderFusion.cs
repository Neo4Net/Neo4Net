﻿using System;

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
namespace Org.Neo4j.Kernel.Api.Impl.Schema.sampler
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using TaskCoordinator = Org.Neo4j.Helpers.TaskCoordinator;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using DirectoryFactory = Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using IndexAccessor = Org.Neo4j.Kernel.Api.Index.IndexAccessor;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexProxyAdapter = Org.Neo4j.Kernel.Impl.Api.index.IndexProxyAdapter;
	using IndexStoreView = Org.Neo4j.Kernel.Impl.Api.index.IndexStoreView;
	using IndexUpdateMode = Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using IndexSamplingJob = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingJob;
	using OnlineIndexSamplingJobFactory = Org.Neo4j.Kernel.Impl.Api.index.sampling.OnlineIndexSamplingJobFactory;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using FusionIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.fusion.FusionIndexProvider;
	using SlotSelector = Org.Neo4j.Kernel.Impl.Index.Schema.fusion.SlotSelector;
	using CapableIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using IndexSampler = Org.Neo4j.Storageengine.Api.schema.IndexSampler;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;
	using Org.Neo4j.Test.rule.fs;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexCapability.NO_CAPABILITY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.LuceneIndexProvider.defaultDirectoryStructure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexProvider.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaTestUtil.simpleNameLookup;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.NullLogProvider.getInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema;

	public class LuceneIndexSamplerReleaseTaskControlUnderFusion
	{
		private bool InstanceFieldsInitialized = false;

		public LuceneIndexSamplerReleaseTaskControlUnderFusion()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Dir = TestDirectory.testDirectory( Fs.get() );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.FileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public FileSystemRule Fs = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory dir = org.neo4j.test.rule.TestDirectory.testDirectory(fs.get());
		 public TestDirectory Dir;

		 private const int INDEX_ID = 1;
		 private static readonly StoreIndexDescriptor _storeIndexDescriptor = forSchema( forLabel( 1, 1 ) ).withId( INDEX_ID );
		 private static readonly CapableIndexDescriptor _capableIndexDescriptor = new CapableIndexDescriptor( _storeIndexDescriptor, NO_CAPABILITY );
		 private static readonly IndexProviderDescriptor _providerDescriptor = IndexProviderDescriptor.UNDECIDED;
		 private static readonly Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory _luceneDirectoryFactory = new Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory();
		 private static readonly Config _config = Config.defaults();
		 private static readonly IndexSamplingConfig _samplingConfig = new IndexSamplingConfig( _config );
		 private static readonly Exception _sampleException = new Exception( "Killroy messed with your index sample." );
		 private IndexDirectoryStructure.Factory _directoryFactory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _directoryFactory = defaultDirectoryStructure( Dir.storeDir() );
		 }

		 /// <summary>
		 /// This test come from a support case where dropping an index would block forever after index sampling failed.
		 /// <para>
		 /// A fusion index has multiple <seealso cref="IndexSampler index samplers"/> that are called sequentially. If one fails, then the other will never be invoked.
		 /// This was a problem for <seealso cref="LuceneIndexSampler"/>. It owns a <seealso cref="org.neo4j.helpers.TaskControl"/> that it will try to release in try-finally
		 /// in <seealso cref="LuceneIndexSampler.sampleIndex()"/>. But it never gets here because a prior <seealso cref="IndexSampler"/> fails.
		 /// </para>
		 /// <para>
		 /// Because the <seealso cref="org.neo4j.helpers.TaskControl"/> was never released the lucene accessor would block forever, waiting for
		 /// <seealso cref="TaskCoordinator.awaitCompletion()"/>.
		 /// </para>
		 /// <para>
		 /// This situation was solved by making <seealso cref="IndexSampler"/> <seealso cref="System.IDisposable"/> and include it in try-with-resource together with
		 /// <seealso cref="IndexReader"/> that created it.
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5_000L) public void failedIndexSamplingMustNotPreventIndexDrop() throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailedIndexSamplingMustNotPreventIndexDrop()
		 {
			  LuceneIndexProvider luceneProvider = luceneProvider();
			  MakeSureIndexHasSomeData( luceneProvider ); // Otherwise no sampler will be created.

			  IndexProvider failingProvider = failingProvider();
			  FusionIndexProvider fusionProvider = CreateFusionProvider( luceneProvider, failingProvider );
			  using ( IndexAccessor fusionAccessor = fusionProvider.GetOnlineAccessor( _storeIndexDescriptor, _samplingConfig ) )
			  {
					IndexSamplingJob indexSamplingJob = CreateIndexSamplingJob( fusionAccessor );

					// Call run from other thread
					try
					{
						 indexSamplingJob.run();
					}
					catch ( Exception e )
					{
						 assertSame( e, _sampleException );
					}

					// then
					fusionAccessor.Drop();
					// should not block forever
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void makeSureIndexHasSomeData(org.neo4j.kernel.api.index.IndexProvider provider) throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private void MakeSureIndexHasSomeData( IndexProvider provider )
		 {
			  using ( IndexAccessor accessor = provider.GetOnlineAccessor( _storeIndexDescriptor, _samplingConfig ), IndexUpdater updater = accessor.NewUpdater( IndexUpdateMode.ONLINE ) )
			  {
					updater.Process( IndexEntryUpdate.add( 1, _storeIndexDescriptor, Values.of( "some string" ) ) );
			  }
		 }

		 private FusionIndexProvider CreateFusionProvider( LuceneIndexProvider luceneProvider, IndexProvider failingProvider )
		 {
			  SlotSelector slotSelector = Org.Neo4j.Kernel.Impl.Index.Schema.fusion.SlotSelector_Fields.NullInstance;
			  return new FusionIndexProvider( failingProvider, EMPTY, EMPTY, EMPTY, luceneProvider, slotSelector, _providerDescriptor, _directoryFactory, Fs.get(), false );
		 }

		 private IndexSamplingJob CreateIndexSamplingJob( IndexAccessor fusionAccessor )
		 {
			  IndexStoreView storeView = Org.Neo4j.Kernel.Impl.Api.index.IndexStoreView_Fields.Empty;
			  IndexProxyAdapter indexProxy = new IndexProxyAdapterAnonymousInnerClass( this, fusionAccessor );
			  OnlineIndexSamplingJobFactory onlineIndexSamplingJobFactory = new OnlineIndexSamplingJobFactory( storeView, simpleNameLookup, Instance );
			  return onlineIndexSamplingJobFactory.Create( 1, indexProxy );
		 }

		 private class IndexProxyAdapterAnonymousInnerClass : IndexProxyAdapter
		 {
			 private readonly LuceneIndexSamplerReleaseTaskControlUnderFusion _outerInstance;

			 private IndexAccessor _fusionAccessor;

			 public IndexProxyAdapterAnonymousInnerClass( LuceneIndexSamplerReleaseTaskControlUnderFusion outerInstance, IndexAccessor fusionAccessor )
			 {
				 this.outerInstance = outerInstance;
				 this._fusionAccessor = fusionAccessor;
			 }

			 public override CapableIndexDescriptor Descriptor
			 {
				 get
				 {
					  return _capableIndexDescriptor;
				 }
			 }

			 public override IndexReader newReader()
			 {
				  return _fusionAccessor.newReader();
			 }
		 }

		 private LuceneIndexProvider LuceneProvider()
		 {
			  return new LuceneIndexProvider( Fs.get(), _luceneDirectoryFactory, _directoryFactory, IndexProvider.Monitor_Fields.EMPTY, _config, OperationalMode.single );
		 }

		 /// <returns> an <seealso cref="IndexProvider"/> that create an <seealso cref="IndexAccessor"/> that create an <seealso cref="IndexReader"/> that create an <seealso cref="IndexSampler"/> that
		 /// throws exception... yeah. </returns>
		 private IndexProvider FailingProvider()
		 {
			  return new AdaptorAnonymousInnerClass( this, _providerDescriptor, _directoryFactory );
		 }

		 private class AdaptorAnonymousInnerClass : IndexProvider.Adaptor
		 {
			 private readonly LuceneIndexSamplerReleaseTaskControlUnderFusion _outerInstance;

			 public AdaptorAnonymousInnerClass( LuceneIndexSamplerReleaseTaskControlUnderFusion outerInstance, IndexProviderDescriptor providerDescriptor, IndexDirectoryStructure.Factory directoryFactory ) : base( providerDescriptor, directoryFactory )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override IndexAccessor getOnlineAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig )
			 {
				  return outerInstance.failingIndexAccessor();
			 }
		 }

		 private IndexAccessor FailingIndexAccessor()
		 {
			  return new IndexAccessor_AdapterAnonymousInnerClass( this );
		 }

		 private class IndexAccessor_AdapterAnonymousInnerClass : Org.Neo4j.Kernel.Api.Index.IndexAccessor_Adapter
		 {
			 private readonly LuceneIndexSamplerReleaseTaskControlUnderFusion _outerInstance;

			 public IndexAccessor_AdapterAnonymousInnerClass( LuceneIndexSamplerReleaseTaskControlUnderFusion outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override IndexReader newReader()
			 {
				  return new IndexReader_AdaptorAnonymousInnerClass( this );
			 }

			 private class IndexReader_AdaptorAnonymousInnerClass : Org.Neo4j.Storageengine.Api.schema.IndexReader_Adaptor
			 {
				 private readonly IndexAccessor_AdapterAnonymousInnerClass _outerInstance;

				 public IndexReader_AdaptorAnonymousInnerClass( IndexAccessor_AdapterAnonymousInnerClass outerInstance )
				 {
					 this.outerInstance = outerInstance;
				 }

				 public override IndexSampler createSampler()
				 {
					  return () =>
					  {
						throw _sampleException;
					  };
				 }
			 }
		 }
	}

}