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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using Test = org.junit.Test;

	using IndexProviderDescriptor = Neo4Net.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	public class OnlineIndexProxyTest
	{
		 private readonly long _indexId = 1;
		 private readonly IndexDescriptor _descriptor = TestIndexDescriptorFactory.forLabel( 1, 2 );
		 private readonly IndexProviderDescriptor _providerDescriptor = mock( typeof( IndexProviderDescriptor ) );
		 private readonly IndexAccessor _accessor = mock( typeof( IndexAccessor ) );
		 private readonly IndexStoreView _storeView = mock( typeof( IndexStoreView ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveIndexCountsWhenTheIndexItselfIsDropped() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveIndexCountsWhenTheIndexItselfIsDropped()
		 {
			  // given
			  CapableIndexDescriptor capableIndexDescriptor = _descriptor.withId( _indexId ).withoutCapabilities();
			  OnlineIndexProxy index = new OnlineIndexProxy( capableIndexDescriptor, _accessor, _storeView, false );

			  // when
			  index.Drop();

			  // then
			  verify( _accessor ).drop();
			  verify( _storeView ).replaceIndexCounts( _indexId, 0L, 0L, 0L );
			  verifyNoMoreInteractions( _accessor, _storeView );
		 }
	}

}