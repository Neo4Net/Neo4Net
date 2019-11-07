﻿using System.Collections.Generic;
using Xunit;

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

namespace Neo4Net.GraphDb.Impl.Traversal
{
    using Evaluation = Neo4Net.GraphDb.Traversal.Evaluation;
    using IEvaluator = Neo4Net.GraphDb.Traversal.IEvaluator;
    using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
    using ITraversalBranch = Neo4Net.GraphDb.Traversal.ITraversalBranch;
    using Mockito = org.mockito.Mockito;

    //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    //	import static org.junit.Assert.assertEquals;
    //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    //	import static org.junit.Assert.assertNull;
    //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    //	import static org.mockito.Mockito.mock;
    //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    //	import static org.mockito.Mockito.when;

    public class StandardBranchCollisionDetectorTest
    {

        [Fact]
        public  void TestFilteredPathEvaluation()
        {
            
            IPropertyContainer endNode = mock(typeof(INode));
            IPropertyContainer alternativeEndNode = mock(typeof(INode));
            
            INode startNode = mock(typeof(INode));
            IEvaluator evaluator = mock(typeof(IEvaluator));
            ITraversalBranch branch = mock(typeof(ITraversalBranch));
            ITraversalBranch alternativeBranch = mock(typeof(ITraversalBranch));

            when(branch.GetEnumerator()).thenAnswer(new IteratorAnswer(endNode));
            when(alternativeBranch.GetEnumerator()).thenAnswer(new IteratorAnswer(alternativeEndNode));
            when(alternativeBranch.StartNode).thenReturn(startNode);
            when(evaluator.Evaluate(Mockito.any(typeof(Path)))).thenReturn(Evaluation.IncludeAndContinue);
            StandardBranchCollisionDetector collisionDetector = new StandardBranchCollisionDetector(evaluator, path => alternativeEndNode.Equals(path.endNode()) && startNode.Equals(path.startNode()));

            ICollection<IPath> incoming = collisionDetector.Evaluate(branch, Direction.INCOMING);
            ICollection<IPath> outgoing = collisionDetector.Evaluate(branch, Direction.OUTGOING);
            ICollection<IPath> alternativeIncoming = collisionDetector.Evaluate(alternativeBranch, Direction.INCOMING);
            ICollection<IPath> alternativeOutgoing = collisionDetector.Evaluate(alternativeBranch, Direction.OUTGOING);

            Assert.Null(incoming);
            Assert.Null(outgoing);
            Assert.Null(alternativeIncoming);
            Assert.Equal(1, alternativeOutgoing.Count);
        }

        private class IteratorAnswer : Answer<object>
        {
            internal readonly IPropertyContainer EndNode;

            internal IteratorAnswer(IPropertyContainer endNode)
            {
                this.EndNode = endNode;
            }

            public override object Answer(InvocationOnMock invocation)
            {
                return Arrays.asList(EndNode).GetEnumerator();
            }
        }
    }
}