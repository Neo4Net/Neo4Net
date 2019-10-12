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
namespace Neo4Net.@unsafe.Impl.Batchimport.executor
{

	using Panicable = Neo4Net.@unsafe.Impl.Batchimport.staging.Panicable;

	/// <summary>
	/// Like an <seealso cref="ExecutorService"/> with additional absolute control of the current processor count,
	/// i.e. the number of threads executing the tasks in parallel.
	/// </summary>
	/// @param <LOCAL> object/state local to each thread, that submitted <seealso cref="Task tasks"/> can get access to
	/// when <seealso cref="Task.run(object) running"/>. </param>
	public interface TaskExecutor<LOCAL> : Parallelizable, AutoCloseable, Panicable
	{
		 /// <summary>
		 /// Submits a task to be executed by one of the processors in this <seealso cref="TaskExecutor"/>. Tasks will be
		 /// executed in the order of which they arrive.
		 /// </summary>
		 /// <param name="task"> a <seealso cref="System.Threading.ThreadStart"/> defining the task to be executed. </param>
		 void Submit( Task<LOCAL> task );

		 /// <summary>
		 /// Closes this <seealso cref="TaskExecutor"/>, disallowing new tasks to be <seealso cref="submit(Task) submitted"/>.
		 /// </summary>
		 void Close();

		 /// <summary>
		 /// Asserts that this <seealso cref="TaskExecutor"/> is healthy. Useful to call when deciding to wait on a condition
		 /// this executor is expected to fulfill.
		 /// </summary>
		 /// <exception cref="RuntimeException"> of some sort if this executor is in a bad stage, the original error that
		 /// made this executor fail. </exception>
		 void AssertHealthy();
	}

}