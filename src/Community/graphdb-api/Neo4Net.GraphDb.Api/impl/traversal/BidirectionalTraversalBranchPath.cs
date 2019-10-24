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

namespace Neo4Net.GraphDb.Impl.Traversal
{
    using System.Collections;
    using ITraversalBranch = Neo4Net.GraphDb.Traversal.ITraversalBranch;
    using Paths = Neo4Net.GraphDb.Traversal.Paths;

    internal class BidirectionalTraversalBranchPath : IPath
    {
        private readonly ITraversalBranch _start;
        private readonly ITraversalBranch _end;
        private readonly INode _endNode;
        private readonly IRelationship _lastRelationship;

        private INode _cachedStartNode;
        private LinkedList<IRelationship> _cachedRelationships;

        internal BidirectionalTraversalBranchPath(ITraversalBranch start, ITraversalBranch end)
        {
            _start = start;
            _end = end;

            // Most used properties: EndNode and LastRelationship, so cache them right away (semi-expensive though).
            IEnumerator<IPropertyContainer> endPathEntities = end.GetEnumerator();
            //$!!$ tac
            endPathEntities.MoveNext(); // $!!$ _endNode = (INode) endPathEntities.Next();
            _endNode = (INode)endPathEntities.Current;

            var hasNext = endPathEntities.MoveNext();
            _lastRelationship = (hasNext) ? (IRelationship)endPathEntities.Current : start.LastRelationship; // $!!$ tac this.lastRelationship = endPathEntities.hasNext() ? (Relationship)endPathEntities.next() : start.lastRelationship();
            //$
        }

        public INode StartNode
        {
            get
            {
                // Getting the start node is expensive in some Path implementations, so cache it
                if (_cachedStartNode == null)
                {
                    _cachedStartNode = _start.StartNode;
                }
                return _cachedStartNode;
            }
        }

        public INode EndNode => _endNode;

        public IRelationship LastRelationship => _lastRelationship;

        public IEnumerable<IRelationship> Relationships
        {
            get
            {
                // Cache the relationships since we use them in hashCode/equals too.
                if (_cachedRelationships == null)
                {
                    _cachedRelationships = GatherRelationships(_start, _end);
                }
                return _cachedRelationships;
            }
        }

        public IEnumerable<IRelationship> ReverseRelationships => GatherRelationships(_end, _start);

        private LinkedList<IRelationship> GatherRelationships(ITraversalBranch first, ITraversalBranch then)
        {
            // TODO Don't loop through them all up front
            LinkedList<IRelationship> relationships = new LinkedList<IRelationship>();
            ITraversalBranch branch = first;
            while (branch.Length > 0)
            {
                relationships.AddFirst(branch.LastRelationship);
                branch = branch.Parent;
            }
            // We can might as well cache start node since we're right now there anyway
            if (_cachedStartNode == null && first == _start && branch.Length >= 0)
            {
                _cachedStartNode = branch.EndNode;
            }
            branch = then;
            while (branch.Length > 0)
            {
                relationships.AddLast(branch.LastRelationship);
                branch = branch.Parent;
            }
            if (_cachedStartNode == null && then == _start && branch.Length >= 0)
            {
                _cachedStartNode = branch.EndNode;
            }
            return relationships;
        }

        public IEnumerable<INode> Nodes => GatherNodes(_start, _end);

        public IEnumerable<INode> ReverseNodes => GatherNodes(_end, _start);

        private IEnumerable<INode> GatherNodes(ITraversalBranch first, ITraversalBranch then)
        {
            // TODO Don't loop through them all up front
            LinkedList<INode> nodes = new LinkedList<INode>();
            ITraversalBranch branch = first;
            while (branch.Length > 0)
            {
                nodes.AddFirst(branch.EndNode);
                branch = branch.Parent;
            }
            if (_cachedStartNode == null && first == _start && branch.Length >= 0)
            {
                _cachedStartNode = branch.EndNode;
            }
            nodes.AddFirst(branch.EndNode);
            branch = then.Parent;
            if (branch != null)
            {
                while (branch.Length > 0)
                {
                    nodes.AddLast(branch.EndNode);
                    branch = branch.Parent;
                }
                if (branch.Length >= 0)
                {
                    nodes.AddLast(branch.EndNode);
                }
            }
            if (_cachedStartNode == null && then == _start && branch != null && branch.Length >= 0)
            {
                _cachedStartNode = branch.EndNode;
            }
            return nodes;
        }

        //
        public int Length => _start.Length + _end.Length;

        public string Path => throw new System.NotImplementedException();

        //
        public IEnumerator<IPropertyContainer> Iterator()
        {
            // TODO Don't loop through them all up front
            LinkedList<IPropertyContainer> entities = new LinkedList<IPropertyContainer>();
            ITraversalBranch branch = _start;
            while (branch.Length > 0)
            {
                entities.AddFirst(branch.EndNode);
                entities.AddFirst(branch.LastRelationship);
                branch = branch.Parent;
            }
            entities.AddFirst(branch.EndNode);
            if (_cachedStartNode == null)
            {
                _cachedStartNode = branch.EndNode;
            }
            if (_end.Length > 0)
            {
                entities.AddLast(_end.LastRelationship);
                branch = _end.Parent;
                while (branch.Length > 0)
                {
                    entities.AddLast(branch.EndNode);
                    entities.AddLast(branch.LastRelationship);
                    branch = branch.Parent;
                }
                entities.AddLast(branch.EndNode);
            }
            return entities.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return Relationships.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (!(obj is IPath))
            {
                return false;
            }

            IPath other = (IPath)obj;
            return Relationships.Equals(other.Relationships) && other.StartNode.Equals(_cachedStartNode);
        }

        public override string ToString()
        {
            return Paths.DefaultPathToString(this);
        }

        public IEnumerator<IPropertyContainer> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}