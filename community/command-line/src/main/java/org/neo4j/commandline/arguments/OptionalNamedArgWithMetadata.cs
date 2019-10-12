using System;
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
namespace Org.Neo4j.Commandline.arguments
{

	using Args = Org.Neo4j.Helpers.Args;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Converters.identity;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Converters.withDefault;

	/// <summary>
	/// Some arguments can have a variable name, such as in neo4j-admin import where the `--relationships` argument may
	/// have the following form:
	/// 
	/// --relationships:MYTYPE=file.csv
	/// 
	/// or the `--nodes` argument which can be
	/// 
	/// --nodes:TYPE1:TYPE2=file.csv
	/// 
	/// This is only used for validation, not to actually read the metadata. See ImportCommand.java.
	/// </summary>
	public class OptionalNamedArgWithMetadata : OptionalNamedArg, NamedArgument
	{
		 protected internal readonly string ExampleMetaData;

		 public OptionalNamedArgWithMetadata( string name, string exampleMetaData, string exampleValue, string defaultValue, string description ) : base( name, exampleValue, defaultValue, description )
		 {
			  this.ExampleMetaData = exampleMetaData;
		 }

		 public override string OptionsListing()
		 {
			  return string.Format( "--{0}[{1}]=<{2}>", NameConflict, ExampleMetaData, ExampleValueConflict );
		 }

		 public override string Usage()
		 {
			  return string.Format( "[--{0}[{1}]=<{2}>]", NameConflict, ExampleMetaData, ExampleValueConflict );
		 }

		 public override string Parse( Args parsedArgs )
		 {
			  throw new Exception( "Arguments with metadata only support multiple value parsing" );
		 }

		 public override ICollection<string> ParseMultiple( Args parsedArgs )
		 {
			  ICollection<Args.Option<string>> vals = parsedArgs.InterpretOptionsWithMetadata( NameConflict, withDefault( DefaultValueConflict ), identity() );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return vals.Select( Args.Option::value ).ToList();
		 }
	}


}