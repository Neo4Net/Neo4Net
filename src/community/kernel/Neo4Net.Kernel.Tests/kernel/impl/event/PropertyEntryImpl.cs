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
namespace Neo4Net.Kernel.Impl.@event
{
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Neo4Net.GraphDb.Events;
	using Strings = Neo4Net.Helpers.Strings;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	internal class PropertyEntryImpl<T> : PropertyEntry<T> where T : Neo4Net.GraphDb.PropertyContainer
	{
		 private readonly T _entity;
		 private readonly string _key;
		 private readonly object _value;
		 private readonly object _valueBeforeTx;

		 internal PropertyEntryImpl( T IEntity, string key, object value, object valueBeforeTx )
		 {
			  this._entity = IEntity;
			  this._key = key;
			  this._value = value;
			  this._valueBeforeTx = valueBeforeTx;
		 }

		 public override T IEntity()
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
			  assertEquals( entry.Entity(), IEntity() );
			  assertEquals( entry.Key(), Key() );
			  AssertEqualsMaybeNull( entry.PreviouslyCommitedValue(), PreviouslyCommitedValue(), entry.Entity(), entry.Key() );
		 }

		 public override string ToString()
		 {
			  return "PropertyEntry[entity=" + _entity + ", key=" + _key + ", value=" + _value + ", valueBeforeTx="
						 + _valueBeforeTx + "]";
		 }

		 public static void AssertEqualsMaybeNull<T>( object o1, object o2, T IEntity, string key ) where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  string IEntityDescription = "For " + IEntity + " and " + key;
			  if ( o1 == null || o2 == null )
			  {
					assertSame( IEntityDescription + ". " + Strings.prettyPrint( o1 ) + " != " + Strings.prettyPrint( o2 ), o1, o2 );
			  }
			  else
			  {
					assertEquals( IEntityDescription, Values.of( o1 ), Values.of( o2 ) );
			  }
		 }
	}

}