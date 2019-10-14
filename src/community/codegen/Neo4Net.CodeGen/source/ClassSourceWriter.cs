using System.Collections.Generic;
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
namespace Neo4Net.CodeGen.Source
{


	internal class ClassSourceWriter : ClassEmitter
	{
		 private readonly StringBuilder _target;
		 internal readonly Configuration Configuration;

		 internal ClassSourceWriter( StringBuilder target, Configuration configuration )
		 {
			  this._target = target;
			  this.Configuration = configuration;
		 }

		 internal virtual void DeclarePackage( TypeReference type )
		 {
			  Append( "package " ).Append( type.PackageName() ).Append(";\n");
		 }

		 internal virtual void Javadoc( string javadoc )
		 {
			  Append( "/** " ).Append( javadoc ).Append( " */\n" );
		 }

		 internal virtual void PublicClass( TypeReference type )
		 {
			  Append( "public class " ).Append( type.Name() );
		 }

		 internal virtual void ExtendClass( TypeReference @base )
		 {
			  Append( " extends " ).Append( @base.FullName() ).Append("\n");
		 }

		 internal virtual void Implement( TypeReference[] interfaces )
		 {
			  string prefix = "    implements ";
			  foreach ( TypeReference iFace in interfaces )
			  {
					Append( prefix ).Append( iFace.FullName() );
					prefix = ", ";
			  }
			  if ( prefix.Length == 2 )
			  {
					Append( "\n" );
			  }
		 }

		 internal virtual void Begin()
		 {
			  Append( "{\n" );
		 }

		 public override MethodEmitter Method( MethodDeclaration signature )
		 {
			  StringBuilder target = new StringBuilder();
			  if ( signature.Constructor )
			  {
					if ( signature.Static )
					{
						 target.Append( "    static\n    {\n" );
						 return new MethodSourceWriter( target, this );
					}
					else
					{
						 target.Append( "    " ).Append( Modifier.ToString( signature.Modifiers() ) ).Append(" ");
						 TypeParameters( target, signature );
						 target.Append( signature.DeclaringClass().name() );
					}
			  }
			  else
			  {
					target.Append( "    " ).Append( Modifier.ToString( signature.Modifiers() ) ).Append(" ");
					TypeParameters( target, signature );
					target.Append( signature.ReturnType().fullName() ).Append(" ").Append(signature.Name());
			  }
			  target.Append( "(" );
			  string prefix = " ";
			  foreach ( Parameter parameter in signature.Parameters() )
			  {
					target.Append( prefix ).Append( parameter.Type().fullName() ).Append(" ").Append(parameter.Name());
					prefix = ", ";
			  }
			  if ( prefix.Length > 1 )
			  {
					target.Append( " " );
			  }
			  target.Append( ')' );
			  string sep = " throws ";
			  foreach ( TypeReference thrown in signature.ThrowsList() )
			  {
					target.Append( sep ).Append( thrown.FullName() );
					sep = ", ";
			  }
			  target.Append( "\n    {\n" );
			  return new MethodSourceWriter( target, this );
		 }

		 private static void TypeParameters( StringBuilder target, MethodDeclaration method )
		 {
			  IList<MethodDeclaration.TypeParameter> parameters = method.TypeParameters();
			  if ( parameters.Count > 0 )
			  {
					target.Append( '<' );
					string sep = "";
					foreach ( MethodDeclaration.TypeParameter parameter in parameters )
					{
						 target.Append( sep ).Append( parameter.Name() );
						 TypeReference ext = parameter.ExtendsBound();
						 TypeReference sup = parameter.SuperBound();
						 if ( ext != null )
						 {
							  target.Append( " extends " ).Append( ext.FullName() );
						 }
						 else if ( sup != null )
						 {
							  target.Append( " super " ).Append( sup.FullName() );
						 }
						 sep = ", ";
					}
					target.Append( "> " );
			  }
		 }

		 public override void Done()
		 {
			  Append( "}\n" );
		 }

		 public override void Field( FieldReference field, Expression value )
		 {
			  string modifiers = Modifier.ToString( field.Modifiers() );
			  Append( "    " ).Append( modifiers );
			  if ( modifiers.Length > 0 )
			  {
					Append( " " );
			  }
			  Append( field.Type().fullName() ).Append(' ').Append(field.Name());
			  if ( value != null )
			  {
					Append( " = " );
					value.Accept( new MethodSourceWriter( _target, this ) );
			  }
			  Append( ";\n" );
		 }

		 internal virtual StringBuilder Append( CharSequence chars )
		 {
			  return _target.Append( chars );
		 }
	}

}