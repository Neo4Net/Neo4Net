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
namespace Org.Neo4j.Io.pagecache.impl.muninn
{
	using EvictionEvent = Org.Neo4j.Io.pagecache.tracing.EvictionEvent;
	using PageFaultEvent = Org.Neo4j.Io.pagecache.tracing.PageFaultEvent;

	internal class StubPageFaultEvent : PageFaultEvent
	{
		 internal long BytesRead;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal long CachePageIdConflict;

		 public override void AddBytesRead( long bytes )
		 {
			  BytesRead += bytes;
		 }

		 public virtual long CachePageId
		 {
			 set
			 {
				  this.CachePageIdConflict = value;
			 }
		 }

		 public override void Done()
		 {
		 }

		 public override void Done( Exception throwable )
		 {
		 }

		 public override EvictionEvent BeginEviction()
		 {
			  return EvictionEvent.NULL;
		 }
	}

}