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
	using NumberValue = Neo4Net.Values.Storable.NumberValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_NUMBER_BYTE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_NUMBER_DOUBLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_NUMBER_FLOAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_NUMBER_INT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_NUMBER_LONG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_NUMBER_SHORT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.SIZE_NUMBER_TYPE;

	internal class NumberType : Type
	{
		 // Affected key state:
		 // long0 (value)
		 // long1 (number type)

		 internal NumberType( sbyte typeId ) : base( ValueGroup.NUMBER, typeId, Values.of( double.NegativeInfinity ), Values.of( double.PositiveInfinity ) )
		 {
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  return NumberKeySize( state.Long1 ) + SIZE_NUMBER_TYPE;
		 }

		 internal override void CopyValue( GenericKey to, GenericKey from )
		 {
			  to.Long0 = from.Long0;
			  to.Long1 = from.Long1;
		 }

		 internal override Value AsValue( GenericKey state )
		 {
			  return AsValue( state.Long0, state.Long1 );
		 }

		 internal override int CompareValue( GenericKey left, GenericKey right )
		 {
			  return Compare( left.Long0, left.Long1, right.Long0, right.Long1 );
		 }

		 internal override void PutValue( PageCursor cursor, GenericKey state )
		 {
			  cursor.PutByte( ( sbyte ) state.Long1 );
			  switch ( ( int ) state.Long1 )
			  {
			  case RawBits.BYTE:
					cursor.PutByte( ( sbyte ) state.Long0 );
					break;
			  case RawBits.SHORT:
					cursor.PutShort( ( short ) state.Long0 );
					break;
			  case RawBits.INT:
			  case RawBits.FLOAT:
					cursor.PutInt( ( int ) state.Long0 );
					break;
			  case RawBits.LONG:
			  case RawBits.DOUBLE:
					cursor.PutLong( state.Long0 );
					break;
			  default:
					throw new System.ArgumentException( "Unknown number type " + state.Long1 );
			  }
		 }

		 internal override bool ReadValue( PageCursor cursor, int size, GenericKey into )
		 {
			  into.Long1 = cursor.Byte;
			  switch ( ( int ) into.Long1 )
			  {
			  case RawBits.BYTE:
					into.Long0 = cursor.Byte;
					return true;
			  case RawBits.SHORT:
					into.Long0 = cursor.Short;
					return true;
			  case RawBits.INT:
			  case RawBits.FLOAT:
					into.Long0 = cursor.Int;
					return true;
			  case RawBits.LONG:
			  case RawBits.DOUBLE:
					into.Long0 = cursor.Long;
					return true;
			  default:
					return false;
			  }
		 }

		 internal static int NumberKeySize( long long1 )
		 {
			  switch ( ( int ) long1 )
			  {
			  case RawBits.BYTE:
					return SIZE_NUMBER_BYTE;
			  case RawBits.SHORT:
					return SIZE_NUMBER_SHORT;
			  case RawBits.INT:
					return SIZE_NUMBER_INT;
			  case RawBits.LONG:
					return SIZE_NUMBER_LONG;
			  case RawBits.FLOAT:
					return SIZE_NUMBER_FLOAT;
			  case RawBits.DOUBLE:
					return SIZE_NUMBER_DOUBLE;
			  default:
					throw new System.ArgumentException( "Unknown number type " + long1 );
			  }
		 }

		 internal static NumberValue AsValue( long long0, long long1 )
		 {
			  return RawBits.AsNumberValue( long0, ( sbyte ) long1 );
		 }

		 internal static int Compare( long thisLong0, long thisLong1, long thatLong0, long thatLong1 )
		 {
			  return RawBits.Compare( thisLong0, ( sbyte ) thisLong1, thatLong0, ( sbyte ) thatLong1 );
		 }

		 internal virtual void Write( GenericKey state, long value, sbyte numberType )
		 {
			  state.Long0 = value;
			  state.Long1 = numberType;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long0=" + state.Long0 );
			  joiner.add( "long1=" + state.Long1 );
		 }
	}

}