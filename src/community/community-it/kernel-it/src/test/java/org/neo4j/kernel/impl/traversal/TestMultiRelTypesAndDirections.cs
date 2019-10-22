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
namespace Neo4Net.Kernel.impl.traversal
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Path = Neo4Net.GraphDb.Path;
	using PathExpanders = Neo4Net.GraphDb.PathExpanders;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TraversalDescription = Neo4Net.GraphDb.traversal.TraversalDescription;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Direction.OUTGOING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.RelationshipType.withName;

	public class TestMultiRelTypesAndDirections : TraversalTestBase
	{
		 private static readonly RelationshipType _one = withName( "ONE" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupGraph()
		 public virtual void SetupGraph()
		 {
			  CreateGraph( "A ONE B", "B ONE C", "A TWO C" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCIsReturnedOnDepthTwoDepthFirst()
		 public virtual void TestCIsReturnedOnDepthTwoDepthFirst()
		 {
			  TestCIsReturnedOnDepthTwo( GraphDb.traversalDescription().depthFirst() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCIsReturnedOnDepthTwoBreadthFirst()
		 public virtual void TestCIsReturnedOnDepthTwoBreadthFirst()
		 {
			  TestCIsReturnedOnDepthTwo( GraphDb.traversalDescription().breadthFirst() );
		 }

		 private void TestCIsReturnedOnDepthTwo( TraversalDescription description )
		 {
			  using ( Transaction transaction = BeginTx() )
			  {
					description = description.Expand( PathExpanders.forTypeAndDirection( _one, OUTGOING ) );
					int i = 0;
					foreach ( Path position in description.Traverse( Node( "A" ) ) )
					{
						 assertEquals( i++, position.Length() );
					}
			  }
		 }
	}

}