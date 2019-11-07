using System;
using System.Text;

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
namespace Neo4Net.CodeGen
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.VOID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.typeReference;

	public class Parameter
	{
		 public static Parameter Param( Type type, string name )
		 {
			  return Param( typeReference( type ), name );
		 }

		 public static Parameter Param( TypeReference type, string name )
		 {
			  return new Parameter( requireNonNull( type, "TypeReference" ), RequireValidName( name ) );
		 }

		 internal static readonly Parameter[] NoParameters = new Parameter[0];

		 private readonly TypeReference _type;
		 private readonly string _name;

		 private Parameter( TypeReference type, string name )
		 {
			  if ( type == VOID )
			  {
					throw new System.ArgumentException( "Variables cannot be declared as void." );
			  }
			  this._type = type;
			  this._name = name;
		 }

		 public override string ToString()
		 {
			  return WriteTo( new StringBuilder() ).ToString();
		 }

		 internal virtual StringBuilder WriteTo( StringBuilder result )
		 {
			  result.Append( "Parameter[ " );
			  _type.WriteTo( result );
			  return result.Append( " " ).Append( _name ).Append( " ]" );
		 }

		 public virtual TypeReference Type()
		 {
			  return _type;
		 }

		 public virtual string Name()
		 {
			  return _name;
		 }

		 internal static string RequireValidName( string name )
		 {
			  if ( string.ReferenceEquals( name, null ) )
			  {
					throw new System.NullReferenceException( "name" );
			  }
			  NotKeyword( name );
			  if ( !Character.isJavaIdentifierStart( char.ConvertToUtf32( name, 0 ) ) )
			  {
					throw new System.ArgumentException( "Invalid name: " + name );
			  }
			  for ( int i = 0, cp; i < name.Length; i += Character.charCount( cp ) )
			  {
					if ( !Character.isJavaIdentifierPart( cp = char.ConvertToUtf32( name, i ) ) )
					{
						 throw new System.ArgumentException( "Invalid name: " + name );
					}
			  }
			  return name;
		 }

		 private static void NotKeyword( string name )
		 {
			  switch ( name )
			  {
			  case "abstract":
			  case "continue":
			  case "for":
			  case "new":
			  case "switch":
			  case "assert":
			  case "default":
			  case "goto":
			  case "package":
			  case "synchronized":
			  case "boolean":
			  case "do":
			  case "if":
			  case "private":
			  case "break":
			  case "double":
			  case "implements":
			  case "protected":
			  case "throw":
			  case "byte":
			  case "else":
			  case "import":
			  case "public":
			  case "throws":
			  case "case":
			  case "enum":
			  case "instanceof":
			  case "return":
			  case "transient":
			  case "catch":
			  case "extends":
			  case "int":
			  case "short":
			  case "try":
			  case "char":
			  case "final":
			  case "interface":
			  case "static":
			  case "void":
			  case "class":
			  case "finally":
			  case "long":
			  case "strictfp":
			  case "volatile":
			  case "const":
			  case "float":
			  case "native":
			  case "super":
			  case "while":
					throw new System.ArgumentException( "'" + name + "' is a java keyword" );
			  case "this":
			  case "null":
			  case "true":
			  case "false":
					throw new System.ArgumentException( "'" + name + "' is a reserved name" );
			  default:
					break;
			  }
		 }

		 internal virtual bool VarArg
		 {
			 get
			 {
				  return false;
			 }
		 }
	}

}