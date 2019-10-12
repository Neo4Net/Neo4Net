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
namespace Org.Neo4j.Io.pagecache.randomharness
{
	/// <summary>
	/// An enum of the commands that the RandomPageCacheTestHarness can perform, and their default probability factors.
	/// </summary>
	public sealed class Command
	{
		 public static readonly Command ReadRecord = new Command( "ReadRecord", InnerEnum.ReadRecord, 0.3 );
		 public static readonly Command WriteRecord = new Command( "WriteRecord", InnerEnum.WriteRecord, 0.6 );
		 public static readonly Command ReadMulti = new Command( "ReadMulti", InnerEnum.ReadMulti, 0.18 );
		 public static readonly Command WriteMulti = new Command( "WriteMulti", InnerEnum.WriteMulti, 0.09 );
		 public static readonly Command FlushFile = new Command( "FlushFile", InnerEnum.FlushFile, 0.06 );
		 public static readonly Command FlushCache = new Command( "FlushCache", InnerEnum.FlushCache, 0.02 );
		 public static readonly Command MapFile = new Command( "MapFile", InnerEnum.MapFile, 0.01 );
		 public static readonly Command UnmapFile = new Command( "UnmapFile", InnerEnum.UnmapFile, 0.01 );

		 private static readonly IList<Command> valueList = new List<Command>();

		 static Command()
		 {
			 valueList.Add( ReadRecord );
			 valueList.Add( WriteRecord );
			 valueList.Add( ReadMulti );
			 valueList.Add( WriteMulti );
			 valueList.Add( FlushFile );
			 valueList.Add( FlushCache );
			 valueList.Add( MapFile );
			 valueList.Add( UnmapFile );
		 }

		 public enum InnerEnum
		 {
			 ReadRecord,
			 WriteRecord,
			 ReadMulti,
			 WriteMulti,
			 FlushFile,
			 FlushCache,
			 MapFile,
			 UnmapFile
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private readonly double defaultProbability;

		 internal Command( string name, InnerEnum innerEnum, double defaultProbability )
		 {
			  this._defaultProbability = defaultProbability;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public double DefaultProbabilityFactor
		 {
			 get
			 {
				  return _defaultProbability;
			 }
		 }

		public static IList<Command> values()
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

		public static Command valueOf( string name )
		{
			foreach ( Command enumInstance in Command.valueList )
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