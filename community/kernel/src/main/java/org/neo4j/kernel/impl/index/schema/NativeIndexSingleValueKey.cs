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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using Preconditions = Org.Neo4j.Util.Preconditions;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.array;

	/// <summary>
	/// Includes value and entity id (to be able to handle non-unique values).
	/// This is the abstraction of what NativeSchemaIndex with friends need from a schema key.
	/// Note that it says nothing about how keys are compared, serialized, read, written, etc. That is the job of Layout.
	/// 
	/// // TODO some of the methods in here are here to mimic the old interface and are kept/reinstated simply to reduce changes in current code
	/// </summary>
	internal abstract class NativeIndexSingleValueKey<SELF> : NativeIndexKey<SELF> where SELF : NativeIndexSingleValueKey<SELF>
	{
		 internal override void AssertValidValue( int stateSlot, Value value )
		 {
			  //noinspection ResultOfMethodCallIgnored
			  Preconditions.requireExactlyZero( stateSlot );
			  AssertCorrectType( value );
		 }

		 internal override void WriteValue( int stateSlot, Value value, Inclusion inclusion )
		 {
			  value.WriteTo( this );
		 }

		 internal override void InitValueAsLowest( int stateSlot, ValueGroup valueGroup )
		 {
			  InitValueAsLowest( valueGroup );
		 }

		 internal abstract void InitValueAsLowest( ValueGroup valueGroups );

		 internal override void InitValueAsHighest( int stateSlot, ValueGroup valueGroup )
		 {
			  InitValueAsHighest( valueGroup );
		 }

		 internal abstract void InitValueAsHighest( ValueGroup valueGroups );

		 internal abstract Value AssertCorrectType( Value value );

		 internal abstract Value AsValue();

		 internal override Value[] AsValues()
		 {
			  return array( AsValue() );
		 }

		 internal virtual void From( Value value )
		 {
			  AssertCorrectType( value );
			  value.WriteTo( this );
		 }

		 internal override int NumberOfStateSlots()
		 {
			  return 1;
		 }
	}

}