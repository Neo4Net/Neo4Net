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
namespace Neo4Net.Kernel.Impl.Api.store
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;

	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using Neo4Net.Kernel.Api.StorageEngine;

	public interface RelationshipIterator : Neo4Net.Kernel.Api.StorageEngine.RelationshipVisitor_Home, LongIterator
	{
		 /// <summary>
		 /// Can be called to visit the data about the most recent id returned from <seealso cref="next()"/>.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <EXCEPTION extends Exception> boolean relationshipVisit(long relationshipId, Neo4Net.Kernel.Api.StorageEngine.RelationshipVisitor<EXCEPTION> visitor) throws EXCEPTION;
		 bool relationshipVisit<EXCEPTION>( long relationshipId, RelationshipVisitor<EXCEPTION> visitor );
	}

	public static class RelationshipIterator_Fields
	{
		 public static readonly RelationshipIterator Empty = new Empty();
	}

	 public class RelationshipIterator_Empty : PrimitiveLongCollections.PrimitiveLongBaseIterator, RelationshipIterator
	 {
		  public override bool RelationshipVisit<EXCEPTION>( long relationshipId, RelationshipVisitor<EXCEPTION> visitor ) where EXCEPTION : Exception
		  { // Nothing to visit
				return false;
		  }

		  protected internal override bool FetchNext()
		  {
				return false;
		  }
	 }

}