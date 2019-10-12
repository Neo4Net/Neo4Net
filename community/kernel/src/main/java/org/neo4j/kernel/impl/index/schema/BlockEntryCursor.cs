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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	/// <summary>
	/// Cursor that traverse over <seealso cref="BlockEntry"/> = key-value pairs. Instead of handing out <seealso cref="BlockEntry"/> instances, it provides direct access to key and
	/// value. Implementing classes are allowed to reuse key and value handed out through <seealso cref="key()"/> and <seealso cref="value()"/>.
	/// </summary>
	public interface BlockEntryCursor<KEY, VALUE> : System.IDisposable
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean next() throws java.io.IOException;
		 bool Next();

		 KEY Key();

		 VALUE Value();
	}

}