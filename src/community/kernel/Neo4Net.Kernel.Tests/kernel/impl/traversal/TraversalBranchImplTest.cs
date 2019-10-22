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
	using Test = org.junit.Test;

	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb.traversal;
	using TraversalBranch = Neo4Net.GraphDb.traversal.TraversalBranch;
	using TraversalContext = Neo4Net.GraphDb.traversal.TraversalContext;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.isNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.Evaluation.INCLUDE_AND_CONTINUE;

	public class TraversalBranchImplTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldExpandOnFirstAccess()
		 public virtual void ShouldExpandOnFirstAccess()
		 {
			  // GIVEN
			  TraversalBranch parent = mock( typeof( TraversalBranch ) );
			  Node source = mock( typeof( Node ) );
			  TraversalBranchImpl branch = new TraversalBranchImpl( parent, source );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") org.Neo4Net.graphdb.PathExpander expander = mock(org.Neo4Net.graphdb.PathExpander.class);
			  PathExpander expander = mock( typeof( PathExpander ) );
			  when( expander.expand( eq( branch ), any( typeof( BranchState ) ) ) ).thenReturn( Iterables.emptyResourceIterable() );
			  TraversalContext context = mock( typeof( TraversalContext ) );
			  when( context.Evaluate( eq( branch ), Null ) ).thenReturn( INCLUDE_AND_CONTINUE );

			  // WHEN initializing
			  branch.Initialize( expander, context );

			  // THEN the branch should not be expanded
			  verifyZeroInteractions( source );

			  // and WHEN actually traversing from it
			  branch.Next( expander, context );

			  // THEN we should expand it
			  verify( expander ).expand( any( typeof( Path ) ), any( typeof( BranchState ) ) );
		 }
	}

}