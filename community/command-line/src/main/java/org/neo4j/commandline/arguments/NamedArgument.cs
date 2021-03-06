﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Commandline.arguments
{

	using Args = Org.Neo4j.Helpers.Args;

	public interface NamedArgument
	{
		 /// <summary>
		 /// Represents the option in the options list.
		 /// </summary>
		 string OptionsListing();

		 /// <summary>
		 /// Represents the option in the usage string.
		 /// </summary>
		 string Usage();

		 /// <summary>
		 /// An explanation of the option in the options list.
		 /// </summary>
		 string Description();

		 /// <summary>
		 /// Name of the option as in '--name=<value>'
		 /// </summary>
		 string Name();

		 /// <summary>
		 /// Example value listed in usage between brackets like '--name=<example-value>'
		 /// </summary>
		 string ExampleValue();

		 /// <summary>
		 /// Parses the option (or possible default value) out of program arguments. Use only if a single argument is allowed.
		 /// </summary>
		 string Parse( Args parsedArgs );

		 /// <summary>
		 /// Parses the option (or possible default value) out of program arguments. Use in case multiple arguments are
		 /// allowed.
		 /// </summary>
		 ICollection<string> ParseMultiple( Args parsedArgs );

		 /// <summary>
		 /// Returns true if this argument was given explicitly on the command line
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default boolean has(org.neo4j.helpers.Args parsedArgs)
	//	 {
	//		  return parsedArgs.has(name());
	//	 }
	}

}