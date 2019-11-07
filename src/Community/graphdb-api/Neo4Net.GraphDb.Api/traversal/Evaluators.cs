﻿using System.Collections.Generic;

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

namespace Neo4Net.GraphDb.Traversal
{
    //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    //	import static Arrays.asList;

    //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    	//using static Neo4Net.GraphDb.Traversal.Evaluation.InnerEnum. IncludeAndContinue;

    /// <summary>
    /// Common <seealso cref="IEvaluator"/>s useful during common traversals.
    /// </summary>
    /// <seealso cref= IEvaluator </seealso>
    /// <seealso cref= TraversalDescription </seealso>
    public abstract class Evaluators
    {
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @SuppressWarnings("rawtypes") private static final PathEvaluator<?> ALL = new PathEvaluator_Adapter()
        //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
        private static readonly IPathEvaluator<object> ALL = new PathEvaluator_AdapterAnonymousInnerClass();

        private class PathEvaluator_AdapterAnonymousInnerClass : PathEvaluator_Adapter
        {
            public Evaluation Evaluate(IPath path, IBranchState state)
            {
                return IncludeAndContinue;
            }
        }

        private static readonly IPathEvaluator _allButStartPosition = FromDepth(1);

        /// @param <STATE> the type of the state object. </param>
        /// <returns> an evaluator which includes everything it encounters and doesn't prune
        ///         anything. </returns>
        public static PathEvaluator<STATE> All<STATE>()
        {
            //noinspection unchecked
            return (PathEvaluator<STATE>)ALL;
        }

        /// <returns> an evaluator which never prunes and includes everything except
        ///         the first position, i.e. the the start node. </returns>
        public static PathEvaluator ExcludeStartPosition()
        {
            return _allButStartPosition;
        }

        /// <summary>
        /// Returns an <seealso cref="IEvaluator"/> which includes positions down to {@code depth}
        /// and prunes everything deeper than that.
        /// </summary>
        /// <param name="depth">   the max depth to traverse to. </param>
        /// @param <STATE> the type of the state object. </param>
        /// <returns> Returns an <seealso cref="IEvaluator"/> which includes positions down to
        ///         {@code depth} and prunes everything deeper than that. </returns>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
        //ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> toDepth(final int depth)
        public static PathEvaluator<STATE> ToDepth<STATE>(int depth)
        {
            return new PathEvaluator_AdapterAnonymousInnerClass2(depth);
        }

        private class PathEvaluator_AdapterAnonymousInnerClass2 : PathEvaluator_Adapter<STATE>
        {
            private int _depth;

            public PathEvaluator_AdapterAnonymousInnerClass2(int depth)
            {
                _depth = depth;
            }

            public override Evaluation Evaluate(IPath path, IBranchState state)
            {
                int pathLength = path.Length;
                return Evaluation.of(pathLength <= _depth, pathLength < _depth);
            }
        }

        /// <summary>
        /// Returns an <seealso cref="IEvaluator"/> which only includes positions from {@code depth}
        /// and deeper and never prunes anything.
        /// </summary>
        /// <param name="depth">   the depth to start include positions from. </param>
        /// @param <STATE> the type of the state object. </param>
        /// <returns> Returns an <seealso cref="IEvaluator"/> which only includes positions from
        ///         {@code depth} and deeper and never prunes anything. </returns>

        public static IPathEvaluator<STATE> FromDepth<STATE>(int depth)
        {
            return new PathEvaluator_AdapterAnonymousInnerClass3(depth);
        }

        private class PathEvaluator_AdapterAnonymousInnerClass3 : PathEvaluator_Adapter<STATE>
        {
            private int _depth;

            public PathEvaluator_AdapterAnonymousInnerClass3(int depth)
            {
                _depth = depth;
            }

            public override Evaluation Evaluate(IPath path, IBranchState state)
            {
                return Evaluation.ofIncludes(path.Length >= _depth);
            }
        }

        /// <summary>
        /// Returns an <seealso cref="IEvaluator"/> which only includes positions at {@code depth}
        /// and prunes everything deeper than that.
        /// </summary>
        /// <param name="depth">   the depth to start include positions from. </param>
        /// @param <STATE> the type of the state object. </param>
        /// <returns> Returns an <seealso cref="IEvaluator"/> which only includes positions at
        ///         {@code depth} and prunes everything deeper than that. </returns>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
        //ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> atDepth(final int depth)
        public static PathEvaluator<STATE> AtDepth<STATE>(int depth)
        {
            return new PathEvaluator_AdapterAnonymousInnerClass4(depth);
        }

        private class PathEvaluator_AdapterAnonymousInnerClass4 : PathEvaluator_Adapter<STATE>
        {
            private int _depth;

            public PathEvaluator_AdapterAnonymousInnerClass4(int depth)
            {
                _depth = depth;
            }

            public override Evaluation Evaluate(IPath path, IBranchState state)
            {
                return path.Length == _depth ? Evaluation.IncludeAndPrune : Evaluation.ExcludeAndContinue;
            }
        }

        /// <summary>
        /// Returns an <seealso cref="IEvaluator"/> which only includes positions between
        /// depths {@code minDepth} and {@code maxDepth}. It prunes everything deeper
        /// than {@code maxDepth}.
        /// </summary>
        /// <param name="minDepth"> minimum depth a position must have to be included. </param>
        /// <param name="maxDepth"> maximum depth a position must have to be included. </param>
        /// @param <STATE>  the type of the state object. </param>
        /// <returns> Returns an <seealso cref="IEvaluator"/> which only includes positions between
        ///         depths {@code minDepth} and {@code maxDepth}. It prunes everything deeper
        ///         than {@code maxDepth}. </returns>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
        //ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> includingDepths(final int minDepth, final int maxDepth)
        public static PathEvaluator<STATE> IncludingDepths<STATE>(int minDepth, int maxDepth)
        {
            return new PathEvaluator_AdapterAnonymousInnerClass5(minDepth, maxDepth);
        }

        private class PathEvaluator_AdapterAnonymousInnerClass5 : PathEvaluator_Adapter<STATE>
        {
            private int _minDepth;
            private int _maxDepth;

            public PathEvaluator_AdapterAnonymousInnerClass5(int minDepth, int maxDepth)
            {
                _minDepth = minDepth;
                _maxDepth = maxDepth;
            }

            public override Evaluation Evaluate(IPath path, IBranchState state)
            {
                int length = path.Length;
                return Evaluation.of(length >= _minDepth && length <= _maxDepth, length < _maxDepth);
            }
        }

        /// <summary>
        /// Returns an <seealso cref="IEvaluator"/> which compares the type of the last relationship
        /// in a <seealso cref="IPath"/> to a given set of relationship types (one or more).If the type of
        /// the last relationship in a path is of one of the given types then
        /// {@code evaluationIfMatch} will be returned, otherwise
        /// {@code evaluationIfNoMatch} will be returned.
        /// </summary>
        /// <param name="evaluationIfMatch">   the <seealso cref="Evaluation"/> to return if the type of the
        ///                            last relationship in the path matches any of the given types. </param>
        /// <param name="evaluationIfNoMatch"> the <seealso cref="Evaluation"/> to return if the type of the
        ///                            last relationship in the path doesn't match any of the given types. </param>
        /// <param name="type">                the (first) type (of possibly many) to match the last relationship
        ///                            in paths with. </param>
        /// <param name="orAnyOfTheseTypes">   additional types to match the last relationship in
        ///                            paths with. </param>
        /// @param <STATE>             the type of the state object. </param>
        /// <returns> an <seealso cref="IEvaluator"/> which compares the type of the last relationship
        ///         in a <seealso cref="IPath"/> to a given set of relationship types. </returns>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
        //ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> lastRelationshipTypeIs(final Evaluation evaluationIfMatch, final Evaluation evaluationIfNoMatch, final Neo4Net.GraphDb.RelationshipType type, Neo4Net.GraphDb.RelationshipType... orAnyOfTheseTypes)
        public static PathEvaluator<STATE> LastRelationshipTypeIs<STATE>(Evaluation evaluationIfMatch, Evaluation evaluationIfNoMatch, IRelationshipType type, params IRelationshipType[] orAnyOfTheseTypes)
        {
            if (orAnyOfTheseTypes.Length == 0)
            {
                return new PathEvaluator_AdapterAnonymousInnerClass6(evaluationIfMatch, evaluationIfNoMatch, type);
            }

            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.Set<String> expectedTypes = new java.util.HashSet<>();
            ISet<string> expectedTypes = new HashSet<string>();
            expectedTypes.Add(type.Name);
            foreach (IRelationshipType otherType in orAnyOfTheseTypes)
            {
                expectedTypes.Add(otherType.Name);
            }

            return new PathEvaluator_AdapterAnonymousInnerClass7(evaluationIfMatch, evaluationIfNoMatch, expectedTypes);
        }

        private class PathEvaluator_AdapterAnonymousInnerClass6 : PathEvaluator_Adapter<STATE>
        {
            private Neo4Net.GraphDb.Traversal.Evaluation _evaluationIfMatch;
            private Neo4Net.GraphDb.Traversal.Evaluation _evaluationIfNoMatch;
            private IRelationshipType _type;

            public PathEvaluator_AdapterAnonymousInnerClass6(Neo4Net.GraphDb.Traversal.Evaluation evaluationIfMatch, Neo4Net.GraphDb.Traversal.Evaluation evaluationIfNoMatch, IRelationshipType type)
            {
                _evaluationIfMatch = evaluationIfMatch;
                _evaluationIfNoMatch = evaluationIfNoMatch;
                _type = type;
            }

            public override Evaluation Evaluate(IPath path, IBranchState state)
            {
                IRelationship rel = path.LastRelationship;
                return rel != null && rel.IsType(_type) ? _evaluationIfMatch : _evaluationIfNoMatch;
            }
        }

        private class PathEvaluator_AdapterAnonymousInnerClass7 : PathEvaluator_Adapter<STATE>
        {
            private Neo4Net.GraphDb.Traversal.Evaluation _evaluationIfMatch;
            private Neo4Net.GraphDb.Traversal.Evaluation _evaluationIfNoMatch;
            private ISet<string> _expectedTypes;

            public PathEvaluator_AdapterAnonymousInnerClass7(Neo4Net.GraphDb.Traversal.Evaluation evaluationIfMatch, Neo4Net.GraphDb.Traversal.Evaluation evaluationIfNoMatch, ISet<string> expectedTypes)
            {
                _evaluationIfMatch = evaluationIfMatch;
                _evaluationIfNoMatch = evaluationIfNoMatch;
                _expectedTypes = expectedTypes;
            }

            public override Evaluation Evaluate(IPath path, IBranchState state)
            {
                IRelationship lastRelationship = path.LastRelationship;
                if (lastRelationship == null)
                {
                    return _evaluationIfNoMatch;
                }

                return _expectedTypes.Contains(lastRelationship.Type.name()) ? _evaluationIfMatch : _evaluationIfNoMatch;
            }
        }

        /// <param name="type">              the (first) type (of possibly many) to match the last relationship
        ///                          in paths with. </param>
        /// <param name="orAnyOfTheseTypes"> types to match the last relationship in paths with. If any matches
        ///                          it's considered a match. </param>
        /// @param <STATE>           the type of the state object. </param>
        /// <returns> an <seealso cref="IEvaluator"/> which compares the type of the last relationship
        ///         in a <seealso cref="IPath"/> to a given set of relationship types. </returns>
        /// <seealso cref= #lastRelationshipTypeIs(Evaluation, Evaluation, RelationshipType, RelationshipType...)
        ///      Uses <seealso cref="Evaluation.IncludeAndContinue"/> for {@code evaluationIfMatch}
        ///      and <seealso cref="Evaluation.EXCLUDE_AND_CONTINUE"/> for {@code evaluationIfNoMatch}. </seealso>
        public static PathEvaluator<STATE> IncludeWhereLastRelationshipTypeIs<STATE>(IRelationshipType type, params IRelationshipType[] orAnyOfTheseTypes)
        {
            return LastRelationshipTypeIs(Evaluation.IncludeAndContinue, Evaluation.ExcludeAndContinue, type, orAnyOfTheseTypes);
        }

        /// <param name="type">              the (first) type (of possibly many) to match the last relationship
        ///                          in paths with. </param>
        /// <param name="orAnyOfTheseTypes"> types to match the last relationship in paths with. If any matches
        ///                          it's considered a match. </param>
        /// @param <STATE>           the type of the state object. </param>
        /// <returns> an <seealso cref="IEvaluator"/> which compares the type of the last relationship
        ///         in a <seealso cref="IPath"/> to a given set of relationship types. </returns>
        /// <seealso cref= #lastRelationshipTypeIs(Evaluation, Evaluation, RelationshipType, RelationshipType...)
        ///      Uses <seealso cref="Evaluation.INCLUDE_AND_PRUNE"/> for {@code evaluationIfMatch}
        ///      and <seealso cref="Evaluation.IncludeAndContinue"/> for {@code evaluationIfNoMatch}. </seealso>
        public static PathEvaluator<STATE> PruneWhereLastRelationshipTypeIs<STATE>(IRelationshipType type, params IRelationshipType[] orAnyOfTheseTypes)
        {
            return LastRelationshipTypeIs(Evaluation.IncludeAndPrune, Evaluation.ExcludeAndContinue, type, orAnyOfTheseTypes);
        }

        /// <summary>
        /// An <seealso cref="IEvaluator"/> which will return {@code evaluationIfMatch} if <seealso cref="IPath.endNode()"/>
        /// for a given path is any of {@code nodes}, else {@code evaluationIfNoMatch}.
        /// </summary>
        /// <param name="evaluationIfMatch">   the <seealso cref="Evaluation"/> to return if the <seealso cref="IPath.endNode()"/>
        ///                            is any of the nodes in {@code nodes}. </param>
        /// <param name="evaluationIfNoMatch"> the <seealso cref="Evaluation"/> to return if the <seealso cref="IPath.endNode()"/>
        ///                            doesn't match any of the nodes in {@code nodes}. </param>
        /// <param name="possibleEndNodes">    a set of nodes to match to end nodes in paths. </param>
        /// @param <STATE>             the type of the state object. </param>
        /// <returns> an <seealso cref="IEvaluator"/> which will return {@code evaluationIfMatch} if
        ///         <seealso cref="IPath.endNode()"/> for a given path is any of {@code nodes},
        ///         else {@code evaluationIfNoMatch}. </returns>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
        //ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> endNodeIs(final Evaluation evaluationIfMatch, final Evaluation evaluationIfNoMatch, Neo4Net.GraphDb.Node... possibleEndNodes)
        public static PathEvaluator<STATE> EndNodeIs<STATE>(Evaluation evaluationIfMatch, Evaluation evaluationIfNoMatch, params INode[] possibleEndNodes)
        {
            if (possibleEndNodes.Length == 1)
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final Neo4Net.GraphDb.Node target = possibleEndNodes[0];
                INode target = possibleEndNodes[0];
                return new PathEvaluator_AdapterAnonymousInnerClass8(evaluationIfMatch, evaluationIfNoMatch, target);
            }

            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.Set<Neo4Net.GraphDb.Node> endNodes = new java.util.HashSet<>(asList(possibleEndNodes));
            ISet<INode> endNodes = new HashSet<INode>(asList(possibleEndNodes));
            return new PathEvaluator_AdapterAnonymousInnerClass9(evaluationIfMatch, evaluationIfNoMatch, endNodes);
        }

        private class PathEvaluator_AdapterAnonymousInnerClass8 : PathEvaluator_Adapter<STATE>
        {
            private Neo4Net.GraphDb.Traversal.Evaluation _evaluationIfMatch;
            private Neo4Net.GraphDb.Traversal.Evaluation _evaluationIfNoMatch;
            private INode _target;

            public PathEvaluator_AdapterAnonymousInnerClass8(Neo4Net.GraphDb.Traversal.Evaluation evaluationIfMatch, Neo4Net.GraphDb.Traversal.Evaluation evaluationIfNoMatch, INode target)
            {
                _evaluationIfMatch = evaluationIfMatch;
                _evaluationIfNoMatch = evaluationIfNoMatch;
                _target = target;
            }

            public Evaluation Evaluate(IPath path, IBranchState state)
            {
                return _target.Equals(path.EndNode) ? _evaluationIfMatch : _evaluationIfNoMatch;
            }
        }

        private class PathEvaluator_AdapterAnonymousInnerClass9 : PathEvaluator_Adapter<STATE>
        {
            private Neo4Net.GraphDb.Traversal.Evaluation _evaluationIfMatch;
            private Neo4Net.GraphDb.Traversal.Evaluation _evaluationIfNoMatch;
            private ISet<INode> _endNodes;

            public PathEvaluator_AdapterAnonymousInnerClass9(Neo4Net.GraphDb.Traversal.Evaluation evaluationIfMatch, Neo4Net.GraphDb.Traversal.Evaluation evaluationIfNoMatch, ISet<INode> endNodes)
            {
                _evaluationIfMatch = evaluationIfMatch;
                _evaluationIfNoMatch = evaluationIfNoMatch;
                _endNodes = endNodes;
            }

            public override Evaluation Evaluate(IPath path, IBranchState state)
            {
                return _endNodes.Contains(path.EndNode) ? _evaluationIfMatch : _evaluationIfNoMatch;
            }
        }

        /// <summary>
        /// Include paths with the specified end nodes.
        ///
        /// Uses Evaluators#endNodeIs(Evaluation, Evaluation, Node...) with
        /// <seealso cref="Evaluation.IncludeAndContinue"/> for {@code evaluationIfMatch} and
        /// <seealso cref="Evaluation.EXCLUDE_AND_CONTINUE"/> for {@code evaluationIfNoMatch}.
        /// </summary>
        /// <param name="nodes">   end nodes for paths to be included in the result. </param>
        /// @param <STATE> the type of the state object. </param>
        /// <returns> paths where the end node is one of {@code nodes} </returns>
        public static PathEvaluator<STATE> IncludeWhereEndNodeIs<STATE>(params INode[] nodes)
        {
            return EndNodeIs(Evaluation.IncludeAndContinue, Evaluation.ExcludeAndContinue, nodes);
        }

        public static PathEvaluator<STATE> PruneWhereEndNodeIs<STATE>(params INode[] nodes)
        {
            return EndNodeIs(Evaluation.IncludeAndPrune, Evaluation.ExcludeAndContinue, nodes);
        }

        /// <summary>
        /// Evaluator which decides to include a <seealso cref="IPath"/> if all the
        /// {@code nodes} exist in it.
        /// </summary>
        /// <param name="nodes">   <seealso cref="INode"/>s that must exist in a <seealso cref="IPath"/> for it
        ///                to be included. </param>
        /// @param <STATE> the type of the state object. </param>
        /// <returns> <seealso cref="Evaluation.IncludeAndContinue"/> if all {@code nodes}
        ///         exist in a given <seealso cref="IPath"/>, otherwise
        ///         <seealso cref="Evaluation.EXCLUDE_AND_CONTINUE"/>. </returns>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
        //ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> includeIfContainsAll(final Neo4Net.GraphDb.Node... nodes)
        public static PathEvaluator<STATE> IncludeIfContainsAll<STATE>(params INode[] nodes)
        {
            if (nodes.Length == 1)
            {
                return new PathEvaluator_AdapterAnonymousInnerClass10(nodes);
            }

            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.Set<Neo4Net.GraphDb.Node> fullSet = new java.util.HashSet<>(asList(nodes));
            ISet<INode> fullSet = new HashSet<INode>(asList(nodes));
            return new PathEvaluator_AdapterAnonymousInnerClass11(fullSet);
        }

        private class PathEvaluator_AdapterAnonymousInnerClass10 : PathEvaluator_Adapter<STATE>
        {
            private INode[] _nodes;

            public PathEvaluator_AdapterAnonymousInnerClass10(INode[] nodes)
            {
                _nodes = nodes;
            }

            public override Evaluation Evaluate(IPath path, IBranchState state)
            {
                foreach (INode node in path.ReverseNodes)
                {
                    if (node.Equals(_nodes[0]))
                    {
                        return Evaluation.IncludeAndContinue;
                    }
                }
                return Evaluation.ExcludeAndContinue;
            }
        }

        private class PathEvaluator_AdapterAnonymousInnerClass11 : PathEvaluator_Adapter<STATE>
        {
            private ISet<INode> _fullSet;

            public PathEvaluator_AdapterAnonymousInnerClass11(ISet<INode> fullSet)
            {
                _fullSet = fullSet;
            }

            public override Evaluation Evaluate(IPath path, IBranchState state)
            {
                ISet<INode> set = new HashSet<INode>(_fullSet);
                foreach (INode node in path.ReverseNodes)
                {
                    if (set.remove(node) && set.Count == 0)
                    {
                        return Evaluation.IncludeAndContinue;
                    }
                }
                return Evaluation.ExcludeAndContinue;
            }
        }

        /// <summary>
        /// Whereas adding <seealso cref="IEvaluator"/>s to a <seealso cref="ITraversalDescription"/> puts those
        /// evaluators in {@code AND-mode} this can group many evaluators in {@code OR-mode}.
        /// </summary>
        /// <param name="evaluators"> represented as one evaluators. If any of the evaluators decides
        ///                   to include a path it will be included. </param>
        /// @param <STATE>    the type of the state object. </param>
        /// <returns> an <seealso cref="IEvaluator"/> which decides to include a path if any of the supplied
        ///         evaluators wants to include it. </returns>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
        //ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> includeIfAcceptedByAny(final PathEvaluator... evaluators)
        public static PathEvaluator<STATE> IncludeIfAcceptedByAny<STATE>(params PathEvaluator[] evaluators)
        {
            return new PathEvaluator_AdapterAnonymousInnerClass12(evaluators);
        }

        private class PathEvaluator_AdapterAnonymousInnerClass12 : PathEvaluator_Adapter<STATE>
        {
            private PathEvaluator[] _evaluators;

            public PathEvaluator_AdapterAnonymousInnerClass12(PathEvaluator[] evaluators)
            {
                _evaluators = evaluators;
            }

            //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
            //ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public Evaluation Evaluate(Neo4Net.GraphDb.Path path, BranchState state)
            public override Evaluation Evaluate(IPath path, IBranchState state)
            {
                foreach (PathEvaluator evaluator in _evaluators)
                {
                    if (evaluator.Evaluate(path, state).includes())
                    {
                        return Evaluation.IncludeAndContinue;
                    }
                }
                return Evaluation.ExcludeAndContinue;
            }
        }

        /// <summary>
        /// Whereas adding <seealso cref="IEvaluator"/>s to a <seealso cref="ITraversalDescription"/> puts those
        /// evaluators in {@code AND-mode} this can group many evaluators in {@code OR-mode}.
        /// </summary>
        /// <param name="evaluators"> represented as one evaluators. If any of the evaluators decides
        ///                   to include a path it will be included. </param>
        /// @param <STATE>    the type of the state object. </param>
        /// <returns> an <seealso cref="IEvaluator"/> which decides to include a path if any of the supplied
        ///         evaluators wants to include it. </returns>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
        //ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> includeIfAcceptedByAny(final Evaluator... evaluators)
        public static PathEvaluator<STATE> IncludeIfAcceptedByAny<STATE>(params IEvaluator[] evaluators)
        {
            return new PathEvaluator_AdapterAnonymousInnerClass13(evaluators);
        }

        private class PathEvaluator_AdapterAnonymousInnerClass13 : PathEvaluator_Adapter<STATE>
        {
            private Neo4Net.GraphDb.Traversal.IEvaluator[] _evaluators;

            public PathEvaluator_AdapterAnonymousInnerClass13(Neo4Net.GraphDb.Traversal.IEvaluator[] evaluators)
            {
                _evaluators = evaluators;
            }

            public override Evaluation Evaluate(IPath path, IBranchState state)
            {
                foreach (IEvaluator evaluator in _evaluators)
                {
                    if (evaluator.Evaluate(path).includes())
                    {
                        return Evaluation.IncludeAndContinue;
                    }
                }
                return Evaluation.ExcludeAndContinue;
            }
        }

        /// <summary>
        /// Returns <seealso cref="IEvaluator"/>s for paths with the specified depth and with an end node from the list of
        /// possibleEndNodes. </summary>
        /// <param name="depth"> The exact depth to filter the returned path evaluators. </param>
        /// <param name="possibleEndNodes"> Filter for the possible nodes to end the path on. </param>
        /// @param <STATE> the type of the state object. </param>
        /// <returns> <seealso cref="IEvaluator"/>s for paths with the specified depth and with an end node from the list of
        /// possibleEndNodes. </returns>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
        //ORIGINAL LINE: public static <STATE> PathEvaluator<STATE> endNodeIsAtDepth(final int depth, Neo4Net.GraphDb.Node... possibleEndNodes)
        public static PathEvaluator<STATE> EndNodeIsAtDepth<STATE>(int depth, params INode[] possibleEndNodes)
        {
            if (possibleEndNodes.Length == 1)
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final Neo4Net.GraphDb.Node target = possibleEndNodes[0];
                INode target = possibleEndNodes[0];
                return new PathEvaluator_AdapterAnonymousInnerClass14(depth, target);
            }

            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.Set<Neo4Net.GraphDb.Node> endNodes = new java.util.HashSet<>(asList(possibleEndNodes));
            ISet<INode> endNodes = new HashSet<INode>(asList(possibleEndNodes));
            return new PathEvaluator_AdapterAnonymousInnerClass15(depth, endNodes);
        }

        private class PathEvaluator_AdapterAnonymousInnerClass14 : PathEvaluator_Adapter<STATE>
        {
            private int _depth;
            private INode _target;

            public PathEvaluator_AdapterAnonymousInnerClass14(int depth, INode target)
            {
                _depth = depth;
                _target = target;
            }

            public override Evaluation Evaluate(IPath path, IBranchState state)
            {
                if (path.Length == _depth)
                {
                    return _target.Equals(path.EndNode) ? Evaluation.IncludeAndPrune : Evaluation.ExcludeAndPrune;
                }
                return Evaluation.ExcludeAndContinue;
            }
        }

        private class PathEvaluator_AdapterAnonymousInnerClass15 : PathEvaluator_Adapter<STATE>
        {
            private int _depth;
            private ISet<INode> _endNodes;

            public PathEvaluator_AdapterAnonymousInnerClass15(int depth, ISet<INode> endNodes)
            {
                _depth = depth;
                _endNodes = endNodes;
            }

            public override Evaluation Evaluate(IPath path, IBranchState state)
            {
                if (path.Length == _depth)
                {
                    return _endNodes.Contains(path.EndNode) ? Evaluation.IncludeAndPrune : Evaluation.ExcludeAndPrune;
                }
                return Evaluation.ExcludeAndContinue;
            }
        }
    }
}