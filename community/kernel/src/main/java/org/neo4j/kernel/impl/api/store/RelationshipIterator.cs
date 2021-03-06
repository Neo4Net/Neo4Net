﻿using System;

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
namespace Org.Neo4j.Kernel.Impl.Api.store
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;

	using PrimitiveLongCollections = Org.Neo4j.Collection.PrimitiveLongCollections;
	using Org.Neo4j.Storageengine.Api;

	public interface RelationshipIterator : Org.Neo4j.Storageengine.Api.RelationshipVisitor_Home, LongIterator
	{
		 /// <summary>
		 /// Can be called to visit the data about the most recent id returned from <seealso cref="next()"/>.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <EXCEPTION extends Exception> boolean relationshipVisit(long relationshipId, org.neo4j.storageengine.api.RelationshipVisitor<EXCEPTION> visitor) throws EXCEPTION;
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