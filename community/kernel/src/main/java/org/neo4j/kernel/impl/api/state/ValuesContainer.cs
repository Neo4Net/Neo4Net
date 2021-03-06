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
namespace Org.Neo4j.Kernel.Impl.Api.state
{

	using Resource = Org.Neo4j.Graphdb.Resource;
	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// A collection of values that associates a special {@code long} reference to each added value.
	/// Instances must be closed with <seealso cref="close()"/> to release underlying resources.
	/// </summary>
	public interface ValuesContainer : Resource
	{
		 /// <param name="value"> value to add </param>
		 /// <returns> a reference associated with the value, that can be passed to <seealso cref="get(long)"/> and <seealso cref="remove(long)"/> </returns>
		 /// <exception cref="IllegalStateException"> if container is closed </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: long add(@Nonnull Value value);
		 long Add( Value value );

		 /// <param name="ref"> a reference obtained from <seealso cref="add(Value)"/> </param>
		 /// <returns> a value associated with the reference </returns>
		 /// <exception cref="IllegalStateException"> if container is closed </exception>
		 /// <exception cref="IllegalArgumentException"> if reference is invalid or associated value was removed </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull Value get(long ref);
		 Value Get( long @ref );

		 /// <param name="ref"> a reference obtained from <seealso cref="add(Value)"/> </param>
		 /// <returns> a value associated with the reference </returns>
		 /// <exception cref="IllegalStateException"> if container is closed </exception>
		 /// <exception cref="IllegalArgumentException"> if reference is invalid or associated value was removed </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull Value remove(long ref);
		 Value Remove( long @ref );
	}

}