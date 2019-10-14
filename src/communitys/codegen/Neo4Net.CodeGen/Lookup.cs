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
	internal abstract class Lookup<T>
	{
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static Lookup<FieldReference> field(final TypeReference type, final String name)
		 internal static Lookup<FieldReference> Field( TypeReference type, string name )
		 {
			  return new LookupAnonymousInnerClass( type, name );
		 }

		 private class LookupAnonymousInnerClass : Lookup<FieldReference>
		 {
			 private Neo4Net.CodeGen.TypeReference _type;
			 private string _name;

			 public LookupAnonymousInnerClass( Neo4Net.CodeGen.TypeReference type, string name )
			 {
				 this._type = type;
				 this._name = name;
			 }

			 internal override FieldReference lookup( CodeBlock method )
			 {
				  FieldReference field = method.Clazz.getField( _name );
				  if ( field == null )
				  {
						throw new System.ArgumentException( method.Clazz.handle() + " has no such field: " + _name + " of type " + _type );
				  }
				  else if ( !_type.Equals( field.Type() ) )
				  {
						throw new System.ArgumentException( method.Clazz.handle() + " has no such field: " + _name + " of type " + _type + ", actual field has type: " + field.Type() );
				  }
				  return field;
			 }
		 }

		 internal abstract T Lookup( CodeBlock method );
	}

}