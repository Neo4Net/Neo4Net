﻿using System;

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
namespace Neo4Net.Index.Internal.gbptree
{

	/// <summary>
	/// A job cleaning something up after recovery. Usually added to <seealso cref="RecoveryCleanupWorkCollector"/>.
	/// <para>
	/// Report state of cleaning progress.
	/// </para>
	/// </summary>
	public interface ICleanupJob
	{
		 /// <returns> {@code true} if gbptree still needs cleaning, meaning job is not yet finished or has not started at all.
		 /// {@code false} if gbptree does not need cleaning, meaning job has finished or it was never needed in the first place. </returns>
		 bool Needed();

		 /// <returns> {@code true} if the job has failed. Use <seealso cref="getCause()"/> to see cause of failure. </returns>
		 bool HasFailed();

		 /// <returns> Cause of failure if <seealso cref="hasFailed()"/> or {@code null} if job has not failed. </returns>
		 Exception Cause { get; }

		 /// <summary>
		 /// Mark this job as closed and cleanup all it's resources.
		 /// </summary>
		 void Close();

		 /// <summary>
		 /// Run cleanup job and use provided executor for parallel tasks.
		 /// This method will wait for all jobs passed to executor to finish before returning.
		 /// </summary>
		 void Run( ExecutorService executor );

		 /// <summary>
		 /// A <seealso cref="CleanupJob"/> that doesn't need cleaning, i.e. it's already clean.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 CleanupJob CLEAN = new CleanupJob()
	//	 {
	//		  @@Override public void run(ExecutorService executor)
	//		  { // no-op
	//		  }
	//
	//		  @@Override public boolean needed()
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public boolean hasFailed()
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public Throwable getCause()
	//		  {
	//				return null;
	//		  }
	//
	//		  @@Override public void close()
	//		  { // no-op
	//		  }
	//	 };
	}

}