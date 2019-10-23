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
namespace Neo4Net.Kernel.Impl.Api.index.sampling
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException;
	using IndexProviderDescriptor = Neo4Net.Kernel.Api.Internal.schema.IndexProviderDescriptor;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using CapableIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.CapableIndexDescriptor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using IndexSample = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample;
	using IndexSampler = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSampler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.InternalIndexState.FAILED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.InternalIndexState.ONLINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory.forSchema;

	public class OnlineIndexSamplingJobTest
	{
		private bool InstanceFieldsInitialized = false;

		public OnlineIndexSamplingJobTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_indexDescriptor = forSchema( forLabel( 1, 2 ), IndexProviderDescriptor.UNDECIDED ).withId( _indexId ).withoutCapabilities();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSampleTheIndexAndStoreTheValueWhenTheIndexIsOnline()
		 public virtual void ShouldSampleTheIndexAndStoreTheValueWhenTheIndexIsOnline()
		 {
			  // given
			  OnlineIndexSamplingJob job = new OnlineIndexSamplingJob( _indexId, _indexProxy, _indexStoreView, "Foo", _logProvider );
			  when( _indexProxy.State ).thenReturn( ONLINE );

			  // when
			  job.Run();

			  // then
			  verify( _indexStoreView ).replaceIndexCounts( _indexId, _indexUniqueValues, _indexSize, _indexSize );
			  verifyNoMoreInteractions( _indexStoreView );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSampleTheIndexButDoNotStoreTheValuesIfTheIndexIsNotOnline()
		 public virtual void ShouldSampleTheIndexButDoNotStoreTheValuesIfTheIndexIsNotOnline()
		 {
			  // given
			  OnlineIndexSamplingJob job = new OnlineIndexSamplingJob( _indexId, _indexProxy, _indexStoreView, "Foo", _logProvider );
			  when( _indexProxy.State ).thenReturn( FAILED );

			  // when
			  job.Run();

			  // then
			  verifyNoMoreInteractions( _indexStoreView );
		 }

		 private readonly LogProvider _logProvider = NullLogProvider.Instance;
		 private readonly long _indexId = 1;
		 private readonly IndexProxy _indexProxy = mock( typeof( IndexProxy ) );
		 private readonly IndexStoreView _indexStoreView = mock( typeof( IndexStoreView ) );
		 private CapableIndexDescriptor _indexDescriptor;
		 private readonly IndexReader _indexReader = mock( typeof( IndexReader ) );
		 private readonly IndexSampler _indexSampler = mock( typeof( IndexSampler ) );

		 private readonly long _indexUniqueValues = 21L;
		 private readonly long _indexSize = 23L;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  when( _indexProxy.Descriptor ).thenReturn( _indexDescriptor );
			  when( _indexProxy.newReader() ).thenReturn(_indexReader);
			  when( _indexReader.createSampler() ).thenReturn(_indexSampler);
			  when( _indexSampler.sampleIndex() ).thenReturn(new IndexSample(_indexSize, _indexUniqueValues, _indexSize));
		 }
	}

}