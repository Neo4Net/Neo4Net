using System;

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
namespace Neo4Net.Codegen
{

	public class ClassHandle : TypeReference
	{
		 private readonly TypeReference _parent;
		 internal readonly CodeGenerator Generator;
		 private readonly long _generation;

		 internal ClassHandle( string packageName, string name, TypeReference parent, CodeGenerator generator, long generation ) : base( packageName, name, parent.Primitive, parent.Array, false, "", Modifier.PUBLIC )
		 {
			  this._parent = parent;
			  this.Generator = generator;
			  this._generation = generation;
		 }

		 public override bool Equals( object obj )
		 {
			  return obj == this;
		 }

		 public override int GetHashCode()
		 {
			  return SimpleName().GetHashCode();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object newInstance() throws CompilationFailureException, IllegalAccessException, InstantiationException
		 public virtual object NewInstance()
		 {
			  return System.Activator.CreateInstance( LoadClass() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Class loadClass() throws CompilationFailureException
		 public virtual Type LoadClass()
		 {
			  return Generator.loadClass( FullName(), _generation );
		 }

		 public virtual TypeReference Parent()
		 {
			  return _parent;
		 }
	}

}