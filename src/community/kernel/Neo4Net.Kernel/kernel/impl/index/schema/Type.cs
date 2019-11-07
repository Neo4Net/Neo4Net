using System.Collections.Generic;

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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.GenericKey.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.NativeIndexKey.Inclusion.HIGH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.NativeIndexKey.Inclusion.LOW;

	/// <summary>
	/// All functionality for reading, writing, comparing, calculating size etc a specific value type in a native index.
	/// This is not an enum mostly because of arrays having a shared subclass with lots of shared functionality applicable to all array types.
	/// The type classes are state-less singletons and operate on state in <seealso cref="GenericKey"/> which is passed in as argument to the methods.
	/// <para>
	/// Looking solely at <seealso cref="GenericKey"/> is has a bunch of state with no specific meaning, but they get meaning when looking at them
	/// through a <seealso cref="Type"/>, where e.g. the fields `long0` and `long1` means perhaps string length of a string or the integer value for a number type.
	/// </para>
	/// </summary>
	internal abstract class Type
	{
		 private const long MASK_BOOLEAN = 0x1;

		 /// <summary>
		 /// Compares <seealso cref="Type types"/> against each other. The ordering adheres to that of <seealso cref="ValueGroup"/>.
		 /// </summary>
		 internal static readonly IComparer<Type> Comparator = comparing( t => t.valueGroup );

		 /// <summary>
		 /// <seealso cref="ValueGroup"/> for values that this type manages.
		 /// </summary>
		 internal readonly ValueGroup ValueGroup;

		 /// <summary>
		 /// An internal id of this type, also persisted into the actual keys in the tree.
		 /// WARNING: changing ids of existing types will change the index format.
		 /// </summary>
		 internal readonly sbyte TypeId;

		 /// <summary>
		 /// Minimum possible value of this type.
		 /// </summary>
		 private readonly Value _minValue;

		 /// <summary>
		 /// Maximum possible value of this type.
		 /// </summary>
		 private readonly Value _maxValue;

		 internal Type( ValueGroup valueGroup, sbyte typeId, Value minValue, Value maxValue )
		 {
			  this.ValueGroup = valueGroup;
			  this.TypeId = typeId;
			  this._minValue = minValue;
			  this._maxValue = maxValue;
		 }

		 /// <summary>
		 /// Size of the key state of this type in the given <seealso cref="GenericKey"/>. </summary>
		 /// <param name="state"> the <seealso cref="GenericKey"/> holding the initialized key state. </param>
		 /// <returns> size, in bytes of the key state, not counting tree overhead or IEntity id. </returns>
		 internal abstract int ValueSize( GenericKey state );

		 /// <summary>
		 /// Copies key state from {@code from} to {@code to}. </summary>
		 /// <param name="to"> key state to copy into. </param>
		 /// <param name="from"> key state to copy from. </param>
		 internal abstract void CopyValue( GenericKey to, GenericKey from );

		 /// <summary>
		 /// Calculates minimal splitter between {@code left} and {@code right} and copies that state, potentially a sub-part of that state into {@code into}. </summary>
		 /// <param name="left"> left key state to compare. </param>
		 /// <param name="right"> right key state to compare. </param>
		 /// <param name="into"> state which gets initialized with the minimal splitter key state between {@code left} and {@code right}. </param>
		 internal virtual void MinimalSplitter( GenericKey left, GenericKey right, GenericKey into )
		 {
			  // if not a specific implementation then default is to just copy from 'right'
			  into.CopyFromInternal( right );
		 }

		 /// <summary>
		 /// Materializes the key state into an actual <seealso cref="Value"/> object. </summary>
		 /// <param name="state"> key state to materialize a <seealso cref="Value"/> from. </param>
		 /// <returns> a <seealso cref="Value"/> from the given {@code state}. </returns>
		 internal abstract Value AsValue( GenericKey state );

		 /// <summary>
		 /// Compares {@code left} and {@code right} key state. Follows semantics of <seealso cref="Comparator.compare(object, object)"/>. </summary>
		 /// <param name="left"> left key state to compare. </param>
		 /// <param name="right"> right key state to compare. </param>
		 /// <returns> comparison between the {@code left} and {@code right} key state. </returns>
		 internal abstract int CompareValue( GenericKey left, GenericKey right );

		 /// <summary>
		 /// Serializes key state from {@code state} into the {@code cursor}. </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> initialized at correct offset, capable of writing the key state. </param>
		 /// <param name="state"> key state to write to the {@code cursor}. </param>
		 internal abstract void PutValue( PageCursor cursor, GenericKey state );

		 /// <summary>
		 /// Deserializes key state from {@code cursor} into {@code state}. </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> initialized at correct offset to read from. </param>
		 /// <param name="size"> total number of remaining bytes for this key state. </param>
		 /// <param name="into"> <seealso cref="GenericKey"/> to deserialize the key state into. </param>
		 /// <returns> whether or not this was a sane read. Returning {@code false} should mean that it was simply a bad read,
		 /// and that the next read in this shouldRetry loop will get a good read. This will signal that it's not worth it to read any further
		 /// for this key and that the cursor have been told about this error, via <seealso cref="PageCursor.setCursorException(string)"/>.
		 /// Otherwise, for a successful read, returns {@code true}. </returns>
		 internal abstract bool ReadValue( PageCursor cursor, int size, GenericKey into );

		 /// <summary>
		 /// Initializes key state to be the lowest possible of this type, i.e. all actual key states of this type are bigger in comparison. </summary>
		 /// <param name="state"> key state to initialize as lowest of this type. </param>
		 internal virtual void InitializeAsLowest( GenericKey state )
		 {
			  state.WriteValue( _minValue, LOW );
		 }

		 /// <summary>
		 /// Initializes key state to be the highest possible of this type, i.e. all actual key states of this type are smaller in comparison. </summary>
		 /// <param name="state"> key state to initialize as highest of this type. </param>
		 internal virtual void InitializeAsHighest( GenericKey state )
		 {
			  state.WriteValue( _maxValue, HIGH );
		 }

		 /// <summary>
		 /// Generate a string-representation of the key state of this type, mainly for debugging purposes. </summary>
		 /// <param name="state"> the key state containing the state to generate string representation for. </param>
		 /// <returns> a string-representation of the key state of this type. </returns>
		 internal virtual string ToString( GenericKey state )
		 {
			  // For most types it's a straight-forward Value#toString().
			  return AsValue( state ).ToString();
		 }

		 internal static sbyte[] EnsureBigEnough( sbyte[] array, int targetLength )
		 {
			  return array == null || array.Length < targetLength ? new sbyte[targetLength] : array;
		 }

		 internal static sbyte[][] EnsureBigEnough( sbyte[][] array, int targetLength )
		 {
			  return array == null || array.Length < targetLength ? new sbyte[targetLength][] : array;
		 }

		 internal static long[] EnsureBigEnough( long[] array, int targetLength )
		 {
			  return array == null || array.Length < targetLength ? new long[targetLength] : array;
		 }

		 internal static bool BooleanOf( long longValue )
		 {
			  return ( longValue & MASK_BOOLEAN ) == TRUE;
		 }

		 internal virtual string ToDetailedString( GenericKey state )
		 {
			  StringJoiner joiner = new StringJoiner( ", " );
			  joiner.add( ToString( state ) );

			  // Mutable, meta-state
			  joiner.add( "type=" + state.TypeConflict.GetType().Name );
			  joiner.add( "inclusion=" + state.Inclusion );
			  joiner.add( "isArray=" + state.IsArray );

			  AddTypeSpecificDetails( joiner, state );
			  return joiner.ToString();
		 }

		 protected internal abstract void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state );
	}

}