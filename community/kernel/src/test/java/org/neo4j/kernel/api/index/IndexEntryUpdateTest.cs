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
namespace Org.Neo4j.Kernel.Api.Index
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;

	public class IndexEntryUpdateTest
	{
		 private readonly Value[] _multiValue = new Value[]{ Values.of( "value" ), Values.of( "value2" ) };
		 private readonly Value _singleValue = Values.of( "value" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException thrown = org.junit.rules.ExpectedException.none();
		 public ExpectedException Thrown = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexEntryUpdatesShouldBeEqual()
		 public virtual void IndexEntryUpdatesShouldBeEqual()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> a = IndexEntryUpdate.add(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4), singleValue);
			  IndexEntryUpdate<object> a = IndexEntryUpdate.Add( 0, SchemaDescriptorFactory.forLabel( 3, 4 ), _singleValue );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> b = IndexEntryUpdate.add(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4), singleValue);
			  IndexEntryUpdate<object> b = IndexEntryUpdate.Add( 0, SchemaDescriptorFactory.forLabel( 3, 4 ), _singleValue );
			  assertThat( a, equalTo( b ) );
			  assertThat( a.GetHashCode(), equalTo(b.GetHashCode()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addShouldRetainValues()
		 public virtual void AddShouldRetainValues()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> single = IndexEntryUpdate.add(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4), singleValue);
			  IndexEntryUpdate<object> single = IndexEntryUpdate.Add( 0, SchemaDescriptorFactory.forLabel( 3, 4 ), _singleValue );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> multi = IndexEntryUpdate.add(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4, 5), multiValue);
			  IndexEntryUpdate<object> multi = IndexEntryUpdate.Add( 0, SchemaDescriptorFactory.forLabel( 3, 4, 5 ), _multiValue );
			  assertThat( single, not( equalTo( multi ) ) );
			  assertThat( single.Values(), equalTo(new object[]{ _singleValue }) );
			  assertThat( multi.Values(), equalTo(_multiValue) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeShouldRetainValues()
		 public virtual void RemoveShouldRetainValues()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> single = IndexEntryUpdate.remove(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4), singleValue);
			  IndexEntryUpdate<object> single = IndexEntryUpdate.Remove( 0, SchemaDescriptorFactory.forLabel( 3, 4 ), _singleValue );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> multi = IndexEntryUpdate.remove(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4, 5), multiValue);
			  IndexEntryUpdate<object> multi = IndexEntryUpdate.Remove( 0, SchemaDescriptorFactory.forLabel( 3, 4, 5 ), _multiValue );
			  assertThat( single, not( equalTo( multi ) ) );
			  assertThat( single.Values(), equalTo(new object[]{ _singleValue }) );
			  assertThat( multi.Values(), equalTo(_multiValue) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addShouldThrowIfAskedForChanged()
		 public virtual void AddShouldThrowIfAskedForChanged()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> single = IndexEntryUpdate.add(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4), singleValue);
			  IndexEntryUpdate<object> single = IndexEntryUpdate.Add( 0, SchemaDescriptorFactory.forLabel( 3, 4 ), _singleValue );
			  Thrown.expect( typeof( System.NotSupportedException ) );
			  single.BeforeValues();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeShouldThrowIfAskedForChanged()
		 public virtual void RemoveShouldThrowIfAskedForChanged()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> single = IndexEntryUpdate.remove(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4), singleValue);
			  IndexEntryUpdate<object> single = IndexEntryUpdate.Remove( 0, SchemaDescriptorFactory.forLabel( 3, 4 ), _singleValue );
			  Thrown.expect( typeof( System.NotSupportedException ) );
			  single.BeforeValues();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updatesShouldEqualRegardlessOfCreationMethod()
		 public virtual void UpdatesShouldEqualRegardlessOfCreationMethod()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> singleAdd = IndexEntryUpdate.add(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4), singleValue);
			  IndexEntryUpdate<object> singleAdd = IndexEntryUpdate.Add( 0, SchemaDescriptorFactory.forLabel( 3, 4 ), _singleValue );
			  Value[] singleAsArray = new Value[] { _singleValue };
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> multiAdd = IndexEntryUpdate.add(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4), singleAsArray);
			  IndexEntryUpdate<object> multiAdd = IndexEntryUpdate.Add( 0, SchemaDescriptorFactory.forLabel( 3, 4 ), singleAsArray );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> singleRemove = IndexEntryUpdate.remove(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4), singleValue);
			  IndexEntryUpdate<object> singleRemove = IndexEntryUpdate.Remove( 0, SchemaDescriptorFactory.forLabel( 3, 4 ), _singleValue );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> multiRemove = IndexEntryUpdate.remove(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4), singleAsArray);
			  IndexEntryUpdate<object> multiRemove = IndexEntryUpdate.Remove( 0, SchemaDescriptorFactory.forLabel( 3, 4 ), singleAsArray );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> singleChange = IndexEntryUpdate.change(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4), singleValue, singleValue);
			  IndexEntryUpdate<object> singleChange = IndexEntryUpdate.Change( 0, SchemaDescriptorFactory.forLabel( 3, 4 ), _singleValue, _singleValue );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> multiChange = IndexEntryUpdate.change(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4), singleAsArray, singleAsArray);
			  IndexEntryUpdate<object> multiChange = IndexEntryUpdate.Change( 0, SchemaDescriptorFactory.forLabel( 3, 4 ), singleAsArray, singleAsArray );
			  assertThat( singleAdd, equalTo( multiAdd ) );
			  assertThat( singleRemove, equalTo( multiRemove ) );
			  assertThat( singleChange, equalTo( multiChange ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void changedShouldRetainValues()
		 public virtual void ChangedShouldRetainValues()
		 {
			  Value singleAfter = Values.of( "Hello" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> singleChange = IndexEntryUpdate.change(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4), singleValue, singleAfter);
			  IndexEntryUpdate<object> singleChange = IndexEntryUpdate.Change( 0, SchemaDescriptorFactory.forLabel( 3, 4 ), _singleValue, singleAfter );
			  Value[] multiAfter = new Value[] { Values.of( "Hello" ), Values.of( "Hi" ) };
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> multiChange = IndexEntryUpdate.change(0, org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel(3, 4, 5), multiValue, multiAfter);
			  IndexEntryUpdate<object> multiChange = IndexEntryUpdate.Change( 0, SchemaDescriptorFactory.forLabel( 3, 4, 5 ), _multiValue, multiAfter );
			  assertThat( new object[]{ _singleValue }, equalTo( singleChange.BeforeValues() ) );
			  assertThat( new object[]{ singleAfter }, equalTo( singleChange.Values() ) );
			  assertThat( _multiValue, equalTo( multiChange.BeforeValues() ) );
			  assertThat( multiAfter, equalTo( multiChange.Values() ) );
		 }
	}

}