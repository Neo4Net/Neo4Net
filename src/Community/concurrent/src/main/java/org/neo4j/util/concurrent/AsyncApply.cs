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
namespace Neo4Net.Util.concurrent
{

	/// <summary>
	/// The past or future application of work submitted asynchronously to a <seealso cref="WorkSync"/>.
	/// </summary>
	public interface AsyncApply
	{
		 /// <summary>
		 /// Await the application of the work submitted to a <seealso cref="WorkSync"/>.
		 /// <para>
		 /// If the work is already done, then this method with return immediately.
		 /// </para>
		 /// <para>
		 /// If the work has not been done, then this method will attempt to grab the {@code WorkSync} lock to complete the
		 /// work, or block to wait for another thread to complete the work on behalf of the current thread.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <exception cref="ExecutionException"> if this thread ends up performing the work, and an exception is thrown from the
		 /// attempt to apply the work. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void await() throws java.util.concurrent.ExecutionException;
		 void Await();
	}

}