using System;

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
//	import static org.Neo4Net.codegen.TypeReference.typeReference;

	public class FieldReference
	{
		 public static FieldReference Field( TypeReference owner, TypeReference type, string name )
		 {
			  return new FieldReference( Modifier.PUBLIC, owner, type, name );
		 }

		 public static FieldReference StaticField( TypeReference owner, TypeReference type, string name )
		 {
			  return new FieldReference( Modifier.STATIC | Modifier.PRIVATE, owner, type, name );
		 }

		 public static FieldReference StaticField( Type owner, Type type, string name )
		 {
			  return StaticField( typeReference( owner ), typeReference( type ), name );
		 }

		 private readonly int _modifiers;
		 private readonly TypeReference _owner;
		 private readonly TypeReference _type;
		 private readonly string _name;

		 internal FieldReference( int modifiers, TypeReference owner, TypeReference type, string name )
		 {
			  this._modifiers = modifiers;
			  this._owner = owner;
			  this._type = type;
			  this._name = name;
		 }

		 public virtual TypeReference Owner()
		 {
			  return _owner;
		 }

		 public virtual TypeReference Type()
		 {
			  return _type;
		 }

		 public virtual string Name()
		 {
			  return _name;
		 }

		 public virtual bool Static
		 {
			 get
			 {
				  return Modifier.isStatic( _modifiers );
			 }
		 }

		 public virtual bool Final
		 {
			 get
			 {
				  return Modifier.isFinal( _modifiers );
			 }
		 }

		 public virtual int Modifiers()
		 {
			  return _modifiers;
		 }
	}

}