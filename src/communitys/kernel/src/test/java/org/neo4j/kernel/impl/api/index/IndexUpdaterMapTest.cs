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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.TestIndexProviderDescriptor.PROVIDER_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema;

	public class IndexUpdaterMapTest
	{
		 private IndexMap _indexMap;

		 private IndexProxy _indexProxy1;
		 private CapableIndexDescriptor _schemaIndexDescriptor1;
		 private IndexUpdater _indexUpdater1;

		 private IndexProxy _indexProxy2;
		 private CapableIndexDescriptor _schemaIndexDescriptor2;

		 private IndexProxy _indexProxy3;
		 private CapableIndexDescriptor _schemaIndexDescriptor3;

		 private IndexUpdaterMap _updaterMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _indexMap = new IndexMap();

			  _indexProxy1 = mock( typeof( IndexProxy ) );
			  _schemaIndexDescriptor1 = forSchema( forLabel( 2, 3 ), PROVIDER_DESCRIPTOR ).withId( 0 ).withoutCapabilities();
			  _indexUpdater1 = mock( typeof( IndexUpdater ) );
			  when( _indexProxy1.Descriptor ).thenReturn( _schemaIndexDescriptor1 );
			  when( _indexProxy1.newUpdater( any( typeof( IndexUpdateMode ) ) ) ).thenReturn( _indexUpdater1 );

			  _indexProxy2 = mock( typeof( IndexProxy ) );
			  _schemaIndexDescriptor2 = forSchema( forLabel( 5, 6 ), PROVIDER_DESCRIPTOR ).withId( 1 ).withoutCapabilities();
			  IndexUpdater indexUpdater2 = mock( typeof( IndexUpdater ) );
			  when( _indexProxy2.Descriptor ).thenReturn( _schemaIndexDescriptor2 );
			  when( _indexProxy2.newUpdater( any( typeof( IndexUpdateMode ) ) ) ).thenReturn( indexUpdater2 );

			  _indexProxy3 = mock( typeof( IndexProxy ) );
			  _schemaIndexDescriptor3 = forSchema( forLabel( 5, 7, 8 ), PROVIDER_DESCRIPTOR ).withId( 2 ).withoutCapabilities();
			  IndexUpdater indexUpdater3 = mock( typeof( IndexUpdater ) );
			  when( _indexProxy3.Descriptor ).thenReturn( _schemaIndexDescriptor3 );
			  when( _indexProxy3.newUpdater( any( typeof( IndexUpdateMode ) ) ) ).thenReturn( indexUpdater3 );

			  _updaterMap = new IndexUpdaterMap( _indexMap, IndexUpdateMode.Online );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRetrieveUpdaterFromIndexMapForExistingIndex()
		 public virtual void ShouldRetrieveUpdaterFromIndexMapForExistingIndex()
		 {
			  // given
			  _indexMap.putIndexProxy( _indexProxy1 );

			  // when
			  IndexUpdater updater = _updaterMap.getUpdater( _schemaIndexDescriptor1.schema() );

			  // then
			  assertEquals( _indexUpdater1, updater );
			  assertEquals( 1, _updaterMap.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRetrieveUpdateUsingLabelAndProperty()
		 public virtual void ShouldRetrieveUpdateUsingLabelAndProperty()
		 {
			  // given
			  _indexMap.putIndexProxy( _indexProxy1 );

			  // when
			  IndexUpdater updater = _updaterMap.getUpdater( _schemaIndexDescriptor1.schema() );

			  // then
			  assertThat( updater, equalTo( _indexUpdater1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRetrieveSameUpdaterFromIndexMapForExistingIndexWhenCalledTwice()
		 public virtual void ShouldRetrieveSameUpdaterFromIndexMapForExistingIndexWhenCalledTwice()
		 {
			  // given
			  _indexMap.putIndexProxy( _indexProxy1 );

			  // when
			  IndexUpdater updater1 = _updaterMap.getUpdater( _schemaIndexDescriptor1.schema() );
			  IndexUpdater updater2 = _updaterMap.getUpdater( _schemaIndexDescriptor1.schema() );

			  // then
			  assertEquals( updater1, updater2 );
			  assertEquals( 1, _updaterMap.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRetrieveNoUpdaterForNonExistingIndex()
		 public virtual void ShouldRetrieveNoUpdaterForNonExistingIndex()
		 {
			  // when
			  IndexUpdater updater = _updaterMap.getUpdater( _schemaIndexDescriptor1.schema() );

			  // then
			  assertNull( updater );
			  assertTrue( "updater map must be empty", _updaterMap.Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseAllUpdaters() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseAllUpdaters()
		 {
			  // given
			  _indexMap.putIndexProxy( _indexProxy1 );
			  _indexMap.putIndexProxy( _indexProxy2 );

			  IndexUpdater updater1 = _updaterMap.getUpdater( _schemaIndexDescriptor1.schema() );
			  IndexUpdater updater2 = _updaterMap.getUpdater( _schemaIndexDescriptor2.schema() );

			  // hen
			  _updaterMap.close();

			  // then
			  verify( updater1 ).close();
			  verify( updater2 ).close();

			  assertTrue( "updater map must be empty", _updaterMap.Empty );
		 }
	}

}