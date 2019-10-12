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
namespace Org.Neo4j.Kernel.Impl.Index.Schema.config
{

	using Org.Neo4j.Index.@internal.gbptree;
	using Header = Org.Neo4j.Index.@internal.gbptree.Header;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;

	/// <summary>
	/// <seealso cref="GBPTree"/> header writer for <seealso cref="SpaceFillingCurveSettings"/>.
	/// </summary>
	/// <seealso cref= SpaceFillingCurveSettingsReader </seealso>
	public class SpaceFillingCurveSettingsWriter : System.Action<PageCursor>
	{
		 internal const sbyte VERSION = 0;

		 /// <summary>
		 /// Biggest theoretical size of a stored setting.
		 /// </summary>
		 private static readonly int _worstCaseSettingsSize = Byte.BYTES + Integer.BYTES + Integer.BYTES + Integer.BYTES + ( Long.BYTES * 2 ) * 3; // two doubles per dimension (max 3 dimensions)

		 private readonly IndexSpecificSpaceFillingCurveSettingsCache _settings;

		 public SpaceFillingCurveSettingsWriter( IndexSpecificSpaceFillingCurveSettingsCache settings )
		 {
			  this._settings = settings;
		 }

		 /// <summary>
		 /// Given the <seealso cref="PageCursor"/> goes through all <seealso cref="SpaceFillingCurveSettings"/> that are in use in specific index.
		 /// The internal <seealso cref="IndexSpecificSpaceFillingCurveSettingsCache"/> provides those settings, which have been collected
		 /// from geometry values from updates.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to write index-specific <seealso cref="SpaceFillingCurveSettings"/> into. </param>
		 public override void Accept( PageCursor cursor )
		 {
			  cursor.PutByte( VERSION );
			  _settings.visitIndexSpecificSettings( new SettingVisitorAnonymousInnerClass( this, cursor ) );
		 }

		 private class SettingVisitorAnonymousInnerClass : IndexSpecificSpaceFillingCurveSettingsCache.SettingVisitor
		 {
			 private readonly SpaceFillingCurveSettingsWriter _outerInstance;

			 private PageCursor _cursor;

			 public SettingVisitorAnonymousInnerClass( SpaceFillingCurveSettingsWriter outerInstance, PageCursor cursor )
			 {
				 this.outerInstance = outerInstance;
				 this._cursor = cursor;
			 }

			 public void count( int count )
			 {
				  _cursor.putInt( count );
			 }

			 public void visit( CoordinateReferenceSystem crs, SpaceFillingCurveSettings settings )
			 {
				  // For tableId+code the native layout is even stricter here, but it'd add unnecessary complexity to shave off a couple of more bits
				  _cursor.putByte( ( sbyte ) assertInt( "table id", crs.Table.TableId, 0xFF ) );
				  _cursor.putInt( crs.Code );
				  _cursor.putShort( ( short ) assertInt( "max levels", settings.MaxLevelsConflict, 0xFFFF ) );
				  _cursor.putShort( ( short ) assertInt( "dimensions", settings.DimensionsConflict, 0xFFFF ) );
				  double[] min = settings.Extents.Min;
				  double[] max = settings.Extents.Max;
				  for ( int i = 0; i < settings.DimensionsConflict; i++ )
				  {
						_cursor.putLong( System.BitConverter.DoubleToInt64Bits( min[i] ) );
						_cursor.putLong( System.BitConverter.DoubleToInt64Bits( max[i] ) );
				  }
			 }

			 private int assertInt( string name, int value, int mask )
			 {
				  if ( ( value & ~mask ) != 0 )
				  {
						throw new System.ArgumentException( "Invalid " + name + " " + value + ", max is " + mask );
				  }
				  return value;
			 }
		 }

		 /// <summary>
		 /// Calculates max number of crs settings that can fit on a tree-state page, given {@code pageSize}.
		 /// </summary>
		 /// <param name="pageSize"> page size in the page cache. </param>
		 /// <returns> max number of crs settings a <seealso cref="GBPTree"/> tree-state page can hold given the {@code pageSize}. </returns>
		 public static int MaxNumberOfSettings( int pageSize )
		 {
			  return ( pageSize - Header.OVERHEAD - Long.BYTES ) / _worstCaseSettingsSize;
		 }
	}

}