using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Cypher.@internal.codegen
{

	/// <summary>
	/// The default implementation of a table used for a full sort by the generated code
	/// 
	/// It accepts tuples as boxed objects that implements Comparable
	/// 
	/// Implements the following interface:
	/// (since the code is generated it does not actually need to declare it with implements)
	/// 
	/// public interface SortTable<T extends Comparable<?>>
	/// {
	///     boolean add( T e );
	/// 
	///     void sort();
	/// 
	///     Iterator<T> iterator();
	/// }
	/// 
	/// This implementation just adapts Java's standard ArrayList
	/// </summary>
	public class DefaultFullSortTable<T> : List<T> // implements SortTable<T>
	{
		 public DefaultFullSortTable( int initialCapacity ) : base( initialCapacity )
		 {
		 }

		 public virtual void Sort()
		 {
			  // Sort using the default array sort implementation (currently ComparableTimSort)
			  sort( null );
		 }
	}

}