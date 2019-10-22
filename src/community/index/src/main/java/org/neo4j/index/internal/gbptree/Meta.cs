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

	using Factory = Neo4Net.Index.Internal.gbptree.TreeNodeSelector.Factory;
	using CursorException = Neo4Net.Io.pagecache.CursorException;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.PageCursorUtil.checkOutOfBounds;

	/// <summary>
	/// About versioning (i.e. the format version {@code int}):
	/// The format version started out as one int controlling the entire version of the tree and its different types of formats.
	/// For compatibility reasons this int has been kept but used differently, i.e. split up into four individual versions,
	/// one {@code byte} each. These are:
	/// 
	/// <pre>
	///     <------- int ------>
	/// msb [ 3 ][ 2 ][ 1 ][ 0 ] lsb
	///       ▲    ▲    ▲    ▲
	///       │    │    │    │
	///       │    │    │    └──────────── <seealso cref="getFormatIdentifier()"/>
	///       │    │    └───────────────── <seealso cref="getFormatVersion()"/>
	///       │    └────────────────────── <seealso cref="getUnusedVersionSlot3()"/>
	///       └─────────────────────────── <seealso cref="getUnusedVersionSlot4()"/>
	/// </pre>
	/// 
	/// <seealso cref="CURRENT_STATE_VERSION"/> and <seealso cref="CURRENT_GBPTREE_VERSION"/> aren't used yet because they have
	/// never needed to be versioned yet, but remain reserved for future use. The are fixed at 0 a.t.m.
	/// </summary>
	public class Meta
	{
		 internal const sbyte CURRENT_STATE_VERSION = 0;
		 internal const sbyte CURRENT_GBPTREE_VERSION = 0;

		 private const int MASK_BYTE = 0xFF;

		 private static readonly int _shiftFormatIdentifier = ( sizeof( sbyte ) * 8 ) * 0;
		 private static readonly int _shiftFormatVersion = ( sizeof( sbyte ) * 8 ) * 1;
		 private static readonly int _shiftUnusedVersionSlot_3 = ( sizeof( sbyte ) * 8 ) * 2;
		 private static readonly int _shiftUnusedVersionSlot_4 = ( sizeof( sbyte ) * 8 ) * 3;
		 internal const sbyte UNUSED_VERSION = 0;

		 private readonly sbyte _formatIdentifier;
		 private readonly sbyte _formatVersion;
		 private readonly sbyte _unusedVersionSlot3;
		 private readonly sbyte _unusedVersionSlot4;
		 private readonly int _pageSize;
		 private readonly long _layoutIdentifier;
		 private readonly int _layoutMajorVersion;
		 private readonly int _layoutMinorVersion;

		 private Meta( sbyte formatIdentifier, sbyte formatVersion, sbyte unusedVersionSlot3, sbyte unusedVersionSlot4, int pageSize, long layoutIdentifier, int layoutMajorVersion, int layoutMinorVersion )
		 {
			  this._formatIdentifier = formatIdentifier;
			  this._formatVersion = formatVersion;
			  this._unusedVersionSlot3 = unusedVersionSlot3;
			  this._unusedVersionSlot4 = unusedVersionSlot4;
			  this._pageSize = pageSize;
			  this._layoutIdentifier = layoutIdentifier;
			  this._layoutMajorVersion = layoutMajorVersion;
			  this._layoutMinorVersion = layoutMinorVersion;
		 }

		 internal Meta<T1>( sbyte formatIdentifier, sbyte formatVersion, int pageSize, Layout<T1> layout ) : this( formatIdentifier, formatVersion, UNUSED_VERSION, UNUSED_VERSION, pageSize, layout.Identifier(), layout.MajorVersion(), layout.MinorVersion() )
		 {
		 }

		 private static Meta ParseMeta( int format, int pageSize, long layoutIdentifier, int majorVersion, int minorVersion )
		 {
			  return new Meta( ExtractIndividualVersion( format, _shiftFormatIdentifier ), ExtractIndividualVersion( format, _shiftFormatVersion ), ExtractIndividualVersion( format, _shiftUnusedVersionSlot_3 ), ExtractIndividualVersion( format, _shiftUnusedVersionSlot_4 ), pageSize, layoutIdentifier, majorVersion, minorVersion );
		 }

		 /// <summary>
		 /// Reads meta information from the meta page. Reading meta information also involves <seealso cref="Layout"/> in that
		 /// it can have written layout-specific information to this page too. The layout identifier and its version
		 /// that the returned <seealso cref="Meta"/> instance will have are the ones read from the page, not retrieved from <seealso cref="Layout"/>.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to read meta information from. </param>
		 /// <param name="layout"> <seealso cref="Layout"/> instance that will get the opportunity to read layout-specific data from the meta page.
		 /// {@code layout} is allowed to be {@code null} where it won't be told to read layout-specific data from the meta page. </param>
		 /// <returns> <seealso cref="Meta"/> instance with all meta information. </returns>
		 /// <exception cref="IOException"> on <seealso cref="PageCursor"/> I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static Meta read(org.Neo4Net.io.pagecache.PageCursor cursor, Layout<?,?> layout) throws java.io.IOException
		 internal static Meta Read<T1>( PageCursor cursor, Layout<T1> layout )
		 {
			  int format;
			  int pageSize;
			  long layoutIdentifier;
			  int layoutMajorVersion;
			  int layoutMinorVersion;
			  try
			  {
					do
					{
						 format = cursor.Int;
						 pageSize = cursor.Int;
						 layoutIdentifier = cursor.Long;
						 layoutMajorVersion = cursor.Int;
						 layoutMinorVersion = cursor.Int;
						 if ( layout != null )
						 {
							  layout.ReadMetaData( cursor );
						 }
					} while ( cursor.ShouldRetry() );
					checkOutOfBounds( cursor );
					cursor.CheckAndClearCursorException();
			  }
			  catch ( CursorException e )
			  {
					throw new MetadataMismatchException( e, "Tried to open, but caught an error while reading meta data. " + "File is expected to be corrupt, try to rebuild." );
			  }

			  return ParseMeta( format, pageSize, layoutIdentifier, layoutMajorVersion, layoutMinorVersion );
		 }

		 public virtual void Verify<T1>( Layout<T1> layout )
		 {
			  if ( _unusedVersionSlot3 != Meta.UNUSED_VERSION )
			  {
					throw new MetadataMismatchException( "Unexpected version " + _unusedVersionSlot3 + " for unused version slot 3" );
			  }
			  if ( _unusedVersionSlot4 != Meta.UNUSED_VERSION )
			  {
					throw new MetadataMismatchException( "Unexpected version " + _unusedVersionSlot4 + " for unused version slot 4" );
			  }

			  if ( !layout.CompatibleWith( _layoutIdentifier, _layoutMajorVersion, _layoutMinorVersion ) )
			  {
					throw new MetadataMismatchException( "Tried to open using layout not compatible with " + "what the index was created with. Created with: layoutIdentifier=%d,majorVersion=%d,minorVersion=%d. " + "Opened with layoutIdentifier=%d,majorVersion=%d,minorVersion=%d", _layoutIdentifier, _layoutMajorVersion, _layoutMinorVersion, layout.Identifier(), layout.MajorVersion(), layout.MinorVersion() );
			  }

			  Factory formatByLayout = TreeNodeSelector.SelectByLayout( layout );
			  if ( formatByLayout.FormatIdentifier() != _formatIdentifier || formatByLayout.FormatVersion() != _formatVersion )
			  {
					throw new MetadataMismatchException( "Tried to open using layout not compatible with what index was created with. " + "Created with formatIdentifier:%d,formatVersion:%d. Opened with formatIdentifier:%d,formatVersion%d", _formatIdentifier, _formatVersion, formatByLayout.FormatIdentifier(), formatByLayout.FormatVersion() );
			  }
		 }

		 /// <summary>
		 /// Writes meta information to the meta page. Writing meta information also involves <seealso cref="Layout"/> in that
		 /// it can write layout-specific information to this page too.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to read meta information from. </param>
		 /// <param name="layout"> <seealso cref="Layout"/> instance that will get the opportunity to write layout-specific data to the meta page. </param>
		 internal virtual void Write<T1>( PageCursor cursor, Layout<T1> layout )
		 {
			  cursor.PutInt( AllVersionsCombined() );
			  cursor.PutInt( PageSize );
			  cursor.PutLong( LayoutIdentifier );
			  cursor.PutInt( LayoutMajorVersion );
			  cursor.PutInt( LayoutMinorVersion );
			  layout.WriteMetaData( cursor );
			  checkOutOfBounds( cursor );
		 }

		 private static sbyte ExtractIndividualVersion( int format, int shift )
		 {
			  return ( sbyte )( ( ( int )( ( uint )format >> shift ) ) & MASK_BYTE );
		 }

		 private int AllVersionsCombined()
		 {
			  return _formatIdentifier << _shiftFormatIdentifier | _formatVersion << _shiftFormatVersion;
		 }

		 internal virtual int PageSize
		 {
			 get
			 {
				  return _pageSize;
			 }
		 }

		 internal virtual sbyte FormatIdentifier
		 {
			 get
			 {
				  return _formatIdentifier;
			 }
		 }

		 internal virtual sbyte FormatVersion
		 {
			 get
			 {
				  return _formatVersion;
			 }
		 }

		 internal virtual sbyte UnusedVersionSlot3
		 {
			 get
			 {
				  return _unusedVersionSlot3;
			 }
		 }

		 internal virtual sbyte UnusedVersionSlot4
		 {
			 get
			 {
				  return _unusedVersionSlot4;
			 }
		 }

		 internal virtual long LayoutIdentifier
		 {
			 get
			 {
				  return _layoutIdentifier;
			 }
		 }

		 internal virtual int LayoutMajorVersion
		 {
			 get
			 {
				  return _layoutMajorVersion;
			 }
		 }

		 internal virtual int LayoutMinorVersion
		 {
			 get
			 {
				  return _layoutMinorVersion;
			 }
		 }
	}

}