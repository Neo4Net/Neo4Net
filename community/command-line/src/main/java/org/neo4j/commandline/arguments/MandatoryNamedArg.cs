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
//	import static org.neo4j.kernel.impl.util.Converters.mandatory;

	public class MandatoryNamedArg : NamedArgument
	{
		 private readonly string _name;
		 private readonly string _exampleValue;
		 private readonly string _description;

		 public MandatoryNamedArg( string name, string exampleValue, string description )
		 {
			  this._name = name;
			  this._exampleValue = exampleValue;
			  this._description = description;
		 }

		 public override string OptionsListing()
		 {
			  return Usage();
		 }

		 public override string Usage()
		 {
			  return string.Format( "--{0}=<{1}>", _name, _exampleValue );
		 }

		 public override string Description()
		 {
			  return _description;
		 }

		 public override string Name()
		 {
			  return _name;
		 }

		 public override string ExampleValue()
		 {
			  return _exampleValue;
		 }

		 public override string Parse( Args parsedArgs )
		 {
			  return parsedArgs.InterpretOption( _name, mandatory(), identity() );
		 }

		 public override ICollection<string> ParseMultiple( Args parsedArgs )
		 {
			  return parsedArgs.InterpretOptions( _name, mandatory(), identity() );
		 }
	}

}