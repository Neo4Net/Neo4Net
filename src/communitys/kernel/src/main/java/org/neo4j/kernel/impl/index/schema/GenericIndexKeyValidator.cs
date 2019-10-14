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

	using Neo4Net.Index.@internal.gbptree;
	using Neo4Net.Index.@internal.gbptree;
	using Neo4Net.Kernel.impl.util;
	using AnyValue = Neo4Net.Values.AnyValue;
	using SequenceValue = Neo4Net.Values.SequenceValue;
	using TextArray = Neo4Net.Values.Storable.TextArray;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.BIGGEST_STATIC_SIZE;

	/// <summary>
	/// Validates Value[] tuples, whether or not they fit inside a <seealso cref="GBPTree"/> with a layout using <seealso cref="CompositeGenericKey"/>.
	/// Most values won't even be serialized to <seealso cref="CompositeGenericKey"/>, values that fit well within the margin.
	/// </summary>
	internal class GenericIndexKeyValidator : Validator<Value[]>
	{
		 private readonly int _maxLength;
		 private readonly Layout<GenericKey, NativeIndexValue> _layout;

		 internal GenericIndexKeyValidator( int maxLength, Layout<GenericKey, NativeIndexValue> layout )
		 {
			  this._maxLength = maxLength;
			  this._layout = layout;
		 }

		 public override void Validate( Value[] values )
		 {
			  int worstCaseSize = WorstCaseLength( values );
			  if ( worstCaseSize > _maxLength )
			  {
					int size = ActualLength( values );
					if ( size > _maxLength )
					{
						 throw new System.ArgumentException( format( "Property value size:%d of %s is too large to index into this particular index. Please see index documentation for limitations.", size, Arrays.ToString( values ) ) );
					}
			  }
		 }

		 /// <summary>
		 /// A method for calculating some sort of worst-case length of a value tuple. This have to be a cheap call and can return false positives.
		 /// It exists to avoid serializing all value tuples into native keys, which can be expensive.
		 /// </summary>
		 /// <param name="values"> the value tuple to calculate some exaggerated worst-case size of. </param>
		 /// <returns> the calculated worst-case size of the value tuple. </returns>
		 private static int WorstCaseLength( Value[] values )
		 {
			  int length = Long.BYTES;
			  foreach ( Value value in values )
			  {
					// Add some generic overhead, slightly exaggerated
					length += Long.BYTES;
					// Add worst-case length of this value
					length += WorstCaseLength( value );
			  }
			  return length;
		 }

		 private static int WorstCaseLength( AnyValue value )
		 {
			  if ( value.SequenceValue )
			  {
					SequenceValue sequenceValue = ( SequenceValue ) value;
					if ( sequenceValue is TextArray )
					{
						 TextArray textArray = ( TextArray ) sequenceValue;
						 int length = 0;
						 for ( int i = 0; i < textArray.Length(); i++ )
						 {
							  length += StringWorstCaseLength( textArray.StringValue( i ).Length );
						 }
						 return length;
					}
					return sequenceValue.Length() * BIGGEST_STATIC_SIZE;
			  }
			  else
			  {
					switch ( ( ( Value ) value ).valueGroup().category() )
					{
					case TEXT:
						 // For text, which is very dynamic in its nature do a worst-case off of number of characters in it
						 return StringWorstCaseLength( ( ( TextValue ) value ).length() );
					default:
						 // For all else then use the biggest possible value for a non-dynamic, non-array value a state can occupy
						 return BIGGEST_STATIC_SIZE;
					}
			  }
		 }

		 private static int StringWorstCaseLength( int stringLength )
		 {
			  return GenericKey.SizeStringLength + stringLength * 4;
		 }

		 private int ActualLength( Value[] values )
		 {
			  GenericKey key = _layout.newKey();
			  key.Initialize( 0 );
			  for ( int i = 0; i < values.Length; i++ )
			  {
					key.InitFromValue( i, values[i], NativeIndexKey.Inclusion.Neutral );
			  }
			  return key.Size();
		 }
	}

}