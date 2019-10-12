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
	using Org.Neo4j.Index.@internal.gbptree;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.DynamicSizeUtil.extractKeySize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.DynamicSizeUtil.extractValueSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.DynamicSizeUtil.getOverhead;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.DynamicSizeUtil.putKeyValueSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.DynamicSizeUtil.readKeyValueSize;

	/// <summary>
	/// A <seealso cref="BlockEntry"/> is a key-value mapping and the smallest unit in the <seealso cref="BlockStorage"/> and <seealso cref="IndexUpdateStorage"/> hierarchy. Except for being a
	/// container class for key-value pairs, it also provide static methods for serializing and deserializing <seealso cref="BlockEntry"/> instances and calculating total
	/// store size of them.
	/// </summary>
	internal class BlockEntry<KEY, VALUE>
	{
		 private readonly KEY _key;
		 private readonly VALUE _value;

		 internal BlockEntry( KEY key, VALUE value )
		 {
			  this._key = key;
			  this._value = value;
		 }

		 internal virtual KEY Key()
		 {
			  return _key;
		 }

		 internal virtual VALUE Value()
		 {
			  return _value;
		 }

		 public override string ToString()
		 {
			  return format( "[%s=%s]", _key, _value );
		 }

		 internal static int EntrySize<VALUE, KEY>( Layout<KEY, VALUE> layout, KEY key, VALUE value )
		 {
			  int keySize = layout.KeySize( key );
			  int valueSize = layout.ValueSize( value );
			  return keySize + valueSize + getOverhead( keySize, valueSize );
		 }

		 internal static int KeySize<VALUE, KEY>( Layout<KEY, VALUE> layout, KEY key )
		 {
			  int keySize = layout.KeySize( key );
			  return keySize + getOverhead( keySize, 0 );
		 }

		 internal static BlockEntry<KEY, VALUE> Read<KEY, VALUE>( PageCursor pageCursor, Layout<KEY, VALUE> layout )
		 {
			  KEY key = layout.NewKey();
			  VALUE value = layout.NewValue();
			  Read( pageCursor, layout, key, value );
			  return new BlockEntry<KEY, VALUE>( key, value );
		 }

		 internal static void Read<KEY, VALUE>( PageCursor pageCursor, Layout<KEY, VALUE> layout, KEY key, VALUE value )
		 {
			  long entrySize = readKeyValueSize( pageCursor );
			  layout.ReadKey( pageCursor, key, extractKeySize( entrySize ) );
			  layout.ReadValue( pageCursor, value, extractValueSize( entrySize ) );
		 }

		 internal static void Read<KEY, VALUE>( PageCursor pageCursor, Layout<KEY, VALUE> layout, KEY key )
		 {
			  long entrySize = readKeyValueSize( pageCursor );
			  layout.ReadKey( pageCursor, key, extractKeySize( entrySize ) );
		 }

		 internal static void Write<KEY, VALUE>( PageCursor pageCursor, Layout<KEY, VALUE> layout, BlockEntry<KEY, VALUE> entry )
		 {
			  Write( pageCursor, layout, entry.Key(), entry.Value() );
		 }

		 internal static void Write<KEY, VALUE>( PageCursor pageCursor, Layout<KEY, VALUE> layout, KEY key, VALUE value )
		 {
			  int keySize = layout.KeySize( key );
			  int valueSize = layout.ValueSize( value );
			  putKeyValueSize( pageCursor, keySize, valueSize );
			  layout.WriteKey( pageCursor, key );
			  layout.WriteValue( pageCursor, value );
		 }

		 internal static void Write<KEY, VALUE>( PageCursor pageCursor, Layout<KEY, VALUE> layout, KEY key )
		 {
			  int keySize = layout.KeySize( key );
			  putKeyValueSize( pageCursor, keySize, 0 );
			  layout.WriteKey( pageCursor, key );
		 }
	}

}