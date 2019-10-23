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
namespace Neo4Net.GraphAlgo.ShortestPath
{
    using Neo4Net.GraphAlgo;
    using Neo4Net.GraphAlgo;
    using Direction = Neo4Net.GraphDb.Direction;
    using Node = Neo4Net.GraphDb.Node;
    using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
    using Relationship = Neo4Net.GraphDb.Relationship;
    using RelationshipType = Neo4Net.GraphDb.RelationshipType;
    using Neo4Net.GraphDb;
    using Neo4Net.GraphDb;
    using Iterables = Neo4Net.Helpers.Collections.Iterables;

    /// <summary>
    /// Dijkstra class. This class can be used to perform shortest path computations
    /// between two nodes. The search is made simultaneously from both the start node
    /// and the end node. Note that per default, only one shortest path will be
    /// searched for. This will be done when the path or the cost is asked for. If at
    /// some later time getPaths is called to get all the paths, the calculation is
    /// redone. In order to avoid this double computation when all paths are desired,
    /// be sure to call getPaths (or calculateMultiple) before any call to getPath or
    /// getCost (or calculate) is made.
    ///
    /// @complexity The <seealso cref="CostEvaluator"/>, the <seealso cref="CostAccumulator"/> and the
    ///             cost comparator will all be called once for every relationship
    ///             traversed. Assuming they run in constant time, the time
    ///             complexity for this algorithm is O(m + n * log(n)).
    /// @author Patrik Larsson </summary>
    /// @param <CostType> The datatype the edge weights will be represented by. </param>
    public class Dijkstra<CostType> : SingleSourceSingleSinkShortestPath<CostType>
    {
        protected internal CostType StartCost; // starting cost for both the start node and
                                               // the end node
                                               //JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
        protected internal Node StartNodeConflict;
        //JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
        protected internal Node EndNodeConflict;
        protected internal RelationshipType[] CostRelationTypes;
        protected internal Direction RelationDirection;
        protected internal CostEvaluator<CostType> CostEvaluator;
        protected internal CostAccumulator<CostType> CostAccumulator;
        protected internal IComparer<CostType> CostComparator;
        protected internal bool CalculateAllShortestPaths;
        // Limits
        protected internal long MaxRelationShipsToTraverse = -1;
        protected internal long NumberOfTraversedRelationShips;
        protected internal long MaxNodesToTraverse = -1;
        protected internal long NumberOfNodesTraversed;
        protected internal CostType MaxCost;

        /// <returns> True if the set limits for the calculation has been reached (but
        ///         not exceeded) </returns>
        protected internal virtual bool LimitReached()
        {
            return MaxRelationShipsToTraverse >= 0 && NumberOfTraversedRelationShips >= MaxRelationShipsToTraverse || MaxNodesToTraverse >= 0 && NumberOfNodesTraversed >= MaxNodesToTraverse;
        }

        protected internal virtual bool LimitReached(CostType cost1, CostType cost2)
        {
            if (MaxCost != default(CostType))
            {
                CostType totalCost = CostAccumulator.addCosts(cost1, cost2);
                if (CostComparator.Compare(totalCost, MaxCost) > 0)
                {
                    FoundPathsMiddleNodes = null;
                    FoundPathsCost = default(CostType);
                    return true;
                }
            }

            return false;
        }

        // Result data
        protected internal bool DoneCalculation;
        protected internal ISet<Node> FoundPathsMiddleNodes;
        protected internal CostType FoundPathsCost;
        protected internal Dictionary<Node, IList<Relationship>> Predecessors1 = new Dictionary<Node, IList<Relationship>>();
        protected internal Dictionary<Node, IList<Relationship>> Predecessors2 = new Dictionary<Node, IList<Relationship>>();

        /// <summary>
        /// Resets the result data to force the computation to be run again when some
        /// result is asked for.
        /// </summary>
        public override void Reset()
        {
            DoneCalculation = false;
            FoundPathsMiddleNodes = null;
            Predecessors1 = new Dictionary<Node, IList<Relationship>>();
            Predecessors2 = new Dictionary<Node, IList<Relationship>>();
            // Limits
            NumberOfTraversedRelationShips = 0;
            NumberOfNodesTraversed = 0;
        }

        /// <param name="startCost"> Starting cost for both the start node and the end node </param>
        /// <param name="startNode"> the start node </param>
        /// <param name="endNode"> the end node </param>
        /// <param name="costRelationTypes"> the relationship that should be included in the
        ///            path </param>
        /// <param name="relationDirection"> relationship direction to follow </param>
        /// <param name="costEvaluator"> the cost function per relationship </param>
        /// <param name="costAccumulator"> adding up the path cost </param>
        /// <param name="costComparator"> comparing to path costs </param>
        public Dijkstra(CostType startCost, Node startNode, Node endNode, CostEvaluator<CostType> costEvaluator, CostAccumulator<CostType> costAccumulator, IComparer<CostType> costComparator, Direction relationDirection, params RelationshipType[] costRelationTypes) : base()
        {
            this.StartCost = startCost;
            this.StartNodeConflict = startNode;
            this.EndNodeConflict = endNode;
            this.CostRelationTypes = costRelationTypes;
            this.RelationDirection = relationDirection;
            this.CostEvaluator = costEvaluator;
            this.CostAccumulator = costAccumulator;
            this.CostComparator = costComparator;
        }

        /// <summary>
        /// A DijkstraIterator computes the distances to nodes from a specified
        /// starting node, one at a time, following the dijkstra algorithm.
        ///
        /// @author Patrik Larsson
        /// </summary>
        protected internal class DijkstraIterator : IEnumerator<Node>
        {
            private readonly Dijkstra<CostType> _outerInstance;

            protected internal Node StartNode;
            // where do we come from
            protected internal Dictionary<Node, IList<Relationship>> Predecessors;
            // observed distances not yet final
            protected internal Dictionary<Node, CostType> MySeen;
            protected internal Dictionary<Node, CostType> OtherSeen;
            // the final distances
            protected internal Dictionary<Node, CostType> MyDistances;
            protected internal Dictionary<Node, CostType> OtherDistances;
            // Flag that indicates if we should follow egdes in the opposite
            // direction instead
            protected internal bool Backwards;
            // The priority queue
            protected internal DijkstraPriorityQueue<CostType> Queue;
            // "Done" flags. The first is set to true when a node is found that is
            // contained in both myDistances and otherDistances. This means the
            // calculation has found one of the shortest paths.
            protected internal bool OneShortestPathHasBeenFound;
            protected internal bool AllShortestPathsHasBeenFound;

            public DijkstraIterator(Dijkstra<CostType> outerInstance, Node startNode, Dictionary<Node, IList<Relationship>> predecessors, Dictionary<Node, CostType> mySeen, Dictionary<Node, CostType> otherSeen, Dictionary<Node, CostType> myDistances, Dictionary<Node, CostType> otherDistances, bool backwards) : base()
            {
                this._outerInstance = outerInstance;
                this.StartNode = startNode;
                this.Predecessors = predecessors;
                this.MySeen = mySeen;
                this.OtherSeen = otherSeen;
                this.MyDistances = myDistances;
                this.OtherDistances = otherDistances;
                this.Backwards = backwards;
                InitQueue();
            }

            /// <returns> The direction to use when searching for relations/edges </returns>
            protected internal virtual Direction Direction
            {
                get
                {
                    if (Backwards)
                    {
                        if (outerInstance.RelationDirection.Equals(Direction.INCOMING))
                        {
                            return Direction.OUTGOING;
                        }
                        if (outerInstance.RelationDirection.Equals(Direction.OUTGOING))
                        {
                            return Direction.INCOMING;
                        }
                    }
                    return outerInstance.RelationDirection;
                }
            }

            // This puts the start node into the queue
            protected internal virtual void InitQueue()
            {
                Queue = new DijkstraPriorityQueueFibonacciImpl<CostType>(outerInstance.CostComparator);
                Queue.insertValue(StartNode, outerInstance.StartCost);
                MySeen[StartNode] = outerInstance.StartCost;
            }

            public override bool HasNext()
            {
                return !Queue.Empty && !outerInstance.LimitReached();
            }

            public override void Remove()
            {
                // Not used
                // Could be used to generate more solutions, by removing an edge
                // from the solution and run again?
            }

            /// <summary>
            /// This checks if a node has been seen by the other iterator/traverser
            /// as well. In that case a path has been found. In that case, the total
            /// cost for the path is calculated and compared to previously found
            /// paths.
            /// </summary>
            /// <param name="currentNode"> The node to be examined. </param>
            /// <param name="currentCost"> The cost from the start node to this node. </param>
            /// <param name="otherSideDistances"> Map over distances from other side. A path
            ///            is found and examined if this contains currentNode. </param>
            protected internal virtual void CheckForPath(Node currentNode, CostType currentCost, Dictionary<Node, CostType> otherSideDistances)
            {
                // Found a path?
                if (otherSideDistances.ContainsKey(currentNode))
                {
                    // Is it better than previously found paths?
                    CostType otherCost = otherSideDistances[currentNode];
                    CostType newTotalCost = outerInstance.CostAccumulator.addCosts(currentCost, otherCost);
                    if (outerInstance.FoundPathsMiddleNodes == null)
                    {
                        outerInstance.FoundPathsMiddleNodes = new HashSet<Node>();
                    }
                    // No previous path found, or equally good one found?
                    if (outerInstance.FoundPathsMiddleNodes.Count == 0 || outerInstance.CostComparator.Compare(outerInstance.FoundPathsCost, newTotalCost) == 0)
                    {
                        outerInstance.FoundPathsCost = newTotalCost; // in case we had no
                                                                     // previous path
                        outerInstance.FoundPathsMiddleNodes.Add(currentNode);
                    }
                    // New better path found?
                    else if (outerInstance.CostComparator.Compare(outerInstance.FoundPathsCost, newTotalCost) > 0)
                    {
                        outerInstance.FoundPathsMiddleNodes.Clear();
                        outerInstance.FoundPathsCost = newTotalCost;
                        outerInstance.FoundPathsMiddleNodes.Add(currentNode);
                    }
                }
            }

            public override Node Next()
            {
                if (!HasNext())
                {
                    throw new NoSuchElementException();
                }

                Node currentNode = Queue.extractMin();
                CostType currentCost = MySeen[currentNode];
                // Already done with this node?
                if (MyDistances.ContainsKey(currentNode))
                {
                    return null;
                }
                if (outerInstance.LimitReached())
                {
                    return null;
                }
                ++outerInstance.NumberOfNodesTraversed;
                MyDistances[currentNode] = currentCost;
                // TODO: remove from seen or not? probably not... because of path
                // detection
                // Check if we have found a better path
                CheckForPath(currentNode, currentCost, OtherSeen);
                // Found a path? (abort traversing from this node)
                if (OtherDistances.ContainsKey(currentNode))
                {
                    OneShortestPathHasBeenFound = true;
                }
                else
                {
                    // Otherwise, follow all edges from this node
                    foreach (RelationshipType costRelationType in outerInstance.CostRelationTypes)
                    {
                        ResourceIterable<Relationship> relationships = Iterables.asResourceIterable(currentNode.GetRelationships(costRelationType, Direction));
                        using (ResourceIterator<Relationship> iterator = relationships.GetEnumerator())
                        {
                            while (iterator.MoveNext())
                            {
                                Relationship relationship = iterator.Current;
                                if (outerInstance.LimitReached())
                                {
                                    break;
                                }
                                ++outerInstance.NumberOfTraversedRelationShips;
                                // Target node
                                Node target = relationship.GetOtherNode(currentNode);
                                if (OtherDistances.ContainsKey(target))
                                {
                                    continue;
                                }
                                // Find out if an eventual path would go in the opposite
                                // direction of the edge
                                bool backwardsEdge = relationship.EndNode.Equals(currentNode) ^ Backwards;
                                CostType newCost = outerInstance.CostAccumulator.addCosts(currentCost, outerInstance.CostEvaluator.getCost(relationship, backwardsEdge ? Direction.INCOMING : Direction.OUTGOING));
                                // Already done with target node?
                                if (MyDistances.ContainsKey(target))
                                {
                                    // Have we found a better cost for a node which is
                                    // already
                                    // calculated?
                                    if (outerInstance.CostComparator.Compare(MyDistances[target], newCost) > 0)
                                    {
                                        throw new Exception("Cycle with negative costs found.");
                                    }
                                    // Equally good path found?
                                    else if (outerInstance.CalculateAllShortestPaths && outerInstance.CostComparator.Compare(MyDistances[target], newCost) == 0)
                                    {
                                        // Put it in predecessors
                                        IList<Relationship> myPredecessors = Predecessors[currentNode];
                                        // Dont do it if this relation is already in
                                        // predecessors (other direction)
                                        if (myPredecessors == null || !myPredecessors.Contains(relationship))
                                        {
                                            IList<Relationship> predList = Predecessors[target];
                                            if (predList == null)
                                            {
                                                // This only happens if we get back to
                                                // the
                                                // start node, which is just bogus
                                            }
                                            else
                                            {
                                                predList.Add(relationship);
                                            }
                                        }
                                    }
                                    continue;
                                }
                                // Have we found a better cost for this node?
                                if (!MySeen.ContainsKey(target) || outerInstance.CostComparator.Compare(MySeen[target], newCost) > 0)
                                {
                                    // Put it in the queue
                                    if (!MySeen.ContainsKey(target))
                                    {
                                        Queue.insertValue(target, newCost);
                                    }
                                    // or update the entry. (It is important to keep
                                    // these
                                    // cases apart to limit the size of the queue)
                                    else
                                    {
                                        Queue.decreaseValue(target, newCost);
                                    }
                                    // Update it
                                    MySeen[target] = newCost;
                                    // Put it in predecessors
                                    IList<Relationship> predList = new LinkedList<Relationship>();
                                    predList.Add(relationship);
                                    Predecessors[target] = predList;
                                }
                                // Have we found an equal cost for (additional path to)
                                // this
                                // node?
                                else if (outerInstance.CalculateAllShortestPaths && outerInstance.CostComparator.Compare(MySeen[target], newCost) == 0)
                                {
                                    // Put it in predecessors
                                    IList<Relationship> predList = Predecessors[target];
                                    predList.Add(relationship);
                                }
                            }
                        }
                    }
                }
                // Check how far we need to continue when searching for all shortest
                // paths
                if (outerInstance.CalculateAllShortestPaths && OneShortestPathHasBeenFound)
                {
                    // If we cannot continue or continuation would only find more
                    // expensive paths: conclude that all shortest paths have been
                    // found.
                    AllShortestPathsHasBeenFound = Queue.Empty || outerInstance.CostComparator.Compare(MySeen[Queue.peek()], currentCost) > 0;
                }
                return currentNode;
            }

            public virtual bool Done
            {
                get
                {
                    if (!outerInstance.CalculateAllShortestPaths)
                    {
                        return OneShortestPathHasBeenFound;
                    }
                    return AllShortestPathsHasBeenFound;
                }
            }
        }

        /// <summary>
        /// Same as calculate(), but will set the flag to calculate all shortest
        /// paths. It sets the flag and then calls calculate, so inheriting classes
        /// only need to override calculate().
        ///
        /// @return
        /// </summary>
        public virtual bool CalculateMultiple()
        {
            if (!CalculateAllShortestPaths)
            {
                Reset();
                CalculateAllShortestPaths = true;
            }
            return Calculate();
        }

        /// <summary>
        /// Makes the main calculation If some limit is set, the shortest path(s)
        /// that could be found within those limits will be calculated.
        /// </summary>
        /// <returns> True if a path was found. </returns>
        public virtual bool Calculate()
        {
            // Do this first as a general error check since this is supposed to be
            // called whenever a result is asked for.
            if (StartNodeConflict == null || EndNodeConflict == null)
            {
                throw new Exception("Start or end node undefined.");
            }
            // Don't do it more than once
            if (DoneCalculation)
            {
                return true;
            }
            DoneCalculation = true;
            // Special case when path length is zero
            if (StartNodeConflict.Equals(EndNodeConflict))
            {
                FoundPathsMiddleNodes = new HashSet<Node>();
                FoundPathsMiddleNodes.Add(StartNodeConflict);
                FoundPathsCost = CostAccumulator.addCosts(StartCost, StartCost);
                return true;
            }
            Dictionary<Node, CostType> seen1 = new Dictionary<Node, CostType>();
            Dictionary<Node, CostType> seen2 = new Dictionary<Node, CostType>();
            Dictionary<Node, CostType> dists1 = new Dictionary<Node, CostType>();
            Dictionary<Node, CostType> dists2 = new Dictionary<Node, CostType>();
            DijkstraIterator iter1 = new DijkstraIterator(this, StartNodeConflict, Predecessors1, seen1, seen2, dists1, dists2, false);
            DijkstraIterator iter2 = new DijkstraIterator(this, EndNodeConflict, Predecessors2, seen2, seen1, dists2, dists1, true);
            Node node1 = null;
            Node node2 = null;
            while (iter1.MoveNext() && iter2.MoveNext())
            {
                if (LimitReached())
                {
                    break;
                }
                //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                if (iter1.HasNext())
                {
                    node1 = iter1.Current;
                    if (node1 == null)
                    {
                        break;
                    }
                }
                if (LimitReached())
                {
                    break;
                }
                //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                if (!iter1.Done && iter2.HasNext())
                {
                    //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                    node2 = iter2.Next();
                    if (node2 == null)
                    {
                        break;
                    }
                }
                if (LimitReached(seen1[node1], seen2[node2]))
                {
                    break;
                }
                if (iter1.Done || iter2.Done) // A path was found
                {
                    return true;
                }
            }

            return false;
        }

        /// <returns> The cost for the found path(s). </returns>
        public virtual CostType Cost
        {
            get
            {
                if (StartNodeConflict == null || EndNodeConflict == null)
                {
                    throw new Exception("Start or end node undefined.");
                }
                Calculate();
                return FoundPathsCost;
            }
        }

        /// <returns> All the found paths or null. </returns>
        public virtual IList<IList<PropertyContainer>> Paths
        {
            get
            {
                if (StartNodeConflict == null || EndNodeConflict == null)
                {
                    throw new Exception("Start or end node undefined.");
                }
                CalculateMultiple();
                if (FoundPathsMiddleNodes == null || FoundPathsMiddleNodes.Count == 0)
                {
                    return Collections.emptyList();
                }

                IList<IList<PropertyContainer>> paths = new LinkedList<IList<PropertyContainer>>();
                foreach (Node middleNode in FoundPathsMiddleNodes)
                {
                    IList<IList<PropertyContainer>> paths1 = Util.ConstructAllPathsToNode(middleNode, Predecessors1, true, false);
                    IList<IList<PropertyContainer>> paths2 = Util.ConstructAllPathsToNode(middleNode, Predecessors2, false, true);
                    // For all combinations...
                    foreach (IList<PropertyContainer> part1 in paths1)
                    {
                        foreach (IList<PropertyContainer> part2 in paths2)
                        {
                            // Combine them
                            LinkedList<PropertyContainer> path = new LinkedList<PropertyContainer>();
                            path.addAll(part1);
                            path.addAll(part2);
                            // Add to collection
                            paths.Add(path);
                        }
                    }
                }

                return paths;
            }
        }

        /// <returns> All the found paths or null. </returns>
        public virtual IList<IList<Node>> PathsAsNodes
        {
            get
            {
                if (StartNodeConflict == null || EndNodeConflict == null)
                {
                    throw new Exception("Start or end node undefined.");
                }
                CalculateMultiple();
                if (FoundPathsMiddleNodes == null || FoundPathsMiddleNodes.Count == 0)
                {
                    return null;
                }

                IList<IList<Node>> paths = new LinkedList<IList<Node>>();
                foreach (Node middleNode in FoundPathsMiddleNodes)
                {
                    IList<IList<Node>> paths1 = Util.ConstructAllPathsToNodeAsNodes(middleNode, Predecessors1, true, false);
                    IList<IList<Node>> paths2 = Util.ConstructAllPathsToNodeAsNodes(middleNode, Predecessors2, false, true);
                    // For all combinations...
                    foreach (IList<Node> part1 in paths1)
                    {
                        foreach (IList<Node> part2 in paths2)
                        {
                            // Combine them
                            LinkedList<Node> path = new LinkedList<Node>();
                            path.addAll(part1);
                            path.addAll(part2);
                            // Add to collection
                            paths.Add(path);
                        }
                    }
                }

                return paths;
            }
        }

        /// <returns> All the found paths or null. </returns>
        public virtual IList<IList<Relationship>> PathsAsRelationships
        {
            get
            {
                if (StartNodeConflict == null || EndNodeConflict == null)
                {
                    throw new Exception("Start or end node undefined.");
                }
                CalculateMultiple();
                if (FoundPathsMiddleNodes == null || FoundPathsMiddleNodes.Count == 0)
                {
                    return null;
                }

                IList<IList<Relationship>> paths = new LinkedList<IList<Relationship>>();
                foreach (Node middleNode in FoundPathsMiddleNodes)
                {
                    IList<IList<Relationship>> paths1 = Util.ConstructAllPathsToNodeAsRelationships(middleNode, Predecessors1, false);
                    IList<IList<Relationship>> paths2 = Util.ConstructAllPathsToNodeAsRelationships(middleNode, Predecessors2, true);
                    // For all combinations...
                    foreach (IList<Relationship> part1 in paths1)
                    {
                        foreach (IList<Relationship> part2 in paths2)
                        {
                            // Combine them
                            LinkedList<Relationship> path = new LinkedList<Relationship>();
                            path.addAll(part1);
                            path.addAll(part2);
                            // Add to collection
                            paths.Add(path);
                        }
                    }
                }

                return paths;
            }
        }

        /// <returns> One of the shortest paths found or null. </returns>
        public virtual IList<PropertyContainer> Path
        {
            get
            {
                if (StartNodeConflict == null || EndNodeConflict == null)
                {
                    throw new Exception("Start or end node undefined.");
                }
                Calculate();
                if (FoundPathsMiddleNodes == null || FoundPathsMiddleNodes.Count == 0)
                {
                    return null;
                }
                Node middleNode = FoundPathsMiddleNodes.GetEnumerator().next();
                LinkedList<PropertyContainer> path = new LinkedList<PropertyContainer>();
                path.addAll(Util.ConstructSinglePathToNode(middleNode, Predecessors1, true, false));
                path.addAll(Util.ConstructSinglePathToNode(middleNode, Predecessors2, false, true));
                return path;
            }
        }

        /// <returns> One of the shortest paths found or null. </returns>
        public virtual IList<Node> PathAsNodes
        {
            get
            {
                if (StartNodeConflict == null || EndNodeConflict == null)
                {
                    throw new Exception("Start or end node undefined.");
                }
                Calculate();
                if (FoundPathsMiddleNodes == null || FoundPathsMiddleNodes.Count == 0)
                {
                    return null;
                }
                Node middleNode = FoundPathsMiddleNodes.GetEnumerator().next();
                LinkedList<Node> pathNodes = new LinkedList<Node>();
                pathNodes.addAll(Util.ConstructSinglePathToNodeAsNodes(middleNode, Predecessors1, true, false));
                pathNodes.addAll(Util.ConstructSinglePathToNodeAsNodes(middleNode, Predecessors2, false, true));
                return pathNodes;
            }
        }

        /// <returns> One of the shortest paths found or null. </returns>
        public virtual IList<Relationship> PathAsRelationships
        {
            get
            {
                if (StartNodeConflict == null || EndNodeConflict == null)
                {
                    throw new Exception("Start or end node undefined.");
                }
                Calculate();
                if (FoundPathsMiddleNodes == null || FoundPathsMiddleNodes.Count == 0)
                {
                    return null;
                }
                Node middleNode = FoundPathsMiddleNodes.GetEnumerator().next();
                IList<Relationship> path = new LinkedList<Relationship>();
                ((IList<Relationship>)path).AddRange(Util.ConstructSinglePathToNodeAsRelationships(middleNode, Predecessors1, false));
                ((IList<Relationship>)path).AddRange(Util.ConstructSinglePathToNodeAsRelationships(middleNode, Predecessors2, true));
                return path;
            }
        }

        /// <summary>
        /// This sets the maximum depth in the form of a maximum number of
        /// relationships to follow.
        /// </summary>
        /// <param name="maxRelationShipsToTraverse"> </param>
        public virtual void LimitMaxRelationShipsToTraverse(long maxRelationShipsToTraverse)
        {
            this.MaxRelationShipsToTraverse = maxRelationShipsToTraverse;
        }

        /// <summary>
        /// This sets the maximum depth in the form of a maximum number of nodes to
        /// scan.
        /// </summary>
        /// <param name="maxNodesToTraverse"> </param>
        public virtual void LimitMaxNodesToTraverse(long maxNodesToTraverse)
        {
            this.MaxNodesToTraverse = maxNodesToTraverse;
        }

        /// <summary>
        /// Set the end node. Will reset the calculation.
        /// </summary>
        /// <param name="endNode"> the endNode to set </param>
        public virtual Node EndNode
        {
            set
            {
                Reset();
                this.EndNodeConflict = value;
            }
        }

        /// <summary>
        /// Set the start node. Will reset the calculation.
        /// </summary>
        /// <param name="startNode"> the startNode to set </param>
        public virtual Node StartNode
        {
            set
            {
                this.StartNodeConflict = value;
                Reset();
            }
        }

        /// <returns> the relationDirection </returns>
        public virtual Direction Direction
        {
            get
            {
                return RelationDirection;
            }
        }

        /// <returns> the costRelationType </returns>
        public virtual RelationshipType[] RelationshipTypes
        {
            get
            {
                return CostRelationTypes;
            }
        }

        /// <summary>
        /// Set the evaluator for pruning the paths when the maximum cost is
        /// exceeded.
        /// </summary>
        /// <param name="maxCost"> </param>
        public virtual void LimitMaxCostToTraverse(CostType maxCost)
        {
            this.MaxCost = maxCost;
        }
    }
}