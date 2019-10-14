using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Converters.identity;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Converters.withDefault;

	public class OptionalNamedArg : NamedArgument
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly string NameConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly string ExampleValueConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly string DefaultValueConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly string DescriptionConflict;
		 protected internal readonly string[] AllowedValues;

		 public OptionalNamedArg( string name, string exampleValue, string defaultValue, string description )
		 {
			  this.NameConflict = name;
			  this.ExampleValueConflict = exampleValue;
			  this.DefaultValueConflict = defaultValue;
			  this.DescriptionConflict = description;
			  AllowedValues = new string[]{};
		 }

		 public OptionalNamedArg( string name, string[] allowedValues, string defaultValue, string description )
		 {
			  this.NameConflict = name;
			  this.AllowedValues = allowedValues;
			  this.ExampleValueConflict = string.join( "|", allowedValues );
			  this.DefaultValueConflict = defaultValue;
			  this.DescriptionConflict = description;
		 }

		 public override string OptionsListing()
		 {
			  return string.Format( "--{0}=<{1}>", NameConflict, ExampleValueConflict );
		 }

		 public override string Usage()
		 {
			  return string.Format( "[--{0}=<{1}>]", NameConflict, ExampleValueConflict );
		 }

		 public override string Description()
		 {
			  return DescriptionConflict;
		 }

		 public override string Name()
		 {
			  return NameConflict;
		 }

		 public override string ExampleValue()
		 {
			  return ExampleValueConflict;
		 }

		 public virtual string DefaultValue()
		 {
			  return DefaultValueConflict;
		 }

		 public override string Parse( Args parsedArgs )
		 {
			  string value = parsedArgs.InterpretOption( NameConflict, withDefault( DefaultValueConflict ), identity() );
			  if ( AllowedValues.Length > 0 )
			  {
					foreach ( string allowedValue in AllowedValues )
					{
						 if ( allowedValue.Equals( value ) )
						 {
							  return value;
						 }
					}
					throw new System.ArgumentException( string.Format( "'{0}' must be one of [{1}], not: {2}", NameConflict, string.join( ",", AllowedValues ), value ) );
			  }
			  return value;
		 }

		 public override ICollection<string> ParseMultiple( Args parsedArgs )
		 {
			  ICollection<string> values = parsedArgs.InterpretOptions( NameConflict, withDefault( DefaultValueConflict ), identity() );
			  foreach ( string value in values )
			  {
					if ( AllowedValues.Length > 0 )
					{
						 IList<string> allowed = Arrays.asList( AllowedValues );
						 if ( !allowed.Contains( value ) )
						 {
							  throw new System.ArgumentException( string.Format( "'{0}' must be one of [{1}], not: {2}", NameConflict, string.join( ",", AllowedValues ), value ) );
						 }
					}
			  }
			  return values;
		 }
	}

}