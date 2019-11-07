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
namespace Neo4Net.Kernel.Api.explicitindex
{
	/// <summary>
	/// Abstract interface for accessing legacy auto indexing facilities for nodes and relationships
	/// </summary>
	/// <seealso cref= AutoIndexOperations </seealso>
	/// <seealso cref= Neo4Net.kernel.impl.api.explicitindex.InternalAutoIndexing </seealso>
	public interface AutoIndexing
	{
		 /// <summary>
		 /// An instance of <seealso cref="AutoIndexing"/> that only returns <seealso cref="AutoIndexOperations"/>
		 /// that throw <seealso cref="System.NotSupportedException"/> when any method is invoked on them.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 AutoIndexing UNSUPPORTED = new AutoIndexing()
	//	 {
	//		  @@Override public AutoIndexOperations nodes()
	//		  {
	//				return AutoIndexOperations.UNSUPPORTED;
	//		  }
	//
	//		  @@Override public AutoIndexOperations relationships()
	//		  {
	//				return AutoIndexOperations.UNSUPPORTED;
	//		  }
	//	 };

		 /// <returns> <seealso cref="AutoIndexOperations"/> for nodes </returns>
		 AutoIndexOperations Nodes();

		 /// <returns> <seealso cref="AutoIndexOperations"/> for relationships </returns>
		 AutoIndexOperations Relationships();
	}

}