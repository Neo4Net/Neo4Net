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
//	import static Neo4Net.codegen.TypeReference.typeReference;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.typeReferences;

	public class MethodReference
	{
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static MethodReference MethodReferenceConflict( Type owner, Type returns, string name, params Type[] parameters )
		 {
			  try
			  {
					int modifiers = owner.GetMethod( name, parameters ).Modifiers;
					return methodReference( typeReference( owner ), typeReference( returns ), name, modifiers, typeReferences( parameters ) );
			  }
			  catch ( NoSuchMethodException e )
			  {
					throw new System.ArgumentException( "No method with name " + name, e );
			  }

		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static MethodReference MethodReferenceConflict( Type owner, TypeReference returns, string name, params Type[] parameters )
		 {
			  try
			  {
					int modifiers = owner.GetMethod( name, parameters ).Modifiers;
					return methodReference( owner, returns, name, modifiers, typeReferences( parameters ) );
			  }
			  catch ( NoSuchMethodException e )
			  {
					throw new System.ArgumentException( "No method with name " + name, e );
			  }

		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 private static MethodReference MethodReferenceConflict( Type owner, TypeReference returns, string name, int modifiers, params TypeReference[] parameters )
		 {
			  return methodReference( typeReference( owner ), returns, name, modifiers, parameters );
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static MethodReference MethodReferenceConflict( TypeReference owner, TypeReference returns, string name, params TypeReference[] parameters )
		 {
			  return new MethodReference( owner, name, returns, Modifier.PUBLIC, parameters );
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static MethodReference MethodReferenceConflict( TypeReference owner, TypeReference returns, string name, int modifiers, params TypeReference[] parameters )
		 {
			  return new MethodReference( owner, name, returns, modifiers, parameters );
		 }

		 public static MethodReference ConstructorReference( Type owner, Type firstParameter, params Type[] parameters )
		 {
			  return ConstructorReference( typeReference( owner ), typeReferences( firstParameter, parameters ) );
		 }

		 public static MethodReference ConstructorReference( Type owner, params TypeReference[] parameters )
		 {
			  return ConstructorReference( typeReference( owner ), parameters );
		 }

		 public static MethodReference ConstructorReference( TypeReference owner, params TypeReference[] parameters )
		 {
			  return new MethodReference( owner, "<init>", TypeReference.VoidConflict, Modifier.PUBLIC, parameters );
		 }

		 private readonly TypeReference _owner;
		 private readonly string _name;
		 private readonly TypeReference _returns;
		 private readonly TypeReference[] _parameters;
		 private readonly int _modifiers;

		 internal MethodReference( TypeReference owner, string name, TypeReference returns, int modifiers, TypeReference[] parameters )
		 {
			  this._owner = owner;

			  this._name = name;
			  this._returns = returns;
			  this._modifiers = modifiers;
			  this._parameters = parameters;
		 }

		 public virtual string Name()
		 {
			  return _name;
		 }

		 public virtual TypeReference Owner()
		 {
			  return _owner;
		 }

		 public virtual TypeReference Returns()
		 {
			  return _returns;
		 }

		 public virtual TypeReference[] Parameters()
		 {
			  return _parameters;
		 }

		 public virtual bool Constructor
		 {
			 get
			 {
				  return "<init>".Equals( _name );
			 }
		 }

		 public virtual int Modifiers()
		 {
			  return _modifiers;
		 }

		 public override string ToString()
		 {
			  StringBuilder result = ( new StringBuilder() ).Append("MethodReference[");
			  WriteTo( result );
			  return result.Append( "]" ).ToString();
		 }

		 internal virtual void WriteTo( StringBuilder result )
		 {
			  _owner.WriteTo( result );
			  result.Append( "#" ).Append( _name ).Append( "(...)" );
		 }
	}

}