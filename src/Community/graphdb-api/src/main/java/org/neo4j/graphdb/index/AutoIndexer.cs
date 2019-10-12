using System;
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
namespace Neo4Net.Graphdb.index
{

	/// <summary>
	/// The primary interaction point with the auto indexing infrastructure of neo4j.
	/// From here it is possible to enable/disable the auto indexing functionality,
	/// set/unset auto indexed properties and retrieve index hits.
	/// 
	/// It only exposes a <seealso cref="ReadableIndex"/> (see <seealso cref="getAutoIndex()"/>) and
	/// the idea is that the mutating operations are managed by the AutoIndexer only
	/// and the user should have no access other than mutating operations on the
	/// database primitives.
	/// </summary>
	/// @deprecated this feature will be removed in the next major release. Please consider using schema indexes instead. 
	[Obsolete("this feature will be removed in the next major release. Please consider using schema indexes instead.")]
	public interface AutoIndexer<T> where T : Neo4Net.Graphdb.PropertyContainer
	{
		 /// <summary>
		 /// Sets the AutoIndexer as enabled or not. Enabled means that appropriately
		 /// configured properties are auto indexed and hits can be returned, disabled
		 /// means that no index additions happen but the index can be queried.
		 /// </summary>
		 /// <param name="enabled"> True to enable this auto indexer, false to disable it. </param>
		 [Obsolete]
		 bool Enabled { set;get; }


		 /// <summary>
		 /// Returns the auto index used by the auto indexer. This should be able
		 /// to be released safely (read only) to the outside world.
		 /// </summary>
		 /// <returns> A read only index </returns>
		 [Obsolete]
		 ReadableIndex<T> AutoIndex { get; }

		 /// <summary>
		 /// Start auto indexing a property. This could lead to an
		 /// IllegalStateException in case there are already ignored properties.
		 /// Adding an already auto indexed property is a no-op.
		 /// </summary>
		 /// <param name="propName"> The property name to start auto indexing. </param>
		 [Obsolete]
		 void StartAutoIndexingProperty( string propName );

		 /// <summary>
		 /// Removes the argument from the set of auto indexed properties. If
		 /// the property was not already monitored, nothing happens
		 /// </summary>
		 /// <param name="propName"> The property name to stop auto indexing. </param>
		 [Obsolete]
		 void StopAutoIndexingProperty( string propName );

		 /// <summary>
		 /// Returns the set of property names that are currently monitored for auto
		 /// indexing. If this auto indexer is set to ignore properties, the result
		 /// is the empty set.
		 /// </summary>
		 /// <returns> An immutable set of the auto indexed property names, possibly
		 ///         empty. </returns>
		 [Obsolete]
		 ISet<string> AutoIndexedProperties { get; }
	}

}