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
namespace Neo4Net.CommandLine.Args
{
	using Args = Neo4Net.Helpers.Args;

	public class OptionalPositionalArgument : PositionalArgument
	{
		 protected internal readonly string Value;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal readonly int PositionConflict;

		 public OptionalPositionalArgument( int position, string value )
		 {
			  this.PositionConflict = position;
			  this.Value = value;
		 }

		 public override int Position()
		 {
			  return PositionConflict;
		 }

		 public override string Usage()
		 {
			  return string.Format( "[<{0}>]", Value );
		 }

		 public override string Parse( Args parsedArgs )
		 {
			  return parsedArgs.Orphans()[PositionConflict];
		 }
	}

}