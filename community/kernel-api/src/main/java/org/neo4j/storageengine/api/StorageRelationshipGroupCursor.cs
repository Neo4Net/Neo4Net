﻿/*
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
namespace Org.Neo4j.Storageengine.Api
{
	public interface StorageRelationshipGroupCursor : StorageCursor
	{
		 void SetCurrent( int groupReference, int firstOut, int firstIn, int firstLoop );

		 int Type();

		 int OutgoingCount();

		 int IncomingCount();

		 int LoopCount();

		 /// <returns> reference to a starting point for outgoing relationships with this type. Can be passed into <seealso cref="init(long, long)"/> at a later point. </returns>
		 long OutgoingReference();

		 /// <returns> reference to a starting point for outgoing relationships with this type. Can be passed into <seealso cref="init(long, long)"/> at a later point. </returns>
		 long IncomingReference();

		 /// <returns> reference to a starting point for outgoing relationships with this type. Can be passed into <seealso cref="init(long, long)"/> at a later point. </returns>
		 long LoopsReference();

		 long OwningNode { get; }

		 long GroupReference();

		 void Init( long nodeReference, long reference );
	}

}