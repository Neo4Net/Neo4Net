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
using System;
using System.Collections.Generic;
using Neo4Net.Functions;
//
namespace Neo4Net.Collections.Pooling
{
    public class LinkedQueuePool<R> : Pool<R>
    {
        public interface Monitor<R>
        {
            void UpdatedCurrentPeakSize(int currentPeakSize);

            void UpdatedTargetSize(int targetSize);

            void Created(R resource);

            void Acquired(R resource);

            void Disposed(R resource);
        }

        public class Monitor_Adapter<R> : Monitor<R>
        {
            private readonly LinkedQueuePool<R> _outerInstance;

            public Monitor_Adapter(LinkedQueuePool<R> outerInstance)
            {
                this._outerInstance = outerInstance;
            }

            public override void UpdatedCurrentPeakSize(int currentPeakSize)
            {
            }

            public override void UpdatedTargetSize(int targetSize)
            {
            }

            public override void Created(R resource)
            {
            }

            public override void Acquired(R resource)
            {
            }

            public override void Disposed(R resource)
            {
            }
        }

        public interface CheckStrategy
        {
            bool ShouldCheck();
        }

        public class CheckStrategy_TimeoutCheckStrategy : CheckStrategy
        {
            private readonly LinkedQueuePool<R> _outerInstance;

            internal readonly long Interval;
            internal long LastCheckTime;
            internal readonly System.Func<long> Clock;

            internal CheckStrategy_TimeoutCheckStrategy(LinkedQueuePool<R> outerInstance, long interval) : this(outerInstance, interval, System.currentTimeMillis)
            {
                this._outerInstance = outerInstance;
            }

            internal CheckStrategy_TimeoutCheckStrategy(LinkedQueuePool<R> outerInstance, long interval, System.Func<long> clock)
            {
                this._outerInstance = outerInstance;
                this.Interval = interval;
                this.LastCheckTime = clock();
                this.Clock = clock;
            }

            public override bool ShouldCheck()
            {
                long currentTime = Clock.AsLong;
                if (currentTime > LastCheckTime + Interval)
                {
                    LastCheckTime = currentTime;
                    return true;
                }
                return false;
            }
        }

        private const int DEFAULT_CHECK_INTERVAL = 60 * 1000;

        private readonly LinkedList<R> _unused = new ConcurrentLinkedQueue<R>();
        private readonly Monitor<R> _monitor;
        private readonly int _minSize;
        private readonly Factory<R> _factory;
        private readonly CheckStrategy _checkStrategy;
        // Guarded by nothing. Those are estimates, losing some values doesn't matter much
        private readonly AtomicInteger _allocated = new AtomicInteger(0);
        private readonly AtomicInteger _queueSize = new AtomicInteger(0);
        private int _currentPeakSize;
        private int _targetSize;

        public LinkedQueuePool(int minSize, Factory<R> factory) : this(minSize, factory, new CheckStrategy_TimeoutCheckStrategy(this, DEFAULT_CHECK_INTERVAL), new Monitor_Adapter<>(this))
        {
        }

        public LinkedQueuePool(int minSize, Factory<R> factory, CheckStrategy strategy, Monitor<R> monitor)
        {
            this._minSize = minSize;
            this._factory = factory;
            this._currentPeakSize = 0;
            this._targetSize = minSize;
            this._checkStrategy = strategy;
            this._monitor = monitor;
        }

        protected internal virtual R Create()
        {
            return _factory.newInstance();
        }

        protected internal virtual void Dispose(R resource)
        {
            _monitor.disposed(resource);
            _allocated.decrementAndGet();
        }

        public override R Acquire()
        {
            R resource = _unused.RemoveFirst();
            if (resource == default(R))
            {
                resource = Create();
                _allocated.incrementAndGet();
                _monitor.created(resource);
            }
            else
            {
                _queueSize.decrementAndGet();
            }
            _currentPeakSize = Math.Max(_currentPeakSize, _allocated.get() - _queueSize.get());
            if (_checkStrategy.shouldCheck())
            {
                _targetSize = Math.Max(_minSize, _currentPeakSize);
                _monitor.updatedCurrentPeakSize(_currentPeakSize);
                _currentPeakSize = 0;
                _monitor.updatedTargetSize(_targetSize);
            }

            _monitor.acquired(resource);
            return resource;
        }

        public override void Release(R toRelease)
        {
            if (_queueSize.get() < _targetSize)
            {
                _unused.AddLast(toRelease);
                _queueSize.incrementAndGet();
            }
            else
            {
                Dispose(toRelease);
            }
        }

        /// <summary>
        /// Dispose of all pooled objects.
        /// </summary>
        public override void Close()
        {
            for (R resource = _unused.RemoveFirst(); resource != default(R); resource = _unused.RemoveFirst())
            {
                Dispose(resource);
            }
        }
    }
}