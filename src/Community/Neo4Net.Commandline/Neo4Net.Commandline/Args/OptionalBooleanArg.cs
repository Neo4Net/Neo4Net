﻿using System;

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
	using WordUtils = org.apache.commons.lang3.text.WordUtils;

	using Args = Neo4Net.Helpers.Args;

	public class OptionalBooleanArg : OptionalNamedArg
	{

		 public OptionalBooleanArg( string name, bool defaultValue, string description ) : base( name, new string[]{ "true", "false" }, Convert.ToString( defaultValue ), description )
		 {
		 }

		 public override string Usage()
		 {
			  return WordUtils.wrap( string.Format( "[--{0}[=<{1}>]]", NameConflict, ExampleValueConflict ), 60 );
		 }

		 public override string Parse( Args parsedArgs )
		 {
			  return Convert.ToString( parsedArgs.GetBoolean( NameConflict, bool.Parse( DefaultValueConflict ) ) );
		 }
	}

}