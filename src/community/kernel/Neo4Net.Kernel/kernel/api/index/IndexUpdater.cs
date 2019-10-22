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
namespace Neo4Net.Kernel.Api.Index
{
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;

	/// <summary>
	/// IndexUpdaters are responsible for updating indexes during the commit process. There is one new instance handling
	/// each commit, created from <seealso cref="org.Neo4Net.kernel.api.index.IndexAccessor"/>.
	/// 
	/// <seealso cref="process(IndexEntryUpdate)"/> is called for each entry, wherein the actual updates are applied.
	/// 
	/// Each IndexUpdater is not thread-safe, and is assumed to be instantiated per transaction.
	/// </summary>
	public interface IndexUpdater : IDisposable
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void process(IndexEntryUpdate<?> update) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 void process<T1>( IndexEntryUpdate<T1> update );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void close() throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException;
		 void Close();
	}

}