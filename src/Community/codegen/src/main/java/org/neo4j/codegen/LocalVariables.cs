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
namespace Neo4Net.Codegen
{

	/// <summary>
	/// Repository of local variables.
	/// </summary>
	public class LocalVariables
	{
		 private readonly AtomicInteger _counter = new AtomicInteger( 0 );
		 private readonly IDictionary<string, LocalVariable> _localVariables = new Dictionary<string, LocalVariable>();

		 internal virtual LocalVariable CreateNew( TypeReference type, string name )
		 {
			  if ( _localVariables.ContainsKey( name ) )
			  {
				  throw new System.InvalidOperationException( string.Format( "Local variable {0} already in scope", name ) );
			  }
			  LocalVariable localVariable = new LocalVariable( type, name, _counter.AndIncrement );
			  _localVariables[name] = localVariable;
			  //if 64 bit types we need to give it one more index
			  if ( type.SimpleName().Equals("double") || type.SimpleName().Equals("long") )
			  {
					_counter.incrementAndGet();
			  }
			  return localVariable;
		 }

		 public virtual LocalVariable Get( string name )
		 {
			  LocalVariable localVariable = _localVariables[name];
			  if ( localVariable == null )
			  {
					throw new NoSuchElementException( "No variable " + name + " in scope" );
			  }
			  return localVariable;
		 }

		 public static LocalVariables Copy( LocalVariables original )
		 {
			  LocalVariables variables = new LocalVariables();
			  variables._counter.set( original._counter.get() );
			  original._localVariables.forEach( variables._localVariables.put );
			  return variables;
		 }
	}

}