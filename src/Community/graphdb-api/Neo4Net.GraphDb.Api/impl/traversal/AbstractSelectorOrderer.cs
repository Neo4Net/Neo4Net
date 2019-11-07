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
    using Neo4Net.GraphDb.Traversal;
    using IBranchSelector = Neo4Net.GraphDb.Traversal.IBranchSelector;
    using ITraversalBranch = Neo4Net.GraphDb.Traversal.ITraversalBranch;
    using ITraversalContext = Neo4Net.GraphDb.Traversal.ITraversalContext;

    public abstract class AbstractSelectorOrderer<T> : ISideSelector
    {
        public abstract ITraversalBranch Next(ITraversalContext metadata);

        private static readonly IBranchSelector _emptySelector = null;

        private readonly IBranchSelector[] _selectors;

        private readonly T[] _states = new T[2];  //private final T[] states = (T[]) new Object[2];

        private int _selectorIndex;

        public AbstractSelectorOrderer(IBranchSelector startSelector, IBranchSelector endSelector)
        {
            _selectors = new IBranchSelector[] { startSelector, endSelector };
            _states[0] = InitialState();
            _states[1] = InitialState();
        }

        protected internal virtual T InitialState()
        {
            return default(T);
        }

        protected internal virtual T StateForCurrentSelector
        {
            set
            {
                _states[_selectorIndex] = value;
            }
            get
            {
                return _states[_selectorIndex];
            }
        }

        protected internal virtual ITraversalBranch NextBranchFromCurrentSelector(ITraversalContext metadata, bool switchIfExhausted)
        {
            return NextBranchFromSelector(metadata, _selectors[_selectorIndex], switchIfExhausted);
        }

        protected internal virtual ITraversalBranch NextBranchFromNextSelector(ITraversalContext metadata, bool switchIfExhausted)
        {
            return NextBranchFromSelector(metadata, NextSelector(), switchIfExhausted);
        }

        private ITraversalBranch NextBranchFromSelector(ITraversalContext metadata, IBranchSelector selector, bool switchIfExhausted)
        {
            ITraversalBranch result = selector.Next(metadata);
            if (result == null)
            {
                _selectors[_selectorIndex] = _emptySelector;
                if (switchIfExhausted)
                {
                    result = NextSelector().Next(metadata);
                    if (result == null)
                    {
                        _selectors[_selectorIndex] = _emptySelector;
                    }
                }
            }
            return result;
        }

        protected internal virtual IBranchSelector NextSelector()
        {
            _selectorIndex = (_selectorIndex + 1) % 2;
            return _selectors[_selectorIndex];
        }

        public Direction CurrentSide => _selectorIndex == 0 ? Direction.Outgoing : Direction.Incoming;
    }
}