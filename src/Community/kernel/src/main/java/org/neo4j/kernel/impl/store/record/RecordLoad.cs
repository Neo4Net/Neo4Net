using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.store.record
{
	using CursorException = Neo4Net.Io.pagecache.CursorException;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	/// <summary>
	/// Specifies what happens when loading records, based on inUse status.
	/// 
	/// Roughly this is what happens for the different modes:
	/// <ul>
	/// <li><seealso cref="RecordLoad.CHECK"/>: Load at least data to determine whether or not it's in use.
	/// If in use then record is loaded into target and returns {@code true},
	/// otherwise return {@code false}.</li>
	/// <li><seealso cref="RecordLoad.NORMAL"/>: Load at least data to determine whether or not it's in use.
	/// if in use then record is loaded into target returns {@code true},
	/// otherwise throws <seealso cref="InvalidRecordException"/>.</li>
	/// <li><seealso cref="RecordLoad.FORCE"/>: Loads record data into target regardless of whether or not record in use.
	/// Returns whether or not record is in use.
	/// 
	/// </summary>
	public sealed class RecordLoad
	{
		 public static readonly RecordLoad Normal = new RecordLoad( "Normal", InnerEnum.Normal );
		 public static readonly RecordLoad Check = new RecordLoad( "Check", InnerEnum.Check );
		 public static readonly RecordLoad Force = new RecordLoad( "Force", InnerEnum.Force );

		 private static readonly IList<RecordLoad> valueList = new List<RecordLoad>();

		 static RecordLoad()
		 {
			 valueList.Add( Normal );
			 valueList.Add( Check );
			 valueList.Add( Force );
		 }

		 public enum InnerEnum
		 {
			 Normal,
			 Check,
			 Force
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private RecordLoad( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 /// <summary>
		 /// Checks whether or not a record should be fully loaded from <seealso cref="PageCursor"/>, based on inUse status.
		 /// </summary>
		 public bool ShouldLoad( bool inUse )
		 {
			  // FORCE mode always return true so that record data will always be loaded, even if not in use.
			  // The other modes only loads records that are in use.
			  return this == FORCE | inUse;
		 }

		 /// <summary>
		 /// Verifies that a record's in use status is in line with the mode, might throw <seealso cref="InvalidRecordException"/>.
		 /// </summary>
		 public bool Verify( AbstractBaseRecord record )
		 {
			  bool inUse = record.InUse();
			  if ( this == NORMAL & !inUse )
			  {
					throw new InvalidRecordException( record + " not in use" );
			  }
			  return this == FORCE | inUse;
		 }

		 /// <summary>
		 /// Depending on the mode, this will - if a cursor error has been raised on the given <seealso cref="PageCursor"/> - either
		 /// throw an <seealso cref="InvalidRecordException"/> with the underlying <seealso cref="CursorException"/>, or clear the error condition
		 /// on the cursor. </summary>
		 /// <param name="cursor"> The <seealso cref="PageCursor"/> to be checked for errors. </param>
		 public void ClearOrThrowCursorError( Neo4Net.Io.pagecache.PageCursor cursor )
		 {
			  if ( this == NORMAL )
			  {
					try
					{
						 cursor.CheckAndClearCursorException();
					}
					catch ( CursorException e )
					{
						 throw new InvalidRecordException( e );
					}
			  }
			  else
			  {
					// The CHECK and FORCE modes do not bother with reporting decoding errors...
					// ... but they must still clear them, since the page cursor may be reused to read other records
					cursor.ClearCursorException();
			  }
		 }

		 /// <summary>
		 /// Checks the given <seealso cref="PageCursor"/> to see if its out-of-bounds flag has been raised, and returns {@code true} if
		 /// that is the case <em>and</em> and out-of-bounds condition should be reported up the stack. </summary>
		 /// <param name="cursor"> The <seealso cref="PageCursor"/> to check the bounds flag for. </param>
		 /// <returns> {@code true} if an out-of-bounds condition should be reported up the stack, {@code false} otherwise. </returns>
		 public bool CheckForOutOfBounds( Neo4Net.Io.pagecache.PageCursor cursor )
		 {
			  return cursor.CheckAndClearBoundsFlag() & this == NORMAL;
		 }

		public static IList<RecordLoad> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static RecordLoad valueOf( string name )
		{
			foreach ( RecordLoad enumInstance in RecordLoad.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}