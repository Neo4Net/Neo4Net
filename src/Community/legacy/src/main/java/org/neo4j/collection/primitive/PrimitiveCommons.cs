using System;

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
namespace Neo4Net.Collection.primitive
{
	/// <summary>
	/// Package-private static methods shared between Primitive, PrimitiveIntCollections and PrimitiveLongCollections
	/// </summary>
	internal class PrimitiveCommons
	{
		 private PrimitiveCommons()
		 {
		 }

		 /// <summary>
		 /// If the given obj is AutoCloseable, then close it.
		 /// Any exceptions thrown from the close method will be wrapped in RuntimeExceptions.
		 /// </summary>
		 internal static void CloseSafely( object obj )
		 {
			  CloseSafely( obj, null );
		 }

		 /// <summary>
		 /// If the given obj is AutoCloseable, then close it.
		 /// Any exceptions thrown from the close method will be wrapped in RuntimeExceptions.
		 /// These RuntimeExceptions can get the given suppressedException attached to them, if any.
		 /// If the given suppressedException argument is null, then it will not be added to the
		 /// thrown RuntimeException.
		 /// </summary>
		 internal static void CloseSafely( object obj, Exception suppressedException )
		 {
			  if ( obj is AutoCloseable )
			  {
					AutoCloseable closeable = ( AutoCloseable ) obj;
					try
					{
						 closeable.close();
					}
					catch ( Exception cause )
					{
						 Exception exception = new Exception( cause );
						 if ( suppressedException != null )
						 {
							  exception.addSuppressed( suppressedException );
						 }
						 throw exception;
					}
			  }
		 }
	}

}