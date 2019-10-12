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
namespace Neo4Net.Kernel.impl.transaction.command
{
	public interface NeoCommandType
	{
		 // means the first byte of the command record was only written but second
		 // (saying what type) did not get written but the file still got expanded
	}

	public static class NeoCommandType_Fields
	{
		 public static readonly sbyte None = ( sbyte ) 0;
		 public static readonly sbyte NodeCommand = ( sbyte ) 1;
		 public static readonly sbyte PropCommand = ( sbyte ) 2;
		 public static readonly sbyte RelCommand = ( sbyte ) 3;
		 public static readonly sbyte RelTypeCommand = ( sbyte ) 4;
		 public static readonly sbyte PropIndexCommand = ( sbyte ) 5;
		 public static readonly sbyte NeostoreCommand = ( sbyte ) 6;
		 public static readonly sbyte SchemaRuleCommand = ( sbyte ) 7;
		 public static readonly sbyte LabelKeyCommand = ( sbyte ) 8;
		 public static readonly sbyte RelGroupCommand = ( sbyte ) 9;
		 public static readonly sbyte IndexDefineCommand = ( sbyte ) 10;
		 public static readonly sbyte IndexAddCommand = ( sbyte ) 11;
		 public static readonly sbyte IndexAddRelationshipCommand = ( sbyte ) 12;
		 public static readonly sbyte IndexRemoveCommand = ( sbyte ) 13;
		 public static readonly sbyte IndexDeleteCommand = ( sbyte ) 14;
		 public static readonly sbyte IndexCreateCommand = ( sbyte ) 15;
		 public static readonly sbyte UpdateRelationshipCountsCommand = ( sbyte ) 16;
		 public static readonly sbyte UpdateNodeCountsCommand = ( sbyte ) 17;
	}

}