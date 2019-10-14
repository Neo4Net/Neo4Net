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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;

	public interface IndexDropAction
	{
		 /// <summary>
		 /// Deletes the index directory and everything in it, as last part of dropping an index.
		 /// Can be configured to create archive with content of index directories for future analysis.
		 /// </summary>
		 /// <param name="indexId"> the index id, for which directory to drop. </param>
		 /// <param name="archiveExistentIndex"> create archive with content of dropped directories </param>
		 /// <seealso cref= GraphDatabaseSettings#archive_failed_index </seealso>
		 void Drop( long indexId, bool archiveExistentIndex );

		 /// <summary>
		 /// Deletes the index directory and everything in it, as last part of dropping an index.
		 /// </summary>
		 /// <param name="indexId"> the index id, for which directory to drop. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void drop(long indexId)
	//	 {
	//		  drop(indexId, false);
	//	 }
	}

}