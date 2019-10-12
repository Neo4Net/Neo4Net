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
namespace Org.Neo4j.Kernel.impl.traversal
{
	using Test = org.junit.Test;

	using Path = Org.Neo4j.Graphdb.Path;
	using Resource = Org.Neo4j.Graphdb.Resource;
	using Org.Neo4j.Graphdb.traversal;
	using Evaluation = Org.Neo4j.Graphdb.traversal.Evaluation;
	using TraversalBranch = Org.Neo4j.Graphdb.traversal.TraversalBranch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;

	public class AbstractTraverserIteratorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseResourceOnce()
		 public virtual void ShouldCloseResourceOnce()
		 {
			  AbstractTraverserIterator iter = new AbstractTraverserIteratorAnonymousInnerClass( this );

			  iter.Close();
			  iter.Close(); // should not fail
		 }

		 private class AbstractTraverserIteratorAnonymousInnerClass : AbstractTraverserIterator
		 {
			 private readonly AbstractTraverserIteratorTest _outerInstance;

			 public AbstractTraverserIteratorAnonymousInnerClass( AbstractTraverserIteratorTest outerInstance ) : base( new AssertOneClose() )
			 {
				 this.outerInstance = outerInstance;
			 }


			 protected internal override Path fetchNextOrNull()
			 {
				  return null;
			 }

			 public override bool isUniqueFirst( TraversalBranch branch )
			 {
				  return false;
			 }

			 public override bool isUnique( TraversalBranch branch )
			 {
				  return false;
			 }

			 public override Evaluation evaluate<STATE>( TraversalBranch branch, BranchState<STATE> state )
			 {
				  return null;
			 }
		 }

		 private class AssertOneClose : Resource
		 {
			  internal bool IsClosed;

			  public override void Close()
			  {
					assertThat( "resource is closed", IsClosed, equalTo( false ) );
					IsClosed = true;
			  }
		 }
	}

}