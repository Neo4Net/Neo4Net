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
namespace Org.Neo4j.Kernel.Impl.Api.index.sampling
{
	public sealed class IndexSamplingMode
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       TRIGGER_REBUILD_ALL(false, true) { public String toString() { return "FORCE REBUILD"; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       TRIGGER_REBUILD_UPDATED(true, true) { public String toString() { return "REBUILD OUTDATED"; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       BACKGROUND_REBUILD_UPDATED(true, false) { public String toString() { return "BACKGROUND-REBUILD OF OUTDATED"; } };

		 private static readonly IList<IndexSamplingMode> valueList = new List<IndexSamplingMode>();

		 static IndexSamplingMode()
		 {
			 valueList.Add( TRIGGER_REBUILD_ALL );
			 valueList.Add( TRIGGER_REBUILD_UPDATED );
			 valueList.Add( BACKGROUND_REBUILD_UPDATED );
		 }

		 public enum InnerEnum
		 {
			 TRIGGER_REBUILD_ALL,
			 TRIGGER_REBUILD_UPDATED,
			 BACKGROUND_REBUILD_UPDATED
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private IndexSamplingMode( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public readonly bool sampleOnlyIfUpdated;
		 public readonly bool blockUntilAllScheduled;

		 public static readonly IndexSamplingMode IndexSamplingMode( boolean sampleOnlyIfUpdated, boolean blockUntilAllScheduled ) { this.sampleOnlyIfUpdated = sampleOnlyIfUpdated; this.blockUntilAllScheduled = blockUntilAllScheduled; } = new IndexSamplingMode( "IndexSamplingMode(boolean sampleOnlyIfUpdated, boolean blockUntilAllScheduled) { this.sampleOnlyIfUpdated = sampleOnlyIfUpdated; this.blockUntilAllScheduled = blockUntilAllScheduled; }", InnerEnum.IndexSamplingMode( boolean sampleOnlyIfUpdated, boolean blockUntilAllScheduled ) { this.sampleOnlyIfUpdated = sampleOnlyIfUpdated; this.blockUntilAllScheduled = blockUntilAllScheduled; } );

		public static IList<IndexSamplingMode> values()
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

		public static IndexSamplingMode valueOf( string name )
		{
			foreach ( IndexSamplingMode enumInstance in IndexSamplingMode.valueList )
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