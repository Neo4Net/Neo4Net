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
namespace Org.Neo4j.Kernel.impl.proc
{

	using QualifiedName = Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName;

	/// <summary>
	/// Simple in memory store for procedures.
	/// 
	/// Should only be accessed from a single thread </summary>
	/// @param <T> the type to be stored </param>
	internal class ProcedureHolder<T>
	{
		 private readonly IDictionary<QualifiedName, int> _nameToId = new Dictionary<QualifiedName, int>();
		 private readonly IDictionary<QualifiedName, int> _caseInsensitveName2Id = new Dictionary<QualifiedName, int>();
		 private readonly List<T> _store = new List<T>();

		 internal virtual T Get( QualifiedName name )
		 {
			  int? id = Name2Id( name );
			  if ( id == null )
			  {
					return default( T );
			  }
			  return _store[id];
		 }

		 internal virtual T Get( int id )
		 {
			  return _store[id];
		 }

		 internal virtual void Put( QualifiedName name, T item, bool caseInsensitive )
		 {
			  int id = _store.Count;
			  _store.Add( item );
			  _nameToId[name] = id;
			  if ( caseInsensitive )
			  {
					_caseInsensitveName2Id[ToLowerCaseName( name )] = id;
			  }
			  else
			  {
					_caseInsensitveName2Id.Remove( ToLowerCaseName( name ) );
			  }
		 }

		 internal virtual int IdOf( QualifiedName name )
		 {
			  int? id = Name2Id( name );
			  if ( id == null )
			  {
					throw new NoSuchElementException();
			  }

			  return id.Value;
		 }

		 internal virtual IList<T> All()
		 {
			  return Collections.unmodifiableList( _store );
		 }

		 private int? Name2Id( QualifiedName name )
		 {
			  int? id = _nameToId[name];
			  if ( id == null )
			  { // Did not find it in the case sensitive lookup - let's check for case insensitive objects
					QualifiedName lowerCaseName = ToLowerCaseName( name );
					id = _caseInsensitveName2Id[lowerCaseName];
			  }
			  return id;
		 }

		 private QualifiedName ToLowerCaseName( QualifiedName name )
		 {
			  string[] oldNs = name.Namespace();
			  string[] lowerCaseNamespace = new string[oldNs.Length];
			  for ( int i = 0; i < oldNs.Length; i++ )
			  {
					lowerCaseNamespace[i] = oldNs[i].ToLower();
			  }
			  string lowercaseName = name.Name().ToLower();
			  return new QualifiedName( lowerCaseNamespace, lowercaseName );
		 }
	}

}