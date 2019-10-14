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
namespace Neo4Net.Consistency.statistics
{
	/// <summary>
	/// Increments counts of different types, per thread. Able to sum a count for all threads as well.
	/// </summary>
	public interface Counts
	{

		 void IncAndGet( Counts_Type type, int threadIndex );

		 long Sum( Counts_Type type );

		 void Reset();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 Counts NONE = new Counts()
	//	 {
	//		  @@Override public void reset()
	//		  {
	//		  }
	//
	//		  @@Override public void incAndGet(Type type, int threadIndex)
	//		  {
	//		  }
	//
	//		  @@Override public long sum(Type type)
	//		  {
	//				return 0;
	//		  }
	//	 };
	}

	 public enum Counts_Type
	 {
		  SkipCheck,
		  MissCheck,
		  Checked,
		  CheckErrors,
		  LegacySkip,
		  CorrectSkipCheck,
		  SkipBackup,
		  Overwrite,
		  NoCacheSkip,
		  ActiveCache,
		  ClearCache,
		  NoCache,
		  RelSourcePrevCheck,
		  RelSourceNextCheck,
		  RelTargetPrevCheck,
		  RelTargetNextCheck,
		  RelCacheCheck,
		  ForwardLinks,
		  BackLinks,
		  NullLinks,
		  NodeSparse,
		  NodeDense
	 }

}