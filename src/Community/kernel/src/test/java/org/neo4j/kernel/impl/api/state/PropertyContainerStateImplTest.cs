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
namespace Neo4Net.Kernel.Impl.Api.state
{
	using Test = org.junit.Test;

	using Iterators = Neo4Net.Helpers.Collection.Iterators;
	using PropertyKeyValue = Neo4Net.Kernel.api.properties.PropertyKeyValue;
	using OnHeapCollectionsFactory = Neo4Net.Kernel.impl.util.collection.OnHeapCollectionsFactory;
	using StorageProperty = Neo4Net.Storageengine.Api.StorageProperty;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class PropertyContainerStateImplTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAddedProperties()
		 public virtual void ShouldListAddedProperties()
		 {
			  // Given
			  PropertyContainerStateImpl state = new PropertyContainerStateImpl( 1, OnHeapCollectionsFactory.INSTANCE );
			  state.AddProperty( 1, Values.of( "Hello" ) );
			  state.AddProperty( 2, Values.of( "Hello" ) );
			  state.RemoveProperty( 1 );

			  // When
			  IEnumerator<StorageProperty> added = state.AddedProperties();

			  // Then
			  assertThat( Iterators.asList( added ), equalTo( asList( new PropertyKeyValue( 2, Values.of( "Hello" ) ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAddedPropertiesEvenIfPropertiesHaveBeenReplaced()
		 public virtual void ShouldListAddedPropertiesEvenIfPropertiesHaveBeenReplaced()
		 {
			  // Given
			  PropertyContainerStateImpl state = new PropertyContainerStateImpl( 1, OnHeapCollectionsFactory.INSTANCE );
			  state.AddProperty( 1, Values.of( "Hello" ) );
			  state.AddProperty( 1, Values.of( "WAT" ) );
			  state.AddProperty( 2, Values.of( "Hello" ) );

			  // When
			  IEnumerator<StorageProperty> added = state.AddedProperties();

			  // Then
			  assertThat( Iterators.asList( added ), equalTo( asList( new PropertyKeyValue( 1, Values.of( "WAT" ) ), new PropertyKeyValue( 2, Values.of( "Hello" ) ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertAddRemoveToChange()
		 public virtual void ShouldConvertAddRemoveToChange()
		 {
			  // Given
			  PropertyContainerStateImpl state = new PropertyContainerStateImpl( 1, OnHeapCollectionsFactory.INSTANCE );

			  // When
			  state.RemoveProperty( 4 );
			  state.AddProperty( 4, Values.of( "another value" ) );

			  // Then
			  assertThat( Iterators.asList( state.ChangedProperties() ), equalTo(asList(new PropertyKeyValue(4, Values.of("another value")))) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( state.AddedProperties().hasNext() );
			  assertTrue( state.RemovedProperties().Empty );
		 }
	}

}