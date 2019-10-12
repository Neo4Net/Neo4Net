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
namespace Neo4Net.Io.fs
{

	/// <summary>
	/// Modes describing how <seealso cref="StoreChannel"/> can be opened using <seealso cref="FileSystemAbstraction"/>.
	/// <br/>
	/// <br/>
	/// Possible values:
	/// <ul>
	/// <li>
	/// <seealso cref="READ"/>:  Open for reading only.  Invoking any of the <b>write</b>
	/// methods of the resulting object will cause an <seealso cref="java.io.IOException"/> to be thrown.
	/// </li>
	/// <li>
	/// <seealso cref="READ_WRITE"/>: Open for reading and writing.  If the file does not already
	/// exist then an attempt will be made to create it.
	/// </li>
	/// <li>
	/// <seealso cref="SYNC"/>: Open for reading and writing, as with <seealso cref="READ_WRITE"/>, and also
	/// require that every update to the file's content or metadata be written synchronously to the underlying storage
	/// device.
	/// </li>
	/// <li>
	/// <seealso cref="DSYNC"/>:  Open for reading and writing, as with <seealso cref="READ_WRITE"/>, and also
	/// require that every update to the file's content be written
	/// synchronously to the underlying storage device.
	/// </li>
	/// </ul>
	/// </summary>
	/// <seealso cref= RandomAccessFile </seealso>
	public sealed class OpenMode
	{
		 public static readonly OpenMode Read = new OpenMode( "Read", InnerEnum.Read, "r" );
		 public static readonly OpenMode ReadWrite = new OpenMode( "ReadWrite", InnerEnum.ReadWrite, "rw" );
		 public static readonly OpenMode Sync = new OpenMode( "Sync", InnerEnum.Sync, "rws" );
		 public static readonly OpenMode Dsync = new OpenMode( "Dsync", InnerEnum.Dsync, "rwd" );

		 private static readonly IList<OpenMode> valueList = new List<OpenMode>();

		 static OpenMode()
		 {
			 valueList.Add( Read );
			 valueList.Add( ReadWrite );
			 valueList.Add( Sync );
			 valueList.Add( Dsync );
		 }

		 public enum InnerEnum
		 {
			 Read,
			 ReadWrite,
			 Sync,
			 Dsync
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;

		 internal OpenMode( string name, InnerEnum innerEnum, string mode )
		 {
			  this._mode = mode;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public string Mode()
		 {
			  return _mode;
		 }

		public static IList<OpenMode> values()
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

		public static OpenMode valueOf( string name )
		{
			foreach ( OpenMode enumInstance in OpenMode.valueList )
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