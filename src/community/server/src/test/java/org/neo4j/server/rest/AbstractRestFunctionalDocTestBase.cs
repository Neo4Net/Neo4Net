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
namespace Neo4Net.Server.rest
{
	using Transaction = Neo4Net.Graphdb.Transaction;

	using Before = org.junit.Before;

	/// <summary>
	/// Remove nodes and relationships between tests.
	/// </summary>
	public class AbstractRestFunctionalDocTestBase : AbstractRestFunctionalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void removeNodesAndRelationships()
		 public virtual void RemoveNodesAndRelationships()
		 {
			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					Graphdb().execute("MATCH (n) DETACH DELETE n");
					tx.Success();
			  }
		 }
	}

}