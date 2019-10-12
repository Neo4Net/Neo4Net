using System.Collections.Generic;
using System.Text;

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
namespace Org.Neo4j.Codegen
{

	public sealed class ByteCodeUtils
	{
		 private ByteCodeUtils()
		 {
			  throw new System.NotSupportedException();
		 }

		 public static string ByteCodeName( TypeReference reference )
		 {
			  StringBuilder builder = new StringBuilder();
			  if ( reference.PackageName().Length > 0 )
			  {
					builder.Append( reference.PackageName().replaceAll("\\.", "/") ).Append('/');
			  }
			  if ( reference.InnerClass )
			  {
					builder.Append( reference.DeclaringClassName() ).Append('$');
			  }
			  builder.Append( reference.Name() );
			  return builder.ToString();
		 }

		 public static string OuterName( TypeReference reference )
		 {
			  if ( !reference.InnerClass )
			  {
					return null;
			  }

			  StringBuilder builder = new StringBuilder();
			  if ( reference.PackageName().Length > 0 )
			  {
					builder.Append( reference.PackageName().replaceAll("\\.", "/") ).Append('/');
			  }
			  builder.Append( reference.SimpleName() );

			  return builder.ToString();
		 }

		 public static string TypeName( TypeReference reference )
		 {
			  StringBuilder builder = new StringBuilder();
			  InternalType( builder, reference, false );

			  return builder.ToString();
		 }

		 public static string Desc( MethodDeclaration declaration )
		 {
			  return InternalDesc( declaration.Erased(), false );
		 }

		 public static string Desc( MethodReference reference )
		 {
			  StringBuilder builder = new StringBuilder();
			  builder.Append( "(" );
			  foreach ( TypeReference parameter in reference.Parameters() )
			  {
					InternalType( builder, parameter, false );
			  }
			  builder.Append( ")" );
			  InternalType( builder, reference.Returns(), false );

			  return builder.ToString();
		 }

		 public static string Signature( TypeReference reference )
		 {
			  if ( !reference.Generic )
			  {
					return null;
			  }

			  return InternalSignature( reference );
		 }

		 public static string Signature( MethodDeclaration declaration )
		 {
			  if ( !declaration.Generic )
			  {
					return null;
			  }
			  return InternalDesc( declaration, true );
		 }

		 public static string[] Exceptions( MethodDeclaration declaration )
		 {

			  IList<TypeReference> throwsList = declaration.Erased().throwsList();
			  if ( throwsList.Count == 0 )
			  {
					return null;
			  }
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return throwsList.Select( ByteCodeUtils.byteCodeName ).ToArray( string[]::new );
		 }

		 private static string InternalDesc( MethodDeclaration declaration, bool showErasure )
		 {
			  StringBuilder builder = new StringBuilder();
			  IList<MethodDeclaration.TypeParameter> typeParameters = declaration.TypeParameters();
			  if ( showErasure && typeParameters.Count > 0 )
			  {
					builder.Append( "<" );
					foreach ( MethodDeclaration.TypeParameter typeParameter in typeParameters )
					{
						 builder.Append( typeParameter.Name() ).Append(":");
						 InternalType( builder, typeParameter.ExtendsBound(), true );
					}
					builder.Append( ">" );
			  }
			  builder.Append( "(" );
			  foreach ( Parameter parameter in declaration.Parameters() )
			  {
					InternalType( builder, parameter.Type(), showErasure );
			  }
			  builder.Append( ")" );
			  InternalType( builder, declaration.ReturnType(), showErasure );
			  IList<TypeReference> throwsList = declaration.ThrowsList();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  if ( showErasure && throwsList.Any( TypeReference::isTypeParameter ) )
			  {
					builder.Append( "^" );
					throwsList.ForEach( t => InternalType( builder, t, false ) );
			  }
			  return builder.ToString();
		 }

		 private static string InternalSignature( TypeReference reference )
		 {
			  return InternalType( new StringBuilder(), reference, true ).ToString();
		 }

		 private static StringBuilder InternalType( StringBuilder builder, TypeReference reference, bool showErasure )
		 {
			  string name = reference.Name();
			  if ( reference.Array )
			  {
					builder.Append( "[" );
			  }

			  switch ( name )
			  {
			  case "int":
					builder.Append( "I" );
					break;
			  case "long":
					builder.Append( "J" );
					break;
			  case "byte":
					builder.Append( "B" );
					break;
			  case "short":
					builder.Append( "S" );
					break;
			  case "char":
					builder.Append( "C" );
					break;
			  case "float":
					builder.Append( "F" );
					break;
			  case "double":
					builder.Append( "D" );
					break;
			  case "boolean":
					builder.Append( "Z" );
					break;
			  case "void":
					builder.Append( "V" );
					break;

			  default:
					if ( reference.TypeParameter )
					{
						 builder.Append( "T" ).Append( name );
					}
					else
					{
						 builder.Append( "L" );
						 string packageName = reference.PackageName().replaceAll("\\.", "\\/");
						 if ( packageName.Length > 0 )
						 {
							  builder.Append( packageName ).Append( "/" );
						 }
						 if ( reference.InnerClass )
						 {
							  builder.Append( reference.DeclaringClassName() ).Append('$');
						 }
						 builder.Append( name.replaceAll( "\\.", "\\/" ) );
					}

					IList<TypeReference> parameters = reference.Parameters();
					if ( showErasure && parameters.Count > 0 )
					{
						 builder.Append( "<" );
						 parameters.ForEach( p => InternalType( builder, p, true ) );
						 builder.Append( ">" );
					}
					builder.Append( ";" );

				break;
			  }
			  return builder;
		 }
	}

}