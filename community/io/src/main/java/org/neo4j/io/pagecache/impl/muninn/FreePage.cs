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
namespace Org.Neo4j.Io.pagecache.impl.muninn
{

	/// <summary>
	/// A free page in the MuninnPageCache.freelist.
	/// 
	/// The next pointers are always other FreePage instances.
	/// </summary>
	internal sealed class FreePage
	{
		 internal readonly long PageRef;
		 internal int Count;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal object NextConflict;

		 internal FreePage( long pageRef )
		 {
			  this.PageRef = pageRef;
		 }

		 internal object Next
		 {
			 set
			 {
				  this.NextConflict = value;
				  if ( value == null )
				  {
						Count = 1;
				  }
				  else if ( value.GetType() == typeof(AtomicInteger) )
				  {
						Count = 1 + ( ( AtomicInteger ) value ).get();
				  }
				  else
				  {
						this.Count = 1 + ( ( FreePage ) value ).Count;
				  }
			 }
		 }
	}

}