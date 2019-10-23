using System;
using System.Collections.Generic;

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

namespace Neo4Net.GraphDb.impl.traversal
{
   using BranchCollisionDetector = Neo4Net.GraphDb.Traversal.BranchCollisionDetector;
   using Evaluation = Neo4Net.GraphDb.Traversal.Evaluation;
   using Evaluator = Neo4Net.GraphDb.Traversal.Evaluator;
   using Predicates = Neo4Net.Functions.Predicates;
   using ITraversalBranch = Neo4Net.GraphDb.Traversal.ITraversalBranch;

   public class StandardBranchCollisionDetector : BranchCollisionDetector
   {
      private readonly IDictionary<INode, ICollection<ITraversalBranch>[]> _paths = new Dictionary<INode, ICollection<ITraversalBranch>[]>(1000);
      private readonly Evaluator _evaluator;
      private readonly ISet<IPath> _returnedPaths = new HashSet<IPath>();
      private System.Predicate<IPath> _pathPredicate = Predicates.alwaysTrue();

      [Obsolete]
      public StandardBranchCollisionDetector(Evaluator evaluator)
      {
         _evaluator = evaluator;
      }

      public StandardBranchCollisionDetector(Evaluator evaluator, System.Predicate<IPath> pathPredicate)
      {
         _evaluator = evaluator;
         if (pathPredicate != null)
         {
            _pathPredicate = pathPredicate;
         }
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      //ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public java.util.Collection<org.Neo4Net.graphdb.Path> evaluate(org.Neo4Net.graphdb.traversal.TraversalBranch branch, org.Neo4Net.graphdb.Direction direction)
      public override ICollection<IPath> Evaluate(ITraversalBranch branch, Direction direction)
      {
         // [0] for paths from start, [1] for paths from end
         ICollection<ITraversalBranch>[] pathsHere = _paths[branch.EndNode];
         int index = direction.Ordinal;
         if (pathsHere == null)
         {
            pathsHere = new System.Collections.ICollection[]
            {
                  new List<ITraversalBranch>(),
                  new List<ITraversalBranch>()
            };
            _paths[branch.EndNode] = pathsHere;
         }
         pathsHere[index].Add(branch);

         // If there are paths from the other side then include all the
         // combined paths
         ICollection<ITraversalBranch> otherCollections = pathsHere[index == 0 ? 1 : 0];
         if (otherCollections.Count > 0)
         {
            ICollection<IPath> foundPaths = new List<IPath>();
            foreach (ITraversalBranch otherBranch in otherCollections)
            {
               ITraversalBranch startPath = index == 0 ? branch : otherBranch;
               ITraversalBranch endPath = index == 0 ? otherBranch : branch;
               BidirectionalTraversalBranchPath path = new BidirectionalTraversalBranchPath(startPath, endPath);
               if (IsAcceptablePath(path))
               {
                  if (_returnedPaths.Add(path) && IncludePath(path, startPath, endPath))
                  {
                     foundPaths.Add(path);
                  }
               }
            }

            if (foundPaths.Count > 0)
            {
               return foundPaths;
            }
         }
         return null;
      }

      private bool IsAcceptablePath(BidirectionalTraversalBranchPath path)
      {
         return _pathPredicate.test(path);
      }

      protected internal virtual bool IncludePath(IPath path, ITraversalBranch startPath, ITraversalBranch endPath)
      {
         Evaluation eval = _evaluator.evaluate(path);
         if (!eval.continues())
         {
            startPath.Evaluation(eval);
            endPath.Evaluation(eval);
         }
         return eval.includes();
      }
   }
}