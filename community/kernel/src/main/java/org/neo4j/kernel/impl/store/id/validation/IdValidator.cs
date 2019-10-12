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
namespace Org.Neo4j.Kernel.impl.store.id.validation
{
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;

	/// <summary>
	/// Utility containing methods to validate record id used in <seealso cref="AbstractBaseRecord"/> and possibly generated by
	/// <seealso cref="IdGenerator"/>. Takes into account special reserved value <seealso cref="IdGeneratorImpl.INTEGER_MINUS_ONE"/>.
	/// </summary>
	public sealed class IdValidator
	{
		 private IdValidator()
		 {
		 }

		 /// <summary>
		 /// Checks if the given id is reserved, i.e. <seealso cref="IdGeneratorImpl.INTEGER_MINUS_ONE"/>.
		 /// </summary>
		 /// <param name="id"> the id to check. </param>
		 /// <returns> <code>true</code> if the given id is <seealso cref="IdGeneratorImpl.INTEGER_MINUS_ONE"/>, <code>false</code>
		 /// otherwise. </returns>
		 /// <seealso cref= IdGeneratorImpl#INTEGER_MINUS_ONE </seealso>
		 public static bool IsReservedId( long id )
		 {
			  return id == IdGeneratorImpl.INTEGER_MINUS_ONE;
		 }

		 /// <summary>
		 /// Asserts that the given id is valid:
		 /// <ul>
		 /// <li>non-negative
		 /// <li>less than the given max id
		 /// <li>not equal to <seealso cref="IdGeneratorImpl.INTEGER_MINUS_ONE"/>
		 /// </ul>
		 /// </summary>
		 /// <param name="id"> the id to check. </param>
		 /// <param name="maxId"> the max allowed id. </param>
		 /// <seealso cref= IdGeneratorImpl#INTEGER_MINUS_ONE </seealso>
		 public static void AssertValidId( IdType idType, long id, long maxId )
		 {
			  if ( IsReservedId( id ) )
			  {
					throw new ReservedIdException( id );
			  }
			  AssertIdWithinCapacity( idType, id, maxId );
		 }

		 /// <summary>
		 /// Asserts that the given id is valid with respect to given max id:
		 /// <ul>
		 /// <li>non-negative
		 /// <li>less than the given max id
		 /// </ul>
		 /// </summary>
		 /// <param name="idType"> </param>
		 /// <param name="id"> the id to check. </param>
		 /// <param name="maxId"> the max allowed id. </param>
		 public static void AssertIdWithinCapacity( IdType idType, long id, long maxId )
		 {
			  if ( id < 0 )
			  {
					throw new NegativeIdException( id );
			  }
			  if ( id > maxId )
			  {
					throw new IdCapacityExceededException( idType, id, maxId );
			  }
		 }

		 public static bool HasReservedIdInRange( long startIdInclusive, long endIdExclusive )
		 {
			  return startIdInclusive <= IdGeneratorImpl.INTEGER_MINUS_ONE && endIdExclusive > IdGeneratorImpl.INTEGER_MINUS_ONE;
		 }
	}

}