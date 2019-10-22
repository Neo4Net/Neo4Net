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
namespace Neo4Net.Index.Internal.gbptree
{

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	/// <summary>
	/// Main point of interaction for customizing a <seealso cref="GBPTree"/>, how its keys and values are represented
	/// as bytes and what keys and values contains.
	/// <para>
	/// Additionally custom meta data can be supplied, which will be persisted in <seealso cref="GBPTree"/>.
	/// </para>
	/// <para>
	/// Rather extend <seealso cref="Adapter"/> as to get standard implementation of e.g. <seealso cref="Adapter.toString()"/>.
	/// 
	/// </para>
	/// </summary>
	/// @param <KEY> type of key </param>
	/// @param <VALUE> type of value </param>
	public interface Layout<KEY, VALUE> : IComparer<KEY>
	{

		 /// <returns> new key instance. </returns>
		 KEY NewKey();

		 /// <summary>
		 /// Copies contents of {@code key} to {@code into}.
		 /// </summary>
		 /// <param name="key"> key (left unchanged as part of this call) to copy contents from. </param>
		 /// <param name="into"> key (changed as part of this call) to copy contents into. </param>
		 /// <returns> the provided {@code into} instance for convenience. </returns>
		 KEY CopyKey( KEY key, KEY into );

		 /// <returns> new value instance. </returns>
		 VALUE NewValue();

		 /// <param name="key"> for which to give size. </param>
		 /// <returns> size, in bytes, of given key. </returns>
		 int KeySize( KEY key );

		 /// <param name="value"> for which to give size. </param>
		 /// <returns> size, in bytes, of given value. </returns>
		 int ValueSize( VALUE value );

		 /// <summary>
		 /// Writes contents of {@code key} into {@code cursor} at its current offset.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to write into, at current offset. </param>
		 /// <param name="key"> key containing data to write. </param>
		 void WriteKey( PageCursor cursor, KEY key );

		 /// <summary>
		 /// Writes contents of {@code value} into {@code cursor} at its current offset.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to write into, at current offset. </param>
		 /// <param name="value"> value containing data to write. </param>
		 void WriteValue( PageCursor cursor, VALUE value );

		 /// <summary>
		 /// Reads key contents at {@code cursor} at its current offset into {@code key}. </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to read from, at current offset. </param>
		 /// <param name="into"> key instances to read into. </param>
		 /// <param name="keySize"> size of key to read or <seealso cref="FIXED_SIZE_KEY"/> if key is fixed size. </param>
		 void ReadKey( PageCursor cursor, KEY into, int keySize );

		 /// <summary>
		 /// Reads value contents at {@code cursor} at its current offset into {@code value}. </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to read from, at current offset. </param>
		 /// <param name="into"> value instances to read into. </param>
		 /// <param name="valueSize"> size of key to read or <seealso cref="FIXED_SIZE_VALUE"/> if value is fixed size. </param>
		 void ReadValue( PageCursor cursor, VALUE into, int valueSize );

		 /// <summary>
		 /// Indicate if keys and values are fixed or dynamix size. </summary>
		 /// <returns> true if keys and values are fixed size, otherwise true. </returns>
		 bool FixedSize();

		 /// <summary>
		 /// Find shortest key (best effort) that separate left from right in sort order
		 /// and initialize into with result. </summary>
		 /// <param name="left"> key that is less than right </param>
		 /// <param name="right"> key that is greater than left. </param>
		 /// <param name="into"> will be initialized with result. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void minimalSplitter(KEY left, KEY right, KEY into)
	//	 {
	//		  copyKey(right, into);
	//	 }

		 /// <summary>
		 /// Used as verification when loading an index after creation, to verify that the same layout is used,
		 /// as the one it was initially created with.
		 /// </summary>
		 /// <returns> a long acting as an identifier, written in the header of an index. </returns>
		 long Identifier();

		 /// <returns> major version of layout. Will be compared to version written into meta page when opening index. </returns>
		 int MajorVersion();

		 /// <returns> minor version of layout. Will be compared to version written into meta page when opening index. </returns>
		 int MinorVersion();

		 /// <summary>
		 /// Writes meta data specific to this layout instance to {@code cursor} at its current offset.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to write into, at its current offset. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void writeMetaData(org.Neo4Net.io.pagecache.PageCursor cursor)
	//	 { // no meta-data by default
	//	 }

		 /// <summary>
		 /// Reads meta data specific to this layout instance from {@code cursor} at its current offset.
		 /// The read meta data must also be verified against meta data provided in constructor of this Layout.
		 /// Constructor-provided meta data can be {@code null} to skip this verification.
		 /// if read meta data doesn't match with the meta data provided in constructor
		 /// <seealso cref="PageCursor.setCursorException(string)"/> should be called with appropriate error message.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to read from, at its current offset. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void readMetaData(org.Neo4Net.io.pagecache.PageCursor cursor)
	//	 { // no meta-data by default
	//	 }

		 /// <summary>
		 /// Utility method for generating an <seealso cref="identifier()"/>. Generates an 8-byte identifier from a short name
		 /// plus a 4-byte identifier.
		 /// </summary>
		 /// <param name="name"> name to be part of this identifier, must at most be 4 characters. </param>
		 /// <param name="identifier"> to include into the returned named identifier. </param>
		 /// <returns> a long which is a combination of {@code name} and {@code identifier}. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static long namedIdentifier(String name, int identifier)
	//	 {
	//		  char[] chars = name.toCharArray();
	//		  if (chars.length > 4)
	//		  {
	//				throw new IllegalArgumentException("Maximum 4 character name, was '" + name + "'");
	//		  }
	//		  long upperInt = 0;
	//		  for (char aChar : chars)
	//		  {
	//				byte byteValue = (byte)(((byte) aChar) ^ ((byte)(aChar >> 8)));
	//				upperInt <<= 8;
	//				upperInt |= byteValue & 0xFF;
	//		  }
	//
	//		  return (upperInt << Integer.SIZE) | identifier;
	//	 }

		 /// <summary>
		 /// Typically, a layout is compatible with given identifier, major and minor version if
		 /// <ul>
		 /// <li>{@code layoutIdentifier == this.identifier()}</li>
		 /// <li>{@code majorVersion == this.majorVersion()}</li>
		 /// <li>{@code minorVersion == this.minorVersion()}</li>
		 /// </ul>
		 /// <para>
		 /// When opening a <seealso cref="GBPTree tree"/> to 'use' it, read and write to it, providing a layout with the right compatibility is
		 /// important because it decides how to read and write entries in the tree.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="layoutIdentifier"> the stored layout identifier we want to check compatibility against. </param>
		 /// <param name="majorVersion"> the stored major version we want to check compatibility against. </param>
		 /// <param name="minorVersion"> the stored minor version we want to check compatibility against. </param>
		 /// <returns> true if this layout is compatible with combination of identifier, major and minor version, false otherwise. </returns>
		 bool CompatibleWith( long layoutIdentifier, int majorVersion, int minorVersion );

		 /// <summary>
		 /// Adapter for <seealso cref="Layout"/>, which contains convenient standard implementations of some methods.
		 /// </summary>
		 /// @param <KEY> type of key </param>
		 /// @param <VALUE> type of value </param>
	}

	public static class Layout_Fields
	{
		 public const int FIXED_SIZE_KEY = -1;
		 public const int FIXED_SIZE_VALUE = -1;
	}

	 public abstract class Layout_Adapter<KEY, VALUE> : Layout<KEY, VALUE>
	 {
		 public abstract long NamedIdentifier( string name, int identifier );
		 public abstract void ReadMetaData( PageCursor cursor );
		 public abstract void WriteMetaData( PageCursor cursor );
		 public abstract int MinorVersion();
		 public abstract int MajorVersion();
		 public abstract long Identifier();
		 public abstract void MinimalSplitter( KEY left, KEY right, KEY into );
		 public abstract bool FixedSize();
		 public abstract void ReadValue( PageCursor cursor, VALUE into, int valueSize );
		 public abstract void ReadKey( PageCursor cursor, KEY into, int keySize );
		 public abstract void WriteValue( PageCursor cursor, VALUE value );
		 public abstract void WriteKey( PageCursor cursor, KEY key );
		 public abstract int ValueSize( VALUE value );
		 public abstract int KeySize( KEY key );
		 public abstract VALUE NewValue();
		 public abstract KEY CopyKey( KEY key, KEY into );
		 public abstract KEY NewKey();
		  public override string ToString()
		  {
				return format( "%s[version:%d.%d, identifier:%d, fixedSize:%b]", this.GetType().Name, MajorVersion(), MinorVersion(), Identifier(), FixedSize() );
		  }

		  public override bool CompatibleWith( long layoutIdentifier, int majorVersion, int minorVersion )
		  {
				return layoutIdentifier == Identifier() && majorVersion == majorVersion() && minorVersion == minorVersion();
		  }
	 }

}