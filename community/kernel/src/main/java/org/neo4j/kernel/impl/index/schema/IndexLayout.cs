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

	internal abstract class IndexLayout<KEY, VALUE> : Org.Neo4j.Index.@internal.gbptree.Layout_Adapter<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 private readonly long _identifier;
		 private readonly int _majorVersion;
		 private readonly int _minorVersion;

		 // allows more control of the identifier, needed for legacy reasons for the two number layouts
		 internal IndexLayout( long identifier, int majorVersion, int minorVersion )
		 {
			  this._identifier = identifier;
			  this._majorVersion = majorVersion;
			  this._minorVersion = minorVersion;
		 }

		 internal IndexLayout( string layoutName, int majorVersion, int minorVersion ) : this( Layout.namedIdentifier( layoutName, NativeIndexValue.SIZE ), majorVersion, minorVersion )
		 {
		 }

		 public override VALUE NewValue()
		 {
			  return ( VALUE ) NativeIndexValue.Instance;
		 }

		 public override int ValueSize( NativeIndexValue nativeIndexValue )
		 {
			  return NativeIndexValue.SIZE;
		 }

		 public override void WriteValue( PageCursor cursor, NativeIndexValue nativeIndexValue )
		 {
			  // nothing to write
		 }

		 public override void ReadValue( PageCursor cursor, NativeIndexValue into, int valueSize )
		 {
			  // nothing to read
		 }

		 public override bool FixedSize()
		 {
			  return true; // for the most case
		 }

		 public override long Identifier()
		 {
			  return _identifier;
		 }

		 public override int MajorVersion()
		 {
			  return _majorVersion;
		 }

		 public override int MinorVersion()
		 {
			  return _minorVersion;
		 }

		 public override int Compare( KEY o1, KEY o2 )
		 {
			  int valueComparison = CompareValue( o1, o2 );
			  if ( valueComparison == 0 )
			  {
					// This is a special case where we need also compare entityId to support inclusive/exclusive
					if ( o1.CompareId & o2.CompareId )
					{
						 return Long.compare( o1.EntityId, o2.EntityId );
					}
			  }
			  return valueComparison;
		 }

		 internal virtual int CompareValue( KEY o1, KEY o2 )
		 {
			  return o1.compareValueTo( o2 );
		 }
	}

}