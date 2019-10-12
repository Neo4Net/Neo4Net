using System;

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
namespace Neo4Net.Bolt.v1.runtime.bookmarking
{

	using BoltResponseHandler = Neo4Net.Bolt.runtime.BoltResponseHandler;
	using AnyValue = Neo4Net.Values.AnyValue;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Values = Neo4Net.Values.Storable.Values;
	using ListValue = Neo4Net.Values.@virtual.ListValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	public class Bookmark
	{
		 private const string BOOKMARK_KEY = "bookmark";
		 private const string BOOKMARKS_KEY = "bookmarks";
		 internal const string BOOKMARK_TX_PREFIX = "neo4j:bookmark:v1:tx";

		 private readonly long _txId;

		 public Bookmark( long txId )
		 {
			  this._txId = txId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static Bookmark fromParamsOrNull(org.neo4j.values.virtual.MapValue params) throws BookmarkFormatException
		 public static Bookmark FromParamsOrNull( MapValue @params )
		 {
			  // try to parse multiple bookmarks, if available
			  Bookmark bookmark = ParseMultipleBookmarks( @params );
			  if ( bookmark == null )
			  {
					// fallback to parsing single bookmark, if available, for backwards compatibility reasons
					// some older drivers can only send a single bookmark
					return ParseSingleBookmark( @params );
			  }
			  return bookmark;
		 }

		 public virtual long TxId()
		 {
			  return _txId;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  Bookmark bookmark = ( Bookmark ) o;
			  return _txId == bookmark._txId;
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _txId );
		 }

		 public override string ToString()
		 {
			  return format( BOOKMARK_TX_PREFIX + "%d", _txId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static Bookmark parseMultipleBookmarks(org.neo4j.values.virtual.MapValue params) throws BookmarkFormatException
		 private static Bookmark ParseMultipleBookmarks( MapValue @params )
		 {
			  AnyValue bookmarksObject = @params.Get( BOOKMARKS_KEY );

			  if ( bookmarksObject == Values.NO_VALUE )
			  {
					return null;
			  }
			  else if ( bookmarksObject is ListValue )
			  {
					ListValue bookmarks = ( ListValue ) bookmarksObject;

					long maxTxId = -1;
					foreach ( AnyValue bookmark in bookmarks )
					{
						 if ( bookmark != Values.NO_VALUE )
						 {
							  long txId = TxIdFrom( bookmark );
							  if ( txId > maxTxId )
							  {
									maxTxId = txId;
							  }
						 }
					}
					return maxTxId == -1 ? null : new Bookmark( maxTxId );
			  }
			  else
			  {
					throw new BookmarkFormatException( bookmarksObject );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static Bookmark parseSingleBookmark(org.neo4j.values.virtual.MapValue params) throws BookmarkFormatException
		 private static Bookmark ParseSingleBookmark( MapValue @params )
		 {
			  AnyValue bookmarkObject = @params.Get( BOOKMARK_KEY );
			  if ( bookmarkObject == Values.NO_VALUE )
			  {
					return null;
			  }

			  return new Bookmark( TxIdFrom( bookmarkObject ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static long txIdFrom(org.neo4j.values.AnyValue bookmark) throws BookmarkFormatException
		 private static long TxIdFrom( AnyValue bookmark )
		 {
			  if ( !( bookmark is TextValue ) )
			  {
					throw new BookmarkFormatException( bookmark );
			  }
			  string bookmarkString = ( ( TextValue ) bookmark ).stringValue();
			  if ( !bookmarkString.StartsWith( BOOKMARK_TX_PREFIX, StringComparison.Ordinal ) )
			  {
					throw new BookmarkFormatException( bookmarkString );
			  }

			  try
			  {
					return long.Parse( bookmarkString.Substring( BOOKMARK_TX_PREFIX.Length ) );
			  }
			  catch ( System.FormatException e )
			  {
					throw new BookmarkFormatException( bookmarkString, e );
			  }
		 }

		 public virtual void AttachTo( BoltResponseHandler state )
		 {
			  state.OnMetadata( BOOKMARK_KEY, stringValue( ToString() ) );
		 }
	}

}