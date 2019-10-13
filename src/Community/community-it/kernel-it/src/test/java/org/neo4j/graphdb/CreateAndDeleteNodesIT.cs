﻿/*
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
namespace Neo4Net.Graphdb
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseServiceCleaner = Neo4Net.Test.GraphDatabaseServiceCleaner;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

	public class CreateAndDeleteNodesIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();

		 internal enum RelTypes
		 {
			  Asd
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void creatingAndDeletingEntitiesShouldNotThrow()
		 public virtual void CreatingAndDeletingEntitiesShouldNotThrow()
		 {
			  // Given
			  GraphDatabaseService dataBase = DbRule.GraphDatabaseAPI;
			  Node myNode;

			  // When
			  using ( Transaction bobTransaction = dataBase.BeginTx() )
			  {
					myNode = dataBase.CreateNode();
					myNode.SetProperty( "Name", "Bob" );

					myNode.CreateRelationshipTo( dataBase.CreateNode(), RelTypes.Asd );
					bobTransaction.Success();
			  }

			  // When
			  GraphDatabaseServiceCleaner.cleanupAllRelationshipsAndNodes( dataBase );
		 }
	}

}