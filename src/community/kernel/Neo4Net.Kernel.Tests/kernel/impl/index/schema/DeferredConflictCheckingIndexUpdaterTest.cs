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
	using Test = org.junit.Test;


	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.Api.schema.index.TestIndexDescriptorFactory;
	using UpdateMode = Neo4Net.Kernel.Impl.Api.index.UpdateMode;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.anyVararg;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.collection.PrimitiveLongCollections.iterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.index.IndexEntryUpdate.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.index.IndexEntryUpdate.change;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.index.IndexEntryUpdate.remove;

	public class DeferredConflictCheckingIndexUpdaterTest
	{
		private bool InstanceFieldsInitialized = false;

		public DeferredConflictCheckingIndexUpdaterTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_descriptor = TestIndexDescriptorFactory.forLabel( _labelId, _propertyKeyIds );
		}

		 private readonly int _labelId = 1;
		 private readonly int[] _propertyKeyIds = new int[] { 2, 3 };
		 private IndexDescriptor _descriptor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldQueryAboutAddedAndChangedValueTuples() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldQueryAboutAddedAndChangedValueTuples()
		 {
			  // given
			  IndexUpdater actual = mock( typeof( IndexUpdater ) );
			  IndexReader reader = mock( typeof( IndexReader ) );
			  when( reader.Query( anyVararg() ) ).thenAnswer(invocation => iterator(0));
			  long nodeId = 0;
			  IList<IndexEntryUpdate<IndexDescriptor>> updates = new List<IndexEntryUpdate<IndexDescriptor>>();
			  updates.Add( add( nodeId++, _descriptor, Tuple( 10, 11 ) ) );
			  updates.Add( change( nodeId++, _descriptor, Tuple( "abc", "def" ), Tuple( "ghi", "klm" ) ) );
			  updates.Add( remove( nodeId++, _descriptor, Tuple( 1001L, 1002L ) ) );
			  updates.Add( change( nodeId++, _descriptor, Tuple( ( sbyte ) 2, ( sbyte ) 3 ), Tuple( ( sbyte ) 4, ( sbyte ) 5 ) ) );
			  updates.Add( add( nodeId++, _descriptor, Tuple( 5, "5" ) ) );
			  using ( DeferredConflictCheckingIndexUpdater updater = new DeferredConflictCheckingIndexUpdater( actual, () => reader, _descriptor ) )
			  {
					// when
					foreach ( IndexEntryUpdate<IndexDescriptor> update in updates )
					{
						 updater.Process( update );
						 verify( actual ).process( update );
					}
			  }

			  // then
			  foreach ( IndexEntryUpdate<IndexDescriptor> update in updates )
			  {
					if ( update.UpdateMode() == UpdateMode.ADDED || update.UpdateMode() == UpdateMode.CHANGED )
					{
						 Value[] tuple = update.Values();
						 IndexQuery[] query = new IndexQuery[tuple.Length];
						 for ( int i = 0; i < tuple.Length; i++ )
						 {
							  query[i] = IndexQuery.exact( _propertyKeyIds[i], tuple[i] );
						 }
						 verify( reader ).query( query );
					}
			  }
			  verify( reader ).close();
			  verifyNoMoreInteractions( reader );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnIndexEntryConflict() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowOnIndexEntryConflict()
		 {
			  // given
			  IndexUpdater actual = mock( typeof( IndexUpdater ) );
			  IndexReader reader = mock( typeof( IndexReader ) );
			  when( reader.Query( anyVararg() ) ).thenAnswer(invocation => iterator(101, 202));
			  DeferredConflictCheckingIndexUpdater updater = new DeferredConflictCheckingIndexUpdater( actual, () => reader, _descriptor );

			  // when
			  updater.Process( add( 0, _descriptor, Tuple( 10, 11 ) ) );
			  try
			  {
					updater.Close();
					fail( "Should have failed" );
			  }
			  catch ( IndexEntryConflictException e )
			  {
					// then good
					assertThat( e.Message, containsString( "101" ) );
					assertThat( e.Message, containsString( "202" ) );
			  }
		 }

		 private Value[] Tuple( params object[] values )
		 {
			  Value[] result = new Value[values.Length];
			  for ( int i = 0; i < values.Length; i++ )
			  {
					result[i] = Values.of( values[i] );
			  }
			  return result;
		 }
	}

}