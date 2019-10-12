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
namespace Neo4Net.Kernel.Impl.Api
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using ExplicitIndexWrite = Neo4Net.@internal.Kernel.Api.ExplicitIndexWrite;
	using NamedToken = Neo4Net.@internal.Kernel.Api.NamedToken;
	using AutoIndexOperations = Neo4Net.Kernel.api.explicitindex.AutoIndexOperations;
	using InternalAutoIndexOperations = Neo4Net.Kernel.Impl.Api.explicitindex.InternalAutoIndexOperations;
	using TokenHolder = Neo4Net.Kernel.impl.core.TokenHolder;
	using TokenNotFoundException = Neo4Net.Kernel.impl.core.TokenNotFoundException;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.explicitindex.InternalAutoIndexing.NODE_AUTO_INDEX;

	public class AutoIndexOperationsTest
	{
		private bool InstanceFieldsInitialized = false;

		public AutoIndexOperationsTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_index = new InternalAutoIndexOperations( _tokens, InternalAutoIndexOperations.EntityType.NODE );
		}

		 private readonly ExplicitIndexWrite _ops = mock( typeof( ExplicitIndexWrite ) );
		 private readonly TokenHolder _tokens = mock( typeof( TokenHolder ) );
		 private AutoIndexOperations _index;

		 private readonly int _nonIndexedProperty = 1337;
		 private readonly string _nonIndexedPropertyName = "foo";
		 private readonly int _indexedProperty = 1338;
		 private readonly int _indexedProperty2 = 1339;
		 private readonly string _indexedPropertyName = "bar";
		 private readonly string _indexedPropertyName2 = "baz";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws org.neo4j.kernel.impl.core.TokenNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  when( _tokens.getTokenById( _nonIndexedProperty ) ).thenReturn( new NamedToken( _nonIndexedPropertyName, _nonIndexedProperty ) );
			  when( _tokens.getTokenById( _indexedProperty ) ).thenReturn( new NamedToken( _indexedPropertyName, _indexedProperty ) );
			  when( _tokens.getTokenById( _indexedProperty2 ) ).thenReturn( new NamedToken( _indexedPropertyName, _indexedProperty2 ) );
			  _index.enabled( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRemoveFromIndexForNonAutoIndexedProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotRemoveFromIndexForNonAutoIndexedProperty()
		 {
			  // Given
			  _index.startAutoIndexingProperty( _indexedPropertyName );

			  // When
			  _index.propertyRemoved( _ops, 11, _nonIndexedProperty );

			  // Then
			  verifyZeroInteractions( _ops );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveSpecificValueFromIndexForAutoIndexedProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveSpecificValueFromIndexForAutoIndexedProperty()
		 {
			  // Given
			  long nodeId = 11;
			  int value1 = 1;
			  int value2 = 2;
			  _index.startAutoIndexingProperty( _indexedPropertyName );
			  _index.startAutoIndexingProperty( _indexedPropertyName2 );
			  _index.propertyAdded( _ops, nodeId, _indexedProperty, Values.of( value1 ) );
			  _index.propertyAdded( _ops, nodeId, _indexedProperty2, Values.of( value2 ) );

			  // When
			  reset( _ops );
			  _index.propertyRemoved( _ops, nodeId, _indexedProperty );

			  // Then
			  verify( _ops ).nodeRemoveFromExplicitIndex( NODE_AUTO_INDEX, nodeId, _indexedPropertyName );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAddToIndexForNonAutoIndexedProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAddToIndexForNonAutoIndexedProperty()
		 {
			  // Given
			  _index.startAutoIndexingProperty( _indexedPropertyName );

			  // When
			  _index.propertyAdded( _ops, 11, _nonIndexedProperty, Values.of( "Hello!" ) );

			  // Then
			  verifyZeroInteractions( _ops );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAddOrRemoveFromIndexForNonAutoIndexedProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAddOrRemoveFromIndexForNonAutoIndexedProperty()
		 {
			  // Given
			  _index.startAutoIndexingProperty( _indexedPropertyName );

			  // When
			  _index.propertyChanged( _ops, 11, _nonIndexedProperty, Values.of( "Goodbye!" ), Values.of( "Hello!" ) );

			  // Then
			  verifyZeroInteractions( _ops );
		 }
	}

}