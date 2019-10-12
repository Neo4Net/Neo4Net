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
namespace Org.Neo4j.Kernel.Impl.@event
{
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Org.Neo4j.Graphdb.@event;
	using Strings = Org.Neo4j.Helpers.Strings;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	internal class PropertyEntryImpl<T> : PropertyEntry<T> where T : Org.Neo4j.Graphdb.PropertyContainer
	{
		 private readonly T _entity;
		 private readonly string _key;
		 private readonly object _value;
		 private readonly object _valueBeforeTx;

		 internal PropertyEntryImpl( T entity, string key, object value, object valueBeforeTx )
		 {
			  this._entity = entity;
			  this._key = key;
			  this._value = value;
			  this._valueBeforeTx = valueBeforeTx;
		 }

		 public override T Entity()
		 {
			  return this._entity;
		 }

		 public override string Key()
		 {
			  return this._key;
		 }

		 public override object Value()
		 {
			  return this._value;
		 }

		 public override object PreviouslyCommitedValue()
		 {
			  return this._valueBeforeTx;
		 }

		 internal virtual void CompareToAssigned( PropertyEntry<T> entry )
		 {
			  BasicCompareTo( entry );
			  AssertEqualsMaybeNull( entry.Value(), Value(), entry.Entity(), entry.Key() );
		 }

		 internal virtual void CompareToRemoved( PropertyEntry<T> entry )
		 {
			  BasicCompareTo( entry );
			  try
			  {
					entry.Value();
					fail( "Should throw IllegalStateException" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// OK
			  }
			  assertNull( Value() );
		 }

		 internal virtual void BasicCompareTo( PropertyEntry<T> entry )
		 {
			  assertEquals( entry.Entity(), Entity() );
			  assertEquals( entry.Key(), Key() );
			  AssertEqualsMaybeNull( entry.PreviouslyCommitedValue(), PreviouslyCommitedValue(), entry.Entity(), entry.Key() );
		 }

		 public override string ToString()
		 {
			  return "PropertyEntry[entity=" + _entity + ", key=" + _key + ", value=" + _value + ", valueBeforeTx="
						 + _valueBeforeTx + "]";
		 }

		 public static void AssertEqualsMaybeNull<T>( object o1, object o2, T entity, string key ) where T : Org.Neo4j.Graphdb.PropertyContainer
		 {
			  string entityDescription = "For " + entity + " and " + key;
			  if ( o1 == null || o2 == null )
			  {
					assertSame( entityDescription + ". " + Strings.prettyPrint( o1 ) + " != " + Strings.prettyPrint( o2 ), o1, o2 );
			  }
			  else
			  {
					assertEquals( entityDescription, Values.of( o1 ), Values.of( o2 ) );
			  }
		 }
	}

}