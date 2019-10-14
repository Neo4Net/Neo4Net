using System;

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

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Neo4Net.Values.Storable;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.BIGGEST_REASONABLE_ARRAY_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_ARRAY_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.setCursorException;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.toNonNegativeShortExact;

	/// <summary>
	/// Common ancestor of all array-types. Many of the methods are implemented by doing array looping and delegating array item operations
	/// to the non-array versions of the specific array type. </summary>
	/// @param <T> type of raw array items for this array type, e.g. <seealso cref="LocalDate"/> for <seealso cref="DateArrayType"/>. </param>
	internal abstract class AbstractArrayType<T> : Type
	{
		 private readonly ArrayElementComparator _arrayElementComparator;
		 private readonly ArrayElementValueFactory<T> _valueFactory;
		 internal readonly ArrayElementWriter ArrayElementWriter;
		 private readonly ArrayElementReader _arrayElementReader;
		 private readonly System.Func<int, T[]> _arrayCreator;
		 private readonly Neo4Net.Values.Storable.ValueWriter_ArrayType _arrayType;

		 internal AbstractArrayType( ValueGroup valueGroup, sbyte typeId, ArrayElementComparator arrayElementComparator, ArrayElementValueFactory<T> valueFactory, ArrayElementWriter arrayElementWriter, ArrayElementReader arrayElementReader, System.Func<int, T[]> arrayCreator, Neo4Net.Values.Storable.ValueWriter_ArrayType arrayType ) : base( valueGroup, typeId, null, null )
		 {
			  this._arrayElementComparator = arrayElementComparator;
			  this._valueFactory = valueFactory;
			  this.ArrayElementWriter = arrayElementWriter;
			  this._arrayElementReader = arrayElementReader;
			  this._arrayCreator = arrayCreator;
			  this._arrayType = arrayType;
		 }

		 internal override void CopyValue( GenericKey to, GenericKey from )
		 {
			  CopyValue( to, from, from.ArrayLength );
		 }

		 internal abstract void CopyValue( GenericKey to, GenericKey from, int arrayLength );

		 internal abstract void InitializeArray( GenericKey key, int length, Neo4Net.Values.Storable.ValueWriter_ArrayType arrayType );

		 internal override void MinimalSplitter( GenericKey left, GenericKey right, GenericKey into )
		 {
			  int lastEqualIndex = -1;
			  if ( left.TypeConflict == right.TypeConflict )
			  {
					int maxLength = min( left.ArrayLength, right.ArrayLength );
					for ( int index = 0; index < maxLength; index++ )
					{
						 if ( _arrayElementComparator.compare( left, right, index ) != 0 )
						 {
							  break;
						 }
						 lastEqualIndex++;
					}
			  }
			  // Convert from last equal index to first index to differ +1
			  // Convert from index to length +1
			  // Total +2
			  int length = Math.Min( right.ArrayLength, lastEqualIndex + 2 );
			  CopyValue( into, right, length );
			  into.ArrayLength = length;
		 }

		 internal override int CompareValue( GenericKey left, GenericKey right )
		 {
			  if ( left.IsHighestArray || right.IsHighestArray )
			  {
					return Boolean.compare( left.IsHighestArray, right.IsHighestArray );
			  }

			  int index = 0;
			  int compare = 0;
			  int length = min( left.ArrayLength, right.ArrayLength );

			  for ( ; compare == 0 && index < length; index++ )
			  {
					compare = _arrayElementComparator.compare( left, right, index );
			  }

			  return compare == 0 ? Integer.compare( left.ArrayLength, right.ArrayLength ) : compare;
		 }

		 internal override Value AsValue( GenericKey state )
		 {
			  T[] array = _arrayCreator.apply( state.ArrayLength );
			  for ( int i = 0; i < state.ArrayLength; i++ )
			  {
					array[i] = _valueFactory.from( state, i );
			  }
			  return Values.of( array );
		 }

		 internal override void PutValue( PageCursor cursor, GenericKey state )
		 {
			  PutArray( cursor, state, ArrayElementWriter );
		 }

		 internal override bool ReadValue( PageCursor cursor, int size, GenericKey into )
		 {
			  return ReadArray( cursor, _arrayType, _arrayElementReader, into );
		 }

		 /// <summary>
		 /// In the array case there's nothing lower than a zero-length array, so simply make sure that the key state is initialized
		 /// with state reflecting that. No specific value required. </summary>
		 /// <param name="state"> key state to initialize as lowest of this type. </param>
		 internal override void InitializeAsLowest( GenericKey state )
		 {
			  state.InitializeArrayMeta( 0 );
			  InitializeArray( state, 0, _arrayType );
		 }

		 internal override void InitializeAsHighest( GenericKey state )
		 {
			  state.InitializeArrayMeta( 0 );
			  InitializeArray( state, 0, _arrayType );
			  state.IsHighestArray = true;
		 }

		 internal virtual int ArrayKeySize( GenericKey key, int elementSize )
		 {
			  return SIZE_ARRAY_LENGTH + key.ArrayLength * elementSize;
		 }

		 internal static void PutArrayHeader( PageCursor cursor, short arrayLength )
		 {
			  cursor.PutShort( arrayLength );
		 }

		 internal static void PutArrayItems( PageCursor cursor, GenericKey key, ArrayElementWriter itemWriter )
		 {
			  for ( int i = 0; i < key.ArrayLength; i++ )
			  {
					itemWriter( cursor, key, i );
			  }
		 }

		 internal static void PutArray( PageCursor cursor, GenericKey key, ArrayElementWriter writer )
		 {
			  PutArrayHeader( cursor, toNonNegativeShortExact( key.ArrayLength ) );
			  PutArrayItems( cursor, key, writer );
		 }

		 internal static bool ReadArray( PageCursor cursor, Neo4Net.Values.Storable.ValueWriter_ArrayType type, ArrayElementReader reader, GenericKey into )
		 {
			  if ( !SetArrayLengthWhenReading( into, cursor, cursor.Short ) )
			  {
					return false;
			  }
			  into.BeginArray( into.ArrayLength, type );
			  for ( int i = 0; i < into.ArrayLength; i++ )
			  {
					if ( !reader( cursor, into ) )
					{
						 return false;
					}
			  }
			  into.EndArray();
			  return true;
		 }

		 internal static bool SetArrayLengthWhenReading( GenericKey state, PageCursor cursor, short arrayLength )
		 {
			  state.ArrayLength = arrayLength;
			  if ( state.ArrayLength < 0 || state.ArrayLength > BIGGEST_REASONABLE_ARRAY_LENGTH )
			  {
					setCursorException( cursor, "non-valid array length, " + state.ArrayLength );
					state.ArrayLength = 0;
					return false;
			  }
			  return true;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "isHighestArray=" + state.IsHighestArray );
			  joiner.add( "arrayLength=" + state.ArrayLength );
			  joiner.add( "currentArrayOffset=" + state.CurrentArrayOffset );
		 }

		 delegate int ArrayElementComparator( GenericKey o1, GenericKey o2, int i );

		 delegate bool ArrayElementReader( PageCursor cursor, GenericKey into );

		 delegate void ArrayElementWriter( PageCursor cursor, GenericKey key, int i );

		 delegate T ArrayElementValueFactory<T>( GenericKey key, int i );
	}

}