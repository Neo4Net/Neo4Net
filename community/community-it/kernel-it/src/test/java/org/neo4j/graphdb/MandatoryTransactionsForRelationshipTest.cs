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
namespace Org.Neo4j.Graphdb
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;

	public class MandatoryTransactionsForRelationshipTest : AbstractMandatoryTransactionsTest<Relationship>
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRequireTransactionsWhenCallingMethodsOnRelationshipFacade()
		 public virtual void ShouldRequireTransactionsWhenCallingMethodsOnRelationshipFacade()
		 {
			  AssertFacadeMethodsThrowNotInTransaction( ObtainEntity(), RelationshipFacadeMethods.values() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateWhenCallingMethodsOnRelationshipFacade()
		 public virtual void ShouldTerminateWhenCallingMethodsOnRelationshipFacade()
		 {
			  AssertFacadeMethodsThrowAfterTerminate( RelationshipFacadeMethods.values() );
		 }

		 protected internal override Relationship ObtainEntityInTransaction( GraphDatabaseService graphDatabaseService )
		 {
			  return graphDatabaseService.CreateNode().createRelationshipTo(graphDatabaseService.CreateNode(), withName("foo"));
		 }
	}

}